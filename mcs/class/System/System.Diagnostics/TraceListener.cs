//
// System.Diagnostics.TraceListener.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// Comments from John R. Hicks <angryjohn69@nc.rr.com> original implementation 
// can be found at: /mcs/docs/apidocs/xml/en/System.Diagnostics
//
// (C) 2002 Jonathan Pryor
//

using System;
using System.Diagnostics;

namespace System.Diagnostics {

	public abstract class TraceListener : MarshalByRefObject, IDisposable {

		[ThreadStatic]
		private int indentLevel = 0;

		[ThreadStatic]
		private int indentSize = 4;

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
	}
}

