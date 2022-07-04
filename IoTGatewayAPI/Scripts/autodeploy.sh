#!/usr/bin bash
#sudo docker service update --update-delay=2m --update-monitor=30s --rollback-delay=20s --with-registry-auth --image $1 $2
#sudo ./docker-service-update-wait.sh internship_api 60 2 "docker service update --image registry.mss.ba/internship-backend:$(Build.BuildId) --stop-grace-period 10s --update-delay 5s internship_api --with-registry-auth"
#? ./docker-service-update-wait.sh internship_api imagename

echo "Commands entered"
echo "First: $1"
echo "Second: $2"
if [[ "$#" -ne 2 ]]; then
  echo Expected arguments are missing: SERVICE_NAME IMAGE_NAME
  exit 1
fi

re="[[:space:]]+"
if [[ $1 =~ $re ]] || [[ $2 =~ $re ]]; then
  echo "Input strings contains one or more spaces"
  exit 1
fi
#else
name=iotgatewayapi_iotgatewayapi
image=registry.mss.ba/iotgatewayapi:alpha
#exit 1

docker pull $image
#--with-registry-auth
#No need for registry auth, we need to run this script as sudoer
if [[ "$(docker images -q $image 2> /dev/null)" == "" ]]; then
  echo "Image doesn't exist?"
  exit 1
fi


timeout_per_replica=60
stability_delay=5
rollback_delay=20
update_cmd="docker service update --update-delay=${stability_delay}s --update-monitor=30s --rollback-delay=${rollback_delay}s --update-parallelism=1 --with-registry-auth --image $image $name"
delay=3

# find out which container IDs to ignore while upgrading
ignore_ids=$(docker service ps $name | grep "_ $name" | awk '{print "|"$1}' | xargs echo | awk '{gsub(/ /, ""); print}')
ignore_ids=${ignore_ids#|*}
function get_new_ids() {
  echo $(docker service ps $name | awk -v ids="$ignore_ids" '(NR > 1) && ($1 !~ ids) {print $1}' | wc -l)
}

n=$(get_new_ids)
replicas=$(docker service inspect $name | grep -i replicas -m 1 | sed 's/[^0-9]//g')
timeout=$(($timeout_per_replica * $replicas))

echo ""
echo "     Update command:  $update_cmd"
echo "      # of replicas:  $replicas"
echo "    Timeout/replica:  $timeout_per_replica sec"
echo "      Total timeout:  $timeout sec"
echo "      Rollback delay:  $rollback_delay sec"
echo "    Stability delay:  $stability_delay sec"
echo -n "   Updating service:  "
eval $update_cmd
exit_code=$?
if [ "$exit_code" -ne 0 ]; then
  echo "ERROR: cannot update service"
  exit $exit_code
fi

# test that there are at least $replicas more containers after update
m=$n
x=0
echo -n " Remaining replicas:  "
while [ "$m" -lt "$(($n+$replicas))" ]; do
  i=$(get_new_ids)
  if [ "$i" -gt "$m" ]; then
    echo -n "$(($n+$replicas-$i+1)) "
  fi
  m=$(get_new_ids)
  if [ "$(($x * $delay))" -gt "$stability_delay" ]; then
    if [ "$m" -eq $n ]; then
      echo "SKIPPED: it seems that there is nothing to update"
      exit 0
    fi
  fi
  if [ "$(($x * $delay))" -gt "$timeout" ]; then
    echo "ERROR: update is taking too long time (timeout reached)"
    exit 2
  fi
  x=$(($x+1))
  sleep $delay
done
echo "done"
echo -n "     Stability test:  "

# test that there are no more containers during small timeframe
k=$(get_new_ids)
if [ "$k" -ne "$(($n+$replicas))" ]; then
  echo "failed"
  exit 3
fi
sleep $stability_delay
k=$(get_new_ids)
if [ "$k" -ne "$(($n+$replicas))" ]; then
  echo "failed"
  exit 3
fi
echo "done"
echo ""
echo "SUCCESS"