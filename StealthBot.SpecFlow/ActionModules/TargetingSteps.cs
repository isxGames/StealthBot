using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SpecAid;
using StealthBot.ActionModules;
using StealthBot.Core;
using StealthBot.Core.Config;
using StealthBot.Core.Interfaces;
using TechTalk.SpecFlow;

namespace StealthBot.SpecFlow.ActionModules
{
    using IModule = EVE.ISXEVE.Interfaces.IModule;

    [Binding, Scope(Tag = "Targeting")]
    public class TargetingSteps : CommonSteps
    {
        // For additional details on SpecFlow step definitions see http://go.specflow.org/doc-stepdef

        [BeforeScenario("Targeting")]
        public void BeforeTargetingFeature()
        {
            Initialize();

            Logging = new Mock<ILogging>();
            MaxRuntimeConfiguration = new Mock<IMaxRuntimeConfiguration>();
            MeCache = new Mock<IMeCache>();
            Drones = new Mock<IDrones>();
            Alerts = new Mock<IAlerts>();
            ModuleManager = new Mock<IModuleManager>();
            TargetQueue = new Mock<ITargetQueue>();
            Movement = new Mock<IMovement>();

            Targeting = new Targeting(Logging.Object, MaxRuntimeConfiguration.Object, MeCache.Object, Ship.Object, Drones.Object, Alerts.Object, 
                ModuleManager.Object, TargetQueue.Object, EntityProvider.Object, Movement.Object);

            MeCache.Setup(m => m.InSpace).Returns(true);
            MeCache.Setup(m => m.InStation).Returns(false);

            Movement.Setup(m => m.IsMoving).Returns(false);

            MaxRuntimeConfiguration.Setup(mrc => mrc.UseMaxRuntime).Returns(false);

            TimesUnlockTargetWasCalled = 0;
        }

        [Given(@"I have entities")]
        protected override void GivenIHaveEntities(Table table)
        {
            base.GivenIHaveEntities(table);

            //Override the default UnlockTarget() callback to unlock call counts
            foreach (var entityMock in EntityMocks)
            {
                entityMock.Setup(e => e.UnlockTarget()).Callback(() =>
                    {
                        entityMock.Setup(m => m.IsLockedTarget).Returns(false);
                        TimesUnlockTargetWasCalled++;
                    });
            }
        }

        [Given(@"I have queued targets")]
        public void GivenIHaveQueuedTargets(Table table)
        {
            var queueTargets = TableAid.ObjectCreator<QueueTarget>(table, (row, target) =>
                {
                    RecallAid.It[row["!this"]] = target;
                });

            var countById = queueTargets.GroupBy(x => x.Id);
            foreach (var g in countById)
            {
                Assert.AreEqual(1, g.Count(), string.Format("More than one QueueTarget exists with ID {0}.", g.Key));
            }

            TargetQueue.Setup(tq => tq.Targets).Returns(queueTargets.ToList().AsReadOnly());
        }

        [Given(@"my ship can lock '(.*)' targets")]
        public void GivenMyShipCanLockTargets(int number)
        {
            Ship.Setup(s => s.MaxLockedTargets).Returns(number);
        }

        [Given(@"whether or not I am in a non-combat mode is '(.*)'")]
        public void GivenWhetherOrNotIAmInANonCombatModeIs(bool value)
        {
            ModuleManager.Setup(mm => mm.IsNonCombatMode).Returns(value);
        }

        [Given(@"I have modules")]
        public void GivenIHaveModules(Table table)
        {
            var moduleMocks = new List<Mock<IModule>>();

            foreach (var row in table.Rows)
            {
                var moduleMock = new Mock<IModule>();

                var id = Int64.Parse(row["ID"]);
                moduleMock.Setup(m => m.ID).Returns(id);

                var isActive = bool.Parse(row["IsActive"]);
                moduleMock.Setup(m => m.IsActive).Returns(isActive);

                var targetId = Int64.Parse(row["TargetID"]);
                moduleMock.Setup(m => m.TargetID).Returns(targetId);

                moduleMocks.Add(moduleMock);

                RecallAid.It[row["!this"]] = moduleMock;
            }

            ModuleMocks = moduleMocks;
        }

