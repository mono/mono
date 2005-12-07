//
// System.Web.Profile.ProfileInfoCollection.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

namespace System.Web.Profile
{
	[Serializable]
	public sealed class ProfileInfoCollection : ICollection, IEnumerable
	{
		public ProfileInfoCollection()
		{
			list = new ArrayList ();
		}

		public void Add (ProfileInfo profileInfo)
		{
			if (readOnly)
				throw new NotSupportedException ();

			list.Add (profileInfo);
		}

		public void Clear ()
		{
			if (readOnly)
				throw new NotSupportedException ();

			list.Clear ();
		}

		public void CopyTo (System.Array array, int index)
		{
			list.CopyTo (array, index);
		}

		public void CopyTo (ProfileInfo[ ] array, int index)
		{
			list.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		public void Remove (string name)
		{
			if (readOnly)
				throw new NotSupportedException ();

			for (int i = 0; i < list.Count; i ++) {
				ProfileInfo info = (ProfileInfo)list[i];
				if (info.UserName == name) {
					list.Remove (i);
					break;
				}
			}
		}

		public void SetReadOnly ()
		{
			readOnly = true;
		}

		public int Count {
			get {
				return list.Count;
			}
		}

		public bool IsSynchronized {
			get {
				return false;
			}
		}

		public object SyncRoot {
			get {
				return this;
			}
		}

		public ProfileInfo this [string name] {
			get {
				for (int i = 0; i < list.Count; i ++) {
					ProfileInfo info = (ProfileInfo)list[i];
					if (info.UserName == name) {
						return info;
					}
				}

				return null;
			}
		}

		ArrayList list;
		bool readOnly;
	}

}
#endif
