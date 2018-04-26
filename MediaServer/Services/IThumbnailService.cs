using System.Threading.Tasks;
using MediaServer.Models;
using Microsoft.AspNetCore.Http;

namespace MediaServer.Services
{
    public interface IThumbnailService
    {
		Task<Image> GetTalkThumbnail(Conference conference, string name);
		Task SaveThumbnail(Conference conference, Talk talk);
		Task<string> GetThumbnailUrl(Conference conference, Talk talk, HttpContext httpContext);
    }
}
