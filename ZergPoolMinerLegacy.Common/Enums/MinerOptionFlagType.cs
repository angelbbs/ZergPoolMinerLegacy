﻿namespace ZergPoolMinerLegacy.Common.Enums
{
    // indicates if uni flag (no parameter), single param or multi param
    public enum MinerOptionFlagType
    {
        NanoMiner,
        Uni,
        SingleParam,
        MultiParam,
        DuplicateMultiParam // the flag is multiplied
    }
}
