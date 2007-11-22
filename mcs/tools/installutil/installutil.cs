//
// Authors:
//   Miguel de Icaza (miguel@novell.com)
//
// (C) 2007 Novell, Inc
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Configuration.Install;

public class InstallUtil {

	static bool showcallstack = false;
	static bool logtoconsole = false;
	static string assembly = null;
	
	static void ShowHelpForAssembly (string assembly)
	{
		Console.WriteLine ("Help for assembly not implemented");
	}
	
	static void ShowHelp ()
	{
		Console.WriteLine ("installutil -- Installs Assemblies that use System.Configuration.Install");
		Console.WriteLine ("Usage is: installutil commands\n");
		Console.WriteLine ("\n" + 
				   "   /help            Shows help\n" +
				   "   /help ASSEM      Shows help for the given assembly\n" +
				   "   /logfile[=out]   Specifies a log file\n" +
				   "   /uninstall ASSEM Uninstall the given assembly\n");
	}

	static void Call (Installer instance, string method, object arg)
	{
		Console.WriteLine ("M: " + method);
		MethodInfo mi = typeof (Installer).GetMethod (method, BindingFlags.Public|BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Static);
		mi.Invoke (instance, new object [] { arg });
	}

	static void Error (string st)
	{
		Console.Error.WriteLine (st);
	}
	
	static void Perform (bool install, string executable)
	{
		ArrayList order = new ArrayList ();
		Hashtable states = new Hashtable ();
		
		try {
			Assembly a;

			if (assembly != null)
				a = Assembly.Load (assembly);
			else
				a = Assembly.LoadFrom (executable);
			
			Type [] types = a.GetTypes ();

			// todo: pass arguments, they are kind of useless though.
			InstallContext ctx = new InstallContext ();
			
			foreach (Type t in types){
				if (!t.IsSubclassOf (typeof (Installer)))
					continue;

				object [] attrs = t.GetCustomAttributes (typeof (RunInstallerAttribute), false);
				if (attrs == null || attrs.Length == 0)
					continue;

				RunInstallerAttribute ria = attrs [0] as RunInstallerAttribute;
				if (ria == null || !ria.RunInstaller)
					continue;

				try {
					Installer installer = (Installer) Activator.CreateInstance (t);
					Hashtable state = new Hashtable ();
					
					order.Add (installer);
					states [installer] = state;
					
					if (install)
						Call (installer, "OnBeforeInstall", state);
					else
						Call (installer, "OnBeforeUninstall", state);
					
					installer.Install (state);

					if (install)
						Call (installer, "OnAfterInstall", state);
					else
						Call (installer, "OnAfterUninstall", state);
					
				} catch (Exception e) {
					Error (String.Format ("Can not create installer of type {0}", t));
					
					//
					// According to the docs uninstall should not do rollback
					//
					if (install){
						foreach (Installer installer in order){
							Hashtable state = (Hashtable) states [installer];
								
							Call (installer, "OnBeforeRollback", state);
							installer.Rollback (state);
							Call (installer, "OnAfterRollback", state);
						}
					}
				}
			}
			//
			// Got it, now commit them
			//
			if (install){
				foreach (Installer inst in order){
					Hashtable state = (Hashtable) states [inst];
					
					Call (inst, "OnCommitting", state);
					inst.Commit (state);
					Call (inst, "OnCommitted", state);
				}
			}
		} catch {
			Error (String.Format ("Unable to load assembly {0}", assembly));
		}		
	}

	static void Install (string assembly)
	{
		Perform (true, assembly);
	}
	
	static void Uninstall (string assembly)
	{
		Perform (false, assembly);
	}
	
	static int Main (string [] args)
	{
		bool did_something = false;
		string logfile = null;
		
		for (int i = 0; i < args.Length; i++){
			string arg = args [i];
			char c = arg [0];

			if (c == '/' || c == '-'){
				switch (arg.ToLower ()){
				case "/help": case "/h": case "/?": 
				case "-help": case "-h": case "-?": 
					if (i + 1 < args.Length){
						i++;
						ShowHelpForAssembly (args [i]);
					} else {
						ShowHelp ();
						return 1;
					}
					break;

				case "-showcallstack":
				case "/showcallstack":
					showcallstack = true;
					break;

				case "-logtoconsole":
				case "/logtoconsole":
					logtoconsole = true;
					break;

				case "-u": case "-uninstall":
				case "/u": case "/uninstall":
					if (i + 1 < args.Length){
						i++;
						Uninstall (args [i]);
						did_something = true;
					} else {
						ShowHelp ();
						return 1;
					}
					break;

				case "-assemblyname":
				case "/assemblyname":
					if (i + 1 < args.Length){
						i++;
						assembly = args [i];
						Install ("");
					} else {
						ShowHelp ();
						return 1;
					}
					break;
					
				default:
					ShowHelp ();
					return 1;
				}
			} else {
				did_something = true;
				Install (args [i]);
			}
		}
		if (!did_something){
			ShowHelp ();
			return 1;
		}

		return 0;
	}
}