﻿namespace ZergPoolMinerLegacy.Common.Enums
{
    public enum AlgorithmType
    {

        // dual algos for grouping
        ZIL = -200,
        KawPowLite = -100,

        Ethashb3SHA512256d = -17,
        Ethashb3PyrinHashV2 = -16,
        //Ethashb3KarlsenHash = -15,

        KarlsenHashV2PyrinHashV2 = -14,
        KarlsenHashV2HooHash = -13,
        EthashSHA512256d = -12,
        EthashPyrinHashV2 = -11,
        //EthashKarlsenHash = -10,

        INVALID = -2,
        NONE = -1,

        Empty = 0,

        Allium = 1000,//_UNUSED
        BMW512 = 1010,//_UNUSED
        CPUPower = 1020,
        Cryptonight_GPU = 1030,
        Cryptonight_UPX = 1040,
        CurveHash = 1050,
        EvrProgPow = 1060,//4gb
        Equihash125 = 1070,
        Equihash144 = 1080,
        Equihash192 = 1090,
        Ethash = 1100,
        Ethashb3 = 1110,
        Flex = 1120,

        FiroPow = 1130,//lite ready. mc=FIRO/KIIRO
        //Frkhash = 1140,
        Ghostrider = 1150,

        HeavyHash = 1160,
        HooHash = 1170,
        //KarlsenHash = 1180,
        KarlsenHashV2 = 1190,
        KawPow = 1200,
        Keccakc = 1210,
        Megabtx = 1220,
        MeowPow = 1225,
        Meraki = 1230,//1gb
        Mike = 1240,
        Minotaurx = 1250,
        NeoScrypt = 1260,
        Neoscrypt_xaya = 1270,
        NexaPow = 1280,//2gb
        NxlHash = 1287,
        Panthera = 1290,
        Power2b = 1295,
        PyrinHashV2 = 1300,
        RandomX = 1310,
        RandomARQ = 1320,
        RandomXEQ = 1330,
        Scryptn2 = 1340, //https://github.com/JayDDee/cpuminer-opt
        Skydoge = 1350,//
        //SHA3d = 1360,//no mem
        SHA256csm = 1370,
        SHA256dt = 1380,//no mem
        SHA512256d = 1390,//no mem
        VertHash = 1400,//1.5gb
        VerusHash = 1410,//1gb
        Whirlpool = 1420,
        X16RT = 1430,
        X16RV2 = 1440,//no mem
        X21S = 1450,//no mem
        X25X = 1460,//no mem
        Xelisv2_Pepew = 1470,
        Yescrypt = 1480,
        YescryptR8 = 1490,
        YescryptR16 = 1500,
        YescryptR32 = 1510,
        Yespower = 1520,
        YespowerLTNCG = 1530,
        YespowerMGPC = 1540,
        YespowerR16 = 1550,
        YespowerSUGAR = 1560,
        YespowerTIDE = 1570,
        YespowerURX = 1580
    }
}
