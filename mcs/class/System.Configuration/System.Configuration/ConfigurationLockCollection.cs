//
// System.Configuration.ConfigurationLockCollection.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
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
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

#if NET_2_0

using System;
using System.Collections;

namespace System.Configuration 
{
	[Flags]
	internal enum ConfigurationLockType
	{
		Attribute = 0x01,
		Element = 0x02,

		Exclude = 0x10
	}

	public sealed class ConfigurationLockCollection : ICollection, IEnumerable
	{
		ArrayList names;
		ConfigurationElement element;
		ConfigurationLockType lockType;

		internal ConfigurationLockCollection (ConfigurationElement element,
						      ConfigurationLockType lockType)
		{
			names = new ArrayList ();
			this.element = element;
			this.lockType = lockType;

			Populate();
		}

		void Populate ()
		{
			foreach (ConfigurationProperty prop in element.Properties) {
				if ((lockType & ConfigurationLockType.Attribute) != ConfigurationLockType.Attribute
				    || !prop.IsDefaultCollection)
					Add (prop.Name);
			}
		}

		public void Add (string name)
		{
			names.Add (name);
		}

		public void Clear ()
		{
			names.Clear ();
		}

		public bool Contains (string name)
		{
			return names.Contains (name);
		}

		public void CopyTo (string[] array, int index)
		{
			names.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator()
		{
			return names.GetEnumerator ();
		}

		[MonoTODO]
		public bool IsReadOnly (string name)
		{
			return false; /* XXX */
		}

		public void Remove (string name)
		{
			names.Remove (name);
		}

		[MonoTODO]
		public void SetFromList (string attributeList)
		{
			throw new NotImplementedException ();
		}

		void ICollection.CopyTo (System.Array array, int index)
		{
			names.CopyTo (array, index);
		}

		[MonoTODO]
		public string AttributeList {
			get {
				throw new NotImplementedException ();
			}
		}

		public int Count {
			get {
				return names.Count;
			}
		}

		[MonoTODO]
		public bool HasParentElements {
			get {
				return false; /* XXX */
			}
		}

		[MonoTODO]
		public bool IsModified {
			get {
				return false; /* XXX */
			}
		}

		[MonoTODO]
		public bool IsSynchronized {
			get {
				return false; /* XXX */
			}
		}

		[MonoTODO]
		public object SyncRoot {
			get {
				return this; /* XXX */
			}
		}
	}
}

#endif
