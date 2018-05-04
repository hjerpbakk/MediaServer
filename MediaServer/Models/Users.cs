using System.Linq;
using System.Collections.Generic;

namespace MediaServer.Models
{
    public class Users
    {
		readonly IDictionary<string, User> users;

		public Users(User[] users) 
            => this.users = users.ToDictionary(u => u.Name);
        
		public User GetUser(string name) {
			if (users.ContainsKey(name)) {
				return users[name];
			}

			return new User(name, "/no-image.png", null, User.UnknownUser);
		}
    }
}
