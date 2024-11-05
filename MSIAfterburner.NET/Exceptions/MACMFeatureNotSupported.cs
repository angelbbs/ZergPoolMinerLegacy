using System;

namespace MSI.Afterburner.Exceptions
{
    public class MACMFeatureNotSupported : CustomErrorException
    {
        private const string errMsg = "You hardware does not support this feature.";

        public MACMFeatureNotSupported()
          : base("You hardware does not support this feature.")
        {
        }

        public MACMFeatureNotSupported(Exception innerEx)
          : base("You hardware does not support this feature.", innerEx)
        {
        }

        public MACMFeatureNotSupported(string errorMessage)
          : base(errorMessage)
        {
        }
    }
}
