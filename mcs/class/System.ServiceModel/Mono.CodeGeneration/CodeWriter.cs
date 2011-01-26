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
// Copyright (C) Lluis Sanchez Gual, 2004
//

#if !MONOTOUCH
using System;
using System.IO;

namespace Mono.CodeGeneration
{

public class CodeWriter
{
	TextWriter writer;
	int indent;
	
	public CodeWriter (TextWriter tw)
	{
		writer = tw;
	}
	
	public CodeWriter BeginLine ()
	{
		writer.Write (new String (' ', indent*4));
		return this;
	}
	
	public CodeWriter Write (string s)
	{
		writer.Write (s);
		return this;
	}
	
	public CodeWriter EndLine ()
	{
		writer.WriteLine ();
		return this;
	}
	
	public CodeWriter WriteLine (string s)
	{
		BeginLine ();
		Write (s);
		EndLine ();
		return this;
	}
	
	public CodeWriter WriteLineInd (string s)
	{
		WriteLine (s);
		indent++;
		return this;
	}
	
	public CodeWriter WriteLineUnind (string s)
	{
		indent--;
		WriteLine (s);
		return this;
	}
	
	public void Indent ()
	{
		indent++;
	}
	
	public void Unindent ()
	{
		indent--;
	}
}

}
#endif
