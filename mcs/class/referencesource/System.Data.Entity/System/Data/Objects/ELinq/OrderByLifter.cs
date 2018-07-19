//---------------------------------------------------------------------
// <copyright file="OrderByLifter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
//---------------------------------------------------------------------

namespace System.Data.Objects.ELinq
{
    using System.Data.Common.CommandTrees;
    using System.Data.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Common.Utils;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;

    internal sealed partial class ExpressionConverter
    {
        /// <summary>
        /// A context-sensitive DbExpression builder class that simulates order preservation
        /// for operators (project, filter, oftype, skip and limit) that are not natively order
        /// preserving. The builder simulates order preservation by 'lifting' order keys in 
        /// the expression tree. For instance, source.Sort(o).Where(f) is rewritten as
        /// source.Where(f).Sort(o) since otherwise the sort keys would be ignored.
        /// 
        /// In general, the lifter works as follows:
        /// 
        /// - The input to the operator is matched against a series of patterns for intrinsically
        ///   ordered expressions.
        /// - For each pattern, the lifter encodes the compensation required for each of the
        ///   lifting operators that can be applied.
        /// </summary>
        private sealed class OrderByLifter
        {
            private readonly AliasGenerator _aliasGenerator;

            internal OrderByLifter(AliasGenerator aliasGenerator)
            {
                _aliasGenerator = aliasGenerator;
            }

            #region 'Public' builder methods.
            internal DbExpression Project(DbExpressionBinding input, DbExpression projection)
            {
                OrderByLifterBase lifter = GetLifter(input.Expression);
                return lifter.Project(input.Project(projection));
            }

            internal DbExpression Filter(DbExpressionBinding input, DbExpression predicate)
            {
                OrderByLifterBase lifter = GetLifter(input.Expression);
                return lifter.Filter(input.Filter(predicate));
            }

            internal DbExpression OfType(DbExpression argument, TypeUsage type)
            {
                OrderByLifterBase lifter = GetLifter(argument);
                return lifter.OfType(type);
            }

            internal DbExpression Skip(DbExpressionBinding input, DbExpression skipCount)
            {
                OrderByLifterBase lifter = GetLifter(input.Expression);
                return lifter.Skip(skipCount);
            }

            internal DbExpression Limit(DbExpression argument, DbExpression limit)
            {
                OrderByLifterBase lifter = GetLifter(argument);
                return lifter.Limit(limit);
            }
            #endregion

            private OrderByLifterBase GetLifter(DbExpression root)
            {
                return OrderByLifterBase.GetLifter(root, _aliasGenerator);
            }

            private abstract class OrderByLifterBase
            {
                protected readonly DbExpression _root;
                protected readonly AliasGenerator _aliasGenerator;

                protected OrderByLifterBase(DbExpression root, AliasGenerator aliasGenerator)
                {
                    _root = root;
                    _aliasGenerator = aliasGenerator;
                }

