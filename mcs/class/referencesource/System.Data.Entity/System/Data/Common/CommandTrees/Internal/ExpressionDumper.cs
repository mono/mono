//---------------------------------------------------------------------
// <copyright file="ExpressionDumper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.Data.Common;
using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees;

namespace System.Data.Common.CommandTrees.Internal
{
    /// <summary>
    /// Writes a description of a given expression, in a format determined by the specific implementation of a derived type
    /// </summary>
    internal abstract class ExpressionDumper : DbExpressionVisitor
    {
        #region Constructors
        internal ExpressionDumper() { }
        #endregion

        #region (Pseudo) Public API

        /// <summary>
        /// Begins a new Dump block with the specified name
        /// </summary>
        /// <param name="name">The name of the block</param>
        internal void Begin(string name) { Begin(name, (Dictionary<string, object>)null); }
        
        /// <summary>
        /// Begins a new Dump block with the specified name and specified attributes
        /// </summary>
        /// <param name="name">The name of the block</param>
        /// <param name="attrs">The named attributes of the block. May be null</param>
        internal abstract void  Begin(string name, Dictionary<string, object> attrs);
        
        /// <summary>
        /// Ends the Dump block with the specified name. 
        /// The caller should not assumer that this name will be verified
        /// against the last name used in a Begin call.
        /// </summary>
        /// <param name="name">The name of the block</param>
        internal abstract void  End(string name);

        /// <summary>
        /// Dumps a DbExpression by visiting it.
        /// </summary>
        /// <param name="target">The DbExpression to dump</param>
        internal void Dump(DbExpression target)
        {
            target.Accept(this);
        }

        /// <summary>
        /// Dumps a DbExpression with the specified block name preceeding and succeeding (decorating) it.
        /// </summary>
        /// <param name="e">The DbExpression to dump</param>
        /// <param name="name">The decorating block name</param>
        internal void Dump(DbExpression e, string name)
        {
            Begin(name);
            Dump(e);
            End(name);
        }

        /// <summary>
        /// Dumps a DbExpressionBinding with the specified decoration
        /// </summary>
        /// <param name="binding">The DbExpressionBinding to dump</param>
        /// <param name="name">The decorating block name</param>
        internal void Dump(DbExpressionBinding binding, string name)
        {
            Begin(name);
            Dump(binding);
            End(name);
        }

        /// <summary>
        /// Dumps a DbExpressionBinding including its VariableName and DbExpression
        /// </summary>
        /// <param name="binding">The DbExpressionBinding to dump</param>
        internal void Dump(DbExpressionBinding binding)
        {
            Begin("DbExpressionBinding", "VariableName", binding.VariableName);
            Begin("Expression");
            Dump(binding.Expression);
            End("Expression");
            End("DbExpressionBinding");
            
        }

        /// <summary>
        /// Dumps a DbGroupExpressionBinding with the specified decoration
        /// </summary>
        /// <param name="binding">The DbGroupExpressionBinding to dump</param>
        /// <param name="name">The decorating block name</param>
        internal void Dump(DbGroupExpressionBinding binding, string name)
        {
            Begin(name);
            Dump(binding);
            End(name);
        }

        /// <summary>
        /// Dumps a DbGroupExpressionBinding including its VariableName, GroupVariableName and DbExpression
        /// </summary>
        /// <param name="binding">The DbGroupExpressionBinding to dump</param>
        internal void Dump(DbGroupExpressionBinding binding)
        {
            Begin("DbGroupExpressionBinding", "VariableName", binding.VariableName, "GroupVariableName", binding.GroupVariableName);
            Begin("Expression");
            Dump(binding.Expression);
            End("Expression");
            End("DbGroupExpressionBinding");
        }

        /// <summary>
        /// Dumps each DbExpression in the specified enumerable. The entire output is decorated with the 'pluralName'
        /// block name while each element DbExpression is decorated with the 'singularName' block name.
        /// If the list is empty only the pluralName decoration start/end will appear.
        /// </summary>
        /// <param name="exprs">The enumerable list of Expressions to dump</param>
        /// <param name="pluralName">The overall list decoration block name</param>
        /// <param name="singularName">The decoration block name that will be applied to each element DbExpression</param>
        internal void Dump(IEnumerable<DbExpression> exprs, string pluralName, string singularName)
        {
            Begin(pluralName);

            foreach (DbExpression expr in exprs)
            {
                Begin(singularName);
                this.Dump(expr);
                End(singularName);
            }

            End(pluralName);
        }

