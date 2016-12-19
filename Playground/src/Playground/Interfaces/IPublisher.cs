using System.Threading.Tasks;

namespace Playground.Exe.Interfaces
{
    public interface IPublisher
    {
        Task PublishAsync<T>(T model, string topic);
    }
}
