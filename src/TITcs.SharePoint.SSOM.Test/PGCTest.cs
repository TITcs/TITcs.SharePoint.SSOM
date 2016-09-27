using Microsoft.SharePoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TITcs.SharePoint.SSOM.Test
{
    [TestClass]
    public class PGCTest
    {
        #region fields and properties

        private readonly string URL = "http://pgc.213.dev/";
        private readonly string[] randomStrings = new string[] {
            @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aliquam sed nulla nisi. Sed ut tristique nulla, at faucibus elit. Nulla et purus erat. Nullam condimentum luctus sapien, non pretium magna sollicitudin ut. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Mauris porttitor vitae lacus at semper. Suspendisse potenti. Morbi ut lectus consequat, imperdiet nunc nec, feugiat massa. Morbi condimentum leo quis sapien mattis, eget consequat nunc dapibus.",
            @"Curabitur nec ex viverra, volutpat enim sit amet, rutrum ante. Morbi dui nisi, pellentesque eget risus eu, ornare aliquam diam. Pellentesque non dui ut dui placerat hendrerit ornare a quam. Cras aliquet, mi ultricies sodales dapibus, libero metus volutpat mi, eget tristique nulla nisi in libero. Proin odio est, vestibulum nec risus eu, egestas varius ex. Donec vitae leo vitae leo commodo pharetra nec quis quam. Sed a tempus turpis.",
            @"Donec commodo porta sapien ac elementum. Integer fringilla in libero nec laoreet. Duis lacinia magna euismod condimentum accumsan. Nunc hendrerit odio vitae ex semper semper. Cras mattis ante nisi, ac porta augue tristique sed. Mauris mattis velit et felis maximus rhoncus. Nunc hendrerit mi pharetra luctus tempus. Integer lobortis est ut lacinia consequat. Vestibulum sed mauris et felis porttitor semper. Duis viverra ipsum eu sem tristique, ac pulvinar ante pretium. Ut mollis est in enim tristique aliquet.",
            @"Suspendisse pharetra ante nec risus interdum ultricies. Vivamus augue diam, lobortis quis augue eu, gravida commodo metus. Donec eleifend sem lorem, sed volutpat lacus posuere finibus. In hac habitasse platea dictumst. Mauris nec sapien lacus. Quisque dictum et libero quis interdum. Nam id volutpat diam, sed ornare nibh. Etiam at congue sapien, nec vestibulum nulla. Mauris tincidunt nibh in arcu feugiat, et lacinia metus vulputate. Suspendisse potenti.",
            @"Cras molestie felis hendrerit ipsum mattis, id imperdiet nisi bibendum. Praesent congue, orci at commodo malesuada, massa leo aliquam odio, non bibendum sapien ipsum sed turpis. Maecenas non interdum urna. Nunc eget magna turpis. Phasellus eu porta nisl, a sollicitudin ex. Praesent id nulla scelerisque, commodo tortor vitae, facilisis lorem. Proin metus risus, consequat a consequat convallis, dictum in lacus. Nam in enim sit amet elit pellentesque volutpat."
        };

        #endregion

        #region events and methods

        [TestMethod]
        public void Deve_Conectar_No_Site()
        {
            using (SPSite site = new SPSite(URL))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    var repo = new MelhoresPraticasComentariosRepository(web);
                    Assert.IsTrue(!String.IsNullOrEmpty(repo.Title));
                }
            }
        }

        #endregion
    }
}
