using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using LavishScriptAPI;
using LavishVMAPI;
using System.Text;
using InnerSpaceAPI;
using StealthBot.Core;

[assembly: System.Security.SecurityRules(System.Security.SecurityRuleSet.Level2)]

namespace StealthBot
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
		[STAThread]
		static void Main(string[] args)
        {
        	Application.EnableVisualStyles();
        	Application.SetCompatibleTextRenderingDefault(false);

        	var loader = new Loader(Application.ProductVersion, args);
        	loader.Load();

        	if (loader.LoadedSuccessfully)
        	{
        		InnerSpace.Echo("Executing StealthBot...");
        		Application.Run(new StealthBotForm(args));
        		InnerSpace.Echo("StealthBot exiting.");
        	}
        	else if (loader.LoadErrorMessage != null)
        	{
				LavishScript.ExecuteCommand("stealthbotLoaded:Set[TRUE]");
        		MessageBox.Show(loader.LoadErrorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        	}
        }
    }
}
