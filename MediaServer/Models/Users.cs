using System.Linq;
using System.Collections.Generic;

namespace MediaServer.Models {
    public class Users {
		readonly Dictionary<string, User> users;

		public Users(User[] users) 
            => this.users = users.ToDictionary(u => u.Name);
        
		public User GetUser(string name) 
			=> users.ContainsKey(name) 
		            ? users[name] 
		            : new User(name, "/no-image.png", null, User.UnknownUser);      
    }
}