        /// <summary>
        /// Dumps each Parameter metadata in the specified enumerable. The entire output is decorated with the "Parameters"
        /// block name while each metadata element is decorated with the "Parameter" block name.
        /// If the list is empty only the "Parameters" decoration start/end will appear.
        /// </summary>
        /// <param name="paramList">The enumerable list of Parameter metadata to dump</param>
        internal void Dump(IEnumerable<FunctionParameter> paramList)
        {
            Begin("Parameters");
            foreach (FunctionParameter param in paramList)
            {
                Begin("Parameter", "Name", param.Name);
                Dump(param.TypeUsage, "ParameterType");
                End("Parameter");
            }
            End("Parameters");
        }

        /// <summary>
        /// Dumps the specified Type metadata instance with the specified decoration
        /// </summary>
        /// <param name="type">The Type metadata to dump</param>
        /// <param name="name">The decorating block name</param>
        internal void Dump(TypeUsage type, string name)
        {
            Begin(name);
            Dump(type);
            End(name);
        }

        /// <summary>
        /// Dumps the specified Type metadata instance
        /// </summary>
        /// <param name="type">The Type metadata to dump</param>
        internal void Dump(TypeUsage type)
        {
            Dictionary<string, object> facetInfo = new Dictionary<string, object>();
            foreach (Facet facet in type.Facets)
            {
                facetInfo.Add(facet.Name, facet.Value);
            }

            Begin("TypeUsage", facetInfo);
            Dump(type.EdmType);   
            End("TypeUsage");
        }

        /// <summary>
        /// Dumps the specified EDM type metadata instance with the specified decoration
        /// </summary>
        /// <param name="type">The type metadata to dump</param>
        /// <param name="name">The decorating block name</param>
        internal void Dump(EdmType type, string name)
        {
            Begin(name);
            Dump(type);
            End(name);
        }

        /// <summary>
        /// Dumps the specified type metadata instance
        /// </summary>
        /// <param name="type">The type metadata to dump</param>
        internal void Dump(EdmType type)
        {
            Begin("EdmType",
                    "BuiltInTypeKind", Enum.GetName(typeof(BuiltInTypeKind), type.BuiltInTypeKind),
                    "Namespace", type.NamespaceName,
                    "Name", type.Name);
            End("EdmType");
        }

        /// <summary>
        /// Dumps the specified Relation metadata instance with the specified decoration
        /// </summary>
        /// <param name="type">The Relation metadata to dump</param>
        /// <param name="name">The decorating block name</param>
        internal void Dump(RelationshipType type, string name)
        {
            Begin(name);
            Dump(type);
            End(name);
        }

        /// <summary>
        /// Dumps the specified Relation metadata instance
        /// </summary>
        /// <param name="type">The Relation metadata to dump</param>
        internal void Dump(RelationshipType type)
        {
            Begin(
                "RelationshipType",
                "Namespace", type.NamespaceName,
                "Name",
                type.Name
            );
            End("RelationshipType");
        }

        /// <summary>
        /// Dumps the specified EdmFunction metadata instance
        /// </summary>
        /// <param name="function">The EdmFunction metadata to dump.</param>
        internal void Dump(EdmFunction function)
        {
            Begin("Function", "Name", function.Name, "Namespace", function.NamespaceName);
            Dump(function.Parameters);
            if (function.ReturnParameters.Count == 1)
            {
                Dump(function.ReturnParameters[0].TypeUsage, "ReturnType");
            }
            else
            {
                Begin("ReturnTypes");
                foreach (var returnParameter in function.ReturnParameters)
                {
                    Dump(returnParameter.TypeUsage, returnParameter.Name);
                }
                End("ReturnTypes");
            }
            End("Function");
        }

        /// <summary>
        /// Dumps the specified EdmProperty metadata instance
        /// </summary>
        /// <param name="prop">The EdmProperty metadata to dump</param>
        internal void Dump(EdmProperty prop)
        {
            Begin("Property", "Name", prop.Name, "Nullable", prop.Nullable);
            Dump(prop.DeclaringType, "DeclaringType");
            Dump(prop.TypeUsage, "PropertyType");
            End("Property");
        }

        /// <summary>
        /// Dumps the specified Relation End EdmMember metadata instance with the specified decoration
        /// </summary>
        /// <param name="end">The Relation End metadata to dump</param>
        /// <param name="name">The decorating block name</param>
        internal void Dump(RelationshipEndMember end, string name)
        {
            Begin(name);
            Begin(
                "RelationshipEndMember",
                "Name", end.Name,
                //"IsParent", end.IsParent,
                "RelationshipMultiplicity", Enum.GetName(typeof(RelationshipMultiplicity), end.RelationshipMultiplicity)
            );
            Dump(end.DeclaringType, "DeclaringRelation");
            Dump(end.TypeUsage, "EndType");
            End("RelationshipEndMember");
            End(name);
        }

