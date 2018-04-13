using System;
using System.Threading.Tasks;
using MediaServer.Models;

namespace MediaServer.Services
{
    public interface ISlackService
    {
		Task PostTalkToChannel(Conference conference, Talk talk, string talkUrl);
    }
}
