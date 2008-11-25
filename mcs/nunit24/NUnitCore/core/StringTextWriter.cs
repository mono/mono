// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System.IO;
using System.Text;

namespace NUnit.Core
{
	// TODO: This class is not currently being used. Review to
	// see if we will use it again, otherwise drop it.
	#region StringTextWriter

	/// <summary>
	/// Use this wrapper to ensure that only strings get passed accross the AppDomain
	/// boundary.  Otherwise tests will break when non-remotable objects are passed to
	/// Console.Write/WriteLine.
	/// </summary>
	public class StringTextWriter : TextWriter
	{
		public StringTextWriter( TextWriter aTextWriter )
		{
			theTextWriter = aTextWriter;
		}

		protected TextWriter theTextWriter;

		override public void Write(char aChar)
		{
			theTextWriter.Write(aChar);
		}

		override public void Write(string aString)
		{
			theTextWriter.Write(aString);
		}

		override public void WriteLine(string aString)
		{
			theTextWriter.WriteLine(aString);
		}

		override public System.Text.Encoding Encoding
		{
			get { return theTextWriter.Encoding; }
		}

		public override void Close()
		{
			this.Flush();
			theTextWriter.Close ();
		}

		public override void Flush()
		{
			theTextWriter.Flush ();
		}
	}

	#endregion

	#region BufferedStringTextWriter

	/// <summary>
	/// This wrapper derives from StringTextWriter and adds buffering
	/// to improve cross-domain performance. The buffer is flushed whenever
	/// it reaches or exceeds a maximum size or when Flush is called.
	/// </summary>
	public class BufferedStringTextWriter : StringTextWriter
	{
		public BufferedStringTextWriter( TextWriter aTextWriter ) : base( aTextWriter ){ }
	
		private static readonly int MAX_BUFFER = 1000;
		private StringBuilder sb = new StringBuilder( MAX_BUFFER );

		override public void Write(char aChar)
		{
			lock( sb )
			{
				sb.Append( aChar );
				this.CheckBuffer();
			}
		}

		override public void Write(string aString)
		{
			lock( sb )
			{
				sb.Append( aString );
				this.CheckBuffer();
			}
		}

		override public void WriteLine(string aString)
		{
			lock( sb )
			{
				sb.Append( aString );
				sb.Append( '\n' );
				this.CheckBuffer();
			}
		}

		override public void Flush()
		{
			if ( sb.Length > 0 )
			{
				lock( sb )
				{
					theTextWriter.Write( sb.ToString() );
					sb.Length = 0;
				}
			}

			theTextWriter.Flush();
		}

		private void CheckBuffer()
		{
			if ( sb.Length >= MAX_BUFFER )
				this.Flush();
		}
	}

	#endregion
}
