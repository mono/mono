#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright © 2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright © 2000-2003 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright © 2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright © 2000-2003 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

using System;
using System.IO;
using System.Text;

namespace NUnit.Core
{
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
