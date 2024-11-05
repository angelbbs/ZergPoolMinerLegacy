using System;

namespace MSI.Afterburner.Exceptions
{
    public class SharedMemoryDead : CustomErrorException
    {
        private const string errMsg = "Connected to MSI Afterburner shared memory that is flagged as dead.";

        public SharedMemoryDead()
          : base("Connected to MSI Afterburner shared memory that is flagged as dead.")
        {
        }

        public SharedMemoryDead(Exception innerEx)
          : base("Connected to MSI Afterburner shared memory that is flagged as dead.", innerEx)
        {
        }
    }
}
