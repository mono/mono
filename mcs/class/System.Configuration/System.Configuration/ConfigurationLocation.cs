//
// System.Configuration.ConfigurationLocation.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
//  Lluis Sanchez Gual (lluis@novell.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

using System.Xml;
using System.IO;

namespace System.Configuration {

	public class ConfigurationLocation
	{
		static readonly char[] pathTrimChars = { '/' };
		
		string path;
		Configuration configuration;
		Configuration parent;
		string xmlContent;
		bool parentResolved;
		bool allowOverride;
		
		internal ConfigurationLocation()
		{
		}
		
		internal ConfigurationLocation (string path, string xmlContent, Configuration parent, bool allowOverride)
		{
			if (!String.IsNullOrEmpty (path)) {
				switch (path [0]) {
					case ' ':
					case '.':
					case '/':
					case '\\':
						throw new ConfigurationErrorsException ("<location> path attribute must be a relative virtual path.  It cannot start with any of ' ' '.' '/' or '\\'.");
				}

				path = path.TrimEnd (pathTrimChars);
			}
			
			this.path = path;
			this.xmlContent = xmlContent;
			this.parent = parent;
			this.allowOverride = allowOverride;
		}
		
		public string Path {
			get { return path; }
		}
		
		internal bool AllowOverride {
			get { return allowOverride; }
		}
		
		internal string XmlContent {
			get { return xmlContent; }
		}
		
		internal Configuration OpenedConfiguration {
			get { return configuration; }
		}

		public Configuration OpenConfiguration ()
		{
			if (configuration == null) {
				if (!parentResolved) {
					Configuration parentFile = parent.GetParentWithFile ();
					if (parentFile != null) {
						string parentRelativePath = parent.ConfigHost.GetConfigPathFromLocationSubPath (parent.LocationConfigPath, path);
						parent = parentFile.FindLocationConfiguration (parentRelativePath, parent);
					}
				}
				
				configuration = new Configuration (parent, path);
				using (XmlTextReader tr = new ConfigXmlTextReader (xmlContent, path))
					configuration.ReadData (tr, allowOverride);

				xmlContent = null;
			}
			return configuration;
		}
		
		internal void SetParentConfiguration (Configuration parent)
		{
			if (parentResolved)
				return;

			parentResolved = true;
			this.parent = parent;
			if (configuration != null)
				configuration.Parent = parent;
		}
	}
}

