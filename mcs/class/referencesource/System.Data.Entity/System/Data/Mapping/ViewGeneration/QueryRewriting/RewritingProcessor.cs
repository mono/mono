//---------------------------------------------------------------------
// <copyright file="RewritingProcessor.cs" company="Microsoft">
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

namespace System.Data.Mapping.ViewGeneration.QueryRewriting
{
    internal abstract class TileProcessor<T_Tile>
    {
        internal abstract bool IsEmpty(T_Tile tile);
        internal abstract T_Tile Union(T_Tile a, T_Tile b);
        internal abstract T_Tile Join(T_Tile a, T_Tile b);
        internal abstract T_Tile AntiSemiJoin(T_Tile a, T_Tile b);

        internal abstract T_Tile GetArg1(T_Tile tile);
        internal abstract T_Tile GetArg2(T_Tile tile);
        internal abstract TileOpKind GetOpKind(T_Tile tile);
    }

    internal class RewritingProcessor<T_Tile> : TileProcessor<T_Tile> where T_Tile : class
    {
        public double PERMUTE_FRACTION = 0.0;
        public int MIN_PERMUTATIONS = 0;
        public int MAX_PERMUTATIONS = 0;
        public bool REORDER_VIEWS = false;

        private int m_numSATChecks;
        private int m_numIntersection;
        private int m_numDifference;
        private int m_numUnion;

        private int m_numErrors;

        private TileProcessor<T_Tile> m_tileProcessor;

        public RewritingProcessor(TileProcessor<T_Tile> tileProcessor)
        {
            m_tileProcessor = tileProcessor;
        }

        internal TileProcessor<T_Tile> TileProcessor
        {
            get { return m_tileProcessor; }
        }

        public void GetStatistics(out int numSATChecks, out int numIntersection, out int numUnion, out int numDifference, out int numErrors)
        {
            numSATChecks = m_numSATChecks;
            numIntersection = m_numIntersection;
            numUnion = m_numUnion;
            numDifference = m_numDifference;
            numErrors = m_numErrors;
        }

        public void PrintStatistics()
        {
            Console.WriteLine("{0} containment checks, {4} set operations ({1} intersections + {2} unions + {3} differences)",
                m_numSATChecks, m_numIntersection, m_numUnion, m_numDifference,
                                m_numIntersection + m_numUnion + m_numDifference);
            Console.WriteLine("{0} errors", m_numErrors);
        }

        internal override T_Tile GetArg1(T_Tile tile)
        {
            return m_tileProcessor.GetArg1(tile);
        }

        internal override T_Tile GetArg2(T_Tile tile)
        {
            return m_tileProcessor.GetArg2(tile);
        }

        internal override TileOpKind GetOpKind(T_Tile tile)
        {
            return m_tileProcessor.GetOpKind(tile);
        }

        internal override bool IsEmpty(T_Tile a)
        {
            m_numSATChecks++;
            return m_tileProcessor.IsEmpty(a);
        }

        public bool IsDisjointFrom(T_Tile a, T_Tile b)
        {
            return m_tileProcessor.IsEmpty(Join(a, b));
        }

        internal bool IsContainedIn(T_Tile a, T_Tile b)
        {
            T_Tile difference = AntiSemiJoin(a, b);
            return IsEmpty(difference);
        }

        internal bool IsEquivalentTo(T_Tile a, T_Tile b)
        {
            bool aInB = IsContainedIn(a, b);
            bool bInA = IsContainedIn(b, a);
            return aInB && bInA;
        }

        internal override T_Tile Union(T_Tile a, T_Tile b)
        {
            m_numUnion++;
            return m_tileProcessor.Union(a, b);
        }

        internal override T_Tile Join(T_Tile a, T_Tile b)
        {
            if (a == null)
            {
                return b;
            }
            m_numIntersection++;
            return m_tileProcessor.Join(a, b);
        }

        internal override T_Tile AntiSemiJoin(T_Tile a, T_Tile b)
        {
            m_numDifference++;
            return m_tileProcessor.AntiSemiJoin(a, b);
        }

        public void AddError()
        {
            m_numErrors++;
        }

        public int CountOperators(T_Tile query)
        {
            int count = 0;
            if (query != null)
            {
                if (GetOpKind(query) != TileOpKind.Named)
                {
                    count++;
                    count += CountOperators(GetArg1(query));
                    count += CountOperators(GetArg2(query));
                }
            }
            return count;
        }

