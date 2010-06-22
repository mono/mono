using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace WCFServers
{
	class Program
	{
		List<object> initialized = new List<object> ();

		object getInstance (Type type) {
			try {
				return Activator.CreateInstance (type);
			}
			catch (Exception e) {
				Console.WriteLine ("Failed to initialize object. Skipping");
				return null;
			} finally {
				Console.Write ("Initialized....");
			}
		}
		void runSetUp (object t) {
			Console.Write ("Attempting to start type " + t.GetType().FullName + " ....");
			var methods = from method in t.GetType ().GetMethods (BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
						  where method.GetCustomAttributes (typeof (NUnit.Framework.SetUpAttribute), true).Length > 0
						  select method;
			if (methods.Count () == 0) {
				Console.WriteLine ("No SetUp methods. Skipping");
				return;
			}
			bool good = false;
			foreach (var method in methods)
				try {
					method.Invoke (t, null);
					good = true;
				}
				catch (Exception e) {
					Console.Write ("Failed to call method " + method.Name);
				}
			if (good) {
				Console.WriteLine ("Success.");
				initialized.Add (t);
			}
			else {
				Console.WriteLine ("No setup methods successfully called. Skipping");
			}
		}
		void runInit (Type type) {
			object t = getInstance (type);
			if (t == null)
				return;
			runSetUp (t);
		}
		void runAllInits (Assembly assem) {
			var types = from type in assem.GetTypes ()
						where type.GetCustomAttributes (typeof (NUnit.Framework.TestFixtureAttribute), true).Length > 0
						select type;
			foreach (Type type in types) {
				runInit (type);
			}
			Console.WriteLine ("Successfully initialized " + initialized.Count + " types: ");
			{
				foreach (object o in initialized)
					Console.WriteLine (o.GetType ().FullName);
			}
		}

		static void Main (string [] args) {

			if (args.Count() > 0 && args [0] == "shutdown") {
				MonoTests.Features.Configuration.onlyClients = true;
				MonoTests.Features.Serialization.ExitProcessHelper exiter = new MonoTests.Features.Serialization.ExitProcessHelper ();
				exiter.Run ();                 // initialize the client
				exiter.Client.ExitProcess (0); // exit the server
				Environment.Exit (0);          // exit this process
			}
				
			string assemblyName = "System.ServiceModel_test_net_3_0.dll";
			Assembly assem;
			try {
				System.IO.FileInfo fi = new FileInfo (Assembly.GetEntryAssembly ().Location);

				assem = Assembly.LoadFrom (Path.Combine (fi.Directory.FullName, assemblyName));
			}
			catch (Exception e) {
				Console.WriteLine ("Could not start server. Could not load: " + assemblyName);
				return;
			}

			// Run only the servers. No need to initialize the clients.
			MonoTests.Features.Configuration.onlyServers = true;

			Program p = new Program ();
			p.runAllInits (assem);

			Console.WriteLine ("Press any key to continue...");
			Console.ReadKey ();
			Console.WriteLine ("Bye bye");
			Environment.Exit (0);
		}
	}
}
