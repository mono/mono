//
// System.Web.Services.Configuration.WebServicesConfigurationSectionHandler
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
using System.Collections;
using System.Configuration;
using System.Xml;

namespace System.Web.Services.Configuration
{
	[Flags]
	enum WSProtocol
	{
		HttpSoap = 1,
		HttpPost = 1 << 1,
		HttpGet =  1 << 2,
		Documentation = 1 << 3,
		HttpSoap12 = 1 << 4,
		HttpPostLocalhost = 1 << 5,
		AnyHttpSoap = HttpSoap | HttpSoap12,
		All = 0xFF
	}
	
	class WSConfig
	{
#if !TARGET_JVM
		volatile static WSConfig instance;
#else
		static WSConfig instance {
			get {
				return (WSConfig)AppDomain.CurrentDomain.GetData("WSConfig.instance");
			}
			set {
				AppDomain.CurrentDomain.SetData("WSConfig.instance", value);
			}
		}

#endif
		WSProtocol protocols;
		string wsdlHelpPage;
		string filePath;
		ArrayList extensionTypes = new ArrayList();
		ArrayList extensionImporterTypes = new ArrayList();
		ArrayList extensionReflectorTypes = new ArrayList();
		ArrayList formatExtensionTypes = new ArrayList();
		static readonly object lockobj = new object ();
		
		public WSConfig (WSConfig parent, object context)
		{
			if (parent == null)
				return;
			
			protocols = parent.protocols;
			wsdlHelpPage = parent.wsdlHelpPage;
			if (wsdlHelpPage != null)
				filePath = parent.filePath;
			else
				filePath = context as string;
		}
		
		static WSProtocol ParseProtocol (string protoName, out string error)
		{
			WSProtocol proto;
			error = null;

#if ONLY_1_1
			switch (protoName) {
				case "HttpSoap1.2":
					protoName = "HttpSoap12";
					break;
				case "HttpSoap12":
					protoName = null;
					break;
			}
#endif
			try {
				proto = (WSProtocol) Enum.Parse (typeof (WSProtocol), protoName);
			} catch {
				error = "Invalid protocol name";
				return 0;
			}

			return proto;
		}

		// Methods to modify configuration values
		public bool AddProtocol (string protoName, out string error)
		{
			if (protoName == "All") {
				error = "Invalid protocol name";
				return false;
			}

			WSProtocol proto = ParseProtocol (protoName, out error);
			if (error != null)
				return false;

			protocols |= proto;
			return true;
		}

		public bool RemoveProtocol (string protoName, out string error)
		{
			if (protoName == "All") {
				error = "Invalid protocol name";
				return false;
			}

			WSProtocol proto = ParseProtocol (protoName, out error);
			if (error != null)
				return false;

			protocols &= ~proto;
			return true;
		}

		public void ClearProtocol ()
		{
			protocols = 0;
		}

		// Methods to query/get configuration
		public static bool IsSupported (WSProtocol proto)
		{
			return ((Instance.protocols & proto) == proto && (proto != 0) && (proto != WSProtocol.All));
		}

		// Properties
		public string WsdlHelpPage {
			get { return wsdlHelpPage; }
			set { wsdlHelpPage = value; }
		}

		public string ConfigFilePath {
			get { return filePath; }
			set { filePath = value; }
		}

		static public WSConfig Instance {
			get {
				//TODO: use HttpContext to get the configuration
				if (instance != null)
					return instance;

				lock (lockobj) {
					if (instance != null)
						return instance;

					instance = (WSConfig) ConfigurationSettings.GetConfig ("system.web/webServices");
				}

				return instance;
			}
		}

		public ArrayList ExtensionTypes {
			get { return extensionTypes; }
		}

		public ArrayList ExtensionImporterTypes {
			get { return extensionImporterTypes; }
		}
		
		public ArrayList ExtensionReflectorTypes {
			get { return extensionReflectorTypes; }
		}
		
		public ArrayList FormatExtensionTypes {
			get { return formatExtensionTypes; }
		}
		
	}
	
