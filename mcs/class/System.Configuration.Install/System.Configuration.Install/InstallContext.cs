// System.Configuration.Install.InstallContext.cs
//
// Author:
// 	Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro Sánchez Acosta
// 

using System.Collections.Specialized;

namespace System.Configuration.Install
{
	public class InstallContext
	{
		private StringDictionary parameters;
		
		[MonoTODO]
		public InstallContext () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public InstallContext (string logFilePath, string[] commandLine) {
			throw new NotImplementedException ();
		}

		public StringDictionary Parameters {
			get {
				return parameters;
			}

			set {
				parameters = value;
			}
		}		

		[MonoTODO]
		public bool IsParameterTrue (string paramName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void LogMessage (string message)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static StringDictionary ParseCommandLine (string[] args)
		{
			throw new NotImplementedException ();
		}
	}
}
