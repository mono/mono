// cilc -- a CIL-to-C binding generator
// Copyright (C) 2003, 2004, 2005, 2006, 2007 Alp Toker <alp@atoker.com>
// Licensed under the terms of the MIT License

using System;
using System.IO;

class CodeWriter
{
	private StreamWriter w;

	public CodeWriter (string fname)
	{
		Init (fname);
	}

	public bool IsDuplicate = false;

	void Init (string fname)
	{
		if (File.Exists (fname)) {
			string newfname = fname + ".x";
			//Console.WriteLine ("Warning: File " + fname + " already exists, using " + newfname);
			IsDuplicate = true;
			Init (newfname);
			return;
		}

		FileStream fs = new FileStream (fname, FileMode.OpenOrCreate, FileAccess.Write);
		w = new StreamWriter (fs);
	}

	public string Indenter = "  ";
	string cur_indent = String.Empty;
	int level = 0;

	public void Indent ()
	{
		level++;
		cur_indent = String.Empty;
		for (int i = 0; i != level ; i++) cur_indent += Indenter;
	}

	public void Outdent ()
	{
		level--;
		cur_indent = String.Empty;
		for (int i = 0; i != level ; i++) cur_indent += Indenter;
	}

	public void Write (string text)
	{
		w.Write (text);
	}

	public void WriteLine (string text)
	{
		WriteLine (text, true);
	}

	public void WriteLine (string text, bool autoindent)
	{
		char[] opentags = {'{', '('};
		char[] closetags = {'}', ')'};

		if (autoindent && text.TrimStart (closetags) != text)
			Outdent ();

		w.Write (cur_indent);
		w.WriteLine (text);

		if (autoindent && text.TrimEnd (opentags) != text)
			Indent ();
	}

	public void WriteLine (string text, CodeWriter cc)
	{
		WriteLine (text, String.Empty, cc, String.Empty);
	}

	public void WriteLine (string text, CodeWriter cc, string suffix)
	{
		WriteLine (text, String.Empty, cc, suffix);
	}

	public void WriteLine (string text, string prefix, CodeWriter cc)
	{
		WriteLine (text, prefix, cc, String.Empty);
	}

	public void WriteLine (string text, string prefix, CodeWriter cc, string suffix)
	{
		WriteLine (text);
		cc.WriteLine (prefix + text + suffix);
	}

	public void WriteComment (string text)
	{
		w.WriteLine ("/* " + text + " */");
	}

	public void WriteLine ()
	{
		w.WriteLine (String.Empty);
	}

	public void Close ()
	{
		w.Flush ();
		w.Close ();
	}
}
