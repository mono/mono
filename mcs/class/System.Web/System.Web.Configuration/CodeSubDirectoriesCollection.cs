//
// System.Web.Configuration.CodeSubDirectoriesCollection
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) Copyright 2005 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Configuration;

namespace System.Web.Configuration
{
	public sealed class CodeSubDirectoriesCollection : ConfigurationElementCollection
	{
		static ConfigurationPropertyCollection props;

		static CodeSubDirectoriesCollection ()
		{
			//FIXME: add properties
			props = new ConfigurationPropertyCollection ();
		}

		public CodeSubDirectory this [int index] {
			get { return (CodeSubDirectory) BaseGet (index); }
			set {
				if (BaseGet (index) != null)
					BaseRemoveAt (index);

				BaseAdd (index, value);
			}
		}

		protected override ConfigurationElementCollectionType CollectionType {
			get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
		}

		protected override string ElementName {
			get { return "add"; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return props; }
		}

		public void Add (CodeSubDirectory codeSubDirectory)
		{
			BaseAdd (codeSubDirectory);
		}

		protected override bool CompareKeys (object key1, object key2)
		{
			return (0 == CaseInsensitiveComparer.DefaultInvariant.Compare ((string) key1, (string) key2));
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return new CodeSubDirectory ();
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			CodeSubDirectory sd = (CodeSubDirectory) element;
			return sd.DirectoryName;
		}
	}
}
#endif // NET_2_0

