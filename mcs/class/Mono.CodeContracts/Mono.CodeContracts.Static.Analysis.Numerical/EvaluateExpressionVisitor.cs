using System;
using System.Collections.Generic;

using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
    class EvaluateExpressionVisitor<TEnv, TVar, TExpr, TInterval, TNumeric> : GenericExpressionVisitor<Pair<TEnv, int>, TInterval, TVar, TExpr>
        where TEnv : IntervalEnvironmentBase<TEnv, TVar, TExpr, TInterval, TNumeric> 
        where TVar : IEquatable<TVar> 
        where TInterval : IntervalBase<TInterval, TNumeric> {

        private readonly ConstToIntervalEvaluator<TEnv, TVar, TExpr, TInterval, TNumeric> constant;
        private readonly VariableOccurences occurences;

        public EvaluateExpressionVisitor(IExpressionDecoder<TVar, TExpr> decoder)
            : base(decoder)
        {
        }

        public Sequence<TVar> DuplicatedOccurences
        {
            get { return occurences.Duplicated; }
        }

        public override TInterval Visit(TExpr expr, Pair<TEnv, int> data)
        {
            if (data.Value >= 10) // to avoid recursion
                return Default(data);

            var intv = base.Visit (expr, PlusOne (data));
            if (intv == null)
                return Default (data);

            intv = this.RefineWithTypeRanges (intv, expr, data.Key);

            var var = this.Decoder.UnderlyingVariable (expr);
            TInterval current;
            if (data.Key.TryGetValue(var, out current))
                intv = intv.Meet (current);

            return intv;
        }

        public override TInterval VisitConstant(TExpr expr, Pair<TEnv, int> data)
        {
            return constant.Visit (expr, data.Key);
        }

        private TInterval VisitBinary(TExpr left, TExpr right, TExpr original, Pair<TEnv, int> data, Func<Pair<TEnv, int>, TInterval, TInterval, TInterval> binop )
        {
            this.occurences.Add(left, right);

            var plusOne = PlusOne(data);
            var leftIntv = this.Visit(left, plusOne);
            var rightIntv = this.Visit(right, plusOne);

            return binop (data, leftIntv, rightIntv);
        }

        public override TInterval VisitAddition(TExpr left, TExpr right, TExpr original, Pair<TEnv, int> data)
        {
            return VisitBinary (left, right, original, data, (d, l, r) => d.Key.Context.Add (l, r));
        }

        public override TInterval VisitDivision(TExpr left, TExpr right, TExpr original, Pair<TEnv, int> data)
        {
            return VisitBinary(left, right, original, data, (d, l, r) => d.Key.Context.Div(l, r));
        }

        public override TInterval VisitMultiply(TExpr left, TExpr right, TExpr original, Pair<TEnv, int> data)
        {
            return VisitBinary(left, right, original, data, (d, l, r) => d.Key.Context.Mul(l, r));
        }

        public override TInterval VisitSubtraction(TExpr left, TExpr right, TExpr original, Pair<TEnv, int> data)
        {
            return VisitBinary(left, right, original, data, (d, l, r) => d.Key.Context.Sub(l, r));
        }

        public override TInterval VisitEqual(TExpr left, TExpr right, TExpr original, Pair<TEnv, int> data)
        {
            this.occurences.Add (left, right);
            
            return Default (data);
        }

        public override TInterval VisitLessThan(TExpr left, TExpr right, TExpr original, Pair<TEnv, int> data)
        {
            this.occurences.Add (left, right);

            return Default (data);
        }

        public override TInterval VisitGreaterEqualThan(TExpr left, TExpr right, TExpr original, Pair<TEnv, int> data)
        {
            this.occurences.Add(left, right);

            return Default(data);
        }

        public override TInterval VisitLessEqualThan (TExpr left, TExpr right, TExpr original, Pair<TEnv, int> data)
        {
            this.occurences.Add(left, right);

            return Default(data);
        }

        public override TInterval VisitGreaterThan (TExpr left, TExpr right, TExpr original, Pair<TEnv, int> data)
        {
            this.occurences.Add(left, right);

            return Default(data);
        }

        public override TInterval VisitUnknown (TExpr expr, Pair<TEnv, int> data)
        {
            this.occurences.Add(expr);

            return Default(data);
        }

        protected override TInterval Default(Pair<TEnv, int> data)
        {
            return data.Key.Context.TopValue;
        }

        private TInterval RefineWithTypeRanges (TInterval intv, TExpr expr, TEnv key)
        {
            switch (Decoder.TypeOf (expr))
            {
                case ExpressionType.Int32:
                    return key.Context.ApplyConversion (ExpressionOperator.ConvertToInt32, intv);
                case ExpressionType.Bool:
                    return key.Context.ApplyConversion(ExpressionOperator.ConvertToInt32, intv);
                default:
                    return intv;
            }
        }

        private static Pair<TEnv, int> PlusOne (Pair<TEnv, int> data)
        {
            return new Pair<TEnv, int> (data.Key, data.Value + 1);
        }

        private class VariableOccurences {
            private readonly Dictionary<TVar, int> occurences;
            private readonly IExpressionDecoder<TVar, TExpr> decoder;
            private Sequence<TVar> duplicated;

            public VariableOccurences (IExpressionDecoder<TVar, TExpr> decoder)
            {
                this.decoder = decoder;
                this.occurences = new Dictionary<TVar, int> ();
                this.duplicated = null;
            }

            public Sequence<TVar> Duplicated { get { return duplicated; } }

            public void Add(TVar var)
            {
                int cnt;
                if (!this.occurences.TryGetValue(var, out cnt))
                    this.occurences[var] = 1;
                else
                {
                    if (cnt == 1)
                        this.duplicated = this.duplicated.Cons (var);

                    this.occurences[var] = cnt + 1;
                }
            }

            public void Add(TExpr e)
            {
                this.Add (decoder.UnderlyingVariable (e));
            }

            public void Add(params TExpr[] exprs)
            {
                foreach (var expr in exprs)
                    this.Add (expr);
            }

            public override string ToString()
            {
                return this.occurences.ToString ();
            }
        }
    }
}