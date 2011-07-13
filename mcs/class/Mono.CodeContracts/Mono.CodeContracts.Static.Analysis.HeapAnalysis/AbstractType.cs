// 
// AbstractType.cs
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

using System;
using System.IO;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis {
	struct AbstractType : IAbstractDomainForEGraph<AbstractType>, IEquatable<AbstractType> {
		public static AbstractType TopValue = new AbstractType (FlatDomain<TypeNode>.TopValue, false);
		public static AbstractType BottomValue = new AbstractType (FlatDomain<TypeNode>.BottomValue, true);

		private FlatDomain<TypeNode> value;

		public AbstractType (FlatDomain<TypeNode> value, bool isZero) : this ()
		{
			IsZero = isZero;
			this.value = value;
		}

		public bool IsZero { get; private set; }

		public FlatDomain<TypeNode> Type
		{
			get { return this.value; }
		}

		public TypeNode ConcreteType
		{
			get { return this.value.Concrete; }
		}

		public bool IsNormal
		{
			get { return this.value.IsNormal; }
		}

		private static AbstractType ForManifestedFieldValue
		{
			get { return TopValue; }
		}

		public AbstractType ButZero
		{
			get { return new AbstractType (this.value, true); }
		}
		public AbstractType With (FlatDomain<TypeNode> type)
		{
			return new AbstractType (type, this.IsZero);
		}

		#region IAbstractDomainForEGraph<AbstractType> Members
		public AbstractType Top
		{
			get { return new AbstractType (FlatDomain<TypeNode>.TopValue, false); }
		}

		public AbstractType Bottom
		{
			get { return new AbstractType (FlatDomain<TypeNode>.BottomValue, true); }
		}

		public bool IsTop
		{
			get { return !IsZero && this.value.IsTop; }
		}

		public bool IsBottom
		{
			get { return IsZero && this.value.IsBottom; }
		}

		public AbstractType Join (AbstractType that, bool widening, out bool weaker)
		{
			if (that.IsZero) {
				weaker = false;
				if (this.value.IsBottom)
					return new AbstractType (that.value, IsZero);
				return this;
			}
			if (IsZero) {
				weaker = true;
				if (that.value.IsBottom)
					return new AbstractType (this.value, that.IsZero);
				return that;
			}

			FlatDomain<TypeNode> resultType = this.value.Join (that.value, widening, out weaker);
			return new AbstractType (resultType, false);
		}

		public AbstractType Meet (AbstractType that)
		{
			return new AbstractType (this.value.Meet (that.value), IsZero || that.IsZero);
		}

		public bool LessEqual (AbstractType that)
		{
			if (IsZero)
				return true;
			if (that.IsZero)
				return false;

			return this.value.LessEqual (that.value);
		}

		public AbstractType ImmutableVersion ()
		{
			return this;
		}

		public AbstractType Clone ()
		{
			return this;
		}

		public void Dump (TextWriter tw)
		{
			if (IsZero)
				tw.Write ("(Zero) ");

			this.value.Dump (tw);
		}

		public bool HasAllBottomFields
		{
			get { return IsZero; }
		}

		public AbstractType ForManifestedField ()
		{
			return ForManifestedFieldValue;
		}
		#endregion

		#region IEquatable<AbstractType> Members
		public bool Equals (AbstractType that)
		{
			return this.IsZero == that.IsZero && this.value.Equals (that.value);
		}
		#endregion

		public override string ToString ()
		{
			return (IsZero ? "(Zero) " : "") + this.value;
		}
	}
}