                /// <summary>
                /// Returns a lifter instance which supports lifting the intrinsic order of the given
                /// source expression across specific operations (filter, project, oftype, skip, and limit)
                /// </summary>
                /// <remarks>
                /// Lifting only occurs for expressions that are ordered. Each of the nested
                /// OrderByLifterBase class implementations represents one or two of the ordered patterns with
                /// the exception of the PassthroughOrderByLifter. The latter class represents expressions
                /// without intrinsic order that therefore require no lifting.
                /// </remarks>
                internal static OrderByLifterBase GetLifter(DbExpression source, AliasGenerator aliasGenerator)
                {
                    if (source.ExpressionKind == DbExpressionKind.Sort)
                    {
                        return new SortLifter((DbSortExpression)source, aliasGenerator);
                    }
                    if (source.ExpressionKind == DbExpressionKind.Project)
                    {
                        var project = (DbProjectExpression)source;
                        DbExpression projectInput = project.Input.Expression;
                        if (projectInput.ExpressionKind == DbExpressionKind.Sort)
                        {
                            return new ProjectSortLifter(project, (DbSortExpression)projectInput, aliasGenerator);
                        }
                        if (projectInput.ExpressionKind == DbExpressionKind.Skip)
                        {
                            return new ProjectSkipLifter(project, (DbSkipExpression)projectInput, aliasGenerator);
                        }
                        if (projectInput.ExpressionKind == DbExpressionKind.Limit)
                        {
                            var limit = (DbLimitExpression)projectInput;
                            DbExpression limitInput = limit.Argument;
                            if (limitInput.ExpressionKind == DbExpressionKind.Sort)
                            {
                                return new ProjectLimitSortLifter(project, limit, (DbSortExpression)limitInput, aliasGenerator);
                            }
                            if (limitInput.ExpressionKind == DbExpressionKind.Skip)
                            {
                                return new ProjectLimitSkipLifter(project, limit, (DbSkipExpression)limitInput, aliasGenerator);
                            }
                        }
                    }
                    if (source.ExpressionKind == DbExpressionKind.Skip)
                    {
                        return new SkipLifter((DbSkipExpression)source, aliasGenerator);
                    }
                    if (source.ExpressionKind == DbExpressionKind.Limit)
                    {
                        var limit = (DbLimitExpression)source;
                        DbExpression limitInput = limit.Argument;
                        if (limitInput.ExpressionKind == DbExpressionKind.Sort)
                        {
                            return new LimitSortLifter(limit, (DbSortExpression)limitInput, aliasGenerator);
                        }
                        if (limitInput.ExpressionKind == DbExpressionKind.Skip)
                        {
                            return new LimitSkipLifter(limit, (DbSkipExpression)limitInput, aliasGenerator);
                        }
                        if (limitInput.ExpressionKind == DbExpressionKind.Project)
                        {
                            var project = (DbProjectExpression)limitInput;
                            DbExpression projectInput = project.Input.Expression;
                            if (projectInput.ExpressionKind == DbExpressionKind.Sort)
                            {
                                // source.Sort(o).Project(p).Limit(k).* is equivalent to transformation for 
                                // source.Sort(o).Limit(k).Project(p).* 
                                return new ProjectLimitSortLifter(project, limit, (DbSortExpression)projectInput, aliasGenerator);
                            }
                            if (projectInput.ExpressionKind == DbExpressionKind.Skip)
                            {
                                // source.Skip(k, o).Project(p).Limit(k2).* is equivalent to transformation for 
                                // source.Skip(k, o).Limit(k2).Project(p).*
                                return new ProjectLimitSkipLifter(project, limit, (DbSkipExpression)projectInput, aliasGenerator);
                            }
                        }
                    }
                    return new PassthroughOrderByLifter(source, aliasGenerator);
                }

                #region Builder methods
                internal abstract DbExpression Project(DbProjectExpression project);
                internal abstract DbExpression Filter(DbFilterExpression filter);
                internal virtual DbExpression OfType(TypeUsage type)
                {
                    // s.OfType<T> is normally translated to s.Filter(e => e is T).Project(e => e as T)
                    DbExpressionBinding rootBinding = _root.BindAs(_aliasGenerator.Next());
                    DbExpression filter = this.Filter(rootBinding.Filter(rootBinding.Variable.IsOf(type)));
                    OrderByLifterBase filterLifter = GetLifter(filter, _aliasGenerator);
                    DbExpressionBinding filterBinding = filter.BindAs(_aliasGenerator.Next());
                    DbExpression project = filterLifter.Project(filterBinding.Project(filterBinding.Variable.TreatAs(type)));
                    return project;
                }
                internal abstract DbExpression Limit(DbExpression k);
                internal abstract DbExpression Skip(DbExpression k);
                #endregion

                #region Lambda composition: merge arguments to operators to create a single operator
                protected DbProjectExpression ComposeProject(DbExpression input, DbProjectExpression first, DbProjectExpression second)
                {
                    // source.Project(first).Project(second) -> source.Project(e => second(first(e)))

                    // create lambda expression representing the second projection (e => second(e))
                    DbLambda secondLambda = DbExpressionBuilder.Lambda(second.Projection, second.Input.Variable);

                    // invoke lambda with variable from the first projection
                    DbProjectExpression composed = first.Input.Project(secondLambda.Invoke(first.Projection));

                    return RebindProject(input, composed);
                }

                protected DbFilterExpression ComposeFilter(DbExpression input, DbProjectExpression first, DbFilterExpression second)
                {
                    // source.Project(first).Filter(second) -> source.Filter(e => second(first(e)))

                    // create lambda expression representing the filter (e => second(e))
                    DbLambda secondLambda = DbExpressionBuilder.Lambda(second.Predicate, second.Input.Variable);

                    // invoke lambda with variable from the project
                    DbFilterExpression composed = first.Input.Filter(secondLambda.Invoke(first.Projection));

                    return RebindFilter(input, composed);
                }
                #endregion

