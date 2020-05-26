using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVE.ISXEVE.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SpecAid;
using StealthBot.Core;
using StealthBot.Core.Interfaces;
using TechTalk.SpecFlow;
using IModule = EVE.ISXEVE.Interfaces.IModule;
using IShip = StealthBot.Core.IShip;

namespace StealthBot.SpecFlow
{
    public class CommonSteps
    {
        protected void Initialize()
        {
            Ship = new Mock<IShip>();
            EntityProvider = new Mock<IEntityProvider>();
            Logging = new Mock<ILogging>();
        }

        #region Scenario Context
        protected T GetFromScenarioContext<T>(string key)
        {
            return (T) ScenarioContext.Current[key];
        }

        protected void SetInScenarioContext<T>(string key, T obj)
        {
            if (!ScenarioContext.Current.ContainsKey(key))
            {
                ScenarioContext.Current.Add(key, obj);
            }
            else
            {
                ScenarioContext.Current[key] = obj;
            }
        }

        protected IEnumerable<Mock<IEntityWrapper>> EntityMocks
        {
            get { return GetFromScenarioContext<IEnumerable<Mock<IEntityWrapper>>>("entityMocks"); }
            set
            {
                SetInScenarioContext("entityMocks", value);

                var entitiesById = value.Select(em => em.Object).ToDictionary(entity => entity.ID, entity => entity);
                EntityProvider.Setup(ep => ep.EntityWrappers).Returns(entitiesById.Values.ToList().AsReadOnly());
                EntityProvider.Setup(ep => ep.EntityWrappersById).Returns(entitiesById);
            }
        }

        protected IEnumerable<Mock<IModule>> ModuleMocks
        {
            get { return GetFromScenarioContext<IEnumerable<Mock<IModule>>>("moduleMocks"); }
            set 
            {
                SetInScenarioContext("moduleMocks", value);

                var modules = value.Select(mm => mm.Object);
                Ship.Setup(s => s.AllModules).Returns(modules.ToList().AsReadOnly());
            }
        }

        protected Mock<IShip> Ship
        {
            get { return GetFromScenarioContext<Mock<IShip>>("ship"); }
            set { SetInScenarioContext("ship", value); }
        }

        protected Mock<IEntityProvider> EntityProvider
        {
            get { return GetFromScenarioContext<Mock<IEntityProvider>>("entityPopulator"); }
            set { SetInScenarioContext("entityPopulator", value); }
        }

        protected Mock<ILogging> Logging
        {
            get { return GetFromScenarioContext<Mock<ILogging>>("logging"); }
            set { SetInScenarioContext("logging", value); }
        }

        protected Mock<IIsxeveProvider> IsxeveProvider
        {
            get { return GetFromScenarioContext<Mock<IIsxeveProvider>>("isxeveProvider"); }
            set { SetInScenarioContext("isxeveProvider", value); }
        }

        protected Mock<IEve> Eve
        {
            get { return GetFromScenarioContext<Mock<IEve>>("eve"); }
            set { SetInScenarioContext("eve", value); }
        }
        #endregion

        #region Table Translation Helpers
        
        protected virtual void GivenIHaveEntities(Table table)
        {
            var entityMocks = new List<Mock<IEntityWrapper>>();

            foreach (var row in table.Rows)
            {
                var entityMock = new Mock<IEntityWrapper>();

                if (row.ContainsKey("ID"))
                    entityMock.Setup(e => e.ID).Returns(Int64.Parse(row["ID"]));

                if (row.ContainsKey("Name"))
                    entityMock.Setup(e => e.Name).Returns(row["Name"]);

                if (row.ContainsKey("IsLockedTarget"))
                    entityMock.Setup(e => e.IsLockedTarget).Returns(bool.Parse(row["IsLockedTarget"]));

                if (row.ContainsKey("TypeID"))
                    entityMock.SetupGet(e => e.TypeID).Returns(int.Parse(row["TypeID"]));

                if (row.ContainsKey("GroupID"))
                    entityMock.SetupGet(e => e.GroupID).Returns(int.Parse(row["GroupID"]));

                if (row.ContainsKey("Distance"))
                    entityMock.SetupGet(e => e.Distance).Returns(double.Parse(row["Distance"]));

                if (row.ContainsKey("X"))
                    entityMock.SetupGet(e => e.X).Returns(double.Parse(row["X"]));

                if (row.ContainsKey("Y"))
                    entityMock.SetupGet(e => e.Y).Returns(double.Parse(row["Y"]));

                if (row.ContainsKey("Z"))
                    entityMock.SetupGet(e => e.Z).Returns(double.Parse(row["Z"]));

                if (row.ContainsKey("IsTargetingMe"))
                    entityMock.SetupGet(e => e.IsTargetingMe).Returns(bool.Parse(row["IsTargetingMe"]));

                entityMock.Setup(e => e.UnlockTarget())
                    .Callback(() => { entityMock.Setup(m => m.IsLockedTarget).Returns(false); });

                entityMock.Setup(e => e.LockTarget()).Callback(() => { entityMock.Setup(m => m.IsLockedTarget).Returns(true); });

                if (row.ContainsKey("!this"))
                {
                    RecallAid.It[row["!this"]] = entityMock;
                }
                entityMocks.Add(entityMock);
            }

            var countById = entityMocks.Select(em => em.Object).GroupBy(x => x.ID);
            foreach (var g in countById)
            {
                Assert.AreEqual(1, g.Count(), string.Format("More than one Mock<IEntityWrapper> exists with ID {0}.", g.Key));
            }

            EntityMocks = entityMocks;
        }

