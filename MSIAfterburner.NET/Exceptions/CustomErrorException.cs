using System;

namespace MSI.Afterburner.Exceptions
{
    [Serializable]
    public class CustomErrorException : ApplicationException
    {
        public string ErrorMessage => this.Message.ToString();

        public CustomErrorException(string errorMessage)
          : base(errorMessage)
        {
        }

        public CustomErrorException(string errorMessage, Exception innerEx)
          : base(errorMessage, innerEx)
        {
        }
    }
}
