//
// System.Threading.Thread.cs
//
// Authors:
//	Zoltan Varga (vargaz@freemail.hu)
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

using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace System.Threading {

	[Serializable]
	public sealed class CompressedStack : ISerializable {
		private ArrayList _list;

		internal CompressedStack (int length)
		{
			if (length > 0)
				_list = new ArrayList (length);
		}

		internal CompressedStack (CompressedStack cs)
		{
			if ((cs != null) && (cs._list != null))
				_list = (ArrayList) cs._list.Clone ();
		}

		[ComVisibleAttribute (false)]
		public CompressedStack CreateCopy ()
		{
			return new CompressedStack (this);
		}

		public static CompressedStack Capture ()
		{
#if !FEATURE_COMPRESSEDSTACK
			throw new NotSupportedException ();
#else
			CompressedStack cs = new CompressedStack (0);
			cs._list = new ArrayList ();

			// include any current CompressedStack inside the new Capture
			CompressedStack currentCs = Thread.CurrentThread.ExecutionContext.SecurityContext.CompressedStack;
			if (currentCs != null) {
				for (int i=0; i < currentCs._list.Count; i++)
					cs._list.Add (currentCs._list [i]);
			}
			return cs;
#endif
		}

		// NOTE: This method doesn't show in the class library status page because
		// it cannot be "found" with the StrongNameIdentityPermission for ECMA key.
		// But it's there!
		[SecurityCritical]
		static public CompressedStack GetCompressedStack ()
		{
#if !FEATURE_COMPRESSEDSTACK
			throw new NotSupportedException ();
#else
			// Note: CompressedStack.GetCompressedStack doesn't return null
			// like Thread.CurrentThread.GetCompressedStack if no compressed
			// stack is present.

            CompressedStack cs = Thread.CurrentThread.ExecutionContext.SecurityContext.CompressedStack;
			if (cs == null || cs.IsEmpty ()) {
				cs = CompressedStack.Capture ();
			} else {
				cs = cs.CreateCopy ();
				// merge the existing compressed stack (from a previous Thread) with the current
				// Thread stack so we can assign "all of it" to yet another Thread
				CompressedStack newstack = CompressedStack.Capture ();
				for (int i=0; i < newstack._list.Count; i++)
					cs._list.Add (newstack._list [i]);
			}
			return cs;
#endif
		}

		[MonoTODO ("incomplete")]
		[SecurityCritical]
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
		}

		[SecurityCritical]
		static public void Run (CompressedStack compressedStack, ContextCallback callback, object state)
		{
#if !FEATURE_COMPRESSEDSTACK
			throw new NotSupportedException ();
#else	
			if (compressedStack == null)
				throw new ArgumentException ("compressedStack");

			Thread t = Thread.CurrentThread;
			CompressedStack original = null;
			try {
				original = t.ExecutionContext.SecurityContext.CompressedStack; 
				t.ExecutionContext.SecurityContext.CompressedStack = compressedStack;
				callback (state);
			}
			finally {
				if (original != null)
					t.ExecutionContext.SecurityContext.CompressedStack = original;
			}
#endif
		}

		// internal stuff
		internal bool Equals (CompressedStack cs)
		{
			if (IsEmpty ())
				return cs.IsEmpty ();
			if (cs.IsEmpty ())
				return false;
			if (_list.Count != cs._list.Count)
				return false;

			return true;
		}

		internal bool IsEmpty ()
		{
			return ((_list == null) || (_list.Count == 0));
		}

		internal IList List {
			get { return _list; }
		}
	}
}		
