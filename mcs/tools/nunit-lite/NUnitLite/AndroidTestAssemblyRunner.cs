
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

		private string app;

		private TcpListener listener;

		private Process adb;

		private Stream stream;

		public AndroidTestAssemblyRunner (string app)
		{
			this.app = app;
		}

		public ITest LoadedTest
		{
			get { throw new NotImplementedException (); }
		}

		public bool Load(string assemblyName, IDictionary settings)
		{
			listener = new TcpListener (IPAddress.Parse (ADDRESS), PORT);
			listener.Start ();

			{
				ProcessStartInfo psi = new ProcessStartInfo ("adb", $"reverse tcp:{PORT} tcp:{PORT}");

				Console.WriteLine ($"Execute \"{psi.FileName} {psi.Arguments}\"");
				using (Process p = Process.Start (psi)) {
					p.WaitForExit();
				}
			}

			{
				ProcessStartInfo psi = new ProcessStartInfo ("adb", $"shell am instrument -w -e testsuite \"{Path.GetFileName (assemblyName)}\" \"{app}\"") {
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
				};

				Console.WriteLine ($"Execute \"{psi.FileName} {psi.Arguments}\"");
				adb = Process.Start (psi);
				adb.OutputDataReceived += (s, e) => { if (e.Data != null) Console.Out.Write (e.Data + "\n"); };
				adb.ErrorDataReceived += (s, e) => { if (e.Data != null) Console.Error.Write (e.Data + "\n"); };
				adb.BeginOutputReadLine ();
				adb.BeginErrorReadLine ();
			}

			stream = listener.AcceptTcpClient().GetStream ();

			return true;
		}

		public bool Load(Assembly assembly, IDictionary settings)
		{
			return Load(assembly.GetName().Name + ".dll", settings);
		}

		public ITestResult Run(ITestListener listener, ITestFilter filter)
		{
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
					throw new Exception ($"unknown message {message}");
				}
			}

			adb.WaitForExit ();
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
}
