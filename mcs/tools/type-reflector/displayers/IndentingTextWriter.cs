//
// IndentingTextWriter.cs: Helper class to indent text written to a TextWriter
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
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

		private static BooleanSwitch info = 
			new BooleanSwitch ("indenting-text-writer", "IndentingTextWriter messages");

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
			Trace.WriteLineIf (info.Enabled, String.Format(
				"WriteIndent: char='{0}',level={1},size={2}",
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
			Trace.WriteLineIf (info.Enabled, String.Format(
				"WriteLine: NeedIndent={0}", NeedIndent));
			if (NeedIndent)
				WriteIndent ();
			_writer.WriteLine (value);
			NeedIndent = true;
		}
	}

	public class Indenter : IDisposable {

		private static BooleanSwitch info = 
			new BooleanSwitch ("indenter", "Indenter Messages");

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
			Trace.WriteLineIf (info.Enabled, String.Format(
				"Disposing; indentlevel={0}", 
				_writer.IndentLevel));
		}
	}
}

