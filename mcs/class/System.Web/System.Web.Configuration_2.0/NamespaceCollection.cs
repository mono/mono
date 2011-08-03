//
// System.Web.Configuration.NamespaceCollection
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.ComponentModel;
using System.Configuration;
using System.Web.UI;
using System.Xml;

namespace System.Web.Configuration
{
	[ConfigurationCollection (typeof (NamespaceInfo), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
	public sealed class NamespaceCollection : ConfigurationElementCollection
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty autoImportVBNamespaceProp;

		static NamespaceCollection ()
		{
			autoImportVBNamespaceProp = new ConfigurationProperty ("autoImportVBNamespace", typeof (bool), true);
			properties = new ConfigurationPropertyCollection ();
			properties.Add (autoImportVBNamespaceProp);
		}

		public NamespaceCollection ()
		{
		}

		public void Add (NamespaceInfo namespaceInformation)
		{
			BaseAdd (namespaceInformation);
		}

		public void Clear ()
		{
			BaseClear ();
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return new NamespaceInfo (null);
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((NamespaceInfo)element).Namespace;
		}

		public void Remove (string s)
		{
			BaseRemove (s);
		}

		public void RemoveAt (int index)
		{
			BaseRemoveAt (index);
		}

		[ConfigurationProperty ("autoImportVBNamespace", DefaultValue = true)]
		public bool AutoImportVBNamespace {
			get { return (bool) base[autoImportVBNamespaceProp]; }
			set { base[autoImportVBNamespaceProp] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		public NamespaceInfo this[int index] {
			get { return (NamespaceInfo) BaseGet (index); }
			set { if (BaseGet (index) != null) BaseRemoveAt (index); BaseAdd (index, value); }
		}

	}

}

#endif
