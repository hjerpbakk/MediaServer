using System;
using System.Threading.Tasks;
using MediaServer.Models;

namespace MediaServer.Services
{
    public interface IThumbnailService
    {
		Task<Image> GetTalkThumbnail(Conference conference, string name);
		Task SaveThumbnail(Conference conference, Talk talk);      
    }
}