                #region Paging op reducers
                protected DbSkipExpression AddToSkip(DbExpression input, DbSkipExpression skip, DbExpression plusK)
                {
                    // source.Skip(k, o).Skip(k2) -> source.Skip(k + k2, o)
                    DbExpression newCount = CombineIntegers(skip.Count, plusK,
                        (l, r) => l + r);
                    return RebindSkip(input, skip, newCount);
                }

                protected DbLimitExpression SubtractFromLimit(DbExpression input, DbLimitExpression limit, DbExpression minusK)
                {
                    DbExpression newCount = CombineIntegers(limit.Limit, minusK,
                        (l, r) => r > l ? 0 : l - r); // can't limit to less than zero rows)
                    return DbExpressionBuilder.Limit(input, newCount);
                }

                protected DbLimitExpression MinimumLimit(DbExpression input, DbLimitExpression limit, DbExpression k)
                {
                    // source.Limit(k).Limit(k2) -> source.Limit(Min(k, k2))
                    DbExpression newCount = CombineIntegers(limit.Limit, k, Math.Min);
                    return DbExpressionBuilder.Limit(input, newCount);
                }

                protected DbExpression CombineIntegers(DbExpression left, DbExpression right,
                    Func<int, int, int> combineConstants)
                {
                    if (left.ExpressionKind == DbExpressionKind.Constant &&
                        right.ExpressionKind == DbExpressionKind.Constant)
                    {
                        object leftValue = ((DbConstantExpression)left).Value;
                        object rightValue = ((DbConstantExpression)right).Value;
                        if (leftValue is int && rightValue is int)
                        {
                            return left.ResultType.Constant(combineConstants((int)leftValue, (int)rightValue));
                        }
                    }
                    Debug.Fail("only valid for integer constants");
                    throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.UnexpectedLinqLambdaExpressionFormat);
                }
                #endregion

                #region Rebinders: take an operator and apply it to a different input
                protected DbProjectExpression RebindProject(DbExpression input, DbProjectExpression project)
                {
                    DbExpressionBinding inputBinding = input.BindAs(project.Input.VariableName);
                    return inputBinding.Project(project.Projection);
                }

                protected DbFilterExpression RebindFilter(DbExpression input, DbFilterExpression filter)
                {
                    DbExpressionBinding inputBinding = input.BindAs(filter.Input.VariableName);
                    return inputBinding.Filter(filter.Predicate);
                }

                protected DbSortExpression RebindSort(DbExpression input, DbSortExpression sort)
                {
                    DbExpressionBinding inputBinding = input.BindAs(sort.Input.VariableName);
                    return inputBinding.Sort(sort.SortOrder);
                }

                protected DbSortExpression ApplySkipOrderToSort(DbExpression input, DbSkipExpression sortSpec)
                {
                    DbExpressionBinding inputBinding = input.BindAs(sortSpec.Input.VariableName);
                    return inputBinding.Sort(sortSpec.SortOrder);
                }

                protected DbSkipExpression ApplySortOrderToSkip(DbExpression input, DbSortExpression sort, DbExpression k)
                {
                    DbExpressionBinding inputBinding = input.BindAs(sort.Input.VariableName);
                    return inputBinding.Skip(sort.SortOrder, k);
                }

                protected DbSkipExpression RebindSkip(DbExpression input, DbSkipExpression skip, DbExpression k)
                {
                    DbExpressionBinding inputBinding = input.BindAs(skip.Input.VariableName);
                    return inputBinding.Skip(skip.SortOrder, k);
                }
                #endregion
            }

            /// <summary>
            /// Represents an expression of the form: source.Skip(k, o).Limit(k2)
            /// </summary>
            private class LimitSkipLifter : OrderByLifterBase
            {
                private readonly DbLimitExpression _limit;
                private readonly DbSkipExpression _skip;
                private readonly DbExpression _source;

                internal LimitSkipLifter(DbLimitExpression limit, DbSkipExpression skip, AliasGenerator aliasGenerator)
                    : base(limit, aliasGenerator)
                {
                    _limit = limit;
                    _skip = skip;
                    _source = skip.Input.Expression;
                }

