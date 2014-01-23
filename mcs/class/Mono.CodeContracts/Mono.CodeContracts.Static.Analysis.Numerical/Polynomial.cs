// 
// Polynomial.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2012 Alexander Chebaturkin
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        struct Polynomial<TVar, TExpr>
                where TVar : IEquatable<TVar> {
                readonly Monomial<TVar>[] left;
                readonly ExpressionOperator? relation;
                readonly Monomial<TVar>[] right;
                IImmutableSet<TVar> cached_variables;

                public Polynomial (Monomial<TVar> monome)
                {
                        relation = null;
                        left = new[] {monome};
                        right = null;

                        cached_variables = null;
                }

                public Polynomial (Monomial<TVar>[] monomes)
                {
                        relation = null;
                        left = monomes;
                        right = null;

                        cached_variables = null;
                }

                public Polynomial (ExpressionOperator op, Monomial<TVar>[] left, Monomial<TVar>[] right)
                {
                        relation = op;
                        this.left = left;
                        this.right = right;

                        cached_variables = null;
                }

                public Polynomial (ExpressionOperator op, Monomial<TVar>[] left, Monomial<TVar> right)
                {
                        relation = op;
                        this.left = left;
                        this.right = new[] {right};

                        cached_variables = null;
                }

                public Polynomial (ExpressionOperator op, Polynomial<TVar, TExpr> left, Polynomial<TVar, TExpr> right)
                {
                        relation = op;
                        this.left = left.left;
                        this.right = right.left;

                        cached_variables = null;
                }

                public Polynomial (Polynomial<TVar, TExpr> that)
                {
                        relation = that.relation;
                        left = DuplicateFrom (that.left);
                        right = that.right != null ? DuplicateFrom (that.right) : null;

                        cached_variables = null;
                }

                public ExpressionOperator? Relation { get { return relation; } }

                public IImmutableSet<TVar> Variables
                {
                        get
                        {
                                if (cached_variables == null) {
                                        cached_variables = ImmutableSet<TVar>.Empty ();
                                        foreach (var monome in left)
                                                cached_variables = cached_variables.AddRange (monome.Variables);
                                        if (relation.HasValue)
                                                foreach (var monome in right)
                                                        cached_variables = cached_variables.AddRange (monome.Variables);
                                }

                                return cached_variables;
                        }
                }

                public Monomial<TVar>[] Left { get { return left; } }
                public Monomial<TVar>[] Right { get { return right; } }

                public Polynomial<TVar, TExpr> LeftAsPolynomial
                {
                        get
                        {
                                Polynomial<TVar, TExpr> polynome;
                                TryToPolynomial (left, out polynome);
                                return polynome;
                        }
                }

                /// <summary>
                /// Returns true if poly is in form: 'k*x &lt; c'
                /// </summary>
                public bool IsIntervalForm
                {
                        get
                        {
                                var op = Relation;

                                if (!op.HasValue)
                                        return false;
                                if (left.Length != 1 || right.Length != 1 || left[0].Degree != 1 || !right[0].IsConstant)
                                        return false;

                                return op.Value == ExpressionOperator.LessEqualThan;
                        }
                }

                public bool IsLinear
                {
                        get
                        {
                                if (left.Any (m => !m.IsLinear))
                                        return false;
                                if (Relation.HasValue && right.Any (m => !m.IsLinear))
                                        return false;
                                return true;
                        }
                }

                static Monomial<TVar>[] DuplicateFrom (Monomial<TVar>[] original)
                {
                        var res = new Monomial<TVar>[original.Length];
                        Array.Copy (original, res, original.Length);
                        return res;
                }

                public static bool TryToPolynomial (Monomial<TVar>[] monomials, out Polynomial<TVar, TExpr> polynome)
                {
                        if (monomials.Length > 1) {
                                polynome = new Polynomial<TVar, TExpr> (monomials);
                                return polynome.TryToCanonicalForm (out polynome);
                        }

                        return true.With (new Polynomial<TVar, TExpr> (monomials), out polynome);
                }

                public bool TryToCanonicalForm (out Polynomial<TVar, TExpr> polynome)
                {
                        var poly = this;
                        if (poly.relation.HasValue) {
                                switch (poly.relation.Value) {
                                case ExpressionOperator.GreaterThan:
                                        poly = poly.SwapOperands (ExpressionOperator.LessThan);
                                        break;
                                case ExpressionOperator.GreaterEqualThan:
                                        poly = poly.SwapOperands (ExpressionOperator.LessEqualThan);
                                        break;
                                }

                                poly = poly.MoveConstantsAndMonomes ();
                                Debug.Assert (poly.relation.HasValue);

                                Monomial<TVar>[] left;
                                Monomial<TVar>[] right;
                                if (TrySimplifyMonomes (poly.left, out left) && TrySimplifyMonomes (poly.right, out right))
                                        return true.With (new Polynomial<TVar, TExpr> (poly.relation.Value, left, right), out polynome);
                        }
                        else {
                                Monomial<TVar>[] monome;
                                if (TrySimplifyMonomes (left, out monome))
                                        return true.With (new Polynomial<TVar, TExpr> (monome), out polynome);
                        }

                        return false.Without (out polynome);
                }

                static bool TrySimplifyMonomes (Monomial<TVar>[] monomes, out Monomial<TVar>[] result)
                {
                        if (monomes.Length <= 1)
                                return true.With (monomes, out result);

                        var dict = new Dictionary<ListEqual<TVar>, Monomial<TVar>> ();
                        foreach (var monomial in monomes) {
                                var key = new ListEqual<TVar> (monomial.Degree, monomial.Variables);
                                Monomial<TVar> monome;
                                if (dict.TryGetValue (key, out monome)) {
                                        Rational coeff;
                                        if (!Rational.TryAdd (monome.Coeff, monomial.Coeff, out coeff))
                                                return false.With (monomes, out result);

                                        var sum = monome.With (coeff);
                                        dict[key] = sum;
                                }
                                else
                                        dict.Add (key, monomial);
                        }

                        var left = new List<Monomial<TVar>> (dict.Count);
                        var right = new List<Monomial<TVar>> (dict.Count);

                        Monomial<TVar>? resMonome = null;
                        foreach (var pair in dict) {
                                var monome = pair.Value;
                                if (!monome.Coeff.IsZero) {
                                        if (monome.IsConstant)
                                                resMonome = monome;
                                        else if (monome.Coeff > 0L)
                                                left.Add (monome);
                                        else
                                                right.Add (monome);
                                }
                        }

                        var list = new List<Monomial<TVar>> (dict.Count);
                        list.AddRange (left);
                        list.AddRange (right);

                        if (resMonome.HasValue)
                                list.Add (resMonome.Value);
                        if (list.Count == 0)
                                list.Add (Monomial<TVar>.From (Rational.Zero));

                        return true.With (list.ToArray (), out result);
                }

                Polynomial<TVar, TExpr> MoveConstantsAndMonomes ()
                {
                        if (!relation.HasValue)
                                return this;

                        var left = new List<Monomial<TVar>> ();
                        var right = new List<Monomial<TVar>> ();

                        foreach (var monomial in this.left)
                                if (!monomial.IsConstant)
                                        left.Add (monomial);
                                else
                                        right.Add (-monomial);

                        foreach (var monomial in this.right)
                                if (!monomial.IsConstant)
                                        left.Add (-monomial);
                                else
                                        right.Add (monomial);

                        if (left.Count == 0)
                                left.Add (Monomial<TVar>.From (0L));
                        if (right.Count == 0)
                                right.Add (Monomial<TVar>.From (0));

                        return new Polynomial<TVar, TExpr> (relation.Value, left.ToArray (), right.ToArray ());
                }

                Polynomial<TVar, TExpr> SwapOperands (ExpressionOperator newOp)
                {
                        return new Polynomial<TVar, TExpr> (newOp, right, left);
                }

                public override string ToString ()
                {
                        if (!relation.HasValue)
                                return ListToString (left);

                        return string.Format ("{0} {1} {2}", ListToString (left), relation, ListToString (right));
                }

                static string ListToString (Monomial<TVar>[] list)
                {
                        if (list == null)
                                return string.Empty;
                        if (list.Length == 0)
                                return "()";

                        var sb = new StringBuilder ();

                        var stringsOrdered = list.Select (x => x.ToString ()).OrderBy (x => x);
                        var first = true;
                        sb.Append ("(");
                        foreach (var str in stringsOrdered) {
                                if (first)
                                        first = false;
                                else
                                        sb.Append (", ");
                                sb.Append (str);
                        }
                        sb.Append (")");

                        return sb.ToString ();
                }

                public static bool TryReduceCoefficients (Monomial<TVar>[] simplifiedLeft, Monomial<TVar>[] simplifiedRight)
                {
                        if (simplifiedRight[0].Coeff.IsMinValue)
                                return false;

                        var abs = Rational.Abs (simplifiedRight[0].Coeff);
                        if (abs.IsZero)
                                return true;

                        if (simplifiedLeft.Any (monome => !(monome.Coeff / abs).IsInteger))
                                return true;

                        simplifiedRight[0] = simplifiedRight[0].With ((c) => c / abs);
                        for (var i = 0; i < simplifiedLeft.Length; i++)
                                simplifiedLeft[i] = simplifiedLeft[i].With (c => c / abs);

                        return true;
                }

                public static bool TryBuildFrom (TExpr expr, IExpressionDecoder<TVar, TExpr> decoder, out Polynomial<TVar, TExpr> result)
                {
                        return new PolynomialBuilder (decoder).Build (expr, out result);
                }

                static bool TryMinus (Polynomial<TVar, TExpr> p, out Polynomial<TVar, TExpr> result)
                {
                        if (p.Relation.HasValue)
                                return false.Without (out result);

                        var monomes = new Monomial<TVar>[p.left.Length];
                        var cnt = 0;
                        foreach (var m in p.left) {
                                var k = -m.Coeff;
                                if (k.IsInfinity)
                                        return false.Without (out result);

                                var mm = m.With (k);
                                monomes[cnt++] = mm;
                        }

                        return true.With (new Polynomial<TVar, TExpr> (monomes), out result);
                }

                bool IsIntConstant (out long constant)
                {
                        if (Relation.HasValue || left.Length != 1)
                                return false.Without (out constant);

                        return left[0].IsIntConstant (out constant);
                }

                static Polynomial<TVar, TExpr> Concatenate (Polynomial<TVar, TExpr> left, Polynomial<TVar, TExpr> right)
                {
                        if (left.Relation.HasValue || right.Relation.HasValue)
                                throw new InvalidOperationException ();

                        var monomes = new Monomial<TVar>[left.left.Length + right.left.Length];

                        Array.Copy (left.left, monomes, left.left.Length);
                        Array.Copy (right.left, 0, monomes, left.left.Length, right.left.Length);

                        return new Polynomial<TVar, TExpr> (left.left);
                }

                public static bool TryToPolynomial (ExpressionOperator op, Polynomial<TVar, TExpr> left, Polynomial<TVar, TExpr> right, out Polynomial<TVar, TExpr> result)
                {
                        if (op.IsBinary () && !left.Relation.HasValue) {
                                try {
                                        var poly = new Polynomial<TVar, TExpr> (op, left, right);
                                        if (poly.TryToCanonicalForm (out result))
                                                return true;
                                }
                                catch (Exception) {
                                }
                        }

                        return false.Without (out result);
                }

                #region Nested type: ConstantGatherer

                class ConstantGatherer : GenericTypeExpressionVisitor<TVar, TExpr, Dummy, Pair<Rational, bool>> {
                        static readonly Pair<Rational, bool> Failure = new Pair<Rational, bool> (Rational.PlusInfinity, false);

                        public ConstantGatherer (IExpressionDecoder<TVar, TExpr> decoder)
                                : base (decoder)
                        {
                        }

                        protected override Pair<Rational, bool> Default (TExpr expr, Dummy input)
                        {
                                if (Decoder.IsNull (expr))
                                        return Success (Rational.Zero);

                                return Failure;
                        }

                        protected override Pair<Rational, bool> VisitBool (TExpr expr, Dummy input)
                        {
                                return Failure;
                        }

                        protected override Pair<Rational, bool> VisitInt32 (TExpr expr, Dummy input)
                        {
                                int value;
                                if (Decoder.TryValueOf (expr, ExpressionType.Int32, out value))
                                        return Success (Rational.For (value));

                                return Failure;
                        }

                        static Pair<Rational, bool> Success (Rational value)
                        {
                                return new Pair<Rational, bool> (value, true);
                        }
                }

                #endregion

                #region Nested type: ListEqual

                class ListEqual<T> {
                        readonly Lazy<int> cached_hash_code;
                        readonly T[] elements;

                        public ListEqual (int len, IEnumerable<T> seq)
                        {
                                elements = seq.Take (len).ToArray ();
                                cached_hash_code = new Lazy<int> (() => elements.Sum (elem => elem.GetHashCode ()), LazyThreadSafetyMode.None);
                        }

                        public override int GetHashCode ()
                        {
                                return cached_hash_code.Value;
                        }

                        public override bool Equals (object obj)
                        {
                                var leq = obj as ListEqual<T>;
                                if (leq == null || elements.Length != leq.elements.Length)
                                        return false;

                                if (elements.Length == 1)
                                        return elements[0].Equals (leq.elements[0]);

                                return elements.Intersect (leq.elements).Count () == elements.Count ();
                        }
                }

                #endregion

                #region Nested type: PolynomialBuilder

                class PolynomialBuilder : GenericExpressionVisitor<Dummy, bool, TVar, TExpr> {
                        Polynomial<TVar, TExpr> poly;

                        public PolynomialBuilder (IExpressionDecoder<TVar, TExpr> decoder)
                                : base (decoder)
                        {
                        }

                        public bool Build (TExpr expr, out Polynomial<TVar, TExpr> result)
                        {
                                if (base.Visit (expr, Dummy.Value))
                                        return true.With (poly, out result);

                                return false.Without (out result);
                        }

                        protected override bool Default (Dummy data)
                        {
                                return false;
                        }

                        public override bool VisitSizeOf (TExpr expr, Dummy data)
                        {
                                int sizeOf;
                                if (!Decoder.TrySizeOf (expr, out sizeOf))
                                        return false;

                                return true.With (new Polynomial<TVar, TExpr> (Monomial<TVar>.From (Rational.For (sizeOf))), out poly);
                        }

                        public override bool VisitConstant (TExpr expr, Dummy data)
                        {
                                var pair = new ConstantGatherer (Decoder).Visit (expr, Dummy.Value);
                                if (!pair.Value || pair.Key.IsInfinity)
                                        return false;

                                poly = new Polynomial<TVar, TExpr> (Monomial<TVar>.From (pair.Key));
                                return true;
                        }

                        public override bool VisitAddition (TExpr left, TExpr right, TExpr original, Dummy data)
                        {
                                Polynomial<TVar, TExpr> polyLeft, polyRight;
                                if (!Build (left, out polyLeft) || !Build (right, out polyRight))
                                        return false;

                                long kLeft, kRight;
                                object addition;
                                if (polyLeft.IsIntConstant (out kLeft) && polyRight.IsIntConstant (out kRight) &&
                                    EvaluateArithmeticWithOverflow.TryBinary (Decoder.TypeOf (original), ExpressionOperator.Add, kLeft, kRight, out addition)) {
                                        var longValue = addition.ConvertToLong ();
                                        if (longValue.HasValue)
                                                return true.With (new Polynomial<TVar, TExpr> (Monomial<TVar>.From (Rational.For (longValue.Value))), out poly);
                                }
                                return true.With (Concatenate (polyLeft, polyRight), out poly);
                        }

                        public override bool VisitSubtraction (TExpr left, TExpr right, TExpr original, Dummy data)
                        {
                                Polynomial<TVar, TExpr> polyLeft, polyRight;
                                if (!Build (left, out polyLeft) || !Build (right, out polyRight))
                                        return false;

                                long kLeft, kRight;
                                object subtraction;
                                if (polyLeft.IsIntConstant (out kLeft) && polyRight.IsIntConstant (out kRight) &&
                                    EvaluateArithmeticWithOverflow.TryBinary (Decoder.TypeOf (original), ExpressionOperator.Sub, kLeft, kRight, out subtraction)) {
                                        var longValue = subtraction.ConvertToLong ();
                                        if (longValue.HasValue)
                                                return true.With (new Polynomial<TVar, TExpr> (Monomial<TVar>.From (Rational.For (longValue.Value))), out poly);
                                }
                                return TryToPolynomialHelperForSubtraction (polyLeft, polyRight, out poly);
                        }

                        public override bool VisitMultiply (TExpr left, TExpr right, TExpr original, Dummy data)
                        {
                                Polynomial<TVar, TExpr> l, r;
                                var lBuilt = Build (left, out l);
                                var rBuilt = Build (right, out r);

                                if (lBuilt && rBuilt) {
                                        long kLeft, kRight;
                                        object mult;
                                        if (l.IsIntConstant (out kLeft) && r.IsIntConstant (out kRight) &&
                                            EvaluateArithmeticWithOverflow.TryBinary (
                                                    Decoder.TypeOf (original), ExpressionOperator.Mult, kLeft, kRight, out mult)) {
                                                var longValue = mult.ConvertToLong ();
                                                if (longValue.HasValue) {
                                                        var monomial = Monomial<TVar>.From (Rational.For (longValue.Value));
                                                        return true.With (new Polynomial<TVar, TExpr> (monomial), out poly);
                                                }
                                        }
                                        return TryToPolynomialHelperForMultiplication (l, r, out poly);
                                }
                                return false;
                        }

                        static bool TryToPolynomialHelperForMultiplication (Polynomial<TVar, TExpr> left, Polynomial<TVar, TExpr> right, out Polynomial<TVar, TExpr> result)
                        {
                                var list = new List<Monomial<TVar>> (left.left.Length + right.left.Length);
                                foreach (var m in left.left)
                                        foreach (var n in right.left) {
                                                Rational mul;
                                                if (!Rational.TryMultiply (m.Coeff, n.Coeff, out mul))
                                                        return false.Without (out result);

                                                list.Add (Monomial<TVar>.From (mul, m.Variables.Concat (n.Variables)));
                                        }

                                return true.With (new Polynomial<TVar, TExpr> (list.ToArray ()), out result);
                        }

                        static bool TryToPolynomialHelperForSubtraction (Polynomial<TVar, TExpr> left, Polynomial<TVar, TExpr> right, out Polynomial<TVar, TExpr> result)
                        {
                                Polynomial<TVar, TExpr> minusRight;
                                if (TryMinus (right, out minusRight))
                                        return true.With (Concatenate (left, right), out result);

                                return false.Without (out result);
                        }

                        public override bool VisitVariable (TVar var, TExpr expr, Dummy data)
                        {
                                var monome = Monomial<TVar>.From (Rational.One, Sequence<TVar>.Singleton (Decoder.UnderlyingVariable (expr)));

                                return true.With (new Polynomial<TVar, TExpr> (monome), out poly);
                        }

                        public override bool VisitNotEqual (TExpr left, TExpr right, TExpr original, Dummy data)
                        {
                                return DefaultRelation (left, right, original);
                        }

                        public override bool VisitLessThan (TExpr left, TExpr right, TExpr original, Dummy data)
                        {
                                return DefaultRelation (left, right, original);
                        }

                        public override bool VisitLessEqualThan (TExpr left, TExpr right, TExpr original, Dummy data)
                        {
                                return DefaultRelation (left, right, original);
                        }

                        public override bool VisitGreaterThan (TExpr left, TExpr right, TExpr original, Dummy data)
                        {
                                return DefaultRelation (left, right, original);
                        }

                        public override bool VisitGreaterEqualThan (TExpr left, TExpr right, TExpr original, Dummy data)
                        {
                                return DefaultRelation (left, right, original);
                        }

                        public override bool VisitDivision (TExpr left, TExpr right, TExpr original, Dummy data)
                        {
                                Polynomial<TVar, TExpr> l, r;
                                if (!Build (left, out l) || !Build (right, out r))
                                        return false;

                                return HelperForDivision (l, r, out poly);
                        }

                        bool HelperForDivision (Polynomial<TVar, TExpr> left, Polynomial<TVar, TExpr> right, out Polynomial<TVar, TExpr> result)
                        {
                                if (right.left.Length == 1) {
                                        var div = right.left[0];

                                        if (div.IsConstant && !div.Coeff.IsZero) {
                                                var monomes = new Monomial<TVar>[left.left.Length];
                                                var cnt = 0;
                                                foreach (var m in left.left) {
                                                        Rational k;
                                                        try {
                                                                k = m.Coeff / div.Coeff;
                                                        }
                                                        catch (ArithmeticException) {
                                                                return false.Without (out result);
                                                        }

                                                        monomes[cnt++] = m.With (k);
                                                }

                                                return true.With (new Polynomial<TVar, TExpr> (monomes), out result);
                                        }
                                }

                                return false.Without (out result);
                        }

                        bool DefaultRelation (TExpr left, TExpr right, TExpr original)
                        {
                                Polynomial<TVar, TExpr> l, r;
                                if (!Build (left, out l) || !Build (right, out r) || l.Relation.HasValue || r.Relation.HasValue)
                                        return false;

                                return true.With (new Polynomial<TVar, TExpr> (Decoder.OperatorFor (original), l.left, r.left), out poly);
                        }
                }

                #endregion
                }
}