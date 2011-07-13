// 
// MethodCallPathElement.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
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

using Mono.CodeContracts.Static.AST;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis.Paths {
	class MethodCallPathElement : PathElement<Method> {
		private readonly bool is_boolean_typed;
		private readonly bool is_compressed;
		private readonly bool is_getter;

		public MethodCallPathElement (Method method,
		                              bool isGetter, bool isBooleanTyped,
		                              bool isStatic, string description,
		                              SymFunction func, bool isCompressed)
			: base (method, description, func)
		{
			this.is_boolean_typed = isBooleanTyped;
			this.is_getter = isGetter;
			this.is_compressed = isCompressed;
			this.isStatic = isStatic;
		}

		public override bool IsAddressOf
		{
			get { return !this.is_compressed; }
		}

		public override bool IsMethodCall
		{
			get { return true; }
		}

		public override bool IsGetter
		{
			get { return this.is_getter; }
		}

		public override bool IsBooleanTyped
		{
			get { return this.is_boolean_typed; }
		}
	}
}
