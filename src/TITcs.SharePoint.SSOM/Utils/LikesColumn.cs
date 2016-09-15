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
                SPFieldUserValueCollection likedBy;

                if (listItem["LikedBy"] != null)
                {
                    likedBy = new SPFieldUserValueCollection(web, Convert.ToString(listItem["LikedBy"].ToString()));
                }
                else
                {
                    likedBy = new SPFieldUserValueCollection();
                }

                var fieldUserValue = new SPFieldUserValue(web, user.ID, user.Name);

                var liked = likedBy.Any(a => a.User.LoginName.Equals(user.LoginName));

                if (!liked)
                {
                    int likes = likedBy.Distinct().Count();

                    likedBy.Add(fieldUserValue);
                    likes = likes + 1;

                    listItem["LikesCount"] = likes;
                    listItem["LikedBy"] = likedBy;

                    listItem.SystemUpdate(false);
                }
            }
            catch (Exception e)
            {
                Logger.Logger.Unexpected("LikesColumn.Like", e.Message);
                throw e;
            }
        }

        public static void UnLike(SPWeb web, SPUser user, SPListItem listItem)
        {
            try
            {
                if (listItem["LikedBy"] != null)
                {
                    var likedBy = new SPFieldUserValueCollection(web, listItem["LikedBy"].ToString());

                    if (likedBy.Any(f => f.LookupId == user.ID))
                    {
                        var deleteUser = likedBy.First(f => f.LookupId == user.ID);

                        int likes = likedBy.Distinct().Count();

                        likedBy.Remove(deleteUser);
                        likes = likes - 1;
                        listItem["LikesCount"] = likes;
                        listItem["LikedBy"] = likedBy;

                        listItem.SystemUpdate(false);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Logger.Unexpected("LikesColumn.UnLike", e.Message);
                throw e;
            }
        }
    }
}
