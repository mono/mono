namespace NUnit.Runner 
{
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.IO;
	using System.IO.IsolatedStorage;
	using System.Reflection;
	using NUnit.Framework;

	/// <summary>
	/// Base class for all test runners.
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	public abstract class BaseTestRunner: MarshalByRefObject, ITestListener
	{
		/// <summary>
		/// 
		/// </summary>
		[Obsolete("Shoud be handled by a loader")]
		public static string SUITE_PROPERTYNAME="Suite";

		private static NameValueCollection fPreferences = new NameValueCollection();
		private static int fgMaxMessageLength = 500;
		private  static bool fgFilterStack = true;

		private bool fLoading = true;
		/// <summary>
		/// 
		/// </summary>
		public BaseTestRunner() 
		{
			fPreferences = new NameValueCollection();
			fPreferences.Add("loading", "true");
			fPreferences.Add("filterstack", "true");
			ReadPreferences();
			fgMaxMessageLength = GetPreference("maxmessage", fgMaxMessageLength);
		}
		
		#region ITestListener Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="test"></param>
		/// <param name="t"></param>
		public abstract void AddError(ITest test, Exception t);
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="test"></param>
		/// <param name="t"></param>
		public abstract void AddFailure(ITest test, AssertionFailedError t);
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="test"></param>
		public abstract void EndTest(ITest test);
		#endregion

#if false
		/// <summary>
		/// Clears the status message.
		/// </summary>
		protected virtual void ClearStatus() 
		{
			// Belongs in the GUI TestRunner class.
		}
#endif		
		/// <summary>
		/// Returns the formatted string of the elapsed time.
		/// </summary>
		public static string ElapsedTimeAsString(long runTime) 
		{
			return ((double)runTime/1000).ToString();
		}
		/// <summary>
		/// Extract the class name from a string in VA/Java style
		/// </summary>
		public static string ExtractClassName(string className) 
		{
			if(className.StartsWith("Default package for")) 
				return className.Substring(className.LastIndexOf(".")+1);
			return className;
		}
    
		static bool FilterLine(string line) 
		{
			string[] patterns = new string[]
				{
					"NUnit.Framework.TestCase",
					"NUnit.Framework.TestResult",
					"NUnit.Framework.TestSuite",
					"NUnit.Framework.Assertion." // don't filter AssertionFailure
				};
			for (int i = 0; i < patterns.Length; i++) 
			{
				if (line.IndexOf(patterns[i]) > 0)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Filters stack frames from internal NUnit classes
		/// </summary>
		public static string FilterStack(string stack) 
		{
			string pref = GetPreference("filterstack");
			if (((pref != null) && !GetPreference("filterstack").Equals("true"))
				|| fgFilterStack == false)
				return stack;

			StringWriter sw = new StringWriter();
			StringReader sr = new StringReader(stack);

			try 
			{
				string line;
				while ((line = sr.ReadLine()) != null) 
				{
					if (!FilterLine(line))
						sw.WriteLine(line);
				}
			} 
			catch (Exception) 
			{
				return stack; // return the stack unfiltered
			}
			return sw.ToString();
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public static string GetFilteredTrace(Exception t) 
		{
			return BaseTestRunner.FilterStack(t.StackTrace);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static string GetPreference(string key) 
		{
			return fPreferences.Get(key);
		}

		private static int GetPreference(String key, int dflt) 
		{
			String value= GetPreference(key);
			int intValue= dflt;
			if (value != null)
			{
				try 
				{
					intValue= int.Parse(value);
				} 
				catch (FormatException) {}
			}
			return intValue;
		}

		private static FileStream GetPreferencesFile() 
		{
			return new IsolatedStorageFileStream("NUnit.Prefs", FileMode.OpenOrCreate);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public virtual ITestLoader GetLoader() 
		{
			if (UseReloadingTestSuiteLoader())
				return new UnloadingLoader();
			return new StandardLoader();
		}

		/// <summary>
		/// Returns the ITest corresponding to the given suite. This is
		/// a template method, subclasses override RunFailed(), ClearStatus().
		/// </summary>
		public ITest GetTest(string suiteClassName) 
		{
			ITest test = null;
			try 
			{
				test = LoadSuiteClass(suiteClassName);
			} 
			catch (TypeLoadException e) 
			{
				RunFailed(e.Message);
				return null;
			} 
			catch (Exception e) 
			{
				RunFailed("Error: " + e.ToString());
				return null;
			}
			//ClearStatus();
			return test;  
		}

		/// <summary>
		/// Returns the loaded Class for a suite name. 
		/// </summary>
		protected ITest LoadSuiteClass(string suiteClassName) 
		{
			return GetLoader().LoadTest(suiteClassName);
		}
		
		private static void ReadPreferences() 
		{
			FileStream fs= null;
			try 
			{
				fs= GetPreferencesFile();
				fPreferences= new NameValueCollection(fPreferences);
				ReadPrefsFromFile(ref fPreferences, fs);
			} 
			catch (IOException) 
			{
				try 
				{
					if (fs != null)
						fs.Close();
				} 
				catch (IOException) 
				{
				}
			}
		}
		
		/// <summary>
		/// Real method reads name/value pairs, populates, or maybe just
		/// deserializes...
		/// </summary>
		/// <param name="prefs"></param>
		/// <param name="fs"></param>
		private static void ReadPrefsFromFile(ref NameValueCollection prefs, FileStream fs) 
		{
		}
		
		/// <summary>
		/// Override to define how to handle a failed loading of a test suite.
		/// </summary>
		protected abstract void RunFailed(string message);
		
		/// <summary>
		/// Truncates a String to the maximum length.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public static string Truncate(string message) 
		{
			if (fgMaxMessageLength != -1 && message.Length > fgMaxMessageLength)
				message = message.Substring(0, fgMaxMessageLength)+"...";
			return message;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="test"></param>
		public abstract void StartTest(ITest test);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		/// <param name="wait"></param>
		/// <returns></returns>
		protected string ProcessArguments(string[] args, ref bool wait) 
		{
			string suiteName="";
			wait = false;
			foreach (string arg in args) 
			{
				if (arg.Equals("/noloading"))
					SetLoading(false);
				else if (arg.Equals("/nofilterstack")) 
					fgFilterStack = false;
				else if (arg.Equals("/wait")) 
					wait = true;
				else if (arg.Equals("/c"))
					suiteName= ExtractClassName(arg);
				else if (arg.Equals("/v"))
				{
					Console.Error.WriteLine("NUnit "+NUnit.Runner.Version.id()
						+ " by Philip Craig");
					Console.Error.WriteLine("ported from JUnit 3.6 by Kent Beck"
						+ " and Erich Gamma");
				} 
				else
					suiteName = arg;
			}
			return suiteName;
		}
		
		/// <summary>
		/// Sets the loading behaviour of the test runner
		/// </summary>
		protected void SetLoading(bool enable) 
		{
			fLoading = enable;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected bool UseReloadingTestSuiteLoader() 
		{
			return bool.TrueString.Equals( GetPreference("loading")) && fLoading;
		}
	}
}
