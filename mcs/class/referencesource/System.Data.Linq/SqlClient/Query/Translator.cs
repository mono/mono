using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Text;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.Linq.Provider;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq.SqlClient {


    internal class Translator {
        IDataServices services;
        SqlFactory sql;
        TypeSystemProvider typeProvider;

        internal Translator(IDataServices services, SqlFactory sqlFactory, TypeSystemProvider typeProvider) {
            this.services = services;
            this.sql = sqlFactory;
            this.typeProvider = typeProvider;
        }

        internal SqlSelect BuildDefaultQuery(MetaType rowType, bool allowDeferred, SqlLink link, Expression source) {
            System.Diagnostics.Debug.Assert(rowType != null && rowType.Table != null);
            if (rowType.HasInheritance && rowType.InheritanceRoot != rowType) {
                // RowType is expected to be an inheritance root.
                throw Error.ArgumentWrongValue("rowType");
            }
            SqlTable table = sql.Table(rowType.Table, rowType, source);
            SqlAlias tableAlias = new SqlAlias(table);
            SqlAliasRef tableAliasRef = new SqlAliasRef(tableAlias);

            SqlExpression projection = this.BuildProjection(tableAliasRef, table.RowType, allowDeferred, link, source);
            return new SqlSelect(projection, tableAlias, source);
        }

        internal SqlExpression BuildProjection(SqlExpression item, MetaType rowType, bool allowDeferred, SqlLink link, Expression source) {
            if (!rowType.HasInheritance) {
                return this.BuildProjectionInternal(item, rowType, (rowType.Table != null) ? rowType.PersistentDataMembers : rowType.DataMembers, allowDeferred, link, source);
            }
            else {
                // Build a type case that represents a switch between the various type.
                List<MetaType> mappedTypes = new List<MetaType>(rowType.InheritanceTypes);
                List<SqlTypeCaseWhen> whens = new List<SqlTypeCaseWhen>();
                SqlTypeCaseWhen @else = null;

                MetaType root = rowType.InheritanceRoot;
                MetaDataMember discriminator = root.Discriminator;
                Type dt = discriminator.Type;
                SqlMember dm = sql.Member(item, discriminator.Member);

                foreach (MetaType type in mappedTypes) {
                    if (type.HasInheritanceCode) {
                        SqlNew defaultProjection = this.BuildProjectionInternal(item, type, type.PersistentDataMembers, allowDeferred, link, source);
                        if (type.IsInheritanceDefault) {
                            @else = new SqlTypeCaseWhen(null, defaultProjection);
                        }
                        // Add an explicit case even for the default.
                        // Redundant results will be optimized out later.
                        object code = InheritanceRules.InheritanceCodeForClientCompare(type.InheritanceCode, dm.SqlType);
                        SqlExpression match = sql.Value(dt, sql.Default(discriminator), code, true, source);
                        whens.Add(new SqlTypeCaseWhen(match, defaultProjection));
                    }
                }
                if (@else == null) {
                    throw Error.EmptyCaseNotSupported();
                }
                whens.Add(@else);   // Add the else at the end.
                
                return sql.TypeCase(root.Type, root, dm, whens.ToArray(), source);
            }
        }

        /// <summary>
        ///  Check whether this member will be preloaded.
        /// </summary>
        private bool IsPreloaded(MemberInfo member) {
            if (this.services.Context.LoadOptions == null) {
                return false;
            }
            return this.services.Context.LoadOptions.IsPreloaded(member);
        }

        private SqlNew BuildProjectionInternal(SqlExpression item, MetaType rowType, IEnumerable<MetaDataMember> members, bool allowDeferred, SqlLink link, Expression source) {
            List<SqlMemberAssign> bindings = new List<SqlMemberAssign>();
            foreach (MetaDataMember mm in members) {
                if (allowDeferred && (mm.IsAssociation || mm.IsDeferred)) {
                    // check if this member is the reverse association to the supplied link
                    if (link != null && mm != link.Member && mm.IsAssociation
                        && mm.MappedName == link.Member.MappedName
                        && !mm.Association.IsMany
                        && !IsPreloaded(link.Member.Member)) {
                        // place a new link here with an expansion that is previous link's root expression.
                        // this will allow joins caused by reverse association references to 'melt' away. :-)
                        SqlLink mlink = this.BuildLink(item, mm, source);
                        mlink.Expansion = link.Expression;
                        bindings.Add(new SqlMemberAssign(mm.Member, mlink));
                    }
                    else {
                        bindings.Add(new SqlMemberAssign(mm.Member, this.BuildLink(item, mm, source)));
                    }
                } 
                else if (!mm.IsAssociation) {
                    bindings.Add(new SqlMemberAssign(mm.Member, sql.Member(item, mm)));
                }
            }
            ConstructorInfo cons = rowType.Type.GetConstructor(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic, null, System.Type.EmptyTypes, null);
            if (cons == null) {
                throw Error.MappedTypeMustHaveDefaultConstructor(rowType.Type);
            }
            return sql.New(rowType, cons, null, null, bindings, source);
        }

        private SqlLink BuildLink(SqlExpression item, MetaDataMember member, Expression source) {
            if (member.IsAssociation) {
                SqlExpression[] exprs = new SqlExpression[member.Association.ThisKey.Count];
                for (int i = 0, n = exprs.Length; i < n; i++) {
                    MetaDataMember mm = member.Association.ThisKey[i];
                    exprs[i] = sql.Member(item, mm.Member);
                }
                MetaType otherType = member.Association.OtherType;
                return new SqlLink(new object(), otherType, member.Type, typeProvider.From(member.Type), item, member, exprs, null, source);
            }
            else {
                // if not association link is always based on primary key
                MetaType thisType = member.DeclaringType;
                System.Diagnostics.Debug.Assert(thisType.IsEntity);
                List<SqlExpression> exprs = new List<SqlExpression>();
                foreach (MetaDataMember mm in thisType.IdentityMembers) {
                    exprs.Add(sql.Member(item, mm.Member));
                }
                SqlExpression expansion = sql.Member(item, member.Member);
                return new SqlLink(new object(), thisType, member.Type, typeProvider.From(member.Type), item, member, exprs, expansion, source);
            }
        }

        internal SqlNode TranslateLink(SqlLink link, bool asExpression) {
            return this.TranslateLink(link, link.KeyExpressions, asExpression);
        }

        /// <summary>
        /// Create an Expression representing the given association and key value expressions.
        /// </summary>
        internal static Expression TranslateAssociation(DataContext context, MetaAssociation association, Expression otherSource, Expression[] keyValues, Expression thisInstance) {
            if (association == null)
                throw Error.ArgumentNull("association");
            if (keyValues == null)
                throw Error.ArgumentNull("keyValues");

            if (context.LoadOptions!=null) {
                LambdaExpression subquery = context.LoadOptions.GetAssociationSubquery(association.ThisMember.Member);
                if (subquery!=null) {
                    RelationComposer rc = new RelationComposer(subquery.Parameters[0], association, otherSource, thisInstance);
                    return rc.Visit(subquery.Body);
                }
            }
            return WhereClauseFromSourceAndKeys(otherSource, association.OtherKey.ToArray(), keyValues);
        }

        internal static Expression WhereClauseFromSourceAndKeys(Expression source, MetaDataMember[] keyMembers, Expression [] keyValues) {
            Type elementType = TypeSystem.GetElementType(source.Type);
            ParameterExpression p = Expression.Parameter(elementType, "p");
            Expression whereExpression=null;
            for (int i = 0; i < keyMembers.Length; i++) {
                MetaDataMember metaMember = keyMembers[i];
                Expression parameterAsDeclaring = elementType == metaMember.Member.DeclaringType ?                    
                    (Expression)p : (Expression)Expression.Convert(p, metaMember.Member.DeclaringType);
                Expression memberExpression = (metaMember.Member is FieldInfo)
                    ? Expression.Field(parameterAsDeclaring, (FieldInfo)metaMember.Member)
                    : Expression.Property(parameterAsDeclaring, (PropertyInfo)metaMember.Member);
                Expression keyValue = keyValues[i];
                if (keyValue.Type != memberExpression.Type)
                    keyValue = Expression.Convert(keyValue, memberExpression.Type);
                Expression memberEqualityExpression = Expression.Equal(memberExpression, keyValue);
                whereExpression = (whereExpression != null)
                    ? Expression.And(whereExpression, memberEqualityExpression)
                    : memberEqualityExpression;
            }
            Expression sequenceExpression = Expression.Call(typeof(Enumerable), "Where", new Type[] {p.Type}, source, Expression.Lambda(whereExpression, p));
            return sequenceExpression;
        }

        /// <summary>
        /// Composes a subquery into a linked association.
        /// </summary>
        private class RelationComposer : ExpressionVisitor {
            ParameterExpression parameter;
            MetaAssociation association;
            Expression otherSouce;
            Expression parameterReplacement;
            internal RelationComposer(ParameterExpression parameter, MetaAssociation association, Expression otherSouce, Expression parameterReplacement) {
                if (parameter==null)
                    throw Error.ArgumentNull("parameter");
                if (association == null)
                    throw Error.ArgumentNull("association");
                if (otherSouce == null)
                    throw Error.ArgumentNull("otherSouce");
                if (parameterReplacement==null)
                    throw Error.ArgumentNull("parameterReplacement");
                this.parameter = parameter;
                this.association = association;
                this.otherSouce = otherSouce;
                this.parameterReplacement = parameterReplacement;
            }
            internal override Expression VisitParameter(ParameterExpression p) {
                if (p == parameter) {
                    return this.parameterReplacement;
                }
                return base.VisitParameter(p);
            }

            private static Expression[] GetKeyValues(Expression expr, ReadOnlyCollection<MetaDataMember> keys) {
                List<Expression> values = new List<Expression>();
                foreach(MetaDataMember key in keys){
                    values.Add(Expression.PropertyOrField(expr, key.Name));
                }
                return values.ToArray();
            }

            internal override Expression VisitMemberAccess(MemberExpression m) {
                if (MetaPosition.AreSameMember(m.Member, this.association.ThisMember.Member)) {
                    Expression[] keyValues = GetKeyValues(this.Visit(m.Expression), this.association.ThisKey);
                    return WhereClauseFromSourceAndKeys(this.otherSouce, this.association.OtherKey.ToArray(), keyValues);
                }
                Expression exp = this.Visit(m.Expression);
                if (exp != m.Expression) {
                    if (exp.Type != m.Expression.Type && m.Member.Name == "Count" && TypeSystem.IsSequenceType(exp.Type)) {
                        return Expression.Call(typeof(Enumerable), "Count", new Type[] {TypeSystem.GetElementType(exp.Type)}, exp);
                    }
                    return Expression.MakeMemberAccess(exp, m.Member);
                }
                return m;
            }

        }

        internal SqlNode TranslateLink(SqlLink link, List<SqlExpression> keyExpressions, bool asExpression) {
            MetaDataMember mm = link.Member;

            if (mm.IsAssociation) {
                // Create the row source.
                MetaType otherType = mm.Association.OtherType;
                Type tableType = otherType.InheritanceRoot.Type;
                ITable table = this.services.Context.GetTable(tableType);
                Expression source = new LinkedTableExpression(link, table, typeof(IQueryable<>).MakeGenericType(otherType.Type));
                // Build key expression nodes.
                Expression[] keyExprs = new Expression[keyExpressions.Count];
                for (int i = 0; i < keyExpressions.Count; ++i) {
                    MetaDataMember metaMember = mm.Association.OtherKey[i];
                    Type memberType = TypeSystem.GetMemberType(metaMember.Member);
                    keyExprs[i] = InternalExpression.Known(keyExpressions[i], memberType);
                }
                Expression lex = link.Expression != null 
                    ? (Expression)InternalExpression.Known(link.Expression) 
                    : (Expression)Expression.Constant(null, link.Member.Member.DeclaringType);
                Expression expr = TranslateAssociation(this.services.Context, mm.Association, source, keyExprs, lex);
                // Convert
                QueryConverter qc = new QueryConverter(this.services, this.typeProvider, this, this.sql);
                SqlSelect sel = (SqlSelect)qc.ConvertInner(expr, link.SourceExpression);
                // Turn it into an expression is necessary
                SqlNode result = sel;
                if (asExpression) {
                    if (mm.Association.IsMany) {
                        result = new SqlSubSelect(SqlNodeType.Multiset, link.ClrType, link.SqlType, sel);
                    }
                    else {
                        result = new SqlSubSelect(SqlNodeType.Element, link.ClrType, link.SqlType, sel);
                    }
                }
                return result;
            }
            else {
                System.Diagnostics.Debug.Assert(link.Expansion != null);
                System.Diagnostics.Debug.Assert(link.KeyExpressions == keyExpressions);
                // deferred expression already defined...
                return link.Expansion;
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification="These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        internal SqlExpression TranslateEquals(SqlBinary expr) {
            System.Diagnostics.Debug.Assert(
                expr.NodeType == SqlNodeType.EQ || expr.NodeType == SqlNodeType.NE ||
                expr.NodeType == SqlNodeType.EQ2V || expr.NodeType == SqlNodeType.NE2V);
            SqlExpression eLeft = expr.Left;
            SqlExpression eRight = expr.Right;

            if (eRight.NodeType == SqlNodeType.Element) {
                SqlSubSelect sub = (SqlSubSelect)eRight;
                SqlAlias alias = new SqlAlias(sub.Select);
                SqlAliasRef aref = new SqlAliasRef(alias);
                SqlSelect select = new SqlSelect(aref, alias, expr.SourceExpression);
                select.Where = sql.Binary(expr.NodeType, sql.DoNotVisitExpression(eLeft), aref);
                return sql.SubSelect(SqlNodeType.Exists, select);
            }
            else if (eLeft.NodeType == SqlNodeType.Element) {
                SqlSubSelect sub = (SqlSubSelect)eLeft;
                SqlAlias alias = new SqlAlias(sub.Select);
                SqlAliasRef aref = new SqlAliasRef(alias);
                SqlSelect select = new SqlSelect(aref, alias, expr.SourceExpression);
                select.Where = sql.Binary(expr.NodeType, sql.DoNotVisitExpression(eRight), aref);
                return sql.SubSelect(SqlNodeType.Exists, select);
            }

            MetaType mtLeft = TypeSource.GetSourceMetaType(eLeft, this.services.Model);
            MetaType mtRight = TypeSource.GetSourceMetaType(eRight, this.services.Model);

            if (eLeft.NodeType == SqlNodeType.TypeCase) {
                eLeft = BestIdentityNode((SqlTypeCase)eLeft);
            }
            if (eRight.NodeType == SqlNodeType.TypeCase) {
                eRight = BestIdentityNode((SqlTypeCase)eRight);
            }

            if (mtLeft.IsEntity && mtRight.IsEntity && mtLeft.Table != mtRight.Table) {
                throw Error.CannotCompareItemsAssociatedWithDifferentTable();
            }

            // do simple or no translation for non-structural types
            if (!mtLeft.IsEntity && !mtRight.IsEntity && 
                (eLeft.NodeType != SqlNodeType.New || eLeft.SqlType.CanBeColumn) && 
                (eRight.NodeType != SqlNodeType.New || eRight.SqlType.CanBeColumn)) {
                if (expr.NodeType == SqlNodeType.EQ2V || expr.NodeType == SqlNodeType.NE2V) {
                    return this.TranslateEqualsOp(expr.NodeType, sql.DoNotVisitExpression(expr.Left), sql.DoNotVisitExpression(expr.Right), false);
                }
                return expr;
            }

            // If the two types are not comparable, we return the predicate "1=0".
            if ((mtLeft != mtRight) && (mtLeft.InheritanceRoot != mtRight.InheritanceRoot)) {
                return sql.Binary(SqlNodeType.EQ, sql.ValueFromObject(0,expr.SourceExpression), sql.ValueFromObject(1,expr.SourceExpression));
            }

            List<SqlExpression> exprs1;
            List<SqlExpression> exprs2;

            SqlLink link1 = eLeft as SqlLink;
            if (link1 != null && link1.Member.IsAssociation && link1.Member.Association.IsForeignKey) {
                exprs1 = link1.KeyExpressions;
            }
            else {
                exprs1 = this.GetIdentityExpressions(mtLeft, sql.DoNotVisitExpression(eLeft));
            }

            SqlLink link2 = eRight as SqlLink;
            if (link2 != null && link2.Member.IsAssociation && link2.Member.Association.IsForeignKey) {
                exprs2 = link2.KeyExpressions;
            }
            else {
                exprs2 = this.GetIdentityExpressions(mtRight, sql.DoNotVisitExpression(eRight));
            }

            System.Diagnostics.Debug.Assert(exprs1.Count > 0);
            System.Diagnostics.Debug.Assert(exprs2.Count > 0);
            System.Diagnostics.Debug.Assert(exprs1.Count == exprs2.Count);

            SqlExpression exp = null;
            SqlNodeType eqKind = (expr.NodeType == SqlNodeType.EQ2V || expr.NodeType == SqlNodeType.NE2V) ? SqlNodeType.EQ2V : SqlNodeType.EQ;
            for (int i = 0, n = exprs1.Count; i < n; i++) {
                SqlExpression eq = this.TranslateEqualsOp(eqKind, exprs1[i], exprs2[i], !mtLeft.IsEntity);
                if (exp == null) {
                    exp = eq;
                }
                else {
                    exp = sql.Binary(SqlNodeType.And, exp, eq);
                }
            }
            if (expr.NodeType == SqlNodeType.NE || expr.NodeType == SqlNodeType.NE2V) {
                exp = sql.Unary(SqlNodeType.Not, exp, exp.SourceExpression);
            }
            return exp;
        }

        private SqlExpression TranslateEqualsOp(SqlNodeType op, SqlExpression left, SqlExpression right, bool allowExpand) {
            switch (op) {
                case SqlNodeType.EQ:
                case SqlNodeType.NE:
                    return sql.Binary(op, left, right);
                case SqlNodeType.EQ2V:
                    if (SqlExpressionNullability.CanBeNull(left) != false &&
                        SqlExpressionNullability.CanBeNull(right) != false) {
                        SqlNodeType eqOp = allowExpand ? SqlNodeType.EQ2V : SqlNodeType.EQ;
                        return
                            sql.Binary(SqlNodeType.Or,
                                sql.Binary(SqlNodeType.And,
                                    sql.Unary(SqlNodeType.IsNull, (SqlExpression)SqlDuplicator.Copy(left)),
                                    sql.Unary(SqlNodeType.IsNull, (SqlExpression)SqlDuplicator.Copy(right))
                                    ),
                                sql.Binary(SqlNodeType.And,
                                    sql.Binary(SqlNodeType.And,
                                        sql.Unary(SqlNodeType.IsNotNull, (SqlExpression)SqlDuplicator.Copy(left)),
                                        sql.Unary(SqlNodeType.IsNotNull, (SqlExpression)SqlDuplicator.Copy(right))
                                        ),
                                    sql.Binary(eqOp, left, right)
                                    )
                                );
                    }
                    else {
                        SqlNodeType eqOp = allowExpand ? SqlNodeType.EQ2V : SqlNodeType.EQ;
                        return sql.Binary(eqOp, left, right);
                    }
                case SqlNodeType.NE2V:
                    if (SqlExpressionNullability.CanBeNull(left) != false &&
                        SqlExpressionNullability.CanBeNull(right) != false) {
                        SqlNodeType eqOp = allowExpand ? SqlNodeType.EQ2V : SqlNodeType.EQ;
                        return
                            sql.Unary(SqlNodeType.Not,
                                sql.Binary(SqlNodeType.Or,
                                    sql.Binary(SqlNodeType.And,
                                        sql.Unary(SqlNodeType.IsNull, (SqlExpression)SqlDuplicator.Copy(left)),
                                        sql.Unary(SqlNodeType.IsNull, (SqlExpression)SqlDuplicator.Copy(right))
                                        ),
                                    sql.Binary(SqlNodeType.And,
                                        sql.Binary(SqlNodeType.And,
                                            sql.Unary(SqlNodeType.IsNotNull, (SqlExpression)SqlDuplicator.Copy(left)),
                                            sql.Unary(SqlNodeType.IsNotNull, (SqlExpression)SqlDuplicator.Copy(right))
                                            ),
                                        sql.Binary(eqOp, left, right)
                                        )
                                    )
                                );
                    }
                    else {
                        SqlNodeType neOp = allowExpand ? SqlNodeType.NE2V : SqlNodeType.NE;
                        return sql.Binary(neOp, left, right);
                    }
                default:
                    throw Error.UnexpectedNode(op);
            }
        }

        internal SqlExpression TranslateLinkEquals(SqlBinary bo) {
            SqlLink link1 = bo.Left as SqlLink;
            SqlLink link2 = bo.Right as SqlLink;
            if ((link1 != null && link1.Member.IsAssociation && link1.Member.Association.IsForeignKey) ||
                (link2 != null && link2.Member.IsAssociation && link2.Member.Association.IsForeignKey)) {
                return this.TranslateEquals(bo);
            }
            return bo;
        }

        internal SqlExpression TranslateLinkIsNull(SqlUnary expr) {
            System.Diagnostics.Debug.Assert(expr.NodeType == SqlNodeType.IsNull || expr.NodeType == SqlNodeType.IsNotNull);

            SqlLink link = expr.Operand as SqlLink;
            if (!(link != null && link.Member.IsAssociation && link.Member.Association.IsForeignKey)) {
                return expr;
            }

            List<SqlExpression> exprs = link.KeyExpressions;
            System.Diagnostics.Debug.Assert(exprs.Count > 0);

            SqlExpression exp = null;
            SqlNodeType combo = (expr.NodeType == SqlNodeType.IsNull) ? SqlNodeType.Or : SqlNodeType.And;
            for (int i = 0, n = exprs.Count; i < n; i++) {
                SqlExpression compare = sql.Unary(expr.NodeType, sql.DoNotVisitExpression(exprs[i]), expr.SourceExpression);
                if (exp == null) {
                    exp = compare;
                }
                else {
                    exp = sql.Binary(combo, exp, compare);
                }
            }
            return exp;
        }

        /// <summary>
        /// Find the alternative in type case that will best identify the object.
        /// If there is a SqlNew it is expected to have all the identity fields.
        /// If there is no SqlNew then we must be dealing with all literal NULL alternatives. In this case,
        /// just return the first one.
        /// </summary>
        private static SqlExpression BestIdentityNode(SqlTypeCase tc) {
            foreach (SqlTypeCaseWhen when in tc.Whens) {
                if (when.TypeBinding.NodeType == SqlNodeType.New) {
                    return when.TypeBinding;
                }
            }
            return tc.Whens[0].TypeBinding; // There were no SqlNews, take the first alternative 
        }

        private static bool IsPublic(MemberInfo mi) {
            FieldInfo fi = mi as FieldInfo;
            if (fi != null) {
                return fi.IsPublic;
            }
            PropertyInfo pi = mi as PropertyInfo;
            if (pi != null) {
                if (pi.CanRead) {
                    var gm = pi.GetGetMethod();
                    if (gm != null) {
                        return gm.IsPublic;
                    } 
                }
            }
            return false;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
        private IEnumerable<MetaDataMember> GetIdentityMembers(MetaType type) {
            if (type.IsEntity) {
                return type.IdentityMembers;
            }
            return type.DataMembers.Where(m => IsPublic(m.Member));
        }

        private List<SqlExpression> GetIdentityExpressions(MetaType type, SqlExpression expr) {
            List<MetaDataMember> members = GetIdentityMembers(type).ToList();
            System.Diagnostics.Debug.Assert(members.Count > 0);
            List<SqlExpression> exprs = new List<SqlExpression>(members.Count);
            foreach (MetaDataMember mm in members) {
                exprs.Add(sql.Member((SqlExpression)SqlDuplicator.Copy(expr), mm));
            }
            return exprs;
        }
    }
}
