//
// BabysitterSupport.cs: Nunit extensions to support test harness used by Mono.
//                       See scripts/babysitter in Mono repository.
//
// Author:
//   Andi McClure (andi.mcclure@xamarin.com)
//
// Copyright (C) 2015 Xamarin, Inc (http://www.xamarin.com)
//

namespace Xamarin
{
	using System;
	using System.IO;
	using System.Collections.Generic;
	using NUnit.Framework.Api;
	using NUnit.Framework.Internal;
	using NUnit.Framework.Internal.Filters;

	public class BabysitterSupport
	{
		enum OverrideMode {None, Run, Exclude};
		public static Dictionary<string, bool> OverrideTests = new Dictionary<string, bool>();
		private static OverrideMode Override = OverrideMode.None;
		private static string CurrentTestFile = null, RanTestFile = null, FailedTestFile = null;

		private static void DeleteFile(string path)
		{
			try {
				File.Delete(path);
			} catch (Exception) {}
		}
		private static void WriteFile(string path, string contents)
		{
			DeleteFile(path);
			File.AppendAllText(path, contents);
		}

		// Environment variables are available from process start, so safe to do setup in a static constructor
		static BabysitterSupport()
		{
			string overrideModeString = Environment.GetEnvironmentVariable("MONO_BABYSITTER_NUNIT_RUN_MODE");
			string overrideTestString = Environment.GetEnvironmentVariable("MONO_BABYSITTER_NUNIT_RUN_TEST");
			if (overrideModeString == "RUN")
				Override = OverrideMode.Run;
			else if (overrideModeString == "EXCLUDE")
				Override = OverrideMode.Exclude;
			if (Override != OverrideMode.None)
			{
				string[] overrideTests = overrideTestString.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries);
				foreach (string s in overrideTests)
					OverrideTests[s] = true;
			}

			CurrentTestFile = Environment.GetEnvironmentVariable("MONO_BABYSITTER_NUNIT_CURRENT_TEST_FILE");
			RanTestFile = Environment.GetEnvironmentVariable("MONO_BABYSITTER_NUNIT_RAN_TEST_FILE");
			FailedTestFile = Environment.GetEnvironmentVariable("MONO_BABYSITTER_NUNIT_FAILED_TEST_FILE");
		}

		// Entry points

		public static void RecordEnterTest( string testName )
		{
			if (CurrentTestFile != null)
				WriteFile(CurrentTestFile, testName);
			if (RanTestFile != null)
				File.AppendAllText(RanTestFile, testName + Environment.NewLine);
		}

		public static void RecordLeaveTest( string testName )
		{
			if (CurrentTestFile != null)
				DeleteFile(CurrentTestFile);
		}

		public static void RecordFailedTest( string testName )
		{
			if (FailedTestFile != null)
				File.AppendAllText(FailedTestFile, testName + Environment.NewLine);
		}

		public static ITestFilter AddBabysitterFilter(ITestFilter currentFilter)
		{
			if (Override == OverrideMode.None)
				return currentFilter;
			return new AndFilter(currentFilter, new BabysitterFilter());
		}

		[Serializable]
		private class BabysitterFilter : TestFilter
		{
			public override bool Match(ITest test)
			{
				if (test.IsSuite) // A suite returning true will automatically run ALL contents, filters ignored
					return false;
				bool inList = OverrideTests.ContainsKey(test.FullName);
				bool allow = true;
				switch (Override)
				{
					case OverrideMode.None:
						break;
					case OverrideMode.Run:
						allow = inList;
						break;
					case OverrideMode.Exclude:
						allow = !inList;
						break;
				}
				return allow;
			}
		}
	}
}