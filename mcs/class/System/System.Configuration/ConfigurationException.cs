//
// System.Configuration.ConfigurationException.cs
//
// Author:
//   Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
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
		readonly string filename;
		readonly int line;

		//
		// Constructors
		//
		[Obsolete ("This class is obsolete.  Use System.Configuration.ConfigurationErrorsException")]
		public ConfigurationException () : this (null)
		{
			filename = null;
			line = 0;
		}

		[Obsolete ("This class is obsolete.  Use System.Configuration.ConfigurationErrorsException")]
		public ConfigurationException (string message)
			: base (message)
		{
		}

		protected ConfigurationException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			filename = info.GetString ("filename");
			line = info.GetInt32 ("line");
		}

		[Obsolete ("This class is obsolete.  Use System.Configuration.ConfigurationErrorsException")]
		public ConfigurationException (string message, Exception inner)
			: base (message, inner)
		{
		}

#if (XML_DEP)
		[Obsolete ("This class is obsolete.  Use System.Configuration.ConfigurationErrorsException")]
		public ConfigurationException (string message, XmlNode node)
			: base (message)
		{
			filename = GetXmlNodeFilename(node);
			line = GetXmlNodeLineNumber(node);
		}

		[Obsolete ("This class is obsolete.  Use System.Configuration.ConfigurationErrorsException")]
		public ConfigurationException (string message, Exception inner, XmlNode node)
			: base (message, inner)
		{
			filename = GetXmlNodeFilename (node);
			line = GetXmlNodeLineNumber (node);
		}
#endif
		[Obsolete ("This class is obsolete.  Use System.Configuration.ConfigurationErrorsException")]
		public ConfigurationException (string message, string filename, int line)
			: base (message)
		{
			this.filename = filename;
			this.line= line;
		}

		[Obsolete ("This class is obsolete.  Use System.Configuration.ConfigurationErrorsException")]
		public ConfigurationException (string message, Exception inner, string filename, int line)
			: base (message, inner)
		{
			this.filename = filename;
			this.line = line;
		}
		//
		// Properties
		//
		public virtual	string BareMessage {
			get { return base.Message; }
		}

		public virtual string Filename {
			get { return filename; }
		}

		public virtual int Line {
			get { return line; }
		}

		public override string Message {
			get {
				string msg;
				if (filename != null && filename.Length != 0) {
					if (line != 0)
						msg = BareMessage + " (" + filename + " line " + line + ")";
					else
						msg = BareMessage + " (" + filename + ")";
				} else {
					if (line != 0)
						msg = BareMessage + " (line " + line + ")";
					else
						msg = BareMessage;
				}
				return msg;
			}
		}

		//
		// Methods
		//
#if (XML_DEP)
		[Obsolete ("This class is obsolete.  Use System.Configuration.ConfigurationErrorsException")]
		public static string GetXmlNodeFilename (XmlNode node)
		{
			if (!(node is IConfigXmlNode))
				return String.Empty;

			return ((IConfigXmlNode) node).Filename;
		}

		[Obsolete ("This class is obsolete.  Use System.Configuration.ConfigurationErrorsException")]
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
