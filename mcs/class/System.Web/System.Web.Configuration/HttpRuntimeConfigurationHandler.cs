//
// System.Web.Configuration.HttpRuntimeConfigurationHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Collections;
using System.Configuration;
using System.Security.Cryptography;
using System.Xml;

namespace System.Web.Configuration
{
	class HttpRuntimeConfigurationHandler : IConfigurationSectionHandler
	{
		public object Create (object parent, object context, XmlNode section)
		{
			if (section.HasChildNodes)
				ThrowException ("Child nodes not allowed here", section.FirstChild);

			HttpRuntimeConfig config = new HttpRuntimeConfig (parent);

			config.ExecutionTimeout = AttUIntValue (section, "executionTimeout", 90);
			config.MaxRequestLength = AttUIntValue (section, "maxRequestLength", 4096);
			config.RequestLengthDiskThreshold = AttUIntValue (section, "requestLengthDiskThreshold", 256);
			config.UseFullyQualifiedRedirectUrl = AttBoolValue (section, "useFullyQualifiedRedirectUrl", false);
			config.MinFreeThreads = AttUIntValue (section, "minFresThreads", 8);
			config.MinLocalRequestFreeThreads = AttUIntValue (section, "minLocalRequestFreeThreads", 4);
			config.AppRequestQueueLimit = AttUIntValue (section, "appRequestQueueLimit", 100);
			config.EnableKernelOutputCache = AttBoolValue (section, "requestLengthDiskThreshold", true);
			config.EnableVersionHeader = AttBoolValue (section, "requestLengthDiskThreshold", true);
			config.RequireRootSaveAsPath = AttBoolValue (section, "requestLengthDiskThreshold", true);
			config.IdleTimeout = AttUIntValue (section, "requestLengthDiskThreshold", 20);
			config.Enable = AttBoolValue (section, "requestLengthDiskThreshold", true);
			config.VersionHeader = AttValue (section, "versionHeader");

			return config;
		}

		//
		static bool AttBoolValue (XmlNode node, string name, bool _default)
		{
			string v = AttValue (node, name);
			if (v == null)
				return _default;

			bool result = (v == "true");
			if (!result && v != "false")
				ThrowException ("Invalid boolean value in " + name, node);

			return result;
		}

		static int AttUIntValue (XmlNode node, string name, int _default)
		{
			string v = AttValue (node, name);
			if (v == null)
				return _default;

			int result = 0;
			try {
				result = (int) UInt32.Parse (v);
			} catch {
				ThrowException ("Invalid number in " + name, node);
			}

			return result;
		}


		static string AttValue (XmlNode node, string name)
		{
			return HandlersUtil.ExtractAttributeValue (name, node, true);
		}

		static void ThrowException (string message, XmlNode node)
		{
			HandlersUtil.ThrowException (message, node);
		}
		//
	}
}

