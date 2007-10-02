//
// System.Windows.Forms.Design.Behavior.GlyphCollection
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2007 Novell, Inc.
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

using System.Collections;

namespace System.Windows.Forms.Design.Behavior
{
	public class GlyphCollection : CollectionBase
	{
		public GlyphCollection ()
		{
		}

		public GlyphCollection (Glyph [] value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			InnerList.AddRange (value);
		}

		public GlyphCollection (GlyphCollection value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			InnerList.AddRange (value);
		}

		public Glyph this [int index] {
			get { return (Glyph) InnerList [index]; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				InnerList [index] = value;
			}
		}

		public int Add (Glyph value)
		{
			return InnerList.Add (value);
		}

		public void AddRange (Glyph [] value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			InnerList.AddRange (value);
		}

		public void AddRange (GlyphCollection value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			InnerList.AddRange (value);
		}

		public bool Contains (Glyph value)
		{
			return InnerList.Contains (value);
		}

		public void CopyTo (Glyph [] array, int index)
		{
			InnerList.CopyTo (array, index);
		}

		public int IndexOf (Glyph value)
		{
			return InnerList.IndexOf (value);
		}

		public void Insert (int index, Glyph value)
		{
			InnerList.Insert (index, value);
		}

		public void Remove (Glyph value)
		{
			InnerList.Remove (value);
		}
	}
}

#endif
