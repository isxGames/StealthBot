using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVE.ISXEVE.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SpecAid;
using StealthBot.Core;
using StealthBot.Core.Config;
using StealthBot.Core.Interfaces;
using TechTalk.SpecFlow;
using IModule = EVE.ISXEVE.Interfaces.IModule;
using IShip = StealthBot.Core.IShip;

namespace StealthBot.SpecFlow
{
    public class CustomScenarioContext
    {
        public IEnumerable<Mock<IEntityWrapper>> EntityMocks { get; set; }
        public IEnumerable<Mock<IModule>> ModuleMocks { get; set; }
        public Mock<IShip> Ship { get; set; }
        public Mock<IEntityProvider> EntityProvider { get; set; }
        public Mock<ILogging> Logging { get; set; }
        public Mock<IIsxeveProvider> IsxeveProvider { get; set; }
        public Mock<IEve> Eve { get; set; }
        // Add other properties as needed
        public bool WereTargetsUnlocked { get; set; }
        public int TimesUnlockTargetWasCalled { get; set; }
        public Mock<IMaxRuntimeConfiguration> MaxRuntimeConfiguration { get; set; }
        public Mock<IMeCache> MeCache { get; set; }
        public Mock<IDrones> Drones { get; set; }
        public Mock<IAlerts> Alerts { get; set; }
        public Mock<IModuleManager> ModuleManager { get; set; }
        public Mock<ITargetQueue> TargetQueue { get; set; }
        public Mock<IMovement> Movement { get; set; }
        public ITargeting Targeting { get; set; }

        public Mock<IInventoryProvider> InventoryProvider { get; set; }
        public Mock<IJettisonContainer> JettisonContainer { get; set; }
    }
}
