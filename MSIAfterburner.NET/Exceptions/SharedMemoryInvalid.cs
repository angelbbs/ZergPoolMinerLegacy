using System;

namespace MSI.Afterburner.Exceptions
{
    public class SharedMemoryInvalid : CustomErrorException
    {
        private const string errMsg = "Connected to invalid MSI Afterburner shared memory.";

        public SharedMemoryInvalid()
          : base("Connected to invalid MSI Afterburner shared memory.")
        {
        }

        public SharedMemoryInvalid(Exception innerEx)
          : base("Connected to invalid MSI Afterburner shared memory.", innerEx)
        {
        }
    }
}
