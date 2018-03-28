//---------------------------------------------------------------------
// <copyright file="QueryExpr.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Common.EntitySql.AST
{
    using System;
    using System.Globalization;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Represents select kind (value,row).
    /// </summary>
    internal enum SelectKind
    {
        Value,
        Row
    }

    /// <summary>
    /// Represents join kind (cross,inner,leftouter,rightouter).
    /// </summary>
    internal enum JoinKind
    {
        Cross,
        Inner,
        LeftOuter,
        FullOuter,
        RightOuter
    }

    /// <summary>
    /// Represents order kind (none=asc,asc,desc).
    /// </summary>
    internal enum OrderKind
    {
        None,
        Asc,
        Desc
    }

    /// <summary>
    /// Represents distinct kind (none=all,all,distinct).
    /// </summary>
    internal enum DistinctKind
    {
        None,
        All,
        Distinct
    }

    /// <summary>
    /// Represents apply kind (cross,outer).
    /// </summary>
    internal enum ApplyKind
    {
        Cross,
        Outer
    }

    /// <summary>
    /// Represents a query expression ast node.
    /// </summary>
    internal sealed class QueryExpr : Node
    {
        private readonly SelectClause _selectClause;
        private readonly FromClause _fromClause;
        private readonly Node _whereClause;
        private readonly GroupByClause _groupByClause;
        private readonly HavingClause _havingClause;
        private readonly OrderByClause _orderByClause;

        /// <summary>
        /// Initializes a query expression ast node.
        /// </summary>
        /// <param name="selectClause">select clause</param>
        /// <param name="fromClause">from clasuse</param>
        /// <param name="whereClause">optional where clause</param>
        /// <param name="groupByClause">optional group by clause</param>
        /// <param name="havingClause">optional having clause</param>
        /// <param name="orderByClause">optional order by clause</param>
        internal QueryExpr(SelectClause selectClause,
                           FromClause fromClause,
                           Node whereClause,
                           GroupByClause groupByClause,
                           HavingClause havingClause,
                           OrderByClause orderByClause)
        {
            _selectClause = selectClause;
            _fromClause = fromClause;
            _whereClause = whereClause;
            _groupByClause = groupByClause;
            _havingClause = havingClause;
            _orderByClause = orderByClause;
        }

        /// <summary>
        /// Returns select clause.
        /// </summary>
        internal SelectClause SelectClause
        {
            get { return _selectClause; }
        }

        /// <summary>
        /// Returns from clause.
        /// </summary>
        internal FromClause FromClause
        {
            get { return _fromClause; }
        }

        /// <summary>
        /// Returns optional where clause (expr).
        /// </summary>
        internal Node WhereClause
        {
            get { return _whereClause; }
        }

        /// <summary>
        /// Returns optional group by clause.
        /// </summary>
        internal GroupByClause GroupByClause
        {
            get { return _groupByClause; }
        }

        /// <summary>
        /// Returns optional having clause (expr).
        /// </summary>
        internal HavingClause HavingClause
        {
            get { return _havingClause; }
        }

        /// <summary>
        /// Returns optional order by clause.
        /// </summary>
        internal OrderByClause OrderByClause
        {
            get { return _orderByClause; }
        }

        /// <summary>
        /// Returns true if method calls are present.
        /// </summary>
        internal bool HasMethodCall
        {
            get
            {
                return _selectClause.HasMethodCall ||
                       (null != _havingClause && _havingClause.HasMethodCall) ||
                       (null != _orderByClause && _orderByClause.HasMethodCall);
            }
        }
    }

    /// <summary>
    /// Represents select clause.
    /// </summary>
    internal sealed class SelectClause : Node
    {
        private readonly NodeList<AliasedExpr> _selectClauseItems;
        private readonly SelectKind _selectKind;
        private readonly DistinctKind _distinctKind;
        private readonly Node _topExpr;
        private readonly uint _methodCallCount;

        /// <summary>
        /// Initialize SelectKind.SelectRow clause.
        /// </summary>
        internal SelectClause(NodeList<AliasedExpr> items, SelectKind selectKind, DistinctKind distinctKind, Node topExpr, uint methodCallCount)
        {
            _selectKind = selectKind;
            _selectClauseItems = items;
            _distinctKind = distinctKind;
            _topExpr = topExpr;
            _methodCallCount = methodCallCount;
        }

        /// <summary>
        /// Projection list.
        /// </summary>
        internal NodeList<AliasedExpr> Items
        {
            get { return _selectClauseItems; }
        }

        /// <summary>
        /// Select kind (row or value).
        /// </summary>
        internal SelectKind SelectKind
        {
            get { return _selectKind; }
        }

        /// <summary>
        /// Distinct kind (none,all,distinct).
        /// </summary>
        internal DistinctKind DistinctKind
        {
            get { return _distinctKind; }
        }

        /// <summary>
        /// Optional top expression.
        /// </summary>
        internal Node TopExpr
        {
            get { return _topExpr; }
        }

        /// <summary>
        /// True if select list has method calls.
        /// </summary>
        internal bool HasMethodCall
        {
            get { return (_methodCallCount > 0); }
        }
    }

    /// <summary>
    /// Represents from clause.
    /// </summary>
    internal sealed class FromClause : Node
    {
        private readonly NodeList<FromClauseItem> _fromClauseItems;

        /// <summary>
        /// Initializes from clause.
        /// </summary>
        internal FromClause(NodeList<FromClauseItem> fromClauseItems)
        {
            _fromClauseItems = fromClauseItems;
        }

        /// <summary>
        /// List of from clause items.
        /// </summary>
        internal NodeList<FromClauseItem> FromClauseItems
        {
            get { return _fromClauseItems; }
        }
    }

    /// <summary>
    /// From clause item kind.
    /// </summary>
    internal enum FromClauseItemKind
    {
        AliasedFromClause,
        JoinFromClause,
        ApplyFromClause
    }

    /// <summary>
    /// Represents single from clause item.
    /// </summary>
    internal sealed class FromClauseItem : Node
    {
        private readonly Node _fromClauseItemExpr;
        private readonly FromClauseItemKind _fromClauseItemKind;

        /// <summary>
        /// Initializes as 'simple' aliased expression.
        /// </summary>
        internal FromClauseItem(AliasedExpr aliasExpr)
        {
            _fromClauseItemExpr = aliasExpr;
            _fromClauseItemKind = FromClauseItemKind.AliasedFromClause;
        }

        /// <summary>
        /// Initializes as join clause item.
        /// </summary>
        internal FromClauseItem(JoinClauseItem joinClauseItem)
        {
            _fromClauseItemExpr = joinClauseItem;
            _fromClauseItemKind = FromClauseItemKind.JoinFromClause;
        }

        /// <summary>
        /// Initializes as apply clause item.
        /// </summary>
        internal FromClauseItem(ApplyClauseItem applyClauseItem)
        {
            _fromClauseItemExpr = applyClauseItem;
            _fromClauseItemKind = FromClauseItemKind.ApplyFromClause;
        }

        /// <summary>
        /// From clause item expression.
        /// </summary>
        internal Node FromExpr
        {
            get { return _fromClauseItemExpr; }
        }

        /// <summary>
        /// From clause item kind (alias,join,apply).
        /// </summary>
        internal FromClauseItemKind FromClauseItemKind
        {
            get { return _fromClauseItemKind; }
        }
    }

    /// <summary>
    /// Represents group by clause.
    /// </summary>
    internal sealed class GroupByClause : Node
    {
        private readonly NodeList<AliasedExpr> _groupItems;

        /// <summary>
        /// Initializes GROUP BY clause
        /// </summary>
        internal GroupByClause(NodeList<AliasedExpr> groupItems)
        {
            _groupItems = groupItems;
        }

        /// <summary>
        /// Group items.
        /// </summary>
        internal NodeList<AliasedExpr> GroupItems
        {
            get { return _groupItems; }
        }
    }

    /// <summary>
    /// Represents having clause.
    /// </summary>
    internal sealed class HavingClause : Node
    {
        private readonly Node _havingExpr;
        private readonly uint _methodCallCount;

        /// <summary>
        /// Initializes having clause.
        /// </summary>
        internal HavingClause(Node havingExpr, uint methodCallCounter)
        {
            _havingExpr = havingExpr;
            _methodCallCount = methodCallCounter;
        }

        /// <summary>
        /// Returns having inner expression.
        /// </summary>
        internal Node HavingPredicate
        {
            get { return _havingExpr; }
        }

        /// <summary>
        /// True if predicate has method calls.
        /// </summary>
        internal bool HasMethodCall
        {
            get { return (_methodCallCount > 0); }
        }
    }

    /// <summary>
    /// Represents order by clause.
    /// </summary>
    internal sealed class OrderByClause : Node
    {
        private readonly NodeList<OrderByClauseItem> _orderByClauseItem;
        private readonly Node _skipExpr;
        private readonly Node _limitExpr;
        private readonly uint _methodCallCount;

        /// <summary>
        /// Initializes order by clause.
        /// </summary>
        internal OrderByClause(NodeList<OrderByClauseItem> orderByClauseItem, Node skipExpr, Node limitExpr, uint methodCallCount)
        {
            _orderByClauseItem = orderByClauseItem;
            _skipExpr = skipExpr;
            _limitExpr = limitExpr;
            _methodCallCount = methodCallCount;
        }

        /// <summary>
        /// Returns order by clause items.
        /// </summary>
        internal NodeList<OrderByClauseItem> OrderByClauseItem
        {
            get { return _orderByClauseItem; }
        }

        /// <summary>
        /// Returns skip sub clause ast node.
        /// </summary>
        internal Node SkipSubClause
        {
            get { return _skipExpr; }
        }

        /// <summary>
        /// Returns limit sub-clause ast node.
        /// </summary>
        internal Node LimitSubClause
        {
            get { return _limitExpr; }
        }

        /// <summary>
        /// True if order by has method calls.
        /// </summary>
        internal bool HasMethodCall
        {
            get { return (_methodCallCount > 0); }
        }
    }

    /// <summary>
    /// Represents a order by clause item.
    /// </summary>
    internal sealed class OrderByClauseItem : Node
    {
        private readonly Node _orderExpr;
        private readonly OrderKind _orderKind;
        private readonly Identifier _optCollationIdentifier;

        /// <summary>
        /// Initializes non-collated order by clause item.
        /// </summary>
        internal OrderByClauseItem(Node orderExpr, OrderKind orderKind)
            : this(orderExpr, orderKind, null)
        {
        }

        /// <summary>
        /// Initializes collated order by clause item.
        /// </summary>
        /// <param name="optCollationIdentifier">optional Collation identifier</param>
        internal OrderByClauseItem(Node orderExpr, OrderKind orderKind, Identifier optCollationIdentifier)
        {
            _orderExpr = orderExpr;
            _orderKind = orderKind;
            _optCollationIdentifier = optCollationIdentifier;
        }

        /// <summary>
        /// Oeturns order expression.
        /// </summary>
        internal Node OrderExpr
        {
            get { return _orderExpr; }
        }

        /// <summary>
        /// Returns order kind (none,asc,desc).
        /// </summary>
        internal OrderKind OrderKind
        {
            get { return _orderKind; }
        }

        /// <summary>
        /// Returns collattion identifier if one exists.
        /// </summary>
        internal Identifier Collation
        {
            get { return _optCollationIdentifier; }
        }
    }

    /// <summary>
    /// Represents join clause item.
    /// </summary>
    internal sealed class JoinClauseItem : Node
    {
        private readonly FromClauseItem _joinLeft;
        private readonly FromClauseItem _joinRight;
        private JoinKind _joinKind;
        private readonly Node _onExpr;

        /// <summary>
        /// Initializes join clause item without ON expression.
        /// </summary>
        internal JoinClauseItem(FromClauseItem joinLeft, FromClauseItem joinRight, JoinKind joinKind)
            : this(joinLeft, joinRight, joinKind, null)
        {
        }

        /// <summary>
        /// Initializes join clause item with ON expression.
        /// </summary>
        internal JoinClauseItem(FromClauseItem joinLeft, FromClauseItem joinRight, JoinKind joinKind, Node onExpr)
        {
            _joinLeft = joinLeft;
            _joinRight = joinRight;
            _joinKind = joinKind;
            _onExpr = onExpr;
        }

        /// <summary>
        /// Returns join left expression.
        /// </summary>
        internal FromClauseItem LeftExpr
        {
            get { return _joinLeft; }
        }

        /// <summary>
        /// Returns join right expression.
        /// </summary>
        internal FromClauseItem RightExpr
        {
            get { return _joinRight; }
        }

        /// <summary>
        /// Join kind (cross, inner, full, left outer,right outer).
        /// </summary>
        internal JoinKind JoinKind
        {
            get { return _joinKind; }
            set { _joinKind = value; }
        }

        /// <summary>
        /// Returns join on expression.
        /// </summary>
        internal Node OnExpr
        {
            get { return _onExpr; }
        }
    }

    /// <summary>
    /// Represents apply expression.
    /// </summary>
    internal sealed class ApplyClauseItem : Node
    {
        private readonly FromClauseItem _applyLeft;
        private readonly FromClauseItem _applyRight;
        private readonly ApplyKind _applyKind;

        /// <summary>
        /// Initializes apply clause item.
        /// </summary>
        internal ApplyClauseItem(FromClauseItem applyLeft, FromClauseItem applyRight, ApplyKind applyKind)
        {
            _applyLeft = applyLeft;
            _applyRight = applyRight;
            _applyKind = applyKind;
        }

        /// <summary>
        /// Returns apply left expression.
        /// </summary>
        internal FromClauseItem LeftExpr
        {
            get { return _applyLeft; }
        }

        /// <summary>
        /// Returns apply right expression.
        /// </summary>
        internal FromClauseItem RightExpr
        {
            get { return _applyRight; }
        }

        /// <summary>
        /// Returns apply kind (cross,outer).
        /// </summary>
        internal ApplyKind ApplyKind
        {
            get { return _applyKind; }
        }
    }
}
