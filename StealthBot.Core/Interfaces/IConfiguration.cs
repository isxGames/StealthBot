using System.Collections.Generic;
using StealthBot.Core.Config;

namespace StealthBot.Core.Interfaces
{
    public interface IConfiguration
    {
        SocialConfiguration SocialConfig { get; }
        IDefensiveConfiguration DefenseConfig { get; }
        MissionConfiguration MissionConfig { get; }
        IMiningConfiguration MiningConfig { get; }
        IMovementConfiguration MovementConfig { get; }
        IMainConfiguration MainConfig { get; }
        FleetConfiguration FleetConfig { get; }
        ICargoConfiguration CargoConfig { get; }
        HaulingConfiguration HaulingConfig { get; }
        AlertConfiguration AlertConfig { get; }
        IRattingConfiguration RattingConfig { get; }
        ShipHubConfiguration ShipHubConfig { get; }
        IMaxRuntimeConfiguration MaxRuntimeConfig { get; }
        ISalvageConfiguration SalvageConfig { get; }
        Dictionary<string, ConfigProperty> ConfigProperties { get; set; }
    }
}