                internal override DbExpression Filter(DbFilterExpression filter)
                {
                    // source.Skip(k, o).Limit(k2).Filter(f) ->
                    // source.Skip(k, o).Limit(k2).Filter(f).Sort(o)
                    return ApplySkipOrderToSort(filter, _skip);
                }

                internal override DbExpression Project(DbProjectExpression project)
                {
                    // the result is already ordered (no compensation is required)
                    return project;
                }

                internal override DbExpression Limit(DbExpression k)
                {
                    // source.Skip(k, o).Limit(k2).Limit(k3) ->
                    // source.Skip(k, o).Limit(Min(k2, k3)) where k2 and k3 are constants
                    // otherwise source.Skip(k, o).Limit(k2).Sort(o).Limit(k3)
                    if (_limit.Limit.ExpressionKind == DbExpressionKind.Constant &&
                        k.ExpressionKind == DbExpressionKind.Constant)
                    {
                        return MinimumLimit(_skip, _limit, k);
                    }
                    else
                    {
                        return ApplySkipOrderToSort(_limit, _skip).Limit(k);
                    }
                }

                internal override DbExpression Skip(DbExpression k)
                {
                    // source.Skip(k, o).Limit(k2).Skip(k3) ->
                    // source.Skip(k, o).Limit(k2).Skip(k3, o)
                    return RebindSkip(_limit, _skip, k);
                }
            }

            /// <summary>
            /// Represents an expression of the form: source.Sort(o).Limit(k)
            /// </summary>
            private class LimitSortLifter : OrderByLifterBase
            {
                private readonly DbLimitExpression _limit;
                private readonly DbSortExpression _sort;
                private readonly DbExpression _source;

                internal LimitSortLifter(DbLimitExpression limit, DbSortExpression sort, AliasGenerator aliasGenerator)
                    : base(limit, aliasGenerator)
                {
                    _limit = limit;
                    _sort = sort;
                    _source = sort.Input.Expression;
                }

                internal override DbExpression Filter(DbFilterExpression filter)
                {
                    // source.Sort(o).Limit(k).Filter(f) -> source.Sort(o).Limit(k).Filter(f).Sort(o)
                    return RebindSort(filter, _sort);
                }

                internal override DbExpression Project(DbProjectExpression project)
                {
                    // the result is already ordered (no compensation is required)
                    return project;
                }

                internal override DbExpression Limit(DbExpression k)
                {
                    // source.Sort(o).Limit(k).Limit(k2) -> source.Sort(o).Limit(Min(k, k2)) when k and k2 are constants
                    // otherwise -> source.Sort(o).Limit(k).Sort(o).Limit(k2)
                    if (_limit.Limit.ExpressionKind == DbExpressionKind.Constant &&
                        k.ExpressionKind == DbExpressionKind.Constant)
                    {
                        return MinimumLimit(_sort, _limit, k);
                    }
                    else
                    {
                        return RebindSort(_limit, _sort).Limit(k);
                    }
                }

                internal override DbExpression Skip(DbExpression k)
                {
                    // source.Sort(o).Limit(k).Skip(k2) -> source.Sort(o).Limit(k).Skip(k2, o)
                    return ApplySortOrderToSkip(_limit, _sort, k);
                }
            }

            /// <summary>
            /// Represents an expression of the form: source.Skip(k, o).Limit(k2).Project(p)
            /// </summary>
            /// <remarks>
            /// This class is also used to represent expressions of the form: source.Skip(k, o).Project(p).Limit(k).
            /// As a result, the rewrites must be spelled out entirely (the implementation cannot assume that 
            /// _limit exists in a particular position in the tree)
            /// </remarks>
            private class ProjectLimitSkipLifter : OrderByLifterBase
            {
                private readonly DbProjectExpression _project;
                private readonly DbLimitExpression _limit;
                private readonly DbSkipExpression _skip;
                private readonly DbExpression _source;

                internal ProjectLimitSkipLifter(DbProjectExpression project, DbLimitExpression limit, DbSkipExpression skip, AliasGenerator aliasGenerator)
                    : base(project, aliasGenerator)
                {
                    _project = project;
                    _limit = limit;
                    _skip = skip;
                    _source = skip.Input.Expression;
                }

