using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Compilation;

[assembly:PreApplicationStartMethod (typeof (RegisterBuildProvider.Test.FooBuildProvider), "RegisterFooBuildProvider")]

namespace RegisterBuildProvider.Test
{
	public class FooBuildProvider : BuildProvider
	{
		public static void RegisterFooBuildProvider ()
		{
			Log.Data.Add ("RegisterFooBuildProvider called");

			try {
				BuildProvider.RegisterBuildProvider (".foo", typeof (string));
				Log.Data.Add ("Registering typeof (string) succeeded.");
			} catch (ArgumentException) {
				Log.Data.Add ("Registering typeof (string) failed (ArgumentException)");
			} catch (Exception ex) {
				Log.Data.Add (String.Format ("Registering typeof (string) failed ({0})", ex.GetType ()));
			}

			try {
				BuildProvider.RegisterBuildProvider (".foo", typeof (BuildProvider));
				Log.Data.Add ("Registering typeof (BuildProvider) succeeded.");
			} catch (ArgumentException) {
				Log.Data.Add ("Registering typeof (BuildProvider) failed (ArgumentException)");
			} catch (Exception ex) {
				Log.Data.Add (String.Format ("Registering typeof (BuildProvider) failed ({0})", ex.GetType ()));
			}

			try {
				BuildProvider.RegisterBuildProvider (".foo", typeof (FooBuildProvider));
				Log.Data.Add ("Registering typeof (FooBuildProvider) succeeded.");
			} catch (ArgumentException) {
				Log.Data.Add ("Registering typeof (FooBuildProvider) failed (ArgumentException)");
			} catch (Exception ex) {
				Log.Data.Add (String.Format ("Registering typeof (FooBuildProvider) failed ({0})", ex.GetType ()));
			}
		}

		public override void GenerateCode (AssemblyBuilder assemblyBuilder)
		{
			Log.Data.Add (String.Format ("{0}.GenerateCode called", this));
			base.GenerateCode (assemblyBuilder);
		}
	}
}