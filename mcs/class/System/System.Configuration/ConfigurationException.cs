//
// System.Configuration.ConfigurationException.cs
//
// Author:
//   Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Globalization;
using System.Runtime.Serialization;

#if (XML_DEP)
using System.Xml;
#endif

namespace System.Configuration 
{
	[Serializable]
	public class ConfigurationException : SystemException
	{
		// Fields		
		string bareMessage;
		string filename;
		int line;

		//
		// Constructors
		//
		public ConfigurationException ()
			: base (Locale.GetText ("There is an error in a configuration setting."))
		{
			filename = null;
			bareMessage = Locale.GetText ("There is an error in a configuration setting.");
			line = 0;
		}
		
		public ConfigurationException (string message)
			: base (message)
		{
			bareMessage = message;
		}

		protected ConfigurationException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			filename = info.GetString ("filename");
			line = info.GetInt32 ("line");
		}

		public ConfigurationException (string message, Exception inner)
			: base (message, inner)
		{
			bareMessage = message;
		}

#if (XML_DEP)
		public ConfigurationException (string message, XmlNode node)
			: base (message)
		{
			filename = GetXmlNodeFilename(node);
			line = GetXmlNodeLineNumber(node);
			bareMessage = message;
		}

		public ConfigurationException (string message, Exception inner, XmlNode node)
			: base (message, inner)
		{
			filename = GetXmlNodeFilename (node);
			line = GetXmlNodeLineNumber (node);
			bareMessage = message;
		}
#endif
		public ConfigurationException (string message, string filename, int line)
			: base (message)
		{
			bareMessage = message;
			this.filename = filename;
			this.line= line;
		}

		public ConfigurationException (string message, Exception inner, string filename, int line)
			: base (message)
		{
			bareMessage = message;
			this.filename = filename;
			this.line = line;
		}
		//
		// Properties
		//
		public string BareMessage
		{
			get  { return bareMessage; }
		}

		public string Filename
		{
			get { return filename; }
		}
		
		public int Line
		{
			get { return line; }
		}

		public override string Message
		{
			get {
				string baseMsg = base.Message;
				string f = (filename == null) ? String.Empty : filename;
				string l = (line == 0) ? String.Empty : (" line " + line);

				return baseMsg + "(" + f + l + ")";
			}
		}

		//
		// Methods
		//
#if (XML_DEP)
		public static string GetXmlNodeFilename (XmlNode node)
		{
			if (!(node is IConfigXmlNode))
				return String.Empty;

			return ((IConfigXmlNode) node).Filename;
		}

		public static int GetXmlNodeLineNumber (XmlNode node)
		{
			if (!(node is IConfigXmlNode))
				return 0;

			return ((IConfigXmlNode) node).LineNumber;
		}
#endif
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("filename", filename);
			info.AddValue ("line", line);
		}
	}
}
