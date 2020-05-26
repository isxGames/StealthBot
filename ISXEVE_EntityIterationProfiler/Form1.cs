using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using EVE.ISXEVE;
using LavishScriptAPI;
using LavishVMAPI;

namespace ISXEVE_EntityIterationProfiler
{
	public partial class Form1 : Form
	{
		public EventHandler<LSEventArgs> OnFrame;
		public uint PulseFrequecy = 2000;
		public DateTime NextPulse = DateTime.Now;
		public bool IsEnabled = false;

		Stopwatch timer = new Stopwatch();
		Stopwatch timer2 = new Stopwatch();
		List<CachedEntity> entities = new List<CachedEntity>();
		List<string> messages = new List<string>();
		string methodName = string.Empty;
		bool printed = false, printRepeated = false;

		public Form1()
		{
			InitializeComponent();
			OnFrame = new EventHandler<LSEventArgs>(_onFrame);
			LavishScript.Events.AttachEventTarget("ISXEVE_onFrame", OnFrame);
		}

		~Form1()
		{
			LavishScript.Events.DetachEventTarget("ISXEVE_onFrame", OnFrame);
		}

		private void buttonIterateEntities_Click(object sender, EventArgs e)
		{
			IsEnabled = true;
		}

		private void _onFrame(object sender, LSEventArgs e)
		{
			if (IsEnabled && DateTime.Now.CompareTo(NextPulse) >= 0)
			{
				NextPulse = DateTime.Now.AddMilliseconds(PulseFrequecy);
				EVE.ISXEVE.EVE eve = new EVE.ISXEVE.EVE();

				timer.Start();
				entities = eve.GetCachedEntities();
				timer.Stop();
				messages.Add(String.Format("Getting {0} entities took: {1}.", entities.Count, timer.Elapsed));
				timer.Reset();

				//int id = 0;
				//double distance = 0;
				timer2.Start();
				foreach (CachedEntity en in entities)
				{
					//timer.Start();
					//id = en.ID;
					//distance = en.Distance;
					//timer.Stop();
					//messages.Add(String.Format("Getting ID {0} took: {1} ms", id, timer.ElapsedMilliseconds));
					//messages.Add(String.Format("Getting distance {0} took: {1} ms", en.Distance, timer.ElapsedMilliseconds));
					//messages.Add(String.Format("Getting name {0} took: {1} ms", en.Name, timer.ElapsedMilliseconds));
					//timer.Reset();

					string Name;
					int GroupID, TypeID, CategoryID;
					Int64 ID;
					double ShieldPct, ArmorPct, StructurePct, Distance;
					bool BeingTargeted, IsLockedTarget, IsNPC, IsPC, IsTargetingMe;

					StartMethodProfiling("CE_Name");
					Name = en.Name;
					EndMethodProfiling();
					StartMethodProfiling("CE_ID");
					ID = en.ID;
					EndMethodProfiling();
					StartMethodProfiling("CE_GroupID");
					GroupID = en.GroupID;
					EndMethodProfiling();
					StartMethodProfiling("CE_TypeID");
					TypeID = en.TypeID;
					EndMethodProfiling();
					StartMethodProfiling("CE_ShieldPct");
					ShieldPct = en.ShieldPct;
					EndMethodProfiling();
					StartMethodProfiling("CE_ArmorPct");
					ArmorPct = en.ArmorPct;
					EndMethodProfiling();
					StartMethodProfiling("CE_StructurePct");
					StructurePct = en.StructurePct;
					EndMethodProfiling();
					StartMethodProfiling("CE_Distance");
					Distance = en.Distance;
					EndMethodProfiling();
					StartMethodProfiling("CE_BeingTargeted");
					BeingTargeted = en.BeingTargeted;
					EndMethodProfiling();
					//CanLoot = en.CanLoot;
					StartMethodProfiling("CE_CategoryID");
					CategoryID = en.CategoryID;
					EndMethodProfiling();
					StartMethodProfiling("CE_IsLockedTarget");
					IsLockedTarget = en.IsLockedTarget;
					EndMethodProfiling();
					StartMethodProfiling("CE_IsNPC");
					IsNPC = en.IsNPC;
					EndMethodProfiling();
					StartMethodProfiling("CE_IsPC");
					IsPC = en.IsPC;
					EndMethodProfiling();
					StartMethodProfiling("CE_IsTargetingMe");
					IsTargetingMe = en.IsTargetingMe;
					EndMethodProfiling();

					printed = true;

					if (en.GroupID == 12)
					{
						messages.Add("Found CachedEntity of groupID 12:");
						messages.Add(String.Format("Name: {0}, ID: {1}", en.Name, en.ID));
					}
				}
				timer2.Stop();
				messages.Add(String.Format("Iteration of {0} entities took {1}.", entities.Count, timer2.Elapsed));
				timer2.Reset();

				foreach (string s in messages)
				{
					InnerSpaceAPI.InnerSpace.Echo(String.Format("ISXEVE_EntityIterationProfiler: {0}", s));
				}
				messages.Clear();
				//IsEnabled = false;
				printed = false;
			}
		}

		private void StartMethodProfiling(string methodName)
		{
			timer.Start();
			this.methodName = methodName;
		}

		private void EndMethodProfiling()
		{
			timer.Stop();
			if (!printed || printRepeated)
			{
				//messages.Add(String.Format("Attribute {0} took {1}.", methodName.PadRight(30), timer.Elapsed));
			}
			timer.Reset();
		}
	}
}
