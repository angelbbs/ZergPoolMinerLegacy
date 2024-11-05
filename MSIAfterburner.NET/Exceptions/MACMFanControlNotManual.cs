using System;

namespace MSI.Afterburner.Exceptions
{
    public class MACMFanControlNotManual : CustomErrorException
    {
        private const string errMsg = "Fan is currently set to auto.  Cannot set fan speed.";

        public MACMFanControlNotManual()
          : base("Fan is currently set to auto.  Cannot set fan speed.")
        {
        }

        public MACMFanControlNotManual(Exception innerEx)
          : base("Fan is currently set to auto.  Cannot set fan speed.", innerEx)
        {
        }
    }
}
