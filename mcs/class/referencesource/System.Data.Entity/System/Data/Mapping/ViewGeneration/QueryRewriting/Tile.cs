//---------------------------------------------------------------------
// <copyright file="Tile.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq;
using System.Globalization;

namespace System.Data.Mapping.ViewGeneration.QueryRewriting
{
    internal enum TileOpKind
    {
        Union,
        Join,
        AntiSemiJoin,
        // Project,
        Named
    }

    internal interface ITileQuery
    {
        string Description { get; }
    }

    internal abstract class TileQueryProcessor<T_Query> where T_Query : ITileQuery
    {
        internal abstract T_Query Intersect(T_Query arg1, T_Query arg2);
        internal abstract T_Query Difference(T_Query arg1, T_Query arg2);
        internal abstract T_Query Union(T_Query arg1, T_Query arg2);
        internal abstract bool IsSatisfiable(T_Query query);
        internal abstract T_Query CreateDerivedViewBySelectingConstantAttributes(T_Query query);
    }

    internal class DefaultTileProcessor<T_Query> : TileProcessor<Tile<T_Query>> where T_Query : ITileQuery
    {
        private readonly TileQueryProcessor<T_Query> _tileQueryProcessor;

        internal DefaultTileProcessor(TileQueryProcessor<T_Query> tileQueryProcessor)
        {
            _tileQueryProcessor = tileQueryProcessor;
        }

        internal TileQueryProcessor<T_Query> QueryProcessor
        {
            get { return _tileQueryProcessor; }
        }

        internal override bool IsEmpty(Tile<T_Query> tile)
        {
            return false == _tileQueryProcessor.IsSatisfiable(tile.Query);
        }

        internal override Tile<T_Query> Union(Tile<T_Query> arg1, Tile<T_Query> arg2)
        {
            return new TileBinaryOperator<T_Query>(arg1, arg2, TileOpKind.Union, _tileQueryProcessor.Union(arg1.Query, arg2.Query));
        }

        internal override Tile<T_Query> Join(Tile<T_Query> arg1, Tile<T_Query> arg2)
        {
            return new TileBinaryOperator<T_Query>(arg1, arg2, TileOpKind.Join, _tileQueryProcessor.Intersect(arg1.Query, arg2.Query));
        }

        internal override Tile<T_Query> AntiSemiJoin(Tile<T_Query> arg1, Tile<T_Query> arg2)
        {
            return new TileBinaryOperator<T_Query>(arg1, arg2, TileOpKind.AntiSemiJoin, _tileQueryProcessor.Difference(arg1.Query, arg2.Query));
        }

        internal override Tile<T_Query> GetArg1(Tile<T_Query> tile)
        {
            return tile.Arg1;
        }

        internal override Tile<T_Query> GetArg2(Tile<T_Query> tile)
        {
            return tile.Arg2;
        }

        internal override TileOpKind GetOpKind(Tile<T_Query> tile)
        {
            return tile.OpKind;
        }

        internal bool IsContainedIn(Tile<T_Query> arg1, Tile<T_Query> arg2)
        {
            return IsEmpty(AntiSemiJoin(arg1, arg2));
        }

        internal bool IsEquivalentTo(Tile<T_Query> arg1, Tile<T_Query> arg2)
        {
            return IsContainedIn(arg1, arg2) && IsContainedIn(arg2, arg1);
        }
    }

    internal abstract class Tile<T_Query> where T_Query : ITileQuery
    {
        private readonly T_Query m_query;
        private readonly TileOpKind m_opKind;

        protected Tile(TileOpKind opKind, T_Query query)
        {
            m_opKind = opKind;
            m_query = query;
        }

        public T_Query Query
        {
            get { return m_query; }
        }

        public abstract string Description { get; }

        // multiple occurrences possible
        public IEnumerable<T_Query> GetNamedQueries()
        {
            return GetNamedQueries(this);
        }
        private static IEnumerable<T_Query> GetNamedQueries(Tile<T_Query> rewriting)
        {
            if (rewriting != null)
            {
                if (rewriting.OpKind == TileOpKind.Named)
                {
                    yield return ((TileNamed<T_Query>)rewriting).NamedQuery;
                }
                else
                {
                    foreach (T_Query query in GetNamedQueries(rewriting.Arg1))
                    {
                        yield return query;
                    }
                    foreach (T_Query query in GetNamedQueries(rewriting.Arg2))
                    {
                        yield return query;
                    }
                }
            }
        }

        public override string ToString()
        {
            string formattedQuery = this.Description;
            if (formattedQuery != null)
            {
                return String.Format(CultureInfo.InvariantCulture, "{0}: [{1}]", this.Description, this.Query);
            }
            else
            {
                return String.Format(CultureInfo.InvariantCulture, "[{0}]", this.Query);
            }
        }

        public abstract Tile<T_Query> Arg1
        {
            get;
        }

        public abstract Tile<T_Query> Arg2
        {
            get;
        }

        public TileOpKind OpKind
        {
            get { return m_opKind; }
        }

        internal abstract Tile<T_Query> Replace(Tile<T_Query> oldTile, Tile<T_Query> newTile);
    }

    internal class TileNamed<T_Query> : Tile<T_Query> where T_Query : ITileQuery
    {
        public TileNamed(T_Query namedQuery)
            : base(TileOpKind.Named, namedQuery)
        {
            Debug.Assert(namedQuery != null);
        }

        public T_Query NamedQuery
        {
            get { return this.Query; }
        }

        public override Tile<T_Query> Arg1 { get { return null; } }
        public override Tile<T_Query> Arg2 { get { return null; } }

        public override string Description
        {
            get { return this.Query.Description; }
        }

        public override string ToString()
        {
            return this.Query.ToString();
        }

        internal override Tile<T_Query> Replace(Tile<T_Query> oldTile, Tile<T_Query> newTile)
        {
            return (this == oldTile) ? newTile : this;
        }
    }

    internal class TileBinaryOperator<T_Query> : Tile<T_Query> where T_Query : ITileQuery
    {
        private readonly Tile<T_Query> m_arg1;
        private readonly Tile<T_Query> m_arg2;

        public TileBinaryOperator(Tile<T_Query> arg1, Tile<T_Query> arg2, TileOpKind opKind, T_Query query)
            : base(opKind, query)
        {
            Debug.Assert(arg1 != null && arg2 != null);
            m_arg1 = arg1;
            m_arg2 = arg2;
        }

        public override Tile<T_Query> Arg1 { get { return m_arg1; } }
        public override Tile<T_Query> Arg2 { get { return m_arg2; } }

        public override string Description
        {
            get
            {
                string descriptionFormat = null;
                switch (OpKind)
                {
                    case TileOpKind.Join: descriptionFormat = "({0} & {1})"; break;
                    case TileOpKind.AntiSemiJoin: descriptionFormat = "({0} - {1})"; break;
                    case TileOpKind.Union: descriptionFormat = "({0} | {1})"; break;
                    default: Debug.Fail("Unexpected binary operator"); break;
                }
                return String.Format(CultureInfo.InvariantCulture, descriptionFormat, this.Arg1.Description, this.Arg2.Description);
            }
        }

        internal override Tile<T_Query> Replace(Tile<T_Query> oldTile, Tile<T_Query> newTile)
        {
            Tile<T_Query> newArg1 = Arg1.Replace(oldTile, newTile);
            Tile<T_Query> newArg2 = Arg2.Replace(oldTile, newTile);
            if (newArg1 != Arg1 || newArg2 != Arg2)
            {
                return new TileBinaryOperator<T_Query>(newArg1, newArg2, OpKind, Query);
            }
            return this;
        }
    }
}
