using System;
using System.Collections;
using System.Collections.Generic;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Decoding;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Analysis;

namespace Mono.CodeContracts.Static.Proving.BoxedExpressions
{
		public class ExternalBox<Variable, LabeledSymbol> : BoxedExpression
			where LabeledSymbol : IEquatable<LabeledSymbol>
		{
			private readonly IFullExpressionDecoder<Variable, LabeledSymbol> decoder;
			private readonly LabeledSymbol expr;
			private Optional<Tuple<bool, BinaryOperator, BoxedExpression, BoxedExpression>> binary;
			private Optional<Tuple<bool, object, TypeNode>> constant;
			private Optional<Tuple<bool, BoxedExpression, TypeNode>> isInst;
			private Optional<Pair<bool, object>> isVar;
			private Optional<Pair<bool, TypeNode>> type;
			private Optional<Tuple<bool, UnaryOperator, BoxedExpression>> unary;
			private Optional<object> var;

			public ExternalBox (LabeledSymbol external, IFullExpressionDecoder<Variable, LabeledSymbol> decoder)
			{
				this.expr = external;
				this.decoder = decoder;
			}

			public override bool IsBinary {
				get {
					Tuple<bool, BinaryOperator, BoxedExpression, BoxedExpression> binary;
					TryGetBinaryFromCache (out binary);
					return binary.Item1;
				}
			}

			public override BinaryOperator BinaryOperator {
				get {
					Tuple<bool, BinaryOperator, BoxedExpression, BoxedExpression> binary;
					bool res = TryGetBinaryFromCache (out binary);
					if (!res)
						throw new InvalidOperationException ();

					return binary.Item2;
				}
			}

			public override BoxedExpression BinaryLeftArgument {
				get {
					Tuple<bool, BinaryOperator, BoxedExpression, BoxedExpression> binary;
					bool res = TryGetBinaryFromCache (out binary);
					if (!res)
						throw new InvalidOperationException ();

					return binary.Item3;
				}
			}

			public override BoxedExpression BinaryRightArgument {
				get {
					Tuple<bool, BinaryOperator, BoxedExpression, BoxedExpression> binary;
					bool res = TryGetBinaryFromCache (out binary);
					if (!res)
						throw new InvalidOperationException ();

					return binary.Item4;
				}
			}

			public override bool IsConstant {
				get {
					Tuple<bool, object, TypeNode> consta;
					TryGetConstantFromCache (out consta);

					return consta.Item1;
				}
			}

			public override object Constant {
				get {
					Tuple<bool, object, TypeNode> consta;
					if (!TryGetConstantFromCache (out consta))
						throw new InvalidOperationException ();

					return consta.Item2;
				}
			}

			public override object ConstantType {
				get {
					Tuple<bool, object, TypeNode> consta;
					if (!TryGetConstantFromCache (out consta))
						throw new InvalidOperationException ();

					return consta.Item3;
				}
			}

			public override bool IsIsinst {
				get {
					Tuple<bool, BoxedExpression, TypeNode> isinst;
					TryGetIsInstFromCache (out isinst);
					return isinst.Item1;
				}
			}

			public override bool IsNull {
				get { return this.decoder.IsNull (this.expr); }
			}

			public override bool IsUnary {
				get {
					Tuple<bool, UnaryOperator, BoxedExpression> unary;
					TryGetUnaryFromCache (out unary);
					return unary.Item1;
				}
			}

			public override UnaryOperator UnaryOperator {
				get {
					Tuple<bool, UnaryOperator, BoxedExpression> unary;
					if (!TryGetUnaryFromCache (out unary))
						throw new InvalidOperationException ();
					return unary.Item2;
				}
			}

			public override BoxedExpression UnaryArgument {
				get {
					Tuple<bool, UnaryOperator, BoxedExpression> unary;
					if (!TryGetUnaryFromCache (out unary))
						throw new InvalidOperationException ();
					return unary.Item3;
				}
			}

			public override bool IsSizeof {
				get {
					TypeNode type;
					return this.decoder.IsSizeof (this.expr, out type);
				}
			}

			public override bool IsVariable {
				get {
					Pair<bool, object> var1;
					TryGetIsVariableFromCache (out var1);
					return var1.Key;
				}
			}

			public override object UnderlyingVariable {
				get {
					if (!this.var.IsValid)
						this.var = this.decoder.UnderlyingVariable (this.expr);

					return this.var.Value;
				}
			}

			private bool TryGetBinaryFromCache (out Tuple<bool, BinaryOperator, BoxedExpression, BoxedExpression> binary)
			{
				if (this.binary.IsValid) {
					binary = this.binary.Value;
					return true;
				}
				BinaryOperator op;
				LabeledSymbol left;
				LabeledSymbol right;
				bool res = this.decoder.IsBinaryExpression (this.expr, out op, out left, out right);
				this.binary = binary = new Tuple<bool, BinaryOperator, BoxedExpression, BoxedExpression> (res, op, For (left, this.decoder), For (right, this.decoder));

				return res;
			}

			private bool TryGetUnaryFromCache (out Tuple<bool, UnaryOperator, BoxedExpression> unary)
			{
				if (this.unary.IsValid) {
					unary = this.unary.Value;
					return true;
				}
				UnaryOperator op;
				LabeledSymbol arg;
				bool res = this.decoder.IsUnaryExpression (this.expr, out op, out arg);
				this.unary = unary = new Tuple<bool, UnaryOperator, BoxedExpression> (res, op, For (arg, this.decoder));

				return res;
			}

			private bool TryGetIsInstFromCache (out Tuple<bool, BoxedExpression, TypeNode> isinst)
			{
				if (this.isInst.IsValid) {
					isinst = this.isInst.Value;
					return true;
				}

				LabeledSymbol arg;
				TypeNode type;
				bool res = this.decoder.IsIsinst (this.expr, out arg, out type);
				this.isInst = isinst = new Tuple<bool, BoxedExpression, TypeNode> (res, For (arg, this.decoder), type);

				return res;
			}

			private bool TryGetConstantFromCache (out Tuple<bool, object, TypeNode> result)
			{
				if (this.constant.IsValid) {
					result = this.constant.Value;
					return true;
				}
				object value;
				TypeNode type;
				bool res = this.decoder.IsConstant (this.expr, out value, out type);
				this.constant = result = new Tuple<bool, object, TypeNode> (res, value, type);

				return res;
			}

			private bool TryGetTypeFromCache (out Pair<bool, TypeNode> result)
			{
				if (this.type.IsValid) {
					result = this.type.Value;
					return true;
				}

				TypeNode type;
				bool res = this.decoder.TryGetType (this.expr, out type);
				this.type = result = new Pair<bool, TypeNode> (res, type);

				return res;
			}

			private bool TryGetIsVariableFromCache (out Pair<bool, object> result)
			{
				if (this.isVar.IsValid) {
					result = this.isVar.Value;
					return true;
				}

				object value;
				bool res = this.decoder.IsVariable (this.expr, out value);
				this.isVar = result = new Pair<bool, object> (res, value);

				return res;
			}

			public override bool IsBinaryExpression (out BinaryOperator op, out BoxedExpression left, out BoxedExpression right)
			{
				Tuple<bool, BinaryOperator, BoxedExpression, BoxedExpression> bin;
				if (!TryGetBinaryFromCache (out bin) || !bin.Item1) {
					op = BinaryOperator.Add;
					left = null;
					right = null;
					return false;
				}

				op = bin.Item2;
				left = bin.Item3;
				right = bin.Item4;
				return true;
			}

			public override bool IsIsinstExpression (out BoxedExpression expr, out TypeNode type)
			{
				Tuple<bool, BoxedExpression, TypeNode> isinst;
				if (!TryGetIsInstFromCache (out isinst) || !isinst.Item1) {
					expr = null;
					type = null;
					return false;
				}

				expr = isinst.Item2;
				type = isinst.Item3;
				return true;
			}

			public override bool IsUnaryExpression (out UnaryOperator op, out BoxedExpression argument)
			{
				Tuple<bool, UnaryOperator, BoxedExpression> unary;
				if (!TryGetUnaryFromCache (out unary) || !unary.Item1) {
					op = UnaryOperator.Conv_i;
					argument = null;
					return false;
				}

				op = unary.Item2;
				argument = unary.Item3;
				return true;
			}

			protected internal override BoxedExpression RecursiveSubstitute (BoxedExpression what, BoxedExpression replace)
			{
				return Convert (this.expr, this.decoder).Substitute (what, replace);
			}

			public override void AddFreeVariables (HashSet<BoxedExpression> set)
			{
				this.decoder.AddFreeVariables (this.expr, new SetWrapper (set, this.decoder));
			}

			public override BoxedExpression Substitute<V> (Func<V, BoxedExpression, BoxedExpression> map)
			{
				return Convert (this.expr, this.decoder).Substitute (map);
			}

			public override bool TryGetType (out object type)
			{
				Pair<bool, TypeNode> result;
				if (!TryGetTypeFromCache (out result) || !result.Key) {
					type = null;
					return false;
				}

				type = result.Value;
				return true;
			} 

			public override Result ForwardDecode<Data, Result, Visitor> (PC pc, Visitor visitor, Data data)
			{
				Tuple<bool, object, TypeNode> constant;
				if (TryGetConstantFromCache (out constant)) {
					if (constant.Item2 != null)
						return visitor.LoadConst (pc, constant.Item3, constant, Dummy.Value, data);

					return visitor.LoadNull (pc, Dummy.Value, data);
				}

				UnaryOperator op;
				LabeledSymbol arg;
				if (this.decoder.IsUnaryExpression (this.expr, out op, out arg))
					return visitor.Unary (pc, op, false, Dummy.Value, Dummy.Value, data);

				BinaryOperator bop;
				LabeledSymbol left;
				LabeledSymbol right;
				if (this.decoder.IsBinaryExpression (this.expr, out bop, out left, out right))
					return visitor.Binary (pc, bop, Dummy.Value, Dummy.Value, Dummy.Value, data);
				TypeNode type;
				if (this.decoder.IsIsinst (this.expr, out arg, out type))
					return visitor.Isinst (pc, type, Dummy.Value, Dummy.Value, data);
				if (this.decoder.IsNull (this.expr))
					return visitor.LoadNull (pc, Dummy.Value, data);
				if (this.decoder.IsSizeof (this.expr, out type))
					return visitor.Sizeof (pc, type, Dummy.Value, data);

				return visitor.Nop (pc, data);
			}

			#region Nested type: SetWrapper
			private struct SetWrapper : ISet<LabeledSymbol>, IEnumerable<LabeledSymbol>
			{
				private readonly IFullExpressionDecoder<Variable, LabeledSymbol> decoder;
				private readonly HashSet<BoxedExpression> set;

				#region Implementation of IEnumerable
				public IEnumerator<LabeledSymbol> GetEnumerator ()
				{
					throw new NotImplementedException ();
				}

				IEnumerator IEnumerable.GetEnumerator ()
				{
					return GetEnumerator ();
				}
				#endregion

				public SetWrapper (HashSet<BoxedExpression> set, IFullExpressionDecoder<Variable, LabeledSymbol> decoder)
				{
					this.set = set;
					this.decoder = decoder;
				}

				#region Implementation of ICollection<ExternalExpression>
				public void Add (LabeledSymbol item)
				{
					this.set.Add (For (item, this.decoder));
				}

				bool ISet<LabeledSymbol>.Add (LabeledSymbol item)
				{
					Add (item);
					return true;
				}

				public void UnionWith (IEnumerable<LabeledSymbol> other)
				{
					throw new NotImplementedException ();
				}

				public void IntersectWith (IEnumerable<LabeledSymbol> other)
				{
					throw new NotImplementedException ();
				}

				public void ExceptWith (IEnumerable<LabeledSymbol> other)
				{
					throw new NotImplementedException ();
				}

				public void SymmetricExceptWith (IEnumerable<LabeledSymbol> other)
				{
					throw new NotImplementedException ();
				}

				public bool IsSubsetOf (IEnumerable<LabeledSymbol> other)
				{
					throw new NotImplementedException ();
				}

				public bool IsSupersetOf (IEnumerable<LabeledSymbol> other)
				{
					throw new NotImplementedException ();
				}

				public bool IsProperSupersetOf (IEnumerable<LabeledSymbol> other)
				{
					throw new NotImplementedException ();
				}

				public bool IsProperSubsetOf (IEnumerable<LabeledSymbol> other)
				{
					throw new NotImplementedException ();
				}

				public bool Overlaps (IEnumerable<LabeledSymbol> other)
				{
					throw new NotImplementedException ();
				}

				public bool SetEquals (IEnumerable<LabeledSymbol> other)
				{
					throw new NotImplementedException ();
				}

				public void Clear ()
				{
					throw new NotImplementedException ();
				}

				public bool Contains (LabeledSymbol item)
				{
					throw new NotImplementedException ();
				}

				public void CopyTo (LabeledSymbol[] array, int arrayIndex)
				{
					throw new NotImplementedException ();
				}

				public bool Remove (LabeledSymbol item)
				{
					throw new NotImplementedException ();
				}

				public int Count {
					get { throw new NotImplementedException (); }
				}

				public bool IsReadOnly {
					get { throw new NotImplementedException (); }
				}
				#endregion
			}
			#endregion
		}
}

