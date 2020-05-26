using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StealthBot.ActionModules;
using StealthBot.Core;
using StealthBot.Core.Config;
using StealthBot.Core.Interfaces;

namespace StealthBot.UnitTests.ActionModules
{
    [TestClass]
    public class TargetingTest
    {
        private Mock<ILogging> _logging;
        private Mock<IMeCache> _meCache;
        private Mock<IMaxRuntimeConfiguration> _maxRuntimeConfiguration;
        private Mock<IShip> _ship;
        private Mock<IDrones> _drones;
        private Mock<IAlerts> _alerts;
        private Mock<ITargetQueue> _targetQueue;
        private Mock<IEntityProvider> _entityPopulator;
        private Mock<IModuleManager> _moduleManager;
        private Mock<IMovement> _movement;

        private ITargeting _targeting;

        [TestInitialize]
        public void TargetingTestInitialize()
        {
            _logging = new Mock<ILogging>();
            _meCache = new Mock<IMeCache>();
            _maxRuntimeConfiguration = new Mock<IMaxRuntimeConfiguration>();
            _ship = new Mock<IShip>();
            _drones = new Mock<IDrones>();
            _alerts = new Mock<IAlerts>();
            _targetQueue = new Mock<ITargetQueue>();
            _entityPopulator = new Mock<IEntityProvider>();
            _moduleManager = new Mock<IModuleManager>();
            _movement = new Mock<IMovement>();

            _targeting = new Targeting(_logging.Object, _maxRuntimeConfiguration.Object, _meCache.Object, _ship.Object, _drones.Object, _alerts.Object,
                _moduleManager.Object, _targetQueue.Object, _entityPopulator.Object, _movement.Object);
        }

        #region CalculateIntendedSlotsByType
        [TestMethod]
        public void CalculateIntendedSlotsByTypeShouldSucceedForOneTarget()
        {
            //Arrange
            var maxLockedTargets = 4;
            _ship.Setup(s => s.MaxLockedTargets).Returns(maxLockedTargets);

            var target = CreateQueueTarget(TargetTypes.Mine);

            var targets = new List<QueueTarget> {target};

            //Act
            var intendedSlotsByType = _targeting.CalculateIntendedSlotsByType(targets);

            //Assert

            //Ensure the result # of target types matches the # of target types of input dictionary
            var uniqueTargetTypes = targets.Select(t => t.Type).Distinct().Count();
            Assert.AreEqual(uniqueTargetTypes, intendedSlotsByType.Count);
            
            var firstPair = intendedSlotsByType.First();
            Assert.AreEqual(target.Type, firstPair.Key); // Mining
            Assert.AreEqual(maxLockedTargets, firstPair.Value); // 4
        }

        [TestMethod]
        public void CalculateIntendedSlotsByTypeShouldSucceedForEvenSplitOfTwoTargetTypes()
        {
            //Arrange
            var maxLockedTargets = 4;
            _ship.Setup(s => s.MaxLockedTargets).Returns(maxLockedTargets);

            var miningTarget = CreateQueueTarget(TargetTypes.Mine);
            var killingTarget = CreateQueueTarget(TargetTypes.Kill);

            var targets = new List<QueueTarget> {miningTarget, killingTarget};

            //Act
            var intendedSlotsByType = _targeting.CalculateIntendedSlotsByType(targets);

            //Assert

            //Ensure the result # of target types matches the # of target types of input dictionary
            var uniqueTargetTypes = targets.Select(t => t.Type).Distinct().Count();
            Assert.AreEqual(uniqueTargetTypes, intendedSlotsByType.Count);

            Assert.IsTrue(intendedSlotsByType.ContainsKey(miningTarget.Type));
            Assert.AreEqual(2, intendedSlotsByType[miningTarget.Type]);

            Assert.IsTrue(intendedSlotsByType.ContainsKey(killingTarget.Type));
            Assert.AreEqual(2, intendedSlotsByType[killingTarget.Type]);
        }

        [TestMethod]
        public void CalculateIntendedSlotsByTypeShouldSucceedForOddSplitOfTwoTargetTypesFavoringKillForCombatModes()
        {
            //Arrange
            var maxLockedTargets = 5;
            _ship.Setup(s => s.MaxLockedTargets).Returns(maxLockedTargets);

            _moduleManager.Setup(m => m.IsNonCombatMode).Returns(false);

            var miningTarget = CreateQueueTarget(TargetTypes.Mine);
            var killingTarget = CreateQueueTarget(TargetTypes.Kill);

            var targets = new List<QueueTarget> { miningTarget, killingTarget };

            //Act
            var intendedSlotsByType = _targeting.CalculateIntendedSlotsByType(targets);

            //Assert

            //Ensure the result # of target types matches the # of target types of input dictionary
            var uniqueTargetTypes = targets.Select(t => t.Type).Distinct().Count();
            Assert.AreEqual(uniqueTargetTypes, intendedSlotsByType.Count);

            Assert.IsTrue(intendedSlotsByType.ContainsKey(miningTarget.Type));
            Assert.AreEqual(2, intendedSlotsByType[miningTarget.Type]);

            Assert.IsTrue(intendedSlotsByType.ContainsKey(killingTarget.Type));
            Assert.AreEqual(3, intendedSlotsByType[killingTarget.Type]);
        }

        [TestMethod]
        public void CalculateIntendedSlotsByTypeShouldSucceedForOddSplitOfTwoTargetTypesFavoringMiningForNonCombatModes()
        {
            //Arrange
            var maxLockedTargets = 5;
            _ship.Setup(s => s.MaxLockedTargets).Returns(maxLockedTargets);

            _moduleManager.Setup(m => m.IsNonCombatMode).Returns(true);

            var miningTarget = CreateQueueTarget(TargetTypes.Mine);
            var killingTarget = CreateQueueTarget(TargetTypes.Kill);

            var targets = new List<QueueTarget> { miningTarget, killingTarget };

            //Act
            var intendedSlotsByType = _targeting.CalculateIntendedSlotsByType(targets);

            //Assert

            //Ensure the result # of target types matches the # of target types of input dictionary
            var uniqueTargetTypes = targets.Select(t => t.Type).Distinct().Count();
            Assert.AreEqual(uniqueTargetTypes, intendedSlotsByType.Count);

            Assert.IsTrue(intendedSlotsByType.ContainsKey(miningTarget.Type));
            Assert.AreEqual(3, intendedSlotsByType[miningTarget.Type]);

            Assert.IsTrue(intendedSlotsByType.ContainsKey(killingTarget.Type));
            Assert.AreEqual(2, intendedSlotsByType[killingTarget.Type]);
        }
        #endregion

        private int _queueTargetId;
        private QueueTarget CreateQueueTarget(TargetTypes targetType)
        {
            var queueTarget = new QueueTarget(_queueTargetId, 0, 0, targetType);

            _queueTargetId++;

            return queueTarget;
        }
    }
}
