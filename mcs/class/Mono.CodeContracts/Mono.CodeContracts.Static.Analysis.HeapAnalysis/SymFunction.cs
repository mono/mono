// 
// SymFunction.cs
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
using Mono.CodeContracts.Static.Analysis.HeapAnalysis.Paths;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis {
	abstract class SymFunction : IConstantInfo, IEquatable<SymFunction>, IVisibilityCheck<Method> {
		protected readonly IMetaDataProvider MetaDataProvider;

		protected SymFunction (ref int idGen, IMetaDataProvider metaDataProvider)
		{
			this.MetaDataProvider = metaDataProvider;
			idGen++;
		}

		public abstract bool ActsAsField { get; }
		public abstract bool IsVirtualMethod { get; }
		public abstract bool IsStatic { get; }

		#region IConstantInfo Members
		public abstract bool KeepAsBottomField { get; }
		public abstract bool ManifestField { get; }
		#endregion

		#region IVisibilityCheck<Method> Members
		public abstract bool IfRootIsParameter { get; }
		public abstract bool IsAsVisibleAs (Method member);
		public abstract bool IsVisibleFrom (Method member);
		#endregion

		public abstract TypeNode FieldAddressType ();
		public abstract PathElementBase ToPathElement (bool tryCompact);

		public static Wrapper<T> For<T> (T value, ref int idGen, IMetaDataProvider metadataDecoder)
		{
			if (value is Parameter)
				return (Wrapper<T>) (object) new ParameterWrapper ((Parameter) (object) value, ref idGen, metadataDecoder);
			if (value is Method)
				return (Wrapper<T>) (object) new MethodWrapper ((Method) (object) value, ref idGen, metadataDecoder);

			return new Wrapper<T> (value, ref idGen, metadataDecoder);
		}

		#region Implementation of IEquatable<HeapAnalysis<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly>.Domain.SymFunction>
		public bool Equals (SymFunction other)
		{
			return ReferenceEquals (this, other);
		}
		#endregion
	}
}
