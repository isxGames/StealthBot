﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.9.0.77
//      SpecFlow Generator Version:1.9.0.0
//      Runtime Version:4.0.30319.18046
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace StealthBot.SpecFlow.Actions
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.77")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("DropOffToJetissonContainerActionTest")]
    [NUnit.Framework.CategoryAttribute("DropOffToJetissonContainerActionTest")]
    public partial class DropOffToJetissonContainerActionTestFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "DropOffToJetissonContainerActionTest.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "DropOffToJetissonContainerActionTest", "As a miner\r\nI want to unload ore to a jetisson container", ProgrammingLanguage.CSharp, new string[] {
                        "DropOffToJetissonContainerActionTest"});
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.TestFixtureTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("1 a - When no jetcan exists / is active, and I have ore in my ore hold, create a " +
            "new one")]
        public virtual void _1A_WhenNoJetcanExistsIsActiveAndIHaveOreInMyOreHoldCreateANewOne()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("1 a - When no jetcan exists / is active, and I have ore in my ore hold, create a " +
                    "new one", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "!this",
                        "Name"});
            table1.AddRow(new string[] {
                        "<scordite>",
                        "Scordite"});
#line 7
testRunner.Given("I have ore hold items", ((string)(null)), table1, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "!this",
                        "ID",
                        "Name",
                        "IsLockedTarget"});
#line 10
testRunner.And("I have entities", ((string)(null)), table2, "And ");
#line 12
testRunner.When("I process jetisson container dropoff (\'1\' time(s))", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 13
testRunner.Then("item \'<scordite>\' should have been jetissoned", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("1 b - When no jetcan exists / is active, and I have ore in my cargo hold, create " +
            "a new one")]
        public virtual void _1B_WhenNoJetcanExistsIsActiveAndIHaveOreInMyCargoHoldCreateANewOne()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("1 b - When no jetcan exists / is active, and I have ore in my cargo hold, create " +
                    "a new one", ((string[])(null)));
#line 15
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "!this",
                        "Name",
                        "CategoryID"});
            table3.AddRow(new string[] {
                        "<scordite>",
                        "Scordite",
                        "25"});
#line 16
testRunner.Given("I have cargo hold items", ((string)(null)), table3, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "!this",
                        "ID",
                        "Name",
                        "IsLockedTarget"});
#line 19
testRunner.And("I have entities", ((string)(null)), table4, "And ");
#line 21
testRunner.When("I process jetisson container dropoff (\'1\' time(s))", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 22
testRunner.Then("item \'<scordite>\' should have been jetissoned", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("1 c - When no jetcan exists / is active, and I have ore in my ore hold and cargo," +
            " create a new one from the ore hold item")]
        public virtual void _1C_WhenNoJetcanExistsIsActiveAndIHaveOreInMyOreHoldAndCargoCreateANewOneFromTheOreHoldItem()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("1 c - When no jetcan exists / is active, and I have ore in my ore hold and cargo," +
                    " create a new one from the ore hold item", ((string[])(null)));
#line 24
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "!this",
                        "Name",
                        "CategoryID"});
            table5.AddRow(new string[] {
                        "<scordite>",
                        "Scordite",
                        "25"});
#line 25
testRunner.Given("I have cargo hold items", ((string)(null)), table5, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "!this",
                        "Name"});
            table6.AddRow(new string[] {
                        "<veldspar>",
                        "Veldspar"});
#line 28
testRunner.And("I have ore hold items", ((string)(null)), table6, "And ");
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "!this",
                        "ID",
                        "Name",
                        "IsLockedTarget"});
#line 31
testRunner.And("I have entities", ((string)(null)), table7, "And ");
#line 33
testRunner.When("I process jetisson container dropoff (\'1\' time(s))", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 34
testRunner.Then("item \'<veldspar>\' should have been jetissoned", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 35
testRunner.And("item \'<scordite>\' should not have been jettisoned", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("2 a - When no jetcan was active, and I had no existing cans, and I jettison an it" +
            "em, I should detect the new can and make it active")]
        public virtual void _2A_WhenNoJetcanWasActiveAndIHadNoExistingCansAndIJettisonAnItemIShouldDetectTheNewCanAndMakeItActive()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("2 a - When no jetcan was active, and I had no existing cans, and I jettison an it" +
                    "em, I should detect the new can and make it active", ((string[])(null)));
#line 37
this.ScenarioSetup(scenarioInfo);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("2 b - When no jetcan was active, and I had existing nearby jetcans, and I jettiso" +
            "n an item, I should detect the new can and make it active")]
        public virtual void _2B_WhenNoJetcanWasActiveAndIHadExistingNearbyJetcansAndIJettisonAnItemIShouldDetectTheNewCanAndMakeItActive()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("2 b - When no jetcan was active, and I had existing nearby jetcans, and I jettiso" +
                    "n an item, I should detect the new can and make it active", ((string[])(null)));
#line 39
this.ScenarioSetup(scenarioInfo);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("3 - Only try to create a new jetisson container after the 3-minute cooldown")]
        public virtual void _3_OnlyTryToCreateANewJetissonContainerAfterThe3_MinuteCooldown()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("3 - Only try to create a new jetisson container after the 3-minute cooldown", ((string[])(null)));
#line 41
this.ScenarioSetup(scenarioInfo);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("4 - When the active jetcan is nearly full, it should be considered full and marke" +
            "d for pickup")]
        public virtual void _4_WhenTheActiveJetcanIsNearlyFullItShouldBeConsideredFullAndMarkedForPickup()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("4 - When the active jetcan is nearly full, it should be considered full and marke" +
                    "d for pickup", ((string[])(null)));
#line 43
this.ScenarioSetup(scenarioInfo);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("5 - When no jetcan exists / is active, and I create one from either source, and j" +
            "etisson a full can, it should finish the dropoff cycle renamed and pickup reques" +
            "ted")]
        public virtual void _5_WhenNoJetcanExistsIsActiveAndICreateOneFromEitherSourceAndJetissonAFullCanItShouldFinishTheDropoffCycleRenamedAndPickupRequested()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("5 - When no jetcan exists / is active, and I create one from either source, and j" +
                    "etisson a full can, it should finish the dropoff cycle renamed and pickup reques" +
                    "ted", ((string[])(null)));
#line 45
this.ScenarioSetup(scenarioInfo);
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