                internal override DbExpression Filter(DbFilterExpression filter)
                {
                    // source.Skip(k, o).Limit(k2).Project(p).Filter(f) ->
                    // source.Skip(k, o).Limit(k2).Filter(e => f(p(e))).Sort(o).Project(p)
                    return RebindProject(
                        ApplySkipOrderToSort(
                            ComposeFilter(
                                DbExpressionBuilder.Limit(_skip, _limit.Limit),
                                _project,
                                filter),
                            _skip),
                        _project);
                }

                internal override DbExpression Project(DbProjectExpression project)
                {
                    // source.Skip(k, o).Limit(k2).Project(p).Project(p2) -> 
                    // source.Skip(k, o).Limit(k2).Project(e => p2(p(e)))
                    return ComposeProject(
                        DbExpressionBuilder.Limit(_skip, _limit.Limit),
                        _project,
                        project);
                }

                internal override DbExpression Limit(DbExpression k)
                {
                    // source.Skip(k, o).Limit(k2).Project(p).Limit(k3) ->
                    // source.Skip(k, o).Limit(Min(k2, k3)).Project(p) where k2 and k2 are constants
                    // otherwise -> source.Skip(k, o).Limit(k2).Sort(o).Limit(k3).Project(p)
                    if (_limit.Limit.ExpressionKind == DbExpressionKind.Constant &&
                        k.ExpressionKind == DbExpressionKind.Constant)
                    {
                        return RebindProject(
                            MinimumLimit(_skip, _limit, k),
                            _project);
                    }
                    else
                    {
                        return RebindProject(
                            DbExpressionBuilder.Limit(
                                ApplySkipOrderToSort(
                                    DbExpressionBuilder.Limit(_skip, _limit.Limit),
                                    _skip),
                                k),
                            _project);
                    }
                }

                internal override DbExpression Skip(DbExpression k)
                {
                    // source.Skip(k, o).Limit(k2).Project(p).Skip(k3) ->
                    // source.Skip(k + k3, o).Limit(k2 – k3).Project(p) when k, k2 and k3 are constants
                    // otherwise -> source.Skip(k, o).Limit(k2).Skip(k3, o).Project(p)
                    if (_skip.Count.ExpressionKind == DbExpressionKind.Constant &&
                        _limit.Limit.ExpressionKind == DbExpressionKind.Constant &&
                        k.ExpressionKind == DbExpressionKind.Constant)
                    {
                        return RebindProject(
                            SubtractFromLimit(
                                AddToSkip(_source, _skip, k),
                                _limit,
                                k),
                            _project);
                    }
                    else
                    {
                        return RebindProject(
                            RebindSkip(
                                DbExpressionBuilder.Limit(_skip, _limit.Limit),
                                _skip,
                                k),
                            _project);
                    }
                }
            }

            /// <summary>
            /// Represents an expression of the form: source.Sort(o).Limit(k).Project(p)
            /// </summary>
            /// <remarks>
            /// This class is also used to represent expressions of the form: source.Sort(o).Project(p).Limit(k).
            /// As a result, the rewrites must be spelled out entirely (the implementation cannot assume that 
            /// _limit exists in a particular position in the tree)
            /// </remarks>
            private class ProjectLimitSortLifter : OrderByLifterBase
            {
                private readonly DbProjectExpression _project;
                private readonly DbLimitExpression _limit;
                private readonly DbSortExpression _sort;
                private readonly DbExpression _source;

                internal ProjectLimitSortLifter(DbProjectExpression project, DbLimitExpression limit, DbSortExpression sort, AliasGenerator aliasGenerator)
                    : base(project, aliasGenerator)
                {
                    _project = project;
                    _limit = limit;
                    _sort = sort;
                    _source = sort.Input.Expression;
                }

                internal override DbExpression Filter(DbFilterExpression filter)
                {
                    // source.Sort(o).Limit(k).Project(p).Filter(f) -> source.Sort(o).Limit(k).Filter(e => f(p(e))).Sort(o).Project(p)
                    return RebindProject(
                        RebindSort(
                            ComposeFilter(
                                DbExpressionBuilder.Limit(_sort, _limit.Limit),
                                _project,
                                filter),
                            _sort),
                        _project);
                }

                internal override DbExpression Project(DbProjectExpression project)
                {
                    // source.Sort(o).Limit(k).Project(p).Project(p2) -> source.Sort(o).Limit(k).Project(e => p2(p(e)))
                    return ComposeProject(
                        DbExpressionBuilder.Limit(_sort, _limit.Limit),
                        _project,
                        project);
                }

