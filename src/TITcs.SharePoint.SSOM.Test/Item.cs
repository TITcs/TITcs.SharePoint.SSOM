namespace TITcs.SharePoint.SSOM.Test
{
    public class Item : SharePointItem
    {
        [SharePointField("WikiField")]
        public string Content { get; set; }

    }
}
