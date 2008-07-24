// System.Configuration.Install.AssemblyInstaller.cs
//
// Author:
// 	Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) Novell
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

		public AssemblyInstaller (string fileName, string[] commandLine)
		{
			Path = System.IO.Path.GetFullPath (fileName);
			_commandLine = commandLine;
			_useNewContext = true;
		}

		[MonoTODO]
		public static void CheckIfInstallable (string assemblyName)
		{
			throw new NotImplementedException ();
		}

		public override void Commit (IDictionary savedState)
		{
			base.Commit (savedState);
		}

		public override void Install (IDictionary savedState)
		{
			base.Install (savedState);
		}

		public override void Rollback (IDictionary savedState)
		{
			base.Rollback (savedState);
		}

		public override void Uninstall (IDictionary savedState)
		{
			base.Uninstall (savedState);
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
				return base.HelpText;
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
