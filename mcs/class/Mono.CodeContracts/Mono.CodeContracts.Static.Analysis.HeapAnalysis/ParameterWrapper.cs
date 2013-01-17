// 
// ParameterWrapper.cs
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

using System;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis {
	class ParameterWrapper : Wrapper<Parameter>
	{
		private readonly int parameter_index;

		public ParameterWrapper(Parameter parameter, ref int idGen, IMetaDataProvider metaDataProvider)
			: base (parameter, ref idGen, metaDataProvider)
		{
			this.parameter_index = metaDataProvider.ParameterIndex (parameter);
		}

		public override bool ActsAsField
		{
			get { return false; }
		}

		public override TypeNode FieldAddressType()
		{
			throw new InvalidOperationException ();
		}

		public override string ToString()
		{
			return this.MetaDataProvider.Name (this.Item);
		}

		public override bool Equals(object obj)
		{
			var other = obj as ParameterWrapper;
			if (other == null)
				return false;

			return other.parameter_index == this.parameter_index;
		}

		public override int GetHashCode()
		{
			return this.parameter_index;
		}
	}
}