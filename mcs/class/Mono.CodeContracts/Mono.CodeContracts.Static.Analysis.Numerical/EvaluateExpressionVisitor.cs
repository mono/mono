using System;
using System.Collections.Generic;

using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
    internal class EvaluateExpressionVisitor<TEnv, TVar, TExpr, TInterval, TNumeric> :
        GenericExpressionVisitor<Counter<TEnv>, TInterval, TVar, TExpr>
        where TEnv : IntervalEnvironmentBase<TEnv, TVar, TExpr, TInterval, TNumeric>
        where TVar : IEquatable<TVar>
        where TInterval : IntervalBase<TInterval, TNumeric> {

        private readonly ConstToIntervalEvaluator<IntervalContextBase<TInterval, TNumeric>, TVar, TExpr, TInterval, TNumeric> constToIntv;
        private readonly VariableOccurences occurences;

        public Sequence<TVar> DuplicatedOccurences { get { return this.occurences.Duplicated; } }

        public EvaluateExpressionVisitor (IExpressionDecoder<TVar, TExpr> decoder)
            : base (decoder)
        {
            this.occurences = new VariableOccurences (decoder);
            this.constToIntv = new ConstToIntervalEvaluator<IntervalContextBase<TInterval, TNumeric>, TVar, TExpr, TInterval, TNumeric>(decoder);
        }

        public override TInterval Visit (TExpr expr, Counter<TEnv> data)
        {
            if (data.Count >= 10) // to avoid recursion if any
                return this.Default (data);

            var intv = base.Visit (expr, data.Incremented ());
            if (intv == null)
                return this.Default (data);

            intv = this.RefineWithTypeRanges (intv, expr, data.Env);

            var var = this.Decoder.UnderlyingVariable (expr);
            
            TInterval current;
            if (data.Env.TryGetValue (var, out current))
                intv = intv.Meet (current);

            return intv;
        }

        public override TInterval VisitConstant (TExpr expr, Counter<TEnv> data)
        {
            return this.constToIntv.Visit (expr, data.Env.Context);
        }

        public override TInterval VisitAddition (TExpr left, TExpr right, TExpr original, Counter<TEnv> data)
        {
            return this.DefaultBinary (left, right, data, (d, l, r) => d.Env.Context.Add (l, r));
        }

        public override TInterval VisitDivision (TExpr left, TExpr right, TExpr original, Counter<TEnv> data)
        {
            return this.DefaultBinary (left, right, data, (d, l, r) => d.Env.Context.Div (l, r));
        }

        public override TInterval VisitMultiply (TExpr left, TExpr right, TExpr original, Counter<TEnv> data)
        {
            return this.DefaultBinary (left, right, data, (d, l, r) => d.Env.Context.Mul (l, r));
        }

        public override TInterval VisitSubtraction (TExpr left, TExpr right, TExpr original, Counter<TEnv> data)
        {
            return this.DefaultBinary (left, right, data, (d, l, r) => d.Env.Context.Sub (l, r));
        }

        public override TInterval VisitEqual (TExpr left, TExpr right, TExpr original, Counter<TEnv> data)
        {
            return this.DefaultComparisons (left, right, data);
        }

        public override TInterval VisitLessThan (TExpr left, TExpr right, TExpr original, Counter<TEnv> data)
        {
            return this.DefaultComparisons (left, right, data);
        }

        public override TInterval VisitGreaterEqualThan (TExpr left, TExpr right, TExpr original, Counter<TEnv> data)
        {
            return this.DefaultComparisons (left, right, data);
        }

        public override TInterval VisitLessEqualThan (TExpr left, TExpr right, TExpr original, Counter<TEnv> data)
        {
            return this.DefaultComparisons (left, right, data);
        }

        public override TInterval VisitGreaterThan (TExpr left, TExpr right, TExpr original, Counter<TEnv> data)
        {
            return this.DefaultComparisons (left, right, data);
        }

        public override TInterval VisitUnknown (TExpr expr, Counter<TEnv> data)
        {
            this.occurences.Add (expr);

            return this.Default (data);
        }

        protected override TInterval Default (Counter<TEnv> data)
        {
            return data.Env.Context.TopValue;
        }

        private delegate TInterval BinaryEvaluator (Counter<TEnv> env, TInterval left, TInterval right);

        private TInterval DefaultBinary(TExpr left, TExpr right, Counter<TEnv> data, BinaryEvaluator binop)
        {
            this.occurences.Add(left, right);

            var incremented = data.Incremented();
            var leftIntv = this.Visit(left, incremented);
            var rightIntv = this.Visit(right, incremented);

            return binop(data, leftIntv, rightIntv);
        }

        private TInterval DefaultComparisons(TExpr left, TExpr right, Counter<TEnv> data)
        {
            this.occurences.Add(left, right);

            return this.Default(data);
        }

        private TInterval RefineWithTypeRanges (TInterval intv, TExpr expr, TEnv env)
        {
            switch (this.Decoder.TypeOf (expr))
            {
                case ExpressionType.Int32:
                    return env.Context.ApplyConversion (ExpressionOperator.ConvertToInt32, intv);
                case ExpressionType.Bool:
                    return env.Context.ApplyConversion (ExpressionOperator.ConvertToInt32, intv);
                default:
                    return intv;
            }
        }

        private class VariableOccurences
        {
            private readonly Dictionary<TVar, int> occurences;

            private readonly IExpressionDecoder<TVar, TExpr> decoder;

            private Sequence<TVar> duplicated;

            public VariableOccurences (IExpressionDecoder<TVar, TExpr> decoder)
            {
                this.decoder = decoder;
                this.occurences = new Dictionary<TVar, int> ();
                this.duplicated = Sequence<TVar>.Empty;
            }

            public Sequence<TVar> Duplicated { get { return this.duplicated; } }

            private void Add (TVar var)
            {
                int cnt;
                if (!this.occurences.TryGetValue (var, out cnt))
                    cnt = 0;

                this.occurences[var] = cnt + 1;

                if (cnt == 1) // if already was occurence
                    this.duplicated = this.duplicated.Cons (var);
            }

            public void Add (TExpr e)
            {
                this.Add (this.decoder.UnderlyingVariable (e));
            }

            public void Add (params TExpr[] exprs)
            {
                foreach (var expr in exprs)
                    this.Add (expr);
            }

            public override string ToString ()
            {
                return this.occurences.ToString ();
            }
        }
    }
}