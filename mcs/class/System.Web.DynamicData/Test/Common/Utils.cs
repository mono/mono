using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.DynamicData;
using System.Web.DynamicData.ModelProviders;
using System.Web.Routing;

using MonoTests.System.Web.DynamicData;

namespace MonoTests.Common
{
	public static class Utils
	{
		public static MetaModel CommonInitialize ()
		{
			return CommonInitialize (false);
		}

		public static MetaModel CommonInitialize (bool myDynamicDataRoute)
		{
			MetaModel m = MetaModel.Default;

			var req = new FakeHttpWorkerRequest ();
			var ctx = new HttpContext (req);
			HttpContext.Current = ctx;

			RouteCollection routes = RouteTable.Routes;
			routes.Clear ();
			if (myDynamicDataRoute) {
				routes.Add (
					new MyDynamicDataRoute ("{table}/{action}.aspx") {
						Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
						Model = m,
						RouteHandler = new MyDynamicDataRouteHandler ()
					});
			} else {
				routes.Add (
					new DynamicDataRoute ("{table}/{action}.aspx") {
						Constraints = new RouteValueDictionary (new { action = "List|Details|Edit|Insert" }),
						Model = m,
						RouteHandler = new MyDynamicDataRouteHandler ()
					});
			}

			return m;
		}

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

		public static void RegisterContext (Type contextType)
		{
			RegisterContext (contextType, null);
		}

		public static void RegisterContext (DataModelProvider model, ContextConfiguration config)
		{
			RegisterContext (model, config, true);
		}

		public static void RegisterContext (Type contextType, ContextConfiguration config)
		{
			RegisterContext (contextType, config, true);
		}
		
		public static void RegisterContext (DataModelProvider model, ContextConfiguration config, bool defaultModel)
		{
			// Just in case no model has been created yet
			MetaModel m = new MetaModel ();

			if (defaultModel)
				m = MetaModel.Default;

			Exception exception = null;
			MetaModel registered = null;

			try {
				registered = MetaModel.GetModel (model.ContextType);
			} catch (Exception) {
				// ignore
			}

			try {
				if (registered == null)
					m.RegisterContext (model, config);
			} catch (InvalidOperationException ex) {
				exception = ex;
			}

			if (exception != null) {
				Console.WriteLine ("RegisterContext exception:");
				Console.WriteLine (exception);
			}
		}

		public static void RegisterContext (Type contextType, ContextConfiguration config, bool defaultModel)
		{
			// Just in case no model has been created yet
			MetaModel m = new MetaModel ();

			if (defaultModel)
				m = MetaModel.Default;

			Exception exception = null;
			MetaModel registered = null;

			try {
				registered = MetaModel.GetModel (contextType);
			} catch (Exception) {
				// ignore
			}

			try {
				if (registered == null) {
					if (config != null)
						m.RegisterContext (contextType, config);
					else
						m.RegisterContext (contextType);
				}
			} catch (InvalidOperationException ex) {
				exception = ex;
			}

			if (exception != null) {
				Console.WriteLine ("RegisterContext exception:");
				Console.WriteLine (exception);
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
