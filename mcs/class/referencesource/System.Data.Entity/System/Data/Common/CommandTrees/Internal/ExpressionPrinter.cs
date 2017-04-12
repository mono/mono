//---------------------------------------------------------------------
// <copyright file="ExpressionPrinter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Common.CommandTrees.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.Utils;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Prints a command tree
    /// </summary>
    internal class ExpressionPrinter : TreePrinter
    {
        private PrinterVisitor _visitor = new PrinterVisitor();

        internal ExpressionPrinter()
            : base() {}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal string Print(DbExpression expr)
        {
            Debug.Assert(expr != null, "Null DbExpression");
            return this.Print(_visitor.VisitExpression(expr));
        }

        internal string Print(DbDeleteCommandTree tree)
        {
            // Predicate should not be null since DbDeleteCommandTree initializes it to DbConstantExpression(true)
            Debug.Assert(tree != null && tree.Predicate != null, "Invalid DbDeleteCommandTree");

            TreeNode targetNode;
            if (tree.Target != null)
            {
                targetNode = _visitor.VisitBinding("Target", tree.Target);
            }
            else
            {
                targetNode = new TreeNode("Target");
            }

            TreeNode predicateNode;
            if (tree.Predicate != null)
            {
                predicateNode = _visitor.VisitExpression("Predicate", tree.Predicate);
            }
            else
            {
                predicateNode = new TreeNode("Predicate");
            }
            
            return this.Print(new TreeNode(
                    "DbDeleteCommandTree",
                    CreateParametersNode(tree),
                    targetNode,
                    predicateNode));
        }

        internal string Print(DbFunctionCommandTree tree)
        {
            Debug.Assert(tree != null, "Null DbFunctionCommandTree");

            TreeNode funcNode = new TreeNode("EdmFunction");
            if (tree.EdmFunction != null)
            {
                funcNode.Children.Add(_visitor.VisitFunction(tree.EdmFunction, null));
            }

            TreeNode typeNode = new TreeNode("ResultType");
            if (tree.ResultType != null)
            {
                PrinterVisitor.AppendTypeSpecifier(typeNode, tree.ResultType);
            }

            return this.Print(new TreeNode("DbFunctionCommandTree", CreateParametersNode(tree), funcNode, typeNode));
        }

        internal string Print(DbInsertCommandTree tree)
        {
            Debug.Assert(tree != null, "Null DbInsertCommandTree");

            TreeNode targetNode = null;
            if (tree.Target != null)
            {
                targetNode = _visitor.VisitBinding("Target", tree.Target);
            }
            else
            {
                targetNode = new TreeNode("Target");
            }

            TreeNode clausesNode = new TreeNode("SetClauses");
            foreach (DbModificationClause clause in tree.SetClauses)
            {
                if (clause != null)
                {
                    clausesNode.Children.Add(clause.Print(_visitor));
                }
            }

            TreeNode returningNode = null;
            if (null != tree.Returning)
            {
                returningNode = new TreeNode("Returning", _visitor.VisitExpression(tree.Returning));
            }
            else 
            {
                returningNode = new TreeNode("Returning");
            }

            return this.Print(new TreeNode(
                "DbInsertCommandTree",
                CreateParametersNode(tree),
                targetNode,
                clausesNode,
                returningNode));
        }

        internal string Print(DbUpdateCommandTree tree)
        {
            // Predicate should not be null since DbUpdateCommandTree initializes it to DbConstantExpression(true)
            Debug.Assert(tree != null && tree.Predicate != null, "Invalid DbUpdateCommandTree");

            TreeNode targetNode = null;
            if (tree.Target != null)
            {
                targetNode = _visitor.VisitBinding("Target", tree.Target);
            }
            else
            {
                targetNode = new TreeNode("Target");
            }

            TreeNode clausesNode = new TreeNode("SetClauses");
            foreach (DbModificationClause clause in tree.SetClauses)
            {
                if (clause != null)
                {
                    clausesNode.Children.Add(clause.Print(_visitor));
                }
            }

            TreeNode predicateNode;
            if (null != tree.Predicate)
            {
                predicateNode = new TreeNode("Predicate", _visitor.VisitExpression(tree.Predicate));
            }
            else
            {
                predicateNode = new TreeNode("Predicate");
            }

            TreeNode returningNode;
            if (null != tree.Returning)
            {
                returningNode = new TreeNode("Returning", _visitor.VisitExpression(tree.Returning));
            }
            else
            {
                returningNode = new TreeNode("Returning");
            }

            return this.Print(new TreeNode(
                "DbUpdateCommandTree",
                CreateParametersNode(tree),
                targetNode,
                clausesNode,
                predicateNode,
                returningNode));
        }

        internal string Print(DbQueryCommandTree tree)
        {
            Debug.Assert(tree != null, "Null DbQueryCommandTree");

            TreeNode queryNode = new TreeNode("Query");
            if (tree.Query != null)
            {
                PrinterVisitor.AppendTypeSpecifier(queryNode, tree.Query.ResultType);
                queryNode.Children.Add(_visitor.VisitExpression(tree.Query));
            }

            return this.Print(new TreeNode("DbQueryCommandTree", CreateParametersNode(tree), queryNode));
        }

        private static TreeNode CreateParametersNode(DbCommandTree tree)
        {
            TreeNode retNode = new TreeNode("Parameters");
            foreach (KeyValuePair<string, TypeUsage> paramInfo in tree.Parameters)
            {
                TreeNode paramNode = new TreeNode(paramInfo.Key);
                PrinterVisitor.AppendTypeSpecifier(paramNode, paramInfo.Value);
                retNode.Children.Add(paramNode);
            }

            return retNode;
        }

        private class PrinterVisitor : DbExpressionVisitor<TreeNode>
        {
            private static Dictionary<DbExpressionKind, string> _opMap = InitializeOpMap();
            
            private static Dictionary<DbExpressionKind, string> InitializeOpMap()
            {
                Dictionary<DbExpressionKind, string> opMap = new Dictionary<DbExpressionKind, string>(12);

                // Arithmetic
                opMap[DbExpressionKind.Divide] = "/";
                opMap[DbExpressionKind.Modulo] = "%";
                opMap[DbExpressionKind.Multiply] = "*";
                opMap[DbExpressionKind.Plus] = "+";
                opMap[DbExpressionKind.Minus] = "-";
                opMap[DbExpressionKind.UnaryMinus] = "-";

                // Comparison
                opMap[DbExpressionKind.Equals] = "=";
                opMap[DbExpressionKind.LessThan] = "<";
                opMap[DbExpressionKind.LessThanOrEquals] = "<=";
                opMap[DbExpressionKind.GreaterThan] = ">";
                opMap[DbExpressionKind.GreaterThanOrEquals] = ">=";
                opMap[DbExpressionKind.NotEquals] = "<>";

                return opMap;
            }

            private int _maxStringLength = 80;
            private bool _infix = true;

            internal TreeNode VisitExpression(DbExpression expr)
            {
                return expr.Accept<TreeNode>(this);
            }

            internal TreeNode VisitExpression(string name, DbExpression expr)
            {
                return new TreeNode(name, expr.Accept<TreeNode>(this));
            }

            internal TreeNode VisitBinding(string propName, DbExpressionBinding binding)
            {
                return this.VisitWithLabel(propName, binding.VariableName, binding.Expression);
            }

            internal TreeNode VisitFunction(EdmFunction func, IList<DbExpression> args)
            {
                TreeNode funcInfo = new TreeNode();
                AppendFullName(funcInfo.Text, func);

                AppendParameters(funcInfo, func.Parameters.Select(fp => new KeyValuePair<string, TypeUsage>(fp.Name, fp.TypeUsage)));
                if (args != null)
                {
                    AppendArguments(funcInfo, func.Parameters.Select(fp => fp.Name).ToArray(), args);
                }

                return funcInfo;
            }

            private static TreeNode NodeFromExpression(DbExpression expr)
            {
                return new TreeNode(Enum.GetName(typeof(DbExpressionKind), expr.ExpressionKind));
            }

            private static void AppendParameters(TreeNode node, IEnumerable<KeyValuePair<string, TypeUsage>> paramInfos)
            {
                node.Text.Append("(");
                int pos = 0;
                foreach(KeyValuePair<string, TypeUsage> paramInfo in paramInfos)
                {
                    if (pos > 0)
                    {
                        node.Text.Append(", ");
                    }
                    AppendType(node, paramInfo.Value);
                    node.Text.Append(" ");
                    node.Text.Append(paramInfo.Key);
                    pos++;
                }
                node.Text.Append(")");
            }

            internal static void AppendTypeSpecifier(TreeNode node, TypeUsage type)
            {
                node.Text.Append(" : ");
                AppendType(node, type);
            }

            internal static void AppendType(TreeNode node, TypeUsage type)
            {
                BuildTypeName(node.Text, type);
            }

            private static void BuildTypeName(StringBuilder text, TypeUsage type)
            {
                RowType rowType = type.EdmType as RowType;
                CollectionType collType = type.EdmType as CollectionType;
                RefType refType = type.EdmType as RefType;

                if (TypeSemantics.IsPrimitiveType(type))
                {
                    text.Append(type);
                }
                else if (collType != null)
                {
                    text.Append("Collection{");
                    BuildTypeName(text, collType.TypeUsage);
                    text.Append("}");
                }
                else if (refType != null)
                {
                    text.Append("Ref<");
                    AppendFullName(text, refType.ElementType);
                    text.Append(">");
                }
                else if (rowType != null)
                {
                    text.Append("Record[");
                    int idx = 0;
                    foreach (EdmProperty recColumn in rowType.Properties)
                    {
                        text.Append("'");
                        text.Append(recColumn.Name);
                        text.Append("'");
                        text.Append("=");
                        BuildTypeName(text, recColumn.TypeUsage);
                        idx++;
                        if (idx < rowType.Properties.Count)
                        {
                            text.Append(", ");
                        }
                    }
                    text.Append("]");
                }
                else
                {
                    // Entity, Relationship, Complex
                    if (!string.IsNullOrEmpty(type.EdmType.NamespaceName))
                    {
                        text.Append(type.EdmType.NamespaceName);
                        text.Append(".");
                    }
                    text.Append(type.EdmType.Name);
                }
            }

            private static void AppendFullName(StringBuilder text, EdmType type)
            {
                if (BuiltInTypeKind.RowType != type.BuiltInTypeKind)
                {
                    if (!string.IsNullOrEmpty(type.NamespaceName))
                    {
                        text.Append(type.NamespaceName);
                        text.Append(".");
                    }
                }

                text.Append(type.Name);
            }

            private List<TreeNode> VisitParams(IList<string> paramInfo, IList<DbExpression> args)
            {
                List<TreeNode> retInfo = new List<TreeNode>();
                for (int idx = 0; idx < paramInfo.Count; idx++)
                {
                    TreeNode paramNode = new TreeNode(paramInfo[idx]);
                    paramNode.Children.Add(this.VisitExpression(args[idx]));
                    retInfo.Add(paramNode);
                }

                return retInfo;
            }

            private void AppendArguments(TreeNode node, IList<string> paramNames, IList<DbExpression> args)
            {
                if (paramNames.Count > 0)
                {
                    node.Children.Add(new TreeNode("Arguments", VisitParams(paramNames, args)));
                }
            }

            private TreeNode VisitWithLabel(string label, string name, DbExpression def)
            {
                TreeNode retInfo = new TreeNode(label);
                retInfo.Text.Append(" : '");
                retInfo.Text.Append(name);
                retInfo.Text.Append("'");
                retInfo.Children.Add(this.VisitExpression(def));

                return retInfo;
            }

            private TreeNode VisitBindingList(string propName, IList<DbExpressionBinding> bindings)
            {
                List<TreeNode> bindingInfos = new List<TreeNode>();
                for (int idx = 0; idx < bindings.Count; idx++)
                {
                    bindingInfos.Add(this.VisitBinding(StringUtil.FormatIndex(propName, idx), bindings[idx]));
                }

                return new TreeNode(propName, bindingInfos);
            }

            private TreeNode VisitGroupBinding(DbGroupExpressionBinding groupBinding)
            {
                TreeNode inputInfo = this.VisitExpression(groupBinding.Expression);
                TreeNode retInfo = new TreeNode();
                retInfo.Children.Add(inputInfo);
                retInfo.Text.AppendFormat(CultureInfo.InvariantCulture, "Input : '{0}', '{1}'", groupBinding.VariableName, groupBinding.GroupVariableName);
                return retInfo;
            }

            private TreeNode Visit(string name, params DbExpression[] exprs)
            {
                TreeNode retInfo = new TreeNode(name);
                foreach (DbExpression expr in exprs)
                {
                    retInfo.Children.Add(this.VisitExpression(expr));
                }
                return retInfo;
            }

            private TreeNode VisitInfix(DbExpression root, DbExpression left, string name, DbExpression right)
            {
                if (_infix)
                {
                    TreeNode nullOp = new TreeNode("");
                    nullOp.Children.Add(this.VisitExpression(left));
                    nullOp.Children.Add(new TreeNode(name));
                    nullOp.Children.Add(this.VisitExpression(right));

                    return nullOp;
                }
                else
                {
                    return Visit(name, left, right);
                }
            }

            private TreeNode VisitUnary(DbUnaryExpression expr)
            {
                return VisitUnary(expr, false);
            }

            private TreeNode VisitUnary(DbUnaryExpression expr, bool appendType)
            {
                TreeNode retInfo = NodeFromExpression(expr);
                if (appendType)
                {
                    AppendTypeSpecifier(retInfo, expr.ResultType);
                }
                retInfo.Children.Add(this.VisitExpression(expr.Argument));
                return retInfo;
            }

            private TreeNode VisitBinary(DbBinaryExpression expr)
            {
                TreeNode retInfo = NodeFromExpression(expr);
                retInfo.Children.Add(this.VisitExpression(expr.Left));
                retInfo.Children.Add(this.VisitExpression(expr.Right));
                return retInfo;
            }

            #region DbExpressionVisitor<DbExpression> Members

            public override TreeNode Visit(DbExpression e)
            {
                throw EntityUtil.NotSupported(System.Data.Entity.Strings.Cqt_General_UnsupportedExpression(e.GetType().FullName));
            }

            public override TreeNode Visit(DbConstantExpression e)
            {
                TreeNode retInfo = new TreeNode();
                string stringVal = e.Value as string;
                if (stringVal != null)
                {
                    stringVal = stringVal.Replace("\r\n", "\\r\\n");
                    int appendLength = stringVal.Length;
                    if (_maxStringLength > 0)
                    {
                        appendLength = Math.Min(stringVal.Length, _maxStringLength);
                    }
                    retInfo.Text.Append("'");
                    retInfo.Text.Append(stringVal, 0, appendLength);
                    if (stringVal.Length > appendLength)
                    {
                        retInfo.Text.Append("...");
                    }
                    retInfo.Text.Append("'");
                }
                else
                {
                    retInfo.Text.Append(e.Value.ToString());
                }

                return retInfo;
            }

            public override TreeNode Visit(DbNullExpression e)
            {
                return new TreeNode("null");
            }

            public override TreeNode Visit(DbVariableReferenceExpression e)
            {
                TreeNode retInfo = new TreeNode();
                retInfo.Text.AppendFormat("Var({0})", e.VariableName);
                return retInfo;
            }

            public override TreeNode Visit(DbParameterReferenceExpression e)
            {
                TreeNode retInfo = new TreeNode();
                retInfo.Text.AppendFormat("@{0}", e.ParameterName);
                return retInfo;
            }

            public override TreeNode Visit(DbFunctionExpression e)
            {
                TreeNode funcInfo = VisitFunction(e.Function, e.Arguments);
                return funcInfo;
            }
                        
            public override TreeNode Visit(DbLambdaExpression expression)
            {
                TreeNode lambdaInfo = new TreeNode();
                lambdaInfo.Text.Append("Lambda");

                AppendParameters(lambdaInfo, expression.Lambda.Variables.Select(v => new KeyValuePair<string, TypeUsage>(v.VariableName, v.ResultType)));
                AppendArguments(lambdaInfo, expression.Lambda.Variables.Select(v => v.VariableName).ToArray(), expression.Arguments);
                lambdaInfo.Children.Add(this.Visit("Body", expression.Lambda.Body));

                return lambdaInfo;
            }

#if METHOD_EXPRESSION
            public override TreeNode Visit(MethodExpression e)
            {
                TreeNode retInfo = null;
                retInfo = new TreeNode(".");
                AppendType(retInfo, e.Method.DefiningType);
                retInfo.Text.Append(".");
                retInfo.Text.Append(e.Method.Name);
                AppendParameters(retInfo, e.Method.Parameters);
                if (e.Instance != null)
                {
                    retInfo.Children.Add(this.Visit("Instance", e.Instance));
                }
                AppendArguments(retInfo, e.Method.Parameters, e.Arguments);

                return retInfo;
            }
#endif

            public override TreeNode Visit(DbPropertyExpression e)
            {
                TreeNode inst = null;
                if (e.Instance != null)
                {
                    inst = this.VisitExpression(e.Instance);
                    if (e.Instance.ExpressionKind == DbExpressionKind.VariableReference ||
                        (e.Instance.ExpressionKind == DbExpressionKind.Property && 0 == inst.Children.Count))
                    {
                        inst.Text.Append(".");
                        inst.Text.Append(e.Property.Name);
                        return inst;
                    }
                }

                TreeNode retInfo = new TreeNode(".");
                EdmProperty prop = e.Property as EdmProperty;
                if (prop != null && !(prop.DeclaringType is RowType))
                {
                    // Entity, Relationship, Complex
                    AppendFullName(retInfo.Text, prop.DeclaringType);
                    retInfo.Text.Append(".");
                }
                retInfo.Text.Append(e.Property.Name);

                if (inst != null)
                {
                    retInfo.Children.Add(new TreeNode("Instance", inst));
                }

                return retInfo;
            }

            public override TreeNode Visit(DbComparisonExpression e)
            {
                return this.VisitInfix(e, e.Left, _opMap[e.ExpressionKind], e.Right);
            }

            public override TreeNode Visit(DbLikeExpression e)
            {
                return this.Visit("Like", e.Argument, e.Pattern, e.Escape);
            }

            public override TreeNode Visit(DbLimitExpression e)
            {
                return this.Visit((e.WithTies ? "LimitWithTies" : "Limit"), e.Argument, e.Limit);
            }

            public override TreeNode Visit(DbIsNullExpression e)
            {
                return this.VisitUnary(e);
            }

            public override TreeNode Visit(DbArithmeticExpression e)
            {
                if (DbExpressionKind.UnaryMinus == e.ExpressionKind)
                {
                    return this.Visit(_opMap[e.ExpressionKind], e.Arguments[0]);
                }
                else
                {
                    return this.VisitInfix(e, e.Arguments[0], _opMap[e.ExpressionKind], e.Arguments[1]);
                }
            }

            public override TreeNode Visit(DbAndExpression e)
            {
                return this.VisitInfix(e, e.Left, "And", e.Right);
            }

            public override TreeNode Visit(DbOrExpression e)
            {
                return this.VisitInfix(e, e.Left, "Or", e.Right);
            }

            public override TreeNode Visit(DbNotExpression e)
            {
                return this.VisitUnary(e);
            }

            public override TreeNode Visit(DbDistinctExpression e)
            {
                return this.VisitUnary(e);
            }

            public override TreeNode Visit(DbElementExpression e)
            {
                return this.VisitUnary(e, true);
            }

            public override TreeNode Visit(DbIsEmptyExpression e)
            {
                return this.VisitUnary(e);
            }

            public override TreeNode Visit(DbUnionAllExpression e)
            {
                return this.VisitBinary(e);
            }

            public override TreeNode Visit(DbIntersectExpression e)
            {
                return this.VisitBinary(e);
            }

            public override TreeNode Visit(DbExceptExpression e)
            {
                return this.VisitBinary(e);
            }

            private TreeNode VisitCastOrTreat(string op, DbUnaryExpression e)
            {
                TreeNode retInfo = null;
                TreeNode argInfo = this.VisitExpression(e.Argument);
                if (0 == argInfo.Children.Count)
                {
                    argInfo.Text.Insert(0, op);
                    argInfo.Text.Insert(op.Length, '(');
                    argInfo.Text.Append(" As ");
                    AppendType(argInfo, e.ResultType);
                    argInfo.Text.Append(")");

                    retInfo = argInfo;
                }
                else
                {
                    retInfo = new TreeNode(op);
                    AppendTypeSpecifier(retInfo, e.ResultType);
                    retInfo.Children.Add(argInfo);
                }

                return retInfo;
            }

            public override TreeNode Visit(DbTreatExpression e)
            {
                return VisitCastOrTreat("Treat", e);
            }

            public override TreeNode Visit(DbCastExpression e)
            {
                return VisitCastOrTreat("Cast", e);
            }

            public override TreeNode Visit(DbIsOfExpression e)
            {
                TreeNode retInfo = new TreeNode();
                if (DbExpressionKind.IsOfOnly == e.ExpressionKind)
                {
                    retInfo.Text.Append("IsOfOnly");
                }
                else
                {
                    retInfo.Text.Append("IsOf");
                }

                AppendTypeSpecifier(retInfo, e.OfType);
                retInfo.Children.Add(this.VisitExpression(e.Argument));

                return retInfo;
            }

            public override TreeNode Visit(DbOfTypeExpression e)
            {
                TreeNode retInfo = new TreeNode(e.ExpressionKind == DbExpressionKind.OfTypeOnly ? "OfTypeOnly" : "OfType");
                AppendTypeSpecifier(retInfo, e.OfType);
                retInfo.Children.Add(this.VisitExpression(e.Argument));

                return retInfo;
            }

            public override TreeNode Visit(DbCaseExpression e)
            {
                TreeNode retInfo = new TreeNode("Case");
                for (int idx = 0; idx < e.When.Count; idx++)
                {
                    retInfo.Children.Add(this.Visit("When", e.When[idx]));
                    retInfo.Children.Add(this.Visit("Then", e.Then[idx]));
                }

                retInfo.Children.Add(this.Visit("Else", e.Else));

                return retInfo;
            }

            public override TreeNode Visit(DbNewInstanceExpression e)
            {
                TreeNode retInfo = NodeFromExpression(e);
                AppendTypeSpecifier(retInfo, e.ResultType);

                if (BuiltInTypeKind.CollectionType == e.ResultType.EdmType.BuiltInTypeKind)
                {
                    foreach (DbExpression element in e.Arguments)
                    {
                        retInfo.Children.Add(this.VisitExpression(element));
                    }
                }
                else
                {
                    string description = (BuiltInTypeKind.RowType == e.ResultType.EdmType.BuiltInTypeKind) ? "Column" : "Property";
                    IList<EdmProperty> properties = TypeHelpers.GetProperties(e.ResultType);
                    for (int idx = 0; idx < properties.Count; idx++)
                    {
                        retInfo.Children.Add(this.VisitWithLabel(description, properties[idx].Name, e.Arguments[idx]));
                    }

                    if (BuiltInTypeKind.EntityType == e.ResultType.EdmType.BuiltInTypeKind &&
                         e.HasRelatedEntityReferences)
                    {
                        TreeNode references = new TreeNode("RelatedEntityReferences");
                        foreach (DbRelatedEntityRef relatedRef in e.RelatedEntityReferences)
                        {
                            TreeNode refNode = CreateNavigationNode(relatedRef.SourceEnd, relatedRef.TargetEnd);
                            refNode.Children.Add(CreateRelationshipNode((RelationshipType)relatedRef.SourceEnd.DeclaringType));
                            refNode.Children.Add(VisitExpression(relatedRef.TargetEntityReference));

                            references.Children.Add(refNode);
                        }

                        retInfo.Children.Add(references);
                    }
                }
                return retInfo;
            }

            public override TreeNode Visit(DbRefExpression e)
            {
                TreeNode retNode = new TreeNode("Ref");
                retNode.Text.Append("<");
                AppendFullName(retNode.Text, TypeHelpers.GetEdmType<RefType>(e.ResultType).ElementType);
                retNode.Text.Append(">");

                TreeNode setNode = new TreeNode("EntitySet : ");
                setNode.Text.Append(e.EntitySet.EntityContainer.Name);
                setNode.Text.Append(".");
                setNode.Text.Append(e.EntitySet.Name);

                retNode.Children.Add(setNode);
                retNode.Children.Add(this.Visit("Keys", e.Argument));

                return retNode;
            }

            private TreeNode CreateRelationshipNode(RelationshipType relType)
            {
                TreeNode rel = new TreeNode("Relationship");
                rel.Text.Append(" : ");
                AppendFullName(rel.Text, relType);
                return rel;
            }

            private TreeNode CreateNavigationNode(RelationshipEndMember fromEnd, RelationshipEndMember toEnd)
            {
                TreeNode nav = new TreeNode();
                nav.Text.Append("Navigation : ");
                nav.Text.Append(fromEnd.Name);
                nav.Text.Append(" -> ");
                nav.Text.Append(toEnd.Name);
                return nav;
            }

            public override TreeNode Visit(DbRelationshipNavigationExpression e)
            {
                TreeNode retInfo = NodeFromExpression(e);
                retInfo.Children.Add(CreateRelationshipNode(e.Relationship));
                retInfo.Children.Add(CreateNavigationNode(e.NavigateFrom, e.NavigateTo));
                retInfo.Children.Add(this.Visit("Source", e.NavigationSource));

                return retInfo;
            }

            public override TreeNode Visit(DbDerefExpression e)
            {
                return this.VisitUnary(e);
            }

            public override TreeNode Visit(DbRefKeyExpression e)
            {
                return this.VisitUnary(e, true);
            }

            public override TreeNode Visit(DbEntityRefExpression e)
            {
                return this.VisitUnary(e, true);
            }

            public override TreeNode Visit(DbScanExpression e)
            {
                TreeNode retInfo = NodeFromExpression(e);
                retInfo.Text.Append(" : ");
                retInfo.Text.Append(e.Target.EntityContainer.Name);
                retInfo.Text.Append(".");
                retInfo.Text.Append(e.Target.Name);
                return retInfo;
            }
                        
            public override TreeNode Visit(DbFilterExpression e)
            {
                TreeNode retInfo = NodeFromExpression(e);
                retInfo.Children.Add(this.VisitBinding("Input", e.Input));
                retInfo.Children.Add(this.Visit("Predicate", e.Predicate));
                return retInfo;
            }

            public override TreeNode Visit(DbProjectExpression e)
            {
                TreeNode retInfo = NodeFromExpression(e);
                retInfo.Children.Add(this.VisitBinding("Input", e.Input));
                retInfo.Children.Add(this.Visit("Projection", e.Projection));
                return retInfo;
            }

            public override TreeNode Visit(DbCrossJoinExpression e)
            {
                TreeNode retInfo = NodeFromExpression(e);
                retInfo.Children.Add(this.VisitBindingList("Inputs", e.Inputs));
                return retInfo;
            }

            public override TreeNode Visit(DbJoinExpression e)
            {
                TreeNode retInfo = NodeFromExpression(e);
                retInfo.Children.Add(this.VisitBinding("Left", e.Left));
                retInfo.Children.Add(this.VisitBinding("Right", e.Right));
                retInfo.Children.Add(this.Visit("JoinCondition", e.JoinCondition));
                
                return retInfo;
            }

            public override TreeNode Visit(DbApplyExpression e)
            {
                TreeNode retInfo = NodeFromExpression(e);
                retInfo.Children.Add(this.VisitBinding("Input", e.Input));
                retInfo.Children.Add(this.VisitBinding("Apply", e.Apply));

                return retInfo;
            }

            public override TreeNode Visit(DbGroupByExpression e)
            {
                List<TreeNode> keys = new List<TreeNode>();
                List<TreeNode> aggs = new List<TreeNode>();

                RowType outputType = TypeHelpers.GetEdmType<RowType>(TypeHelpers.GetEdmType<CollectionType>(e.ResultType).TypeUsage);
                int keyIdx = 0;
                for (int idx = 0; idx < e.Keys.Count; idx++)
                {
                    keys.Add(this.VisitWithLabel("Key", outputType.Properties[idx].Name, e.Keys[keyIdx]));
                    keyIdx++;
                }

                int aggIdx = 0;
                for (int idx = e.Keys.Count; idx < outputType.Properties.Count; idx++)
                {
                    TreeNode aggInfo = new TreeNode("Aggregate : '");
                    aggInfo.Text.Append(outputType.Properties[idx].Name);
                    aggInfo.Text.Append("'");

                    DbFunctionAggregate funcAgg = e.Aggregates[aggIdx] as DbFunctionAggregate;
                    if (funcAgg != null)
                    {
                        TreeNode funcInfo = this.VisitFunction(funcAgg.Function, funcAgg.Arguments);
                        if (funcAgg.Distinct)
                        {
                            funcInfo = new TreeNode("Distinct", funcInfo);
                        }
                        aggInfo.Children.Add(funcInfo);
                    }
                    else
                    {
                        DbGroupAggregate groupAgg = e.Aggregates[aggIdx] as DbGroupAggregate;
                        Debug.Assert(groupAgg != null, "Invalid DbAggregate");
                        aggInfo.Children.Add(this.Visit("GroupAggregate", groupAgg.Arguments[0]));
                    }
                    
                    aggs.Add(aggInfo);
                    aggIdx++;
                }

                TreeNode retInfo = NodeFromExpression(e);
                retInfo.Children.Add(this.VisitGroupBinding(e.Input));
                if (keys.Count > 0)
                {
                    retInfo.Children.Add(new TreeNode("Keys", keys));
                }

                if (aggs.Count > 0)
                {
                    retInfo.Children.Add(new TreeNode("Aggregates", aggs));
                }

                return retInfo;
            }

            private TreeNode VisitSortOrder(IList<DbSortClause> sortOrder)
            {
                TreeNode keyInfo = new TreeNode("SortOrder");
                foreach (DbSortClause clause in sortOrder)
                {
                    TreeNode key = this.Visit((clause.Ascending ? "Asc" : "Desc"), clause.Expression);
                    if (!string.IsNullOrEmpty(clause.Collation))
                    {
                        key.Text.Append(" : ");
                        key.Text.Append(clause.Collation);
                    }

                    keyInfo.Children.Add(key);
                }

                return keyInfo;
            }

            public override TreeNode Visit(DbSkipExpression e)
            {
                TreeNode retInfo = NodeFromExpression(e);
                retInfo.Children.Add(this.VisitBinding("Input", e.Input));
                retInfo.Children.Add(this.VisitSortOrder(e.SortOrder));
                retInfo.Children.Add(this.Visit("Count", e.Count));
                return retInfo;
            }

            public override TreeNode Visit(DbSortExpression e)
            {
                TreeNode retInfo = NodeFromExpression(e);
                retInfo.Children.Add(this.VisitBinding("Input", e.Input));
                retInfo.Children.Add(this.VisitSortOrder(e.SortOrder));

                return retInfo;
            }

            public override TreeNode Visit(DbQuantifierExpression e)
            {
                TreeNode retInfo = NodeFromExpression(e);
                retInfo.Children.Add(this.VisitBinding("Input", e.Input));
                retInfo.Children.Add(this.Visit("Predicate", e.Predicate));
                return retInfo;
            }
            #endregion
        }
    }
}
