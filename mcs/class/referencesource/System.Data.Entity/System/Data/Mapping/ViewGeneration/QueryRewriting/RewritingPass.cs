//---------------------------------------------------------------------
// <copyright file="RewritingPass.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace System.Data.Mapping.ViewGeneration.QueryRewriting
{
    // Goal: use the next view to get rewritingSoFar to be closer to the goal
    internal class RewritingPass<T_Tile> where T_Tile : class
    {
        // region that rewriting needs to cover
        private readonly T_Tile m_toFill;
        // region that rewriting needs to be disjoint with
        private readonly T_Tile m_toAvoid;
        private readonly List<T_Tile> m_views;
        private readonly RewritingProcessor<T_Tile> m_qp;
        private readonly Dictionary<T_Tile, TileOpKind> m_usedViews = new Dictionary<T_Tile, TileOpKind>();

        public RewritingPass(T_Tile toFill, T_Tile toAvoid, List<T_Tile> views, RewritingProcessor<T_Tile> qp)
        {
            m_toFill = toFill;
            m_toAvoid = toAvoid;
            m_views = views;
            m_qp = qp;
        }

        public static bool RewriteQuery(T_Tile toFill, T_Tile toAvoid, out T_Tile rewriting, List<T_Tile> views, RewritingProcessor<T_Tile> qp)
        {
            RewritingPass<T_Tile> rewritingPass = new RewritingPass<T_Tile>(toFill, toAvoid, views, qp);
            if (rewritingPass.RewriteQuery(out rewriting))
            {
                RewritingSimplifier<T_Tile>.TrySimplifyUnionRewriting(ref rewriting, toFill, toAvoid, qp);
                return true;
            }
            return false;
        }

        private static bool RewriteQueryInternal(T_Tile toFill, T_Tile toAvoid, out T_Tile rewriting, List<T_Tile> views, HashSet<T_Tile> recentlyUsedViews, RewritingProcessor<T_Tile> qp)
        {
            if (qp.REORDER_VIEWS && recentlyUsedViews.Count > 0)
            {
                // move recently used views toward the end
                List<T_Tile> reorderedViews = new List<T_Tile>();
                foreach (T_Tile view in views)
                {
                    if (false == recentlyUsedViews.Contains(view))
                    {
                        reorderedViews.Add(view);
                    }
                }
                reorderedViews.AddRange(recentlyUsedViews);
                views = reorderedViews;
            }

            RewritingPass<T_Tile> rewritingPass = new RewritingPass<T_Tile>(toFill, toAvoid, views, qp);
            return rewritingPass.RewriteQuery(out rewriting);
        }

        private bool RewriteQuery(out T_Tile rewriting)
        {
            rewriting = m_toFill;
            T_Tile rewritingSoFar;

            if (false == FindRewritingByIncludedAndDisjoint(out rewritingSoFar))
            {
                if (false == FindContributingView(out rewritingSoFar))
                {
                    return false;
                }
            }

            bool hasExtraTuples = !m_qp.IsDisjointFrom(rewritingSoFar, m_toAvoid);

            // try to cut off extra tuples using joins
            if (hasExtraTuples)
            {
                foreach (T_Tile view in AvailableViews)
                {
                    if (TryJoin(view, ref rewritingSoFar))
                    {
                        hasExtraTuples = false;
                        break;
                    }
                }
            }

            // try to cut off extra tuples using anti-semijoins
            if (hasExtraTuples)
            {
                foreach (T_Tile view in AvailableViews)
                {
                    if (TryAntiSemiJoin(view, ref rewritingSoFar))
                    {
                        hasExtraTuples = false;
                        break;
                    }
                }
            }

            if (hasExtraTuples)
            {
                return false; // won't be able to cut off extra tuples
            }

            // remove redundant joins and anti-semijoins
            RewritingSimplifier<T_Tile>.TrySimplifyJoinRewriting(ref rewritingSoFar, m_toAvoid, m_usedViews, m_qp);

            // find rewriting for missing tuples, if any
            T_Tile missingTuples = m_qp.AntiSemiJoin(m_toFill, rewritingSoFar);
            if (!m_qp.IsEmpty(missingTuples))
            {
                T_Tile rewritingForMissingTuples;
                if (false == RewritingPass<T_Tile>.RewriteQueryInternal(missingTuples, m_toAvoid, out rewritingForMissingTuples, m_views, new HashSet<T_Tile>(m_usedViews.Keys), m_qp))
                {
                    rewriting = rewritingForMissingTuples;
                    return false; // failure
                }
                else
                {
                    // Although a more general optimization for UNIONs will handle this case,
                    // adding this check reduces the overall number of containment tests
                    if (m_qp.IsContainedIn(rewritingSoFar, rewritingForMissingTuples))
                    {
                        rewritingSoFar = rewritingForMissingTuples;
                    }
                    else
                    {
                        rewritingSoFar = m_qp.Union(rewritingSoFar, rewritingForMissingTuples);
                    }
                }
            }

            // if we reached this point, we have a successful rewriting
            rewriting = rewritingSoFar;
            return true;
        }

        // returns true if no more extra tuples are left
        private bool TryJoin(T_Tile view, ref T_Tile rewriting)
        {
            T_Tile newRewriting = m_qp.Join(rewriting, view);
            if (!m_qp.IsEmpty(newRewriting))
            {
                m_usedViews[view] = TileOpKind.Join;
                rewriting = newRewriting;
                return m_qp.IsDisjointFrom(rewriting, m_toAvoid);
            }
            return false;
        }

        // returns true if no more extra tuples are left
        private bool TryAntiSemiJoin(T_Tile view, ref T_Tile rewriting)
        {
            T_Tile newRewriting = m_qp.AntiSemiJoin(rewriting, view);
            if (!m_qp.IsEmpty(newRewriting))
            {
                m_usedViews[view] = TileOpKind.AntiSemiJoin;
                rewriting = newRewriting;
                return m_qp.IsDisjointFrom(rewriting, m_toAvoid);
            }
            return false;
        }

        // Try to find a rewriting by intersecting all views which contain the query
        // and subtracting all views that are disjoint from the query
        private bool FindRewritingByIncludedAndDisjoint(out T_Tile rewritingSoFar)
        {
            // intersect all views in which m_toFill is contained
            rewritingSoFar = null;
            foreach (T_Tile view in AvailableViews)
            {
                if (m_qp.IsContainedIn(m_toFill, view)) // query <= view
                {
                    if (rewritingSoFar == null)
                    {
                        rewritingSoFar = view;
                        m_usedViews[view] = TileOpKind.Join;
                    }
                    else
                    {
                        T_Tile newRewriting = m_qp.Join(rewritingSoFar, view);
                        if (!m_qp.IsContainedIn(rewritingSoFar, newRewriting))
                        {
                            rewritingSoFar = newRewriting;
                            m_usedViews[view] = TileOpKind.Join; // it is a useful join
                        }
                        else
                        {
                            continue; // useless join
                        }
                    }
                    if (m_qp.IsContainedIn(rewritingSoFar, m_toFill))
                    {
                        return true;
                    }
                }
            }
            // subtract all views that are disjoint from m_toFill
            if (rewritingSoFar != null)
            {
                foreach (T_Tile view in AvailableViews)
                {
                    if (m_qp.IsDisjointFrom(m_toFill, view)) // query ^ view = {}
                    {
                        if (!m_qp.IsDisjointFrom(rewritingSoFar, view))
                        {
                            rewritingSoFar = m_qp.AntiSemiJoin(rewritingSoFar, view);
                            m_usedViews[view] = TileOpKind.AntiSemiJoin;
                            if (m_qp.IsContainedIn(rewritingSoFar, m_toFill))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return rewritingSoFar != null;
        }

        private bool FindContributingView(out T_Tile rewriting)
        {
            // find some view that helps reduce toFill
            foreach (T_Tile view in AvailableViews)
            {
                if (false == m_qp.IsDisjointFrom(view, m_toFill))
                {
                    rewriting = view;
                    m_usedViews[view] = TileOpKind.Join; // positive, intersected
                    return true;
                }
            }
            rewriting = null;
            return false;
        }

        private IEnumerable<T_Tile> AvailableViews
        {
            get { return m_views.Where(view => !m_usedViews.ContainsKey(view)); }
        }
    }
}
