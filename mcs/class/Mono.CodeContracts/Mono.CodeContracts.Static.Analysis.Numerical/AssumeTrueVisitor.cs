using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    abstract class AssumeTrueVisitor<Domain, Var, Expr> : GenericExpressionVisitor<Domain, Domain, Var, Expr>
        where Domain : IAbstractDomainForEnvironments<Domain, Var, Expr>
    {
        public AssumeFalseVisitor<Domain, Var,Expr> FalseVisitor { get; set; }

        protected AssumeTrueVisitor (IExpressionDecoder<Var, Expr> decoder)
            : base (decoder)
        {
        }

        protected override Domain Default(Domain data)
        {
            return data;
        }

        public override Domain VisitConstant(Expr left, Domain data)
        {
            bool valueBool;
            int valueInt;
            
            var result = data;
            if (this.Decoder.TryValueOf(left, ExpressionType.Bool, out valueBool))
                result = valueBool ? data : data.Bottom;
            else if (this.Decoder.TryValueOf(left, ExpressionType.Int32, out valueInt))
                result = valueInt != 0 ? data : data.Bottom;

            return result;
        }

        public override Domain VisitLogicalAnd(Expr left, Expr right, Expr original, Domain data)
        {
            bool leftIsVariable = Decoder.IsVariable (left);
            bool rightIsVariable = Decoder.IsVariable (right);

            bool leftIsConstant = Decoder.IsConstant (left);
            bool rightIsConstant = Decoder.IsConstant (right);

            if (leftIsVariable && rightIsConstant || leftIsConstant && rightIsVariable)
                return data;
            
            return data.AssumeTrue(left).AssumeTrue(right);
        }

        public override Domain VisitLogicalOr(Expr left, Expr right, Expr original, Domain data)
        {
            bool leftIsVariable = Decoder.IsVariable(left);
            bool rightIsVariable = Decoder.IsVariable(right);

            bool leftIsConstant = Decoder.IsConstant(left);
            bool rightIsConstant = Decoder.IsConstant(right);

            if (leftIsVariable && rightIsConstant || leftIsConstant && rightIsVariable)
                return data;

            Domain leftBranch = data.AssumeTrue (left);
            Domain rightBranch = data.AssumeTrue (right);

            return leftBranch.Join (rightBranch);
        }

        public override Domain VisitNot(Expr expr, Domain data)
        {
            return FalseVisitor.Visit (expr, data);
        }


        protected override bool TryPolarity(Expr expr, Domain data, out bool shouldNegate)
        {
            if (base.TryPolarity(expr, data, out shouldNegate))
                return true;

            FlatDomain<bool> holds =  data.CheckIfHolds (expr);
            if (!holds.IsNormal())
                return false.Without (out shouldNegate);

            return true.With (!holds.Value, out shouldNegate);
        }    }
}