	enum WSExtensionGroup
	{
		High,
		Low
	}
	
	class WSExtensionConfig
	{
		Type type;
		int priority;
		WSExtensionGroup group;

		public Exception SetType (string typeName)
		{
			Exception exc = null;
			
			try {
				type = Type.GetType (typeName, true);
			} catch (Exception e) {
				exc = e;
			}

			return exc;
		}
		
		public Exception SetPriority (string prio)
		{
			if (prio == null || prio == "")
				return null;

			Exception exc = null;
			try {
				priority = Int32.Parse (prio);
			} catch (Exception e) {
				exc = e;
			}

			return exc;
		}
		
		public Exception SetGroup (string grp)
		{
			if (grp == null || grp == "")
				return null;

			Exception exc = null;
			try {
				group = (WSExtensionGroup) Int32.Parse (grp);
				if (group < WSExtensionGroup.High || group > WSExtensionGroup.Low)
					throw new ArgumentOutOfRangeException ("group", "Must be 0 or 1");
			} catch (Exception e) {
				exc = e;
			}

			return exc;
		}
		
		// Getters
		public Type Type {
			get { return type; }
		}

		public int Priority {
			get { return priority; }
		}

		public WSExtensionGroup Group {
			get { return group; }
		}
	}
	
	class WebServicesConfigurationSectionHandler : IConfigurationSectionHandler
	{
		public object Create (object parent, object context, XmlNode section)
		{
			WSConfig config = new WSConfig (parent as WSConfig, context);

			if (section.Attributes != null && section.Attributes.Count != 0)
				ThrowException ("Unrecognized attribute", section);

			XmlNodeList nodes = section.ChildNodes;
			foreach (XmlNode child in nodes) {
				XmlNodeType ntype = child.NodeType;
				if (ntype == XmlNodeType.Whitespace || ntype == XmlNodeType.Comment)
					continue;

				if (ntype != XmlNodeType.Element)
					ThrowException ("Only elements allowed", child);
				
				string name = child.Name;
				if (name == "protocols") {
					ConfigProtocols (child, config);
					continue;
				}

				if (name == "soapExtensionTypes") {
					ConfigSoapExtensionTypes (child, config.ExtensionTypes);
					continue;
				}

				if (name == "soapExtensionReflectorTypes") {
					ConfigSoapExtensionTypes (child, config.ExtensionReflectorTypes);
					continue;
				}

				if (name == "soapExtensionImporterTypes") {
					ConfigSoapExtensionTypes (child, config.ExtensionImporterTypes);
					continue;
				}

				if (name == "serviceDescriptionFormatExtensionTypes") {
					ConfigFormatExtensionTypes (child, config);
					continue;
				}

				if (name == "wsdlHelpGenerator") {
					string href = AttValue ("href", child, false);
					if (child.Attributes != null && child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					config.ConfigFilePath = context as string;
					config.WsdlHelpPage = href;
					continue;
				}

				ThrowException ("Unexpected element", child);
			}

			return config;
		}