        /// <summary>
        /// Dumps the specified Navigation Property EdmMember metadata instance with the specified decoration
        /// </summary>
        /// <param name="navProp">The Navigation Property metadata to dump</param>
        /// <param name="name">The decorating block name</param>
        internal void Dump(NavigationProperty navProp, string name)
        {
            Begin(name);
            Begin(
                "NavigationProperty",
                "Name", navProp.Name,
                //"IsParent", end.IsParent,
                "RelationshipTypeName", navProp.RelationshipType.FullName,
                "ToEndMemberName", navProp.ToEndMember.Name
            );
            Dump(navProp.DeclaringType, "DeclaringType");
            Dump(navProp.TypeUsage, "PropertyType");
            End("NavigationProperty");
            End(name);
        }

#if METHOD_EXPRESSION
        /// <summary>
        /// Dumps the specified Method metadata instance
        /// </summary>
        /// <param name="meth">The Method metadata to dump</param>
        internal void Dump(MethodMetadata meth)
        {
            Begin("MethodMetadata", "Name", meth.Name, "IsStatic", meth.IsStatic);
            Dump(meth.DefiningType, "DeclaringType");
            Dump(meth.Parameters);
            Dump(meth.Type, "ReturnType");
            End("MethodMetadata");
        }
#endif

        /// <summary>
        /// Dumps the specified DbLambda instance
        /// </summary>
        /// <param name="lambda">The DbLambda to dump.</param>
        internal void Dump(DbLambda lambda)
        {
            Begin("DbLambda");
            Dump(System.Linq.Enumerable.Cast<DbExpression>(lambda.Variables), "Variables", "Variable");
            Dump(lambda.Body, "Body");
            End("DbLambda");
        }

        #endregion

        #region Private Implementation
        private void Begin(DbExpression expr)
        {
            Begin(expr, new Dictionary<string, object>());
        }

        private void Begin(DbExpression expr, Dictionary<string, object> attrs)
        {            
            attrs.Add("DbExpressionKind", Enum.GetName(typeof(DbExpressionKind), expr.ExpressionKind));
            Begin(expr.GetType().Name, attrs);
            Dump(expr.ResultType, "ResultType");
        }

        private void Begin(DbExpression expr, string attributeName, object attributeValue)
        {
            Dictionary<string, object> attrs = new Dictionary<string, object>();
            attrs.Add(attributeName, attributeValue);
            Begin(expr, attrs);
        }

        private void Begin(string expr, string attributeName, object attributeValue)
        {
            Dictionary<string, object> attrs = new Dictionary<string, object>();
            attrs.Add(attributeName, attributeValue);
            Begin(expr, attrs);
        }
                
        private void Begin(string expr,
                           string attributeName1,
                           object attributeValue1,
                           string attributeName2,
                           object attributeValue2)
        {
            Dictionary<string, object> attrs = new Dictionary<string, object>();
            attrs.Add(attributeName1, attributeValue1);
            attrs.Add(attributeName2, attributeValue2);
            Begin(expr, attrs);
        }
                
        private void Begin(string expr,
                           string attributeName1,
                           object attributeValue1,
                           string attributeName2,
                           object attributeValue2,
                           string attributeName3,
                           object attributeValue3)
        {
            Dictionary<string, object> attrs = new Dictionary<string, object>();
            attrs.Add(attributeName1, attributeValue1);
            attrs.Add(attributeName2, attributeValue2);
            attrs.Add(attributeName3, attributeValue3);
            Begin(expr, attrs);
        }

        private void End(DbExpression expr)
        {
            End(expr.GetType().Name);
        }

        private void BeginUnary(DbUnaryExpression e)
        {
            Begin(e);
            Begin("Argument");
            Dump(e.Argument);
            End("Argument");
        }

        private void BeginBinary(DbBinaryExpression e)
        {
            Begin(e);
            Begin("Left");
            Dump(e.Left);
            End("Left");
            Begin("Right");
            Dump(e.Right);
            End("Right");
        }
        #endregion

        #region DbExpressionVisitor<DbExpression> Members

        public override void Visit(DbExpression e)
        {
            Begin(e);
            End(e);
        }

        public override void Visit(DbConstantExpression e)
        {
            Dictionary<string, object> attrs = new Dictionary<string,object>();
            attrs.Add("Value", e.Value);
            Begin(e, attrs);
            End(e);
        }

        public override void Visit(DbNullExpression e)
        {
            Begin(e);
            End(e);
        }

