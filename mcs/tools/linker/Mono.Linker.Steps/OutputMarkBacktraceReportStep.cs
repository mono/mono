//
// OutputStep.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// (C) 2006 Jb Evain
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
using System.IO;
using System.Text;
using Mono.Cecil;

namespace Mono.Linker.Steps
{

	public class OutputMarkBacktraceReportStep : BaseStep
	{

		protected override void ProcessAssembly(AssemblyDefinition assembly)
		{
			var sw = new StreamWriter(Path.Combine(Context.OutputDirectory, assembly.Name.Name+"."+assembly.Kind.ToString().ToLower()+"_MarkBacktraceReport.txt"));
			foreach(TypeDefinition t in assembly.MainModule.Types)
			{
				if (Annotations.IsMarked(t))
				{
					sw.WriteLine(t.FullName + " marked by\n" + GenerateMarkBackTrace(t));
					sw.WriteLine();
				}
				foreach(MethodDefinition method in t.Methods)
				{
					if (Annotations.IsMarked(method))
					{
						sw.WriteLine(method + " marked by\n" + GenerateMarkBackTrace(method));
						sw.WriteLine();
					}	
				}
			}
			sw.Close();
		}

		private string GenerateMarkBackTrace(IAnnotationProvider t)
		{
			return GenerateMarkBackTrace(t,0);
		}
		private string GenerateMarkBackTrace(IAnnotationProvider t, int depth)
		{
			if (depth == 40) return "";
			var r = Annotations.GetMarkReason(t);
			if (r == null) return "unknown";
			var sb = new StringBuilder();
			if (r is IAnnotationProvider)
			{
				sb.Append(r);
				sb.AppendLine();
				for (int i = 0; i != depth+1; i++) sb.Append("...");
				sb.Append(GenerateMarkBackTrace((IAnnotationProvider) r, depth + 1));
				return sb.ToString();
			}
			return r.ToString();
		}
	}
}
