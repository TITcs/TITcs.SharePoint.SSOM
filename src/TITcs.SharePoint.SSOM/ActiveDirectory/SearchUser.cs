using System;
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
                    if (!string.IsNullOrEmpty(loginName))
                        userPrincipal.SamAccountName = loginName;

                    var principalSearcher = new PrincipalSearcher(userPrincipal);

                    foreach (var found in principalSearcher.FindAll())
                    {
                        if (found is UserPrincipal)
                        {
                            var foundUserPrincipal = (UserPrincipal) found;

                            var user = new User()
                            {
                                Id = foundUserPrincipal.Guid.ToString(),
                                Email = foundUserPrincipal.EmailAddress,
                                Name = foundUserPrincipal.Name,
                                Login = foundUserPrincipal.SamAccountName,
                                Groups = foundUserPrincipal.GetAuthorizationGroups().Select(i => new Group()
                                {
                                    Name = i.Name,
                                    Id = i.Guid.ToString()
                                }).ToArray()
                            };

                            return user;
                        }
                    }
                }
            }

            return null;
        }

        public Group GetGroup(string name)
        {
            throw new NotImplementedException();
        }
    }
}
