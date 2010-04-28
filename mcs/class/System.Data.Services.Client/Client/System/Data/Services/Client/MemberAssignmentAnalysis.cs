//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.


namespace System.Data.Services.Client
{
    #region Namespaces.

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    #endregion Namespaces.

    internal class MemberAssignmentAnalysis : ExpressionVisitor
    {
        #region Fields.

        internal static readonly Expression[] EmptyExpressionArray = new Expression[0];

        private readonly Expression entity;

        private Exception incompatibleAssignmentsException;

        private bool multiplePathsFound;

        private List<Expression> pathFromEntity;

        #endregion Fields.

        #region Constructor.

        private MemberAssignmentAnalysis(Expression entity)
        {
            Debug.Assert(entity != null, "entity != null");

            this.entity = entity;
            this.pathFromEntity = new List<Expression>();
        }

        #endregion Constructor.

        #region Properties.

        internal Exception IncompatibleAssignmentsException
        {
            get { return this.incompatibleAssignmentsException; }
        }

        internal bool MultiplePathsFound
        {
            get { return this.multiplePathsFound; }
        }

        #endregion Properites.

        #region Methods.

        internal static MemberAssignmentAnalysis Analyze(Expression entityInScope, Expression assignmentExpression)
        {
            Debug.Assert(entityInScope != null, "entityInScope != null");
            Debug.Assert(assignmentExpression != null, "assignmentExpression != null");

            MemberAssignmentAnalysis result = new MemberAssignmentAnalysis(entityInScope);
            result.Visit(assignmentExpression);
            return result;
        }

        internal Exception CheckCompatibleAssignments(Type targetType, ref MemberAssignmentAnalysis previous)
        {
            if (previous == null)
            {
                previous = this;
                return null;
            }

            Expression[] previousExpressions = previous.GetExpressionsToTargetEntity();
            Expression[] candidateExpressions = this.GetExpressionsToTargetEntity();
            return CheckCompatibleAssignments(targetType, previousExpressions, candidateExpressions);
        }

        internal override Expression Visit(Expression expression)
        {
            if (this.multiplePathsFound || this.incompatibleAssignmentsException != null)
            {
                return expression;
            }

            return base.Visit(expression);
        }

        internal override Expression VisitConditional(ConditionalExpression c)
        {
            Expression result;

            var nullCheck = ResourceBinder.PatternRules.MatchNullCheck(this.entity, c);
            if (nullCheck.Match)
            {
                this.Visit(nullCheck.AssignExpression);
                result = c;
            }
            else
            {
                result = base.VisitConditional(c);
            }

            return result;
        }

        internal override Expression VisitParameter(ParameterExpression p)
        {
            if (p == this.entity)
            {
                if (this.pathFromEntity.Count != 0)
                {
                    this.multiplePathsFound = true;
                }
                else
                {
                    this.pathFromEntity.Add(p);
                }
            }

            return p;
        }

        internal override Expression VisitMemberInit(MemberInitExpression init)
        {
            Expression result = init;
            MemberAssignmentAnalysis previousNested = null;
            foreach (var binding in init.Bindings)
            {
                MemberAssignment assignment = binding as MemberAssignment;
                if (assignment == null)
                {
                    continue;
                }

                MemberAssignmentAnalysis nested = MemberAssignmentAnalysis.Analyze(this.entity, assignment.Expression);
                if (nested.MultiplePathsFound)
                {
                    this.multiplePathsFound = true;
                    break;
                }

                Exception incompatibleException = nested.CheckCompatibleAssignments(init.Type, ref previousNested);
                if (incompatibleException != null)
                {
                    this.incompatibleAssignmentsException = incompatibleException;
                    break;
                }

                if (this.pathFromEntity.Count == 0)
                {
                    this.pathFromEntity.AddRange(nested.GetExpressionsToTargetEntity());
                }
            }

            return result;
        }

        internal override Expression VisitMemberAccess(MemberExpression m)
        {
            Expression result = base.VisitMemberAccess(m);
            if (this.pathFromEntity.Contains(m.Expression))
            {
                this.pathFromEntity.Add(m);
            }

            return result;
        }

        internal override Expression VisitMethodCall(MethodCallExpression call)
        {
            if (ReflectionUtil.IsSequenceMethod(call.Method, SequenceMethod.Select))
            {
                this.Visit(call.Arguments[0]);
                return call;
            }

            return base.VisitMethodCall(call);
        }

        internal Expression[] GetExpressionsBeyondTargetEntity()
        {
            Debug.Assert(!this.multiplePathsFound, "this.multiplePathsFound -- otherwise GetExpressionsToTargetEntity won't return reliable (consistent) results");

            if (this.pathFromEntity.Count <= 1)
            {
                return EmptyExpressionArray;
            }

            Expression[] result = new Expression[1];
            result[0] = this.pathFromEntity[this.pathFromEntity.Count - 1];
            return result;
        }

        internal Expression[] GetExpressionsToTargetEntity()
        {
            Debug.Assert(!this.multiplePathsFound, "this.multiplePathsFound -- otherwise GetExpressionsToTargetEntity won't return reliable (consistent) results");

            if (this.pathFromEntity.Count <= 1)
            {
                return EmptyExpressionArray;
            }

            Expression[] result = new Expression[this.pathFromEntity.Count - 1];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = this.pathFromEntity[i];
            }

            return result;
        }

        private static Exception CheckCompatibleAssignments(Type targetType, Expression[] previous, Expression[] candidate)
        {
            Debug.Assert(targetType != null, "targetType != null");
            Debug.Assert(previous != null, "previous != null");
            Debug.Assert(candidate != null, "candidate != null");

            if (previous.Length != candidate.Length)
            {
                throw CheckCompatibleAssignmentsFail(targetType, previous, candidate);
            }

            for (int i = 0; i < previous.Length; i++)
            {
                Expression p = previous[i];
                Expression c = candidate[i];
                if (p.NodeType != c.NodeType)
                {
                    throw CheckCompatibleAssignmentsFail(targetType, previous, candidate);
                }

                if (p == c)
                {
                    continue;
                }

                if (p.NodeType != ExpressionType.MemberAccess)
                {
                    return CheckCompatibleAssignmentsFail(targetType, previous, candidate);
                }

                if (((MemberExpression)p).Member.Name != ((MemberExpression)c).Member.Name)
                {
                    return CheckCompatibleAssignmentsFail(targetType, previous, candidate);
                }
            }

            return null;
        }

        private static Exception CheckCompatibleAssignmentsFail(Type targetType, Expression[] previous, Expression[] candidate)
        {
            Debug.Assert(targetType != null, "targetType != null");
            Debug.Assert(previous != null, "previous != null");
            Debug.Assert(candidate != null, "candidate != null");

            string message = Strings.ALinq_ProjectionMemberAssignmentMismatch(targetType.FullName, previous.LastOrDefault(), candidate.LastOrDefault());
            return new NotSupportedException(message);
        }

        #endregion Methods.
    }
}
