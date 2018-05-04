using System.Threading.Tasks;
using MediaServer.Models;

namespace MediaServer.Clients {
    public interface ISlackMetaClient {
        Task<User[]> GetUsers();
        Task PopulateMetaData();
    }
}