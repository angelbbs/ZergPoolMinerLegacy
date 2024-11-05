using System;

namespace MSI.Afterburner.Exceptions
{
    public class SharedMemoryVersionNotSupported : CustomErrorException
    {
        private const string errMsg = "Connected to an unsupported version of MSI Afterburner shared memory.";

        public SharedMemoryVersionNotSupported()
          : base("Connected to an unsupported version of MSI Afterburner shared memory.")
        {
        }

        public SharedMemoryVersionNotSupported(Exception innerEx)
          : base("Connected to an unsupported version of MSI Afterburner shared memory.", innerEx)
        {
        }
    }
}
