//
// System.LocalDataStoreSlot.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System 
{
#if NET_2_0
	[ComVisible (true)]
#endif
	public sealed class LocalDataStoreSlot
	{
		internal int slot;
		internal bool thread_local; // false for context-local

		static object lock_obj = new object ();
		static bool[] slot_bitmap_thread;
		static bool[] slot_bitmap_context;

		internal LocalDataStoreSlot (bool in_thread)
		{
			thread_local = in_thread;
			lock (lock_obj) {
				int i;
				bool[] slot_bitmap;
				if (in_thread)
					slot_bitmap = slot_bitmap_thread;
				else
					slot_bitmap = slot_bitmap_context;
				if (slot_bitmap != null) {
					for (i = 0; i < slot_bitmap.Length; ++i) {
						if (!slot_bitmap [i]) {
							slot = i;
							slot_bitmap [i] = true;
							return;
						}
					}
					bool[] new_bitmap = new bool [i + 2];
					slot_bitmap.CopyTo (new_bitmap, 0);
					slot_bitmap = new_bitmap;
				} else {
					slot_bitmap = new bool [2];
					i = 0;
				}
				slot_bitmap [i] = true;
				slot = i;
				if (in_thread)
					slot_bitmap_thread = slot_bitmap;
				else
					slot_bitmap_context = slot_bitmap;
			}
		}

		~LocalDataStoreSlot ()
		{
			/* first remove all the values from the slots and
			 * then free the slot itself for reuse.
			 */
			System.Threading.Thread.FreeLocalSlotValues (slot, thread_local);
			lock (lock_obj) {
				if (thread_local)
					slot_bitmap_thread [slot] = false;
				else
					slot_bitmap_context [slot] = false;
			}
		}
	}
}
