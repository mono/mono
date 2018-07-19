using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq.SqlClient {

	/// <summary>
	/// </summary>
	internal class SqlOuterApplyReducer { 
		internal static SqlNode Reduce(SqlNode node, SqlFactory factory, SqlNodeAnnotations annotations) {
			Visitor r = new Visitor(factory, annotations);
			return r.Visit(node);			
		}

		class Visitor : SqlVisitor {
            SqlFactory factory;
			SqlNodeAnnotations annotations;

            internal Visitor(SqlFactory factory, SqlNodeAnnotations annotations) {
                this.factory = factory;
                this.annotations = annotations;
            }

            internal override SqlSource VisitSource(SqlSource source) {
                source = base.VisitSource(source);

                SqlJoin join = source as SqlJoin;
                if (join != null) {
                    if (join.JoinType == SqlJoinType.OuterApply) {
                        // Reduce outer-apply into left-outer-join
                        HashSet<SqlAlias> leftProducedAliases = SqlGatherProducedAliases.Gather(join.Left);
                        HashSet<SqlExpression> liftedExpressions = new HashSet<SqlExpression>();

                        if (SqlPredicateLifter.CanLift(join.Right, leftProducedAliases, liftedExpressions) &&
                            SqlSelectionLifter.CanLift(join.Right, leftProducedAliases, liftedExpressions) &&
                            !SqlAliasDependencyChecker.IsDependent(join.Right, leftProducedAliases, liftedExpressions) ) {

                            SqlExpression liftedPredicate = SqlPredicateLifter.Lift(join.Right, leftProducedAliases);
                            List<List<SqlColumn>> liftedSelections = SqlSelectionLifter.Lift(join.Right, leftProducedAliases, liftedExpressions);

                            join.JoinType = SqlJoinType.LeftOuter;
                            join.Condition = liftedPredicate;

                            if (liftedSelections != null) {
                                foreach(List<SqlColumn> selection in liftedSelections) {
                                    source = this.PushSourceDown(source, selection);
                                }
                            }
                        }
                        else {
                            this.AnnotateSqlIncompatibility(join, SqlProvider.ProviderMode.Sql2000);
                        }
                    }
                    else if (join.JoinType == SqlJoinType.CrossApply) {
                        // reduce cross apply with special nested left-outer-join's into a single left-outer-join
                        //
                        // SELECT x.*, y.*
                        // FROM X
                        // CROSS APPLY (
                        //      SELECT y.*
                        //       FROM (
                        //          SELECT ?
                        //       ) 
                        //       LEFT OUTER JOIN (
                        //          SELECT y.* FROM Y
                        //       ) AS y
                        //
                        // ==>
                        // 
                        // SELECT x.*, y.*
                        // FROM X
                        // LEFT OUTER JOIN (
                        //     SELECT y.* FROM Y
                        // )

                        SqlJoin leftOuter = this.GetLeftOuterWithUnreferencedSingletonOnLeft(join.Right);
                        if (leftOuter != null) {
                            HashSet<SqlAlias> leftProducedAliases = SqlGatherProducedAliases.Gather(join.Left);
                            HashSet<SqlExpression> liftedExpressions = new HashSet<SqlExpression>();

                            if (SqlPredicateLifter.CanLift(leftOuter.Right, leftProducedAliases, liftedExpressions) &&
                                SqlSelectionLifter.CanLift(leftOuter.Right, leftProducedAliases, liftedExpressions) &&
                                !SqlAliasDependencyChecker.IsDependent(leftOuter.Right, leftProducedAliases, liftedExpressions)
                                ) {

                                SqlExpression liftedPredicate = SqlPredicateLifter.Lift(leftOuter.Right, leftProducedAliases);
                                List<List<SqlColumn>> liftedSelections = SqlSelectionLifter.Lift(leftOuter.Right, leftProducedAliases, liftedExpressions);

                                // add intermediate selections 
                                this.GetSelectionsBeforeJoin(join.Right, liftedSelections);

                                // push down all selections
                                foreach(List<SqlColumn> selection in liftedSelections.Where(s => s.Count > 0)) {
                                    source = this.PushSourceDown(source, selection);
                                }

                                join.JoinType = SqlJoinType.LeftOuter;
                                join.Condition = this.factory.AndAccumulate(leftOuter.Condition, liftedPredicate);
                                join.Right = leftOuter.Right;
                            }
                            else {
                                this.AnnotateSqlIncompatibility(join, SqlProvider.ProviderMode.Sql2000);
                            }
                        }
                    }

                    // re-balance join tree of left-outer-joins to expose LOJ w/ leftside unreferenced
                    while (join.JoinType == SqlJoinType.LeftOuter) {
                        // look for buried left-outer-joined-with-unreferenced singleton
                        SqlJoin leftLeftOuter = this.GetLeftOuterWithUnreferencedSingletonOnLeft(join.Left);
                        if (leftLeftOuter == null)
                            break;

                        List<List<SqlColumn>> liftedSelections = new List<List<SqlColumn>>();

                        // add intermediate selections 
                        this.GetSelectionsBeforeJoin(join.Left, liftedSelections);

                        // push down all selections
                        foreach(List<SqlColumn> selection in liftedSelections) {
                            source = this.PushSourceDown(source, selection);
                        }

                        // bubble this one up on-top of this 'join'.
                        SqlSource jRight = join.Right;
                        SqlExpression jCondition = join.Condition;

                        join.Left = leftLeftOuter.Left;
                        join.Right = leftLeftOuter;
                        join.Condition = leftLeftOuter.Condition;

                        leftLeftOuter.Left = leftLeftOuter.Right;
                        leftLeftOuter.Right = jRight;
                        leftLeftOuter.Condition = jCondition;
                    }
                }

                return source;
            }

            private void AnnotateSqlIncompatibility(SqlNode node, params SqlProvider.ProviderMode[] providers) {
                this.annotations.Add(node, new SqlServerCompatibilityAnnotation(Strings.SourceExpressionAnnotation(node.SourceExpression), providers));
            }

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
            private SqlSource PushSourceDown(SqlSource sqlSource, List<SqlColumn> cols) {
                SqlSelect ns = new SqlSelect(new SqlNop(cols[0].ClrType, cols[0].SqlType, sqlSource.SourceExpression), sqlSource, sqlSource.SourceExpression);
                ns.Row.Columns.AddRange(cols);
                return new SqlAlias(ns);
            }

            private SqlJoin GetLeftOuterWithUnreferencedSingletonOnLeft(SqlSource source) {
                SqlAlias alias = source as SqlAlias;
                if (alias != null) {
                    SqlSelect select = alias.Node as SqlSelect;
                    if (select != null &&
                        select.Where == null &&
                        select.Top == null &&
                        select.GroupBy.Count == 0 &&
                        select.OrderBy.Count == 0) {
                        return this.GetLeftOuterWithUnreferencedSingletonOnLeft(select.From);
                    }
                }
                SqlJoin join = source as SqlJoin;
                if (join == null || join.JoinType != SqlJoinType.LeftOuter)
                    return null;
                if (!this.IsSingletonSelect(join.Left))
                    return null;
                HashSet<SqlAlias> p = SqlGatherProducedAliases.Gather(join.Left);
				HashSet<SqlAlias> c = SqlGatherConsumedAliases.Gather(join.Right);
                if (p.Overlaps(c)) {
                    return null;
                }
                return join;
            }

            private void GetSelectionsBeforeJoin(SqlSource source, List<List<SqlColumn>> selections) {
                SqlJoin join = source as SqlJoin;
                if (join != null)
                    return;
                SqlAlias alias = source as SqlAlias;
                if (alias != null) {
                    SqlSelect select = alias.Node as SqlSelect;
                    if (select != null) {
                        this.GetSelectionsBeforeJoin(select.From, selections);
                        selections.Add(select.Row.Columns);
                    }
                }
            }

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
            private bool IsSingletonSelect(SqlSource source) {
                SqlAlias alias = source as SqlAlias;
                if (alias == null)
                    return false;
                SqlSelect select = alias.Node as SqlSelect;
                if (select == null)
                    return false;
                if (select.From != null)
                    return false;
                return true;
            }
		}

        class SqlGatherReferencedColumns {
            private SqlGatherReferencedColumns() { }
            internal static HashSet<SqlColumn> Gather(SqlNode node, HashSet<SqlColumn> columns) {
                Visitor v = new Visitor(columns);
                v.Visit(node);
                return columns;
            }
            class Visitor : SqlVisitor {
                HashSet<SqlColumn> columns;
                internal Visitor(HashSet<SqlColumn> columns) {
                    this.columns = columns;
                }
                internal override SqlExpression VisitColumnRef(SqlColumnRef cref) {
                    if (!this.columns.Contains(cref.Column)) {
                        this.columns.Add(cref.Column);
                        if (cref.Column.Expression != null) {
                            this.Visit(cref.Column.Expression);
                        }
                    }
                    return cref;
                }
            }
        }

        class SqlAliasesReferenced {
            HashSet<SqlAlias> aliases;
            bool referencesAny;
            Visitor visitor;

            internal SqlAliasesReferenced(HashSet<SqlAlias> aliases) {
                this.aliases = aliases;
                this.visitor = new Visitor(this);
            }

            internal bool ReferencesAny(SqlExpression expression) {
                this.referencesAny = false;
                this.visitor.Visit(expression);
                return this.referencesAny;
            }

            class Visitor: SqlVisitor {
                SqlAliasesReferenced parent;

                internal Visitor(SqlAliasesReferenced parent) {
                    this.parent = parent;
                }
                internal override SqlExpression VisitColumnRef(SqlColumnRef cref) {
                    if (this.parent.aliases.Contains(cref.Column.Alias)) {
                        this.parent.referencesAny = true;
                    }
                    else if (cref.Column.Expression != null) {
                        this.Visit(cref.Column.Expression);
                    }
                    return cref;
                }

                internal override SqlExpression VisitColumn(SqlColumn col) {
                    if (col.Expression != null) {
                        this.Visit(col.Expression);
                    }
                    return col;
                }
            }
        }

        static class SqlAliasDependencyChecker {
            internal static bool IsDependent(SqlNode node, HashSet<SqlAlias> aliasesToCheck, HashSet<SqlExpression> ignoreExpressions) {
                Visitor v = new Visitor(aliasesToCheck, ignoreExpressions);
                v.Visit(node);
                return v.hasDependency;
            }
            class Visitor : SqlVisitor {
                HashSet<SqlAlias> aliasesToCheck;
                HashSet<SqlExpression> ignoreExpressions;
                internal bool hasDependency;

                internal Visitor(HashSet<SqlAlias> aliasesToCheck, HashSet<SqlExpression> ignoreExpressions) {
                    this.aliasesToCheck = aliasesToCheck;
                    this.ignoreExpressions = ignoreExpressions;
                }
                internal override SqlNode Visit(SqlNode node) {
                    SqlExpression e = node as SqlExpression;
                    if (this.hasDependency)
                        return node;
                    if (e != null && this.ignoreExpressions.Contains(e)) {
                        return node;
                    }
                    return base.Visit(node);
                }
                internal override SqlExpression VisitColumnRef(SqlColumnRef cref) {
                    if (this.aliasesToCheck.Contains(cref.Column.Alias)) {
                        this.hasDependency = true;
                    }
                    else if (cref.Column.Expression != null) {
                        this.Visit(cref.Column.Expression);
                    }
                    return cref;
                }
                internal override SqlExpression VisitColumn(SqlColumn col) {
                    if (col.Expression != null) {
                        this.Visit(col.Expression);
                    }
                    return col;
                }
            }
        }

        static class SqlPredicateLifter {
            internal static bool CanLift(SqlSource source, HashSet<SqlAlias> aliasesForLifting, HashSet<SqlExpression> liftedExpressions) {
                System.Diagnostics.Debug.Assert(source != null);
                System.Diagnostics.Debug.Assert(aliasesForLifting != null);
                Visitor v = new Visitor(false, aliasesForLifting, liftedExpressions);
                v.VisitSource(source);
                return v.canLiftAll;
            }

            internal static SqlExpression Lift(SqlSource source, HashSet<SqlAlias> aliasesForLifting) {
                System.Diagnostics.Debug.Assert(source != null);
                System.Diagnostics.Debug.Assert(aliasesForLifting != null);
                Visitor v = new Visitor(true, aliasesForLifting, null);
                v.VisitSource(source);
                return v.lifted;
            }

            class Visitor : SqlVisitor {
                SqlAliasesReferenced aliases;
                HashSet<SqlExpression> liftedExpressions;
                bool doLifting;
                internal bool canLiftAll;
                internal SqlExpression lifted;
                SqlAggregateChecker aggregateChecker;


                internal Visitor(bool doLifting, HashSet<SqlAlias> aliasesForLifting, HashSet<SqlExpression> liftedExpressions) {
                    this.doLifting = doLifting;
                    this.aliases = new SqlAliasesReferenced(aliasesForLifting);
                    this.liftedExpressions = liftedExpressions;
                    this.canLiftAll = true;
                    this.aggregateChecker = new SqlAggregateChecker();
                }

                internal override SqlSelect VisitSelect(SqlSelect select) {
                    // check subqueries first
                    this.VisitSource(select.From);

                    // don't allow lifting through these operations
                    if (select.Top != null ||
                        select.GroupBy.Count > 0 ||
                        this.aggregateChecker.HasAggregates(select) ||
                        select.IsDistinct) {
                        this.canLiftAll = false;
                    }

                    // only lift predicates that actually reference the aliases
                    if (this.canLiftAll && select.Where != null) {
                        bool referencesAliases = this.aliases.ReferencesAny(select.Where);
                        if (referencesAliases) {
                            if (this.liftedExpressions != null) {
                                this.liftedExpressions.Add(select.Where);
                            }
                            if (this.doLifting) {
                                if (this.lifted != null)
                                    this.lifted = new SqlBinary(SqlNodeType.And, this.lifted.ClrType, this.lifted.SqlType, this.lifted, select.Where);
                                else
                                    this.lifted = select.Where;
                                select.Where = null;
                            }
                        }
                    }

                    return select;
                }
            }
        }

        static class SqlSelectionLifter {
            internal static bool CanLift(SqlSource source, HashSet<SqlAlias> aliasesForLifting, HashSet<SqlExpression> liftedExpressions) {
                Visitor v = new Visitor(false, aliasesForLifting, liftedExpressions);
                v.VisitSource(source);
                return v.canLiftAll;
            }

            internal static List<List<SqlColumn>> Lift(SqlSource source, HashSet<SqlAlias> aliasesForLifting, HashSet<SqlExpression> liftedExpressions) {
                Visitor v = new Visitor(true, aliasesForLifting, liftedExpressions);
                v.VisitSource(source);
                return v.lifted;
            }

            class Visitor : SqlVisitor {
                SqlAliasesReferenced aliases;
                HashSet<SqlColumn> referencedColumns;
                HashSet<SqlExpression> liftedExpressions;
                internal List<List<SqlColumn>> lifted;
                internal bool canLiftAll;
                bool hasLifted;
                bool doLifting;
                SqlAggregateChecker aggregateChecker;

                internal Visitor(bool doLifting, HashSet<SqlAlias> aliasesForLifting, HashSet<SqlExpression> liftedExpressions) {
                    this.doLifting = doLifting;
                    this.aliases = new SqlAliasesReferenced(aliasesForLifting);
                    this.referencedColumns = new HashSet<SqlColumn>();
                    this.liftedExpressions = liftedExpressions;
                    this.canLiftAll = true;
                    if (doLifting)
                        this.lifted = new List<List<SqlColumn>>();
                    this.aggregateChecker = new SqlAggregateChecker();
                }

                internal override SqlSource VisitJoin(SqlJoin join) {
                    this.ReferenceColumns(join.Condition);
                    return base.VisitJoin(join);
                }

                internal override SqlSelect VisitSelect(SqlSelect select) {
                    // reference all columns
                    this.ReferenceColumns(select.Where);
                    foreach(SqlOrderExpression oe in select.OrderBy) {
                        // 
                        this.ReferenceColumns(oe.Expression);
                    }
                    foreach(SqlExpression e in select.GroupBy) {
                        // 
                        this.ReferenceColumns(e);
                    }
                    this.ReferenceColumns(select.Having);

                    // determine what if anything should be lifted from this select
                    List<SqlColumn> lift = null;
                    List<SqlColumn> keep = null;
                    foreach (SqlColumn sc in select.Row.Columns) {
                        bool referencesAliasesForLifting = this.aliases.ReferencesAny(sc.Expression);
                        bool isLockedExpression = this.referencedColumns.Contains(sc);
                        if (referencesAliasesForLifting) {
                            // 
                            if (isLockedExpression) {
                                this.canLiftAll = false;
                                this.ReferenceColumns(sc);
                            }
                            else {
                                this.hasLifted = true;
                                if (this.doLifting) {
                                    if (lift == null)
                                        lift = new List<SqlColumn>();
                                    lift.Add(sc);
                                }
                            }
                        }
                        else {
                            if (this.doLifting) {
                                if (keep == null)
                                    keep = new List<SqlColumn>();
                                keep.Add(sc);
                            }
                            this.ReferenceColumns(sc);
                        }
                    }

                    // check subqueries too
                    if (this.canLiftAll) {
                        this.VisitSource(select.From);
                    }

                    // don't allow lifting through these operations
                    if (select.Top != null ||
                        select.GroupBy.Count > 0 ||
                        this.aggregateChecker.HasAggregates(select) ||
                        select.IsDistinct) {
                        if (this.hasLifted) {
                            // 
                            this.canLiftAll = false;
                        }
                    }

                    // do the actual lifting for this select
                    if (this.doLifting && this.canLiftAll) {
                        select.Row.Columns.Clear();
                        if (keep != null)
                            select.Row.Columns.AddRange(keep);
                        if (lift != null) {
                            // 
                            this.lifted.Add(lift);
                        }
                    }

                    return select;
                }

                private void ReferenceColumns(SqlExpression expression) {
                    if (expression != null) {
                        if (this.liftedExpressions == null || !this.liftedExpressions.Contains(expression)) {
                            SqlGatherReferencedColumns.Gather(expression, this.referencedColumns);
                        }
                    }
                }
            }
        }
	}
}
