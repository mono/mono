//
// System.Diagnostics.TraceListener.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// Comments from John R. Hicks <angryjohn69@nc.rr.com> original
// implementation.
//
// (C) 2002 Jonathan Pryor
//

using System;
using System.Diagnostics;

namespace System.Diagnostics {

	/// <summary>
	/// Provides the abstract base class for the listeners who monitor
	/// trace and debug output
	/// </summary>
	public abstract class TraceListener : MarshalByRefObject, IDisposable {

		private int indentLevel = 0;
		private int indentSize = 4;
		private string name = null;
		private bool needIndent = false;

		/// <summary>
		/// Initializes a new instance of the <see cref="TraceListener">
		/// TraceListener</see> class.
		/// </summary>
		protected TraceListener () : this ("")
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TraceListener">
		/// TraceListener</see> class using the specified name as the listener.
		/// </summary>
		protected TraceListener (string name)
		{
			Name = name;
		}

		/// <summary>
		/// Gets or sets the indent level.
		/// </summary>
		/// <value>
		/// The indent level.  The default is zero.
		/// </value>
		public int IndentLevel {
			get {return indentLevel;}
			set {indentLevel = value;}
		}

		/// <summary>
		/// Gets or sets the number of spaces in an indent.
		/// </summary>
		/// <value>
		/// The number of spaces in an indent.  The default is four spaces.
		/// </value>
		public int IndentSize {
			get {return indentSize;}
			set {indentSize = value;}
		}

