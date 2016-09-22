using Microsoft.SharePoint;

namespace TITcs.SharePoint.SSOM.Utils
{
    public static class ChoiceColumn
    {
        public static void AddItem(SPWeb web, string listTitle, string fieldName, string item)
        {
            AddItem(web, listTitle, fieldName, new[] {item});
            ;
        }

        /// <summary>
        /// Add a new item to choice field.
        /// </summary>
        public static void AddItem(SPWeb web, string listTitle, string fieldName, string[] items)
        {
            var list = ListUtils.GetList(web, listTitle);

            if (list != null)
            {
                SPFieldChoice chFldGender = (SPFieldChoice)list.Fields[fieldName];

                foreach (var item in items)
                {
                    if (!chFldGender.Choices.Contains(item))
                        chFldGender.Choices.Add(item);
                }

                chFldGender.Update();

                list.Update();
            }
        }
    }
}
