using System.Collections.Generic;

namespace StealthBot.Core.Config
{
    public interface ISalvageConfiguration : IConfigurationBase
    {
        bool CreateSalvageBookmarks { get; set; }
        bool SaveBookmarksForCorporation { get; set; }
    }

    public sealed class SalvageConfiguration : ConfigurationBase, ISalvageConfiguration
    {
        private static readonly string CreateSalvageBookmarksTag = "Salvage_EnableBM",
                                       SaveBookmarksForCorporationTag = "Salvage_CorpBM";
        #region Salvaging
        public bool CreateSalvageBookmarks
        {
            get { return GetConfigValue<bool>(CreateSalvageBookmarksTag); }
            set { SetConfigValue(CreateSalvageBookmarksTag, value); }
        }

        public bool SaveBookmarksForCorporation
        {
            get { return GetConfigValue<bool>(SaveBookmarksForCorporationTag); }
            set { SetConfigValue(SaveBookmarksForCorporationTag, value); }
        }
        #endregion

        public SalvageConfiguration(Dictionary<string, ConfigProperty> configProperties)
            : base(configProperties)
        {
            AddDefaultConfigProperties();
        }

        public override void AddDefaultConfigProperties()
        {
            AddDefaultConfigProperty(new ConfigProperty<bool>(CreateSalvageBookmarksTag, false));
            AddDefaultConfigProperty(new ConfigProperty<bool>(SaveBookmarksForCorporationTag, false));
        }
    }
}
