//
// System.Threading.Thread.cs
//
// Authors:
//	Zoltan Varga (vargaz@freemail.hu)
//	Sebastien Pouliot  <sebastien@ximian.com>
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

using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System.Threading {

#if NET_2_0
	[Serializable]
	[ComVisibleAttribute (false)]
	public sealed class CompressedStack : ISerializable {
#else
	public class CompressedStack {
#endif
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

		~CompressedStack ()
		{
		}

#if NET_2_0
		[ComVisibleAttribute (false)]
		public CompressedStack CreateCopy ()
		{
			// in 2.0 beta1 Object.ReferenceEquals (cs, cs.CreateCopy ()) == true !!!
			return this;
//			return new CompressedStack (this);
		}

		[MonoTODO ("incomplete")]
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
		}

		static public CompressedStack Capture ()
		{
			CompressedStack cs = new CompressedStack (0);
			cs._list = SecurityFrame.GetStack (1);
			return cs;
		}

		static public CompressedStack GetCompressedStack ()
		{
			// Note: CompressedStack.GetCompressedStack doesn't return null
			// like Thread.CurrentThread.GetCompressedStack if no compressed
			// stack is present.
			return new CompressedStack (Thread.CurrentThread.GetCompressedStack ());
		}

		static public CompressedStackSwitcher SetCompressedStack (CompressedStack cs)
		{
			Thread t = Thread.CurrentThread;
			CompressedStack ctcs = t.GetCompressedStack ();
			if (ctcs != null) {
				string msg = Locale.GetText ("You must Undo previous CompressedStack.");
				throw new SecurityException (msg);
			}
			CompressedStackSwitcher csw = new CompressedStackSwitcher (ctcs, t);
			t.SetCompressedStack (cs);
			return csw;
		}
#endif
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
