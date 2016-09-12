using System;
using System.Linq;
using Microsoft.SharePoint;

namespace TITcs.SharePoint.SSOM.Utils
{
    public static class LikesColumn
    {
        /// <summary>
        /// Active likes column in list
        /// </summary>
        /// <param name="web"></param>
        /// <param name="listTitle"></param>
        public static void Activate(SPWeb web, string listTitle)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.Load("Microsoft.SharePoint.Portal, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c");
            Type reputationHelper = assembly.GetType("Microsoft.SharePoint.Portal.ReputationHelper");
            System.Reflection.MethodInfo method = reputationHelper.GetMethod("EnableReputation", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            SPList list = ListUtils.GetList(web, listTitle);

            method.Invoke(null, new Object[] { list, "Likes", false });
        }

        public static void Like(SPWeb web, SPUser user, SPListItem listItem)
        {
            try
            {
                SPFieldUserValueCollection likedBy = new SPFieldUserValueCollection(web, listItem["LikedBy"].ToString());

                SPFieldUserValue newUser = new SPFieldUserValue(web, user.ID, user.Name);

                likedBy.Add(newUser);

                int likes = likedBy.Distinct().Count();

                listItem["LikesCount"] = likes;
                listItem["LikedBy"] = likedBy;
                listItem.SystemUpdate(false);
            }
            catch (Exception e)
            {
                Logger.Logger.Unexpected("LikesColumn.GetCountLikeItem", e.Message);
            }
        }
    }
}
