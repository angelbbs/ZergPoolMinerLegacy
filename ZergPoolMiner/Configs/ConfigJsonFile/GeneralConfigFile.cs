using ZergPoolMiner.Configs.Data;

namespace ZergPoolMiner.Configs.ConfigJsonFile
{
    public class GeneralConfigFile : ConfigFile<GeneralConfig>
    {
        public GeneralConfigFile()
            : base(Folders.Config, "General.json", "General_old.json")
        { }
    }
}
