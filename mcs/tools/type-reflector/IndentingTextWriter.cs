//
// IndentingTextWriter.cs: Helper class to indent text written to a TextWriter
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//
// Permission is hereby granted, free of charge, to any           
// person obtaining a copy of this software and associated        
// documentation files (the "Software"), to deal in the           
// Software without restriction, including without limitation     
// the rights to use, copy, modify, merge, publish,               
// distribute, sublicense, and/or sell copies of the Software,    
// and to permit persons to whom the Software is furnished to     
// do so, subject to the following conditions:                    
//                                                                 
// The above copyright notice and this permission notice          
// shall be included in all copies or substantial portions        
// of the Software.                                               
//                                                                 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY      
// KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO         
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A               
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL      
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,      
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,  
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION       
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Mono.TypeReflector
{
	public class IndentingTextWriter : TextWriter {

		private TextWriter _writer;

		private int indentLevel = 0;
		private int indentSize = 4;
		private bool needIndent = true;
		private char indentChar = ' ';

		public IndentingTextWriter (TextWriter writer)
		{
			_writer = writer;
		}

		public int IndentLevel {
			get {return indentLevel;}
			set {indentLevel = value;}
		}

		public int IndentSize {
			get {return indentSize;}
			set {indentSize = value;}
		}

		public char IndentChar {
			get {return indentChar;}
			set {indentChar = value;}
		}

		public void Indent ()
		{
			++IndentLevel;
		}

		public void Unindent ()
		{
			--IndentLevel;
		}

		protected bool NeedIndent {
			get {return needIndent;}
			set {needIndent = value;}
		}

		protected virtual void WriteIndent ()
		{
			NeedIndent = false;
			Trace.WriteLine (String.Format(
				"** WriteIndent: char='{0}',level={1},size={2}",
				IndentChar, IndentLevel, IndentSize));
			string indent = new string (IndentChar, 
					IndentLevel * IndentSize);
			Write (indent);
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing)
				_writer.Close ();
		}

		public override System.Text.Encoding Encoding {
			get {return _writer.Encoding;}
		}

		public override void Write (string value)
		{
			if (NeedIndent)
				WriteIndent ();
			_writer.Write (value);
		}

		public override void WriteLine ()
		{
			if (NeedIndent)
				WriteIndent ();
			_writer.WriteLine ();
			NeedIndent = true;
		}

		public override void WriteLine (string value)
		{
			Trace.WriteLine (String.Format(
				"** WriteLine: NeedIndent={0}", NeedIndent));
			if (NeedIndent)
				WriteIndent ();
			_writer.WriteLine (value);
			NeedIndent = true;
		}
	}

	public class Indenter : IDisposable {

		private IndentingTextWriter _writer;
		private int level;

		public Indenter (IndentingTextWriter writer) 
			: this (writer, 1)
		{
		}

		public Indenter (IndentingTextWriter writer, int level)
		{
			this.level = level;
			_writer = writer;
			_writer.IndentLevel += level;
			// _writer.Indent ();
		}

		public void Dispose ()
		{
			_writer.IndentLevel -= level;
			// _writer.Unindent ();
			Trace.WriteLine (String.Format(
				"** Disposing; indentlevel={0}", 
				_writer.IndentLevel));
		}
	}
}

