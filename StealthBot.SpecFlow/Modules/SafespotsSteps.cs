using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVE.ISXEVE.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SpecAid;
using StealthBot.ActionModules;
using StealthBot.Core;
using StealthBot.Core.Config;
using StealthBot.Core.Interfaces;
using TechTalk.SpecFlow;
using IShip = StealthBot.Core.IShip;

namespace StealthBot.SpecFlow.Modules
{
    [Binding]
    public class SafespotsSteps : CommonSteps
    {
        private Mock<IMeCache> _meCache;
        private Mock<IBookMarkCache> _bookMarkCache;
        private Mock<IMeToEntityCache> _meToEntityCache;
        private Mock<IMovementConfiguration> _movementConfiguration;
        private Mock<ISocial> _social;
        private Mock<ILogging> _logging;

        private MathUtility _mathUtility;
        private Safespots _safespots;

        private bool? _isAtSafeSpot;
        private Destination _safeSpotDestination;

        public SafespotsSteps(CustomScenarioContext context, ScenarioContext scenarioContext) : base(context, scenarioContext)
        {
            // ... rest of the constructor code
        }

        [BeforeScenario("@Safespots")]
        public void BeforeScenarioSafespots()
        {
            _meCache = new Mock<IMeCache>();
            _bookMarkCache = new Mock<IBookMarkCache>();
            _meToEntityCache = new Mock<IMeToEntityCache>();
            _movementConfiguration = new Mock<IMovementConfiguration>();
            _social = new Mock<ISocial>();

            EntityProvider = new Mock<IEntityProvider>();
            EntityMocks = new List<Mock<IEntityWrapper>>();

            IsxeveProvider = new Mock<IIsxeveProvider>();
            Eve = new Mock<IEve>();
            IsxeveProvider.SetupGet(ip => ip.Eve).Returns(Eve.Object);

            Ship = new Mock<IShip>();
            Ship.Setup(s => s.CloakingDeviceModules).Returns(new List<EVE.ISXEVE.Interfaces.IModule>().AsReadOnly());

            _mathUtility = new MathUtility();

            _logging = new Mock<ILogging>();

            _safespots = new Safespots(_meCache.Object, _bookMarkCache.Object, _movementConfiguration.Object, _meToEntityCache.Object, EntityProvider.Object,
                IsxeveProvider.Object, Ship.Object, _social.Object, _mathUtility, _logging.Object);
        }

        [Given(@"I am in a station")]
        public void GivenIAmInAStation()
        {
            _meCache.SetupGet(mc => mc.InStation).Returns(true);
            _meCache.SetupGet(mc => mc.InSpace).Returns(false);
        }

        [Given(@"I am in space")]
        public void GivenIAmInSpace()
        {
            _meCache.SetupGet(mc => mc.InStation).Returns(false);
            _meCache.SetupGet(mc => mc.InSpace).Returns(true);
        }

        [Given(@"I am in solarsystem ID '(.*)'")]
        public void GivenIAmInSolarsystemID(int solarSystemId)
        {
            _meCache.SetupGet(mc => mc.SolarSystemId).Returns(solarSystemId);
        }

        [Given(@"I have bookmarks")]
        public void GivenIHaveBookmarks(Table table)
        {
            var bookMarkMocks = CreateBookMarkMocksFromTable(table);
            var bookMarks = bookMarkMocks.Select(bmm => bmm.Object).ToList();

            _bookMarkCache.SetupGet(bm => bm.BookMarks).Returns(bookMarks.AsReadOnly());
        }

        [Given(@"I am at point '(.*)','(.*)','(.*)'")]
        public void GivenIAmAtPoint(double x, double y, double z)
        {
            _meToEntityCache.SetupGet(mc => mc.X).Returns(x);
            _meToEntityCache.SetupGet(mc => mc.Y).Returns(y);
            _meToEntityCache.SetupGet(mc => mc.Z).Returns(z);
        }

        [Given(@"my safe bookmark prefix is '(.*)'")]
        public void GivenMySafeBookmarkPrefixIs(string safeBookMarkPrefix)
        {
            _movementConfiguration.SetupGet(mc => mc.SafeBookmarkPrefix).Returns(safeBookMarkPrefix);
        }

