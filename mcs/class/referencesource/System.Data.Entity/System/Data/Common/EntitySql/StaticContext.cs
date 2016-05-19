//---------------------------------------------------------------------
// <copyright file="StaticContext.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Common.EntitySql
{
    using System.Collections.Generic;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity;
    using System.Diagnostics;

    /// <summary>
    /// Represents a scope of key-value pairs.
    /// </summary>
    internal sealed class Scope : IEnumerable<KeyValuePair<string, ScopeEntry>>
    {
        private readonly Dictionary<string, ScopeEntry> _scopeEntries;

        /// <summary>
        /// Initialize using a given key comparer.
        /// </summary>
        /// <param name="keyComparer"></param>
        internal Scope(IEqualityComparer<string> keyComparer)
        {
            _scopeEntries = new Dictionary<string, ScopeEntry>(keyComparer);
        }

        /// <summary>
        /// Add new key to the scope. If key already exists - throw.
        /// </summary>
        internal Scope Add(string key, ScopeEntry value)
        {
            _scopeEntries.Add(key, value);
            return this;
        }

        /// <summary>
        /// Remove an entry from the scope.
        /// </summary>
        internal void Remove(string key)
        {
            Debug.Assert(Contains(key));
            _scopeEntries.Remove(key);
        }

        internal void Replace(string key, ScopeEntry value)
        {
            Debug.Assert(Contains(key));
            _scopeEntries[key] = value;
        }

        /// <summary>
        /// Returns true if the key belongs to the scope.
        /// </summary>
        internal bool Contains(string key)
        {
            return _scopeEntries.ContainsKey(key);
        }

        /// <summary>
        /// Search item by key. Returns true in case of success and false otherwise.
        /// </summary>
        internal bool TryLookup(string key, out ScopeEntry value)
        {
            return (_scopeEntries.TryGetValue(key, out value));
        }

        #region GetEnumerator
        public Dictionary<string, ScopeEntry>.Enumerator GetEnumerator()
        {
            return _scopeEntries.GetEnumerator();
        }

        System.Collections.Generic.IEnumerator<KeyValuePair<string, ScopeEntry>> System.Collections.Generic.IEnumerable<KeyValuePair<string, ScopeEntry>>.GetEnumerator()
        {
            return _scopeEntries.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _scopeEntries.GetEnumerator();
        }
        #endregion
    }

    internal enum ScopeEntryKind
    {
        SourceVar,
        GroupKeyDefinition,
        ProjectionItemDefinition,
        FreeVar,
        /// <summary>
        /// Represents a group input scope entry that should no longer be referenced. 
        /// </summary>
        InvalidGroupInputRef
    }

    /// <summary>
    /// Represents an entry in the scope.
    /// </summary>
    internal abstract class ScopeEntry
    {
        private readonly ScopeEntryKind _scopeEntryKind;

        internal ScopeEntry(ScopeEntryKind scopeEntryKind)
        {
            _scopeEntryKind = scopeEntryKind;
        }

        internal ScopeEntryKind EntryKind
        {
            get { return _scopeEntryKind; }
        }

        /// <summary>
        /// Returns CQT expression corresponding to the scope entry.
        /// </summary>
        internal abstract DbExpression GetExpression(string refName, ErrorContext errCtx);
    }

    internal interface IGroupExpressionExtendedInfo
    {
        /// <summary>
        /// Returns <see cref="DbGroupExpressionBinding.GroupVariable"/> based expression during the <see cref="DbGroupByExpression"/> construction process, otherwise null.
        /// </summary>
        DbExpression GroupVarBasedExpression { get; }

        /// <summary>
        /// Returns <see cref="DbGroupAggregate"/> based expression during the <see cref="DbGroupByExpression"/> construction process, otherwise null.
        /// </summary>
        DbExpression GroupAggBasedExpression { get; }
    }

    internal interface IGetAlternativeName
    {
        /// <summary>
        /// If current scope entry reperesents an alternative group key name (see SemanticAnalyzer.ProcessGroupByClause(...) for more info)
        /// then this property returns the alternative name, otherwise null.
        /// </summary>
        string[] AlternativeName { get; }
    }

    /// <summary>
    /// Represents simple source var scope entry.
    /// </summary>
    internal sealed class SourceScopeEntry : ScopeEntry, IGroupExpressionExtendedInfo, IGetAlternativeName
    {
        private readonly string[] _alternativeName;
        private List<string> _propRefs;
        private DbExpression _varBasedExpression;
        private DbExpression _groupVarBasedExpression;
        private DbExpression _groupAggBasedExpression;
        private bool _joinClauseLeftExpr = false;

        internal SourceScopeEntry(DbVariableReferenceExpression varRef) : this(varRef, null) { }

        internal SourceScopeEntry(DbVariableReferenceExpression varRef, string[] alternativeName)
            : base(ScopeEntryKind.SourceVar)
        {
            _varBasedExpression = varRef;
            _alternativeName = alternativeName;
        }

        internal override DbExpression GetExpression(string refName, ErrorContext errCtx)
        {
            return _varBasedExpression;
        }

        DbExpression IGroupExpressionExtendedInfo.GroupVarBasedExpression
        {
            get { return _groupVarBasedExpression; }
        }

        DbExpression IGroupExpressionExtendedInfo.GroupAggBasedExpression
        {
            get { return _groupAggBasedExpression; }
        }

        internal bool IsJoinClauseLeftExpr
        {
            get { return _joinClauseLeftExpr; }
            set { _joinClauseLeftExpr = value; }
        }

        string[] IGetAlternativeName.AlternativeName
        {
            get { return _alternativeName; }
        }

        /// <summary>
        /// Prepend <paramref name="parentVarRef"/> to the property chain.
        /// </summary>
        internal SourceScopeEntry AddParentVar(DbVariableReferenceExpression parentVarRef)
        {
            //
            // No parent var adjustment is allowed while adjusted to group var (see AdjustToGroupVar(...) for more info).
            //
            Debug.Assert(_groupVarBasedExpression == null, "_groupVarBasedExpression == null");
            Debug.Assert(_groupAggBasedExpression == null, "_groupAggBasedExpression == null");

            if (_propRefs == null)
            {
                Debug.Assert(_varBasedExpression is DbVariableReferenceExpression, "_varBasedExpression is DbVariableReferenceExpression");
                _propRefs = new List<string>(2);
                _propRefs.Add(((DbVariableReferenceExpression)_varBasedExpression).VariableName);
            }
            
            _varBasedExpression = parentVarRef;
            for (int i = _propRefs.Count - 1; i >= 0; --i)
            {
                _varBasedExpression = _varBasedExpression.Property(_propRefs[i]);
            }
            _propRefs.Add(parentVarRef.VariableName);

            return this;
        }

        /// <summary>
        /// Replace existing var at the head of the property chain with the new <paramref name="parentVarRef"/>.
        /// </summary>
        internal void ReplaceParentVar(DbVariableReferenceExpression parentVarRef)
        {
            //
            // No parent var adjustment is allowed while adjusted to group var (see AdjustToGroupVar(...) for more info).
            //
            Debug.Assert(_groupVarBasedExpression == null, "_groupVarBasedExpression == null");
            Debug.Assert(_groupAggBasedExpression == null, "_groupAggBasedExpression == null");

            if (_propRefs == null)
            {
                Debug.Assert(_varBasedExpression is DbVariableReferenceExpression, "_varBasedExpression is DbVariableReferenceExpression");
                _varBasedExpression = parentVarRef;
            }
            else
            {
                Debug.Assert(_propRefs.Count > 0, "_propRefs.Count > 0");
                _propRefs.RemoveAt(_propRefs.Count - 1);
                AddParentVar(parentVarRef);
            }
        }

        /// <summary>
        /// Rebuild the current scope entry expression as the property chain off the <paramref name="parentVarRef"/> expression.
        /// Also build 
        ///     - <see cref="IGroupExpressionExtendedInfo.GroupVarBasedExpression"/> off the <paramref name="parentGroupVarRef"/> expression;
        ///     - <see cref="IGroupExpressionExtendedInfo.GroupAggBasedExpression"/> off the <paramref name="groupAggRef"/> expression.
        /// This adjustment is reversable by <see cref="RollbackAdjustmentToGroupVar"/>(...).
        /// </summary>
        internal void AdjustToGroupVar(DbVariableReferenceExpression parentVarRef, DbVariableReferenceExpression parentGroupVarRef, DbVariableReferenceExpression groupAggRef)
        {
            // Adjustment is not reentrant.
            Debug.Assert(_groupVarBasedExpression == null, "_groupVarBasedExpression == null");
            Debug.Assert(_groupAggBasedExpression == null, "_groupAggBasedExpression == null");

            //
            // Let's assume this entry represents variable "x" in the following query:
            //      select x, y, z from {1, 2} as x join {2, 3} as y on x = y join {3, 4} as z on y = z
            // In this case _propRefs contains x._##join0._##join1 and the corresponding input expression looks like this:
            //     |_Input : '_##join1'
            //     | |_InnerJoin
            //     |   |_Left : '_##join0'
            //     |   | |_InnerJoin
            //     |   |   |_Left : 'x'
            //     |   |   |_Right : 'y'
            //     |   |_Right : 'z'
            // When we start processing a group by, like in this query:
            //      select k1, k2, k3 from {1, 2} as x join {2, 3} as y on x = y join {3, 4} as z on y = z group by x as k1, y as k2, z as k3
            // we are switching to the following input expression:
            //     |_Input : '_##geb2', '_##group3'
            //     | |_InnerJoin
            //     |   |_Left : '_##join0'
            //     |   | |_InnerJoin
            //     |   |   |_Left : 'x'
            //     |   |   |_Right : 'y'
            //     |   |_Right : 'z'
            // where _##join1 is replaced by _##geb2 for the regular expression and by _##group3 for the group var based expression.
            // So the switch, or the adjustment, is done by 
            //      a. replacing _##join1 with _##geb2 in _propRefs and rebuilding the regular expression accordingly to get
            //         the following property chain: _##geb2._##join1.x
            //      b. building a group var based expression using _##group3 instead of _##geb2 to get
            //         the following property chain: _##group3._##join1.x
            //

            //
            // Rebuild ScopeEntry.Expression using the new parent var.
            //
            ReplaceParentVar(parentVarRef);

            //
            // Build the GroupVarBasedExpression and GroupAggBasedExpression, 
            // take into account that parentVarRef has already been added to the _propRefs in the AdjustToParentVar(...) call, so ignore it.
            //
            _groupVarBasedExpression = parentGroupVarRef;
            _groupAggBasedExpression = groupAggRef;
            if (_propRefs != null)
            {
                for (int i = _propRefs.Count - 2/*ignore the parentVarRef*/; i >= 0; --i)
                {
                    _groupVarBasedExpression = _groupVarBasedExpression.Property(_propRefs[i]);
                    _groupAggBasedExpression = _groupAggBasedExpression.Property(_propRefs[i]);
                }
            }
        }

        /// <summary>
        /// Rolls back the <see cref="AdjustToGroupVar"/>(...) adjustment, clears the <see cref="IGroupExpressionExtendedInfo.GroupVarBasedExpression"/>.
        /// </summary>
        internal void RollbackAdjustmentToGroupVar(DbVariableReferenceExpression pregroupParentVarRef)
        {
            Debug.Assert(_groupVarBasedExpression != null, "_groupVarBasedExpression != null");

            _groupVarBasedExpression = null;
            _groupAggBasedExpression = null;
            ReplaceParentVar(pregroupParentVarRef);
        }
    }

    /// <summary>
    /// Represents a group input scope entry that should no longer be referenced. 
    /// </summary>
    internal sealed class InvalidGroupInputRefScopeEntry : ScopeEntry
    {
        internal InvalidGroupInputRefScopeEntry()
            : base(ScopeEntryKind.InvalidGroupInputRef) { }

        internal override DbExpression GetExpression(string refName, ErrorContext errCtx)
        {
            throw EntityUtil.EntitySqlError(errCtx, Strings.InvalidGroupIdentifierReference(refName));
        }
    }

    /// <summary>
    /// Represents group key during GROUP BY clause processing phase, used during group aggregate search mode.
    /// This entry will be replaced by the <see cref="SourceScopeEntry"/> when GROUP BY processing is complete.
    /// </summary>
    internal sealed class GroupKeyDefinitionScopeEntry : ScopeEntry, IGroupExpressionExtendedInfo, IGetAlternativeName
    {
        private readonly DbExpression _varBasedExpression;
        private readonly DbExpression _groupVarBasedExpression;
        private readonly DbExpression _groupAggBasedExpression; 
        private readonly string[] _alternativeName;

        internal GroupKeyDefinitionScopeEntry(
            DbExpression varBasedExpression, 
            DbExpression groupVarBasedExpression, DbExpression 
            groupAggBasedExpression, 
            string[] alternativeName) : base(ScopeEntryKind.GroupKeyDefinition)
        {
            _varBasedExpression = varBasedExpression;
            _groupVarBasedExpression = groupVarBasedExpression;
            _groupAggBasedExpression = groupAggBasedExpression;
            _alternativeName = alternativeName;
        }

        internal override DbExpression GetExpression(string refName, ErrorContext errCtx)
        {
            return _varBasedExpression;
        }

        DbExpression IGroupExpressionExtendedInfo.GroupVarBasedExpression
        {
            get
            {
                return _groupVarBasedExpression;
            }
        }

        DbExpression IGroupExpressionExtendedInfo.GroupAggBasedExpression
        {
            get { return _groupAggBasedExpression; }
        }

        string[] IGetAlternativeName.AlternativeName
        {
            get { return _alternativeName; }
        }
    }

    /// <summary>
    /// Represents a projection item definition scope entry.
    /// </summary>
    internal sealed class ProjectionItemDefinitionScopeEntry : ScopeEntry
    {
        private readonly DbExpression _expression;

        internal ProjectionItemDefinitionScopeEntry(DbExpression expression)
            : base(ScopeEntryKind.ProjectionItemDefinition)
        {
            _expression = expression;
        }

        internal override DbExpression GetExpression(string refName, ErrorContext errCtx)
        {
            return _expression;
        }
    }

    /// <summary>
    /// Represents a free variable scope entry. 
    /// Example: parameters of an inline function definition are free variables in the scope of the function definition.
    /// </summary>
    internal sealed class FreeVariableScopeEntry : ScopeEntry
    {
        private readonly DbVariableReferenceExpression _varRef;

        internal FreeVariableScopeEntry(DbVariableReferenceExpression varRef)
            : base(ScopeEntryKind.FreeVar)
        {
            _varRef = varRef;
        }

        internal override DbExpression GetExpression(string refName, ErrorContext errCtx)
        {
            return _varRef;
        }
    }

    /// <summary>
    /// Represents a generic list of scopes.
    /// </summary>
    internal sealed class ScopeManager
    {
        private readonly IEqualityComparer<string> _keyComparer;
        private readonly List<Scope> _scopes = new List<Scope>();

        /// <summary>
        /// Initialize scope manager using given key-string comparer.
        /// </summary>
        internal ScopeManager(IEqualityComparer<string> keyComparer)
        {
            _keyComparer = keyComparer;
        }

        /// <summary>
        /// Enter a new scope.
        /// </summary>
        internal void EnterScope()
        {
            _scopes.Add(new Scope(_keyComparer));
        }

        /// <summary>
        /// Leave the current scope.
        /// </summary>
        internal void LeaveScope()
        {
            Debug.Assert(CurrentScopeIndex >= 0);
            _scopes.RemoveAt(CurrentScopeIndex);
        }

        /// <summary>
        /// Return current scope index.
        /// Outer scopes have smaller index values than inner scopes.
        /// </summary>
        internal int CurrentScopeIndex
        {
            get
            {
                return _scopes.Count - 1;
            }
        }

        /// <summary>
        /// Return current scope.
        /// </summary>
        internal Scope CurrentScope
        {
            get
            {
                return _scopes[CurrentScopeIndex];
            }
        }

        /// <summary>
        /// Get a scope by the index.
        /// </summary>
        internal Scope GetScopeByIndex(int scopeIndex)
        {
            Debug.Assert(scopeIndex >= 0, "scopeIndex >= 0");
            Debug.Assert(scopeIndex <= CurrentScopeIndex, "scopeIndex <= CurrentScopeIndex");
            if (0 > scopeIndex || scopeIndex > CurrentScopeIndex)
            {
                throw EntityUtil.EntitySqlError(Strings.InvalidScopeIndex);
            }
            return _scopes[scopeIndex];
        }

        /// <summary>
        /// Rollback all scopes to the scope at the index.
        /// </summary>
        internal void RollbackToScope(int scopeIndex)
        {
            //
            // assert preconditions
            //
            Debug.Assert(scopeIndex >= 0, "[PRE] savePoint.ScopeIndex >= 0");
            Debug.Assert(scopeIndex <= CurrentScopeIndex, "[PRE] savePoint.ScopeIndex <= CurrentScopeIndex");
            Debug.Assert(CurrentScopeIndex >= 0, "[PRE] CurrentScopeIndex >= 0");

            if (scopeIndex > CurrentScopeIndex || scopeIndex < 0 || CurrentScopeIndex < 0)
            {
                throw EntityUtil.EntitySqlError(Strings.InvalidSavePoint);
            }

            int delta = CurrentScopeIndex - scopeIndex;
            if (delta > 0)
            {
                _scopes.RemoveRange(scopeIndex + 1, CurrentScopeIndex - scopeIndex);
            }

            //
            // make sure invariants are preserved
            //
            Debug.Assert(scopeIndex == CurrentScopeIndex, "[POST] savePoint.ScopeIndex == CurrentScopeIndex");
            Debug.Assert(CurrentScopeIndex >= 0, "[POST] CurrentScopeIndex >= 0");

        }

        /// <summary>
        /// True if key exists in current scope.
        /// </summary>
        internal bool IsInCurrentScope(string key)
        {
            return CurrentScope.Contains(key);
        }
    }
}
