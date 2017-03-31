//
// ConnectionConfig.cs  - Holds information on a specific connection and
// corresponding engine to test.
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

#if !NO_CONFIGURATION

using System;
using System.Collections;
using System.Configuration;
using System.Globalization;
using System.Xml;

namespace MonoTests.System.Data.Connected
{
	internal sealed class ConnectionConfig
	{
		private readonly string name;
		private readonly string factory;
		private readonly string connectionString;
		private readonly EngineConfig engine;

		private ConnectionConfig (string name, string factory, string connectionString, EngineConfig engine)
		{
			this.name = name;
			this.factory = factory;
			this.connectionString = connectionString;
			this.engine = engine;
		}

		internal static ConnectionConfig FromXml (XmlNode connNode, Hashtable engines)
		{
			return new ConnectionConfig (
				GetAttribValue (connNode, "name", true),
				GetAttribValue (connNode, "factory", true),
				GetAttribValue (connNode, "connectionString", true),
				GetEngine (connNode, engines));
		}

		public string Name {
			get { return name; }
		}

		public string Factory {
			get { return factory; }
		}

		public string ConnectionString {
			get { return connectionString; }
		}

		public EngineConfig Engine {
			get { return engine; }
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

		static EngineConfig GetEngine (XmlNode connNode, Hashtable engines)
		{
			XmlAttribute engineAttr = connNode.Attributes ["engine"];
			if (engineAttr == null)
				throw CreateAttributeMissingException ("engine", connNode);

			string engineName = engineAttr.Value;
			EngineConfig engine = (EngineConfig) engines [engineName];
			if (engine == null) {
				string msg = string.Format (CultureInfo.InvariantCulture,
					"Engine '{0}' does not exist.", engineName);
				throw new ConfigurationErrorsException (msg, engineAttr);
			}
			return engine;
		}

		static Exception CreateAttributeMissingException (string name, XmlNode node)
		{
			string msg = string.Format (CultureInfo.InvariantCulture,
				"Missing '{0}' attribute.", name);
			throw new ConfigurationErrorsException (msg, node);
		}
	}
}

#endif