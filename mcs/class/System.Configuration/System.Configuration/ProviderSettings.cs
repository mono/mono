//
// System.Configuration.ProviderSettings.cs
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

#if NET_2_0

using System;
using System.Xml;
using System.Collections.Specialized;

namespace System.Configuration
{
	public sealed class ProviderSettings: ConfigurationElement
	{
		ConfigNameValueCollection parameters;
		
		public ProviderSettings ()
		{
		}
		
		public ProviderSettings (string name, string type)
		{
			Name = name;
			Type = type;
		}
		
		protected override bool OnDeserializeUnrecognizedAttribute (string name, string value)
		{
			if (parameters == null)
				parameters = new ConfigNameValueCollection ();
			parameters [name] = value;
			parameters.ResetModified ();
			return true;
		}

		protected internal override bool IsModified ()
		{
			return (parameters != null && parameters.IsModified) || base.IsModified ();
		}

		protected internal override void Reset (ConfigurationElement parentElement)
		{
			base.Reset (parentElement);

			ProviderSettings sec = parentElement as ProviderSettings;
			if (sec != null && sec.parameters != null)
				parameters = new ConfigNameValueCollection (sec.parameters);
			else
				parameters = null;
		}

		[MonoTODO]
		protected internal override void Unmerge (
				ConfigurationElement source, ConfigurationElement parent,
				ConfigurationSaveMode updateMode)
		{
			base.Unmerge (source, parent, updateMode);
		}
		
		[ConfigurationProperty ("name", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string Name {
			get { return (string) this ["name"]; }
			set { this ["name"] = value; }
		}
		
		[ConfigurationProperty ("type", Options = ConfigurationPropertyOptions.IsRequired)]
		public string Type {
			get { return (string) this ["type"]; }
			set { this ["type"] = value; }
		}

		[MonoTODO]
		protected internal override ConfigurationPropertyCollection Properties {
			get {
				throw new NotImplementedException ();
			}
		}
		
		public NameValueCollection Parameters {
			get {
				if (parameters == null)
					parameters = new ConfigNameValueCollection ();
				return parameters;
			}
		}
	}
}

#endif
