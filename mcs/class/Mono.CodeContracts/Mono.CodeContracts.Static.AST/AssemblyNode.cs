// 
// AssemblyNode.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
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

using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Mono.CodeContracts.Static.AST {
	class AssemblyNode : Node {
		private readonly AssemblyDefinition definition;
		private IEnumerable<Module> modules;

		public AssemblyNode (AssemblyDefinition definition) : base (NodeType.Assembly)
		{
			this.definition = definition;
		}

		public string FullName
		{
			get { return this.definition.FullName; }
		}

		public IEnumerable<Module> Modules
		{
			get
			{
				if (this.modules == null)
					this.modules = this.definition.Modules.Select (it => new Module (it)).ToList ();
				return this.modules;
			}
		}

		public TypeNode GetType (string ns, string className)
		{
			foreach (Module module in Modules) {
				TypeNode type = module.GetType (ns, className);
				if (type != null)
					return type;
			}
			IEnumerable<TypeDefinition> enumerable = this.definition.Modules.SelectMany (m => m.Types);
			TypeDefinition firstOrDefault = enumerable.FirstOrDefault (t => t.Namespace == ns && t.Name == className);
			if (firstOrDefault == null)
				return null;

			return TypeNode.Create (firstOrDefault);
		}

		public static AssemblyNode ReadAssembly (string filename)
		{
			var readerParameters = new ReaderParameters ();
			AssemblyDefinition definition = AssemblyDefinition.ReadAssembly (filename, readerParameters);

			return new AssemblyNode (definition);
		}

		public static AssemblyNode GetSystemAssembly ()
		{
			return ReadAssembly (typeof (object).Module.Assembly.Location);
		}
	}
}
