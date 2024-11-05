using System;

namespace MSI.Afterburner.Exceptions
{
    public class SharedMemoryNotFound : CustomErrorException
    {
        private const string errMsg = "Could not connect to MSI Afterburner 2.1 or later.";

        public SharedMemoryNotFound()
          : base("Could not connect to MSI Afterburner 2.1 or later.")
        {
        }

        public SharedMemoryNotFound(Exception innerEx)
          : base("Could not connect to MSI Afterburner 2.1 or later.", innerEx)
        {
        }
    }
}
