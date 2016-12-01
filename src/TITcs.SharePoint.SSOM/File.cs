using System;
using System.IO;

namespace TITcs.SharePoint.SSOM
{
    public class File
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public DateTime Created { get; set; }
        public long Length { get; set; }
        public string Url { get; set; }
        public string Extension { get; set; }
        public byte[] Content { get; set; }

        public static implicit operator File(Microsoft.SharePoint.SPFile file)
        {
            if (file == null)
                return null;

            return new File
            {
                Id = Convert.ToInt32(file.Item["Id"]),
                Name = file.Name,
                Title = file.Title,
                Created = file.TimeCreated,
                Length = file.Length,
                Url = file.ServerRelativeUrl,
                Extension = Path.GetExtension(file.ServerRelativeUrl),
                Content = file.OpenBinary()
            };
        }
    }
}