        [When(@"I unlock unqueued targets")]
        public void WhenIUnlockUnqueuedTargets()
        {
            WereTargetsUnlocked = Targeting.UnlockUnqueuedTargets();
        }

        [When(@"I process queued targets '(.*)' times")]
        public void WhenIProcessQueuedTargetsTimes(int count)
        {
            for (var index = 0; index < count; index++)
            {
                //Targeting.ProcessTargetQueue();
                Targeting.Pulse();
            }
        }

        [Then(@"exactly '(.*)' locked target\(s\) should be unlocked")]
        public void ThenExactlyNumberLockedButUnqueuedTargetShouldBeUnlocked(int number)
        {
            Assert.AreEqual(number, TimesUnlockTargetWasCalled);
        }

        [Then(@"Unlock unqueued targets should return '(.*)'")]
        public void ThenUnlockUnqueuedTargetsShouldReturn(bool value)
        {
            Assert.AreEqual(value, WereTargetsUnlocked);
        }

        [Then(@"target '(.*)' should have been locked")]
        public void ThenTargetShouldBeLocked(string entityRef)
        {
            var entity = (Mock<IEntityWrapper>) RecallAid.It[entityRef];

            entity.Verify(ew => ew.LockTarget(), Times.Once());
        }

        [Then(@"target '(.*)' should not have been unlocked")]
        public void ThenTargetShouldNotHaveBeenUnlocked(string entityRef)
        {
            var entityMock = (Mock<IEntityWrapper>) RecallAid.It[entityRef];

            entityMock.Verify(em => em.UnlockTarget(), Times.Never());
        }

        #region ScenarioContext Objects

        private bool WereTargetsUnlocked
        {
            get { return GetFromScenarioContext<bool>("wereTargetsUnlocked"); }
            set { SetInScenarioContext("wereTargetsUnlocked", value); }
        }

        private int TimesUnlockTargetWasCalled
        {
            get { return GetFromScenarioContext<int>("timesUnlockTargetWasCalled"); }
            set { SetInScenarioContext("timesUnlockTargetWasCalled", value); }
        }

        private new Mock<ILogging> Logging
        {
            get { return GetFromScenarioContext<Mock<ILogging>>("logging"); }
            set { SetInScenarioContext("logging", value); }
        }

        private Mock<IMaxRuntimeConfiguration> MaxRuntimeConfiguration
        {
            get { return GetFromScenarioContext<Mock<IMaxRuntimeConfiguration>>("maxRuntimeConfiguration"); }
            set { SetInScenarioContext("maxRuntimeConfiguration", value); }
        }

        private Mock<IMeCache> MeCache
        {
            get { return GetFromScenarioContext<Mock<IMeCache>>("meCache"); }
            set { SetInScenarioContext("meCache", value); }
        }

        private Mock<IDrones> Drones
        {
            get { return GetFromScenarioContext<Mock<IDrones>>("drones"); }
            set { SetInScenarioContext("drones", value); }
        }

        private Mock<IAlerts> Alerts
        {
            get { return GetFromScenarioContext<Mock<IAlerts>>("alerts"); }
            set { SetInScenarioContext("alerts", value); }
        }

        private Mock<IModuleManager> ModuleManager
        {
            get { return GetFromScenarioContext<Mock<IModuleManager>>("moduleManager"); }
            set { SetInScenarioContext("moduleManager", value); }
        }

        private Mock<ITargetQueue> TargetQueue
        {
            get { return GetFromScenarioContext<Mock<ITargetQueue>>("targetQueue"); }
            set { SetInScenarioContext("targetQueue", value); }
        }

        private Mock<IMovement> Movement
        {
            get { return GetFromScenarioContext<Mock<IMovement>>("movement"); }
            set { SetInScenarioContext("movement", value); }
        }

        private ITargeting Targeting
        {
            get { return GetFromScenarioContext<ITargeting>("targeting"); }
            set { SetInScenarioContext("targeting", value); }
        }
        #endregion
    }
}
