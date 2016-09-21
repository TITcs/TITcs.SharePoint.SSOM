using Microsoft.SharePoint;

namespace TITcs.SharePoint.SSOM.Utils
{
    public static class ChoiceColumn
    {
        /// <summary>
        /// Add a new item to choice field.
        /// </summary>
        /// <param name="web">Context web</param>
        /// <param name="listTitle">Name to list.</param>
        /// <param name="fieldName">Name to field (type choice)</param>
        /// <param name="item">Text to new item to add</param>
        public static void AddItem(SPWeb web, string listTitle, string fieldName, string item)
        {
            var list = ListUtils.GetList(web, listTitle);

            if (list != null)
            {
                SPFieldChoice chFldGender = (SPFieldChoice)list.Fields[fieldName];

                if (!chFldGender.Choices.Contains(item))
                    chFldGender.Choices.Add(item);

                chFldGender.Update();

                list.Update();
            }
        }
    }
}
