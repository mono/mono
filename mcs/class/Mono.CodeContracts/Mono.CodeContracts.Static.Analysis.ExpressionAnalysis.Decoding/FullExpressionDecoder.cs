// 
// FullExpressionDecoder.cs
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
using System.Collections.Generic;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.Analysis.HeapAnalysis.Paths;
using Mono.CodeContracts.Static.Analysis.Numerical;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Lattices;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Decoding {
	class FullExpressionDecoder<V, E> : IFullExpressionDecoder<V, E>
		where V : IEquatable<V>
		where E : IEquatable<E> {
		public readonly VisitorForIsBinaryExpression<V, E> BinaryExpressionVisitor;
		public readonly VisitorForIsInst<V, E> IsInstVisitor;
		public readonly VisitorForIsNull<V, E> IsNullVisitor;
		public readonly VisitorForSizeOf<V, E> SizeOfVisitor;
		public readonly VisitorForIsUnaryExpression<V, E> UnaryExpressionVisitor;
		public readonly VisitorForUnderlyingVariable<V, E> UnderlyingVariableVisitor;
		public readonly VisitorForValueOf<V, E> ValueOfVisitor;
		public readonly VisitorForVariable<V, E> VariableVisitor;
		public readonly VisitorForVariablesIn<V, E> VariablesInVisitor;
		protected readonly IMetaDataProvider MetaDataProvider;

		#region Implementation of IFullExpressionDecoder<V,E>
		public bool IsVariable (E expr, out object variable)
		{
			V var;
			bool res = VisitorForVariable<V, E>.IsVariable (expr, out var, this);

			variable = var;
			return res;
		}

		public V UnderlyingVariable (E expr)
		{
			return VisitorForUnderlyingVariable<V, E>.IsUnderlyingVariable (expr, this);
		}

		public bool IsNull (E expr)
		{
			return VisitorForIsNull<V, E>.IsNull (expr, this);
		}

		public bool IsConstant (E expr, out object value, out TypeNode type)
		{
			return VisitorForValueOf<V, E>.IsConstant (expr, out value, out type, this);
		}

		public bool IsSizeof (E expr, out TypeNode type)
		{
			return VisitorForSizeOf<V, E>.IsSizeOf (expr, out type, this);
		}

		public bool IsIsinst (E expr, out E arg, out TypeNode type)
		{
			return VisitorForIsInst<V, E>.IsIsInst (expr, out type, out arg, this);
		}

		public bool IsUnaryExpression (E expr, out UnaryOperator op, out E arg)
		{
			return VisitorForIsUnaryExpression<V, E>.IsUnary (expr, out op, out arg, this);
		}

		public bool IsBinaryExpression (E expr, out BinaryOperator op, out E left, out E right)
		{
			return VisitorForIsBinaryExpression<V, E>.IsBinary (expr, out op, out left, out right, this);
		}

		public void AddFreeVariables (E expr, ISet<E> set)
		{
			VisitorForVariablesIn<V, E>.AddFreeVariables (expr, set, this);
		}

		public Sequence<PathElement> GetVariableAccessPath (E expr)
		{
			return ContextProvider.ValueContext.AccessPathList (ContextProvider.ExpressionContext.GetPC (expr), ContextProvider.ExpressionContext.Unrefine (expr), true, false);
		}

		public bool TryGetType (E expr, out TypeNode type)
		{
			FlatDomain<TypeNode> aType = ContextProvider.ExpressionContext.GetType (expr);
			if (aType.IsNormal()) {
				type = aType.Value;
				return true;
			}

			type = null;
			return false;
		}

		public bool TrySizeOfAsConstant (E expr, out int sizeAsConstant)
		{
			return TrySizeOf (expr, out sizeAsConstant);
		}

	    private bool TrySizeOf (E expr, out int sizeAsConstant)
		{
			TypeNode type;
			if (VisitorForSizeOf<V, E>.IsSizeOf (expr, out type, this)) {
				int size = this.MetaDataProvider.TypeSize (type);
				if (size != -1) {
					sizeAsConstant = size;
					return true;
				}
			}

			sizeAsConstant = -1;
			return false;
		}
		#endregion

		public FullExpressionDecoder (IMetaDataProvider metaDataProvider, IExpressionContextProvider<E, V> contextProvider)
		{
			ContextProvider = contextProvider;
			this.MetaDataProvider = metaDataProvider;
			this.VariableVisitor = new VisitorForVariable<V, E> ();
			this.UnderlyingVariableVisitor = new VisitorForUnderlyingVariable<V, E> ();
			this.UnaryExpressionVisitor = new VisitorForIsUnaryExpression<V, E> ();
			this.BinaryExpressionVisitor = new VisitorForIsBinaryExpression<V, E> ();
			this.VariablesInVisitor = new VisitorForVariablesIn<V, E> (contextProvider);
			this.ValueOfVisitor = new VisitorForValueOf<V, E> ();
			this.SizeOfVisitor = new VisitorForSizeOf<V, E> ();
			this.IsInstVisitor = new VisitorForIsInst<V, E> ();
			this.IsNullVisitor = new VisitorForIsNull<V, E> ();
		}

		public IExpressionContextProvider<E, V> ContextProvider { get; private set; }
	}
}
