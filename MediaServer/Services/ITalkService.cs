using System;
using System.Threading.Tasks;
using MediaServer.Models;

namespace MediaServer.Services
{
    public interface ITalkService
    {
      Task<Talk[]> GetTalksFromConference(Conference conference);
      Task<Talk> GetTalkByName(Conference conference, string name);
      Task SaveTalkFromConference(Conference conference, Talk talk);
	  Task DeleteTalkFromConference(Conference conference, Talk talk);
      Task<string[]> GetTalkNamesFromConference(Conference conference);
    }
}
