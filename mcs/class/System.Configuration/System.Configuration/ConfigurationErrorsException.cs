//
// System.Configuration.ConfigurationErrorsException.cs
//
// Authors:
// 	Duncan Mak (duncan@ximian.com)
// 	Chris Toshok (toshok@ximian.com)
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Configuration.Internal;
using System.Globalization;
using System.Runtime.Serialization;
using System.Collections;
using System.Xml;

namespace System.Configuration 
{
/* disable the obsolete warnings about ConfigurationException */
#pragma warning disable 618

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
			filename = info.GetString ("ConfigurationErrors_Filename");
			line = info.GetInt32 ("ConfigurationErrors_Line");
		}

		public ConfigurationErrorsException (string message, Exception inner)
			: base (message, inner)
		{
		}

		public ConfigurationErrorsException (string message, XmlNode node)
			: this (message, null, GetFilename (node), GetLineNumber (node))
		{
		}

		public ConfigurationErrorsException (string message, Exception inner, XmlNode node)
			: this (message, inner, GetFilename (node), GetLineNumber (node))
		{
		}
		
		public ConfigurationErrorsException (string message, XmlReader reader)
			: this (message, null, GetFilename (reader), GetLineNumber (reader))
		{
		}

		public ConfigurationErrorsException (string message, Exception inner, XmlReader reader)
			: this (message, inner, GetFilename (reader), GetLineNumber (reader))
		{
		}
		
		public ConfigurationErrorsException (string message, string filename, int line)
			: this (message, null, filename, line)
		{
		}

		public ConfigurationErrorsException (string message, Exception inner, string filename, int line)
			: base (message, inner)
		{
			this.filename = filename;
			this.line = line;
		}
		
		//
		// Properties
		//
		public override string BareMessage {
			get  { return base.BareMessage; }
		}

		public ICollection Errors {
			get { throw new NotImplementedException (); }
		}

		public override string Filename {
			get { return filename; }
		}
		
		public override int Line {
			get { return line; }
		}

		public override string Message {
			get {
				string msg;
				if (!String.IsNullOrEmpty (filename)) {
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
		public static string GetFilename (XmlReader reader)
		{
			// FIXME: eliminate this silly compatibility.
			if (reader is IConfigErrorInfo)
				return ((IConfigErrorInfo) reader).Filename;

			return reader != null ? reader.BaseURI : null;
		}

		public static int GetLineNumber (XmlReader reader)
		{
			// FIXME: eliminate this silly compatibility.
			if (reader is IConfigErrorInfo)
				return ((IConfigErrorInfo) reader).LineNumber;

			IXmlLineInfo li = reader as IXmlLineInfo;
			return li != null ? li.LineNumber : 0;
		}

		public static string GetFilename (XmlNode node)
		{
			if (!(node is IConfigErrorInfo))
				return null;

			return ((IConfigErrorInfo) node).Filename;
		}

		public static int GetLineNumber (XmlNode node)
		{
			if (!(node is IConfigErrorInfo))
				return 0;

			return ((IConfigErrorInfo) node).LineNumber;
		}
		
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("ConfigurationErrors_Filename", filename);
			info.AddValue ("ConfigurationErrors_Line", line);
		}

		readonly string filename;
		readonly int line;
	}
#pragma warning restore
}

