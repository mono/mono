// System.Configuration.Install.AssemblyInstaller.cs
//
// Author:
// 	Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) Novell
//

using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace System.Configuration.Install
{
	public class AssemblyInstaller : Installer
	{
		public AssemblyInstaller ()
		{
		}

		public AssemblyInstaller (Assembly assembly, string[] commandLine)
		{
			_assembly = assembly;
			_commandLine = commandLine;
			_useNewContext = true;
		}

		public AssemblyInstaller (string filename, string[] commandLine)
		{
			Path = System.IO.Path.GetFullPath (filename);
			_commandLine = commandLine;
			_useNewContext = true;
		}

		[MonoTODO]
		public static void CheckIfInstallable (string assemblyName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Commit (IDictionary savedState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Install (IDictionary savedState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Rollback (IDictionary savedState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Uninstall (IDictionary savedState)
		{
			throw new NotImplementedException ();
		}

		public Assembly Assembly {
			get {
				return _assembly;
			}
			set {
				_assembly = value;
			}
		}

		public string[] CommandLine {
			get {
				return _commandLine;
			}
			set {
				_commandLine = value;
			}
		}

		public override string HelpText {
			get {
				throw new NotImplementedException ();
			}
		}
		public string Path {
			get {
				if (_assembly == null)
					return null;
				
				return _assembly.Location;
			}
			set {
				if (value == null)
					_assembly = null;

				_assembly = Assembly.LoadFrom (value);
			}
		}
		public bool UseNewContext {
			get {
				return _useNewContext;
			}
			set {
				_useNewContext = value;
			}
		}

		private Assembly _assembly;
		private string[] _commandLine;
		private bool _useNewContext;
	}
}
