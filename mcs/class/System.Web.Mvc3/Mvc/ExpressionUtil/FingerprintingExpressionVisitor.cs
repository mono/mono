namespace System.Web.Mvc.ExpressionUtil {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    // This is a visitor which produces a fingerprint of an expression. It doesn't
    // rewrite the expression in a form which can be compiled and cached.

    internal sealed class FingerprintingExpressionVisitor : ExpressionVisitor {

        private readonly List<object> _seenConstants = new List<object>();
        private readonly List<ParameterExpression> _seenParameters = new List<ParameterExpression>();
        private readonly ExpressionFingerprintChain _currentChain = new ExpressionFingerprintChain();
        private bool _gaveUp;

        private FingerprintingExpressionVisitor() { }

        private T GiveUp<T>(T node) {
            // We don't understand this node, so just quit.

            _gaveUp = true;
            return node;
        }

        // Returns the fingerprint chain + captured constants list for this expression, or null
        // if the expression couldn't be fingerprinted.
        public static ExpressionFingerprintChain GetFingerprintChain(Expression expr, out List<object> capturedConstants) {
            FingerprintingExpressionVisitor visitor = new FingerprintingExpressionVisitor();
            visitor.Visit(expr);

            if (visitor._gaveUp) {
                capturedConstants = null;
                return null;
            }
            else {
                capturedConstants = visitor._seenConstants;
                return visitor._currentChain;
            }
        }

        public override Expression Visit(Expression node) {
            if (node == null) {
                _currentChain.Elements.Add(null);
                return null;
            }
            else {
                return base.Visit(node);
            }
        }

        protected override Expression VisitBinary(BinaryExpression node) {
            if (_gaveUp) { return node; }
            _currentChain.Elements.Add(new BinaryExpressionFingerprint(node.NodeType, node.Type, node.Method));
            return base.VisitBinary(node);
        }

        protected override Expression VisitBlock(BlockExpression node) {
            return GiveUp(node);
        }

        protected override CatchBlock VisitCatchBlock(CatchBlock node) {
            return GiveUp(node);
        }

        protected override Expression VisitConditional(ConditionalExpression node) {
            if (_gaveUp) { return node; }
            _currentChain.Elements.Add(new ConditionalExpressionFingerprint(node.NodeType, node.Type));
            return base.VisitConditional(node);
        }

        protected override Expression VisitConstant(ConstantExpression node) {
            if (_gaveUp) { return node; }

            _seenConstants.Add(node.Value);
            _currentChain.Elements.Add(new ConstantExpressionFingerprint(node.NodeType, node.Type));
            return base.VisitConstant(node);
        }

        protected override Expression VisitDebugInfo(DebugInfoExpression node) {
            return GiveUp(node);
        }

        protected override Expression VisitDefault(DefaultExpression node) {
            if (_gaveUp) { return node; }
            _currentChain.Elements.Add(new DefaultExpressionFingerprint(node.NodeType, node.Type));
            return base.VisitDefault(node);
        }

        protected override Expression VisitDynamic(DynamicExpression node) {
            return GiveUp(node);
        }

        protected override ElementInit VisitElementInit(ElementInit node) {
            return GiveUp(node);
        }

        protected override Expression VisitExtension(Expression node) {
            return GiveUp(node);
        }

        protected override Expression VisitGoto(GotoExpression node) {
            return GiveUp(node);
        }

        protected override Expression VisitIndex(IndexExpression node) {
            if (_gaveUp) { return node; }
            _currentChain.Elements.Add(new IndexExpressionFingerprint(node.NodeType, node.Type, node.Indexer));
            return base.VisitIndex(node);
        }

        protected override Expression VisitInvocation(InvocationExpression node) {
            return GiveUp(node);
        }

        protected override Expression VisitLabel(LabelExpression node) {
            return GiveUp(node);
        }

        protected override LabelTarget VisitLabelTarget(LabelTarget node) {
            return GiveUp(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node) {
            if (_gaveUp) { return node; }
            _currentChain.Elements.Add(new LambdaExpressionFingerprint(node.NodeType, node.Type));
            return base.VisitLambda<T>(node);
        }

        protected override Expression VisitListInit(ListInitExpression node) {
            return GiveUp(node);
        }

        protected override Expression VisitLoop(LoopExpression node) {
            return GiveUp(node);
        }

        protected override Expression VisitMember(MemberExpression node) {
            if (_gaveUp) { return node; }
            _currentChain.Elements.Add(new MemberExpressionFingerprint(node.NodeType, node.Type, node.Member));
            return base.VisitMember(node);
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node) {
            return GiveUp(node);
        }

        protected override MemberBinding VisitMemberBinding(MemberBinding node) {
            return GiveUp(node);
        }

        protected override Expression VisitMemberInit(MemberInitExpression node) {
            return GiveUp(node);
        }

        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node) {
            return GiveUp(node);
        }

        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node) {
            return GiveUp(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node) {
            if (_gaveUp) { return node; }
            _currentChain.Elements.Add(new MethodCallExpressionFingerprint(node.NodeType, node.Type, node.Method));
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitNew(NewExpression node) {
            return GiveUp(node);
        }

        protected override Expression VisitNewArray(NewArrayExpression node) {
            return GiveUp(node);
        }

        protected override Expression VisitParameter(ParameterExpression node) {
            if (_gaveUp) { return node; }

            int parameterIndex = _seenParameters.IndexOf(node);
            if (parameterIndex < 0) {
                // first time seeing this parameter
                parameterIndex = _seenParameters.Count;
                _seenParameters.Add(node);
            }

            _currentChain.Elements.Add(new ParameterExpressionFingerprint(node.NodeType, node.Type, parameterIndex));
            return base.VisitParameter(node);
        }

        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node) {
            return GiveUp(node);
        }

        protected override Expression VisitSwitch(SwitchExpression node) {
            return GiveUp(node);
        }

        protected override SwitchCase VisitSwitchCase(SwitchCase node) {
            return GiveUp(node);
        }

        protected override Expression VisitTry(TryExpression node) {
            return GiveUp(node);
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression node) {
            if (_gaveUp) { return node; }
            _currentChain.Elements.Add(new TypeBinaryExpressionFingerprint(node.NodeType, node.Type, node.TypeOperand));
            return base.VisitTypeBinary(node);
        }

        protected override Expression VisitUnary(UnaryExpression node) {
            if (_gaveUp) { return node; }
            _currentChain.Elements.Add(new UnaryExpressionFingerprint(node.NodeType, node.Type, node.Method));
            return base.VisitUnary(node);
        }

    }
}