        public int CountViews(T_Tile query)
        {
            HashSet<T_Tile> views = new HashSet<T_Tile>();
            GatherViews(query, views);
            return views.Count;
        }

        public void GatherViews(T_Tile rewriting, HashSet<T_Tile> views)
        {
            if (rewriting != null)
            {
                if (GetOpKind(rewriting) == TileOpKind.Named)
                {
                    views.Add(rewriting);
                }
                else
                {
                    GatherViews(GetArg1(rewriting), views);
                    GatherViews(GetArg2(rewriting), views);
                }
            }
        }

        public static IEnumerable<T> AllButOne<T>(IEnumerable<T> list, int toSkipPosition)
        {
            int valuePosition = 0;
            foreach (T value in list)
            {
                if (valuePosition++ != toSkipPosition)
                {
                    yield return value;
                }
            }
        }

        public static IEnumerable<T> Concat<T>(T value, IEnumerable<T> rest)
        {
            yield return value;
            foreach (T restValue in rest)
            {
                yield return restValue;
            }
        }

        public static IEnumerable<IEnumerable<T>> Permute<T>(IEnumerable<T> list)
        {
            IEnumerable<T> rest = null;
            int valuePosition = 0;
            foreach (T value in list)
            {
                rest = AllButOne<T>(list, valuePosition++);
                foreach (IEnumerable<T> restPermutation in Permute<T>(rest))
                {
                    yield return Concat<T>(value, restPermutation);
                }
            }
            if (rest == null)
            {
                yield return list; // list is empty enumeration
            }
        }

        static Random rnd = new Random(1507);
        public static List<T> RandomPermutation<T>(IEnumerable<T> input)
        {
            List<T> output = new List<T>(input);
            for (int i = 0; i < output.Count; i++)
            {
                int j = rnd.Next(output.Count);
                T tmp = output[i];
                output[i] = output[j];
                output[j] = tmp;
            }
            return output;
        }

        public static IEnumerable<T> Reverse<T>(IEnumerable<T> input, HashSet<T> filter)
        {
            List<T> output = new List<T>(input);
            output.Reverse();
            foreach (T t in output)
            {
                if (filter.Contains(t))
                {
                    yield return t;
                }
            }
        }

        public bool RewriteQuery(T_Tile toFill, T_Tile toAvoid, IEnumerable<T_Tile> views, out T_Tile rewriting)
        {
            if (RewriteQueryOnce(toFill, toAvoid, views, out rewriting))
            {
                HashSet<T_Tile> usedViews = new HashSet<T_Tile>();
                GatherViews(rewriting, usedViews);
                int opCount = CountOperators(rewriting);

                // try several permutations of views, pick one with fewer operators
                T_Tile newRewriting;
                int permuteTries = 0;
                int numPermutations = Math.Min(MAX_PERMUTATIONS, Math.Max(MIN_PERMUTATIONS, (int)(usedViews.Count * PERMUTE_FRACTION)));
                while (permuteTries++ < numPermutations)
                {
                    IEnumerable<T_Tile> permutedViews;
                    if (permuteTries == 1)
                    {
                        permutedViews = Reverse(views, usedViews);
                    }
                    else
                    {
                        permutedViews = RandomPermutation(usedViews); // Tradeoff: views vs. usedViews!
                    }
                    bool succeeded = RewriteQueryOnce(toFill, toAvoid, permutedViews, out newRewriting);
                    Debug.Assert(succeeded);
                    int newOpCount = CountOperators(newRewriting);
                    if (newOpCount < opCount)
                    {
                        opCount = newOpCount;
                        rewriting = newRewriting;
                    }
                    HashSet<T_Tile> newUsedViews = new HashSet<T_Tile>();
                    GatherViews(newRewriting, newUsedViews);
                    usedViews = newUsedViews; // can only be fewer!
                }
                return true;
            }
            return false;
        }

        public bool RewriteQueryOnce(T_Tile toFill, T_Tile toAvoid, IEnumerable<T_Tile> views, out T_Tile rewriting)
        {
            List<T_Tile> viewList = new List<T_Tile>(views);
            return RewritingPass<T_Tile>.RewriteQuery(toFill, toAvoid, out rewriting, viewList, this);
        }
    }
}
