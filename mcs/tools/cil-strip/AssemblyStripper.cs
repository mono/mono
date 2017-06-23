//
// AssemblyStripper.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//
// (C) 2008 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.IO;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mono.CilStripper {

	class AssemblyStripper {

		AssemblyDefinition assembly;
		string outputFileName;

		AssemblyStripper (AssemblyDefinition assembly, string outputFileName)
		{
			this.assembly = assembly;
			this.outputFileName = outputFileName;
		}

		void Strip ()
		{
			ClearMethodBodies ();
			Write ();
		}

		void ClearMethodBodies ()
		{
			foreach (TypeDefinition type in assembly.MainModule.GetTypes()) {
				ClearMethodBodies (type.Methods);
			}
		}

		void ClearMethodBodies (ICollection methods)
		{
			foreach (MethodDefinition method in methods) {
				method.ImplAttributes |= MethodImplAttributes.NoInlining;

				if (!method.HasBody)
					continue;

				var processor = method.Body.GetILProcessor ();
				processor.Body.Instructions.Clear ();
				processor.Emit (OpCodes.Ret);

				method.Body.Variables.Clear ();
				method.Body.ExceptionHandlers.Clear ();
			}
		}

		void Write ()
		{
			if (outputFileName == null)
				assembly.Write ();
			else
				assembly.Write (outputFileName);
		}

		public static void StripAssembly (AssemblyDefinition assembly, string outputFileName)
		{
			new AssemblyStripper (assembly, outputFileName).Strip ();
		}
	}
}
