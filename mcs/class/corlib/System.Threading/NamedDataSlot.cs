//
// NamedDataSlot.cs:
//
// Authors:
//   Dick Porter (dick@ximian.com)
//   Marek Safar (marek.safar@gmail.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
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

using System.Collections.Generic;

namespace System.Threading
{
	sealed class NamedDataSlot
	{
		// Stores a hash keyed by strings of LocalDataStoreSlot objects
		Dictionary<string, LocalDataStoreSlot> datastorehash;

		public LocalDataStoreSlot Allocate (string name)
		{		
			lock (this) {
				if (datastorehash == null)
					datastorehash = new Dictionary<string, LocalDataStoreSlot> ();

				if (datastorehash.ContainsKey (name)) {
					// This exception isnt documented (of
					// course) but .net throws it
					throw new ArgumentException ("Named data slot already added");
				}
			
				var slot = new LocalDataStoreSlot (true);
				datastorehash.Add (name, slot);
				return slot;
			}
		}

		public LocalDataStoreSlot Get (string name)
		{
			lock (this) {
				if (datastorehash == null)
					datastorehash = new Dictionary<string, LocalDataStoreSlot> ();

				LocalDataStoreSlot slot;
				if (!datastorehash.TryGetValue (name, out slot)) {
					slot = new LocalDataStoreSlot (true);
				}
			
				return slot;
			}
		}		

		public void Free (string name)
		{			
			lock (this) {
				if (datastorehash == null)
					datastorehash = new Dictionary<string, LocalDataStoreSlot> ();

				if (datastorehash.ContainsKey (name)) {
					datastorehash.Remove (name);
				}
			}
		}
	}
}