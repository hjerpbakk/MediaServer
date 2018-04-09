using Hjerpbakk.Media.Server.Model;

namespace Hjerpbakk.Media.Server.Configuration
{
    public interface IConferenceConfiguration
    {
        Conference[] Conferences { get; }
        Conference GetConference(string conferenceId);
    }
}
