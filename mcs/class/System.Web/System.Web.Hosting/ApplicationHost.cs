//
// System.Web.Hosting.ApplicationHost
//
// Authors:
// 	Patrik Torstensson (Patrik.Torstensson@labs2.com)
// 	(class signature from Bob Smith <bob@thestuff.net> (C) )
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
using System;
using System.Collections;
using System.IO;
using System.Runtime.Remoting;

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
			//FIXME: this should be the directory where dlls reside.
			domain.SetData (".hostingInstallDir", "FIXME hostingInstallDir");
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

