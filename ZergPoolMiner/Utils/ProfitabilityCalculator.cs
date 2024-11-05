using ZergPoolMinerLegacy.Common.Enums;
using System.Collections.Generic;

namespace ZergPoolMiner
{
    internal static class ProfitabilityCalculator
    {
        private const double kHs = 1000;
        private const double MHs = 1000000;
        private const double GHs = 1000000000;
        private const double THs = 1000000000000;

        // divide factor to mirror web values
        private static readonly Dictionary<AlgorithmType, double> Div = new Dictionary<AlgorithmType, double>
        {
            { AlgorithmType.INVALID , 1 },
            { AlgorithmType.NONE , 1 },
            { AlgorithmType.Empty,                  MHs }, // NOT used
        };

        public static double GetFormatedSpeed(double speed, AlgorithmType type)
        {
            if (Div.ContainsKey(type))
            {
                return speed / Div[type];
            }
            return speed; // should never happen
        }
    }
}
