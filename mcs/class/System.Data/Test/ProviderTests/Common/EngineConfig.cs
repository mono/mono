//
// EngineConfig.cs  - Holds information on the capabilities and behavior of an
// RDBMS engine.
//
// Author:
//	Gert Driesen (drieseng@users.sourceforge.net
//
// Copyright (c) 2008 Gert Driesen
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
using System.Xml;

namespace MonoTests.System.Data.Connected
{
	public sealed class EngineConfig
	{
		private string name;
		private string quoteCharacter;
		private bool removesTrailingSpaces;
		private bool emptyBinaryAsNull;
		private bool supportsMicroseconds;
		private bool supportsUniqueIdentifier;
		private bool supportsDate;
		private bool supportsTime;
		private bool supportsTimestamp;
		private EngineType type;
		private int clientVersion;

		public EngineConfig ()
		{
		}

		public string Name {
			get { return name; }
		}

		/// <summary>
		/// Returns the character(s) for quoting identifiers.
		/// </summary>
		public string QuoteCharacter {
			get { return quoteCharacter; }
			set { quoteCharacter = value; }
		}

		public EngineType Type {
			get { return type; }
			set { type = value; }
		}

		public bool RemovesTrailingSpaces {
			get { return removesTrailingSpaces; }
			set { removesTrailingSpaces = value; }
		}

		public bool EmptyBinaryAsNull {
			get { return emptyBinaryAsNull; }
			set { emptyBinaryAsNull = value; }
		}

		public bool SupportsMicroseconds {
			get { return supportsMicroseconds; }
			set { supportsMicroseconds = value; }
		}

		public bool SupportsUniqueIdentifier {
			get { return supportsUniqueIdentifier; }
			set { supportsUniqueIdentifier = value; }
		}

		public bool SupportsDate {
			get { return supportsDate; }
			set { supportsDate = value; }
		}

		public bool SupportsTime {
			get { return supportsTime; }
			set { supportsTime = value; }
		}

		public bool SupportsTimestamp {
			get { return supportsTimestamp; }
			set { supportsTimestamp = value; }
		}

		public int ClientVersion {
		       get { return clientVersion; }
		       set { clientVersion = value; }
		}

		public static EngineConfig FromXml (XmlNode config)
		{
			EngineConfig engine = new EngineConfig ();
			engine.name = GetAttribValue (config, "name", true);
			engine.quoteCharacter = GetAttribValue (config, "quoteCharacter", true);
			engine.removesTrailingSpaces = ParseBoolean (config, "removesTrailingSpaces", false, true);
			engine.emptyBinaryAsNull = ParseBoolean (config, "emptyBinaryAsNull", false, true);
			engine.supportsMicroseconds = ParseBoolean (config, "supportsMicroseconds", false, true);
			engine.supportsUniqueIdentifier = ParseBoolean (config, "supportsUniqueIdentifier", false, true);
			engine.supportsDate = ParseBoolean (config, "supportsDate", false, true);
			engine.supportsTime = ParseBoolean (config, "supportsTime", false, true);
			engine.supportsTimestamp = ParseBoolean (config, "supportsTimestamp", false, true);
			engine.type = ParseEngineType (config, "type");
			engine.clientVersion = ParseClientVersion (config, "clientversion");
			return engine;
		}

		static string GetAttribValue (XmlNode node, string name, bool required)
		{
			XmlAttribute attr = node.Attributes [name];
			if (attr == null) {
				if (required)
					throw CreateAttributeMissingException (name, node);
				return null;
			}
			return attr.Value;
		}

		static bool ParseBoolean (XmlNode config, string attrName, bool required, bool defaultValue)
		{
			XmlAttribute attr = config.Attributes [attrName];
			if (attr == null) {
				if (required)
					throw CreateAttributeMissingException (attrName, config);
				return defaultValue;
			}

			string value = attr.Value;

			try {
				return bool.Parse (value);
			} catch (Exception ex) {
				throw CreateInvalidValueException (attrName,
					value, attr, ex);
			}
		}

		static EngineType ParseEngineType (XmlNode config, string attrName)
		{
			XmlAttribute attr = config.Attributes [attrName];
			if (attr == null)
				throw CreateAttributeMissingException (attrName, config);

			string value = attr.Value;

			try {
				return (EngineType) Enum.Parse (typeof (EngineType), value);
			} catch (Exception ex) {
				throw CreateInvalidValueException (attrName,
					value, attr, ex);
			}
		}

		static int ParseClientVersion (XmlNode config, string attrName)
		{
			XmlAttribute attr = config.Attributes [attrName];
			if (attr == null)
				return -1;

			string value = attr.Value;

			try {
				return Int32.Parse (value);
			} catch (Exception ex) {
				throw CreateInvalidValueException (attrName,
					value, attr, ex);
			}			
		}

		static Exception CreateInvalidValueException (string name, string value, XmlNode node, Exception cause)
		{
			string msg = string.Format (CultureInfo.InvariantCulture,
					"Invalid value '{0}' for attribute {1}.",
					value, name);
			throw new ArgumentOutOfRangeException (msg, cause);
		}

		static Exception CreateAttributeMissingException (string name, XmlNode node)
		{
			string msg = string.Format (CultureInfo.InvariantCulture,
				"Missing '{0}' attribute.", name);
			throw new ArgumentException (msg);
		}
	}
}
