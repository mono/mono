// AssemblyInstaller.cs
//   System.Configuration.Install.AssemblyInstaller class implementation
//
// Author:
//    Muthu Kannan (t.manki@gmail.com)
//
// (C) 2005 Novell, Inc.  http://www.novell.com/
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
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Soap;
using System.IO;
using System.Text;

namespace System.Configuration.Install
{

public class AssemblyInstaller : Installer {
	private string [] cmdLine;
	private Assembly assembly;
	private bool useNewContext;
	private const string STATE_FILE_EXT = ".InstallState";

	// Initialises private members
	private void initFields ()
	{
		UseNewContext = true;
	}

	// Constructors
	public AssemblyInstaller ()
	{
		Assembly = null;
		CommandLine = null;

		initFields();
	}

	public AssemblyInstaller (Assembly ass, string [] args)
	{
		Assembly = ass;
		CommandLine = args;

		initFields();
	}

	public AssemblyInstaller (string assFile, string [] args)
	{
		Path = assFile;
		CommandLine = args;

		initFields();
	}

	// Properties
	public Assembly Assembly {
		get {
			return assembly;
		}
		set {
			assembly = value;
		}
	}

	public string [] CommandLine {
		get {
			return cmdLine;
		}
		set {
			cmdLine = value;
		}
	}

	public override string HelpText {
		get {
			addSubInstallers ();
			StringBuilder sb = new StringBuilder ();
			foreach (Installer ins in Installers)
				sb.Append (ins.HelpText + "\n");
			return sb.ToString ();
		}
	}

	public string Path {
		get {
			if (Assembly == null)
				return null;
			else
				return Assembly.CodeBase.Substring (7);	// remove leading 'file://'
		}
		set {
			Assembly = Assembly.LoadFrom (value);
		}
	}

	public bool UseNewContext {
		get {
			return useNewContext;
		}
		set {
			if (value)
				Context = new InstallContext (Path + ".InstallLog", null);
			else
				Context = new InstallContext (null, CommandLine);
			useNewContext = value;
		}
	}

	// Methods
	public static void CheckIfInstallable (string assemblyName)
	{
		if (! File.Exists (assemblyName))
			throw new Exception ("The path specified (" + assemblyName + ") does not exist.");

		Assembly ass = Assembly.LoadFrom (assemblyName);
		if ((getInstallersFromAssembly (ass)).Length == 0)
			throw new Exception ("Could not find any type with RunInstaller attribute set to True");
	}

	private void addSubInstallers ()
	{
		// Return, if installers have already been filled
		if (Installers.Count > 0)
			return;

		foreach (Type t in getInstallersFromAssembly (Assembly)) {
			Installer i = (Installer) Activator.CreateInstance (t);
			Installers.Add (i);
		}
	}

	private static Type [] getInstallersFromAssembly (Assembly ass)
	{
		ArrayList insTypes = new ArrayList ();

		foreach (Type t in ass.GetExportedTypes ()) {
			object [] attribs = t.GetCustomAttributes (typeof (RunInstallerAttribute), false);
			if (attribs.Length > 0) {
				RunInstallerAttribute atr = (RunInstallerAttribute) attribs [0];
				if ((typeof (Installer)).IsAssignableFrom (t) && atr.Equals (RunInstallerAttribute.Yes))
					insTypes.Add (t);
			}
		}

		Type [] ret = new Type [insTypes.Count];
		for (int i = 0; i < insTypes.Count; ++i)
			ret [i] = (Type) insTypes [i];
		return ret;
	}

	public override void Install (IDictionary state)
	{
		// Make sure that the assembly is installable
		CheckIfInstallable (Path);

		addSubInstallers ();

		state = new Hashtable ();
		string stateFile = System.IO.Path.ChangeExtension (this.Path, STATE_FILE_EXT);
		try {
			Console.WriteLine ("Starting installation");
			base.Install (state);
		} finally {
			// Serialise state
			FileStream file = new FileStream (stateFile, FileMode.Create, FileAccess.Write, FileShare.Read);
			try {
				SoapFormatter sf = new SoapFormatter ();
				sf.Serialize (file, state);
			} finally {
				file.Close ();
			}
		}
		Console.WriteLine ("Completed installation");
	}

	public override void Commit (IDictionary state)
	{
		addSubInstallers ();
		string stateFile = System.IO.Path.ChangeExtension (this.Path, STATE_FILE_EXT);
		// Read serialised state
		FileStream file = new FileStream (stateFile, FileMode.Open, FileAccess.Read, FileShare.Read);
		try {
			SoapFormatter sf = new SoapFormatter ();
			state = (IDictionary) sf.Deserialize (file);
		} finally {
			file.Close ();
		}

		base.Commit (state);
	}

	public override void Rollback (IDictionary state)
	{
		addSubInstallers ();
		string stateFile = System.IO.Path.ChangeExtension (this.Path, STATE_FILE_EXT);

		// Read serialised state
		FileStream file = new FileStream (stateFile, FileMode.Open, FileAccess.Read, FileShare.Read);
		try {
			SoapFormatter sf = new SoapFormatter ();
			state = (IDictionary) sf.Deserialize (file);
		} finally {
			file.Close ();
		}

		base.Rollback (state);

		File.Delete (stateFile);
	}

	public override void Uninstall (IDictionary state)
	{
		addSubInstallers ();
		string stateFile = System.IO.Path.ChangeExtension (this.Path, STATE_FILE_EXT);

		// Read serialised state
		FileStream file = new FileStream (stateFile, FileMode.Open, FileAccess.Read, FileShare.Read);
		try {
			SoapFormatter sf = new SoapFormatter ();
			state = (IDictionary) sf.Deserialize (file);
			file.Close ();
		} catch (Exception) {
			state = null;
		}

		base.Uninstall (state);

		File.Delete (stateFile);
	}
}

}
