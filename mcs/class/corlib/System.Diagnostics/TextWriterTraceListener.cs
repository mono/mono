//
// System.Diagnostics.TextWriterTraceListener.cs
//
// Author:
//		John R. Hicks (angryjohn69@nc.rr.com)
//
// (C) 2001
//
namespace System.Diagnostics
{
	using System;
	using System.IO;
	using System.Text;
	
	/// <summary>
	/// Directs tracing or debugging output to a <see cref="System.IO.TextWriter">
	/// TextWriter</see> or to a <see cref="System.IO.Stream">Stream</see>,
	/// such as <see cref="System.Console.Out">Console.Out</see> or
	/// <see cref="System.IO.FileStream">FileStream</see>.
	/// </summary>
	public class TextWriterTraceListener : TraceListener
	{	
		
		private TextWriter writer;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="TextWriterTraceListener">
		/// TextWriterTraceListener</see> class with <see cref="System.IO.TextWriter">
		/// TextWriter</see> as the output recipient.
		/// </summary>
		public TextWriterTraceListener() : base("TextWriter")
		{
			writer = Console.Out;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="TextWriterTraceListener">
		/// TextWriterTraceListener</see> class, using the stream as the output
		/// recipient of the debugging and tracing output.
		/// </summary>
		/// <param name="stream">
		/// A <see cref="System.IO.Stream">Stream</see> that represents the stream the
		/// <see cref="TextWriterTraceListener">TextWriterTraceListener</see> writes to.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// The stream is a null reference.
		/// </exception>
		public TextWriterTraceListener(Stream stream) : base("")
		{
			if(stream == null)
				throw new ArgumentNullException("Stream is null");
			
			writer = new StreamWriter(stream);
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="TextWriterTraceListener">
		/// TextWriterTraceListener</see> class, using the file as the recipient
		/// of the debugging and tracing output.
		/// </summary>
		/// <param name="fileName">
		/// The name of the file the <see cref="TextWriterTraceListener">
		/// TextWriterTraceListener</see> writes to.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// The fileName is null.
		/// </exception>
		public TextWriterTraceListener(string fileName) : base("")
		{
			if(fileName == null)
				throw new ArgumentNullException("Filename is null");
			
			FileStream fileStream = new FileStream(fileName, FileMode.Create);
			writer = new StreamWriter(fileStream);
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="TextWriterTraceListener">
		/// TextWriterTraceListener</see> class using the specified writer as the
		/// recipient of the tracing or debugging output.
		/// </summary>
		/// <param name="writer">
		/// A <see cref="System.IO.TextWriter">TextWriter</see> that receives output
		/// from the <see cref="TextWriterTraceListener">TextWriterTraceListener</see>.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// The writer is a null reference
		/// </exception>
		public TextWriterTraceListener(TextWriter writer) : base("")
		{
			if(writer == null)
				throw new ArgumentNullException("Given TextWriter is null");
			this.writer = writer;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="TextWriterTraceListener">
		/// TextWriterTraceListener</see> class with the specified name, using the
		/// stream as the recipient of the tracing or debugging output.
		/// </summary>
		/// <param name="stream">
		/// A <see cref="System.IO.Stream">Stream</see> that represents the stream the
		/// <see cref="TextWriterTraceListener">TextWriterTraceListener</see>
		/// writes to.
		/// </param>
		/// <param name="name">
		/// The name of the new instance
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// The stream is a null reference
		/// </exception>
		public TextWriterTraceListener(Stream stream, string name) : base(name)
		{
			if(stream == null)
				throw new ArgumentNullException("Supplied Stream is null");
			writer = new StreamWriter(stream);
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="TextWriterTraceListener">
		/// TextWriterTraceListener</see> class with the specified name, using the
		/// file as the recipient of the tracing or debugging output.
		/// </summary>
		/// <param name="fileName">
		/// The name of the file the <see cref="TextWriterTraceListener">
		/// TextWriterTraceListener</see> writes to.
		/// </param>
		/// <param name="name">
		/// The name of the new instance
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// The file is a null reference.
		/// </exception>
		public TextWriterTraceListener(string fileName, string name) : base(name)
		{
			if(fileName == null)
				throw new ArgumentNullException("Supplied file name is null");
			FileStream fileStream = new FileStream(fileName, FileMode.Create);
			writer = new StreamWriter(fileStream);
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="TextWriterTraceListener">
		/// TextWriterTraceListener</see> class with the specified name, using
		/// the specified writer as the recipient of the tracing or debugging output.
		/// </summary>
		/// <param name="writer">
		/// A <see cref="System.IO.TextWriter">TextWriter</see> that receives the output
		/// from the <see cref="TextWriterTraceListener">TextWriterTraceListener</see>.
		/// </param>
		/// <param name="name">
		/// The name of the new instance.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// The writer is a null reference.
		/// </exception>
		public TextWriterTraceListener(TextWriter writer, string name) : base(name)
		{
			if(writer == null)
				throw new ArgumentNullException("The supplied writer is null");
			this.writer = writer;
		}
		
		/// <summary>
		/// Gets or sets the writer that receives the debugging or tracing output.
		/// </summary>
		/// <value>
		/// A <see cref="System.IO.TextWriter">TextWriter</see> that represents the writer
		/// that receives the tracing or debugging output.
		/// </value>
		public TextWriter Writer
		{
			get
			{
				return writer;
			}
			set
			{
				writer = value;
			}
		}
		
		/// <summary>
		/// Closes the <see cref="System.IO.Writer">Writer</see> so that it no longer
		/// receives tracing or debugging output.
		/// </summary>
		public override void Close()
		{
			writer.Close();
		}
		
		/// <summary>
		/// Flushes the output buffer for the <see cref="System.IO.Writer">Writer</see>.
		/// </summary>
		public override void Flush()
		{
			writer.Flush();
		}
		
		/// <summary>
		/// Writes a message to this instance's <see cref="System.IO.Writer">Writer</see>.
		/// </summary>
		/// <param name="message">
		/// A message to write.
		/// </param>
		public override void Write(string message)
		{
			Console.Error.WriteLine("We should be getting output here");
			writer.Write(message);
			
		}
		
		/// <summary>
		/// Writes a message to this instance's <see cref="System.IO.Writer">Writer</see>
		/// followed by a line terminator.
		/// </summary>
		/// <param name="message">
		/// A message to write.
		/// </param>
		public override void WriteLine(string message)
		{
			Console.Error.WriteLine("We should be getting output here, too");	
			writer.WriteLine(message);
			
		}
	}
}
