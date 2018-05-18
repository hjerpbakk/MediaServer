using System.Linq;
using System.Collections.Generic;

namespace MediaServer.Models {
    public class Users {
		static readonly User dipsUser;

		readonly Dictionary<string, User> users;

		static Users() {
			dipsUser = new User("DIPS", "/ms-icon-310x310.png", description: "Together we create the world's most efficient health care software!");   
		}

		public Users(User[] users) 
            => this.users = users.ToDictionary(u => u.Name);
        
		public User GetUser(string name) {
			if (name == dipsUser.Name) {
				return dipsUser;
			}

			return users.ContainsKey(name) 
                    ? users[name] 
                    : new User(name, "/no-image.png"); 
		}
    }
}
