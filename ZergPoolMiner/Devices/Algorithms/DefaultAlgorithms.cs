using ZergPoolMiner.Algorithms;
using ZergPoolMinerLegacy.Common.Enums;
using ZergPoolMinerLegacy.Extensions;
using System.Collections.Generic;
using System;

namespace ZergPoolMiner.Devices.Algorithms
{
    public static class DefaultAlgorithms
    {
        #region All

        private static Dictionary<MinerBaseType, List<Algorithm>> All => new Dictionary<MinerBaseType, List<Algorithm>>
        {
            {

                MinerBaseType.NONE,
                new List<Algorithm>
                {

                }
            }
        };

        #endregion

        #region GPU

        private static Dictionary<MinerBaseType, List<Algorithm>> Gpu => new Dictionary<MinerBaseType, List<Algorithm>>
        {

            {
                MinerBaseType.Nanominer,
                new List<Algorithm>()
                {

                }
            },
        };

        #endregion

        #region CPU

        public static Dictionary<MinerBaseType, List<Algorithm>> Cpu => new Dictionary<MinerBaseType, List<Algorithm>>
        {
            {
                MinerBaseType.Xmrig,
                new List<Algorithm>()
                {
                            new Algorithm(MinerBaseType.Xmrig, AlgorithmType.RandomX, "RandomX")
                            {
                            },
                            new Algorithm(MinerBaseType.Xmrig, AlgorithmType.Ghostrider, "Ghostrider")
                            {
                            }
                }
            },
            {
            MinerBaseType.Nanominer,
                new List<Algorithm>()
                {
                    new Algorithm(MinerBaseType.Nanominer, AlgorithmType.VerusHash, "VerusHash")
                    {
                        ExtraLaunchParameters = ""
                    }
                }
            },
            {
                MinerBaseType.SRBMiner,
                        new List<Algorithm>() {
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.RandomX, "RandomX")
                            {
                              ExtraLaunchParameters = "--max-rejected-shares 100 --cpu-threads " + (Environment.ProcessorCount).ToString()
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.VerusHash, "VerusHash")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100 --cpu-threads " + (Environment.ProcessorCount).ToString()
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.CPUPower, "CPUPower")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100 --cpu-threads " + (Environment.ProcessorCount).ToString()
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.Cryptonight_UPX, "Cryptonight_UPX")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100 --cpu-threads " + (Environment.ProcessorCount).ToString()
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.Flex, "Flex")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100 --cpu-threads " + (Environment.ProcessorCount).ToString()
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.Ghostrider, "Ghostrider")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100 --cpu-threads " + (Environment.ProcessorCount).ToString()
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.Mike, "Mike")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100 --cpu-threads " + (Environment.ProcessorCount).ToString()
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.Minotaurx, "Minotaurx")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100 --cpu-threads " + (Environment.ProcessorCount).ToString()
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.Panthera, "Panthera")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100 --cpu-threads " + (Environment.ProcessorCount).ToString()
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.Xelisv2_Pepew, "XelisV2-Pepew")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100 --cpu-threads " + (Environment.ProcessorCount).ToString()
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.Yespower, "Yespower")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100 --cpu-threads " + (Environment.ProcessorCount).ToString()
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.YespowerLTNCG, "YespowerLTNCG")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100 --cpu-threads " + (Environment.ProcessorCount).ToString()
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.YespowerMGPC, "YespowerMGPC")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100 --cpu-threads " + (Environment.ProcessorCount).ToString()
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.YespowerR16, "YespowerR16")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100 --cpu-threads " + (Environment.ProcessorCount).ToString()
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.YespowerSUGAR, "YespowerSUGAR")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100 --cpu-threads " + (Environment.ProcessorCount).ToString()
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.YespowerTIDE, "YespowerTIDE")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100 --cpu-threads " + (Environment.ProcessorCount).ToString()
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.YespowerURX, "YespowerURX")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100 --cpu-threads " + (Environment.ProcessorCount).ToString()
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.RandomARQ, "RandomARQ")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100 --cpu-threads " + (Environment.ProcessorCount).ToString()
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.RandomXEQ, "RandomXEQ")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100 --cpu-threads " + (Environment.ProcessorCount).ToString()
                            },
                            /*
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.Yescrypt, "Yescrypt")
                            {
                            ExtraLaunchParameters = "--max-rejected-shares 100 --cpu-threads " + (ComputeDeviceManager.CoresCount).ToString()
                            },
                            */
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.YescryptR8, "YescryptR8")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100 --cpu-threads " + (ComputeDeviceManager.CoresCount).ToString()
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.YescryptR16, "YescryptR16")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100 --cpu-threads " + (ComputeDeviceManager.CoresCount).ToString()
                            },
                            /*
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.YescryptR32, "YescryptR32")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100 --cpu-threads " + (ComputeDeviceManager.CoresCount).ToString()
                            },
                            */
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.Power2b, "Power2b")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100 --cpu-threads " + (ComputeDeviceManager.CoresCount).ToString()
                            }
                        }
            }
        }.ConcatDict(All);

        #endregion

        #region INTEL
        public static Dictionary<MinerBaseType, List<Algorithm>> Intel => new Dictionary<MinerBaseType, List<Algorithm>>
        {
            {
                MinerBaseType.SRBMiner,
                        new List<Algorithm>()
                        {
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.VertHash, "VertHash")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            /*
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.SHA3d, "SHA3d")
                            {
                            },
                            */
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.SHA256dt, "SHA256dt")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            /*
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.SHA512256d, "SHA512256d")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            */
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.Ethash, "Ethash")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.Ethashb3, "Ethashb3")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.EvrProgPow, "EvrProgPow")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.FiroPow, "FiroPow")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.HeavyHash, "HeavyHash")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            /*
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.KarlsenHash, "KarlsenHash")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            */
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.KarlsenHashV2, "KarlsenHashV2")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.KawPow, "KawPow")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.Meraki, "Meraki")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.VerusHash, "VerusHash")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            /*
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.PyrinHashV2, "PyrinHashV2")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            */
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.HooHash, "HooHash")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new DualAlgorithm(MinerBaseType.SRBMiner, AlgorithmType.KarlsenHashV2,
                                AlgorithmType.HooHash, "KarlsenHashV2+HooHash")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            /*
                            new DualAlgorithm(MinerBaseType.SRBMiner, AlgorithmType.KarlsenHashV2,
                                AlgorithmType.PyrinHashV2, "KarlsenHashV2+PyrinHashV2")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            */
                        }
            },
            {
                MinerBaseType.lolMiner,
                        new List<Algorithm>() {
                            new Algorithm(MinerBaseType.lolMiner, AlgorithmType.Equihash125, "Equihash125")
                            {
                                ExtraLaunchParameters = ""
                            },
                            new Algorithm(MinerBaseType.lolMiner, AlgorithmType.Equihash144, "Equihash144")
                            {
                                ExtraLaunchParameters = ""
                            },
                            /*
                            new Algorithm(MinerBaseType.lolMiner, AlgorithmType.SHA512256d, "SHA512256d")
                            {
                                ExtraLaunchParameters = ""
                            }
                            */
                        }
            },

            {
                MinerBaseType.Nanominer,
                new List<Algorithm>()
                {
                    new Algorithm(MinerBaseType.Nanominer, AlgorithmType.Ethash, "Ethash")
                    {
                        ExtraLaunchParameters = ""
                    },
                    new Algorithm(MinerBaseType.Nanominer, AlgorithmType.KawPow, "KawPow")
                    {
                        ExtraLaunchParameters = ""
                    }
                }
            },

        };

        #endregion

        #region AMD

        public static Dictionary<MinerBaseType, List<Algorithm>> Amd => new Dictionary<MinerBaseType, List<Algorithm>>
        {
            {
                MinerBaseType.SRBMiner,
                        new List<Algorithm>() {
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.VertHash, "VertHash")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            /*
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.SHA3d, "SHA3d")
                            {
                            },
                            */
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.SHA256dt, "SHA256dt")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            /*
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.SHA512256d, "SHA512256d")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            */
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.Cryptonight_GPU, "Cryptonight_GPU")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.Cryptonight_UPX, "Cryptonight_UPX")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.CurveHash, "CurveHash")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            /*
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.Ethash, "Ethash")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            */
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.Ethashb3, "Ethashb3")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.EvrProgPow, "EvrProgPow")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.FiroPow, "FiroPow")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.HeavyHash, "HeavyHash")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.PhiHash, "PhiHash")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.KarlsenHashV2, "KarlsenHashV2")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.KawPow, "KawPow")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.Meraki, "Meraki")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            /*
                             //unstable
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.NxlHash, "NxlHash")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            */
                            /*
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.PyrinHashV2, "PyrinHashV2")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            */
                            /*
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.Yescrypt, "Yescrypt")
                            {
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.YescryptR16, "YescryptR16")
                            {
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.YescryptR32, "YescryptR32")
                            {
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.YescryptR8, "YescryptR8")
                            {
                            },
                            */
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.VerusHash, "VerusHash")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.HooHash, "HooHash")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new DualAlgorithm(MinerBaseType.SRBMiner, AlgorithmType.KarlsenHashV2,
                                AlgorithmType.HooHash, "KarlsenHashV2+HooHash")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            /*
                            new DualAlgorithm(MinerBaseType.SRBMiner, AlgorithmType.KarlsenHashV2,
                                AlgorithmType.PyrinHashV2, "KarlsenHashV2+PyrinHashV2")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            */
                        }
            },
            {
                MinerBaseType.GMiner,
                    new List<Algorithm>
                    {
                    new Algorithm(MinerBaseType.GMiner, AlgorithmType.Ethash, "Ethash"),
                    new Algorithm(MinerBaseType.GMiner, AlgorithmType.KawPow, "KawPow")
                    {
                    },
                    /*
                    new Algorithm(MinerBaseType.GMiner, AlgorithmType.KawPowLite, "KAWPOWLite")
                    {
                    },
                    */
                    new Algorithm(MinerBaseType.GMiner, AlgorithmType.Equihash125, "Equihash125")
                    {
                    },
                    new Algorithm(MinerBaseType.GMiner, AlgorithmType.Equihash144, "Equihash144")
                    {
                    },
                    /*
                    new Algorithm(MinerBaseType.GMiner, AlgorithmType.FiroPow, "FiroPow")
                    {
                    },
                    */
                }
            },
            {
                MinerBaseType.teamredminer,
                        new List<Algorithm>() {

                            new Algorithm(MinerBaseType.teamredminer, AlgorithmType.Ethash, "Ethash"),
                            new Algorithm(MinerBaseType.teamredminer, AlgorithmType.KawPow, "KawPow"),
                            //new Algorithm(MinerBaseType.teamredminer, AlgorithmType.KarlsenHash, "KarlsenHash"),

                            /*
                            new DualAlgorithm(MinerBaseType.teamredminer, AlgorithmType.Autolykos, AlgorithmType.KHeavyHash, "AutolykosKHeavyHash")
                            {
                                ExtraLaunchParameters = "--kas_end"
                            },
                            */
                            
                        }
            },
            {
                MinerBaseType.lolMiner,
                        new List<Algorithm>() {
                            new Algorithm(MinerBaseType.lolMiner, AlgorithmType.Equihash125, "Equihash125")
                            {
                                ExtraLaunchParameters = ""
                            },
                            new Algorithm(MinerBaseType.lolMiner, AlgorithmType.Equihash144, "Equihash144")
                            {
                                ExtraLaunchParameters = ""
                            },
                            /*
                            //не работает
                            new Algorithm(MinerBaseType.lolMiner, AlgorithmType.Equihash192, "Equihash192")
                            {
                                ExtraLaunchParameters = ""
                            },
                            */
                            /*
                            new Algorithm(MinerBaseType.lolMiner, AlgorithmType.KarlsenHash, "KarlsenHash")
                            {
                                ExtraLaunchParameters = ""
                            },
                            */
                            new Algorithm(MinerBaseType.lolMiner, AlgorithmType.KarlsenHashV2, "KarlsenHashV2")
                            {
                                ExtraLaunchParameters = ""
                            },
                            new Algorithm(MinerBaseType.lolMiner, AlgorithmType.NexaPow, "NexaPow")
                            {
                                ExtraLaunchParameters = "--keepfree 1024"
                            }
                            /*
                            new Algorithm(MinerBaseType.lolMiner, AlgorithmType.PyrinHashV2, "PyrinHashV2")
                            {
                                ExtraLaunchParameters = ""
                            },
                            */
                            /*
                            new Algorithm(MinerBaseType.lolMiner, AlgorithmType.SHA512256d, "SHA512256d")
                            {
                                ExtraLaunchParameters = ""
                            },
                            */
                            /*
                            new Algorithm(MinerBaseType.lolMiner, AlgorithmType.Ethash, "Ethash")
                            {
                                ExtraLaunchParameters = "--enablezilcache=1"
                            },
                            */
                            //duals
                            /*
                            new DualAlgorithm(MinerBaseType.lolMiner, AlgorithmType.KarlsenHashV2,
                                AlgorithmType.PyrinHashV2, "KarlsenHashV2+PyrinHashV2")
                            {
                            },
                            */
                        }
            },
            {
                MinerBaseType.Claymore,
                            new List<Algorithm>
                            {
                                new Algorithm(MinerBaseType.Claymore, AlgorithmType.NeoScrypt, "NeoScrypt"),
                            }
            },
            {
                MinerBaseType.Nanominer,
                new List<Algorithm>()
                {
                    /*
                    new Algorithm(MinerBaseType.Nanominer, AlgorithmType.Autolykos, "Autolykos")
                    {
                        ExtraLaunchParameters = "memTweak=1"
                    }
                    */
                }
            },
            { MinerBaseType.miniZ,
                        new List<Algorithm>() {
                            new Algorithm(MinerBaseType.miniZ, AlgorithmType.Equihash125, "Equihash125")
                            {
                            },
                            new Algorithm(MinerBaseType.miniZ, AlgorithmType.Equihash144, "Equihash144")
                            {
                            },
                            new Algorithm(MinerBaseType.miniZ, AlgorithmType.Equihash192, "Equihash192")
                            {
                            },
                            /*
                            //не работает
                            new Algorithm(MinerBaseType.miniZ, AlgorithmType.EvrProgPow, "EvrProgPow")
                            {
                            },
                            */
                            new Algorithm(MinerBaseType.miniZ, AlgorithmType.Ethashb3, "Ethashb3")
                            {
                            },
                            new Algorithm(MinerBaseType.miniZ, AlgorithmType.Meraki, "Meraki")
                            {
                            }
                        }
            },
        }.ConcatDictList(All, Gpu);

        #endregion

        #region NVIDIA

        public static Dictionary<MinerBaseType, List<Algorithm>> Nvidia => new Dictionary<MinerBaseType, List<Algorithm>>
        {
            {
                MinerBaseType.SRBMiner,
                        new List<Algorithm>() {
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.SHA256dt, "SHA256dt")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            /*
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.SHA512256d, "SHA512256d")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            */
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.VertHash, "VertHash")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            /*
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.SHA3d, "SHA3d")
                            {
                            },
                            */
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.Cryptonight_GPU, "Cryptonight_GPU")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            /*
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.Ethash, "Ethash")
                            {
                            },
                            */
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.Ethashb3, "Ethashb3")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.EvrProgPow, "EvrProgPow")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.FiroPow, "FiroPow")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.HeavyHash, "HeavyHash")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.PhiHash, "PhiHash")
                            {
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.KarlsenHashV2, "KarlsenHashV2")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.KawPow, "KawPow")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.MeowPow, "MeowPow")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.Meraki, "Meraki")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            /*
                             //unstable
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.NxlHash, "NxlHash")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            */
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.VerusHash, "VerusHash")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new Algorithm(MinerBaseType.SRBMiner, AlgorithmType.HooHash, "HooHash")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            new DualAlgorithm(MinerBaseType.SRBMiner, AlgorithmType.KarlsenHashV2,
                                AlgorithmType.HooHash, "KarlsenHashV2+HooHash")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            /*
                            new DualAlgorithm(MinerBaseType.SRBMiner, AlgorithmType.KarlsenHashV2,
                                AlgorithmType.PyrinHashV2, "KarlsenHashV2+PyrinHashV2")
                            {
                                ExtraLaunchParameters = "--max-rejected-shares 100"
                            },
                            */
                        }
            },
            { MinerBaseType.CryptoDredge,
                        new List<Algorithm>() {
                            new Algorithm(MinerBaseType.CryptoDredge, AlgorithmType.NeoScrypt, "NeoScrypt"),
                            //после переключения монет реджекты
                            //new Algorithm(MinerBaseType.CryptoDredge, AlgorithmType.Cryptonight_GPU, "Cryptonight_GPU"),
                            new Algorithm(MinerBaseType.CryptoDredge, AlgorithmType.Allium, "Allium"),
                            new Algorithm(MinerBaseType.CryptoDredge, AlgorithmType.SHA256csm, "SHA256csm"),
                            //new Algorithm(MinerBaseType.CryptoDredge, AlgorithmType.FiroPow, "FiroPow")
                        }
            },
            { MinerBaseType.trex,
                        new List<Algorithm>() {
                            new Algorithm(MinerBaseType.trex, AlgorithmType.X16RV2, "X16RV2")
                            {
                            },
                            new Algorithm(MinerBaseType.trex, AlgorithmType.X21S, "X21S")
                            {
                            },
                            new Algorithm(MinerBaseType.trex, AlgorithmType.X25X, "X25X")
                            {
                            },
                            //блоки не подтверждаются
                            /*
                            new Algorithm(MinerBaseType.trex, AlgorithmType.Megabtx, "Megabtx")
                            {
                            }
                            */
                        }
            },

            { MinerBaseType.miniZ,
                        new List<Algorithm>() {
                            new Algorithm(MinerBaseType.miniZ, AlgorithmType.Equihash125, "Equihash125")
                            {
                            },
                            new Algorithm(MinerBaseType.miniZ, AlgorithmType.Equihash144, "Equihash144")
                            {
                            },
                            new Algorithm(MinerBaseType.miniZ, AlgorithmType.Equihash192, "Equihash192")
                            {
                            },
                            new Algorithm(MinerBaseType.miniZ, AlgorithmType.EvrProgPow, "EvrProgPow")
                            {
                            },
                            new Algorithm(MinerBaseType.miniZ, AlgorithmType.Ethashb3, "Ethashb3")
                            {
                            },
                            new Algorithm(MinerBaseType.miniZ, AlgorithmType.Meraki, "Meraki")
                            {
                            }
                        }
            },

            {
            MinerBaseType.GMiner,
                new List<Algorithm>
                {
                    new Algorithm(MinerBaseType.GMiner, AlgorithmType.Ethash, "Ethash"),
                    new Algorithm(MinerBaseType.GMiner, AlgorithmType.KawPow, "KawPow")
                    {
                    },
                    /*
                    new Algorithm(MinerBaseType.GMiner, AlgorithmType.KawPowLite, "KAWPOWLite")
                    {
                    },
                    */
                    /*
                    new Algorithm(MinerBaseType.GMiner, AlgorithmType.KarlsenHash, "KarlsenHash")
                    {
                    },
                    */
                    new Algorithm(MinerBaseType.GMiner, AlgorithmType.Equihash125, "Equihash125")
                    {
                    },
                    new Algorithm(MinerBaseType.GMiner, AlgorithmType.Equihash144, "Equihash144")
                    {
                    },
                    /*
                    new Algorithm(MinerBaseType.GMiner, AlgorithmType.Equihash192, "Equihash192")//2.75
                    {
                    },
                    */
                    /*
                    //иногда нет хешрейта
                    new Algorithm(MinerBaseType.GMiner, AlgorithmType.FiroPow, "FiroPow")
                    {
                    },
                    */
                    /*
                    new Algorithm(MinerBaseType.GMiner, AlgorithmType.SHA512256d, "SHA512256d")
                    {
                    },
                    */
                    /*
                    new DualAlgorithm(MinerBaseType.GMiner, AlgorithmType.Autolykos, AlgorithmType.IronFish, AlgorithmType.AutolykosIronFish.ToString())
                    {
                        ExtraLaunchParameters = "--mt 1 -di 10"
                    },
                    */
                    
                }
            },
            {
                MinerBaseType.lolMiner,
                        new List<Algorithm>() {
                            new Algorithm(MinerBaseType.lolMiner, AlgorithmType.Equihash125, "Equihash125")
                            {
                                ExtraLaunchParameters = ""
                            },
                            new Algorithm(MinerBaseType.lolMiner, AlgorithmType.Equihash144, "Equihash144")
                            {
                                ExtraLaunchParameters = ""
                            },
                            /*
                            //не работает
                            new Algorithm(MinerBaseType.lolMiner, AlgorithmType.Equihash192, "Equihash192")
                            {
                                ExtraLaunchParameters = ""
                            },
                            */
                            /*
                            new Algorithm(MinerBaseType.lolMiner, AlgorithmType.KarlsenHash, "KarlsenHash")
                            {
                                ExtraLaunchParameters = ""
                            },
                            */
                            new Algorithm(MinerBaseType.lolMiner, AlgorithmType.KarlsenHashV2, "KarlsenHashV2")
                            {
                                ExtraLaunchParameters = ""
                            },
                            new Algorithm(MinerBaseType.lolMiner, AlgorithmType.NexaPow, "NexaPow")
                            {
                                ExtraLaunchParameters = "--keepfree 1024"
                            }
                            /*
                            new Algorithm(MinerBaseType.lolMiner, AlgorithmType.PyrinHashV2, "PyrinHashV2")
                            {
                                ExtraLaunchParameters = ""
                            },
                            */
                            /*
                            new Algorithm(MinerBaseType.lolMiner, AlgorithmType.SHA512256d, "SHA512256d")
                            {
                                ExtraLaunchParameters = ""
                            },
                            */
                            /*
                            new DualAlgorithm(MinerBaseType.lolMiner, AlgorithmType.KarlsenHashV2, 
                                AlgorithmType.PyrinHashV2, "KarlsenHashV2+PyrinHashV2")
                            {
                            },
                            */
                            /*
                            new Algorithm(MinerBaseType.lolMiner, AlgorithmType.Ethash, "Ethash")
                            {
                                ExtraLaunchParameters = "--enablezilcache=1"
                            },
                            */
                        }
            },

            {
                MinerBaseType.Rigel,
                new List<Algorithm>()
                {
                    new Algorithm(MinerBaseType.Rigel, AlgorithmType.KawPow, "KawPow")
                    {
                        ExtraLaunchParameters = "--no-tui --dag-reset-mclock off"
                    },
                    new Algorithm(MinerBaseType.Rigel, AlgorithmType.Ethash, "Ethash")
                    {
                        ExtraLaunchParameters = "--no-tui --dag-reset-mclock off"
                    },
                    new Algorithm(MinerBaseType.Rigel, AlgorithmType.Ethashb3, "Ethashb3")
                    {
                        ExtraLaunchParameters = "--no-tui --dag-reset-mclock off"
                    },
                    new Algorithm(MinerBaseType.Rigel, AlgorithmType.NexaPow, "NexaPow")
                    {
                        ExtraLaunchParameters = "--no-tui"
                    },
                    /*
                    new Algorithm(MinerBaseType.Rigel, AlgorithmType.KarlsenHash, "KarlsenHash")
                    {
                        ExtraLaunchParameters = "--no-tui"
                    },
                    */
                    new Algorithm(MinerBaseType.Rigel, AlgorithmType.KarlsenHashV2, "KarlsenHashV2")
                    {
                        ExtraLaunchParameters = "--no-tui --dag-reset-mclock off"
                    },
                    /*
                    new Algorithm(MinerBaseType.Rigel, AlgorithmType.PyrinHashV2, "PyrinHashV2")
                    {
                        ExtraLaunchParameters = "--no-tui --dag-reset-mclock off"
                    },
                    */
                    /*
                    new Algorithm(MinerBaseType.Rigel, AlgorithmType.SHA512256d, "SHA512256d")
                    {
                        ExtraLaunchParameters = "--no-tui"
                    },
                    */
                    //duals
                    /*
                    new DualAlgorithm(MinerBaseType.Rigel, AlgorithmType.Ethash, AlgorithmType.KarlsenHash,
                        "Ethash+KarlsenHash")
                    {
                        ExtraLaunchParameters = "--no-tui --dag-reset-mclock off"
                    },
                    */
                    /*
                    new DualAlgorithm(MinerBaseType.Rigel, AlgorithmType.Ethash, AlgorithmType.PyrinHashV2,
                        "Ethash+PyrinHashV2")
                    {
                        ExtraLaunchParameters = "--no-tui --dag-reset-mclock off"
                    },
                    */
                    /*
                     new DualAlgorithm(MinerBaseType.Rigel, AlgorithmType.Ethash, AlgorithmType.SHA512256d,
                        "Ethash+SHA512256d")
                     {
                        ExtraLaunchParameters = "--no-tui --dag-reset-mclock off"
                     },
                    */
                    /*
                     new DualAlgorithm(MinerBaseType.Rigel, AlgorithmType.Ethashb3, AlgorithmType.KarlsenHash,
                        "Ethashb3+KarlsenHash")
                     {
                        ExtraLaunchParameters = "--no-tui --dag-reset-mclock off"
                     },
                    */
                    /*
                     new DualAlgorithm(MinerBaseType.Rigel, AlgorithmType.Ethashb3, AlgorithmType.PyrinHashV2,
                        "Ethashb3+PyrinHashV2")
                     {
                        ExtraLaunchParameters = "--no-tui --dag-reset-mclock off"
                     },
                    */
                     /*
                     new DualAlgorithm(MinerBaseType.Rigel, AlgorithmType.Ethashb3, AlgorithmType.SHA512256d,
                        "Ethashb3+SHA512256d")
                     {
                        ExtraLaunchParameters = "--no-tui --dag-reset-mclock off"
                     },
                     */
                     /*
                     new DualAlgorithm(MinerBaseType.Rigel, AlgorithmType.KarlsenHashV2, AlgorithmType.PyrinHashV2,
                        "KarlsenHashV2+PyrinHashV2")
                     {
                        ExtraLaunchParameters = "--no-tui --dag-reset-mclock off"
                     },
                     */
                }
            }

        }.ConcatDictList(All, Gpu);

        #endregion
    }
}
