using System;
using EVE.ISXEVE;
using EVE.ISXEVE.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StealthBot.Core;
using StealthBot.Core.Interfaces;

namespace StealthBot.UnitTests.Core
{
    [TestClass]
    public class InventoryProviderTest
    {
        private Mock<ILogging> _logging;
        private Mock<IEveWindowProvider> _eveWindowProvider;
        private Mock<IShipCache> _shipCache;
        private Mock<IEve> _eve;

        private IInventoryProvider _inventoryProvider;

        [TestInitialize]
        public void InventoryProviderTestInitialize()
        {
            _logging = new Mock<ILogging>();
            _eveWindowProvider = new Mock<IEveWindowProvider>();
            _shipCache = new Mock<IShipCache>();
            _eve = new Mock<IEve>();

            _inventoryProvider = new InventoryProvider(_logging.Object, _eveWindowProvider.Object, _shipCache.Object, _eve.Object);
        }

        [TestMethod]
        public void GetInventoryWindowShouldQueryForInventoryWindow()
        {
            //Arrange


            //Act
            var inventoryWindow = _inventoryProvider.GetInventoryWindow();

            //Assert
            _eveWindowProvider.Verify(ewp => ewp.GetInventoryWindow(), Times.Once());
        }

        [TestMethod]
        public void IsInventoryOpenShouldUseInventoryWindow()
        {
            //Arrange

            
            //Act
            var isInventoryOpen = _inventoryProvider.IsInventoryOpen;

            //Assert
            _eveWindowProvider.Verify(ewp => ewp.GetInventoryWindow(), Times.Once());
        }

        [TestMethod]
        public void IsInventoryOpenShouldReturnFalseWhenInventoryWindowIsNull()
        {
            //Arrange
            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns<IEveInvWindow>(null);

            //Act
            var isInventoryOpen = _inventoryProvider.IsInventoryOpen;

            //Assert
            Assert.IsFalse(isInventoryOpen);
        }

        [TestMethod]
        public void IsInventoryOpenShouldReturnFalseWhenInventoryWindowIsInvalid()
        {
            //Arrange
            var eveInventoryWindow = new Mock<IEveInvWindow>();
            eveInventoryWindow.Setup(eiw => eiw.IsValid).Returns(false);

            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInventoryWindow.Object);

            //Act
            var isInventoryOpen = _inventoryProvider.IsInventoryOpen;

            //Asert
            Assert.IsFalse(isInventoryOpen);
        }

        [TestMethod]
        public void IsInventoryOpenShouldReturnTrueWhenInventoryWindowIsValid()
        {
            //Arrange
            var eveInventoryWindow = new Mock<IEveInvWindow>();
            eveInventoryWindow.Setup(eiw => eiw.IsValid).Returns(true);

            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInventoryWindow.Object);

            //Act
            var isInventoryOpen = _inventoryProvider.IsInventoryOpen;

