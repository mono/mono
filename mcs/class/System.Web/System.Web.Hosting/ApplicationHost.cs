//
// System.Web.Hosting.ApplicationHost
//
// Authors:
// 	Patrik Torstensson (Patrik.Torstensson@labs2.com)
// 	(class signature from Bob Smith <bob@thestuff.net> (C) )
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
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
using System.IO;
using System.Runtime.Remoting;
using System.Web.Util;

namespace System.Web.Hosting
{
	public sealed class ApplicationHost
	{
		internal class ConfigInitHelper : MarshalByRefObject
		{
			internal void InitConfig ()
			{
			}
		}
		
		private ApplicationHost ()
		{
		}

		public static object CreateApplicationHost (Type hostType,
							    string virtualDir,
							    string physicalDir)
		{
			if (hostType == null)
				throw new ArgumentException ("hostType");

			if (virtualDir == null || virtualDir.Length == 0)
				throw new ArgumentException ("virtualDir");
			
			if (physicalDir == null || physicalDir.Length == 0)
				throw new ArgumentException ("physicalDir");

			if (physicalDir [physicalDir.Length - 1] != Path.DirectorySeparatorChar)
				physicalDir += Path.DirectorySeparatorChar;

			int nowInt = DateTime.Now.ToString ().GetHashCode ();
			string nowHash = nowInt.ToString ("x");
			nowInt += physicalDir.GetHashCode ();
			string sum = nowInt.ToString ("x");
			Hashtable hTable = new Hashtable ();
			AppDomainSetup domainSetup = new AppDomainSetup();

			AppDomainFactory.PopulateDomainBindings (nowHash,
								 sum,
								 sum,
								 physicalDir,
								 virtualDir,
								 domainSetup,
								 hTable);
			
			AppDomain domain = AppDomain.CreateDomain (nowHash, null, domainSetup);
			foreach (string key in hTable.Keys)
				domain.SetData (key, (string) hTable [key]);

			domain.SetData (".hostingVirtualPath", virtualDir);
			domain.SetData (".hostingInstallDir", ICalls.GetMachineInstallDirectory ());
			InitConfigInNewAppDomain (domain);
			ObjectHandle o = domain.CreateInstance (hostType.Assembly.FullName,
								hostType.FullName);
			return o.Unwrap();
		}

		private static void InitConfigInNewAppDomain (AppDomain appDomain)
		{
			Type t = typeof (ConfigInitHelper);
			ObjectHandle o = appDomain.CreateInstance (t.Assembly.FullName, t.FullName);
			ConfigInitHelper helper = (ConfigInitHelper) o.Unwrap();
			helper.InitConfig ();
		}
	}
}

