

namespace IoTGatewayAPI.Contracts
{
    public interface IAzureTableRepository<TResult>
    {
        Task<TResult> Get(string id);
        //Task<IEnumerable<TResult>> GetAll();
        Task CreateOrUpdate(TResult record);
    }
}