            //Asert
            Assert.IsTrue(isInventoryOpen);
        }

        [TestMethod]
        public void OpenInventoryShouldExecuteCommandOpenInventory()
        {
            //Arrange


            //Act
            _inventoryProvider.OpenInventory();

            //Assert
            _eve.Verify(e => e.Execute(It.Is<ExecuteCommand>(ec => ec == ExecuteCommand.OpenInventory)), Times.Once());
        }

        [TestMethod]
        public void OpenInventoryShouldNotExecuteCommandOpenInventoryIfInventoryWindowIsNotValid()
        {
            //Arrange
            var eveInventoryWindow = new Mock<IEveInvWindow>();
            eveInventoryWindow.Setup(eiw => eiw.IsValid).Returns(true);

            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInventoryWindow.Object);

            //Act
            _inventoryProvider.OpenInventory();

            //Assert
            _eve.Verify(e => e.Execute(It.Is<ExecuteCommand>(ec => ec == ExecuteCommand.OpenInventory)), Times.Never());
        }

        #region Cargo Hold

        [TestMethod]
        public void IsCargoHoldActiveShouldReturnFalseWhenInventoryWindowIsNull()
        {
            //Arrange
            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns<IEveInvWindow>(null);

            //Act
            var isCargoHoldActive = _inventoryProvider.IsCargoHoldActive;

            //Assert
            Assert.IsFalse(isCargoHoldActive);
        }

        [TestMethod]
        public void IsCargoHoldActiveShouldReturnTrueWhenActiveChildIsCargoHold()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(true);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(true);
            eveInvChildWindow.Setup(eicw => eicw.ItemId).Returns(1);
            eveInvChildWindow.Setup(eicw => eicw.Name).Returns(InventoryProvider.ShipCargoName);
            eveInvWindow.Setup(eiw => eiw.ActiveChild).Returns(eveInvChildWindow.Object);

            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);
            
            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            var isCargoHoldActive = _inventoryProvider.IsCargoHoldActive;

            //Assert
            Assert.IsTrue(isCargoHoldActive);
        }

        [TestMethod]
        public void IsCargoHoldActiveShouldReturnFalseWhenActiveChildIsNull()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(true);

            eveInvWindow.Setup(eiw => eiw.ActiveChild).Returns<IEveInvChildWindow>(null);

            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            //Act
            var isCargoHoldActive = _inventoryProvider.IsCargoHoldActive;

            //Assert
            Assert.IsFalse(isCargoHoldActive);
        }

        [TestMethod]
        public void IsCargoHoldActiveShouldReturnFalseWhenActiveChildIsInvalid()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(true);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(false);
            eveInvWindow.Setup(eiw => eiw.ActiveChild).Returns(eveInvChildWindow.Object);

            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            //Act
            var isCargoHoldActive = _inventoryProvider.IsCargoHoldActive;

            //Assert
            Assert.IsFalse(isCargoHoldActive);
        }

        [TestMethod]
        public void IsCargoHoldActiveShouldReturnFalseWhenActiveChildIsFromAnotherItem()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(true);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(true);
            eveInvChildWindow.Setup(eicw => eicw.ItemId).Returns(-1);
            eveInvChildWindow.Setup(eicw => eicw.Name).Returns(InventoryProvider.ShipCargoName);
            eveInvWindow.Setup(eiw => eiw.ActiveChild).Returns(eveInvChildWindow.Object);

            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            //Act
            var isCargoHoldActive = _inventoryProvider.IsCargoHoldActive;

            //Assert
            Assert.IsFalse(isCargoHoldActive);
        }

        [TestMethod]
        public void IsCargoHoldActiveShouldReturnFalseWhenActiveChildNameDoesNotMatchShipCargoName()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(true);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(true);
            eveInvChildWindow.Setup(eicw => eicw.ItemId).Returns(1);
            eveInvChildWindow.Setup(eicw => eicw.Name).Returns("");
            eveInvWindow.Setup(eiw => eiw.ActiveChild).Returns(eveInvChildWindow.Object);

            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            //Act
            var isCargoHoldActive = _inventoryProvider.IsCargoHoldActive;

            //Assert
            Assert.IsFalse(isCargoHoldActive);
        }

        [TestMethod]
        public void CargoCapacityShouldReturnShipCargoCapacity()
        {
            //Arrange


            //Act
            var cargoCapacity = _inventoryProvider.CargoCapacity;

            //Assert
            _shipCache.VerifyGet(sc => sc.CargoCapacity);
        }

        [TestMethod]
        public void UsedCargoCapacityShouldReturnShipUsedCargoCapacity()
        {
            //Arrange


            //Act
            var usedCargoCapacity = _inventoryProvider.UsedCargoCapacity;

            //Assert
            _shipCache.VerifyGet(sc => sc.UsedCargoCapacity);
        }

        [TestMethod]
        public void CargoShouldReturnShipCargo()
        {
            //Arrange


            //Act
            var cargo = _inventoryProvider.Cargo;

            //Assert
            _shipCache.VerifyGet(sc => sc.Cargo);
        }

        [TestMethod]
        public void GetCargoHoldWindowShouldQueryForCargoHoldWindow()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(true);
            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(true);
            eveInvWindow.Setup(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipCargoName)))
                .Returns(eveInvChildWindow.Object);

            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            var cargoHoldWindow = _inventoryProvider.GetCargoHoldWindow();

            //Assert
            _eveWindowProvider.Verify(ewp => ewp.GetInventoryWindow(), Times.Once());
            eveInvWindow.Verify(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipCargoName)),
                Times.Once());

            Assert.IsNotNull(cargoHoldWindow);
            Assert.IsTrue(cargoHoldWindow.IsValid);
        }

        [TestMethod]
        public void GetCargoHoldWindowShouldReturnNullWhenEveInvWindowIsNull()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns<IEveInvWindow>(null);

            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            var cargoHoldWindow = _inventoryProvider.GetCargoHoldWindow();

            //Assert
            Assert.IsNull(cargoHoldWindow);
        }

        [TestMethod]
        public void GetCargoHoldWindowShouldReturnNullWhenEveInvWindowIsInvalid()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(false);
            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(true);
            eveInvWindow.Setup(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipCargoName)))
                .Returns(eveInvChildWindow.Object);

            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            var cargoHoldWindow = _inventoryProvider.GetCargoHoldWindow();

            //Assert
            Assert.IsNull(cargoHoldWindow);
        }

        [TestMethod]
        public void MakeCargoHoldActiveShouldMakeCargoHoldActive()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(true);
            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(true);
            eveInvWindow.Setup(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipCargoName)))
                .Returns(eveInvChildWindow.Object);

            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            _inventoryProvider.MakeCargoHoldActive();

            //Assert
            _eveWindowProvider.Verify(ewp => ewp.GetInventoryWindow(), Times.Once());
            eveInvWindow.Verify(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipCargoName)),
                Times.Once());

            eveInvChildWindow.Verify(eicw => eicw.MakeActive(), Times.Once());
        }

        [TestMethod]
        public void MakeCargoHoldActiveShouldNotMakeCargoHoldActiveWhenCargoHoldWindowIsNull()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(true);
            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(true);
            eveInvWindow.Setup(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipCargoName)))
                .Returns<IEveInvChildWindow>(null);

            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            _inventoryProvider.MakeCargoHoldActive();

            //Assert
            _eveWindowProvider.Verify(ewp => ewp.GetInventoryWindow(), Times.Once());
            eveInvWindow.Verify(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipCargoName)),
                Times.Once());

            eveInvChildWindow.Verify(eicw => eicw.MakeActive(), Times.Never());
        }

        [TestMethod]
        public void MakeCargoHoldActiveShouldNotMakeCargoHoldActiveWhenCargoHoldWindowIsInvalid()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(true);
            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(false);
            eveInvWindow.Setup(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipCargoName)))
                .Returns(eveInvChildWindow.Object);

            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            _inventoryProvider.MakeCargoHoldActive();

            //Assert
            _eveWindowProvider.Verify(ewp => ewp.GetInventoryWindow(), Times.Once());
            eveInvWindow.Verify(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipCargoName)),
                Times.Once());

            eveInvChildWindow.Verify(eicw => eicw.MakeActive(), Times.Never());
        }
        #endregion

        #region Ore Hold

        [TestMethod]
        public void HaveOreHoldShouldReturnShipHasOreHold()
        {
            //Arrange


            //Act
            var haveOreHold = _inventoryProvider.HaveOreHold;

            //Assert
            _shipCache.VerifyGet(sc => sc.HasOreHold);
        }

        [TestMethod]
        public void IsOreHoldActiveShouldReturnTrueIfActiveChildIsValidAndMatchesItemIdAndMatchesName()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(true);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(true);
            eveInvChildWindow.Setup(eicw => eicw.ItemId).Returns(1);
            eveInvChildWindow.Setup(eicw => eicw.Name).Returns(InventoryProvider.ShipOreHoldName);
            eveInvWindow.Setup(eiw => eiw.ActiveChild).Returns(eveInvChildWindow.Object);

            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            var isOreHoldActive = _inventoryProvider.IsOreHoldActive;

            //Assert
            Assert.IsTrue(isOreHoldActive);
        }

        [TestMethod]
        public void IsOreHoldActiveShouldReturnFalseIfActiveChildIsNull()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(true);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(true);
            eveInvChildWindow.Setup(eicw => eicw.ItemId).Returns(1);
            eveInvChildWindow.Setup(eicw => eicw.Name).Returns(InventoryProvider.ShipOreHoldName);
            eveInvWindow.Setup(eiw => eiw.ActiveChild).Returns<IEveInvChildWindow>(null);

            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            var isOreHoldActive = _inventoryProvider.IsOreHoldActive;

            //Assert
            Assert.IsFalse(isOreHoldActive);
        }

        [TestMethod]
        public void IsOreHoldActiveShouldReturnFalseIfActiveChildIsInvalid()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(true);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(false);
            eveInvChildWindow.Setup(eicw => eicw.ItemId).Returns(1);
            eveInvChildWindow.Setup(eicw => eicw.Name).Returns(InventoryProvider.ShipOreHoldName);
            eveInvWindow.Setup(eiw => eiw.ActiveChild).Returns(eveInvChildWindow.Object);

            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            var isOreHoldActive = _inventoryProvider.IsOreHoldActive;

            //Assert
            Assert.IsFalse(isOreHoldActive);
        }

        [TestMethod]
        public void IsOreHoldActiveShouldReturnFalseIfInventoryWindowIsNull()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(true);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(true);
            eveInvChildWindow.Setup(eicw => eicw.ItemId).Returns(1);
            eveInvChildWindow.Setup(eicw => eicw.Name).Returns(InventoryProvider.ShipOreHoldName);
            eveInvWindow.Setup(eiw => eiw.ActiveChild).Returns(eveInvChildWindow.Object);

            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns<IEveInvWindow>(null);

            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            var isOreHoldActive = _inventoryProvider.IsOreHoldActive;

            //Assert
            Assert.IsFalse(isOreHoldActive);
        }

        [TestMethod]
        public void IsOreHoldActiveShouldReturnFalseIfInventoryWindowIsInvalid()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(false);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(true);
            eveInvChildWindow.Setup(eicw => eicw.ItemId).Returns(1);
            eveInvChildWindow.Setup(eicw => eicw.Name).Returns(InventoryProvider.ShipOreHoldName);
            eveInvWindow.Setup(eiw => eiw.ActiveChild).Returns(eveInvChildWindow.Object);

            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            var isOreHoldActive = _inventoryProvider.IsOreHoldActive;

            //Assert
            Assert.IsFalse(isOreHoldActive);
        }

        [TestMethod]
        public void IsOreHoldActiveShouldReturnFalseIfShipIdDoesNotMatch()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(true);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(true);
            eveInvChildWindow.Setup(eicw => eicw.ItemId).Returns(2);
            eveInvChildWindow.Setup(eicw => eicw.Name).Returns(InventoryProvider.ShipOreHoldName);
            eveInvWindow.Setup(eiw => eiw.ActiveChild).Returns(eveInvChildWindow.Object);

            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            var isOreHoldActive = _inventoryProvider.IsOreHoldActive;

            //Assert
            Assert.IsFalse(isOreHoldActive);
        }

        [TestMethod]
        public void IsOreHoldActiveShouldReturnFalseIfNameDoesNotMatch()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(true);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(true);
            eveInvChildWindow.Setup(eicw => eicw.ItemId).Returns(1);
            eveInvChildWindow.Setup(eicw => eicw.Name).Returns(InventoryProvider.ShipCargoName);
            eveInvWindow.Setup(eiw => eiw.ActiveChild).Returns(eveInvChildWindow.Object);

            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            var isOreHoldActive = _inventoryProvider.IsOreHoldActive;

            //Assert
            Assert.IsFalse(isOreHoldActive);
        }

        [TestMethod]
        public void GetOreHoldWindowShouldQueryForOreHoldWindow()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(true);
            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(true);
            eveInvWindow.Setup(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipOreHoldName)))
                .Returns(eveInvChildWindow.Object);

            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            var oreHoldWindow = _inventoryProvider.GetOreHoldWindow();

            //Assert
            _eveWindowProvider.Verify(ewp => ewp.GetInventoryWindow(), Times.Once());
            eveInvWindow.Verify(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipOreHoldName)),
                Times.Once());

            Assert.IsNotNull(oreHoldWindow);
            Assert.IsTrue(oreHoldWindow.IsValid);
        }

        [TestMethod]
        public void GetOreHoldWindowShouldReturnNullWhenEveInvWindowIsNull()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns<IEveInvWindow>(null);

            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            var oreHoldWindow = _inventoryProvider.GetOreHoldWindow();

            //Assert
            Assert.IsNull(oreHoldWindow);
        }

        [TestMethod]
        public void GetOreHoldWindowShouldReturnNullWhenEveInvWindowIsInvalid()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(false);
            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(true);
            eveInvWindow.Setup(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipOreHoldName)))
                .Returns(eveInvChildWindow.Object);

            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            var cargoHoldWindow = _inventoryProvider.GetOreHoldWindow();

            //Assert
            Assert.IsNull(cargoHoldWindow);
        }

        [TestMethod]
        public void MakeOreHoldActiveShouldMakeOreHoldActive()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(true);
            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(true);
            eveInvWindow.Setup(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipOreHoldName)))
                .Returns(eveInvChildWindow.Object);

            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            _inventoryProvider.MakeOreHoldActive();

            //Assert
            _eveWindowProvider.Verify(ewp => ewp.GetInventoryWindow(), Times.Once());
            eveInvWindow.Verify(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipOreHoldName)),
                Times.Once());

            eveInvChildWindow.Verify(eicw => eicw.MakeActive(), Times.Once());
        }

        [TestMethod]
        public void MakeOreHoldActiveShouldNotMakeOreHoldActiveWhenOreHoldWindowIsNull()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(true);
            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(true);
            eveInvWindow.Setup(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipOreHoldName)))
                .Returns<IEveInvChildWindow>(null);

            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            _inventoryProvider.MakeOreHoldActive();

            //Assert
            _eveWindowProvider.Verify(ewp => ewp.GetInventoryWindow(), Times.Once());
            eveInvWindow.Verify(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipOreHoldName)),
                Times.Once());

            eveInvChildWindow.Verify(eicw => eicw.MakeActive(), Times.Never());
        }

        [TestMethod]
        public void MakeOreHoldActiveShouldNotMakeOreHoldActiveWhenOreHoldWindowIsInvalid()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(true);
            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(false);
            eveInvWindow.Setup(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipOreHoldName)))
                .Returns(eveInvChildWindow.Object);

            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            _inventoryProvider.MakeOreHoldActive();

            //Assert
            _eveWindowProvider.Verify(ewp => ewp.GetInventoryWindow(), Times.Once());
            eveInvWindow.Verify(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipOreHoldName)),
                Times.Once());

            eveInvChildWindow.Verify(eicw => eicw.MakeActive(), Times.Never());
        }

        [TestMethod]
        public void OreHoldCapacityShouldReturnOreHoldWindowCapacity()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(true);
            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(true);
            eveInvWindow.Setup(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipOreHoldName)))
                .Returns(eveInvChildWindow.Object);

            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            var oreHoldCapacity = _inventoryProvider.OreHoldCapacity;

            //Assert
            _eveWindowProvider.Verify(ewp => ewp.GetInventoryWindow(), Times.Once());
            eveInvWindow.Verify(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipOreHoldName)),
                Times.Once());

            eveInvChildWindow.VerifyGet(eicw => eicw.Capacity, Times.Once());
        }

        [TestMethod]
        public void OreHoldCapacityShouldReturnNegativeOneWhenOreHoldWindowIsNull()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(true);
            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(true);
            eveInvWindow.Setup(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipOreHoldName)))
                .Returns<IEveInvChildWindow>(null);

            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            var oreHoldCapacity = _inventoryProvider.OreHoldCapacity;

            //Assert
            _eveWindowProvider.Verify(ewp => ewp.GetInventoryWindow(), Times.Once());
            eveInvWindow.Verify(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipOreHoldName)),
                Times.Once());

            eveInvChildWindow.VerifyGet(eicw => eicw.Capacity, Times.Never());

            Assert.AreEqual(-1, oreHoldCapacity);
        }

        [TestMethod]
        public void OreHoldCapacityShouldReturnNegativeOneWhenOreHoldWindowIsInvalid()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(true);
            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(false);
            eveInvWindow.Setup(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipOreHoldName)))
                .Returns(eveInvChildWindow.Object);

            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            var oreHoldCapacity = _inventoryProvider.OreHoldCapacity;

            //Assert
            _eveWindowProvider.Verify(ewp => ewp.GetInventoryWindow(), Times.Once());
            eveInvWindow.Verify(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipOreHoldName)),
                Times.Once());

            eveInvChildWindow.VerifyGet(eicw => eicw.Capacity, Times.Never());

            Assert.AreEqual(-1, oreHoldCapacity);
        }

        [TestMethod]
        public void OreHoldUsedCapacityShouldReturnOreHoldWindowUsedCapacity()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(true);
            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(true);
            eveInvWindow.Setup(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipOreHoldName)))
                .Returns(eveInvChildWindow.Object);

            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            var oreHoldUsedCapacity = _inventoryProvider.OreHoldUsedCapacity;

            //Assert
            _eveWindowProvider.Verify(ewp => ewp.GetInventoryWindow(), Times.Once());
            eveInvWindow.Verify(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipOreHoldName)),
                Times.Once());

            eveInvChildWindow.VerifyGet(eicw => eicw.UsedCapacity, Times.Once());
        }

        [TestMethod]
        public void OreHoldUsedCapacityShouldReturnNegativeOneWhenOreHoldWindowIsNull()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(true);
            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(true);
            eveInvWindow.Setup(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipOreHoldName)))
                .Returns<IEveInvChildWindow>(null);

            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            var oreHoldUsedCapacity = _inventoryProvider.OreHoldUsedCapacity;

            //Assert
            _eveWindowProvider.Verify(ewp => ewp.GetInventoryWindow(), Times.Once());
            eveInvWindow.Verify(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipOreHoldName)),
                Times.Once());

            eveInvChildWindow.VerifyGet(eicw => eicw.UsedCapacity, Times.Never());

            Assert.AreEqual(-1, oreHoldUsedCapacity);
        }

        [TestMethod]
        public void OreHoldUsedCapacityShouldReturnNegativeOneWhenOreHoldWindowIsInvalid()
        {
            //Arrange
            var eveInvWindow = new Mock<IEveInvWindow>();
            eveInvWindow.Setup(eiw => eiw.IsValid).Returns(true);
            _eveWindowProvider.Setup(ewp => ewp.GetInventoryWindow()).Returns(eveInvWindow.Object);

            var eveInvChildWindow = new Mock<IEveInvChildWindow>();
            eveInvChildWindow.Setup(eicw => eicw.IsValid).Returns(false);
            eveInvWindow.Setup(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipOreHoldName)))
                .Returns(eveInvChildWindow.Object);

            _shipCache.Setup(sc => sc.Id).Returns(1);

            //Act
            var oreHoldUsedCapacity = _inventoryProvider.OreHoldUsedCapacity;

            //Assert
            _eveWindowProvider.Verify(ewp => ewp.GetInventoryWindow(), Times.Once());
            eveInvWindow.Verify(eiw => eiw.GetChildWindow(It.Is<Int64>(i => i == 1), It.Is<string>(s => s == InventoryProvider.ShipOreHoldName)),
                Times.Once());

            eveInvChildWindow.VerifyGet(eicw => eicw.UsedCapacity, Times.Never());

            Assert.AreEqual(-1, oreHoldUsedCapacity);
        }

        [TestMethod]
        public void OreHoldCargoShouldReturnShipCacheOreHoldCargo()
        {
            //Arrange

            //Act
            var oreHoldCargo = _inventoryProvider.OreHoldCargo;

            //Assert
            _shipCache.VerifyGet(sc => sc.OreHoldCargo, Times.Once());
        }
        #endregion
    }
}
