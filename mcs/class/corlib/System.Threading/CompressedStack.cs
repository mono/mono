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
			CompressedStack cs = new CompressedStack (0);
			cs._list = SecurityFrame.GetStack (1);

			// include any current CompressedStack inside the new Capture
			CompressedStack currentCs = Thread.CurrentThread.GetCompressedStack ();
			if (currentCs != null) {
				for (int i=0; i < currentCs._list.Count; i++)
					cs._list.Add (currentCs._list [i]);
			}
			return cs;
		}

		// NOTE: This method doesn't show in the class library status page because
		// it cannot be "found" with the StrongNameIdentityPermission for ECMA key.
		// But it's there!
#if NET_4_0
		[SecurityCritical]
#else
		[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
		[StrongNameIdentityPermission (SecurityAction.LinkDemand, PublicKey="00000000000000000400000000000000")]
#endif
		static public CompressedStack GetCompressedStack ()
		{
			// Note: CompressedStack.GetCompressedStack doesn't return null
			// like Thread.CurrentThread.GetCompressedStack if no compressed
			// stack is present.
			CompressedStack cs = Thread.CurrentThread.GetCompressedStack ();
			if (cs == null) {
				cs = CompressedStack.Capture ();
			} else {
				// merge the existing compressed stack (from a previous Thread) with the current
				// Thread stack so we can assign "all of it" to yet another Thread
				CompressedStack newstack = CompressedStack.Capture ();
				for (int i=0; i < newstack._list.Count; i++)
					cs._list.Add (newstack._list [i]);
			}
			return cs;
		}

		[MonoTODO ("incomplete")]
#if NET_4_0
		[SecurityCritical]
#else
		[ReflectionPermission (SecurityAction.Demand, MemberAccess = true)]
#endif
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
		}

#if NET_4_0
		[SecurityCritical]
#else
		[SecurityPermission (SecurityAction.LinkDemand, Infrastructure = true)]
#endif
		static public void Run (CompressedStack compressedStack, ContextCallback callback, object state)
		{
			if (compressedStack == null)
				throw new ArgumentException ("compressedStack");

			Thread t = Thread.CurrentThread;
			CompressedStack original = null;
			try {
				original = t.GetCompressedStack (); 
				t.SetCompressedStack (compressedStack);
				callback (state);
			}
			finally {
				if (original != null)
					t.SetCompressedStack (original);
			}
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

			for (int i=0; i < _list.Count; i++) {
				SecurityFrame sf1 = (SecurityFrame) _list [i];
				SecurityFrame sf2 = (SecurityFrame) cs._list [i];
				if (!sf1.Equals (sf2))
					return false;
			}
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
