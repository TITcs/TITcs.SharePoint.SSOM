using System.Collections.Generic;

namespace TITcs.SharePoint.SSOM
{
    public class User
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Login { get; set; }
        public string Claims { get; set; }
        public ICollection<Group> Groups { get; set; }
    }
}
