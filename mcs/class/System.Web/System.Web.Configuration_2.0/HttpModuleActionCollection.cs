//
// System.Web.Configuration.HttpModuleActionCollection
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


using System;
using System.Configuration;

namespace System.Web.Configuration
{
	[ConfigurationCollection (typeof (HttpModuleAction), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
	public sealed class HttpModuleActionCollection : ConfigurationElementCollection
	{
		static ConfigurationPropertyCollection properties;

		static HttpModuleActionCollection ()
		{
			properties = new ConfigurationPropertyCollection ();
		}

		public HttpModuleActionCollection ()
		{
		}
			
		public void Add (HttpModuleAction httpModule)
		{
			BaseAdd (httpModule);
		}

		public void Clear ()
		{
			BaseClear ();
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return new HttpModuleAction ();
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((HttpModuleAction)element).Name;
		}

		public int IndexOf (HttpModuleAction action)
		{
			return BaseIndexOf (action);
		}

		public void Remove (string name)
		{
			BaseRemove (name);
		}

		public void Remove (HttpModuleAction action)
		{
			BaseRemove (action.Name);
		}

		public void RemoveAt (int index)
		{
			BaseRemoveAt (index);
		}

		protected override bool IsElementRemovable (ConfigurationElement element)
		{
			return base.IsElementRemovable (element);
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		public HttpModuleAction this[int index] {
			get { return (HttpModuleAction)BaseGet (index); }
			set { if (BaseGet (index) != null) BaseRemoveAt (index); BaseAdd (index, value); }
		}
	}
}

