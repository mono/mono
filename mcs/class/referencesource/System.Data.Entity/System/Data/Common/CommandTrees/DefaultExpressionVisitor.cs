//---------------------------------------------------------------------
// <copyright file="DefaultExpressionVisitor.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Common.CommandTrees
{
    using System;
    using System.Collections.Generic;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;
    using CqtBuilder = System.Data.Common.CommandTrees.ExpressionBuilder.DbExpressionBuilder;

    /// <summary>
    /// Visits each element of an expression tree from a given root expression. If any element changes, the tree is
    /// rebuilt back to the root and the new root expression is returned; otherwise the original root expression is returned.
    /// </summary>
    public class DefaultExpressionVisitor : DbExpressionVisitor<DbExpression>
    {
        private readonly Dictionary<DbVariableReferenceExpression, DbVariableReferenceExpression> varMappings = new Dictionary<DbVariableReferenceExpression, DbVariableReferenceExpression>();

        protected DefaultExpressionVisitor()
        {
        }

        protected virtual void OnExpressionReplaced(DbExpression oldExpression, DbExpression newExpression)
        {
        }

        protected virtual void OnVariableRebound(DbVariableReferenceExpression fromVarRef, DbVariableReferenceExpression toVarRef)
        {
        }

        protected virtual void OnEnterScope(IEnumerable<DbVariableReferenceExpression> scopeVariables)
        {
        }

        protected virtual void OnExitScope()
        {
        }
                
        protected virtual DbExpression VisitExpression(DbExpression expression)
        {
            DbExpression newValue = null;
            if (expression != null)
            {
                newValue = expression.Accept<DbExpression>(this);
            }
            
            return newValue;
        }
                
        protected virtual IList<DbExpression> VisitExpressionList(IList<DbExpression> list)
        {
            return VisitList(list, this.VisitExpression);
        }

        protected virtual DbExpressionBinding VisitExpressionBinding(DbExpressionBinding binding)
        {
            DbExpressionBinding result = binding;
            if (binding != null)
            {
                DbExpression newInput = this.VisitExpression(binding.Expression);
                if (!object.ReferenceEquals(binding.Expression, newInput))
                {
                    result = CqtBuilder.BindAs(newInput, binding.VariableName);
                    this.RebindVariable(binding.Variable, result.Variable);
                }
            }
            return result;
        }

        protected virtual IList<DbExpressionBinding> VisitExpressionBindingList(IList<DbExpressionBinding> list)
        {
            return this.VisitList(list, this.VisitExpressionBinding);
        }

        protected virtual DbGroupExpressionBinding VisitGroupExpressionBinding(DbGroupExpressionBinding binding)
        {
            DbGroupExpressionBinding result = binding;
            if (binding != null)
            {
                DbExpression newInput = this.VisitExpression(binding.Expression);
                if (!object.ReferenceEquals(binding.Expression, newInput))
                {
                    result = CqtBuilder.GroupBindAs(newInput, binding.VariableName, binding.GroupVariableName);
                    this.RebindVariable(binding.Variable, result.Variable);
                    this.RebindVariable(binding.GroupVariable, result.GroupVariable);
                }
            }
            return result;
        }

        protected virtual DbSortClause VisitSortClause(DbSortClause clause)
        {
            DbSortClause result = clause;
            if (clause != null)
            {
                DbExpression newExpression = this.VisitExpression(clause.Expression);
                if (!object.ReferenceEquals(clause.Expression, newExpression))
                {
                    if (!string.IsNullOrEmpty(clause.Collation))
                    {
                        result = (clause.Ascending ? CqtBuilder.ToSortClause(newExpression, clause.Collation) : CqtBuilder.ToSortClauseDescending(newExpression, clause.Collation));
                    }
                    else
                    {
                        result = (clause.Ascending ? CqtBuilder.ToSortClause(newExpression) : CqtBuilder.ToSortClauseDescending(newExpression));
                    }
                }
            }
            return result;
        }

        protected virtual IList<DbSortClause> VisitSortOrder(IList<DbSortClause> sortOrder)
        {
            return VisitList(sortOrder, this.VisitSortClause);
        }

        protected virtual DbAggregate VisitAggregate(DbAggregate aggregate)
        {
            // Currently only function or group aggregate are possible
            DbFunctionAggregate functionAggregate = aggregate as DbFunctionAggregate;
            if (functionAggregate != null)
            {
                return VisitFunctionAggregate(functionAggregate);
            }

            DbGroupAggregate groupAggregate = (DbGroupAggregate)aggregate;
            return VisitGroupAggregate(groupAggregate);
        }

        protected virtual DbFunctionAggregate VisitFunctionAggregate(DbFunctionAggregate aggregate)
        {
            DbFunctionAggregate result = aggregate;
            if (aggregate != null)
            {
                EdmFunction newFunction = this.VisitFunction(aggregate.Function);
                IList<DbExpression> newArguments = this.VisitExpressionList(aggregate.Arguments);

                Debug.Assert(newArguments.Count == 1, "Function aggregate had more than one argument?");

                if (!object.ReferenceEquals(aggregate.Function, newFunction) ||
                    !object.ReferenceEquals(aggregate.Arguments, newArguments))
                {
                    if (aggregate.Distinct)
                    {
                        result = CqtBuilder.AggregateDistinct(newFunction, newArguments[0]);
                    }
                    else
                    {
                        result = CqtBuilder.Aggregate(newFunction, newArguments[0]);
                    }
                }
            }
            return result;
        }

        protected virtual DbGroupAggregate VisitGroupAggregate(DbGroupAggregate aggregate)
        {
            DbGroupAggregate result = aggregate;
            if (aggregate != null)
            {
                IList<DbExpression> newArguments = this.VisitExpressionList(aggregate.Arguments);
                Debug.Assert(newArguments.Count == 1, "Group aggregate had more than one argument?");

                if (!object.ReferenceEquals(aggregate.Arguments, newArguments))
                {
                    result = CqtBuilder.GroupAggregate(newArguments[0]);
                }
            }
            return result;
        }

        protected virtual DbLambda VisitLambda(DbLambda lambda)
        {
            EntityUtil.CheckArgumentNull(lambda, "lambda");

            DbLambda result = lambda;
            IList<DbVariableReferenceExpression> newFormals = this.VisitList(lambda.Variables, varRef =>
                {
                    TypeUsage newVarType = this.VisitTypeUsage(varRef.ResultType);
                    if (!object.ReferenceEquals(varRef.ResultType, newVarType))
                    {
                        return CqtBuilder.Variable(newVarType, varRef.VariableName);
                    }
                    else
                    {
                        return varRef;
                    }
                }
            );
            this.EnterScope(newFormals.ToArray()); // ToArray: Don't pass the List instance directly to OnEnterScope
            DbExpression newBody = this.VisitExpression(lambda.Body);
            this.ExitScope();

            if (!object.ReferenceEquals(lambda.Variables, newFormals) ||
                !object.ReferenceEquals(lambda.Body, newBody))
            {
                result = CqtBuilder.Lambda(newBody, newFormals);
            }
            return result;
        }

        // Metadata 'Visitor' methods
        protected virtual EdmType VisitType(EdmType type) { return type; }
        protected virtual TypeUsage VisitTypeUsage(TypeUsage type) { return type; }
        protected virtual EntitySetBase VisitEntitySet(EntitySetBase entitySet) { return entitySet; }
        protected virtual EdmFunction VisitFunction(EdmFunction functionMetadata) { return functionMetadata; }
                
        #region Private Implementation

        private void NotifyIfChanged(DbExpression originalExpression, DbExpression newExpression)
        {
            if (!object.ReferenceEquals(originalExpression, newExpression))
            {
                this.OnExpressionReplaced(originalExpression, newExpression);
            }
        }

        private IList<TElement> VisitList<TElement>(IList<TElement> list, Func<TElement, TElement> map)
        {
            IList<TElement> result = list;
            if(list != null)
            {
                List<TElement> newList = null;
                for (int idx = 0; idx < list.Count; idx++)
                {
                    TElement newElement = map(list[idx]);
                    if (newList == null &&
                        !object.ReferenceEquals(list[idx], newElement))
                    {
                        newList = new List<TElement>(list);
                        result = newList;
                    }

                    if (newList != null)
                    {
                        newList[idx] = newElement;
                    }
                }
            }
            return result;
        }

        private DbExpression VisitUnary(DbUnaryExpression expression, Func<DbExpression, DbExpression> callback)
        {
            DbExpression result = expression;
            DbExpression newArgument = this.VisitExpression(expression.Argument);
            if (!object.ReferenceEquals(expression.Argument, newArgument))
            {
                result = callback(newArgument);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        private DbExpression VisitTypeUnary(DbUnaryExpression expression, TypeUsage type, Func<DbExpression, TypeUsage, DbExpression> callback)
        {
            DbExpression result = expression;

            DbExpression newArgument = this.VisitExpression(expression.Argument);
            TypeUsage newType = this.VisitTypeUsage(type);

            if (!object.ReferenceEquals(expression.Argument, newArgument) ||
                !object.ReferenceEquals(type, newType))
            {
                result = callback(newArgument, newType);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        private DbExpression VisitBinary(DbBinaryExpression expression, Func<DbExpression, DbExpression, DbExpression> callback)
        {
            DbExpression result = expression;

            DbExpression newLeft = this.VisitExpression(expression.Left);
            DbExpression newRight = this.VisitExpression(expression.Right);
            if (!object.ReferenceEquals(expression.Left, newLeft) ||
                !object.ReferenceEquals(expression.Right, newRight))
            {
                result = callback(newLeft, newRight);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        private DbRelatedEntityRef VisitRelatedEntityRef(DbRelatedEntityRef entityRef)
        {
            RelationshipEndMember newSource; 
            RelationshipEndMember newTarget;
            VisitRelationshipEnds(entityRef.SourceEnd, entityRef.TargetEnd, out newSource, out newTarget);
            DbExpression newTargetRef = this.VisitExpression(entityRef.TargetEntityReference);

            if (!object.ReferenceEquals(entityRef.SourceEnd, newSource) ||
                !object.ReferenceEquals(entityRef.TargetEnd, newTarget) ||
                !object.ReferenceEquals(entityRef.TargetEntityReference, newTargetRef))
            {
                return CqtBuilder.CreateRelatedEntityRef(newSource, newTarget, newTargetRef);
            }
            else
            {
                return entityRef;
            }
        }

        private void VisitRelationshipEnds(RelationshipEndMember source, RelationshipEndMember target, out RelationshipEndMember newSource, out RelationshipEndMember newTarget)
        {
            // 
            Debug.Assert(source.DeclaringType.EdmEquals(target.DeclaringType), "Relationship ends not declared by same relationship type?");
            RelationshipType mappedType = (RelationshipType)this.VisitType(target.DeclaringType);

            newSource = mappedType.RelationshipEndMembers[source.Name];
            newTarget = mappedType.RelationshipEndMembers[target.Name];
        }

        private DbExpression VisitTerminal(DbExpression expression, Func<TypeUsage, DbExpression> reconstructor)
        {
            DbExpression result = expression;
            TypeUsage newType = this.VisitTypeUsage(expression.ResultType);
            if (!object.ReferenceEquals(expression.ResultType, newType))
            {
                result = reconstructor(newType);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        private void RebindVariable(DbVariableReferenceExpression from, DbVariableReferenceExpression to)
        {
            //
            // The variable is only considered rebound if the name and/or type is different.
            // Otherwise, the original variable reference and the new variable reference are
            // equivalent, and no rebinding of references to the old variable is necessary.
            //
            // When considering the new/old result types,  the TypeUsage instance may be equal
            // or equivalent, but the EdmType must be the same instance, so that expressions
            // such as a DbPropertyExpression with the DbVariableReferenceExpression as the Instance
            // continue to be valid.
            //
            if (!from.VariableName.Equals(to.VariableName, StringComparison.Ordinal) ||
                !object.ReferenceEquals(from.ResultType.EdmType, to.ResultType.EdmType) ||
                !from.ResultType.EdmEquals(to.ResultType))
            {
                this.varMappings[from] = to;
                this.OnVariableRebound(from, to);
            }
        }

        private DbExpressionBinding VisitExpressionBindingEnterScope(DbExpressionBinding binding)
        {
            DbExpressionBinding result = this.VisitExpressionBinding(binding);
            this.OnEnterScope(new[] { result.Variable });
            return result;
        }

        private void EnterScope(params DbVariableReferenceExpression[] scopeVars)
        {
            this.OnEnterScope(scopeVars);
        }

        private void ExitScope()
        {
            this.OnExitScope();
        }

        #endregion

        #region DbExpressionVisitor<DbExpression> Members

        public override DbExpression Visit(DbExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            throw EntityUtil.NotSupported(System.Data.Entity.Strings.Cqt_General_UnsupportedExpression(expression.GetType().FullName));
        }

        public override DbExpression Visit(DbConstantExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            // Note that it is only safe to call DbConstantExpression.GetValue because the call to
            // DbExpressionBuilder.Constant must clone immutable values (byte[]).
            return VisitTerminal(expression, newType => CqtBuilder.Constant(newType, expression.GetValue()));
        }
                
        public override DbExpression Visit(DbNullExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            return VisitTerminal(expression, CqtBuilder.Null);
        }

        public override DbExpression Visit(DbVariableReferenceExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            DbExpression result = expression;
            DbVariableReferenceExpression newRef;
            if (this.varMappings.TryGetValue(expression, out newRef))
            {
                result = newRef;
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        public override DbExpression Visit(DbParameterReferenceExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            return VisitTerminal(expression, newType => CqtBuilder.Parameter(newType, expression.ParameterName));
        }

        public override DbExpression Visit(DbFunctionExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            DbExpression result = expression;
            IList<DbExpression> newArguments = this.VisitExpressionList(expression.Arguments);
            EdmFunction newFunction = this.VisitFunction(expression.Function);
            if (!object.ReferenceEquals(expression.Arguments, newArguments) ||
                !object.ReferenceEquals(expression.Function, newFunction))
            {
                result = CqtBuilder.Invoke(newFunction, newArguments);
            }
            
            NotifyIfChanged(expression, result);
            return result;
        }
        
        public override DbExpression Visit(DbLambdaExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            DbExpression result = expression;
            IList<DbExpression> newArguments = this.VisitExpressionList(expression.Arguments);
            DbLambda newLambda = this.VisitLambda(expression.Lambda);
            
            if (!object.ReferenceEquals(expression.Arguments, newArguments) ||
                !object.ReferenceEquals(expression.Lambda, newLambda))
            {
                result = CqtBuilder.Invoke(newLambda, newArguments);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        public override DbExpression Visit(DbPropertyExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            DbExpression result = expression;
            DbExpression newInstance = this.VisitExpression(expression.Instance);
            if (!object.ReferenceEquals(expression.Instance, newInstance))
            {
                result = CqtBuilder.Property(newInstance, expression.Property.Name);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        public override DbExpression Visit(DbComparisonExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            switch(expression.ExpressionKind)
            {
                case DbExpressionKind.Equals:
                    return this.VisitBinary(expression, CqtBuilder.Equal);

                case DbExpressionKind.NotEquals:
                    return this.VisitBinary(expression, CqtBuilder.NotEqual);

                case DbExpressionKind.GreaterThan:
                    return this.VisitBinary(expression, CqtBuilder.GreaterThan);

                case DbExpressionKind.GreaterThanOrEquals:
                    return this.VisitBinary(expression, CqtBuilder.GreaterThanOrEqual);

                case DbExpressionKind.LessThan:
                    return this.VisitBinary(expression, CqtBuilder.LessThan);

                case DbExpressionKind.LessThanOrEquals:
                    return this.VisitBinary(expression, CqtBuilder.LessThanOrEqual);

                default:
                    throw EntityUtil.NotSupported();
            }
        }

        public override DbExpression Visit(DbLikeExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            DbExpression result = expression;

            DbExpression newArgument = this.VisitExpression(expression.Argument);
            DbExpression newPattern = this.VisitExpression(expression.Pattern);
            DbExpression newEscape = this.VisitExpression(expression.Escape);

            if (!object.ReferenceEquals(expression.Argument, newArgument) ||
                !object.ReferenceEquals(expression.Pattern, newPattern) ||
                !object.ReferenceEquals(expression.Escape, newEscape))
            {
                result = CqtBuilder.Like(newArgument, newPattern, newEscape);
            }
            NotifyIfChanged(expression, result);
            return result;
        }
        
        public override DbExpression Visit(DbLimitExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            DbExpression result = expression;

            DbExpression newArgument = this.VisitExpression(expression.Argument);
            DbExpression newLimit = this.VisitExpression(expression.Limit);
            
            if (!object.ReferenceEquals(expression.Argument, newArgument) ||
                !object.ReferenceEquals(expression.Limit, newLimit))
            {
                Debug.Assert(!expression.WithTies, "Limit.WithTies == true?");
                result = CqtBuilder.Limit(newArgument, newLimit);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        public override DbExpression Visit(DbIsNullExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            return VisitUnary(expression, exp =>
                {
                    if(TypeSemantics.IsRowType(exp.ResultType))
                    {
                        // 
                        return CqtBuilder.CreateIsNullExpressionAllowingRowTypeArgument(exp);
                    }
                    else
                    {
                        return CqtBuilder.IsNull(exp);
                    }
                }
            );
        }

        public override DbExpression Visit(DbArithmeticExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            DbExpression result = expression;
            IList<DbExpression> newArguments = this.VisitExpressionList(expression.Arguments);
            if (!object.ReferenceEquals(expression.Arguments, newArguments))
            {
                switch(expression.ExpressionKind)
                {
                    case DbExpressionKind.Divide:
                        result = CqtBuilder.Divide(newArguments[0], newArguments[1]);
                        break;

                    case DbExpressionKind.Minus:
                        result = CqtBuilder.Minus(newArguments[0], newArguments[1]);
                        break;

                    case DbExpressionKind.Modulo:
                        result = CqtBuilder.Modulo(newArguments[0], newArguments[1]);
                        break;

                    case DbExpressionKind.Multiply:
                        result = CqtBuilder.Multiply(newArguments[0], newArguments[1]);
                        break;

                    case DbExpressionKind.Plus:
                        result = CqtBuilder.Plus(newArguments[0], newArguments[1]);
                        break;

                    case DbExpressionKind.UnaryMinus:
                        result = CqtBuilder.UnaryMinus(newArguments[0]);
                        break;

                    default:
                        throw EntityUtil.NotSupported();
                }
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        public override DbExpression Visit(DbAndExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            return VisitBinary(expression, CqtBuilder.And);
        }

        public override DbExpression Visit(DbOrExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            return VisitBinary(expression, CqtBuilder.Or);
        }

        public override DbExpression Visit(DbNotExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            return VisitUnary(expression, CqtBuilder.Not);
        }

        public override DbExpression Visit(DbDistinctExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            return VisitUnary(expression, CqtBuilder.Distinct);
        }

        public override DbExpression Visit(DbElementExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            Func<DbExpression, DbExpression> resultConstructor;
            if (expression.IsSinglePropertyUnwrapped)
            {
                // 
                resultConstructor = CqtBuilder.CreateElementExpressionUnwrapSingleProperty;
            }
            else
            {
                resultConstructor = CqtBuilder.Element;
            }

            return VisitUnary(expression, resultConstructor);
        }

        public override DbExpression Visit(DbIsEmptyExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            return VisitUnary(expression, CqtBuilder.IsEmpty);
        }

        public override DbExpression Visit(DbUnionAllExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            return VisitBinary(expression, CqtBuilder.UnionAll);
        }

        public override DbExpression Visit(DbIntersectExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            return VisitBinary(expression, CqtBuilder.Intersect);
        }

        public override DbExpression Visit(DbExceptExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            return VisitBinary(expression, CqtBuilder.Except);
        }

        public override DbExpression Visit(DbTreatExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            return this.VisitTypeUnary(expression, expression.ResultType, CqtBuilder.TreatAs);
        }

        public override DbExpression Visit(DbIsOfExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            if (expression.ExpressionKind == DbExpressionKind.IsOfOnly)
            {
                return this.VisitTypeUnary(expression, expression.OfType, CqtBuilder.IsOfOnly);
            }
            else
            {
                return this.VisitTypeUnary(expression, expression.OfType, CqtBuilder.IsOf);
            }
        }

        public override DbExpression Visit(DbCastExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            return this.VisitTypeUnary(expression, expression.ResultType, CqtBuilder.CastTo);
        }

        public override DbExpression Visit(DbCaseExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            DbExpression result = expression;

            IList<DbExpression> newWhens = this.VisitExpressionList(expression.When);
            IList<DbExpression> newThens = this.VisitExpressionList(expression.Then);
            DbExpression newElse = this.VisitExpression(expression.Else);

            if (!object.ReferenceEquals(expression.When, newWhens) ||
                !object.ReferenceEquals(expression.Then, newThens) ||
                !object.ReferenceEquals(expression.Else, newElse))
            {
                result = CqtBuilder.Case(newWhens, newThens, newElse);
            }
            NotifyIfChanged(expression, result);
            return result;
        }
        
        public override DbExpression Visit(DbOfTypeExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            if (expression.ExpressionKind == DbExpressionKind.OfTypeOnly)
            {
                return this.VisitTypeUnary(expression, expression.OfType, CqtBuilder.OfTypeOnly);
            }
            else
            {
                return this.VisitTypeUnary(expression, expression.OfType, CqtBuilder.OfType);
            }
        }

        public override DbExpression Visit(DbNewInstanceExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            DbExpression result = expression;
            TypeUsage newType = this.VisitTypeUsage(expression.ResultType);
            IList<DbExpression> newArguments = this.VisitExpressionList(expression.Arguments);
            bool unchanged = (object.ReferenceEquals(expression.ResultType, newType) && object.ReferenceEquals(expression.Arguments, newArguments));
            if (expression.HasRelatedEntityReferences)
            {
                IList<DbRelatedEntityRef> newRefs = this.VisitList(expression.RelatedEntityReferences, this.VisitRelatedEntityRef);
                if (!unchanged ||
                    !object.ReferenceEquals(expression.RelatedEntityReferences, newRefs))
                {
                    result = CqtBuilder.CreateNewEntityWithRelationshipsExpression((EntityType)newType.EdmType, newArguments, newRefs);
                }
            }
            else
            {
                if (!unchanged)
                {
                    result = CqtBuilder.New(newType, System.Linq.Enumerable.ToArray(newArguments));
                }
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        public override DbExpression Visit(DbRefExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            DbExpression result = expression;

            EntityType targetType = (EntityType)TypeHelpers.GetEdmType<RefType>(expression.ResultType).ElementType;

            DbExpression newArgument = this.VisitExpression(expression.Argument);
            EntityType newType = (EntityType)this.VisitType(targetType);
            EntitySet newSet = (EntitySet)this.VisitEntitySet(expression.EntitySet);
            if (!object.ReferenceEquals(expression.Argument, newArgument) ||
                !object.ReferenceEquals(targetType, newType) ||
                !object.ReferenceEquals(expression.EntitySet, newSet))
            {
                result = CqtBuilder.RefFromKey(newSet, newArgument, newType);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        public override DbExpression Visit(DbRelationshipNavigationExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            DbExpression result = expression;

            RelationshipEndMember newFrom;
            RelationshipEndMember newTo;
            VisitRelationshipEnds(expression.NavigateFrom, expression.NavigateTo, out newFrom, out newTo);
            DbExpression newNavSource = this.VisitExpression(expression.NavigationSource);

            if (!object.ReferenceEquals(expression.NavigateFrom, newFrom) ||
                !object.ReferenceEquals(expression.NavigateTo, newTo) ||
                !object.ReferenceEquals(expression.NavigationSource, newNavSource))
            {
                result = CqtBuilder.Navigate(newNavSource, newFrom, newTo);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        public override DbExpression Visit(DbDerefExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            return this.VisitUnary(expression, CqtBuilder.Deref);
        }

        public override DbExpression Visit(DbRefKeyExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            return this.VisitUnary(expression, CqtBuilder.GetRefKey);
        }

        public override DbExpression Visit(DbEntityRefExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            return this.VisitUnary(expression, CqtBuilder.GetEntityRef);
        }

        public override DbExpression Visit(DbScanExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            DbExpression result = expression;

            EntitySetBase newSet = this.VisitEntitySet(expression.Target);
            if (!object.ReferenceEquals(expression.Target, newSet))
            {
                result = CqtBuilder.Scan(newSet);
            }
            NotifyIfChanged(expression, result);
            return result;
        }
                
        public override DbExpression Visit(DbFilterExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            DbExpression result = expression;

            DbExpressionBinding input = this.VisitExpressionBindingEnterScope(expression.Input);
            DbExpression predicate = this.VisitExpression(expression.Predicate);
            this.ExitScope();
            if (!object.ReferenceEquals(expression.Input, input) ||
                !object.ReferenceEquals(expression.Predicate, predicate))
            {
                result = CqtBuilder.Filter(input, predicate);
            }
            NotifyIfChanged(expression, result);
            return result;
        }
        
        public override DbExpression Visit(DbProjectExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            DbExpression result = expression;

            DbExpressionBinding input = this.VisitExpressionBindingEnterScope(expression.Input);
            DbExpression projection = this.VisitExpression(expression.Projection);
            this.ExitScope();
            if (!object.ReferenceEquals(expression.Input, input) ||
                !object.ReferenceEquals(expression.Projection, projection))
            {
                result = CqtBuilder.Project(input, projection);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        public override DbExpression Visit(DbCrossJoinExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            DbExpression result = expression;

            IList<DbExpressionBinding> newInputs = this.VisitExpressionBindingList(expression.Inputs);
            if (!object.ReferenceEquals(expression.Inputs, newInputs))
            {
                result = CqtBuilder.CrossJoin(newInputs);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        public override DbExpression Visit(DbJoinExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            DbExpression result = expression;

            DbExpressionBinding newLeft = this.VisitExpressionBinding(expression.Left);
            DbExpressionBinding newRight = this.VisitExpressionBinding(expression.Right);
            
            this.EnterScope(newLeft.Variable, newRight.Variable);
            DbExpression newCondition = this.VisitExpression(expression.JoinCondition);
            this.ExitScope();

            if (!object.ReferenceEquals(expression.Left, newLeft) ||
                !object.ReferenceEquals(expression.Right, newRight) ||
                !object.ReferenceEquals(expression.JoinCondition, newCondition))
            {
                if (DbExpressionKind.InnerJoin == expression.ExpressionKind)
                {
                    result = CqtBuilder.InnerJoin(newLeft, newRight, newCondition);
                }
                else if (DbExpressionKind.LeftOuterJoin == expression.ExpressionKind)
                {
                    result = CqtBuilder.LeftOuterJoin(newLeft, newRight, newCondition);
                }
                else
                {
                    Debug.Assert(expression.ExpressionKind == DbExpressionKind.FullOuterJoin, "DbJoinExpression had ExpressionKind other than InnerJoin, LeftOuterJoin or FullOuterJoin?");
                    result = CqtBuilder.FullOuterJoin(newLeft, newRight, newCondition);
                }
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        public override DbExpression Visit(DbApplyExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            DbExpression result = expression;

            DbExpressionBinding newInput = this.VisitExpressionBindingEnterScope(expression.Input);
            DbExpressionBinding newApply = this.VisitExpressionBinding(expression.Apply);
            this.ExitScope();

            if (!object.ReferenceEquals(expression.Input, newInput) ||
                !object.ReferenceEquals(expression.Apply, newApply))
            {
                if (DbExpressionKind.CrossApply == expression.ExpressionKind)
                {
                    result = CqtBuilder.CrossApply(newInput, newApply);
                }
                else
                {
                    Debug.Assert(expression.ExpressionKind == DbExpressionKind.OuterApply, "DbApplyExpression had ExpressionKind other than CrossApply or OuterApply?");
                    result = CqtBuilder.OuterApply(newInput, newApply);
                }
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        public override DbExpression Visit(DbGroupByExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            DbExpression result = expression;

            DbGroupExpressionBinding newInput = this.VisitGroupExpressionBinding(expression.Input);
            this.EnterScope(newInput.Variable);
            IList<DbExpression> newKeys = this.VisitExpressionList(expression.Keys);
            this.ExitScope();
            this.EnterScope(newInput.GroupVariable);
            IList<DbAggregate> newAggs = this.VisitList<DbAggregate>(expression.Aggregates, this.VisitAggregate);
            this.ExitScope();

            if (!object.ReferenceEquals(expression.Input, newInput) ||
                !object.ReferenceEquals(expression.Keys, newKeys) ||
                !object.ReferenceEquals(expression.Aggregates, newAggs))
            {
                RowType groupOutput =
                    TypeHelpers.GetEdmType<RowType>(TypeHelpers.GetEdmType<CollectionType>(expression.ResultType).TypeUsage);

                var boundKeys = groupOutput.Properties.Take(newKeys.Count).Select(p => p.Name).Zip(newKeys).ToList();
                var boundAggs = groupOutput.Properties.Skip(newKeys.Count).Select(p => p.Name).Zip(newAggs).ToList();

                result = CqtBuilder.GroupBy(newInput, boundKeys, boundAggs);
            }
            NotifyIfChanged(expression, result);
            return result;
        }
                
        public override DbExpression Visit(DbSkipExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            DbExpression result = expression;

            DbExpressionBinding newInput = this.VisitExpressionBindingEnterScope(expression.Input);
            IList<DbSortClause> newSortOrder = this.VisitSortOrder(expression.SortOrder);
            this.ExitScope();
            DbExpression newCount = this.VisitExpression(expression.Count);

            if (!object.ReferenceEquals(expression.Input, newInput) ||
                !object.ReferenceEquals(expression.SortOrder, newSortOrder) ||
                !object.ReferenceEquals(expression.Count, newCount))
            {
                result = CqtBuilder.Skip(newInput, newSortOrder, newCount);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        public override DbExpression Visit(DbSortExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            DbExpression result = expression;

            DbExpressionBinding newInput = this.VisitExpressionBindingEnterScope(expression.Input);
            IList<DbSortClause> newSortOrder = this.VisitSortOrder(expression.SortOrder);
            this.ExitScope();

            if (!object.ReferenceEquals(expression.Input, newInput) ||
                !object.ReferenceEquals(expression.SortOrder, newSortOrder))
            {
                result = CqtBuilder.Sort(newInput, newSortOrder);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        public override DbExpression Visit(DbQuantifierExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            DbExpression result = expression;

            DbExpressionBinding input = this.VisitExpressionBindingEnterScope(expression.Input);
            DbExpression predicate = this.VisitExpression(expression.Predicate);
            this.ExitScope();

            if (!object.ReferenceEquals(expression.Input, input) ||
                !object.ReferenceEquals(expression.Predicate, predicate))
            {
                if (DbExpressionKind.All == expression.ExpressionKind)
                {
                    result = CqtBuilder.All(input, predicate);
                }
                else
                {
                    Debug.Assert(expression.ExpressionKind == DbExpressionKind.Any, "DbQuantifierExpression had ExpressionKind other than All or Any?");
                    result = CqtBuilder.Any(input, predicate);
                }
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        #endregion
    }
}