        public override void Visit(DbVariableReferenceExpression e)
        {
            Dictionary<string, object> attrs = new Dictionary<string, object>();
            attrs.Add("VariableName", e.VariableName);
            Begin(e, attrs);
            End(e);
        }

        public override void Visit(DbParameterReferenceExpression e)
        {
            Dictionary<string, object> attrs = new Dictionary<string, object>();
            attrs.Add("ParameterName", e.ParameterName);
            Begin(e, attrs);
            End(e);
        }

        public override void Visit(DbFunctionExpression e)
        {
            Begin(e);
            Dump(e.Function);
            Dump(e.Arguments, "Arguments", "Argument");
            End(e);
        }

        public override void Visit(DbLambdaExpression expression)
        {
            Begin(expression);
            Dump(expression.Lambda);
            Dump(expression.Arguments, "Arguments", "Argument");
            End(expression);
        }

#if METHOD_EXPRESSION
        public override void Visit(MethodExpression e)
        {
            Begin(e);
            Dump(e.Method);
            Dump(e.Arguments, "Arguments", "Argument");
            if (e.Instance != null)
            {
                Dump(e.Instance, "Instance");
            }
            End(e);
        }
#endif

        public override void Visit(DbPropertyExpression e)
        {
            //
            // Currently the DbPropertyExpression.EdmProperty member property may only be either:
            // - EdmProperty 
            // - RelationshipEndMember
            // - NavigationProperty
            //
            Begin(e);
            RelationshipEndMember end = e.Property as RelationshipEndMember;
            if (end != null)
            {
                Dump(end, "Property");
            }
            else if (Helper.IsNavigationProperty(e.Property))
            {
                Dump((NavigationProperty)e.Property, "Property");
            }
            else
            {
                Dump((EdmProperty)e.Property);
            }
                        
            if (e.Instance != null)
            {
                Dump(e.Instance, "Instance");
            }
            End(e);
        }

        public override void Visit(DbComparisonExpression e)
        {
            BeginBinary(e);
            End(e);
        }

        public override void Visit(DbLikeExpression e)
        {
            Begin(e);
            Dump(e.Argument, "Argument");
            Dump(e.Pattern, "Pattern");
            Dump(e.Escape, "Escape");
            End(e);
        }

        public override void Visit(DbLimitExpression e)
        {
            Begin(e, "WithTies", e.WithTies);
            Dump(e.Argument, "Argument");
            Dump(e.Limit, "Limit");
            End(e);
        }

        public override void Visit(DbIsNullExpression e)
        {
            BeginUnary(e);
            End(e);
        }

        public override void Visit(DbArithmeticExpression e)
        {
            Begin(e);
            Dump(e.Arguments, "Arguments", "Argument");
            End(e);
        }

        public override void Visit(DbAndExpression e)
        {
            BeginBinary(e);
            End(e);
        }

        public override void Visit(DbOrExpression e)
        {
            BeginBinary(e);
            End(e);
        }

        public override void Visit(DbNotExpression e)
        {
            BeginUnary(e);
            End(e);
        }

        public override void Visit(DbDistinctExpression e)
        {
            BeginUnary(e);
            End(e);
        }

        public override void Visit(DbElementExpression e)
        {
            BeginUnary(e);
            End(e);
        }

        public override void Visit(DbIsEmptyExpression e)
        {
            BeginUnary(e);
            End(e);
        }

        public override void Visit(DbUnionAllExpression e)
        {
            BeginBinary(e);
            End(e);
        }

        public override void Visit(DbIntersectExpression e)
        {
            BeginBinary(e);
            End(e);
        }

        public override void Visit(DbExceptExpression e)
        {
            BeginBinary(e);
            End(e);
        }

        public override void Visit(DbTreatExpression e)
        {
            BeginUnary(e);
            End(e);
        }

        public override void Visit(DbIsOfExpression e)
        {
            BeginUnary(e);
            Dump(e.OfType, "OfType") ;
            End(e);
        }

        public override void Visit(DbCastExpression e)
        {
            BeginUnary(e);
            End(e);
        }

        public override void Visit(DbCaseExpression e)
        {
            Begin(e);
            Dump(e.When, "Whens", "When");
            Dump(e.Then, "Thens", "Then");
            Dump(e.Else, "Else");
        }

        public override void Visit(DbOfTypeExpression e)
        {
            BeginUnary(e);
            Dump(e.OfType, "OfType");
            End(e);
        }

