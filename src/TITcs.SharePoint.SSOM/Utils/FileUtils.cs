using System.IO;
using Microsoft.SharePoint;
using TITcs.SharePoint.SOM.Logger;

namespace TITcs.SharePoint.SSOM.Utils
{
    public class FileUtils
    {
        public static void Delete(string relativePath)
        {
            var site = SPContext.Current.Site;

            using (SPWeb web = site.OpenWeb())
            {
                if (!relativePath.StartsWith("/"))
                    relativePath = "/" + relativePath;

                var file = site.Url + relativePath;

                SPFile spFile = web.GetFile(file);

                if (spFile.Exists)
                    spFile.Delete();

                Logger.Information("FileUtils.Delete", "Deleted file: {0}", file);
            }
        }

        public static Stream GetStream(string relativePath)
        {
            var site = SPContext.Current.Site;

            if (!relativePath.StartsWith("/"))
                relativePath = "/" + relativePath;

            using (SPWeb web = site.OpenWeb())
            {
                var file = string.Format("{0}{1}", web.Url, relativePath);

                var spFile = web.GetFile(file);

                Logger.Information("FileUtils.GetStream", "RelativePath: {0}", file);

                return CopyStream(spFile);
            }
        }

        private static Stream CopyStream(SPFile spFile)
        {
            var memoryStream = new MemoryStream();

            using (var stream = spFile.OpenBinaryStream())
                stream.CopyTo(memoryStream);

            return memoryStream;
        }

        private static SPFile CheckOut(SPWeb web, string url)
        {
            SPFile file = web.GetFile(url);
            file.CheckOut();

            return file;
        }

        public static void CheckIn(SPFile file, bool publish)
        {
            file.CheckIn("", SPCheckinType.MajorCheckIn);

            if (publish)
                file.Publish("");
        }

        public static void CheckIn(SPWeb web, string url)
        {
            SPFile file = web.GetFile(url);
            file.CheckIn("", SPCheckinType.MajorCheckIn);
            file.Publish("");
        }
    }
}