                internal override DbExpression Limit(DbExpression k)
                {
                    // source.Sort(o).Limit(k).Project(p).Limit(k2) -> source.Sort(o).Limit(Min(k, k2)).Project(p) where k and k2 are constants
                    // otherwise -> source.Sort(o).Limit(k).Sort(o).Limit(k2).Project(p) 
                    if (_limit.Limit.ExpressionKind == DbExpressionKind.Constant &&
                        k.ExpressionKind == DbExpressionKind.Constant)
                    {
                        return RebindProject(
                            MinimumLimit(_sort, _limit, k),
                            _project);
                    }
                    else
                    {
                        return RebindProject(
                            DbExpressionBuilder.Limit(
                                RebindSort(
                                    DbExpressionBuilder.Limit(_sort, _limit.Limit),
                                    _sort),
                                k),
                            _project);
                    }
                }

                internal override DbExpression Skip(DbExpression k)
                {
                    // source.Sort(o).Limit(k).Project(p).Skip(k2) -> source.Sort(o).Limit(k).Skip(k2, o).Project(p)
                    return RebindProject(
                        ApplySortOrderToSkip(
                            DbExpressionBuilder.Limit(_sort, _limit.Limit),
                            _sort,
                            k),
                        _project);
                }
            }

            /// <summary>
            /// Represents an expression of the form: source.Skip(k, o).Project(p)
            /// </summary>
            private class ProjectSkipLifter : OrderByLifterBase
            {
                private readonly DbProjectExpression _project;
                private readonly DbSkipExpression _skip;
                private readonly DbExpression _source;

                internal ProjectSkipLifter(DbProjectExpression project, DbSkipExpression skip, AliasGenerator aliasGenerator)
                    : base(project, aliasGenerator)
                {
                    _project = project;
                    _skip = skip;
                    _source = _skip.Input.Expression;
                }

                internal override DbExpression Filter(DbFilterExpression filter)
                {
                    // source.Skip(k, o).Project(p).Filter(f) -> source.Skip(k, o).Filter(e => f(p(e))).Sort(o).Project(p)
                    return RebindProject(
                        ApplySkipOrderToSort(
                            ComposeFilter(_skip, _project, filter),
                            _skip),
                        _project);
                }

                internal override DbExpression Limit(DbExpression k)
                {
                    // the result is already ordered (no compensation is required)
                    return DbExpressionBuilder.Limit(_root, k);
                }

                internal override DbExpression Project(DbProjectExpression project)
                {
                    // source.Skip(k, o).Project(p).Project(p2) -> source.Skip(k, o).Project(e => p2(p(e)))
                    return ComposeProject(_skip, _project, project);
                }

                internal override DbExpression Skip(DbExpression k)
                {
                    // source.Skip(k, o).Project(p).Skip(k2) -> source.Skip(k + k2, o).Project(p) where k and k2 are constants,
                    // otherwise -> source.Skip(k, o).Skip(k2, o).Project(p) 
                    if (_skip.Count.ExpressionKind == DbExpressionKind.Constant &&
                        k.ExpressionKind == DbExpressionKind.Constant)
                    {
                        return RebindProject(AddToSkip(_source, _skip, k), _project);
                    }
                    else
                    {
                        return RebindProject(RebindSkip(_skip, _skip, k), _project);
                    }
                }
            }

            /// <summary>
            /// Represents an expression of the form: source.Skip(k, o)
            /// </summary>
            private class SkipLifter : OrderByLifterBase
            {
                private readonly DbSkipExpression _skip;
                private readonly DbExpression _source;

                internal SkipLifter(DbSkipExpression skip, AliasGenerator aliasGenerator)
                    : base(skip, aliasGenerator)
                {
                    _skip = skip;
                    _source = skip.Input.Expression;
                }

                internal override DbExpression Filter(DbFilterExpression filter)
                {
                    // source.Skip(k, o).Filter(f) -> source.Skip(k, o).Filter(f).Sort(o)
                    return ApplySkipOrderToSort(filter, _skip);
                }

                internal override DbExpression Project(DbProjectExpression project)
                {
                    // the result is already ordered (no compensation is required)
                    return project;
                }

                internal override DbExpression Limit(DbExpression k)
                {
                    // the result is already ordered (no compensation is required)
                    return DbExpressionBuilder.Limit(_root, k);
                }