        protected virtual void GivenIHaveItemInfo(Table table)
        {
            var itemInfoMocks = new List<Mock<IItemInfo>>();

            foreach (var row in table.Rows)
            {
                var itemInfoMock = new Mock<IItemInfo>();

                itemInfoMock.SetupGet(ii => ii.TypeID).Returns(int.Parse(row["TypeID"]));

                if (row.ContainsKey("!this"))
                    RecallAid.It[row["!this"]] = itemInfoMock;

                if (row.ContainsKey("Type"))
                    itemInfoMock.SetupGet(ii => ii.Type).Returns(row["Type"]);

                if (row.ContainsKey("ShieldRadius"))
                    itemInfoMock.SetupGet(ii => ii.ShieldRadius).Returns(int.Parse(row["ShieldRadius"]));

                Eve.Setup(e => e.ItemInfo(It.Is<int>(i => i == int.Parse(row["TypeID"])))).Returns(itemInfoMock.Object);
                itemInfoMocks.Add(itemInfoMock);
            }
        }

        protected IEnumerable<Mock<IItem>> CreateItemMocksFromTable(Table table)
        {
            var itemMocks = new List<Mock<IItem>>();

            foreach (var row in table.Rows)
            {
                var itemMock = CreateItemMockFromRow(row);
                itemMocks.Add(itemMock);
            }

            return itemMocks;
        }

        protected Mock<IItem> CreateItemMockFromRow(TableRow row)
        {
            var itemMock = new Mock<IItem>();

            if (row.ContainsKey("!this"))
            {
                RecallAid.It[row["!this"]] = itemMock;
            }

            if (row.ContainsKey("Name"))
            {
                itemMock.Setup(item => item.Name).Returns(row["Name"]);
            }

            if (row.ContainsKey("CategoryID"))
            {
                itemMock.Setup(item => item.CategoryID).Returns(int.Parse(row["CategoryID"]));
            }

            return itemMock;
        }

        protected IEnumerable<Mock<IBookMark>> CreateBookMarkMocksFromTable(Table table)
        {
            var bookMarkMocks = new List<Mock<IBookMark>>();

            foreach (var row in table.Rows)
            {
                var bookMarkMock = CreateBookMarkMockFromRow(row);
                bookMarkMocks.Add(bookMarkMock);
            }

            return bookMarkMocks;
        }

        protected Mock<IBookMark> CreateBookMarkMockFromRow(TableRow row)
        {
            var bookmarkMock = new Mock<IBookMark>();

            if (row.ContainsKey("!this"))
                RecallAid.It[row["!this"]] = bookmarkMock;

            if (row.ContainsKey("Label"))
                bookmarkMock.SetupGet(bm => bm.Label).Returns(row["Label"]);

            if (row.ContainsKey("X"))
                bookmarkMock.SetupGet(bm => bm.X).Returns(double.Parse(row["X"]));

            if (row.ContainsKey("Y"))
                bookmarkMock.SetupGet(bm => bm.Y).Returns(double.Parse(row["Y"]));

            if (row.ContainsKey("Z"))
                bookmarkMock.SetupGet(bm => bm.Z).Returns(double.Parse(row["Z"]));

            if (row.ContainsKey("SolarSystemID"))
                bookmarkMock.SetupGet(bm => bm.SolarSystemID).Returns(int.Parse(row["SolarSystemID"]));

            if (row.ContainsKey("ID"))
                bookmarkMock.SetupGet(bm => bm.ID).Returns(long.Parse(row["ID"]));

            if (row.ContainsKey("ItemID"))
                bookmarkMock.SetupGet(bm => bm.ItemID).Returns(int.Parse(row["ItemID"]));

            return bookmarkMock;
        }
        #endregion
    }
}
