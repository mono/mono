//
// Authors:
//      Marek Habersack <grendel@twistedcode.net>
//
// (C) 2011 Novell, Inc (http://novell.com)
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
using System.Collections.Concurrent;
using System.Web;
using System.Web.Hosting;

namespace System.Web.Compilation
{
	class BuildManagerHost : MarshalByRefObject, IRegisteredObject
	{

		private static bool _inClientBuildManager;

		internal static bool InClientBuildManager {
			get { return _inClientBuildManager; }
			set { _inClientBuildManager = true; }
		}

		// This method is used by the Cassini ASP.NET host application (and all of its
		// derivatives, e.g. CassiniDev, see http://cassinidev.codeplex.com) to register the
		// host assembly with System.Web's assembly resolver in order to enable loading
		// types from assemblies not installed in GAC.
		//
		protected void RegisterAssembly (string assemblyName, string assemblyLocation)
		{
			if (String.IsNullOrEmpty (assemblyName) || String.IsNullOrEmpty (assemblyLocation))
				return;

			HttpRuntime.RegisteredAssemblies.InsertOrUpdate ((uint)assemblyName.GetHashCode (), assemblyName, assemblyLocation, assemblyLocation);
			HttpRuntime.EnableAssemblyMapping (true);
		}

		public void Stop (bool immediate)
		{}
	}
}
