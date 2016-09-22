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

                    foreach (var result in principalSearcher.FindAll())
                    {
                        if (result is UserPrincipal)
                        {
                            var foundUserPrincipal = (UserPrincipal) result;

                            var user = bindUser(foundUserPrincipal);

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

                    foreach (var result in principalSearcher.FindAll())
                    {
                        if (result is UserPrincipal)
                        {
                            var foundUserPrincipal = (UserPrincipal)result;

                            var user = bindUser(foundUserPrincipal);

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

                    var result = principalSearcher.FindAll().SingleOrDefault(i => i.Name == name);

                    var foundGroupPrincipal = (GroupPrincipal) result;

                    var group = new Group()
                    {
                        Id = foundGroupPrincipal.Guid.ToString(),
                        Name = foundGroupPrincipal.Name,
                        Users = getUsers(foundGroupPrincipal.Members)
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
