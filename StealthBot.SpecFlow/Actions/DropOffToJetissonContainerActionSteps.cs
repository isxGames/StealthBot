using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using EVE.ISXEVE.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SpecAid;
using StealthBot.Actions;
using StealthBot.BehaviorModules.PartialBehaviors;
using StealthBot.Core.Interfaces;
using TechTalk.SpecFlow;

namespace StealthBot.SpecFlow.Actions
{
    [Binding]
    public class DropOffToJetissonContainerActionSteps : CommonSteps
    {
        private IPartialBehaviorBase _dropOffToJetissonContainerAction;

        [BeforeScenario("@DropOffToJetissonContainerActionTest")]
        public void DropoffToJetissonContanierActionTestBeforeScenario()
        {
            Initialize();

            InventoryProvider = new Mock<IInventoryProvider>();
            InventoryProvider.Setup(ip => ip.Cargo).Returns(new List<IItem>().AsReadOnly());
            InventoryProvider.Setup(ip => ip.OreHoldCargo).Returns(new List<IItem>().AsReadOnly());

            JettisonContainer = new Mock<IJettisonContainer>();

            _dropOffToJetissonContainerAction = new DropOffToJetissonContainerAction(Logging.Object, InventoryProvider.Object, EntityProvider.Object, JettisonContainer.Object);
        }

        [Given(@"I have ore hold items")]
        public void GivenIHaveOreHoldItems(Table table)
        {
            var itemMocks = CreateItemMocksFromTable(table);

            var items = itemMocks.Select(itemMock => itemMock.Object);
            InventoryProvider.Setup(ip => ip.OreHoldCargo)
                .Returns(items.ToList().AsReadOnly());

            InventoryProvider.Setup(ip => ip.HaveOreHold).Returns(true);
        }

        [Given(@"I have cargo hold items")]
        public void GivenIHaveCargoHoldItems(Table table)
        {
            var itemMocks = CreateItemMocksFromTable(table);
            var items = itemMocks.Select(itemMock => itemMock.Object);

            InventoryProvider.Setup(ip => ip.Cargo)
                .Returns(items.ToList().AsReadOnly());
        }


        [When(@"I process jetisson container dropoff \('(.*)' time\(s\)\)")]
        public void WhenIProcessJetissonContainerDropoffNTimes(int times)
        {
            var result = BehaviorExecutionResults.Incomplete;
            for (var index = 0; result == BehaviorExecutionResults.Incomplete && index < times; index++)
            {
                result = _dropOffToJetissonContainerAction.Execute();
            }
        }

        [Then(@"item '(.*)' should have been jetissoned")]
        public void ThenItemShouldHaveBeenJetissoned(string itemRef)
        {
            var itemMock = (Mock<IItem>) RecallAid.It[itemRef];

            itemMock.Verify(im => im.Jettison(), Times.Once());
        }

        [Then(@"item '(.*)' should not have been jettisoned")]
        public void ThenItemShouldNotHaveBeenJettisoned(string itemRef)
        {
            var itemMock = (Mock<IItem>)RecallAid.It[itemRef];

            itemMock.Verify(im => im.Jettison(), Times.Never());
        }

        private Mock<IInventoryProvider> InventoryProvider
        {
            get { return GetFromScenarioContext<Mock<IInventoryProvider>>("inventoryProvider"); }
            set { SetInScenarioContext("inventoryProvider", value); }
        }

        private Mock<IJettisonContainer> JettisonContainer
        {
            get { return GetFromScenarioContext<Mock<IJettisonContainer>>("jettisonContainer"); }
            set { SetInScenarioContext("jettisonContainer", value); }
        }
    }
}
