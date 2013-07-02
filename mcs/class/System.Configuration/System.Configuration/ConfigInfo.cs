//
// System.Configuration.ConfigInfo.cs
//
// Authors:
//	Lluis Sanchez (lluis@novell.com)
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

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.IO;
using System.Text;
using System.Configuration.Internal;

namespace System.Configuration {

	internal abstract class ConfigInfo
	{
		public string Name;
		public string TypeName;
		protected Type Type;
		string streamName;
		public ConfigInfo Parent;
		public IInternalConfigHost ConfigHost;
		
		public virtual object CreateInstance ()
		{
			if (Type == null) Type = ConfigHost.GetConfigType (TypeName, true);
			return Activator.CreateInstance (Type, true);
		}
		
		public string XPath {
			get {
				StringBuilder path = new StringBuilder (Name);
				ConfigInfo cinfo = Parent;
				while (cinfo != null) {
					path.Insert (0, cinfo.Name + "/");
					cinfo = cinfo.Parent;
				}
				return path.ToString ();
			}
		}
		
		public string StreamName {
			get { return streamName; }
			set { streamName = value; }
		}
		
		public abstract bool HasConfigContent (Configuration cfg);
		public abstract bool HasDataContent (Configuration cfg);
		
		protected void ThrowException (string text, XmlReader reader)
		{
//			IXmlLineInfo li = reader as IXmlLineInfo;
			throw new ConfigurationErrorsException (text, reader);
		}
		
		public abstract void ReadConfig (Configuration cfg, string streamName, XmlReader reader);
		public abstract void WriteConfig (Configuration cfg, XmlWriter writer, ConfigurationSaveMode mode);
		public abstract void ReadData (Configuration config, XmlReader reader, bool overrideAllowed);
		public abstract void WriteData (Configuration config, XmlWriter writer, ConfigurationSaveMode mode);
		
		internal abstract void Merge (ConfigInfo data);

		internal abstract bool HasValues (Configuration config, ConfigurationSaveMode mode);
		internal abstract void ResetModified (Configuration config);
	}
}
