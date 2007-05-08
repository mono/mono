//
// System.Diagnostics.TraceListener.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//   Atsushi Enomoto (atsushi@ximian.com)
//
// Comments from John R. Hicks <angryjohn69@nc.rr.com> original implementation 
// can be found at: /mcs/docs/apidocs/xml/en/System.Diagnostics
//
// (C) 2002 Jonathan Pryor
// (C) 2007 Novell, Inc.
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
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace System.Diagnostics {

	public abstract class TraceListener : MarshalByRefObject, IDisposable {

#if TARGET_JVM
		readonly LocalDataStoreSlot _indentLevelStore = System.Threading.Thread.AllocateDataSlot ();
		readonly LocalDataStoreSlot _indentSizeStore = System.Threading.Thread.AllocateDataSlot ();
		
		private int indentLevel {
			get {
				object o = System.Threading.Thread.GetData (_indentLevelStore);
				if (o == null)
					return 0;
				return (int) o;
			}
			set { System.Threading.Thread.SetData (_indentLevelStore, value); }
		}
		
		private int indentSize {
			get {
				object o = System.Threading.Thread.GetData (_indentSizeStore);
				if (o == null)
					return 4;
				return (int) o;
			}
			set { System.Threading.Thread.SetData (_indentSizeStore, value); }
		}
#else
		[ThreadStatic]
		private int indentLevel = 0;

		[ThreadStatic]
		private int indentSize = 4;
#endif

		private string name = null;
		private bool needIndent = true;

		protected TraceListener () : this ("")
		{
		}

		protected TraceListener (string name)
		{
			Name = name;
		}

		public int IndentLevel {
			get {return indentLevel;}
			set {indentLevel = value;}
		}

		public int IndentSize {
			get {return indentSize;}
			set {indentSize = value;}
		}

		public virtual string Name {
			get {return name;}
			set {name = value;}
		}

		protected bool NeedIndent {
			get {return needIndent;}
			set {needIndent = value;}
		}

		public virtual void Close ()
		{
			Dispose ();
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
		}

		public virtual void Fail (string message)
		{
			Fail (message, "");
		}

		public virtual void Fail (string message, string detailMessage)
		{
			WriteLine ("---- DEBUG ASSERTION FAILED ----");
			WriteLine ("---- Assert Short Message ----");
			WriteLine (message);
			WriteLine ("---- Assert Long Message ----");
			WriteLine (detailMessage);
			WriteLine ("");
		}

		public virtual void Flush ()
		{
		}

		public virtual void Write (object o)
		{
			Write (o.ToString());
		}

		public abstract void Write (string message);

		public virtual void Write (object o, string category)
		{
			Write (o.ToString(), category);
		}

		public virtual void Write (string message, string category)
		{
			Write (category + ": " + message);
		}

		protected virtual void WriteIndent ()
		{
			// Must set NeedIndent to false before Write; otherwise, we get endless
			// recursion with Write->WriteIndent->Write->WriteIndent...*boom*
			NeedIndent = false;
			String indent = new String (' ', IndentLevel*IndentSize);
			Write (indent);
		}

		public virtual void WriteLine (object o)
		{
			WriteLine (o.ToString());
		}

		public abstract void WriteLine (string message);

		public virtual void WriteLine (object o, string category)
		{
			WriteLine (o.ToString(), category);
		}

		public virtual void WriteLine (string message, string category)
		{
			WriteLine (category + ": " + message);
		}

#if NET_2_0
		[ComVisible (false)]
		public virtual void TraceData (TraceEventCache eventCache, string source,
			TraceEventType eventType, int id, object data)
		{
			WriteLine (String.Format ("{0} {1}: {2} : {3}", source, eventType, id, data));

			// any of the eventCache content are not written.
		}

		[ComVisible (false)]
		public virtual void TraceData (TraceEventCache eventCache, string source,
			TraceEventType eventType, int id, params object [] data)
		{
			string s = null;
			string [] arr = new string [data.Length];
			for (int i = 0; i < arr.Length; i++)
				arr [i] = data [i] != null ? data [i].ToString () : String.Empty;
			WriteLine (String.Format ("{0} {1}: {2} : {3}", source, eventType, id, String.Join (" ", arr)));

			// any of the eventCache content are not written.
		}

		[ComVisible (false)]
		public virtual void TraceEvent (TraceEventCache eventCache, string source, TraceEventType eventType, int id)
		{
			TraceEvent (eventCache, source, eventType, id, null);
		}

		[ComVisible (false)]
		public virtual void TraceEvent (TraceEventCache eventCache, string source, TraceEventType eventType,
			int id, string message)
		{
			TraceData (eventCache, source, eventType, id, message);
		}

		[ComVisible (false)]
		public virtual void TraceEvent (TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object [] args)
		{
			TraceEvent (eventCache, source, eventType, id, String.Format (format, args));
		}

		[ComVisible (false)]
		public virtual void TraceTransfer (TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
		{
			TraceData (eventCache, source, TraceEventType.Transfer, id, String.Format ("{0}, relatedActivityId={1}", message, relatedActivityId));
		}
#endif
	}
}

