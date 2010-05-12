using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Compilation;

using ApplicationPreStartMethods;

namespace ApplicationPreStartMethods.Tests
{
	public class PreStartMethodsBase
	{
		public static void PublicStaticMethod ()
		{
			//throw new InvalidOperationException ("test");
			_default.PreApplicationStartMessages.Add ("Public static method called");
			try {
				string path = Path.Combine (HttpRuntime.AppDomainAppPath, "ExternalAssemblies", "ExternalAssembly1.dll");
				if (!File.Exists (path))
					return;

				Assembly asm = Assembly.LoadFrom (path);
				BuildManager.AddReferencedAssembly (asm);
			} catch {
				// ignore
			}
		}
	}

	public class PreStartMethods : PreStartMethodsBase
	{
		public void PublicInstanceMethod (string param)
		{
		}

		public static void PublicStaticMethod (string val)
		{
		}

		internal void InternalInstanceMethod ()
		{
		}

		static internal void InternalStaticMethod ()
		{
		}

		void PrivateInstanceMethod ()
		{
		}

		static void PrivateStaticMethod ()
		{
		}
	}
}