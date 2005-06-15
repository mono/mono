//
// System.Configuration.ConfigurationErrorsException.cs
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
using System.Xml;

namespace System.Configuration 
{
	[Serializable]
	public class ConfigurationErrorsException : ConfigurationException
	{
		//
		// Constructors
		//
		public ConfigurationErrorsException ()
		{
		}
		
		public ConfigurationErrorsException (string message)
			: base (message)
		{
		}

		protected ConfigurationErrorsException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public ConfigurationErrorsException (string message, Exception inner)
			: base (message, inner)
		{
		}

		public ConfigurationErrorsException (string message, XmlNode node)
			: base (message, GetFilename (node), GetLineNumber (node))
		{
		}

		public ConfigurationErrorsException (string message, Exception inner, XmlNode node)
			: base (message, inner, GetFilename (node), GetLineNumber (node))
		{
		}
		
		public ConfigurationErrorsException (string message, XmlReader reader)
			: base (message, GetFilename (reader), GetLineNumber (reader))
		{
		}

		public ConfigurationErrorsException (string message, Exception inner, XmlReader reader)
			: base (message, inner, GetFilename (reader), GetLineNumber (reader))
		{
		}
		
		public ConfigurationErrorsException (string message, string filename, int line)
			: base (message, filename, line)
		{
		}

		public ConfigurationErrorsException (string message, Exception inner, string filename, int line)
			: base (message, inner, filename, line)
		{
		}
		
		//
		// Properties
		//
/*		public override string BareMessage
		{
			get  { return bareMessage; }
		}

		public override string Filename
		{
			get { return filename; }
		}
		
		public override int Line
		{
			get { return line; }
		}

		public override string Message
		{
			get {
				string baseMsg = base.Message;
				string f = (filename == null) ? String.Empty : filename;
				string l = (line == 0) ? String.Empty : (" line " + line);

				return baseMsg + " (" + f + l + ")";
			}
		}
*/
		//
		// Methods
		//
		public static string GetFilename (XmlReader reader)
		{
			if (reader is XmlTextReader)
				return ((XmlTextReader)reader).BaseURI;
			else
				return String.Empty;
		}

		public static int GetLineNumber (XmlReader reader)
		{
			if (reader is XmlTextReader)
				return ((XmlTextReader)reader).LineNumber;
			else
				return 0;
		}

		static string GetFilename (XmlNode node)
		{
			if (!(node is IConfigXmlNode))
				return String.Empty;

			return ((IConfigXmlNode) node).Filename;
		}

		static int GetLineNumber (XmlNode node)
		{
			if (!(node is IConfigXmlNode))
				return 0;

			return ((IConfigXmlNode) node).LineNumber;
		}
		
/*		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("filename", filename);
			info.AddValue ("line", line);
		}
		*/
	}
}