        [Given(@"I have entities")]
        protected override void GivenIHaveEntities(Table table)
        {
            base.GivenIHaveEntities(table);
        }

        [Given(@"I have item info")]
		public new void GivenIHaveItemInfo(Table table)
        {
            base.GivenIHaveItemInfo(table);
        }

        [Given(@"local is safe")]
        public void GivenLocalIsSafe()
        {
            _social.SetupGet(s => s.IsLocalSafe).Returns(true);
        }

        [Given(@"local is not safe")]
        public void GivenLocalIsNotSafe()
        {
            _social.SetupGet(s => s.IsLocalSafe).Returns(false);
        }

        [Given(@"I have cloaking device modules")]
        public void GivenIHaveCloakingDeviceModules(Table table)
        {
            var moduleMocks = new List<Mock<EVE.ISXEVE.Interfaces.IModule>>();

            foreach (var row in table.Rows)
            {
                var moduleMock = new Mock<EVE.ISXEVE.Interfaces.IModule>();

                moduleMock.SetupGet(m => m.ID).Returns(long.Parse(row["ID"]));

                moduleMock.SetupGet(m => m.ToItem).Returns(((Mock<IItem>) RecallAid.It[row["ToItem"]]).Object);

                moduleMocks.Add(moduleMock);
            }

            Ship.SetupGet(s => s.CloakingDeviceModules).Returns(moduleMocks.Select(m => m.Object).ToList().AsReadOnly());
        }

        [Given(@"Items exist")]
        public void GivenItemsExist(Table table)
        {
            foreach (var row in table.Rows)
            {
                var itemMock = new Mock<IItem>();

                itemMock.SetupGet(i => i.Type).Returns(row["Type"]);
                itemMock.SetupGet(i => i.TypeID).Returns(int.Parse(row["TypeID"]));
                itemMock.SetupGet(i => i.GroupID).Returns(int.Parse(row["GroupID"]));
                itemMock.SetupGet(i => i.Group).Returns(row["Group"]);

                RecallAid.It[row["!this"]] = itemMock;
            }
        }

        [Given(@"my home station is '(.*)'")]
        public void GivenMyHomeStationIs(string homeStationName)
        {
            _movementConfiguration.SetupGet(mc => mc.HomeStation).Returns(homeStationName);
        }

        [When(@"I check if I am safe")]
        public void WhenICheckIfIAmSafe()
        {
            _isAtSafeSpot = _safespots.IsSafe();
        }

        [When(@"I get a safe spot")]
        public void WhenIGetASafeSpot()
        {
            _safeSpotDestination = _safespots.GetSafeSpot();
        }

        [Then(@"the result should be '(.*)'")]
        public void ThenTheResultShouldBe(bool expectedResult)
        {
            Assert.IsNotNull(_isAtSafeSpot);

            Assert.AreEqual(expectedResult, _isAtSafeSpot.Value);
        }

        [Then(@"the the destination should be a bookmark destination matching bookmark ID '(.*)'")]
        public void ThenTheTheDestinationShouldBeABookmarkDestinationMatchingBookmarkID(int bookMarkId)
        {
            Assert.IsNotNull(_safeSpotDestination);

            Assert.AreEqual(DestinationTypes.BookMark, _safeSpotDestination.Type);
            Assert.AreEqual(bookMarkId, _safeSpotDestination.BookMarkId);
        }

        [Then(@"the the destination should be an entity destination matching entity ID '(.*)'")]
        public void ThenTheTheDestinationShouldBeAnEntityDestinationMatchingEntityID(long entityId)
        {
            Assert.IsNotNull(_safeSpotDestination);

            Assert.AreEqual(DestinationTypes.Entity, _safeSpotDestination.Type);
            Assert.AreEqual(entityId, _safeSpotDestination.EntityId);
        }

        [Then(@"we should dock at the destination")]
        public void ThenWeShouldDockAtTheDestination()
        {
            Assert.IsNotNull(_safeSpotDestination);

            Assert.IsTrue(_safeSpotDestination.Dock);
        }

        [Then(@"the destination\'s distance should be '(.*)'")]
        public void ThenTheDestination(long distance)
        {
            Assert.IsNotNull(_safeSpotDestination);

            Assert.AreEqual(distance, _safeSpotDestination.Distance);
        }
    }
}
