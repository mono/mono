using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.DynamicData;
using System.Web.DynamicData.ModelProviders;

namespace MonoTests.Common
{
	static class Utils
	{
		public static MetaModel GetModel<ContextType> ()
		{
			// This is really, really dumb but we need that since if the type has already
			// been registered by another test, or tests are re-ran without nunit having
			// reloaded the dll we'll get a duplicate entry exception.
			MetaModel m;
			try {
				m = MetaModel.GetModel (typeof (ContextType));
			} catch (InvalidOperationException) {
				m = new MetaModel ();
				m.RegisterContext (typeof (ContextType));
			} finally {
				MetaModel.ResetRegistrationException ();
			}

			return m;
		}

		public static void RegisterContext (DataModelProvider model)
		{
			RegisterContext (model, null);
		}

		public static void RegisterContext (DataModelProvider model, ContextConfiguration config)
		{
			// Just in case no model has been created yet
			MetaModel m = new MetaModel ();

			// And get the default model instead, whatever it is
			m = MetaModel.Default;
			try {
				m.RegisterContext (model, config);
			} catch (InvalidOperationException) {
				// ignore
			}
		}

		public static string BuildActionName (MetaTable table, string action)
		{
			return "/" + table.Name + "/" + action + ".aspx";
		}

		public static string BuildActionName (MetaTable table, string action, string query)
		{
			string ret = "/" + table.Name + "/" + action + ".aspx";
			if (!String.IsNullOrEmpty (query))
				ret += "?" + query;
			return ret;
		}
	}
}
