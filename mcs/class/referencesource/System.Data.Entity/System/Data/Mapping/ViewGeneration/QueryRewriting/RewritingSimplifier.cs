//---------------------------------------------------------------------
// <copyright file="RewritingSimplifier.cs" company="Microsoft">
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
    internal class RewritingSimplifier<T_Tile> where T_Tile : class
    {
        private readonly T_Tile m_originalRewriting;
        private readonly T_Tile m_toAvoid;
        private readonly RewritingProcessor<T_Tile> m_qp;
        private readonly Dictionary<T_Tile, TileOpKind> m_usedViews = new Dictionary<T_Tile, TileOpKind>();

        // used for join/antisemijoin simplification
        private RewritingSimplifier(T_Tile originalRewriting, T_Tile toAvoid, Dictionary<T_Tile, TileOpKind> usedViews,
                                     RewritingProcessor<T_Tile> qp)
        {
            m_originalRewriting = originalRewriting;
            m_toAvoid = toAvoid;
            m_qp = qp;
            m_usedViews = usedViews;
        }

        // used for union simplification
        private RewritingSimplifier(T_Tile rewriting, T_Tile toFill, T_Tile toAvoid, RewritingProcessor<T_Tile> qp)
        {
            m_originalRewriting = toFill;
            m_toAvoid = toAvoid;
            m_qp = qp;
            m_usedViews = new Dictionary<T_Tile, TileOpKind>();
            GatherUnionedSubqueriesInUsedViews(rewriting);
        }

        // called for top query only
        internal static bool TrySimplifyUnionRewriting(ref T_Tile rewriting, T_Tile toFill, T_Tile toAvoid, RewritingProcessor<T_Tile> qp)
        {
            RewritingSimplifier<T_Tile> simplifier = new RewritingSimplifier<T_Tile>(rewriting, toFill, toAvoid, qp);
            // gather all unioned subqueries
            T_Tile simplifiedRewriting;
            if (simplifier.SimplifyRewriting(out simplifiedRewriting))
            {
                rewriting = simplifiedRewriting;
                return true;
            }
            return false;
        }

        // modifies usedViews - removes all redundant views from it
        internal static bool TrySimplifyJoinRewriting(ref T_Tile rewriting, T_Tile toAvoid, Dictionary<T_Tile, TileOpKind> usedViews, RewritingProcessor<T_Tile> qp)
        {
            RewritingSimplifier<T_Tile> simplifier = new RewritingSimplifier<T_Tile>(rewriting, toAvoid, usedViews, qp);
            T_Tile simplifiedRewriting;
            if (simplifier.SimplifyRewriting(out simplifiedRewriting))
            {
                rewriting = simplifiedRewriting;
                return true;
            }
            return false;
        }

        private void GatherUnionedSubqueriesInUsedViews(T_Tile query)
        {
            if (query != null)
            {
                if (m_qp.GetOpKind(query) != TileOpKind.Union)
                {
                    m_usedViews[query] = TileOpKind.Union;
                }
                else
                {
                    GatherUnionedSubqueriesInUsedViews(m_qp.GetArg1(query));
                    GatherUnionedSubqueriesInUsedViews(m_qp.GetArg2(query));
                }
            }
        }

        // isExactAnswer: matters for Intersections/Differences only
        private bool SimplifyRewriting(out T_Tile simplifiedRewriting)
        {
            bool compacted = false;
            simplifiedRewriting = null;
            T_Tile simplifiedOnce;
            while (SimplifyRewritingOnce(out simplifiedOnce))
            {
                compacted = true;
                simplifiedRewriting = simplifiedOnce;
            }
            return compacted;
        }

        // try removing one redundant view from intersected and subtracted views
        // This method uses a dynamic divide-and-conquer algorithm that avoids recomputing many intersections/differences
        private bool SimplifyRewritingOnce(out T_Tile simplifiedRewriting)
        {
            // check whether removing one or multiple views from intersected and subtracted views
            // still (a) reduces extra tuples, and (b) has no missing tuples
            // First, try removing a subtracted view
            HashSet<T_Tile> remainingViews = new HashSet<T_Tile>(m_usedViews.Keys);
            foreach (T_Tile usedView in m_usedViews.Keys)
            {
                // pick an intersected view, and nail it down
                switch (m_usedViews[usedView])
                {
                    case TileOpKind.Join:
                    case TileOpKind.Union:
                        remainingViews.Remove(usedView);
                        if (SimplifyRewritingOnce(usedView, remainingViews, out simplifiedRewriting))
                        {
                            return true;
                        }
                        remainingViews.Add(usedView);
                        break;
                }
            }
            simplifiedRewriting = null;
            return false;
        }

        // remainingViews may contain either unions only or intersections + differences
        private bool SimplifyRewritingOnce(T_Tile newRewriting, HashSet<T_Tile> remainingViews,
                                           out T_Tile simplifiedRewriting)
        {
            simplifiedRewriting = null;
            if (remainingViews.Count == 0)
            {
                return false;
            }
            if (remainingViews.Count == 1)
            {
                // determine the remaining view
                T_Tile remainingView = remainingViews.First();

                // check whether rewriting obtained so far is good enough
                // try disposing of this remaining view
                bool isDisposable = false;
                switch (m_usedViews[remainingView])
                {
                    case TileOpKind.Union:
                        // check whether rewriting still covers toFill
                        isDisposable = m_qp.IsContainedIn(m_originalRewriting, newRewriting);
                        break;
                    default: // intersection
                        isDisposable = m_qp.IsContainedIn(m_originalRewriting, newRewriting) &&
                                       m_qp.IsDisjointFrom(m_toAvoid, newRewriting);
                        break;
                }
                if (isDisposable)
                {
                    // yes, the remaining view is disposable
                    simplifiedRewriting = newRewriting;
                    m_usedViews.Remove(remainingView);
                    return true;
                }
                return false; // no, can't trash the remaining view
            }
            // split remainingViews into two halves
            // Compute rewriting for first half. Call recursively on second half.
            // Then, compute rewriting for second half. Call recursively on first half.
            int halfCount = remainingViews.Count / 2;
            int count = 0;
            T_Tile firstHalfRewriting = newRewriting;
            T_Tile secondHalfRewriting = newRewriting;
            HashSet<T_Tile> firstHalf = new HashSet<T_Tile>();
            HashSet<T_Tile> secondHalf = new HashSet<T_Tile>();
            foreach (T_Tile remainingView in remainingViews)
            {
                TileOpKind viewKind = m_usedViews[remainingView];
                // add to first half
                if (count++ < halfCount)
                {
                    firstHalf.Add(remainingView);
                    firstHalfRewriting = GetRewritingHalf(firstHalfRewriting, remainingView, viewKind);
                }
                else // add to second half
                {
                    secondHalf.Add(remainingView);
                    secondHalfRewriting = GetRewritingHalf(secondHalfRewriting, remainingView, viewKind);
                }
            }
            // now, call recursively
            return SimplifyRewritingOnce(firstHalfRewriting, secondHalf, out simplifiedRewriting)
                || SimplifyRewritingOnce(secondHalfRewriting, firstHalf, out simplifiedRewriting);
        }

        private T_Tile GetRewritingHalf(T_Tile halfRewriting, T_Tile remainingView, TileOpKind viewKind)
        {
            switch (viewKind)
            {
                case TileOpKind.Join:
                    halfRewriting = m_qp.Join(halfRewriting, remainingView); break;
                case TileOpKind.AntiSemiJoin:
                    halfRewriting = m_qp.AntiSemiJoin(halfRewriting, remainingView); break;
                case TileOpKind.Union:
                    halfRewriting = m_qp.Union(halfRewriting, remainingView); break;
                default: Debug.Fail("unexpected"); break;
            }
            return halfRewriting;
        }
    }
}
