using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TITcs.SharePoint.SSOM.Utils
{
    public static class MethodUtils
    {
        public static TResult Call<TResult>(Func<TResult> method)
        {
            try
            {
                return method();
            }
            catch (Exception exception)
            {
                Logger.Logger.Unexpected("MethodUtils.Call", exception.Message);
                throw;
            }
        }

        public static void Exec(Action method)
        {
            try
            {
                method();
            }
            catch (Exception exception)
            {
                Logger.Logger.Unexpected("MethodUtils.Exec", exception.Message);
                throw;
            }
        }
    }
}
