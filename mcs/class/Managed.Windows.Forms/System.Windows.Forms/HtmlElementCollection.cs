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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Andreia Gaita	<avidigal@novell.com>

#if NET_2_0

using System;
using System.Collections;

namespace System.Windows.Forms
{
	[MonoTODO ("Needs Implementation")]
	public sealed class HtmlElementCollection: ICollection, IEnumerable
	{
		[MonoTODO ("Needs Implementation")]
		public int Count
		{
			get { return 0; }
		}

		[MonoTODO ("Needs Implementation")]
		public HtmlElement this[string elementId]
		{
			get { return null; }
		}

		[MonoTODO ("Needs Implementation")]
		public HtmlElement this[int index]
		{
			get { return null; }
		}

		[MonoTODO ("Needs Implementation")]
		public HtmlElementCollection GetElementsByName (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Needs Implementation")]
		public IEnumerator GetEnumerator ()
		{
			return null;
		}

		void ICollection.CopyTo (Array dest, int index)
		{
			throw new NotImplementedException ();
		}

		object ICollection.SyncRoot
		{
			get { return this; }
		}

		bool ICollection.IsSynchronized
		{
			get { return false; }
		}


	}
}

#endif