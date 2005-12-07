//
// System.Web.Configuration.WebControlsSectionHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
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
using System.Configuration;
using System.IO;
using System.Web;
using System.Xml;

namespace System.Web.Configuration
{
	class WebControlsConfig
	{
		volatile static WebControlsConfig instance;
		string scriptsVDir;
		string configFilePath;
		static readonly object lockobj = new object ();
		
		public WebControlsConfig (WebControlsConfig parent, object context)
		{
			configFilePath = context as string;
			if (parent == null)
				return;
			
			scriptsVDir = parent.scriptsVDir;
			if (scriptsVDir != null)
				configFilePath = parent.configFilePath;
		}
		
		public void SetClientScriptsLocation (string location, out string error)
		{
			error = null;
			if (location == null || location.Length == 0) {
				error = "empty or null value for clientScriptsLocation";
				return;
			}

			if (location [0] != '/')
				location = "/" + location;

			string [] splitted = location.Split ('/');
			int end = splitted.Length;
			for (int i = 0; i < end; i++)
				splitted [i] = HttpUtility.UrlEncode (splitted [i]);

			scriptsVDir = String.Join ("/", splitted);
		}

		public string ScriptsPhysicalDirectory {
			get { return Path.Combine (Path.GetDirectoryName (configFilePath), "web_scripts"); }
		}

		public string ScriptsVirtualDirectory {
			get { return scriptsVDir; }
			set { scriptsVDir = value; }
		}

		static public WebControlsConfig Instance {
			get {
				//TODO: use HttpContext to get the configuration
				if (instance != null)
					return instance;

				lock (lockobj) {
					if (instance != null)
						return instance;

					instance = (WebControlsConfig) ConfigurationSettings.GetConfig ("system.web/webControls");
				}

				return instance;
			}
		}
	}
	
	class WebControlsSectionHandler : IConfigurationSectionHandler
	{
		public object Create (object parent, object context, XmlNode section)
		{
			WebControlsConfig config = new WebControlsConfig (parent as WebControlsConfig, context);

			if (section.Attributes == null && section.Attributes.Count == 0)
				ThrowException ("Lack of clientScriptsLocation attribute", section);

			string clientLocation = AttValue ("clientScriptsLocation", section, false);
			if (section.Attributes != null && section.Attributes.Count != 0)
				HandlersUtil.ThrowException ("Unrecognized attribute", section);

			string error;
			config.SetClientScriptsLocation (clientLocation, out error);
			if (error != null)
				HandlersUtil.ThrowException (error, section);

			return config;
		}

		// To save some typing...
		static string AttValue (string name, XmlNode node, bool optional)
		{
			return HandlersUtil.ExtractAttributeValue (name, node, optional);
		}

		static void ThrowException (string message, XmlNode node)
		{
			HandlersUtil.ThrowException (message, node);
		}
		//
	}
}

