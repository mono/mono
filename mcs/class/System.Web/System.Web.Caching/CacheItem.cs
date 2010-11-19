//
// System.Web.Caching.CacheItem
//
// Author(s):
//  Lluis Sanchez <lluis@ximian.com>
//
// (C) 2005-2009 Novell, Inc (http://novell.com)
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
using System.Threading;

namespace System.Web.Caching
{
	class CacheItem
	{
		public object Value;
		public string Key;
		public CacheDependency Dependency;
		public DateTime AbsoluteExpiration;
		public TimeSpan SlidingExpiration;
		public CacheItemPriority Priority;
		public CacheItemRemovedCallback OnRemoveCallback;
		public CacheItemUpdateCallback OnUpdateCallback;
		public DateTime LastChange;
		public long ExpiresAt;
		public bool Disabled;
		public bool IsTimedItem;
#if DEBUG
		public Guid Guid;

		public CacheItem ()
		{
			Guid = Guid.NewGuid ();
		}

		public override string ToString ()
		{
			return String.Format ("CacheItem [{0}]\n[{1}][{2}][{3}]", this.Guid, Key, Disabled, ExpiresAt > 0 ? new DateTime (ExpiresAt).ToString () : "0");
		}
#endif
	}
}
