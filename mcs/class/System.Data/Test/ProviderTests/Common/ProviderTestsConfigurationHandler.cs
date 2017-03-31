//
// ProviderTestsConfigurationHandler.cs  - Provides access to configuration info
// for the connected System.Data tests.
//
// Author:
//      Gert Driesen (drieseng@users.sourceforge.net)
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
using System.Text;
using System.Xml;

namespace MonoTests.System.Data.Connected
{
	class ProviderTestsConfigurationHandler : IConfigurationSectionHandler
	{
		public object Create (object parent, object configContext, XmlNode section)
		{
			Hashtable engines = new Hashtable ();

			foreach (XmlNode engineNode in section.SelectNodes ("engines/engine")) {
				EngineConfig engine = EngineConfig.FromXml (engineNode);
				if (engines.Contains (engine.Name)) {
					string msg = string.Format (CultureInfo.InvariantCulture,
						"A engine with name '{0}' already exists.",
						engine.Name);
					throw new ConfigurationErrorsException (msg, engineNode);
				}
				engines.Add (engine.Name, engine);
			}

			Hashtable connections = new Hashtable ();

			foreach (XmlNode connNode in section.SelectNodes ("connections/connection")) {
				ConnectionConfig conn = ConnectionConfig.FromXml (connNode, engines);
				if (connections.Contains (conn.Name)) {
					string msg = string.Format (CultureInfo.InvariantCulture,
						"A connection with name '{0}' already exists.",
						conn.Name);
					throw new ConfigurationErrorsException (msg, connNode);
				}
				connections.Add (conn.Name, conn);
			}

			ConnectionConfig [] c = new ConnectionConfig [connections.Count];
			connections.Values.CopyTo (c, 0);
			return c;
		}
	}
}

#endif