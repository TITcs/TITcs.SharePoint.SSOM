using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices.AccountManagement;
using System.Linq;

namespace TITcs.SharePoint.SSOM.ActiveDirectory
{
    public class Search
    {
        private PrincipalContext[] _contexts { get; set; }

        public Search(PrincipalContext[] contexts)
        {
            _contexts = contexts;
        }

        public User GetUser(string loginName)
        {
            foreach (var context in _contexts)
            {
                using (UserPrincipal userPrincipal = new UserPrincipal(context))
                {
                    userPrincipal.SamAccountName = loginName;

                    var principalSearcher = new PrincipalSearcher(userPrincipal);

                    foreach (var principal in principalSearcher.FindAll())
                    {
                        if (principal is UserPrincipal)
                        {
                            var userPrincipal1 = (UserPrincipal) principal;

                            var user = bindUser(userPrincipal1);

                            return user;
                        }
                    }
                }
            }

            return null;
        }

        public User GetUserByDisplayName(string displayName)
        {
            foreach (var context in _contexts)
            {
                using (UserPrincipal userPrincipal = new UserPrincipal(context))
                {
                    userPrincipal.DisplayName = displayName;

                    var principalSearcher = new PrincipalSearcher(userPrincipal);

                    foreach (var principal in principalSearcher.FindAll())
                    {
                        if (principal is UserPrincipal)
                        {
                            var userPrincipal1 = (UserPrincipal)principal;

                            var user = bindUser(userPrincipal1);

                            return user;
                        }
                    }
                }
            }

            return null;
        }

        private static User bindUser(UserPrincipal userPrincipal)
        {
            var user = new User()
            {
                Id = userPrincipal.Guid.ToString(),
                Email = userPrincipal.EmailAddress,
                Name = userPrincipal.Name,
                Login = userPrincipal.SamAccountName,
                Groups = userPrincipal.GetAuthorizationGroups().Select(i => new Group()
                {
                    Name = i.Name,
                    Id = i.Guid.ToString()
                }).ToArray()
            };
            return user;
        }

        public Group GetGroup(string name)
        {
            foreach (var context in _contexts)
            {
                using (GroupPrincipal groupPrincipal = new GroupPrincipal(context))
                {
                    PrincipalSearcher principalSearcher = new PrincipalSearcher(groupPrincipal);

                    var principal = principalSearcher.FindAll().SingleOrDefault(i => i.Name == name);

                    var groupPrincipal1 = (GroupPrincipal) principal;

                    var group = new Group()
                    {
                        Id = groupPrincipal1.Guid.ToString(),
                        Name = groupPrincipal1.Name,
                        Users = getUsers(groupPrincipal1.Members)
                    };

                    return group;
                }
            }

            return null;
        }

        private static ICollection<User> getUsers(PrincipalCollection principalCollection)
        {
            var users = new Collection<User>();

            foreach (var principal in principalCollection)
            {
                if (principal is UserPrincipal)
                {
                    var foundUserPrincipal = (UserPrincipal)principal;
                    users.Add(bindUser(foundUserPrincipal));
                }
            }

            return users;
        }

    }
}
