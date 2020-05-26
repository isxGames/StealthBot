using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StealthBot.Core.CustomEventArgs
{
	public class WalletStatisticsUpdatedEventArgs : EventArgs
	{
		public double AverageIskPerHour, IskDeltaThisSession;

		public WalletStatisticsUpdatedEventArgs(double averageIskPerHour, double iskDeltaThisSession)
		{
			AverageIskPerHour = averageIskPerHour;
			IskDeltaThisSession = iskDeltaThisSession;
		}
	}
}
