using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    struct Polynomial<Var, Expr> 
        where Var : IEquatable<Var>
    {
        private IImmutableSet<Var> cached_variables; 

        private readonly ExpressionOperator? relation;
        private readonly Monomial<Var>[] left;
        private readonly Monomial<Var>[] right;

        public Polynomial(Monomial<Var> monome)
        {
            this.relation = null;
            this.left = new[] {monome};
            this.right = null;

            cached_variables = null;
        }

        public Polynomial(Monomial<Var>[] monomes)
        {
            this.relation = null;
            this.left = monomes;
            this.right = null;

            cached_variables = null;
        }

        public Polynomial(ExpressionOperator op, Monomial<Var>[] left, Monomial<Var>[] right)
        {
            this.relation = op;
            this.left = left;
            this.right = right;

            cached_variables = null;
        }

        public Polynomial(ExpressionOperator op, Monomial<Var>[] left, Monomial<Var> right )
        {
            this.relation = op;
            this.left = left;
            this.right = new[] { right };

            cached_variables = null;
        }

        public Polynomial (ExpressionOperator op, Polynomial<Var, Expr> left, Polynomial<Var, Expr> right)
        {
            this.relation = op;
            this.left = left.left;
            this.right = right.left;

            cached_variables = null;
        }

        public Polynomial (Polynomial<Var, Expr> that)
        {
            this.relation = that.relation;
            this.left = DuplicateFrom(that.left);
            this.right = that.right != null ? DuplicateFrom (that.right) : null;

            cached_variables = null;
        }

        private static Monomial<Var>[] DuplicateFrom (Monomial<Var>[] original)
        {
            var res = new Monomial<Var>[original.Length];
            Array.Copy (original, res, original.Length);
            return res;
        }

        public ExpressionOperator? Relation { get { return relation; } }
        public IImmutableSet<Var> Variables
        {
            get
            {
                if (cached_variables == null)
                {
                    cached_variables = ImmutableSet<Var>.Empty();
                    foreach (var monome in this.left)
                        cached_variables = cached_variables.AddRange(monome.Variables);
                    if (relation.HasValue)
                        foreach (var monome in this.right)
                            cached_variables = cached_variables.AddRange(monome.Variables);
                }

                return cached_variables;
            }
        }

        public Monomial<Var>[] Left { get { return left; } }
        public Monomial<Var>[] Right { get { return right; } }

        public Polynomial<Var, Expr> LeftAsPolynomial
        {
            get
            {
                Polynomial<Var, Expr> polynome;
                TryToPolynomial(this.left, out polynome);
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
                ExpressionOperator? op = this.Relation;
                
                if (!op.HasValue)
                    return false;
                if (this.left.Length != 1 || this.right.Length != 1 || this.left[0].Degree != 1 || !this.right[0].IsConstant)
                    return false;

                return op.Value == ExpressionOperator.LessEqualThan;
            }
        }

        public bool IsLinear
        {
            get
            {
                if (this.left.Any (m => !m.IsLinear))
                    return false;
                if (this.Relation.HasValue && this.right.Any (m => !m.IsLinear))
                    return false;
                return true;
            }
        }

        public static bool TryToPolynomial(Monomial<Var>[] monomials, out Polynomial<Var, Expr> polynome)
        {
            if (monomials.Length > 1)
            {
                polynome = new Polynomial<Var, Expr>(monomials);
                return polynome.TryToCanonicalForm(out polynome);
            }
            
            return true.With(new Polynomial<Var, Expr>(monomials), out polynome);
        }

        public bool TryToCanonicalForm(out Polynomial<Var, Expr> polynome)
        {
            var poly = this;
            if (poly.relation.HasValue)
            {
                switch (poly.relation.Value)
                {
                    case ExpressionOperator.GreaterThan:
                        poly = poly.SwapOperands(ExpressionOperator.LessThan);
                        break;
                    case ExpressionOperator.GreaterEqualThan:
                        poly = poly.SwapOperands(ExpressionOperator.LessEqualThan);
                        break;
                }

                poly = poly.MoveConstantsAndMonomes();

                Monomial<Var>[] left;
                Monomial<Var>[] right;
                if (TrySimplifyMonomes(poly.left, out left) && TrySimplifyMonomes(poly.right, out right))
                    return true.With(new Polynomial<Var, Expr>(poly.relation.Value, left, right), out polynome);
            } else
            {
                Monomial<Var>[] monome;
                if (TrySimplifyMonomes(this.left, out monome))
                    return true.With(new Polynomial<Var, Expr>(monome), out polynome);
            }

            return false.Without(out polynome);
        }

        private static bool TrySimplifyMonomes(Monomial<Var>[] monomes, out Monomial<Var>[] result)
        {
            if (monomes.Length <= 1)
                return true.With (monomes, out result);

            var dict = new Dictionary<ListEqual<Var>, Monomial<Var>> ();
            foreach (var monomial in monomes)
            {
                var key = new ListEqual<Var> (monomial.Degree, monomial.Variables);
                Monomial<Var> monome;
                if (dict.TryGetValue (key, out monome))
                {
                    Rational coeff;
                    if (!Rational.TryAdd (monome.Coeff, monomial.Coeff, out coeff))
                        return false.With (monomes, out result);

                    var sum = monome.With (coeff);
                    dict[key] = sum;
                } 
                else 
                    dict.Add (key, monomial);
            }

            var left = new List<Monomial<Var>> (dict.Count);
            var right = new List<Monomial<Var>> (dict.Count);

            Monomial<Var>? resMonome = null;
            foreach (var pair in dict)
            {
                var monome = pair.Value;
                if (!monome.Coeff.IsZero)
                {
                    if (monome.IsConstant)
                        resMonome = monome;
                    else if (monome.Coeff > 0L)
                        left.Add (monome);
                    else 
                        right.Add (monome);
                }
            }

            var list = new List<Monomial<Var>> (dict.Count);
            list.AddRange (left);
            list.AddRange (right);

            if (resMonome.HasValue)
                list.Add (resMonome.Value);
            if (list.Count == 0)
                list.Add (Monomial<Var>.From (Rational.Zero));

            return true.With (list.ToArray (), out result);
        }

        private Polynomial<Var, Expr> MoveConstantsAndMonomes()
        {
            if (!relation.HasValue)
                return this;

            var left = new List<Monomial<Var>> ();
            var right = new List<Monomial<Var>> ();

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
                left.Add (Monomial<Var>.From (0L));
            if (right.Count == 0)
                right.Add (Monomial<Var>.From (0));

            return new Polynomial<Var, Expr> (this.relation.Value, left.ToArray (), right.ToArray ());
        }

        private Polynomial<Var,Expr> SwapOperands(ExpressionOperator newOp)
        {
            return new Polynomial<Var, Expr> (newOp, right, left);
        }

        public override string ToString()
        {
            if (!relation.HasValue)
                return ListToString (this.left);

            return string.Format ("{0} {1} {2}", ListToString (left), relation, ListToString (right));
        }

        private static string ListToString (Monomial<Var>[] list)
        {
            if (list == null)
                return string.Empty;
            if (list.Length == 0)
                return "()";

            var sb = new StringBuilder ();

            var stringsOrdered = list.Select (x => x.ToString ()).OrderBy (x=>x);
            bool first = true;
            sb.Append ("(");
            foreach (var str in stringsOrdered)
            {
                if (first)
                    first = false;
                else
                    sb.Append (", ");
                sb.Append (str);
            }
            sb.Append(")");

            return sb.ToString ();
        }

        private class ListEqual<Elem>
        {
            private readonly Elem[] elements;
            private readonly Lazy<int> cachedHashCode;

            public ListEqual(int len, IEnumerable<Elem> seq)
            {
                this.elements = seq.Take(len).ToArray();
                cachedHashCode = new Lazy<int>(() => elements.Sum(elem => elem.GetHashCode()), LazyThreadSafetyMode.None);
            }

            public override int GetHashCode()
            {
                return cachedHashCode.Value;
            }

            public override bool Equals(object obj)
            {
                var leq = obj as ListEqual<Elem>;
                if (leq == null || this.elements.Length != leq.elements.Length)
                    return false;

                if (elements.Length == 1)
                    return this.elements[0].Equals(leq.elements[0]);

                return elements.Intersect(leq.elements).Count() == this.elements.Count();
            }
        }

        public static bool TryReduceCoefficients(Monomial<Var>[] simplifiedLeft, Monomial<Var>[] simplifiedRight)
        {
            if (simplifiedRight[0].Coeff.IsMinValue)
                return false;

            var abs = Rational.Abs (simplifiedRight[0].Coeff);
            if (abs.IsZero)
                return true;

            if (simplifiedLeft.Any (monome => !(monome.Coeff / abs).IsInteger))
                return true;

            simplifiedRight[0] = simplifiedRight[0].With ((c) => c / abs);
            for (int i = 0; i < simplifiedLeft.Length; i++)
                simplifiedLeft[i] = simplifiedLeft[i].With (c => c / abs);

            return true;
        }

        public static bool TryBuildFrom(Expr expr, IExpressionDecoder<Var, Expr> decoder, out Polynomial<Var, Expr> result)
        {
            return new PolynomialBuilder (decoder).Build (expr, out result);
        }

        private static bool TryMinus(Polynomial<Var, Expr> p, out Polynomial<Var, Expr> result)
        {
            if (p.Relation.HasValue)
                return false.Without (out result);

            var monomes = new Monomial<Var>[p.left.Length];
            int cnt = 0;
            foreach (var m in p.left)
            {
                Rational k = -m.Coeff;
                if (k.IsInfinity)
                    return false.Without (out result);
                
                var mm = m.With (k);
                monomes[cnt++] = mm;
            }

            return true.With (new Polynomial<Var, Expr> (monomes), out result);
        }

        private class PolynomialBuilder : GenericExpressionVisitor<Dummy, bool, Var, Expr>
        {
            private Polynomial<Var, Expr> poly;

            public PolynomialBuilder (IExpressionDecoder<Var, Expr> decoder)
                : base (decoder)
            {
            }

            public bool Build (Expr expr, out Polynomial<Var, Expr> result)
            {
                if (base.Visit (expr, Dummy.Value))
                    return true.With (poly, out result);
                
                return false.Without (out result);
            }

            protected override bool Default (Dummy data)
            {
                return false;
            }

            public override bool VisitSizeOf(Expr expr, Dummy data)
            {
                int sizeOf;
                if (!this.Decoder.TrySizeOf(expr, out sizeOf))
                    return false;
             
                return true.With (new Polynomial<Var, Expr> (Monomial<Var>.From (Rational.For (sizeOf))), out poly);
            }

            public override bool VisitConstant(Expr expr, Dummy data)
            {
                var pair = new ConstantGatherer (Decoder).Visit (expr, Dummy.Value);
                if (!pair.Value || pair.Key.IsInfinity)
                    return false;

                poly = new Polynomial<Var, Expr> (Monomial<Var>.From (pair.Key));
                return true;
            }

            public override bool VisitAddition(Expr left, Expr right, Expr original, Dummy data)
            {
                Polynomial<Var, Expr> polyLeft, polyRight;
                if (!this.Build(left, out polyLeft) || !this.Build(right, out polyRight))
                    return false;

                long kLeft, kRight;
                object addition;
                if (polyLeft.IsIntConstant(out kLeft) && polyRight.IsIntConstant(out kRight) && 
                    EvaluateArithmeticWithOverflow.TryBinary<long>(Decoder.TypeOf (original), ExpressionOperator.Add, kLeft, kRight, out addition))
                {
                    var longValue = addition.ConvertToLong ();
                    if (longValue.HasValue)
                        return true.With(new Polynomial<Var, Expr>(Monomial<Var>.From(Rational.For(longValue.Value))), out poly);
                }
                return true.With (Concatenate (polyLeft, polyRight), out poly);
            }

            public override bool VisitSubtraction(Expr left, Expr right, Expr original, Dummy data)
            {
                Polynomial<Var, Expr> polyLeft, polyRight;
                if (!this.Build(left, out polyLeft) || !this.Build(right, out polyRight))
                    return false;

                long kLeft, kRight;
                object subtraction;
                if (polyLeft.IsIntConstant(out kLeft) && polyRight.IsIntConstant(out kRight) &&
                    EvaluateArithmeticWithOverflow.TryBinary(Decoder.TypeOf(original), ExpressionOperator.Sub, kLeft, kRight, out subtraction))
                {
                    var longValue = subtraction.ConvertToLong();
                    if (longValue.HasValue)
                        return true.With(new Polynomial<Var, Expr>(Monomial<Var>.From(Rational.For(longValue.Value))), out poly);
                }
                return TryToPolynomialHelperForSubtraction (polyLeft, polyRight, out poly);
            }

            public override bool VisitMultiply(Expr left, Expr right, Expr original, Dummy data)
            {
                Polynomial<Var, Expr> l, r;
                var lBuilt = this.Build (left, out l);
                var rBuilt = this.Build (right, out r);

                if (lBuilt && rBuilt)
                {
                    long kLeft, kRight;
                    object mult;
                    if (l.IsIntConstant (out kLeft) && r.IsIntConstant (out kRight) &&
                        EvaluateArithmeticWithOverflow.TryBinary (
                            Decoder.TypeOf (original), ExpressionOperator.Mult, kLeft, kRight, out mult))
                    {
                        long? longValue = mult.ConvertToLong ();
                        if (longValue.HasValue)
                        {
                            var monomial = Monomial<Var>.From (Rational.For (longValue.Value));
                            return true.With (new Polynomial<Var, Expr> (monomial), out this.poly);
                        }
                    }
                    return TryToPolynomialHelperForMultiplication (l, r, out this.poly);
                }
                return false;
            }

            private static bool TryToPolynomialHelperForMultiplication (Polynomial<Var, Expr> left, Polynomial<Var, Expr> right, out Polynomial<Var, Expr> result)
            {
                var list = new List<Monomial<Var>> (left.left.Length + right.left.Length);
                foreach (var m in left.left)
                    foreach (var n in right.left)
                    {
                        Rational mul;
                        if (!Rational.TryMultiply(m.Coeff, n.Coeff, out mul))
                            return false.Without (out result);

                        list.Add (Monomial<Var>.From (mul, m.Variables.Concat (n.Variables)));
                    }

                return true.With (new Polynomial<Var, Expr> (list.ToArray ()), out result);
            }

            private static bool TryToPolynomialHelperForSubtraction (Polynomial<Var, Expr> left, Polynomial<Var, Expr> right, out Polynomial<Var, Expr> result)
            {
                Polynomial<Var, Expr> minusRight;
                if (TryMinus(right, out minusRight))
                    return true.With (Concatenate (left, right), out result);

                return false.Without (out result);
            }

            public override bool VisitVariable(Var var, Expr expr, Dummy data)
            {
                var monome = Monomial<Var>.From (Rational.One, Sequence<Var>.Singleton (Decoder.UnderlyingVariable (expr)));

                return true.With (new Polynomial<Var, Expr> (monome), out poly);
            }

            public override bool VisitNotEqual(Expr left, Expr right, Expr original, Dummy data)
            {
                return DefaultRelation (left, right, original);
            }

            public override bool VisitLessThan (Expr left, Expr right, Expr original, Dummy data)
            {
                return DefaultRelation (left, right, original);
            }

            public override bool VisitLessEqualThan (Expr left, Expr right, Expr original, Dummy data)
            {
                return DefaultRelation (left, right, original);
            }

            public override bool VisitGreaterThan (Expr left, Expr right, Expr original, Dummy data)
            {
                return DefaultRelation (left, right, original);
            }

            public override bool VisitGreaterEqualThan (Expr left, Expr right, Expr original, Dummy data)
            {
                return DefaultRelation (left, right, original);
            }

            public override bool VisitDivision(Expr left, Expr right, Expr original, Dummy data)
            {
                Polynomial<Var, Expr> l,r;
                if (!this.Build(left, out l) || !this.Build(right, out r))
                    return false;

                return this.HelperForDivision (l, r, out poly);
            }

            private bool HelperForDivision(Polynomial<Var, Expr> left, Polynomial<Var, Expr> right, out Polynomial<Var, Expr> result)
            {
                if (right.left.Length == 1)
                {
                    var div = right.left[0];

                    if (div.IsConstant && !div.Coeff.IsZero)
                    {
                        var monomes = new Monomial<Var>[left.left.Length];
                        int cnt = 0;
                        foreach (var m in left.left)
                        {
                            Rational k;
                            try
                            {
                                k = m.Coeff / div.Coeff;
                            }
                            catch (ArithmeticException)
                            {
                                return false.Without (out result);
                            }

                            monomes[cnt++] = m.With (k);
                        }

                        return true.With (new Polynomial<Var, Expr> (monomes), out result);
                    }
                }

                return false.Without (out result);
            }

            private bool DefaultRelation (Expr left, Expr right, Expr original)
            {
                Polynomial<Var, Expr> l,r;
                if (!this.Build(left, out l) || !this.Build(right, out r) || l.Relation.HasValue || r.Relation.HasValue)
                    return false;

                return true.With (new Polynomial<Var, Expr> (Decoder.OperatorFor (original), l.left, r.left), out poly);
            }
        }

        private bool IsMonomial (out Monomial<Var> monomial)
        {
            throw new NotImplementedException ();
        }

        private bool IsIntConstant (out long constant)
        {
            if (this.Relation.HasValue || this.left.Length != 1)
                return false.Without (out constant);

            return this.left[0].IsIntConstant (out constant);
        }

        private static Polynomial<Var, Expr> Concatenate(Polynomial<Var, Expr> left, Polynomial<Var, Expr> right)
        {
            if (left.Relation.HasValue || right.Relation.HasValue)
                throw new InvalidOperationException ();

            var monomes = new Monomial<Var>[left.left.Length + right.left.Length];

            Array.Copy (left.left, monomes, left.left.Length);
            Array.Copy (right.left, 0, monomes, left.left.Length, right.left.Length);
         
            return new Polynomial<Var, Expr> (left.left);
        }

        private class ConstantGatherer : GenericTypeExpressionVisitor<Var, Expr, Dummy, Pair<Rational, bool>>
        {
            private static readonly Pair<Rational, bool> Failure = new Pair<Rational, bool>(Rational.PlusInfinity, false);

            public ConstantGatherer(IExpressionDecoder<Var, Expr> decoder)
                : base(decoder)
            {
            }

            protected override Pair<Rational, bool> Default (Expr expr, Dummy input)
            {
                if (this.Decoder.IsNull(expr))
                    return Success (Rational.Zero);

                return Failure;
            }

            protected override Pair<Rational, bool> VisitBool(Expr expr, Dummy input)
            {
                return Failure;
            }

            protected override Pair<Rational, bool> VisitInt32(Expr expr, Dummy input)
            {
                int value;
                if (Decoder.TryValueOf(expr, ExpressionType.Int32, out value))
                    return Success (Rational.For(value));

                return Failure;
            }

            private static Pair<Rational, bool> Success (Rational value)
            {
                return new Pair<Rational, bool> (value, true);
            }
        }

        public static bool TryToPolynomial(ExpressionOperator op, Polynomial<Var, Expr> left, Polynomial<Var, Expr> right, out Polynomial<Var, Expr> result)
        {
            if (op.IsBinary() && !left.Relation.HasValue)
            {
                try
                {
                    var poly = new Polynomial<Var, Expr> (op, left, right);
                    if (poly.TryToCanonicalForm(out result))
                        return true;
                }catch(Exception)
                {
                }
            }

            return false.Without (out result);
        }
    }
}
