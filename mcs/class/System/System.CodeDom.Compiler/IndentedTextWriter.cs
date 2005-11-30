//
// System.CodeDom.Compiler IndentedTextWriter class
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2002 Ximian, Inc.
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.IO;
using System.Security.Permissions;
using System.Text;

namespace System.CodeDom.Compiler {
	
	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	[PermissionSet (SecurityAction.InheritanceDemand, Unrestricted = true)]
	public class IndentedTextWriter
		: TextWriter
	{
		private TextWriter writer;
		private string tabString;
		private int indent;
		private bool newline;

		//
		// Constructors
		//
		public IndentedTextWriter( TextWriter writer )
		{
			this.writer = writer;
			this.tabString = DefaultTabString;
			newline = true;
		}

		public IndentedTextWriter( TextWriter writer, string tabString )
		{
			this.writer = writer;
			this.tabString = tabString;
			newline = true;
		}

		
		//
		// Fields
		//
		public const string DefaultTabString = "    ";

		//
		// Properties
		//
		public override Encoding Encoding {
			get {
				return writer.Encoding;
			}
		}

		public int Indent {
			get {
				return indent;
			}
			set {
				if (value < 0) {
					value = 0;
				}
				indent = value;
			}
		}

		public TextWriter InnerWriter {
			get {
				return writer;
			}
		}

		public override string NewLine {
			get {
				return writer.NewLine;
			}
			set {
				writer.NewLine = value;
			}
		}

		//
		// Methods
		//
		public override void Close()
		{
			writer.Close();
		}

		public override void Flush()
		{
			writer.Flush();
		}

		public override void Write( bool value )
		{
			OutputTabs();
			writer.Write( value );
		}

		public override void Write( char value )
		{
			OutputTabs();
			writer.Write( value );
		}
		
		public override void Write( char[] value )
		{
			OutputTabs();
			writer.Write( value );
		}

		public override void Write( double value )
		{
			OutputTabs();
			writer.Write( value );
		}

		public override void Write( int value )
		{
			OutputTabs();
			writer.Write( value );
		}

		public override void Write( long value )
		{
			OutputTabs();
			writer.Write( value );
		}

		public override void Write( object value )
		{
			OutputTabs();
			writer.Write( value );
		}

		public override void Write( float value )
		{
			OutputTabs();
			writer.Write( value );
		}

		public override void Write( string value )
		{
			OutputTabs();
			writer.Write( value );
		}

		public override void Write( string format, object arg )
		{
			OutputTabs();
			writer.Write( format, arg );
		}

		public override void Write( string format, params object[] args )
		{
			OutputTabs();
			writer.Write( format, args );
		}

		public override void Write( char[] buffer, int index, int count )
		{
			OutputTabs();
			writer.Write( buffer, index, count );
		}
		
		public override void Write( string format, object arg0, object arg1 )
		{
			OutputTabs();
			writer.Write( format, arg0, arg1 );
		}

		
		public override void WriteLine()
		{
			OutputTabs();
			writer.WriteLine();
			newline = true;
		}

		public override void WriteLine( bool value )
		{
			OutputTabs();
			writer.WriteLine( value );
			newline = true;
		}

		public override void WriteLine( char value )
		{
			OutputTabs();
			writer.WriteLine( value );
			newline = true;
		}

		public override void WriteLine( char[] value )
		{
			OutputTabs();
			writer.WriteLine( value );
			newline = true;
		}

		public override void WriteLine( double value )
		{
			OutputTabs();
			writer.WriteLine( value );
			newline = true;
		}

		public override void WriteLine( int value )
		{
			OutputTabs();
			writer.WriteLine( value );
			newline = true;
		}

		public override void WriteLine( long value )
		{
			OutputTabs();
			writer.WriteLine( value );
			newline = true;
		}

		public override void WriteLine( object value )
		{
			OutputTabs();
			writer.WriteLine( value );
			newline = true;
		}

		public override void WriteLine( float value )
		{
			OutputTabs();
			writer.WriteLine( value );
			newline = true;
		}

		public override void WriteLine( string value )
		{
			OutputTabs();
			writer.WriteLine( value );
			newline = true;
		}

		[CLSCompliant(false)]
		public override void WriteLine( uint value )
		{
			OutputTabs();
			writer.WriteLine( value );
			newline = true;
		}

		public override void WriteLine( string format, object arg )
		{
			OutputTabs();
			writer.WriteLine( format, arg );
			newline = true;
		}

		public override void WriteLine( string format, params object[] args )
		{
			OutputTabs();
			writer.WriteLine( format, args );
			newline = true;
		}

		public override void WriteLine( char[] buffer, int index, int count )
		{
			OutputTabs();
			writer.WriteLine( buffer, index, count );
			newline = true;
		}

		public override void WriteLine( string format, object arg0, object arg1 )
		{
			OutputTabs();
			writer.WriteLine( format, arg0, arg1 );
			newline = true;
		}


		public void WriteLineNoTabs( string value )
		{
			writer.WriteLine( value );
			newline = true;
		}


		protected virtual void OutputTabs()
		{
			if ( newline ) {
				for ( int i = 0; i < indent; ++i )
					writer.Write( tabString );
				newline = false;
			}
		}
	}
}
