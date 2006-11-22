//
// System.Web.Services.Configuration.WsiProfilesElementCollection
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Configuration;

#if NET_2_0

namespace System.Web.Services.Configuration {

	[ConfigurationCollection (typeof (WsiProfilesElement), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
	public sealed class WsiProfilesElementCollection : ConfigurationElementCollection
	{
		public void Add (WsiProfilesElement element)
		{
			BaseAdd (element);
		}

		public void Clear ()
		{
			BaseClear ();
		}

		public bool ContainsKey (object key)
		{
			return (BaseGet (key) != null);
		}

		public void CopyTo (WsiProfilesElement[] array, int index)
		{
			((ICollection)this).CopyTo (array, index);
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return new WsiProfilesElement ();
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((WsiProfilesElement)element).Name;
		}

		public int IndexOf (WsiProfilesElement element)
		{
			return BaseIndexOf (element);
		}

		public void Remove (WsiProfilesElement element)
		{
			BaseRemove (element.Name);
		}

		public void RemoveAt (int index)
		{
			BaseRemoveAt (index);
		}

		[MonoTODO ("is this right?")]
		public void RemoveAt (object key)
		{
			BaseRemove (key);
		}

		[MonoTODO]
		public WsiProfilesElement this [int index] {
			get { return (WsiProfilesElement)BaseGet (index); }
			set { if (BaseGet (index) != null) BaseRemoveAt (index); BaseAdd (index, value); }
		}

		[MonoTODO]
		public WsiProfilesElement this [object key] {
			get { return (WsiProfilesElement)BaseGet (key); }
			set {
				WsiProfilesElement el = (WsiProfilesElement)BaseGet (key);
				if (el == null) {
					BaseAdd (value);
					return;
				}
				int index = IndexOf (el);
				BaseRemoveAt (index);
				BaseAdd (index, value);
			}
		}

	}

}

#endif

