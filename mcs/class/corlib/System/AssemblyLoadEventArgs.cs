//
// System.AssemblyLoadEventArgs.cs
//
// Author:
//   Chris Hynes (chrish@assistedsolutions.com)
//
// (C) 2001 Chris Hynes
//

using System.Reflection;

namespace System 
{
	public class AssemblyLoadEventArgs: EventArgs
	{
		private Assembly m_loadedAssembly;

		public AssemblyLoadEventArgs (Assembly loadedAssembly)
		{
			this.m_loadedAssembly = loadedAssembly;
		}

		public Assembly LoadedAssembly {
			get {
				return m_loadedAssembly;
			}
		}
	}
}
