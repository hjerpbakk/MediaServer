using System;
using System.Threading.Tasks;
using MediaServer.Models;

namespace MediaServer.Services
{
    public class DummyTalkService : ITalkService
    {
		public Task<Talk[]> GetTalksFromConference(Conference conference) {

			return Task.FromResult(new[] { 
                new Talk("IoC-container i Arena-plugin", "Really gudd talk", "../../images/thumbs/henrik.png", DateTime.UtcNow, "Henrik Heggelund Berg"),
                new Talk("no talk without slides", "Lorem ipsum dolor sit amet, consectetur adipisicing elit. Amet numquam aspernatur!", "http://placehold.it/700x400", DateTime.UtcNow - TimeSpan.FromDays(5), "NN"),
                new Talk("Gudd talk", "So many gudd points", "http://placehold.it/700x400", DateTime.UtcNow - TimeSpan.FromDays(8), "NN")
			});         
        }

		public Task<Talk> GetTalkByName(Conference conference, string name) {
            if (name == "IoC-container i Arena-plugin") {
				return Task.FromResult(new Talk("IoC-container i Arena-plugin", "Really gudd talk", "../../images/thumbs/henrik.png", DateTime.UtcNow, "Henrik Heggelund Berg", "IoC-container i Arena-plugin.pdf"));    
            }

            if (name == "Gudd talk") {
				return Task.FromResult(new Talk("Gudd talk", "Lorem ipsum dolor sit amet, consectetur adipisicing elit. Amet numquam aspernatur! Lorem ipsum dolor sit amet.", "http://placehold.it/700x400", DateTime.UtcNow - TimeSpan.FromDays(8), "NN", "http://seesharper.github.io/Presentations/DotNetCore/#1"));    
            }

            if (name == "no talk without slides") {
				return Task.FromResult(new Talk("no talk without slides", "Lorem ipsum dolor sit amet, consectetur adipisicing elit. Amet numquam aspernatur!", "http://placehold.it/700x400", DateTime.UtcNow - TimeSpan.FromDays(5), "NN"));
            }

            return null;
        }

		public Task SaveTalkFromConference(Conference conference, Talk talk) {
			return Task.CompletedTask;
		}
    }
}
