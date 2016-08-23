
namespace TITcs.SharePoint.SOM.Extensions
{
    public static class ConvertExtensions
    {
        public static bool ToBool(this string value)
        {
            bool result = false;
            bool.TryParse(value, out result);
            return result;
        }

        public static int ToInt32(this string value)
        {
            int result = 0;
            int.TryParse(value, out result);
            return result;
        }

        public static int ToInt32(this double value)
        {
            int result = 0;
            int.TryParse(value.ToString(), out result);
            return result;
        }

        public static long ToInt64(this string value)
        {
            long result = 0;
            long.TryParse(value, out result);
            return result;
        }

        public static double ToDouble(this string value)
        {
            var result = 0.0;
            double.TryParse(value, out result);
            return result;
        }

        public static decimal ToDecimal(this string value)
        {
            decimal result = 0;
            decimal.TryParse(value, out result);
            return result;
        }
    }
}