		/// <summary>
		/// Gets or sets a name for this 
		/// <see cref="TraceListener">TraceListener</see>.
		/// </summary>
		/// <value>
		/// A name for this <see cref="TraceListener">TraceListener</see>.
		/// The default is the empty string ("")
		/// </value>
		public virtual string Name {
			get {return name;}
			set {name = value;}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to indent the output.
		/// </summary>
		/// <value>
		/// <b>true</b> if the output should be indented; otherwise <b>false</b>.
		/// </value>
		protected bool NeedIndent {
			get {return needIndent;}
			set {needIndent = value;}
		}

		/// <summary>
		/// When overridden in a derived class, closes the output stream so it 
		/// no longer receives tracing or debugging output.
		/// </summary>
		public virtual void Close ()
		{
			Dispose ();
		}

		/// <summary>
		/// Releases all resources used by the i
		/// <see cref="TraceListener">TraceListener</see>.
		/// </summary>
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		/// <summary>
		/// Releases the unmanaged resources used by the 
		/// <see cref="TraceListener">TraceListener</see> and optionally 
		/// releases the managed resources.
		/// </summary>
		/// <param name="disposing">
		/// <b>true</b> to release both managed and unmanaged resources;
		/// <b>false</b> to release only unmanaged resources.
		/// </param>
		protected virtual void Dispose (bool disposing)
		{
		}

		/// <summary>
		/// Emits an error message to the listener you create when you 
		/// implement the <see cref="TraceListener">TraceListener</see> class.
		/// </summary>
		/// <param name="message">
		/// A message to emit.
		/// </param>
		public virtual void Fail (string message)
		{
			Fail (message, "");
		}

		/// <summary>
		/// Emits an error message, and a detailed error message to the listener
		/// you create when you implement the 
		/// <see cref="TraceListener">TraceListener</see> class.
		/// </summary>
		/// <param name="message">
		/// A message to emit.
		/// </param>
		/// <param name="detailMessage">
		/// A detailed message to emit.
		/// </param>
		public virtual void Fail (string message, string detailMessage)
		{
			WriteLine ("---- DEBUG ASSERTION FAILED ----");
			WriteLine ("---- Assert Short Message ----");
			WriteLine (message);
			WriteLine ("---- Assert Long Message ----");
			WriteLine (detailMessage);
			WriteLine ("");
		}

		/// <summary>
		/// When overridden in a derived class, flushes the output buffer.
		/// </summary>
		public virtual void Flush ()
		{
		}

		/// <summary>
		/// Writes the value of the object's 
		/// <see cref="System.Object.ToString">ToString</see>
		/// method to the listener you create when you implement the
		/// <see cref="TraceListener">TraceListener</see> class.
		/// </summary>
		/// <param name="o">
		/// An <see cref="System.Object">Object</see> whose fully qualified
		/// class name you want to write.
		/// </param>
		public virtual void Write (object o)
		{
			Write (o.ToString());
		}

		/// <summary>
		/// When overridden in a derived class, writes the specified message to 
		/// the listener you create in the derived class.
		/// </summary>
		/// <param name="message">
		/// A message to write.
		/// </param>
		public abstract void Write (string message);

		/// <summary>
		/// Writes a category name and the value of the object's 
		/// <see cref="System.Object.ToString">ToString</see>
		/// method to the listener you create when you implement the
		/// <see cref="TraceListener">TraceListener</see> class.
		/// </summary>
		/// <param name="o">
		/// An <see cref="System.Object">Object</see> whose fully qualified
		/// class name you wish to write.
		/// </param>
		/// <param name="category">
		/// A category name used to organize the output.
		/// </param>
		public virtual void Write (object o, string category)
		{
			Write (o.ToString(), category);
		}

		/// <summary>
		/// Writes a category name and a message to the listener you create when 
		/// you implement the <see cref="TraceListener">TraceListener</see> class.
		/// </summary>
		/// <param name="message">
		/// A message to write.
		/// </param>
		/// <param name="category">
		/// A category name used to organize the output.
		/// </param>
		public virtual void Write (string message, string category)
		{
			Write (category + ": " + message);
		}

		/// <summary>
		/// Writes the indent to the listener you create when you implement 
		/// this class, and resets the <see cref="NeedIndent">NeedIndent</see> 
		/// Property to <b>false</b>.
		/// </summary>
		protected virtual void WriteIndent ()
		{
			String indent = new String (' ', IndentLevel*IndentSize);
			Write (indent);
			NeedIndent = false;
		}

		/// <summary>
		/// Writes the value of the object's 
		/// <see cref="System.Object.ToString">ToString</see>
		/// method to the listener you create when you implement the
		/// <see cref="TraceListener">TraceListener</see> class, followed 
		/// by a line terminator.
		/// </summary>
		/// <param name="o">
		/// An <see cref="System.Object">Object</see> whose fully qualified
		/// class name you want to write.
		/// </param>
		public virtual void WriteLine (object o)
		{
			WriteLine (o.ToString());
		}

		/// <summary>
		/// When overridden in a derived class, writes the specified message to 
		/// the listener you create in the derived class, followed by a 
		/// line terminator.
		/// </summary>
		/// <param name="message">
		/// A message to write.
		/// </param>
		public abstract void WriteLine (string message);

		/// <summary>
		/// Writes a category name and the value of the object's 
		/// <see cref="System.Object.ToString">ToString</see>
		/// method to the listener you create when you implement the
		/// <see cref="TraceListener">TraceListener</see> class, followed by a
		/// line terminator.
		/// </summary>
		/// <param name="o">
		/// An <see cref="System.Object">Object</see> whose fully qualified
		/// class name you wish to write.
		/// </param>
		/// <param name="category">
		/// A category name used to organize the output.
		/// </param>
		public virtual void WriteLine (object o, string category)
		{
			WriteLine (o.ToString(), category);
		}

		/// <summary>
		/// Writes a category name and a message to the listener you create when 
		/// you implement the <see cref="TraceListener">TraceListener</see> class,
		/// followed by a line terminator.
		/// </summary>
		/// <param name="message">
		/// A message to write.
		/// </param>
		/// <param name="category">
		/// A category name used to organize the output.
		/// </param>
		public virtual void WriteLine (string message, string category)
		{
			WriteLine (category + ": " + message);
		}
	}
}

