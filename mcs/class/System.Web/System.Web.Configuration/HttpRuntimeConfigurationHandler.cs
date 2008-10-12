//
// System.Web.Configuration.HttpRuntimeConfigurationHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Novell, Inc. (http://www.novell.com)
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

			config.ApartmentThreading = AttBoolValue (section, "apartmentThreading", false);
			config.AppRequestQueueLimit = AttUIntValue (section, "appRequestQueueLimit", 100);
			config.DelayNotificationTimeout = AttUIntValue (section, "delayNotificationTimeout", 5);
			config.EnableHeaderChecking = AttBoolValue (section, "enableHeaderChecking", true);
			config.EnableKernelOutputCache = AttBoolValue (section, "enableKernelOutputCache", true);
			config.EnableVersionHeader = AttBoolValue (section, "enableVersionHeader", true);
			config.ExecutionTimeout = AttUIntValue (section, "executionTimeout", 90);
			config.MaxRequestLength = AttUIntValue (section, "maxRequestLength", 4096);
			config.MaxWaitChangeNotification = AttUIntValue (section, "maxWaitChangeNotification", 0);
			config.MinFreeThreads = AttUIntValue (section, "minFreeThreads", 8);
			config.MinLocalRequestFreeThreads = AttUIntValue (section, "minLocalRequestFreeThreads", 4);
			config.RequestLengthDiskThreshold = AttUIntValue (section, "requestLengthDiskThreshold", 256);
			config.SendCacheControlHeader = AttBoolValue (section, "sendCacheControlHeader", true);
			config.ShutdownTimeout = AttUIntValue (section, "shutdownTimeout", 90);
			config.UseFullyQualifiedRedirectUrl = AttBoolValue (section, "useFullyQualifiedRedirectUrl", false);
			config.WaitChangeNotification = AttUIntValue (section, "waitChangeNotification", 0);
			return config;
		}

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
	}
}

