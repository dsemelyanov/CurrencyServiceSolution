using System.Xml.Linq;

namespace BackgroundService.Services.HttpClients.CbrSender
{
    public interface ICbrSender
    {
        Task<XDocument?> GetCbr(CancellationToken cancellationToken);
        Task<XDocument?> GetCbrLoad(CancellationToken cancellationToken);
    }
}
