using System;
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

        private Polynomial(Monomial<Var> monome)
        {
            this.relation = null;
            this.left = new[] {monome};
            this.right = null;

            cached_variables = null;
        }

        private Polynomial(Monomial<Var>[] monomes)
        {
            this.relation = null;
            this.left = monomes;
            this.right = null;

            cached_variables = null;
        }

        private Polynomial(ExpressionOperator op, Monomial<Var>[] left, Monomial<Var>[] right)
        {
            throw new NotImplementedException();
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

        private static bool TryToPolynomial(Monomial<Var>[] monomials, out Polynomial<Var, Expr> polynome)
        {
            if (monomials.Length > 1)
            {
                polynome = new Polynomial<Var, Expr>(monomials);
                return polynome.TryToCanonicalForm(out polynome);
            }
            
            return true.With(new Polynomial<Var, Expr>(monomials), out polynome);
        }

        private bool TryToCanonicalForm(out Polynomial<Var, Expr> polynome)
        {
            if (relation.HasValue)
            {
                switch (relation.Value)
                {
                    case ExpressionOperator.GreaterThan:
                        this.SwapOperands(ExpressionOperator.LessThan);
                        break;
                    case ExpressionOperator.GreaterEqualThan:
                        this.SwapOperands(ExpressionOperator.LessEqualThan);
                        break;
                }

                Polynomial<Var, Expr> poly = this.MoveConstantsAndMonomes();

                Monomial<Var>[] left;
                Monomial<Var>[] right;
                if (TrySimplifyMonomes(poly.left, out left) && TrySimplifyMonomes(poly.right, out right))
                    return true.With(new Polynomial<Var, Expr>(relation.Value, left, right), out polynome);
            } else
            {
                Monomial<Var>[] monome;
                if (TrySimplifyMonomes(this.left, out monome))
                    return true.With(new Polynomial<Var, Expr>(monome), out polynome);
            }

            return false.Without(out polynome);
        }

        private bool TrySimplifyMonomes(Monomial<Var>[] monomes, out Monomial<Var>[] result)
        {
            throw new NotImplementedException();
        }

        private Polynomial<Var, Expr> MoveConstantsAndMonomes()
        {
            //todo: hack it
            return this;
        }

        private void SwapOperands(ExpressionOperator newOp)
        {
            //todo: hack it
            this.relation = newOp;

        }
    }
}
