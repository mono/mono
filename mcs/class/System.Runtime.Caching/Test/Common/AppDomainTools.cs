//
// AppDomainTools.cs
//
// Authors:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc. (http://novell.com/)
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;

using NUnit.Framework;

namespace MonoTests.Common
{
	class AppDomainTools
	{
		public static void RunInSeparateDomain (Action handler, string format, params object[] parms)
		{
			var setup = AppDomain.CurrentDomain.SetupInformation;
			setup.ShadowCopyDirectories = null;
			setup.ShadowCopyFiles = null;
			setup.ApplicationBase = Path.GetDirectoryName (typeof (AppDomainTools).Assembly.Location);
			var ad = AppDomain.CreateDomain ("Test", new Evidence (AppDomain.CurrentDomain.Evidence) , setup);
			ad.SetData ("testHandler", handler);
			string message;
			if (parms != null && parms.Length > 0)
				message = String.Format (format, parms);
			else
				message = format;
			ad.SetData ("failureMessage", message);
			//ad.AssemblyResolve += ResolveAssemblyEventHandler;
			ad.DoCallBack (RunTest);
		}

		static Assembly ResolveAssemblyEventHandler (object sender, ResolveEventArgs args)
		{
			string path = Path.Combine (AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "nunit.framework.dll");
			if (File.Exists (path))
				return Assembly.LoadFrom (path);

			return null;
		}

		static void RunTest ()
		{
			Action handler = AppDomain.CurrentDomain.GetData ("testHandler") as Action;
			if (handler == null) {
				string message = AppDomain.CurrentDomain.GetData ("failureMessage") as string;
				Assert.Fail (message);
			}

			handler ();
		}
	}
}