        public override void Visit(DbNewInstanceExpression e)
        {
            Begin(e);
            Dump(e.Arguments, "Arguments", "Argument");
            if (e.HasRelatedEntityReferences)
            {
                Begin("RelatedEntityReferences");
                foreach (DbRelatedEntityRef relatedRef in e.RelatedEntityReferences)
                {
                    Begin("DbRelatedEntityRef");
                    Dump(relatedRef.SourceEnd, "SourceEnd");
                    Dump(relatedRef.TargetEnd, "TargetEnd");
                    Dump(relatedRef.TargetEntityReference, "TargetEntityReference");
                    End("DbRelatedEntityRef");
                }
                End("RelatedEntityReferences");
            }
            End(e);
        }

        public override void Visit(DbRelationshipNavigationExpression e)
        {
            Begin(e);
            Dump(e.NavigateFrom, "NavigateFrom");
            Dump(e.NavigateTo, "NavigateTo");
            Dump(e.Relationship, "Relationship");
            Dump(e.NavigationSource, "NavigationSource");
            End(e);
        }

        public override void Visit(DbRefExpression e)
        {
            BeginUnary(e);
            End(e);
        }

        public override void Visit(DbDerefExpression e)
        {
            BeginUnary(e);
            End(e);
        }

        public override void Visit(DbRefKeyExpression e)
        {
            BeginUnary(e);
            End(e);
        }

        public override void Visit(DbEntityRefExpression e)
        {
            BeginUnary(e);
            End(e);
        }

        public override void Visit(DbScanExpression e)
        {
            Begin(e);
            Begin("Target", "Name", e.Target.Name, "Container", e.Target.EntityContainer.Name);
            Dump(e.Target.ElementType, "TargetElementType");
            End("Target");
            End(e);
        }
                
        public override void Visit(DbFilterExpression e)
        {
            Begin(e);
            Dump(e.Input, "Input");
            Dump(e.Predicate, "Predicate");
            End(e);
        }

        public override void Visit(DbProjectExpression e)
        {
            Begin(e);
            Dump(e.Input, "Input");
            Dump(e.Projection, "Projection");
            End(e);
        }

        public override void Visit(DbCrossJoinExpression e)
        {
            Begin(e);
            Begin("Inputs");
            foreach (DbExpressionBinding binding in e.Inputs)
            {
                Dump(binding, "Input");
            }
            End("Inputs");
            End(e);
        }
        
        public override void Visit(DbJoinExpression e)
        {
            Begin(e);
            Dump(e.Left, "Left");
            Dump(e.Right, "Right");
            Dump(e.JoinCondition, "JoinCondition");
            End(e);
        }

        public override void Visit(DbApplyExpression e)
        {
            Begin(e);
            Dump(e.Input, "Input");
            Dump(e.Apply, "Apply");
            End(e);
        }

        public override void Visit(DbGroupByExpression e)
        {
            Begin(e);
            Dump(e.Input, "Input");
            Dump(e.Keys, "Keys", "Key");
            Begin("Aggregates");
            foreach (DbAggregate agg in e.Aggregates)
            {
                DbFunctionAggregate funcAgg = agg as DbFunctionAggregate;

                if (funcAgg != null)
                {
                    Begin("DbFunctionAggregate");
                    Dump(funcAgg.Function);
                    Dump(funcAgg.Arguments, "Arguments", "Argument");
                    End("DbFunctionAggregate"); 
                }
                else
                {
                    DbGroupAggregate groupAgg = agg as DbGroupAggregate;
                    Debug.Assert(groupAgg != null, "Invalid DbAggregate");
                    Begin("DbGroupAggregate");
                    Dump(groupAgg.Arguments, "Arguments", "Argument");
                    End("DbGroupAggregate");
                }
            }
            End("Aggregates");
            End(e);
        }

        protected virtual void Dump(IList<DbSortClause> sortOrder)
        {
            Begin("SortOrder");
            foreach (DbSortClause clause in sortOrder)
            {
                string collStr = clause.Collation;
                if (null == collStr)
                {
                    collStr = "";
                }

                Begin("DbSortClause", "Ascending", clause.Ascending, "Collation", collStr);
                Dump(clause.Expression, "Expression");
                End("DbSortClause");
            }
            End("SortOrder");
        }

        public override void Visit(DbSkipExpression e)
        {
            Begin(e);
            Dump(e.Input, "Input");
            Dump(e.SortOrder);
            Dump(e.Count, "Count");
            End(e);
        }

        public override void Visit(DbSortExpression e)
        {
            Begin(e);
            Dump(e.Input, "Input");
            Dump(e.SortOrder);
            End(e);
        }

        public override void Visit(DbQuantifierExpression e)
        {
            Begin(e);
            Dump(e.Input, "Input");
            Dump(e.Predicate, "Predicate");
            End(e);
        }

        #endregion
    }
}
