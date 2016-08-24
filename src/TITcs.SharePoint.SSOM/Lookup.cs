
namespace TITcs.SharePoint.SSOM
{
    public class Lookup
    {
        public Lookup(int id, string text)
        {
            Id = id;
            Text = text;
        }
        public int Id { get; set; }
        public string Text { get; set; }
    }
}
