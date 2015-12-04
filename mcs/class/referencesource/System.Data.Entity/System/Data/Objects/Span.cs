//---------------------------------------------------------------------
// <copyright file="Span.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Objects
{
    using System.Collections.Generic;
    using System.Data.Common.Internal;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// A collection of paths to determine which entities are spanned into a query.
    /// </summary>
    internal sealed class Span
    {
        private List<SpanPath> _spanList;
        private string _cacheKey;

        internal Span()
        {
            _spanList = new List<SpanPath>();
        }

        /// <summary>
        /// The list of paths that should be spanned into the query
        /// </summary>
        internal List<SpanPath> SpanList
        {
            get { return _spanList; }
        }

        /// <summary>
        /// Checks whether relationship span needs to be performed. Currently this is only when the query is
        /// not using MergeOption.NoTracking.
        /// </summary>
        /// <param name="mergeOption"></param>
        /// <returns>True if the query needs a relationship span rewrite</returns>
        internal static bool RequiresRelationshipSpan(MergeOption mergeOption)
        {
            return (mergeOption != MergeOption.NoTracking);
        }

        /// <summary>
        /// Includes the specified span path in the specified span instance and returns the updated span instance.
        /// If <paramref name="spanToIncludeIn"/> is null, a new span instance is constructed and returned that contains
        /// the specified include path.
        /// </summary>
        /// <param name="spanToIncludeIn">The span instance to which the include path should be added. May be null</param>
        /// <param name="pathToInclude">The include path to add</param>
        /// <returns>A non-null span instance that contains the specified include path in addition to any paths ut already contained</returns>
        internal static Span IncludeIn(Span spanToIncludeIn, string pathToInclude)
        {
            if (null == spanToIncludeIn)
            {
                spanToIncludeIn = new Span();
            }

            spanToIncludeIn.Include(pathToInclude);
            return spanToIncludeIn;
        }

        /// <summary>
        /// Returns a span instance that is the union of the two specified span instances.
        /// If <paramref name="span1"/> and <paramref name="span2"/> are both <c>null</c>,
        /// then <c>null</c> is returned.
        /// If <paramref name="span1"/> or <paramref name="span2"/> is null, but the remaining argument is non-null,
        /// then the non-null argument is returned.
        /// If neither <paramref name="span1"/> nor <paramref name="span2"/> are null, a new span instance is returned
        /// that contains the merged span paths from both.
        /// </summary>
        /// <param name="span1">The first span instance from which to include span paths; may be <c>null</c></param>
        /// <param name="span2">The second span instance from which to include span paths; may be <c>null</c></param>
        /// <returns>A span instance representing the union of the two arguments; may be <c>null</c> if both arguments are null</returns>
        internal static Span CopyUnion(Span span1, Span span2)
        {
            if (null == span1)
            {
                return span2;
            }

            if (null == span2)
            {
                return span1;
            }

            Span retSpan = span1.Clone();
            foreach (SpanPath path in span2.SpanList)
            {
                retSpan.AddSpanPath(path);
            }

            return retSpan;
        }

        internal string GetCacheKey()
        {
            if (null == _cacheKey)
            {
                if (_spanList.Count > 0)
                {
                    // If there is only a single Include path with a single property,
                    // then simply use the property name as the cache key rather than
                    // creating any new strings.
                    if (_spanList.Count == 1 &&
                       _spanList[0].Navigations.Count == 1)
                    {
                        _cacheKey = _spanList[0].Navigations[0];
                    }
                    else
                    {
                        StringBuilder keyBuilder = new StringBuilder();
                        for (int pathIdx = 0; pathIdx < _spanList.Count; pathIdx++)
                        {
                            if (pathIdx > 0)
                            {
                                keyBuilder.Append(";");
                            }

                            SpanPath thisPath = _spanList[pathIdx];
                            keyBuilder.Append(thisPath.Navigations[0]);
                            for (int propIdx = 1; propIdx < thisPath.Navigations.Count; propIdx++)
                            {
                                keyBuilder.Append(".");
                                keyBuilder.Append(thisPath.Navigations[propIdx]);
                            }
                        }

                        _cacheKey = keyBuilder.ToString();
                    }
                }
            }

            return _cacheKey;
        }

        /// <summary>
        /// Adds a path to span into the query.
        /// </summary>
        /// <param name="path">The path to span</param>
        public void Include(string path)
        {
            EntityUtil.CheckStringArgument(path, "path");
            if (path.Trim().Length == 0)
            {
                throw new ArgumentException(System.Data.Entity.Strings.ObjectQuery_Span_WhiteSpacePath, "path");
            }
            
            SpanPath spanPath = new SpanPath(ParsePath(path));
            AddSpanPath(spanPath);
            _cacheKey = null;
        }

        /// <summary>
        /// Creates a new Span with the same SpanPaths as this Span
        /// </summary>
        /// <returns></returns>
        internal Span Clone()
        {
            Span newSpan = new Span();
            newSpan.SpanList.AddRange(_spanList);
            newSpan._cacheKey = this._cacheKey;

            return newSpan;
        }

        /// <summary>
        /// Adds the path if it does not already exist
        /// </summary>
        /// <param name="spanPath"></param>
        internal void AddSpanPath(SpanPath spanPath)
        {
            if (ValidateSpanPath(spanPath))
            {
                RemoveExistingSubPaths(spanPath);
                _spanList.Add(spanPath);
            }
        }

        /// <summary>
        /// Returns true if the path can be added
        /// </summary>
        /// <param name="spanPath"></param>
        private bool ValidateSpanPath(SpanPath spanPath)
        {

            // Check for dupliacte entries
            for (int i = 0; i < _spanList.Count; i++)
            { 
                // make sure spanPath is not a sub-path of anything already in the list
                if (spanPath.IsSubPath(_spanList[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private void RemoveExistingSubPaths(SpanPath spanPath)
        {
            List<SpanPath> toDelete = new List<SpanPath>();
            for (int i = 0; i < _spanList.Count; i++)
            {
                // make sure spanPath is not a sub-path of anything already in the list
                if (_spanList[i].IsSubPath(spanPath))
                {
                    toDelete.Add(_spanList[i]);
                }
            }

            foreach (SpanPath path in toDelete)
            {
                _spanList.Remove(path);
            }
        }

        /// <summary>
        /// Storage for a span path
        /// Currently this includes the list of navigation properties
        /// </summary>
        internal class SpanPath
        {
            public readonly List<string> Navigations;

            public SpanPath(List<string> navigations)
            {
                Navigations = navigations;
            }

            public bool IsSubPath(SpanPath rhs)
            {
                // this is a subpath of rhs if it has fewer paths, and all the path element values are equal
                if (Navigations.Count > rhs.Navigations.Count)
                {
                    return false;
                }

                for (int i = 0; i < Navigations.Count; i++)
                {
                    if (!Navigations[i].Equals(rhs.Navigations[i], StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private static List<string> ParsePath(string path)
        {
            List<string> navigations = MultipartIdentifier.ParseMultipartIdentifier(path, "[", "]", '.');

            for (int i = navigations.Count - 1; i >= 0; i--)
            {
                if (navigations[i] == null)
                {
                    navigations.RemoveAt(i);
                }
                else if (navigations[i].Length == 0)
                {
                    throw EntityUtil.SpanPathSyntaxError();
                }
            }

            Debug.Assert(navigations.Count > 0, "Empty path found");
            return navigations;
        }
    
    }
}
