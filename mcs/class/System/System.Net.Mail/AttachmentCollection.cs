//
// System.Net.Mail.AttachmentCollection.cs
//
// Author:
//	Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004
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
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace System.Net.Mail {
	public sealed class AttachmentCollection : Collection<Attachment>, IDisposable
	{
		internal AttachmentCollection ()
		{
		}
		
		public void Dispose ()
		{
			for (int i = 0; i < Count; i += 1)
				this [i].Dispose ();
		}

		protected override void ClearItems ()
		{
			base.ClearItems ();
		}

		protected override void InsertItem (int index, Attachment item)
		{
			base.InsertItem (index, item);
		}

		protected override void RemoveItem (int index)
		{
			base.RemoveItem (index);
		}

		protected override void SetItem (int index, Attachment item)
		{
			base.SetItem (index, item);
		}
	}
}