                internal override DbExpression Skip(DbExpression k)
                {
                    // source.Skip(k, o).Skip(k2) -> source.Skip(k + k2, o) where k and k2 are both constants
                    // otherwise, -> source.Skip(k, o).Skip(k2, o)
                    if (_skip.Count.ExpressionKind == DbExpressionKind.Constant &&
                        k.ExpressionKind == DbExpressionKind.Constant)
                    {
                        return AddToSkip(_source, _skip, k);
                    }
                    else
                    {
                        return RebindSkip(_skip, _skip, k);
                    }
                }
            }

            /// <summary>
            /// Represents an expression of the form: source.Sort(o).Project(p)
            /// </summary>
            private class ProjectSortLifter : OrderByLifterBase
            {
                private readonly DbProjectExpression _project;
                private readonly DbSortExpression _sort;
                private readonly DbExpression _source;

                internal ProjectSortLifter(DbProjectExpression project, DbSortExpression sort, AliasGenerator aliasGenerator)
                    : base(project, aliasGenerator)
                {
                    _project = project;
                    _sort = sort;
                    _source = sort.Input.Expression;
                }

                internal override DbExpression Project(DbProjectExpression project)
                {
                    // source.Sort(o).Project(p).Project(p2) -> source.Sort(o).Project(e => p2(p(2)))
                    return ComposeProject(_sort, _project, project);
                }

                internal override DbExpression Filter(DbFilterExpression filter)
                {
                    // source.Sort(o).Project(p).Filter(f) -> source.Filter(e => f(p(e))).Sort(o).Project(p)
                    return RebindProject(
                        RebindSort(
                            ComposeFilter(_source, _project, filter),
                            _sort),
                        _project);
                }

                internal override DbExpression Limit(DbExpression k)
                {
                    // the result is already ordered (no compensation is required)
                    return DbExpressionBuilder.Limit(_root, k);
                }

                internal override DbExpression Skip(DbExpression k)
                {
                    // source.Sort(o).Project(p).Skip(k) -> source.Skip(k, o).Project(p)
                    return RebindProject(ApplySortOrderToSkip(_source, _sort, k), _project);
                }
            }

            /// <summary>
            /// Represents an expression for which there is an explicit order by: source.Sort(o)
            /// </summary>
            private class SortLifter : OrderByLifterBase
            {
                private readonly DbSortExpression _sort;
                private readonly DbExpression _source;

                internal SortLifter(DbSortExpression sort, AliasGenerator aliasGenerator)
                    : base(sort, aliasGenerator)
                {
                    _sort = sort;
                    _source = sort.Input.Expression;
                }

                internal override DbExpression Project(DbProjectExpression project)
                {
                    // the result is already ordered (no compensation is required)
                    return project;
                }

                internal override DbExpression Filter(DbFilterExpression filter)
                {
                    // source.Sort(o).Filter(f) -> source.Filter(f).Sort(o)
                    return RebindSort(RebindFilter(_source, filter), _sort);
                }

                internal override DbExpression Limit(DbExpression k)
                {
                    // the result is already ordered (no compensation is required)
                    return DbExpressionBuilder.Limit(_root, k);
                }

                internal override DbExpression Skip(DbExpression k)
                {
                    // source.Sort(o).Skip(k) -> source.Skip(k, o)
                    return ApplySortOrderToSkip(_source, _sort, k);
                }
            }

            /// <summary>
            /// Used for sources that do not have any intrinsic order.
            /// </summary>
            private class PassthroughOrderByLifter : OrderByLifterBase
            {
                internal PassthroughOrderByLifter(DbExpression source, AliasGenerator aliasGenerator)
                    : base(source, aliasGenerator)
                {
                }

                internal override DbExpression Project(DbProjectExpression project)
                {
                    return project;
                }

                internal override DbExpression Filter(DbFilterExpression filter)
                {
                    return filter;
                }

                internal override DbExpression OfType(TypeUsage type)
                {
                    return DbExpressionBuilder.OfType(_root, type);
                }

                internal override DbExpression Limit(DbExpression k)
                {
                    return DbExpressionBuilder.Limit(_root, k);
                }

                internal override DbExpression Skip(DbExpression k)
                {
                    // since the source has no intrinsic order, we need to throw (skip
                    // requires order)
                    throw EntityUtil.NotSupported(System.Data.Entity.Strings.ELinq_SkipWithoutOrder);
                }
            }
        }
    }
}
