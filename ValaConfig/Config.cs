using System;
using System.Globalization;
using System.Threading;

namespace ValaConfig
{
	public static class Config
	{
		private static void SetCCulture()
		{
			// Clone the current Culture, and alter it as needed
			CultureInfo CCulture = Thread.CurrentThread.CurrentCulture.Clone() as CultureInfo;
			CCulture.NumberFormat.NumberDecimalSeparator = ".";
			CCulture.NumberFormat.NaNSymbol = "NaN";
			CCulture.NumberFormat.PositiveInfinitySymbol = "infin";
			CCulture.NumberFormat.NegativeInfinitySymbol = "-infin";

			// Change the current thread culture
			Thread.CurrentThread.CurrentCulture = CCulture;

			// Set the new culture for all new threads
			CultureInfo.DefaultThreadCurrentCulture = CCulture;
		}

		static Config() {
			SetCCulture();
		}

		public static string PACKAGE_SUFFIX { get; set; } = "";
		public static string PACKAGE_DATADIR { get; set; } = "";
		public static string BUILD_VERSION { get; set; } = "0.37.91";
		public static string API_VERSION { get; set; } = "";
	}
}