		static void ConfigProtocols (XmlNode section, WSConfig config)
		{
			if (section.Attributes != null && section.Attributes.Count != 0)
				ThrowException ("Unrecognized attribute", section);

			XmlNodeList nodes = section.ChildNodes;
			foreach (XmlNode child in nodes) {
				XmlNodeType ntype = child.NodeType;
				if (ntype == XmlNodeType.Whitespace || ntype == XmlNodeType.Comment)
					continue;

				if (ntype != XmlNodeType.Element)
					ThrowException ("Only elements allowed", child);
				
				string name = child.Name;
				string error;
				if (name == "add") {
					string protoName = AttValue ("name", child, false);
					if (child.Attributes != null && child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					if (!config.AddProtocol (protoName, out error))
						ThrowException (error, child);
					
					continue;
				}

				if (name == "remove") {
					string protoName = AttValue ("name", child, false);
					if (child.Attributes != null && child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					if (!config.RemoveProtocol (protoName, out error))
						ThrowException (error, child);
					
					continue;
				}

				if (name == "clear") {
					if (child.Attributes != null && child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					config.ClearProtocol ();
					continue;
				}

				ThrowException ("Unexpected element", child);
			}
		}
		
		static void ConfigSoapExtensionTypes (XmlNode section, ArrayList extensions)
		{
			if (section.Attributes != null && section.Attributes.Count != 0)
				ThrowException ("Unrecognized attribute", section);

			XmlNodeList nodes = section.ChildNodes;
			foreach (XmlNode child in nodes) {
				XmlNodeType ntype = child.NodeType;
				if (ntype == XmlNodeType.Whitespace || ntype == XmlNodeType.Comment)
					continue;

				if (ntype != XmlNodeType.Element)
					ThrowException ("Only elements allowed", child);
				
				string name = child.Name;
				if (name == "add") {
					string seType = AttValue ("type", child, false);
					string priority = AttValue ("priority", child);
					string group = AttValue ("group", child);
					if (child.Attributes != null && child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					WSExtensionConfig wse = new WSExtensionConfig ();
					Exception e = wse.SetType (seType);
					if (e != null)
						ThrowException (e.Message, child);

					e = wse.SetPriority (priority);
					if (e != null)
						ThrowException (e.Message, child);

					e = wse.SetGroup (group);
					if (e != null)
						ThrowException (e.Message, child);

					extensions.Add (wse);
					continue;
				}

				ThrowException ("Unexpected element", child);
			}
		}
		
		static void ConfigFormatExtensionTypes (XmlNode section, WSConfig config)
		{
			if (section.Attributes != null && section.Attributes.Count != 0)
				ThrowException ("Unrecognized attribute", section);

			XmlNodeList nodes = section.ChildNodes;
			foreach (XmlNode child in nodes) {
				XmlNodeType ntype = child.NodeType;
				if (ntype == XmlNodeType.Whitespace || ntype == XmlNodeType.Comment)
					continue;

				if (ntype != XmlNodeType.Element)
					ThrowException ("Only elements allowed", child);
				
				string name = child.Name;
				if (name == "add") {
					string typeName = AttValue ("name", child, false);
					if (child.Attributes != null && child.Attributes.Count != 0)
						HandlersUtil.ThrowException ("Unrecognized attribute", child);

					try {
						config.FormatExtensionTypes.Add (Type.GetType (typeName, true));
					} catch (Exception e) {
						ThrowException (e.Message, child);
					}
					continue;
				}

				ThrowException ("Unexpected element", child);
			}
		}
		
		// To save some typing...
		static string AttValue (string name, XmlNode node, bool optional)
		{
			return HandlersUtil.ExtractAttributeValue (name, node, optional);
		}

		static string AttValue (string name, XmlNode node)
		{
			return HandlersUtil.ExtractAttributeValue (name, node, true);
		}

		static void ThrowException (string message, XmlNode node)
		{
			HandlersUtil.ThrowException (message, node);
		}
		//
	}
	
	class HandlersUtil
	{
		private HandlersUtil ()
		{
		}

		static internal string ExtractAttributeValue (string attKey, XmlNode node)
		{
			return ExtractAttributeValue (attKey, node, false);
		}
			
		static internal string ExtractAttributeValue (string attKey, XmlNode node, bool optional)
		{
			if (node.Attributes == null) {
				if (optional)
					return null;

				ThrowException ("Required attribute not found: " + attKey, node);
			}

			XmlNode att = node.Attributes.RemoveNamedItem (attKey);
			if (att == null) {
				if (optional)
					return null;
				ThrowException ("Required attribute not found: " + attKey, node);
			}

			string value = att.Value;
			if (value == String.Empty) {
				string opt = optional ? "Optional" : "Required";
				ThrowException (opt + " attribute is empty: " + attKey, node);
			}

			return value;
		}

		static internal void ThrowException (string msg, XmlNode node)
		{
			if (node != null && node.Name != String.Empty)
				msg = msg + " (node name: " + node.Name + ") ";
			throw new ConfigurationException (msg, node);
		}
	}

}
