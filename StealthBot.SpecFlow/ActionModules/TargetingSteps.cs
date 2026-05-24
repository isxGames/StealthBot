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

        public TargetingSteps(CustomScenarioContext context, ScenarioContext scenarioContext) : base(context, scenarioContext)
        {
            // ... rest of the constructor code
        }

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

        #region CustomScenarioContext Properties

        private bool WereTargetsUnlocked
        {
            get { return _context.WereTargetsUnlocked; }
            set { _context.WereTargetsUnlocked = value; }
        }

        private int TimesUnlockTargetWasCalled
        {
            get { return _context.TimesUnlockTargetWasCalled; }
            set { _context.TimesUnlockTargetWasCalled = value; }
        }

        private new Mock<ILogging> Logging
        {
            get { return _context.Logging; }
            set { _context.Logging = value; }
        }

        private Mock<IMaxRuntimeConfiguration> MaxRuntimeConfiguration
        {
            get { return _context.MaxRuntimeConfiguration; }
            set { _context.MaxRuntimeConfiguration = value; }
        }

        private Mock<IMeCache> MeCache
        {
            get { return _context.MeCache; }
            set { _context.MeCache = value; }
        }

        private Mock<IDrones> Drones
        {
            get { return _context.Drones; }
            set { _context.Drones = value; }
        }

        private Mock<IAlerts> Alerts
        {
            get { return _context.Alerts; }
            set { _context.Alerts = value; }
        }

        private Mock<IModuleManager> ModuleManager
        {
            get { return _context.ModuleManager; }
            set { _context.ModuleManager = value; }
        }

        private Mock<ITargetQueue> TargetQueue
        {
            get { return _context.TargetQueue; }
            set { _context.TargetQueue = value; }
        }

        private Mock<IMovement> Movement
        {
            get { return _context.Movement; }
            set { _context.Movement = value; }
        }

        private ITargeting Targeting
        {
            get { return _context.Targeting; }
            set { _context.Targeting = value; }
        }
        #endregion
    }
}
