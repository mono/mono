//
// System.Web.Configuration.HttpHandlerActionCollection
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
using System.Configuration;

namespace System.Web.Configuration
{
	[ConfigurationCollection (typeof (HttpHandlerAction), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMapAlternate)]
	public sealed class HttpHandlerActionCollection : ConfigurationElementCollection
	{
		static ConfigurationPropertyCollection properties;

		static HttpHandlerActionCollection ()
		{
			properties = new ConfigurationPropertyCollection ();
		}

		public HttpHandlerActionCollection ()
		{
		}
			
		public void Add (HttpHandlerAction httpHandlerAction)
		{
			HttpApplication.ClearHandlerCache ();
			BaseAdd (httpHandlerAction);
		}

		public void Clear ()
		{
			HttpApplication.ClearHandlerCache ();
			BaseClear ();
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return new HttpHandlerAction ();
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((HttpHandlerAction)element).Path + "-" + ((HttpHandlerAction)element).Verb;
		}

		public int IndexOf (HttpHandlerAction action)
		{
			return BaseIndexOf (action);
		}

		public void Remove (string verb, string path)
		{
			HttpApplication.ClearHandlerCache ();
			BaseRemove (path + "-" + verb);
		}

		public void Remove (HttpHandlerAction action)
		{
			HttpApplication.ClearHandlerCache ();
			BaseRemove (action.Path + "-" + action.Verb);
		}

		public void RemoveAt (int index)
		{
			HttpApplication.ClearHandlerCache ();
			BaseRemoveAt (index);
		}

		public override ConfigurationElementCollectionType CollectionType {
			get { return ConfigurationElementCollectionType.AddRemoveClearMapAlternate; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		public HttpHandlerAction this[int index] {
			get { return (HttpHandlerAction)BaseGet (index); }
			set { if (BaseGet (index) != null) BaseRemoveAt (index); BaseAdd (index, value); }
		}

		protected override bool ThrowOnDuplicate {
			get { return false; }
		}
	}
}

#endif
