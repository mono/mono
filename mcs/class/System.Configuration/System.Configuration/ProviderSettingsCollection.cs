//
// System.Web.Configuration.ProviderSettingsCollection.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Configuration;

namespace System.Configuration
{
	[ConfigurationCollection (typeof(ProviderSettings), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
	public sealed class ProviderSettingsCollection: ConfigurationElementCollection
	{
		static ConfigurationPropertyCollection props = new ConfigurationPropertyCollection ();
		
		public void Add (ProviderSettings provider)
		{
			BaseAdd (provider);
		}
		
		public void Clear ()
		{
			BaseClear ();
		}
		
		protected override ConfigurationElement CreateNewElement ()
		{
			return new ProviderSettings ();
		}
		
		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((ProviderSettings)element).Name;
		}
		
		public void Remove (string key)
		{
			BaseRemove (key);
		}
		
		public ProviderSettings this [int n]
		{
			get { return (ProviderSettings) BaseGet (n); }
			set { BaseAdd (n, value); }
		}
		
		public new ProviderSettings this [string key]
		{
			get { return (ProviderSettings) BaseGet (key); }
		}
		
		protected internal override ConfigurationPropertyCollection Properties {
			get { return props; }
		}
	}
}

