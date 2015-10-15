using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Linq.SqlClient {

    /// <summary>
    /// Binds MemberAccess
    /// Prefetches deferrable expressions (SqlLink) if necessary
    /// Translates structured object comparision (EQ, NE) into memberwise comparison
    /// Translates shared expressions (SqlSharedExpression, SqlSharedExpressionRef)
    /// Optimizes out simple redundant operations : 
    ///     XXX OR TRUE ==> TRUE
    ///     XXX AND FALSE ==> FALSE
    ///     NON-NULL EQ NULL ==> FALSE
    ///     NON-NULL NEQ NULL ==> TRUE
    /// </summary>

    internal class SqlBinder {
        SqlColumnizer columnizer;
        Visitor visitor;
        SqlFactory sql;
        Func<SqlNode, SqlNode> prebinder;

        bool optimizeLinkExpansions = true;
        bool simplifyCaseStatements = true;

        internal SqlBinder(Translator translator, SqlFactory sqlFactory, MetaModel model, DataLoadOptions shape, SqlColumnizer columnizer, bool canUseOuterApply) {
            this.sql = sqlFactory;
            this.columnizer = columnizer;
            this.visitor = new Visitor(this, translator, this.columnizer, this.sql, model, shape, canUseOuterApply);
        }

        internal Func<SqlNode, SqlNode> PreBinder {
            get { return this.prebinder; }
            set { this.prebinder = value; }
        }

        private SqlNode Prebind(SqlNode node) {
            if (this.prebinder != null) {
                node = this.prebinder(node);
            }
            return node;
        }

        class LinkOptimizationScope {
            Dictionary<object, SqlExpression> map;
            LinkOptimizationScope previous;

            internal LinkOptimizationScope(LinkOptimizationScope previous) {
                this.previous = previous;
            }
            internal void Add(object linkId, SqlExpression expr) {
                if (this.map == null) {
                    this.map = new Dictionary<object,SqlExpression>();
                }
                this.map.Add(linkId, expr);
            }
            internal bool TryGetValue(object linkId, out SqlExpression expr) {
                expr = null;
                return (this.map != null && this.map.TryGetValue(linkId, out expr)) ||
                       (this.previous != null && this.previous.TryGetValue(linkId, out expr));
            }
        }

        internal SqlNode Bind(SqlNode node) {
            node = Prebind(node);
            node = this.visitor.Visit(node);
            return node;
        }

        internal bool OptimizeLinkExpansions {
            get { return this.optimizeLinkExpansions; }
            set { this.optimizeLinkExpansions = value; }
        }

        internal bool SimplifyCaseStatements {
            get { return this.simplifyCaseStatements; }
            set { this.simplifyCaseStatements = value; }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification="These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        class Visitor : SqlVisitor {
            SqlBinder binder;
            Translator translator;
            SqlFactory sql;
            TypeSystemProvider typeProvider;
            SqlExpander expander;
            SqlColumnizer columnizer;
            SqlAggregateChecker aggregateChecker;
            SqlSelect currentSelect;
            SqlAlias currentAlias;
            Dictionary<SqlAlias, SqlAlias> outerAliasMap;
            LinkOptimizationScope linkMap;
            MetaModel model;
            HashSet<MetaType> alreadyIncluded;
            DataLoadOptions shape;
            bool disableInclude;
            bool inGroupBy;
            bool canUseOuterApply;

            internal Visitor(SqlBinder binder, Translator translator, SqlColumnizer columnizer, SqlFactory sqlFactory, MetaModel model, DataLoadOptions shape, bool canUseOuterApply) {
                this.binder = binder;
                this.translator = translator;
                this.columnizer = columnizer;
                this.sql = sqlFactory;
                this.typeProvider = sqlFactory.TypeProvider;
                this.expander = new SqlExpander(this.sql);
                this.aggregateChecker = new SqlAggregateChecker();
                this.linkMap = new LinkOptimizationScope(null);
                this.outerAliasMap = new Dictionary<SqlAlias, SqlAlias>();
                this.model = model;
                this.shape = shape;
                this.canUseOuterApply = canUseOuterApply;
            }

            internal override SqlExpression VisitExpression(SqlExpression expr) {
                return this.ConvertToExpression(this.Visit(expr));
            }

            internal override SqlNode VisitIncludeScope(SqlIncludeScope scope) {
                this.alreadyIncluded = new HashSet<MetaType>();
                try {
                    return this.Visit(scope.Child); // Strip the include scope so SqlBinder will be idempotent.
                }
                finally {
                    this.alreadyIncluded = null;
                }
            }

            internal override SqlUserQuery VisitUserQuery(SqlUserQuery suq) {
                this.disableInclude = true;
                return base.VisitUserQuery(suq);
            }

            internal SqlExpression FetchExpression(SqlExpression expr) {
                return this.ConvertToExpression(this.ConvertToFetchedExpression(this.ConvertLinks(this.VisitExpression(expr))));
            }

            internal override SqlExpression VisitFunctionCall(SqlFunctionCall fc) {
                for (int i = 0, n = fc.Arguments.Count; i < n; i++) {
                    fc.Arguments[i] = this.FetchExpression(fc.Arguments[i]);
                }
                return fc;
            }

            internal override SqlExpression VisitLike(SqlLike like) {
                like.Expression = this.FetchExpression(like.Expression);
                like.Pattern = this.FetchExpression(like.Pattern);
                return base.VisitLike(like);
            }

            internal override SqlExpression VisitGrouping(SqlGrouping g) {
                g.Key = this.FetchExpression(g.Key);
                g.Group = this.FetchExpression(g.Group);
                return g;
            }

            internal override SqlExpression VisitMethodCall(SqlMethodCall mc) {
                mc.Object = this.FetchExpression(mc.Object);
                for (int i = 0, n = mc.Arguments.Count; i < n; i++) {
                    mc.Arguments[i] = this.FetchExpression(mc.Arguments[i]);
                }
                return mc;
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            internal override SqlExpression VisitBinaryOperator(SqlBinary bo) {
                // Below we translate comparisons with constant NULL to either IS NULL or IS NOT NULL.
                // We only want to do this if the type of the binary expression is not nullable.
                switch (bo.NodeType) {
                    case SqlNodeType.EQ:
                    case SqlNodeType.EQ2V:
                        if (this.IsConstNull(bo.Left) && !TypeSystem.IsNullableType(bo.ClrType)) {
                            return this.VisitUnaryOperator(this.sql.Unary(SqlNodeType.IsNull, bo.Right, bo.SourceExpression));
                        }
                        else if (this.IsConstNull(bo.Right) && !TypeSystem.IsNullableType(bo.ClrType)) {
                            return this.VisitUnaryOperator(this.sql.Unary(SqlNodeType.IsNull, bo.Left, bo.SourceExpression));
                        }
                        break;
                    case SqlNodeType.NE:
                    case SqlNodeType.NE2V:
                        if (this.IsConstNull(bo.Left) && !TypeSystem.IsNullableType(bo.ClrType)) {
                            return this.VisitUnaryOperator(this.sql.Unary(SqlNodeType.IsNotNull, bo.Right, bo.SourceExpression));
                        }
                        else if (this.IsConstNull(bo.Right) && !TypeSystem.IsNullableType(bo.ClrType)) {
                            return this.VisitUnaryOperator(this.sql.Unary(SqlNodeType.IsNotNull, bo.Left, bo.SourceExpression));
                        }
                        break;
                }
                
                bo.Left = this.VisitExpression(bo.Left);
                bo.Right = this.VisitExpression(bo.Right);

                switch (bo.NodeType) {
                    case SqlNodeType.EQ:
                    case SqlNodeType.EQ2V:   
                    case SqlNodeType.NE:
                    case SqlNodeType.NE2V: {
                        SqlValue vLeft = bo.Left as SqlValue;
                        SqlValue vRight = bo.Right as SqlValue;
                        bool leftIsBool = vLeft!=null && vLeft.Value is bool;
                        bool rightIsBool = vRight!=null && vRight.Value is bool;
                        if (leftIsBool || rightIsBool) {
                            bool equal = bo.NodeType != SqlNodeType.NE && bo.NodeType != SqlNodeType.NE2V;
                            bool isTwoValue = bo.NodeType == SqlNodeType.EQ2V || bo.NodeType == SqlNodeType.NE2V;
                            SqlNodeType negator = isTwoValue ? SqlNodeType.Not2V : SqlNodeType.Not;
                            if (leftIsBool && !rightIsBool) {
                                bool value = (bool)vLeft.Value;
                                if (value^equal) {
                                    return VisitUnaryOperator(new SqlUnary(negator, bo.ClrType, bo.SqlType, sql.DoNotVisitExpression(bo.Right), bo.SourceExpression));
                                }
                                if (bo.Right.ClrType==typeof(bool)) { // If the other side is nullable bool then this expression is already a reasonable way to handle three-values
                                    return bo.Right;
                                }
                            }
                            else if (!leftIsBool && rightIsBool) {
                                bool value = (bool)vRight.Value;
                                if (value^equal) {                               
                                    return VisitUnaryOperator(new SqlUnary(negator, bo.ClrType, bo.SqlType, sql.DoNotVisitExpression(bo.Left), bo.SourceExpression));
                                }
                                if (bo.Left.ClrType==typeof(bool)) { // If the other side is nullable bool then this expression is already a reasonable way to handle three-values
                                    return bo.Left;
                                }                                
                            } else if (leftIsBool && rightIsBool) {
                                // Here, both left and right are bools.
                                bool leftValue = (bool)vLeft.Value;
                                bool rightValue = (bool)vRight.Value;
                                
                                if (equal) {
                                    return sql.ValueFromObject(leftValue==rightValue, false, bo.SourceExpression);
                                } else {
                                    return sql.ValueFromObject(leftValue!=rightValue, false, bo.SourceExpression);
                                }
                            }
                        }
                        break;
                    }
                } 
                
                switch (bo.NodeType) {
                    case SqlNodeType.And: {
                            SqlValue vLeft = bo.Left as SqlValue;
                            SqlValue vRight = bo.Right as SqlValue;
                            if (vLeft != null && vRight == null) {
                                if (vLeft.Value != null && (bool)vLeft.Value) {
                                    return bo.Right;
                                }
                                return sql.ValueFromObject(false, false, bo.SourceExpression);
                            }
                            else if (vLeft == null && vRight != null) {
                                if (vRight.Value != null && (bool)vRight.Value) {
                                    return bo.Left;
                                }
                                return sql.ValueFromObject(false, false, bo.SourceExpression);
                            }
                            else if (vLeft != null && vRight != null) {
                                return sql.ValueFromObject((bool)(vLeft.Value ?? false) && (bool)(vRight.Value ?? false), false, bo.SourceExpression);
                            }
                            break;
                        }

                    case SqlNodeType.Or: {
                            SqlValue vLeft = bo.Left as SqlValue;
                            SqlValue vRight = bo.Right as SqlValue;
                            if (vLeft != null && vRight == null) {
                                if (vLeft.Value != null && !(bool)vLeft.Value) {
                                    return bo.Right;
                                }
                                return sql.ValueFromObject(true, false, bo.SourceExpression);
                            }
                            else if (vLeft == null && vRight != null) {
                                if (vRight.Value != null && !(bool)vRight.Value) {
                                    return bo.Left;
                                }
                                return sql.ValueFromObject(true, false, bo.SourceExpression);
                            }
                            else if (vLeft != null && vRight != null) {
                                return sql.ValueFromObject((bool)(vLeft.Value ?? false) || (bool)(vRight.Value ?? false), false, bo.SourceExpression);
                            }
                            break;
                        }

                    case SqlNodeType.EQ:
                    case SqlNodeType.NE:
                    case SqlNodeType.EQ2V:
                    case SqlNodeType.NE2V: {
                        SqlExpression translated = this.translator.TranslateLinkEquals(bo);
                        if (translated != bo) {
                            return this.VisitExpression(translated);
                        }
                        break;
                    }
                }

                bo.Left = this.ConvertToFetchedExpression(bo.Left);
                bo.Right = this.ConvertToFetchedExpression(bo.Right);

                switch (bo.NodeType) {
                    case SqlNodeType.EQ:
                    case SqlNodeType.NE:
                    case SqlNodeType.EQ2V:
                    case SqlNodeType.NE2V:
                        SqlExpression translated = this.translator.TranslateEquals(bo);
                        if (translated != bo) {
                            return this.VisitExpression(translated);
                        }

                        // Special handling for typeof(Type) nodes. Reduce to a static check if possible;
                        // strip SqlDiscriminatedType if possible;
                        if (typeof(Type).IsAssignableFrom(bo.Left.ClrType)) {
                            SqlExpression left = TypeSource.GetTypeSource(bo.Left);
                            SqlExpression right = TypeSource.GetTypeSource(bo.Right);

                            MetaType[] leftPossibleTypes = GetPossibleTypes(left);
                            MetaType[] rightPossibleTypes = GetPossibleTypes(right);

                            bool someMatch = false;
                            for (int i = 0; i < leftPossibleTypes.Length; ++i) {
                                for (int j = 0; j < rightPossibleTypes.Length; ++j) {
                                    if (leftPossibleTypes[i] == rightPossibleTypes[j]) {
                                        someMatch = true;
                                        break;
                                    }
                                }
                            }

                            // Is a match possible?
                            if (!someMatch) {
                                // No match is possible
                                return this.VisitExpression(sql.ValueFromObject(bo.NodeType == SqlNodeType.NE, false, bo.SourceExpression));
                            }

                            // Is the match known statically?
                            if (leftPossibleTypes.Length == 1 && rightPossibleTypes.Length == 1) {
                                // Yes, the match is statically known.
                                return this.VisitExpression(sql.ValueFromObject(
                                    (bo.NodeType == SqlNodeType.EQ) == (leftPossibleTypes[0] == rightPossibleTypes[0]),
                                    false,
                                    bo.SourceExpression));
                            }

                            // If both sides are discriminated types, then create a comparison of discriminators instead;
                            SqlDiscriminatedType leftDt = bo.Left as SqlDiscriminatedType;
                            SqlDiscriminatedType rightDt = bo.Right as SqlDiscriminatedType;
                            if (leftDt != null && rightDt != null) {
                                return this.VisitExpression(sql.Binary(bo.NodeType, leftDt.Discriminator, rightDt.Discriminator));
                            }
                        }

                        // can only compare sql scalars
                        if (TypeSystem.IsSequenceType(bo.Left.ClrType)) {
                            throw Error.ComparisonNotSupportedForType(bo.Left.ClrType);
                        }
                        if (TypeSystem.IsSequenceType(bo.Right.ClrType)) {
                            throw Error.ComparisonNotSupportedForType(bo.Right.ClrType);
                        }
                        break;
                }
                return bo;
            }

            /// <summary>
            /// Given an expression, return the set of dynamic types that could be returned.
            /// </summary>
            private MetaType[] GetPossibleTypes(SqlExpression typeExpression) {
                if (!typeof(Type).IsAssignableFrom(typeExpression.ClrType)) {
                    return new MetaType[0];
                }
                if (typeExpression.NodeType == SqlNodeType.DiscriminatedType) {
                    SqlDiscriminatedType dt = (SqlDiscriminatedType)typeExpression;
                    List<MetaType> concreteTypes = new List<MetaType>();
                    foreach (MetaType mt in dt.TargetType.InheritanceTypes) {
                        if (!mt.Type.IsAbstract) {
                            concreteTypes.Add(mt);
                        }
                    }
                    return concreteTypes.ToArray();
                }
                else if (typeExpression.NodeType == SqlNodeType.Value) {
                    SqlValue val = (SqlValue)typeExpression;
                    MetaType mt = this.model.GetMetaType((Type)val.Value);
                    return new MetaType[] { mt };
                } else if (typeExpression.NodeType == SqlNodeType.SearchedCase) {
                    SqlSearchedCase sc = (SqlSearchedCase)typeExpression;
                    HashSet<MetaType> types = new HashSet<MetaType>();
                    foreach (var when in sc.Whens) {
                        types.UnionWith(GetPossibleTypes(when.Value));
                    }
                    return types.ToArray();
                }
                throw Error.UnexpectedNode(typeExpression.NodeType);
            }

            /// <summary>
            /// Evaluate the object and extract its discriminator.
            /// </summary>
            internal override SqlExpression VisitDiscriminatorOf(SqlDiscriminatorOf dof) {
                SqlExpression obj = this.FetchExpression(dof.Object); // FetchExpression removes Link.
                // It's valid to unwrap optional and outer-join values here because type case already handles
                // NULL values correctly.
                while (obj.NodeType == SqlNodeType.OptionalValue
                    || obj.NodeType == SqlNodeType.OuterJoinedValue) {
                    if (obj.NodeType == SqlNodeType.OptionalValue) {
                        obj = ((SqlOptionalValue)obj).Value;
                    }
                    else {
                        obj = ((SqlUnary)obj).Operand;
                    }
                }
                if (obj.NodeType == SqlNodeType.TypeCase) {
                    SqlTypeCase tc = (SqlTypeCase)obj;
                    // Rewrite a case of discriminators. We can't just reduce to 
                    // discriminator (yet) because the ELSE clause needs to be considered.
                    // Later in the conversion there is an optimization that will turn the CASE
                    // into a simple combination of ANDs and ORs.
                    // Also, cannot reduce to IsNull(Discriminator,DefaultDiscriminator) because
                    // other unexpected values besides NULL need to be handled.
                    List<SqlExpression> matches = new List<SqlExpression>();
                    List<SqlExpression> values = new List<SqlExpression>();
                    MetaType defaultType = tc.RowType.InheritanceDefault;
                    object discriminator = defaultType.InheritanceCode;
                    foreach (SqlTypeCaseWhen when in tc.Whens) {
                        matches.Add(when.Match);
                        if (when.Match == null) {
                            SqlExpression @default = sql.Value(discriminator.GetType(), tc.Whens[0].Match.SqlType, defaultType.InheritanceCode, true, tc.SourceExpression);
                            values.Add(@default);
                        }
                        else {
                            // Must duplicate so that columnizer doesn't nominate the match as a value.
                            values.Add(sql.Value(discriminator.GetType(), when.Match.SqlType, ((SqlValue)when.Match).Value, true, tc.SourceExpression));
                        }
                    }
                    return sql.Case(tc.Discriminator.ClrType, tc.Discriminator, matches, values, tc.SourceExpression);
                } else {
                    var mt = this.model.GetMetaType(obj.ClrType).InheritanceRoot;
                    if (mt.HasInheritance) {
                        return this.VisitExpression(sql.Member(dof.Object, mt.Discriminator.Member));
                    }
                }
                return sql.TypedLiteralNull(dof.ClrType, dof.SourceExpression);
            }

            internal override SqlExpression VisitSearchedCase(SqlSearchedCase c) {
                if ((c.ClrType == typeof(bool) || c.ClrType == typeof(bool?)) &&
                    c.Whens.Count == 1 && c.Else != null) {
                    SqlValue litElse = c.Else as SqlValue;
                    SqlValue litWhen = c.Whens[0].Value as SqlValue;

                    if (litElse != null && litElse.Value != null && !(bool)litElse.Value) {
                        return this.VisitExpression(sql.Binary(SqlNodeType.And, c.Whens[0].Match, c.Whens[0].Value));
                    }
                    else if (litWhen != null && litWhen.Value != null && (bool)litWhen.Value) {
                        return this.VisitExpression(sql.Binary(SqlNodeType.Or, c.Whens[0].Match, c.Else));
                    }
                }
                return base.VisitSearchedCase(c);
            }

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
            private bool IsConstNull(SqlExpression sqlExpr) {
                SqlValue sqlValue = sqlExpr as SqlValue;
                if (sqlValue == null) {
                    return false;
                }
                // literal nulls are encoded as IsClientSpecified=false
                return sqlValue.Value == null && !sqlValue.IsClientSpecified;
            }

            /// <summary>
            /// Apply the 'TREAT' operator into the given target. The goal is for instances of non-assignable types 
            /// to be nulled out.
            /// </summary>
            private SqlExpression ApplyTreat(SqlExpression target, Type type) {
                switch (target.NodeType) {
                    case SqlNodeType.OptionalValue:
                        SqlOptionalValue optValue = (SqlOptionalValue)target;
                        return ApplyTreat(optValue.Value, type);
                    case SqlNodeType.OuterJoinedValue:
                        SqlUnary unary = (SqlUnary)target;
                        return ApplyTreat(unary.Operand, type);
                    case SqlNodeType.New:
                        var n = (SqlNew)target;
                        // Are we constructing a concrete instance of a type we know can't be assigned
                        // to 'type'? If so, make it null.
                        if (!type.IsAssignableFrom(n.ClrType)) { 
                            return sql.TypedLiteralNull(type, target.SourceExpression);
                        }
                        return target;
                    case SqlNodeType.TypeCase:
                        SqlTypeCase tc = (SqlTypeCase)target;
                        // Null out type case options that are impossible now.
                        int reducedToNull = 0;
                        foreach (SqlTypeCaseWhen when in tc.Whens) {
                            when.TypeBinding = (SqlExpression)ApplyTreat(when.TypeBinding, type);
                            if (this.IsConstNull(when.TypeBinding)) {
                                ++reducedToNull;
                            }
                        }
                        // If every case reduced to NULL then reduce the whole clause entirely to NULL.
                        if (reducedToNull == tc.Whens.Count) {
                            // This is not an optimization. We need to do this because the type-case may be the l-value of an assign.
                            tc.Whens[0].TypeBinding.SetClrType(type);
                            return tc.Whens[0].TypeBinding; // <-- Points to a SqlValue null.
                        }
                        tc.SetClrType(type);
                        return target;
                    default:
                        SqlExpression expr = target as SqlExpression;
                        if (expr != null) {
                            if (!type.IsAssignableFrom(expr.ClrType) && !expr.ClrType.IsAssignableFrom(type)) {
                                return sql.TypedLiteralNull(type, target.SourceExpression);
                            } 
                        }
                        else {
                            System.Diagnostics.Debug.Assert(false, "Don't know how to apply 'as' to " + target.NodeType);
                        }
                        return target;
                }
            }

            internal override SqlExpression VisitTreat(SqlUnary a) {
                return VisitUnaryOperator(a);
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            internal override SqlExpression VisitUnaryOperator(SqlUnary uo) {
                uo.Operand = this.VisitExpression(uo.Operand);
                // ------------------------------------------------------------
                // PHASE 1: If possible, evaluate without fetching the operand.
                // This is preferred because fetching LINKs causes them to not
                // be deferred.
                // ------------------------------------------------------------
                if (uo.NodeType == SqlNodeType.IsNull || uo.NodeType == SqlNodeType.IsNotNull) {
                    SqlExpression translated = this.translator.TranslateLinkIsNull(uo);
                    if (translated != uo) {
                        return this.VisitExpression(translated);
                    }
                    if (uo.Operand.NodeType==SqlNodeType.OuterJoinedValue) {
                        SqlUnary ojv = uo.Operand as SqlUnary;
                        if (ojv.Operand.NodeType == SqlNodeType.OptionalValue) {
                            SqlOptionalValue ov = (SqlOptionalValue)ojv.Operand;
                            return this.VisitUnaryOperator(
                                new SqlUnary(uo.NodeType, uo.ClrType, uo.SqlType,
                                    new SqlUnary(SqlNodeType.OuterJoinedValue, ov.ClrType, ov.SqlType, ov.HasValue, ov.SourceExpression)
                                , uo.SourceExpression)
                            );
                        }
                        else if (ojv.Operand.NodeType == SqlNodeType.TypeCase) {
                            SqlTypeCase tc = (SqlTypeCase)ojv.Operand;
                            return new SqlUnary(uo.NodeType, uo.ClrType, uo.SqlType,
                                       new SqlUnary(SqlNodeType.OuterJoinedValue, tc.Discriminator.ClrType, tc.Discriminator.SqlType, tc.Discriminator, tc.SourceExpression),
                                       uo.SourceExpression
                                       );
                        }
                    }
                }

                // Fetch the expression.
                uo.Operand = this.ConvertToFetchedExpression(uo.Operand);

                // ------------------------------------------------------------
                // PHASE 2: Evaluate operator on fetched expression.
                // ------------------------------------------------------------
                if ((uo.NodeType == SqlNodeType.Not || uo.NodeType == SqlNodeType.Not2V) && uo.Operand.NodeType == SqlNodeType.Value) {
                    SqlValue val = (SqlValue)uo.Operand;
                    return sql.Value(typeof(bool), val.SqlType, !(bool)val.Value, val.IsClientSpecified, val.SourceExpression);
                }
                else if (uo.NodeType == SqlNodeType.Not2V) {
                    if (SqlExpressionNullability.CanBeNull(uo.Operand) != false) {
                        SqlSearchedCase c = new SqlSearchedCase(
                            typeof(int),
                            new [] { new SqlWhen(uo.Operand, sql.ValueFromObject(1, false, uo.SourceExpression)) },
                            sql.ValueFromObject(0, false, uo.SourceExpression),
                            uo.SourceExpression
                            );
                        return sql.Binary(SqlNodeType.EQ, c, sql.ValueFromObject(0, false, uo.SourceExpression));
                    }
                    else {
                        return sql.Unary(SqlNodeType.Not, uo.Operand);
                    }
                }
                // push converts of client-expressions inside the client-expression (to be evaluated client side) 
                else if (uo.NodeType == SqlNodeType.Convert && uo.Operand.NodeType == SqlNodeType.Value) {
                    SqlValue val = (SqlValue)uo.Operand;
                    return sql.Value(uo.ClrType, uo.SqlType, DBConvert.ChangeType(val.Value, uo.ClrType), val.IsClientSpecified, val.SourceExpression);
                }
                else if (uo.NodeType == SqlNodeType.IsNull || uo.NodeType == SqlNodeType.IsNotNull) {
                    bool? canBeNull = SqlExpressionNullability.CanBeNull(uo.Operand);
                    if (canBeNull == false) {
                        return sql.ValueFromObject(uo.NodeType == SqlNodeType.IsNotNull, false, uo.SourceExpression);
                    }
                    SqlExpression exp = uo.Operand;
                    switch (exp.NodeType) {
                        case SqlNodeType.Element:
                            exp = sql.SubSelect(SqlNodeType.Exists, ((SqlSubSelect)exp).Select);
                            if (uo.NodeType == SqlNodeType.IsNull) {
                                exp = sql.Unary(SqlNodeType.Not, exp, exp.SourceExpression);
                            }
                            return exp;
                        case SqlNodeType.ClientQuery: {
                                SqlClientQuery cq = (SqlClientQuery)exp;
                                if (cq.Query.NodeType == SqlNodeType.Element) {
                                    exp = sql.SubSelect(SqlNodeType.Exists, cq.Query.Select);
                                    if (uo.NodeType == SqlNodeType.IsNull) {
                                        exp = sql.Unary(SqlNodeType.Not, exp, exp.SourceExpression);
                                    }
                                    return exp;
                                }
                                return sql.ValueFromObject(uo.NodeType == SqlNodeType.IsNotNull, false, uo.SourceExpression);
                            }
                        case SqlNodeType.OptionalValue:
                            uo.Operand = ((SqlOptionalValue)exp).HasValue;
                            return uo;

                        case SqlNodeType.ClientCase: {
                                // Distribute unary into simple case.
                                SqlClientCase sc = (SqlClientCase)uo.Operand;
                                List<SqlExpression> matches = new List<SqlExpression>();
                                List<SqlExpression> values = new List<SqlExpression>();
                                foreach (SqlClientWhen when in sc.Whens) {
                                    matches.Add(when.Match);
                                    values.Add(VisitUnaryOperator(sql.Unary(uo.NodeType, when.Value, when.Value.SourceExpression)));
                                }
                                return sql.Case(sc.ClrType, sc.Expression, matches, values, sc.SourceExpression);
                            }
                        case SqlNodeType.TypeCase: {
                                // Distribute unary into type case. In the process, convert to simple case.
                                SqlTypeCase tc = (SqlTypeCase)uo.Operand;
                                List<SqlExpression> newMatches = new List<SqlExpression>();
                                List<SqlExpression> newValues = new List<SqlExpression>();
                                foreach (SqlTypeCaseWhen when in tc.Whens) {
                                    SqlUnary un = new SqlUnary(uo.NodeType, uo.ClrType, uo.SqlType, when.TypeBinding, when.TypeBinding.SourceExpression);
                                    SqlExpression expr = VisitUnaryOperator(un);
                                    if (expr is SqlNew) {
                                        throw Error.DidNotExpectTypeBinding();
                                    }
                                    newMatches.Add(when.Match);
                                    newValues.Add(expr);
                                }
                                return sql.Case(uo.ClrType, tc.Discriminator, newMatches, newValues, tc.SourceExpression);
                            }
                        case SqlNodeType.Value: {
                                SqlValue val = (SqlValue)uo.Operand;
                                return sql.Value(typeof(bool), this.typeProvider.From(typeof(int)), (val.Value == null) == (uo.NodeType == SqlNodeType.IsNull), val.IsClientSpecified, uo.SourceExpression);
                            }
                    }
                }
                else if (uo.NodeType == SqlNodeType.Treat) {
                    return ApplyTreat(VisitExpression(uo.Operand), uo.ClrType);
                }

                return uo;
            }

            internal override SqlExpression VisitNew(SqlNew sox) {
                for (int i = 0, n = sox.Args.Count; i < n; i++) {
                    if (inGroupBy) {
                        // we don't want to fetch expressions for group by,
                        // since we want links to remain links so SqlFlattener
                        // can deal with them properly
                        sox.Args[i] = this.VisitExpression(sox.Args[i]);
                    }
                    else {
                        sox.Args[i] = this.FetchExpression(sox.Args[i]);
                    }
                }
                for (int i = 0, n = sox.Members.Count; i < n; i++) {
                    SqlMemberAssign ma = sox.Members[i];
                    MetaDataMember mm = sox.MetaType.GetDataMember(ma.Member);
                    MetaType otherType = mm.DeclaringType.InheritanceRoot;
                    if (mm.IsAssociation && ma.Expression != null && ma.Expression.NodeType != SqlNodeType.Link
                        && this.shape != null && this.shape.IsPreloaded(mm.Member) && mm.LoadMethod == null
                        && this.alreadyIncluded != null && !this.alreadyIncluded.Contains(otherType)) {
                        // The expression is already fetched, add it to the alreadyIncluded set.
                        this.alreadyIncluded.Add(otherType);
                        ma.Expression = this.VisitExpression(ma.Expression);
                        this.alreadyIncluded.Remove(otherType);
                    }
                    else if (mm.IsAssociation || mm.IsDeferred) {
                        ma.Expression = this.VisitExpression(ma.Expression);
                    }
                    else {
                        ma.Expression = this.FetchExpression(ma.Expression);
                    }
                }
                return sox;
            }

            internal override SqlNode VisitMember(SqlMember m) {
                return this.AccessMember(m, this.FetchExpression(m.Expression));
            }

            [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals", Justification="These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            private SqlNode AccessMember(SqlMember m, SqlExpression expo) {
                SqlExpression exp = expo;

                switch (exp.NodeType) {
                    case SqlNodeType.ClientCase: {
                            // Distribute into each case.
                            SqlClientCase sc = (SqlClientCase)exp;
                            Type newClrType = null;
                            List<SqlExpression> matches = new List<SqlExpression>();
                            List<SqlExpression> values = new List<SqlExpression>();
                            foreach (SqlClientWhen when in sc.Whens) {
                                SqlExpression newValue = (SqlExpression)AccessMember(m, when.Value);
                                if (newClrType == null) {
                                    newClrType = newValue.ClrType;
                                }
                                else if (newClrType != newValue.ClrType) {
                                    throw Error.ExpectedClrTypesToAgree(newClrType, newValue.ClrType);
                                }
                                matches.Add(when.Match);
                                values.Add(newValue);
                            }

                            SqlExpression result = sql.Case(newClrType, sc.Expression, matches, values, sc.SourceExpression);
                            return result;
                        }
                    case SqlNodeType.SimpleCase: {
                            // Distribute into each case.
                            SqlSimpleCase sc = (SqlSimpleCase)exp;
                            Type newClrType = null;
                            List<SqlExpression> newMatches = new List<SqlExpression>();
                            List<SqlExpression> newValues = new List<SqlExpression>();
                            foreach (SqlWhen when in sc.Whens) {
                                SqlExpression newValue = (SqlExpression)AccessMember(m, when.Value);
                                if (newClrType == null) {
                                    newClrType = newValue.ClrType;
                                }
                                else if (newClrType != newValue.ClrType) {
                                  throw Error.ExpectedClrTypesToAgree(newClrType, newValue.ClrType);
                                }
                                newMatches.Add(when.Match);
                                newValues.Add(newValue);
                            }
                            SqlExpression result = sql.Case(newClrType, sc.Expression, newMatches, newValues, sc.SourceExpression);
                            return result;
                        }
                    case SqlNodeType.SearchedCase: {
                            // Distribute into each case.
                            SqlSearchedCase sc = (SqlSearchedCase)exp;
                            List<SqlWhen> whens = new List<SqlWhen>(sc.Whens.Count);
                            foreach (SqlWhen when in sc.Whens) {
                                SqlExpression value = (SqlExpression)AccessMember(m, when.Value);
                                whens.Add(new SqlWhen(when.Match, value));
                            }
                            SqlExpression @else = (SqlExpression)AccessMember(m, sc.Else);
                            return sql.SearchedCase(whens.ToArray(), @else, sc.SourceExpression);
                        }
                    case SqlNodeType.TypeCase: {
                            // We don't allow derived types to map members to different database fields.
                            // Therefore, just pick the best SqlNew to call AccessMember on.
                            SqlTypeCase tc = (SqlTypeCase)exp;

                            // Find the best type binding for this member.
                            SqlNew tb = tc.Whens[0].TypeBinding as SqlNew;
                            foreach (SqlTypeCaseWhen when in tc.Whens) {
                                if (when.TypeBinding.NodeType == SqlNodeType.New) {
                                    SqlNew sn = (SqlNew)when.TypeBinding;
                                    if (m.Member.DeclaringType.IsAssignableFrom(sn.ClrType)) {
                                        tb = sn;
                                        break;
                                    }
                                }
                            }
                            return AccessMember(m, tb);
                        }
                    case SqlNodeType.AliasRef: {
                            // convert alias.Member => column
                            SqlAliasRef aref = (SqlAliasRef)exp;
                            // if its a table, find the matching column
                            SqlTable tab = aref.Alias.Node as SqlTable;
                            if (tab != null) {
                                MetaDataMember mm = GetRequiredInheritanceDataMember(tab.RowType, m.Member);
                                System.Diagnostics.Debug.Assert(mm != null);
                                string name = mm.MappedName;
                                SqlColumn c = tab.Find(name);
                                if (c == null) {
                                    ProviderType sqlType = sql.Default(mm);
                                    c = new SqlColumn(m.ClrType, sqlType, name, mm, null, m.SourceExpression);
                                    c.Alias = aref.Alias;
                                    tab.Columns.Add(c);
                                }
                                return new SqlColumnRef(c);
                            }
                            // if it is a table valued function, find the matching result column                                
                            SqlTableValuedFunctionCall fc = aref.Alias.Node as SqlTableValuedFunctionCall;
                            if (fc != null) {
                                MetaDataMember mm = GetRequiredInheritanceDataMember(fc.RowType, m.Member);
                                System.Diagnostics.Debug.Assert(mm != null);
                                string name = mm.MappedName;
                                SqlColumn c = fc.Find(name);
                                if (c == null) {
                                    ProviderType sqlType = sql.Default(mm);
                                    c = new SqlColumn(m.ClrType, sqlType, name, mm, null, m.SourceExpression);
                                    c.Alias = aref.Alias;
                                    fc.Columns.Add(c);
                                }
                                return new SqlColumnRef(c);
                            }
                            break;
                        }
                    case SqlNodeType.OptionalValue:
                        // convert option(exp).Member => exp.Member
                        return this.AccessMember(m, ((SqlOptionalValue)exp).Value);

                    case SqlNodeType.OuterJoinedValue: {
                            SqlNode n = this.AccessMember(m, ((SqlUnary)exp).Operand);
                            SqlExpression e = n as SqlExpression;
                            if (e != null) return sql.Unary(SqlNodeType.OuterJoinedValue, e);
                            return n;
                        }

                    case SqlNodeType.Lift:
                        return this.AccessMember(m, ((SqlLift)exp).Expression);

                    case SqlNodeType.UserRow: {
                            // convert UserRow.Member => UserColumn
                            SqlUserRow row = (SqlUserRow)exp;
                            SqlUserQuery suq = row.Query;
                            MetaDataMember mm = GetRequiredInheritanceDataMember(row.RowType, m.Member);
                            System.Diagnostics.Debug.Assert(mm != null);
                            string name = mm.MappedName;
                            SqlUserColumn c = suq.Find(name);
                            if (c == null) {
                                ProviderType sqlType = sql.Default(mm);
                                c = new SqlUserColumn(m.ClrType, sqlType, suq, name, mm.IsPrimaryKey, m.SourceExpression);
                                suq.Columns.Add(c);
                            }
                            return c;
                        }
                    case SqlNodeType.New: {
                            // convert (new {Member = expr}).Member => expr
                            SqlNew sn = (SqlNew)exp;
                            SqlExpression e = sn.Find(m.Member);
                            if (e != null) {
                                return e;
                            }
                            MetaDataMember mm = sn.MetaType.PersistentDataMembers.FirstOrDefault(p => p.Member == m.Member);
                            if (!sn.SqlType.CanBeColumn && mm != null) {
                                throw Error.MemberNotPartOfProjection(m.Member.DeclaringType, m.Member.Name);
                            }
                            break;
                        }
                    case SqlNodeType.Element:
                    case SqlNodeType.ScalarSubSelect: {
                            // convert Scalar/Element(select exp).Member => Scalar/Element(select exp.Member) / select exp.Member
                            SqlSubSelect sub = (SqlSubSelect)exp;
                            SqlAlias alias = new SqlAlias(sub.Select);
                            SqlAliasRef aref = new SqlAliasRef(alias);

                            SqlSelect saveSelect = this.currentSelect;
                            try {
                                SqlSelect newSelect = new SqlSelect(aref, alias, sub.SourceExpression);
                                this.currentSelect = newSelect;
                                SqlNode result = this.Visit(sql.Member(aref, m.Member));

                                SqlExpression rexp = result as SqlExpression;
                                if (rexp != null) {

                                    // If the expression is still a Member after being visited, but it cannot be a column, then it cannot be collapsed
                                    // into the SubSelect because we need to keep track of the fact that this member has to be accessed on the client.
                                    // This must be done after the expression has been Visited above, because otherwise we don't have
                                    // enough context to know if the member can be a column or not.
                                    if (rexp.NodeType == SqlNodeType.Member && !SqlColumnizer.CanBeColumn(rexp)) {
                                        // If the original member expression is an Element, optimize it by converting to an OuterApply if possible.
                                        // We have to do this here because we are creating a new member expression based on it, and there are no
                                        // subsequent visitors that will do this optimization.
                                        if (this.canUseOuterApply && exp.NodeType == SqlNodeType.Element && this.currentSelect != null) {
                                            // Reset the currentSelect since we are not going to use the previous SqlSelect that was created
                                            this.currentSelect = saveSelect;                                            
                                            this.currentSelect.From = sql.MakeJoin(SqlJoinType.OuterApply, this.currentSelect.From, alias, null, sub.SourceExpression);
                                            exp = this.VisitExpression(aref);
                                        }                                        
                                        return sql.Member(exp, m.Member);
                                    }

                                    // Since we are going to make a SubSelect out of this member expression, we need to make
                                    // sure it gets columnized before it gets to the PostBindDotNetConverter, otherwise only the
                                    // entire SubSelect will be columnized as a whole. Subsequent columnization does not know how to handle
                                    // any function calls that may be produced by the PostBindDotNetConverter, but we know how to handle it here.
                                    newSelect.Selection = rexp;
                                    newSelect.Selection = this.columnizer.ColumnizeSelection(newSelect.Selection);
                                    newSelect.Selection = this.ConvertLinks(newSelect.Selection);
                                    SqlNodeType subType = (rexp is SqlTypeCase || !rexp.SqlType.CanBeColumn) ? SqlNodeType.Element : SqlNodeType.ScalarSubSelect;
                                    SqlSubSelect subSel = sql.SubSelect(subType, newSelect);
                                    return this.FoldSubquery(subSel);
                                }

                                SqlSelect rselect = result as SqlSelect;
                                if (rselect != null) {
                                    SqlAlias ralias = new SqlAlias(rselect);
                                    SqlAliasRef rref = new SqlAliasRef(ralias);
                                    newSelect.Selection = this.ConvertLinks(this.VisitExpression(rref));
                                    newSelect.From = new SqlJoin(SqlJoinType.CrossApply, alias, ralias, null, m.SourceExpression);
                                    return newSelect;
                                }
                                throw Error.UnexpectedNode(result.NodeType);
                            }
                            finally {
                                this.currentSelect = saveSelect;
                            }
                        }
                    case SqlNodeType.Value: {
                            SqlValue val = (SqlValue)exp;
                            if (val.Value == null) {
                                return sql.Value(m.ClrType, m.SqlType, null, val.IsClientSpecified, m.SourceExpression);
                            }
                            else if (m.Member is PropertyInfo) {
                                PropertyInfo p = (PropertyInfo)m.Member;
                                return sql.Value(m.ClrType, m.SqlType, p.GetValue(val.Value, null), val.IsClientSpecified, m.SourceExpression);
                            }
                            else {
                                FieldInfo f = (FieldInfo)m.Member;
                                return sql.Value(m.ClrType, m.SqlType, f.GetValue(val.Value), val.IsClientSpecified, m.SourceExpression);
                            }
                        }
                    case SqlNodeType.Grouping: {
                            SqlGrouping g = ((SqlGrouping)exp);
                            if (m.Member.Name == "Key") {
                                return g.Key;
                            }
                            break;
                        }
                    case SqlNodeType.ClientParameter: {
                            SqlClientParameter cp = (SqlClientParameter)exp;
                            // create new accessor including this member access
                            LambdaExpression accessor =
                                Expression.Lambda(
                                    typeof(Func<,>).MakeGenericType(typeof(object[]), m.ClrType),
                                    Expression.MakeMemberAccess(cp.Accessor.Body, m.Member),
                                    cp.Accessor.Parameters
                                    );
                            return new SqlClientParameter(m.ClrType, m.SqlType, accessor, cp.SourceExpression);
                        }
                    default:
                        break;  
                }
                if (m.Expression == exp) {
                    return m;
                }
                else {
                    return sql.Member(exp, m.Member);
                }
            }

            private SqlExpression FoldSubquery(SqlSubSelect ss) {
                // convert ELEMENT(SELECT MULTISET(SELECT xxx FROM t1 WHERE p1) FROM t2 WHERE p2)
                // into MULTISET(SELECT xxx FROM t2 CA (SELECT xxx FROM t1 WHERE p1) WHERE p2))
                while (true) {
                    if (ss.NodeType == SqlNodeType.Element && ss.Select.Selection.NodeType == SqlNodeType.Multiset) {
                        SqlSubSelect msub = (SqlSubSelect)ss.Select.Selection;
                        SqlAlias alias = new SqlAlias(msub.Select);
                        SqlAliasRef aref = new SqlAliasRef(alias);
                        SqlSelect sel = ss.Select;
                        sel.Selection = this.ConvertLinks(this.VisitExpression(aref));
                        sel.From = new SqlJoin(SqlJoinType.CrossApply, sel.From, alias, null, ss.SourceExpression);
                        SqlSubSelect newss = sql.SubSelect(SqlNodeType.Multiset, sel, ss.ClrType);
                        ss = newss;
                    }
                    else if (ss.NodeType == SqlNodeType.Element && ss.Select.Selection.NodeType == SqlNodeType.Element) {
                        SqlSubSelect msub = (SqlSubSelect)ss.Select.Selection;
                        SqlAlias alias = new SqlAlias(msub.Select);
                        SqlAliasRef aref = new SqlAliasRef(alias);
                        SqlSelect sel = ss.Select;
                        sel.Selection = this.ConvertLinks(this.VisitExpression(aref));
                        sel.From = new SqlJoin(SqlJoinType.CrossApply, sel.From, alias, null, ss.SourceExpression);
                        SqlSubSelect newss = sql.SubSelect(SqlNodeType.Element, sel);
                        ss = newss;
                    }
                    else {
                        break;
                    }
                }
                return ss;
            }

            /// <summary>
            /// Get the MetaDataMember from the given table. Look in the inheritance hierarchy.
            /// The member is expected to be there and an exception will be thrown if it isn't.
            /// </summary>
            /// <param name="type">The hierarchy type that should have the member.</param>
            /// <param name="mi">The member to retrieve.</param>
            /// <returns>The MetaDataMember for the type.</returns>
            private static MetaDataMember GetRequiredInheritanceDataMember(MetaType type, MemberInfo mi) {
                System.Diagnostics.Debug.Assert(type != null);
                System.Diagnostics.Debug.Assert(mi != null);
                MetaType root = type.GetInheritanceType(mi.DeclaringType);
                if (root == null) {
                    throw Error.UnmappedDataMember(mi, mi.DeclaringType, type);
                }
                return root.GetDataMember(mi);
            }

            internal override SqlStatement VisitAssign(SqlAssign sa) {
                sa.LValue = this.FetchExpression(sa.LValue);
                sa.RValue = this.FetchExpression(sa.RValue);
                return sa;
            }

            internal SqlExpression ExpandExpression(SqlExpression expression) {
                SqlExpression expanded = this.expander.Expand(expression);
                if (expanded != expression) {
                    expanded = this.VisitExpression(expanded);
                }
                return expanded;
            }

            internal override SqlExpression VisitAliasRef(SqlAliasRef aref) {
                return this.ExpandExpression(aref);
            }

            internal override SqlAlias VisitAlias(SqlAlias a) {
                SqlAlias saveAlias = this.currentAlias;
                if (a.Node.NodeType == SqlNodeType.Table) {
                    this.outerAliasMap[a] = this.currentAlias;
                }
                this.currentAlias = a;
                try {
                    a.Node = this.ConvertToFetchedSequence(this.Visit(a.Node));
                    return a;
                }
                finally {
                    this.currentAlias = saveAlias;
                }
            }

            internal override SqlNode VisitLink(SqlLink link) {
                link = (SqlLink)base.VisitLink(link);

                // prefetch all 'LoadWith' links
                if (!this.disableInclude && this.shape != null && this.alreadyIncluded != null) {
                    MetaDataMember mdm = link.Member;
                    MemberInfo mi = mdm.Member;
                    if (this.shape.IsPreloaded(mi) && mdm.LoadMethod == null) {
                        // Is the other side of the relation in the list already?
                        MetaType otherType = mdm.DeclaringType.InheritanceRoot;
                        if (!this.alreadyIncluded.Contains(otherType)) {
                            this.alreadyIncluded.Add(otherType);
                            SqlNode fetched = this.ConvertToFetchedExpression(link);
                            this.alreadyIncluded.Remove(otherType);
                            return fetched;
                        }
                    }
                }

                if (this.inGroupBy && link.Expansion != null) {
                    return this.VisitLinkExpansion(link);
                }

                return link;
            }

            internal override SqlExpression VisitSharedExpressionRef(SqlSharedExpressionRef sref) {
                // always make a copy
                return (SqlExpression) SqlDuplicator.Copy(sref.SharedExpression.Expression);
            }

            internal override SqlExpression VisitSharedExpression(SqlSharedExpression shared) {
                shared.Expression = this.VisitExpression(shared.Expression);
                // shared expressions in group-by/select must be only column refs
                if (shared.Expression.NodeType == SqlNodeType.ColumnRef) {
                    return shared.Expression;
                }
                else {
                    // not simple? better push it down (make a sub-select that projects the relevant bits
                    shared.Expression = this.PushDownExpression(shared.Expression);
                    return shared.Expression;
                }
            }
       
            internal override SqlExpression VisitSimpleExpression(SqlSimpleExpression simple) {
                simple.Expression = this.VisitExpression(simple.Expression);
                if (SimpleExpression.IsSimple(simple.Expression)) {
                    return simple.Expression;
                }
                SqlExpression result = this.PushDownExpression(simple.Expression);
                // simple expressions must be scalar (such that they can be formed into a single column declaration)
                System.Diagnostics.Debug.Assert(result is SqlColumnRef);
                return result;
            }

            // add a new sub query that projects the given expression
            private SqlExpression PushDownExpression(SqlExpression expr) {
                // make sure this expression was columnized like a selection
                if (expr.NodeType == SqlNodeType.Value && expr.SqlType.CanBeColumn) {
                    expr = new SqlColumn(expr.ClrType, expr.SqlType, null, null, expr, expr.SourceExpression);
                }
                else {
                    expr = this.columnizer.ColumnizeSelection(expr);
                }

                SqlSelect simple = new SqlSelect(expr, this.currentSelect.From, expr.SourceExpression);
                this.currentSelect.From = new SqlAlias(simple);

                // make a copy of the expression for the current scope
                return this.ExpandExpression(expr);
            }

            internal override SqlSource VisitJoin(SqlJoin join) {
                if (join.JoinType == SqlJoinType.CrossApply ||
                    join.JoinType == SqlJoinType.OuterApply) {
                    join.Left = this.VisitSource(join.Left);

                    SqlSelect saveSelect = this.currentSelect;
                    try {
                        this.currentSelect = this.GetSourceSelect(join.Left);
                        join.Right = this.VisitSource(join.Right);

                        this.currentSelect = null;
                        join.Condition = this.VisitExpression(join.Condition);

                        return join;
                    }
                    finally {
                        this.currentSelect = saveSelect;
                    }
                }
                else {
                    return base.VisitJoin(join);
                }
            }

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
            private SqlSelect GetSourceSelect(SqlSource source) {
                SqlAlias alias = source as SqlAlias;
                if (alias == null) { 
                    return null; 
                }
                return alias.Node as SqlSelect;
            }

            internal override SqlSelect VisitSelect(SqlSelect select) {
                LinkOptimizationScope saveScope = this.linkMap;
                SqlSelect saveSelect = this.currentSelect;
                bool saveInGroupBy = inGroupBy;
                inGroupBy = false;

                try {
                    // don't preserve any link optimizations across a group or distinct boundary
                    bool linkOptimize = true;
                    if (this.binder.optimizeLinkExpansions &&
                        (select.GroupBy.Count > 0 || this.aggregateChecker.HasAggregates(select) || select.IsDistinct)) {
                        linkOptimize = false;
                        this.linkMap = new LinkOptimizationScope(this.linkMap);
                    }
                    select.From = this.VisitSource(select.From);
                    this.currentSelect = select;

                    select.Where = this.VisitExpression(select.Where);

                    this.inGroupBy = true;
                    for (int i = 0, n = select.GroupBy.Count; i < n; i++) {
                        select.GroupBy[i] = this.VisitExpression(select.GroupBy[i]);
                    }
                    this.inGroupBy = false;

                    select.Having = this.VisitExpression(select.Having);
                    for (int i = 0, n = select.OrderBy.Count; i < n; i++) {
                        select.OrderBy[i].Expression = this.VisitExpression(select.OrderBy[i].Expression);
                    }
                    select.Top = this.VisitExpression(select.Top);
                    select.Row = (SqlRow)this.Visit(select.Row);

                    select.Selection = this.VisitExpression(select.Selection);
                    select.Selection = this.columnizer.ColumnizeSelection(select.Selection);
                    if (linkOptimize) {
                        select.Selection = ConvertLinks(select.Selection);
                    }

                    // optimize out where clause for WHERE TRUE
                    if (select.Where != null && select.Where.NodeType == SqlNodeType.Value && (bool)((SqlValue)select.Where).Value) {
                        select.Where = null;
                    }
                }
                finally {
                    this.currentSelect = saveSelect;
                    this.linkMap = saveScope;
                    this.inGroupBy = saveInGroupBy;
                }

                return select;
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss) {
                // don't preserve any link optimizations across sub-queries
                LinkOptimizationScope saveScope = this.linkMap;
                SqlSelect saveSelect = this.currentSelect;
                try {
                    this.linkMap = new LinkOptimizationScope(this.linkMap);
                    this.currentSelect = null;
                    return base.VisitSubSelect(ss);
                }
                finally {
                    this.linkMap = saveScope;
                    this.currentSelect = saveSelect;
                }
            }

            /// <summary>
            /// Convert links. Need to recurse because there may be a client case with cases that are links.
            /// </summary>
            private SqlExpression ConvertLinks(SqlExpression node) {
                if (node == null) {
                    return null;
                }
                switch (node.NodeType) {
                    case SqlNodeType.Column: {
                            SqlColumn col = (SqlColumn)node;
                            if (col.Expression != null) {
                                col.Expression = this.ConvertLinks(col.Expression);
                            }
                            return node;
                        }
                    case SqlNodeType.OuterJoinedValue: {
                        SqlExpression o = ((SqlUnary)node).Operand;
                        SqlExpression e = this.ConvertLinks(o);
                        if (e == o) {
                            return node;
                        }
                        if (e.NodeType != SqlNodeType.OuterJoinedValue) {
                            return sql.Unary(SqlNodeType.OuterJoinedValue, e);
                        }
                        return e;
                    }
                    case SqlNodeType.Link:
                        return this.ConvertToFetchedExpression((SqlLink)node);
                    case SqlNodeType.ClientCase: {
                            SqlClientCase sc = (SqlClientCase)node;
                            foreach (SqlClientWhen when in sc.Whens) {
                                SqlExpression converted = ConvertLinks(when.Value);
                                when.Value = converted;
                                if (!sc.ClrType.IsAssignableFrom(when.Value.ClrType)) {
                                    throw Error.DidNotExpectTypeChange(when.Value.ClrType, sc.ClrType);
                                }

                            }
                            return node;
                        }
                }
                return node;
            }

            internal SqlExpression ConvertToExpression(SqlNode node) {
                if (node == null) {
                    return null;
                }
                SqlExpression x = node as SqlExpression;
                if (x != null) {
                    return x;
                }
                SqlSelect select = node as SqlSelect;
                if (select != null) {
                    SqlSubSelect ms = sql.SubSelect(SqlNodeType.Multiset, select);
                    return ms;
                }
                throw Error.UnexpectedNode(node.NodeType);
            }

            [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "Microsoft: Cast is dependent on node type and casts do not happen unecessarily in a single code path.")]
            [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            internal SqlExpression ConvertToFetchedExpression(SqlNode node) {
                if (node == null) {
                    return null;
                }
                switch (node.NodeType) {
                    case SqlNodeType.OuterJoinedValue: {
                            SqlExpression o = ((SqlUnary)node).Operand;
                            SqlExpression e = this.ConvertLinks(o);
                            if (e == o) {
                                return (SqlExpression)node;
                            }
                            return e;
                        }
                    case SqlNodeType.ClientCase: {
                            // Need to recurse in case the object case has links.
                            SqlClientCase cc = (SqlClientCase)node;
                            List<SqlNode> fetchedValues = new List<SqlNode>();
                            bool allExprs = true;
                            foreach (SqlClientWhen when in cc.Whens) {
                                SqlNode fetchedValue = ConvertToFetchedExpression(when.Value);
                                allExprs = allExprs && (fetchedValue is SqlExpression);
                                fetchedValues.Add(fetchedValue);
                            }

                            if (allExprs) {
                                // All WHEN values are simple expressions (no sequences). 
                                List<SqlExpression> matches = new List<SqlExpression>();
                                List<SqlExpression> values = new List<SqlExpression>();
                                for (int i = 0, c = fetchedValues.Count; i < c; ++i) {
                                    SqlExpression fetchedValue = (SqlExpression)fetchedValues[i];
                                    if (!cc.ClrType.IsAssignableFrom(fetchedValue.ClrType)) {
                                        throw Error.DidNotExpectTypeChange(cc.ClrType, fetchedValue.ClrType);
                                    }
                                    matches.Add(cc.Whens[i].Match);
                                    values.Add(fetchedValue);
                                }
                                node = sql.Case(cc.ClrType, cc.Expression, matches, values, cc.SourceExpression);
                            }
                            else {
                                node = SimulateCaseOfSequences(cc, fetchedValues);
                            }
                            break;
                        }
                    case SqlNodeType.TypeCase: {
                            SqlTypeCase tc = (SqlTypeCase)node;
                            List<SqlNode> fetchedValues = new List<SqlNode>();
                            foreach (SqlTypeCaseWhen when in tc.Whens) {
                                SqlNode fetchedValue = ConvertToFetchedExpression(when.TypeBinding);
                                fetchedValues.Add(fetchedValue);
                            }

                            for (int i = 0, c = fetchedValues.Count; i < c; ++i) {
                                SqlExpression fetchedValue = (SqlExpression)fetchedValues[i];
                                tc.Whens[i].TypeBinding = fetchedValue;
                            }
                            break;
                        }
                    case SqlNodeType.SearchedCase: {
                            SqlSearchedCase sc = (SqlSearchedCase)node;
                            foreach (SqlWhen when in sc.Whens) {
                                when.Match = this.ConvertToFetchedExpression(when.Match);
                                when.Value = this.ConvertToFetchedExpression(when.Value);
                            }
                            sc.Else = this.ConvertToFetchedExpression(sc.Else);
                            break;
                        }
                    case SqlNodeType.Link: {
                            SqlLink link = (SqlLink)node;

                            if (link.Expansion != null) {
                                return this.VisitLinkExpansion(link);
                            }

                            SqlExpression cached;
                            if (this.linkMap.TryGetValue(link.Id, out cached)) {
                                return this.VisitExpression(cached);
                            }

                            // translate link into expanded form
                            node = this.translator.TranslateLink(link, true);

                            // New nodes may have been produced because of Subquery.
                            // Prebind again for method-call and static treat handling.
                            node = binder.Prebind(node);

                            // Make it an expression.
                            node = this.ConvertToExpression(node);

                            // bind the translation
                            node = this.Visit(node);

                            // Check for element node, rewrite as sql apply.
                            if (this.currentSelect != null 
                                && node != null 
                                && node.NodeType == SqlNodeType.Element 
                                && link.Member.IsAssociation
                                && this.binder.OptimizeLinkExpansions
                                ) {
                                // if link in a non-nullable foreign key association then inner-join is okay to use (since it must always exist)
                                // otherwise use left-outer-join 
                                SqlJoinType joinType = (link.Member.Association.IsForeignKey && !link.Member.Association.IsNullable)
                                    ? SqlJoinType.Inner : SqlJoinType.LeftOuter;
                                SqlSubSelect ss = (SqlSubSelect)node;
                                SqlExpression where = ss.Select.Where;
                                ss.Select.Where = null;
                                // form cross apply 
                                SqlAlias sa = new SqlAlias(ss.Select);
                                if (joinType == SqlJoinType.Inner && this.IsOuterDependent(this.currentSelect.From, sa, where))
                                {
                                    joinType = SqlJoinType.LeftOuter;
                                }
                                this.currentSelect.From = sql.MakeJoin(joinType, this.currentSelect.From, sa, where, ss.SourceExpression);
                                SqlExpression result = new SqlAliasRef(sa);
                                this.linkMap.Add(link.Id, result);
                                return this.VisitExpression(result);
                            }
                        }
                        break;
                }
                return (SqlExpression)node;
            }

            // insert new join in an appropriate location within an existing join tree
            private bool IsOuterDependent(SqlSource location, SqlAlias alias, SqlExpression where)
            {
                HashSet<SqlAlias> consumed = SqlGatherConsumedAliases.Gather(where);
                consumed.ExceptWith(SqlGatherProducedAliases.Gather(alias));
                HashSet<SqlAlias> produced;
                if (this.IsOuterDependent(false, location, consumed, out produced))
                    return true;
                return false;
            }

            // insert new join closest to the aliases it depends on
            private bool IsOuterDependent(bool isOuterDependent, SqlSource location, HashSet<SqlAlias> consumed, out HashSet<SqlAlias> produced)
            {
                if (location.NodeType == SqlNodeType.Join)
                {

                    // walk down join tree looking for best location for join
                    SqlJoin join = (SqlJoin)location;
                    if (this.IsOuterDependent(isOuterDependent, join.Left, consumed, out produced))
                        return true;

                    HashSet<SqlAlias> rightProduced;
                    bool rightIsOuterDependent = join.JoinType == SqlJoinType.LeftOuter || join.JoinType == SqlJoinType.OuterApply;
                    if (this.IsOuterDependent(rightIsOuterDependent, join.Right, consumed, out rightProduced))
                        return true;
                    produced.UnionWith(rightProduced);
                }
                else 
                {
                    SqlAlias a = location as SqlAlias;
                    if (a != null)
                    {
                        SqlSelect s = a.Node as SqlSelect;
                        if (s != null && !isOuterDependent && s.From != null)
                        {
                            if (this.IsOuterDependent(false, s.From, consumed, out produced))
                                return true;
                        }
                    }
                    produced = SqlGatherProducedAliases.Gather(location);
                }
                // look to see if this subtree fully satisfies join condition
                if (consumed.IsSubsetOf(produced))
                {
                    return isOuterDependent;
                }
                return false;
            }

            /// <summary>
            /// The purpose of this function is to look in 'node' for delay-fetched structures (eg Links)
            /// and to make them into fetched structures that will be evaluated directly in the query.
            /// </summary>
            internal SqlNode ConvertToFetchedSequence(SqlNode node) {
                if (node == null) {
                    return node;
                }

                while (node.NodeType == SqlNodeType.OuterJoinedValue) {
                    node = ((SqlUnary)node).Operand;
                }

                SqlExpression expr = node as SqlExpression;
                if (expr == null) {
                    return node;
                }

                if (!TypeSystem.IsSequenceType(expr.ClrType)) {
                    throw Error.SequenceOperatorsNotSupportedForType(expr.ClrType);
                }

                if (expr.NodeType == SqlNodeType.Value) {
                    throw Error.QueryOnLocalCollectionNotSupported();
                }

                if (expr.NodeType == SqlNodeType.Link) {
                    SqlLink link = (SqlLink)expr;

                    if (link.Expansion != null) {
                        return this.VisitLinkExpansion(link);
                    }

                    // translate link into expanded form
                    node = this.translator.TranslateLink(link, false);

                    // New nodes may have been produced because of Subquery.
                    // Prebind again for method-call and static treat handling.
                    node = binder.Prebind(node);

                    // bind the translation
                    node = this.Visit(node);
                }
                else if (expr.NodeType == SqlNodeType.Grouping) {
                    node = ((SqlGrouping)expr).Group;
                }
                else if (expr.NodeType == SqlNodeType.ClientCase) {
                  /* 
                     * Client case needs to be handled here because it may be a client-case
                     * of delay-fetch structures such as links (or other client cases of links):
                     * 
                     * CASE [Disc]
                     *  WHEN 'X' THEN A
                     *  WHEN 'Y' THEN B
                     * END
                     * 
                     * Abstractly, this would be rewritten as 
                     * 
                     * CASE [Disc]
                     *  WHEN 'X' THEN ConvertToFetchedSequence(A)
                     *  WHEN 'Y' THEN ConvertToFetchedSequence(B)
                     * END
                     * 
                     * The hitch is that the result of ConvertToFetchedSequence() is likely 
                     * to be a SELECT which is not legal in a CASE. Instead, we need to rewrite as
                     * 
                     * SELECT [ProjectionX] WHERE [Disc]='X'
                     * UNION ALL
                     * SELECT [ProjectionY] WHERE [Disc]='Y'
                     * 
                     * In other words, a Union where only one SELECT will have a WHERE clase
                     * that can produce a non-empty set for each instance of [Disc].
                     */
                    SqlClientCase sc = (SqlClientCase)expr;
                    List<SqlNode> newValues = new List<SqlNode>();
                    bool rewrite = false;
                    bool allSame = true;
                    foreach (SqlClientWhen when in sc.Whens) {
                        SqlNode newValue = ConvertToFetchedSequence(when.Value);
                        rewrite = rewrite || (newValue != when.Value);
                        newValues.Add(newValue);
                        allSame = allSame && SqlComparer.AreEqual(when.Value, sc.Whens[0].Value);
                    }

                    if (rewrite) {
                        if (allSame) {
                            // If all branches are the same then just take one.
                            node = newValues[0];
                        }
                        else {
                            node = this.SimulateCaseOfSequences(sc, newValues);
                        }
                    }
                }

                SqlSubSelect ss = node as SqlSubSelect;
                if (ss != null) {
                    node = ss.Select;
                }

                return node;
            }

            private SqlExpression VisitLinkExpansion(SqlLink link) {
                SqlAliasRef aref = link.Expansion as SqlAliasRef;
                if (aref != null && aref.Alias.Node.NodeType == SqlNodeType.Table) {
                    SqlAlias outerAlias;
                    if (this.outerAliasMap.TryGetValue(aref.Alias, out outerAlias)) {
                        return this.VisitAliasRef(new SqlAliasRef(outerAlias));
                    }
                    // should not happen
                    System.Diagnostics.Debug.Assert(false);
                }
                return this.VisitExpression(link.Expansion);
            }

            /// <summary>
            /// Given a ClientCase and a list of sequence (one for each case), construct a structure
            /// that is equivalent to a CASE of SELECTs. To accomplish this we use UNION ALL and attach
            /// a WHERE clause which will pick the SELECT that matches the discriminator in the Client Case.
            /// </summary>
            private SqlSelect SimulateCaseOfSequences(SqlClientCase clientCase, List<SqlNode> sequences) {
                /*
                   * There are two situations we may be in:
                   * (1) There is exactly one case alternative. 
                   *     Here, no where clause is needed.
                   * (2) There is more than case alternative.
                   *     Here, each WHERE clause needs to be ANDed with [Disc]=D where D
                   *     is the literal discriminanator value.
                   */
                if (sequences.Count == 1) {
                    return (SqlSelect)sequences[0];
                }
                else {
                    SqlNode union = null;
                    SqlSelect sel = null;
                    int elseIndex = clientCase.Whens.Count - 1;
                    int elseCount = clientCase.Whens[elseIndex].Match == null ? 1 : 0;
                    SqlExpression elseFilter = null;
                    for (int i = 0; i < sequences.Count - elseCount; ++i) {
                        sel = (SqlSelect)sequences[i];
                        SqlExpression discriminatorPredicate = sql.Binary(SqlNodeType.EQ, clientCase.Expression, clientCase.Whens[i].Match);
                        sel.Where = sql.AndAccumulate(sel.Where, discriminatorPredicate);
                        elseFilter = sql.AndAccumulate(elseFilter, sql.Binary(SqlNodeType.NE, clientCase.Expression, clientCase.Whens[i].Match));

                        if (union == null) {
                            union = sel;
                        }
                        else {
                            union = new SqlUnion(sel, union, true /* Union All */);
                        }
                    }
                    // Handle 'else' if present.
                    if (elseCount == 1) {
                        sel = (SqlSelect)sequences[elseIndex];
                        sel.Where = sql.AndAccumulate(sel.Where, elseFilter);

                        if (union == null) {
                            union = sel;
                        }
                        else {
                            union = new SqlUnion(sel, union, true /* Union All */);
                        }

                    }
                    SqlAlias alias = new SqlAlias(union);
                    SqlAliasRef aref = new SqlAliasRef(alias);
                    return new SqlSelect(aref, alias, union.SourceExpression);
                }
            }
        }
    }
}
