//
// System.AssemblyLoadEventArgs.cs 
//
// Author:
//   Chris Hynes (chrish@assistedsolutions.com)
//
// (C) 2001 Chris Hynes
//

using System;
using System.Reflection;

namespace System 
{
	public class AssemblyLoadEventArgs: EventArgs
	{
		protected Assembly loadedAssembly;

		public AssemblyLoadEventArgs(Assembly loadedAssembly)
		{
			this.loadedAssembly = loadedAssembly;
		}

		public Assembly LoadedAssembly
		{
			get 
			{
				return loadedAssembly;
			}
		}
	}
}


