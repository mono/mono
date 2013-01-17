// 
// Class.cs
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
	class Class : TypeNode {
		public Class (TypeDefinition firstOrDefault) : base (firstOrDefault)
		{
			NodeType = NodeType.Class;
		}

		public IEnumerable<Method> GetMethods (string name, params TypeNode[] args)
		{
			IEnumerable<Method> enumerable = Methods.Where (m => m.Name == name);
			foreach (Method method in enumerable) {
				List<Parameter> parameters = method.Parameters;
				bool ok = true;
				if (args.Length != parameters.Count)
					continue;

				for (int i = 0; i < args.Length; i++) {
					if (!parameters [i].Type.Equals (args [i])) {
						ok = false;
						break;
					}
				}

				if (ok)
					yield return method;
			}
		}

		public Method GetMethod (string name, params TypeNode[] args)
		{
			return GetMethods (name, args).FirstOrDefault ();
		}
	}
}
