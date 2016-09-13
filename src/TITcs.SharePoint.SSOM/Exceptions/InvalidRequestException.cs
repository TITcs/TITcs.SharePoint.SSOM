using System;

namespace TITcs.SharePoint.SSOM.Exceptions
{
    public class InvalidRequestException : Exception
    {
        public InvalidRequestException() : base("Tentativa de requisição inválida")
        {
        }

        public InvalidRequestException(string message) : base(string.Format("Tentativa de requisição inválida. {0}", message))
        {
        }
    }
}
