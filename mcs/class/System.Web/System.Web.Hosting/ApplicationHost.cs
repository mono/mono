//
// System.Web.Hosting.ApplicationHost.cs 
// 
// Author:
//	Miguel de Icaza (miguel@novell.com)
//
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Security.Policy;

namespace System.Web.Hosting {

	public sealed class ApplicationHost {

		static string [] types = { "Web.config", "web.config" };

		private ApplicationHost ()
		{
		}

		static string FindWebConfig (string basedir)
		{
			string r = null;
				
			foreach (string s in types){
				r = Path.Combine (basedir, s);

				if (File.Exists (r))
					return r;
			}
			// default: return the last one
			return r;
		}

		//
		// For furthe details see `Hosting the ASP.NET runtime'
		//
		//    http://www.west-wind.com/presentations/aspnetruntime/aspnetruntime.asp
		// 
		public static object CreateApplicationHost (Type hostType, string virtualDir, string physicalDir)
		{
			if (physicalDir == null)
				throw new NullReferenceException ();

#pragma warning disable 219
			//
			// This is done just for validation: it might throw an exception
			//
			Uri u = new Uri (physicalDir);
#pragma warning restore 219 

			if (hostType == null)
				throw new NullReferenceException ();

			if (virtualDir == null)
				throw new NullReferenceException ();

			if (hostType == null || virtualDir == null || physicalDir == null)
				throw new NullReferenceException ();

			Evidence evidence = new Evidence (AppDomain.CurrentDomain.Evidence);

			// 
			// Unique Domain ID
			//
			string domain_id = "ASPHOST_" + DateTime.Now.ToString().GetHashCode().ToString("x");
			
			//
			// Setup
			//
			AppDomainSetup setup = new AppDomainSetup ();

			setup.ApplicationBase = physicalDir;

			setup.CachePath = null;
			setup.ConfigurationFile = FindWebConfig (physicalDir);
			setup.DisallowCodeDownload = true;
			setup.PrivateBinPath = "bin";
			setup.PrivateBinPathProbe = "*";
			setup.ShadowCopyFiles = "true";
			UriBuilder b = new UriBuilder ("file://", null, 0, Path.Combine (physicalDir, "bin"));
			setup.ShadowCopyDirectories = b.Uri.ToString ();

			//
			// Create app domain
			//
			AppDomain appdomain;
			appdomain = AppDomain.CreateDomain (domain_id, evidence, setup);

			//
			// Populate with the AppDomain data keys expected, Mono only uses a
			// few, but third party apps might use others:
			//
			appdomain.SetData (".appDomain", "*");
			appdomain.SetData (".appPath", physicalDir);
			appdomain.SetData (".appVPath", virtualDir);
			appdomain.SetData (".domainId", domain_id);
			appdomain.SetData (".hostingVirtualPath", virtualDir);
			appdomain.SetData (".hostingInstallDir", Path.GetDirectoryName (typeof (Object).Assembly.CodeBase));

			return appdomain.CreateInstanceAndUnwrap (hostType.Module.Assembly.FullName, hostType.FullName);
		}
	}
}
