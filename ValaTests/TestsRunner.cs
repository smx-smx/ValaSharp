using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ValaTests
{
	public static class TestsRunner
	{
		public static string ToolchainPath { get; private set; }

		[ClassInitialize]
		public static void InitializeEnvironment(TestContext ctx)
		{
			ToolchainPath = ctx.Properties["toolchain"].ToString();
		}
	}
}
