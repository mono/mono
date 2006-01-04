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

		[MonoTODO]
		public bool ContainsKey (object key)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (WsiProfilesElement[] array, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override ConfigurationElement CreateNewElement ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override object GetElementKey (ConfigurationElement element)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int IndexOf (WsiProfilesElement element)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove (WsiProfilesElement element)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveAt (int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveAt (object key)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public WsiProfilesElement this [int index] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public WsiProfilesElement this [object key] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

	}

}

#endif

