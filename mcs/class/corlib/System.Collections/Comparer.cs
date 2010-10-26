//
// System.Collections.Comparer.cs
//
// Authors:
//	Sergey Chaban (serge@wildwestsoftware.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
#if !DISABLE_SECURITY
using System.Security.Permissions;
#endif

namespace System.Collections
{
#if NET_2_0
	[ComVisible(true)]
#endif
	[Serializable]
#if INSIDE_CORLIB
	public
#else
	internal
#endif
	sealed class Comparer : IComparer, ISerializable {

		public static readonly Comparer Default = new Comparer ();
#if NET_1_1
		public
#else
		internal
#endif
		static readonly Comparer DefaultInvariant = new Comparer (CultureInfo.InvariantCulture);

		// This field was introduced for MS kompatibility. see bug #77701
		CompareInfo m_compareInfo;

		private Comparer ()
		{
			//LAMESPEC: This seems to be encoded at runtime while CaseInsensitiveComparer does at creation
		}

#if NET_1_1
		public
#else
		internal
#endif
		Comparer (CultureInfo culture)
		{
			if (culture == null)
				throw new ArgumentNullException ("culture");

			m_compareInfo = culture.CompareInfo;
		}


		// IComparer
		public int Compare (object a, object b)
		{
			if (a == b)
				return 0;
			else if (a == null)
				return -1;
			else if (b == null)
				return 1;

			if (m_compareInfo != null) {
				string sa = a as string;
				string sb = b as string;
				if (sa != null && sb != null)
					return m_compareInfo.Compare (sa, sb);
			}

			if (a is IComparable)
				return (a as IComparable).CompareTo (b);
			else if (b is IComparable)
				return -(b as IComparable).CompareTo (a);

			throw new ArgumentException (Locale.GetText ("Neither 'a' nor 'b' implements IComparable."));
		}

		// ISerializable
		#if !DISABLE_SECURITY
		[SecurityPermission (SecurityAction.LinkDemand, SerializationFormatter = true)]
		#endif
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			info.AddValue ("CompareInfo", m_compareInfo, typeof (CompareInfo));
		}
	}
}
