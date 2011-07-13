// 
// Parameter.cs
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

using System;
using Mono.Cecil;

namespace Mono.CodeContracts.Static.AST {
	class Parameter : Variable {
		private readonly ParameterDefinition definition;
		private Method declaringMethod;
		private bool declaringMethodSpecified;

		public Parameter () : base (NodeType.Parameter)
		{
		}

		public Parameter (ParameterDefinition definition) : base (NodeType.Parameter)
		{
			this.definition = definition;
			this.type = TypeNode.Create (definition.ParameterType);
		}

		public string Name { get; protected set; }

		public Method DeclaringMethod
		{
			get
			{
				if (!this.declaringMethodSpecified && this.declaringMethod == null) {
					var methodReference = this.definition.Method as MethodReference;
					if (methodReference == null)
						throw new NotImplementedException ("Function pointers are not implemented");

					this.declaringMethod = new Method (methodReference.Resolve ());
				}
				return this.declaringMethod;
			}
			set
			{
				this.declaringMethod = value;
				this.declaringMethodSpecified = true;
			}
		}

		public virtual int Index
		{
			get { return this.definition.Index; }
		}

		public virtual bool IsOut
		{
			get { return this.definition.IsOut; }
		}

		public override string ToString ()
		{
			return string.Format ("Parameter({0})", this.definition);
		}
	}
}
