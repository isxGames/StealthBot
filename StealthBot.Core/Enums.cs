using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace StealthBot
{
	public enum StealthBotBuilds
	{
		Stable,
		Testing,
		Beta
	}

    public enum Ranges : long
    {
        Warp = 150000,
        Dock = 200,
        LootActivate = 2500,
		PlanetWarpIn = 100000000000		//1 AU = 150 billion meters
    }

    public enum TypeIDs
    {
        SolarSystem = 5,
		Plagioclase = 18,
		Spodumain = 19,
		Kernite = 20,
		Hedbergite = 21,
		Arkonor = 22,
        CarbonizedLeadS = 178,
        NuclearS = 179,
        ProtonS = 180,
        DepletedUraniumS = 181,
        TitaniumSabotS = 182,
        FusionS = 183,
        PhasedPlasmaS = 184,
        EMPS = 185,
        CarbonizedLeadM = 186,
        NuclearM = 187,
        ProtonM = 188,
        DepletedUraniumM = 189,
        TitaniumSabotM = 190,
        FusionM = 191,
        PhasedPlasmaM = 192,
        EMPM = 193,
        CarbonizedLeadL = 194,
        NuclearL = 195,
        ProtonL = 196,
        DepletedUraniumL = 197,
        TitaniumSabotL = 198,
        FusionL = 199,
        PhasedPlasmaL = 200,
        EMPL = 201,
        Capsule = 670,
		Bistot = 1223,
		Pyroxeres = 1224,
		Crokite = 1225,
		Jaspet = 1226,
		Omber = 1227,
		Scordite = 1228,
		Gneiss = 1229,
		Veldspar = 1230,
		Hemorphite = 1231,
		Dark_Ochre = 1232,
		Amarr_Trade_Post = 1932,
		Customs_Office = 2233,
		Minmatar_Hub = 2496,
		Minmatar_Trade_Post = 2502,
		Concord_Starbase = 9868,
		Beacon = 10124,
        PrototypeCloakingDevice = 11370,
		Mercoxit = 11396,
        CovertOpsCloakingDevice = 11578,
		Clear_Icicle = 16262,
		Glacial_Mass = 16263,
		Blue_Ice = 16264,
		White_Glaze = 16265,
		Glare_Crust = 16266,
		Dark_Glitter = 16267,
		Gelidus = 16268,
		Krystallos = 16269,
        Ice_Harvester_I = 16278,
        GuristasGreatWall = 16793,
		Crimson_Arkonor = 17425,
		Prime_Arkonor = 17426,
		Triclinic_Bistot = 17428,
		Monoclinic_Bistot = 17429,
		Sharp_Crokite = 17432,
		Crystalline_Crokite = 17433,
		Onyx_Ochre = 17436,
		Obsidian_Ochre = 17437,
		Vitric_Hedbergite = 17440,
		Glazed_Hedbergite = 17441,
		Vivid_Hemorphite = 17444,
		Radiant_Hemorphite = 17445,
		Pure_Jaspet = 17448,
		Pristine_Jaspet = 17449,
		Luminous_Kernite = 17452,
		Fiery_Kernite = 17453,
		Azure_Plagioclase = 17455,
		Rich_Plagioclase = 17456,
		Solid_Pyroxeres = 17459,
		Viscous_Pyroxeres = 17460,
		Condensed_Scordite = 17463,
		Massive_Scordite = 17464,
		Bright_Spodumain = 17466,
		Gleaming_Spodumain = 17467,
		Concentrated_Veldspar = 17470,
		Dense_Veldspar = 17471,
        Covetor = 17476,
        Retriever = 17478,
        Procurer = 17480,
		CorporateHangarArray = 17621,
		Iridescent_Gneiss = 17865,
		Prismatic_Gneiss = 17866,
		Silvery_Omber = 17867,
		Golden_Omber = 17868,
		Magma_Mercoxit = 17869,
		Vitreous_Mercoxit = 17870,
		Thick_Blue_Ice = 17975,
		Pristine_White_Glaze = 17976,
		Smooth_Glacial_Mass = 17977,
		Enriched_Clear_Icicle = 17978,
        Ice_Harvester_II = 22229,
        Hulk = 22544,
        Skiff = 22546,
        Mackinaw = 22548,
		MiningForemanLink_HarvesterCapacitorEfficiency = 22553,
		MiningForemanLink_MiningLaserFieldEnhancement = 22555,
		MiningForemanLink_LaserOptimization = 22557,
        IndustrialCore = 28583,
        Salvager = 25861,
		Orca = 28606,
		Banidine = 28617,
		Augumene = 28618,
		Mercium = 28619,
		Lyavite = 28620,
		Pithix = 28621,
		Green_Arisite = 28622,
		Oeryl = 28623,
		Geodite = 28624,
		Polygypsum = 28625,
		Zuthrine = 28626,
		Azure_Ice = 28627,
		Crystalline_Icicle = 28628,
		Gamboge_Cytoserocin = 28629,
		Chartreuse_Cytoserocin = 28630
    }

    public enum CategoryIDs
    {
		Celestial = 2,
        Station = 3,
        Ship = 6,
		Charge = 8,
        Entity = 11,
        Structure = 23,
        Asteroid = 25
    }

    public enum GroupIDs
    {
		Planet = 7,
		Moon = 8,
        AsteroidBelt = 9,
        Stargate = 10,
        CargoContainer = 12,
        Station = 15,
        Industrial = 28,
        ShieldRecharger = 39,
        ShieldBooster = 40,
		AfterBurner = 46,
		SurveyScanner = 49,
        EnergyWeapon = 53,
        MiningLaser = 54,
        ProjectileWeapon = 55,
        MissileLauncher = 56,
        DamageControl = 60,
        ArmorRepairer = 62,
		StasisWebifier = 65,
		EnergyVampire = 68,
        HybridWeapon = 74,
        ShieldHardener = 77,
		ProjectileCharge = 83,
		HybridCharge = 85,
		FrequencyCrystal = 86,
        Torpedo = 89,
		SentryGun = 99,
		CombatDrone = 100,
		PoliceDrone = 182,
		Wreck = 186,
        ECCM = 202,
		SensorBooster = 212,
		TrackingComputer = 213,
		LargeCollidableObject = 226,
		FactionDrone = 288,
		ConvoyDrone = 297,
		ConcordDrone = 301,
		SpawnContainer = 306,
		GangLink = 316,
		LargeCollidableStructure = 319,
		Billboard = 323,
        ArmorHardener = 328,
        CloakingDevice = 330,
		ControlTower = 365,
        WarpGate = 366,
		AdvancedAutoCannonCharge = 372,
		AdvancedBlasterCharge = 373,
		AdvancedBeamLaserCrystal = 374,
		AdvancedPulseLaserCrystal = 375,
		AdvancedArtilleryCharge = 376,
		AdvancedRailGunCharge = 377,
		TargetPainter = 379,
        TransportShip = 380,
		DestructibleSentryGun = 383,
        LightMissile = 384,
        HeavyMissile = 385,
        CruiseMissile = 386,
        DeadspaceOverseer = 435,
		CustomsOfficial = 446,
		Arkonor = 450,
		Bistot = 451,
		Crokite = 452,
		Dark_Ochre = 453,
		Hedbergite = 454,
		Hemorphite = 455,
		Jaspet = 456,
		Kernite = 457,
		Plagioclase = 458,
		Pyroxeres = 459,
		Scordite = 460,
		Spodumain = 461,
		Veldspar = 462,
        MiningBarge = 463,
        StripMiner = 464,
		Gneiss = 467,
		Mercoxit = 468,
		Omber = 469,
		AnchorableCargoContainerNEEDSUPDATE = 471,
        MiningCrystal = 482,
        FrequencyMiningLaser = 483,
		DeadspaceOverseersStructure = 494,
        MissileLauncher_Cruise = 506,
        MissileLauncher_Rocket = 507,
        MissileLauncher_Siege = 508,
        MissileLauncher_Standard = 509,
        MissileLauncher_Heavy = 510,
        MissileLauncher_Assault = 511,
        MissileLauncher_Defender = 512,
        IndustrialCore = 515,
        DataMiner = 538,
        Exhumer = 543,
        AsteroidAngelCartelFrigate = 550,
        AsteroidAngelCartelCruiser = 551,
        AsteroidAngelCartelBattleship = 552,
        AsteroidAngelCartelOfficer = 553,
        AsteroidAngelCartelHauler = 554,
        AsteroidBloodRaidersCruiser = 555,
        AsteroidBloodRaidersBattleship = 556,
        AsteroidBloodRaidersFrigate = 557,
        AsteroidBloodRaidersHauler = 558,
        AsteroidBloodRaidersOfficer = 559,
		AsteroidGuristasBattleship = 560,
        AsteroidGuristasCruiser = 561,
        AsteroidGuristasFrigate = 562,
        AsteroidGuristasHauler = 563,
        AsteroidGuristasOfficer = 564,
        AsteroidSanshasNationBattleship = 565,
        AsteroidSanshasNationCruiser = 566,
        AsteroidSanshasNationFrigate = 567,
        AsteroidSanshasNationHauler = 568,
        AsteroidSanshasNationOfficer = 569,
        AsteroidSerpentisBattleship = 570,
        AsteroidSerpentisCruiser = 571,
        AsteroidSerpentisFrigate = 572,
        AsteroidSerpentisHauler = 573,
        AsteroidSerpentisOfficer = 574,
        AsteroidAngelCartelDestroyer = 575,
        AsteroidAngelCartelBattleCruiser = 576,
        AsteroidBloodRaidersDestroyer = 577,
        AsteroidBloodRaidersBattleCruiser = 578,
		AsteroidGuristasDestroyer = 579,
		AsteroidGuristasBattleCruiser = 580,
        AsteroidSanshasNationDestroyer = 581,
        AsteroidSanshasNationBattleCruiser = 582,
        AsteroidSerpentisDestroyer = 583,
        AsteroidSerpentisBattleCruiser = 584,
		DeadspaceAngelCartelBattleCruiser = 593,
        DeadspaceAngelCartelBattleship = 594,
		DeadspaceAngelCartelCruiser = 595,
		DeadspaceAngelCartelDestroyer = 596,
		DeadspaceAngelCartelFrigate = 597,
		DeadspaceBloodRaidersBattleCruiser = 602,
        DeadspaceBloodRaidersBattleship = 603,
		DeadspaceBloodRaidersCruiser = 604,
		DeadspaceBloodRaidersDestroyer = 605,
		DeadspaceBloodRaidersFrigate = 606,
		DeadspaceGuristasBattleCruiser = 611,
		DeadspaceGuristasBattleship = 612,
		DeadspaceGuristasCruiser = 613,
		DeadspaceGuristasDestroyer = 614,
		DeadspaceGuristasFrigate = 615,
        DeadspaceSanshasNationBattleCruiser = 620,
        DeadspaceSanshasNationBattleship = 621,
        DeadspaceSanshasNationCruiser = 622,
        DeadspaceSanshasNationDestroyer = 623,
        DeadspaceSanshasNationFrigate = 624,
		DeadspaceSerpentisBattleCruiser = 629,
        DeadspaceSerpentisBattleship = 630,
		DeadspaceSerpentisCruiser = 631,
		DeadspaceSerpentisDestroyer = 632,
		DeadspaceSerpentisFrigate = 633,
        AdvancedRocket = 648,
        TractorBeam = 650,
        AdvancedLightMissile = 653,
        AdvancedAssaultMissile = 654,
        AdvancedHeavyMissile = 655,
        AdvancedCruiseMissile = 656,
        AdvancedTorpedo = 657,
        MercoxitMiningCrystal = 663,
        MissionAmarrEmpireFrigate = 665,
        MissionAmarrEmpireBattleCruiser = 666,
        MissionAmarrEmpireBattleship = 667,
        MissionAmarrEmpireCruiser = 668,
        MissionAmarrEmpireDestroyer = 669,
        MissionAmarrEmpireOther = 670,
        MissionCaldariStateFrigate = 671,
        MissionCaldariStateBattleCruiser = 672,
        MissionCaldariStateCruiser = 673,
        MissionCaldariStateBattleship = 674,
        MissionCaldariStateOther = 675,
        MissionCaldariStateDestroyer = 676,
		MissionGallenteFederationFrigate = 677,
		MissionGallenteFederationCruiser = 678,
        MissionGallenteFederationDestroyer = 679,
        MissionGallenteFederationBattleship = 680,
        MissionGallenteFederationBattleCruiser = 681,
        MissionGallenteFederationOther = 682,
        MissionMinmatarRepublicFrigate = 683,
        MissionMinmatarRepublicDestroyer = 684,
        MissionMinmatarRepublicBattleCruiser = 685,
        MissionMinmatarRepublicOther = 686,
        MissionKhanidFrigate = 687,
        MissionKhanidDestroyer = 688,
        MissionKhanidCruiser = 689,
        MissionKhanidBattleCruiser = 690,
        MissionKhanidBattleship = 691,
        MissionKhanidOther = 692,
        MissionCONCORDFrigate = 693,
        MissionCONCORDDestroyer = 694,
        MissionCONCORDCruiser = 695,
        MissionCONCORDBattleCruiser = 696,
        MissionCONCORDBattleship = 697,
        MissionCONCORDOther = 698,
		MissionMorduFrigate = 699,
        MissionMorduDestroyer = 700,
		MissionMorduCruiser = 701,
        MissionMorduBattleCruiser = 702,
		MissionMorduBattleship = 703,
        MissionMorduOther = 704,
        MissionMinmatarRepublicCruiser = 705,
        MissionMinmatarRepublicBattleship = 706,
		AsteroidRogueDroneBattleCruiser = 755,
		AsteroidRogueDroneBattleship = 756,
		AsteroidRogueDroneCruiser = 757,
		AsteroidRogueDroneDestroyer = 758,
		AsteroidRogueDroneFrigate = 759,
        AsteroidRogueDroneHauler = 760,
        AsteroidRogueDroneSwarm = 761,
        MissileLauncher_HeavyAssault = 771,
        AssaultMissile = 772,
		LargeCollidableShip = 784,
        AsteroidAngelCartelCommanderFrigate = 789,
        AsteroidAngelCartelCommanderCruiser = 790,
        AsteroidBloodRaidersCommanderCruiser = 791,
        AsteroidBloodRaidersCommanderFrigate = 792,
        AsteroidAngelCartelCommanderBattleCruiser = 793,
        AsteroidAngelCartelCommanderDestroyer = 794,
        AsteroidBloodRaidersCommanderBattleCruiser = 795,
        AsteroidBloodRaidersCommanderDestroyer = 796,
        AsteroidGuristasCommanderBattleCruiser = 797,
        AsteroidGuristasCommanderCruiser = 798,
        AsteroidGuristasCommanderDestroyer = 799,
        AsteroidGuristasCommanderFrigate = 800,
		DeadspaceRogueDroneBattleCruiser = 801,
        DeadspaceRogueDroneBattleship = 802,
		DeadspaceRogueDroneCruiser = 803,
        DeadspaceRogueDroneDestroyer = 804,
		DeadspaceRogueDroneFrigate = 805,
        AsteroidSanshasNationCommanderBattleCruiser = 807,
        AsteroidSanshasNationCommanderCruiser = 808,
        AsteroidSanshasNationCommanderDestroyer = 809,
        AsteroidSanshasNationCommanderFrigate = 810,
        AsteroidSerpentisCommanderBattleCruiser = 811,
        AsteroidSerpentisCommanderCruiser = 812,
        AsteroidSerpentisCommanderDestroyer = 813,
        AsteroidSerpentisCommanderFrigate = 814,
        MissionGenericBattleships = 816,
		MissionGenericCruisers = 817,
		MissionGenericFrigates = 818,
        DeadspaceOverseerFrigate = 819,
        DeadspaceOverseerCruser = 820,
        DeadspaceOverseerBattleship = 821,
        MissionThukkerBattleCruiser = 822,
        MissionThukkerBattleship = 823,
        MissionThukkerCruiser = 824,
        MissionThukkerDestroyer = 825,
        MissionThukkerFrigate = 826,
        MissionThukkerOther = 827,
        MissionGenericBattleCruisers = 828,
        MissionGenericDestroyers = 829,
        AsteroidRogueDroneCommanderBattleCruiser = 843,
        AsteroidRogueDroneCommanderBattleship = 844,
        AsteroidRogueDroneCommanderCruiser = 845,
        AsteroidRogueDroneCommanderDestroyer = 846,
        AsteroidRogueDroneCommanderFrigate = 847,
        AsteroidAngelCartelCommanderBattleship = 848,
        AsteroidBloodRaidersCommanderBattleship = 849,
        AsteroidGuristasCommanderBattleship = 850,
        AsteroidSanshasNationCommanderBattleship = 851,
        AsteroidSerpentisCommanderBattleship = 852,
        MissileLauncher_Bomb = 862,
        MissionAmarrEmpireCarrier = 865,
        MissionCaldariStateCarrier = 866,
        MissionGallenteFederationCarrier = 867,
        MissionMinmatarRepublicCarrier = 868,
        CapitalIndustrialShip = 883,
        IndustrialCommandShip = 941,
		Orbital_Infrastructure = 1025
    }

	public enum FactionIDs
	{
		Caldari_State = 500001,
		Minmatar_Republic = 500002,
		Amarr_Empire = 500003,
		Gallente_Federation = 500004,
		Jove_Empire = 500005,
		CONCORD_Assembly = 500006,
		Ammatar_Mandate = 500007,
		Khanid_Kingdom = 500008,
		The_Syndicate = 500009,
		Guristas_Pirates = 500010,
		Angel_Cartel = 500011,
		Blood_Raider_Covenant = 500012,
		Interbus = 500013,
		ORE = 500014,
		Thukker_Tribe = 500015,
		Servant_Sisters_of_Eve = 500016,
		The_Society = 500017,
		Mordus_Legion_Command = 500018,
		Sanshas_Nation = 500019,
		Serpentis = 500020,
		Unknown = 500021
	}

    public enum Modes
    {
        Stopped = 2,
        Warp = 3,
		Invulnerable = 11
    }

	public enum BeltSubsetModes
	{
		First = 0,
		Middle,
		Last
	}

    public enum Capacities
    {
        JettisonContainer = 27500,
        OrcaCorpHangarArray = 40000,
		CorporateHangarArray = 1400000
    }

    public enum DroneStates
    {
        Idle = 0,
        Fighting = 1,
        Unknown_State_2 = 2,
        Unknown_State_3 = 3,
        Returning = 4
    }

	public enum MiningMissionTypes
	{
		Ore,
		Ice,
		Gas
	}

    public enum AmmoTypes
    {
        Projectile,
        Frequency,
        Hybrid
    }

    public enum TargetTypes
    {
        /// <summary>
        /// Indicates a target is to be mined.
        /// </summary>
        Mine,
        /// <summary>
        /// Indicates a target is to be looted and, if possible, salvaged.
        /// </summary>
        LootSalvage,
        /// <summary>
        /// Indicates a target is to be killed.
        /// </summary>
        Kill
    }

    public enum TargetPriorities    //Lower is higher priority
    {
        Kill_EnergyNeutralizer = 0,
        Kill_WarpScrambler,
        Kill_Webifier,
		Kill_RemoteSensorDampener,
        Kill_OtherElectronicWarfare,
		Kill_DestructibleSentryGun,
		Kill_BattleCruiser,			//Battlecruiser above BattleShip and Destroyer above Cruiser because
        Kill_BattleShip,			//both do about the same damage, but BattleCruisers are much easier to
		Kill_Destroyer,				//pop than BattleShips and same for Destoryers vs Cruisers, thus should
        Kill_Cruiser,				//be higher priority to faster eliminate incoming DPS
        Kill_Frigate,
        Kill_LargeCollidableObject,
        Kill_Other,
        Wreck_TractorSalvage,
        Mining
    }

	public enum RattingTargetPriorities	//Do battleships last as they're usually triggers
	{
        Kill_EnergyNeutralizer = 0,
		Kill_WarpScrambler,
		Kill_ElectronicWarfare,
		Kill_DestructibleSentryGun,
		Kill_BattleCruiser,						
		Kill_Destroyer,				
		Kill_Cruiser,				
		Kill_Frigate,
		Kill_LargeCollidableObject,
		Kill_Other,
		Kill_BattleShip,
		Wreck_TractorSalvage,
		Mining
	}

    public enum LocationTypes
    {
        Station,            //Unload to station
        CorpHangarArray,    //Unload to a pos CHA  
		StationCorpHangar,	//Unload to a station CHA
        AnchoredContainer,  //Unload to an anchored can
        Jetcan,             //Jettison
        ShipBay             //Rorqual/Orca CHA
    }

	public enum BounceWarpStates
	{
		Idle,
		RemoveTemporaryBookmarks,
		CreateTemporaryBookmark,
		SetTemporaryBookmark,
		QueueDestinations
	}

	public enum StationDropoffStates
	{
		Idle,
		MakeCargoholdActive,
		UnloadCargo,
		StackHangar,
		RearmCrystals
	}

	public enum StationCorpHangarDropoffStates
	{
		Idle,
		MakeCargoholdActive,
		UnloadCargo,
		StackHangar,
		RearmCrystals
	}

	public enum MoveToDropoffStates
	{
		Idle,
		RemoveTemporaryMiningBookmarks,
		CreateTemporaryMiningBookmark,
		SetTemporaryMiningBookmark,
		GoToDropoff
	}

	public enum MissionProcessorStates
	{
		Idle,
		GetMissionRecord,
		WaitForArrival,
		Fleeing,
		ProcessMission,
		Finished
	}

	/*
	public enum PulseSpeeds
	{
        Hyper = 1,
        VeryFast = 2,
        Fast = 3,
		Average = 4,
		Slow = 5,
		VerySlow = 6        
	}
	 */

    public enum BotModes
    {
        Idle,
        Mining,
        Ratting,
        Hauling,
        Scavenging,
        Missioning,
        BoostCanOrca,
        BoostOrca,
        Freighting,
        JumpStabilityTest
    }

    public enum MissionStates
    {
        Offered = 1,
        Accepted = 2
    }

    public enum HaulerModes
    {
        /// <summary>
        /// Only go to pickup a can when we receive request events.
        /// </summary>
        [ProtoEnum(Name = "WaitForRequestEvent", Value = 0)]
        WaitForRequestEvent = 0,
        /// <summary>
        /// Cycle all members in the fleet without waiting for request events.
        /// </summary>
        [ProtoEnum(Name = "CycleFleetMembers", Value = 1)]
        CycleFleetMembers = 1
    }

    public enum FreighterModes
    {
        PointToPoint,
        AssetConsolidation
    }

	public enum PrimaryWeaponPlatforms
	{
		Drones,
		Missiles,
		Turrets
	}

    public enum ShipRoles
    {
        Combat,
        IceMining,
        OreMining,
        Salvaging,
        Hauling
    }

    [Flags]
    public enum DamageTypes
    {
        None = 0,
        Kinetic = 1,
        Thermal = 2,
        Explosive = 4,
        EM = 8,
        All = Kinetic | Thermal | Explosive | EM
    }
}
