
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework.Api;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Filters;

namespace Xamarin
{
	class AndroidTestAssemblyRunner : ITestAssemblyRunner
	{
		const string ADDRESS = "127.0.0.1";

		const int PORT = 6100;

		private TcpListener listener;

		private Process adb;

		private Stream stream;

		public AndroidTestAssemblyRunner (string app)
		{
			AndroidRemoteRunner.App = app;
		}

		public ITest LoadedTest
		{
			get { throw new NotImplementedException (); }
		}

		public bool Load(string assemblyName, IDictionary settings)
		{
			listener = new TcpListener (IPAddress.Parse (ADDRESS), PORT);
			listener.Start ();

			adb = AndroidRemoteRunner.Run (Path.GetFileName (assemblyName), port: PORT);

			stream = listener.AcceptTcpClient().GetStream ();

			return true;
		}

		public bool Load(Assembly assembly, IDictionary settings)
		{
			return Load(assembly.GetName().Name + ".dll", settings);
		}

		public ITestResult Run(ITestListener listener, ITestFilter filter)
		{
			try {
				IFormatter formatter = new BinaryFormatter();

				formatter.Serialize (stream, filter);

				while (true)
				{
					object message = formatter.Deserialize (stream);
					if (message is TestStartedMessage tsmessage) {
						listener.TestStarted (tsmessage.Test);
					} else if (message is TestFinishedMessage tfmessage) {
						listener.TestFinished (tfmessage.TestResult);
					} else if (message is ExceptionMessage emessage) {
						throw emessage.Exception;
					} else if (message is ResultMessage rmessage) {
						return rmessage.TestResult;
					} else {
						adb.Kill ();
						throw new Exception ($"unknown message {message}");
					}
				}
			} finally {
				adb.WaitForExit ();
			}
		}

		class Driver : ITestListener
		{
			public static void RunTests (string assemblyName)
			{
				Driver driver = new Driver ();
				driver.Execute (Assembly.LoadFrom(assemblyName));
			}

			private ITestAssemblyRunner runner;

			private FinallyDelegate finallyDelegate = new FinallyDelegate();

			private IFormatter formatter = new BinaryFormatter();

			private Stream stream;

			public Driver()
			{
				runner = new NUnitLiteTestAssemblyRunner(new NUnitLiteTestAssemblyBuilder(), finallyDelegate);

				stream = new TcpClient(ADDRESS, PORT).GetStream ();
			}

			public void Execute(Assembly assembly)
			{
				try {
					Randomizer.InitialSeed = 0;

					if (!runner.Load(assembly, new Dictionary<string, string>()))
					{
						throw new Exception($"No tests found in assembly {AssemblyHelper.GetAssemblyName(assembly).Name}");
					}

					ITestFilter filter = (ITestFilter) formatter.Deserialize (stream);

					AppDomain.CurrentDomain.UnhandledException += TopLevelHandler;

					ITestResult result = (ITestResult) runner.Run(this, filter);
					formatter.Serialize (stream, new Xamarin.ResultMessage { TestResult = result });

					AppDomain.CurrentDomain.UnhandledException -= TopLevelHandler;
				}
				catch (Exception ex)
				{
					formatter.Serialize (stream, new Xamarin.ExceptionMessage { Exception = ex });
					throw;
				}
			}

			void TopLevelHandler(object sender, UnhandledExceptionEventArgs e)
			{
				// Make sure that the test harness knows this exception was thrown
				this.finallyDelegate.HandleUnhandledExc(e.ExceptionObject as Exception);
			}

			public void TestStarted(ITest test)
			{
				formatter.Serialize (stream, new Xamarin.TestStartedMessage { Test = new Xamarin.Test (test) });
			}

			public void TestFinished(ITestResult result)
			{
				formatter.Serialize (stream, new Xamarin.TestFinishedMessage { TestResult = new Xamarin.TestResult (result) });
			}

			public void TestOutput(TestOutput testOutput)
			{
			}
		}
	}

	public static class AndroidRemoteRunner
	{
		internal static string App;

		public static Process Run (string testSuite, int port = 6100)
		{
			if (App == null)
				throw new InvalidOperationException ($"{nameof(App)} needs to be set before calling this method");

			ProcessStartInfo psi1 = new ProcessStartInfo ("adb", $"reverse tcp:{port} tcp:{port}");

			Console.WriteLine ($"Execute \"{psi1.FileName} {psi1.Arguments}\"");
			using (Process p = Process.Start (psi1)) {
				p.WaitForExit();
			}

			bool waitForLLDB = Environment.GetEnvironmentVariable("MONO_WAIT_LLDB") != null;

			ProcessStartInfo psi2 = new ProcessStartInfo ("adb", $"shell am instrument -w -e TestSuite \"{testSuite}\" -e WaitForLLDB \"{waitForLLDB}\" \"{App}\"") {
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
			};

			Console.WriteLine ($"Execute \"{psi2.FileName} {psi2.Arguments}\"");
			Process adb = Process.Start (psi2);
			adb.OutputDataReceived += (s, e) => { if (e.Data != null) Console.Out.Write (e.Data + "\n"); };
			adb.ErrorDataReceived += (s, e) => { if (e.Data != null) Console.Error.Write (e.Data + "\n"); };
			adb.BeginOutputReadLine ();
			adb.BeginErrorReadLine ();

			return adb;
		}
	}
}
