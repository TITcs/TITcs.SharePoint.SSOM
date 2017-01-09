using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SharePoint;
using Newtonsoft.Json;

namespace TITcs.SharePoint.SSOM
{
    public class User
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("login")]
        public string Login { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonIgnore]
        public string Claims { get; set; }
        [JsonIgnore]
        public ICollection<Group> Groups { get; set; }

        public static implicit operator User(SPFieldUserValue user)
        {
            if (user == null)
                return null;

            return new User
            {
                Id = user.User.ID.ToString(),
                Name = user.User.Name,
                Login = user.User.LoginName,
                Groups = user.User.Groups.Cast<SPGroup>().Select(i => new Group
                {
                    Id = i.ID.ToString(),
                    Name = i.Name
                }).ToList()
            };
        }

        public static implicit operator User(SPUser user)
        {
            if (user == null)
                return null;

            return new User
            {
                Id = user.ID.ToString(),
                Name = user.Name,
                Login = user.LoginName,
                Groups = user.Groups.Cast<SPGroup>().Select(i => new Group
                {
                    Id = i.ID.ToString(),
                    Name = i.Name
                }).ToList()
            };
        }

        public static User Current
        {
            get
            {
                User user = SPContext.Current.Web.CurrentUser;
                return user;
            }
        }
    }
}
