//
// AssemblyRef
//
// Author:
//	Bruno Lauze     (brunolauze@msn.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2015 Microsoft (http://www.microsoft.com)
//

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
using System;
using System.Collections;
using System.Globalization;
using System.IO;

namespace System.Management.Instrumentation
{
	internal class CodeWriter
	{
		private int depth;

		private ArrayList children;

		public CodeWriter()
		{
			this.children = new ArrayList();
		}

		public CodeWriter AddChild(string name)
		{
			this.Line(name);
			this.Line("{");
			CodeWriter codeWriter = new CodeWriter();
			codeWriter.depth = this.depth + 1;
			this.children.Add(codeWriter);
			this.Line("}");
			return codeWriter;
		}

		public CodeWriter AddChild(string[] parts)
		{
			return this.AddChild(string.Concat(parts));
		}

		public CodeWriter AddChild(CodeWriter snippet)
		{
			snippet.depth = this.depth;
			this.children.Add(snippet);
			return snippet;
		}

		public CodeWriter AddChildNoIndent(string name)
		{
			this.Line(name);
			CodeWriter codeWriter = new CodeWriter();
			codeWriter.depth = this.depth + 1;
			this.children.Add(codeWriter);
			return codeWriter;
		}

		public void Line(string line)
		{
			this.children.Add(line);
		}

		public void Line(string[] parts)
		{
			this.Line(string.Concat(parts));
		}

		public void Line()
		{
			this.children.Add(null);
		}

		public static explicit operator String(CodeWriter writer)
		{
			return writer.ToString();
		}

		public override string ToString()
		{
			StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
			this.WriteCode(stringWriter);
			string str = stringWriter.ToString();
			stringWriter.Close();
			return str;
		}

		private void WriteCode(TextWriter writer)
		{
			string str = new string(' ', this.depth * 4);
			foreach (object child in this.children)
			{
				if (child != null)
				{
					if (child as string == null)
					{
						((CodeWriter)child).WriteCode(writer);
					}
					else
					{
						writer.Write(str);
						writer.WriteLine(child);
					}
				}
				else
				{
					writer.WriteLine();
				}
			}
		}
	}
}