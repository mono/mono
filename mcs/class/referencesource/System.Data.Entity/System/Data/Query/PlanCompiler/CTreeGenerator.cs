//---------------------------------------------------------------------
// <copyright file="CTreeGenerator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

//using System.Diagnostics; // Please use PlanCompiler.Assert instead of Debug.Assert in this class...

// It is fine to use Debug.Assert in cases where you assert an obvious thing that is supposed
// to prevent from simple mistakes during development (e.g. method argument validation 
// in cases where it was you who created the variables or the variables had already been validated or 
// in "else" clauses where due to code changes (e.g. adding a new value to an enum type) the default 
// "else" block is chosen why the new condition should be treated separately). This kind of asserts are 
// (can be) helpful when developing new code to avoid simple mistakes but have no or little value in 
// the shipped product. 
// PlanCompiler.Assert *MUST* be used to verify conditions in the trees. These would be assumptions 
// about how the tree was built etc. - in these cases we probably want to throw an exception (this is
// what PlanCompiler.Assert does when the condition is not met) if either the assumption is not correct 
// or the tree was built/rewritten not the way we thought it was.
// Use your judgment - if you rather remove an assert than ship it use Debug.Assert otherwise use
// PlanCompiler.Assert.


namespace System.Data.Query.PlanCompiler
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Common.Utils;
    using System.Data.Metadata.Edm;
    using System.Data.Query.InternalTrees;
    using System.Globalization;

    internal class CTreeGenerator : BasicOpVisitorOfT<DbExpression>
    {
        #region Nested Types

        /// <summary>
        /// The VarInfo class tracks how a single IQT Var should be referenced in terms of CQT Expressions.
        /// The tracked Var must have been introduced by an IQT RelOp that was converted to a DbExpression that
        /// is subsequently used in a DbExpressionBinding, otherwise the Var is either a ParameterVar or a locally
        /// defined Var, which are tracked by the parameters collection of the Command and the VarDefScope
        /// class, respectively.
        /// An IQT Var that is tracked by a VarInfo instance is reachable in the following way:
        /// 1. By a DbVariableReferenceExpression that references the Variable of the DbExpressionBinding that contains the DbExpression that logically publishes the IQT Var.
        ///     This is tracked by the PublisherName property of the RelOpInfo class, which is used to track Vars brought into scope by a DbExpressionBinding.
        ///     Without an enclosing RelOpInfo, the VarInfo is unbound and cannot be used to instantiate a CQT expression tree that is the equivalent of a VarRef of the IQT Var)
        /// 2. By zero or more PropertyRefExpressions starting with a property of the DbVariableReferenceExpression created in step 1.
        ///     These PropertyRefExpressions are introduced on top of the DbVariableReferenceExpression because of Join or ApplyExpressions that
        ///     occur in the CQT between the expression that publishes the Var and the expression higher in the tree that contains a VarRefOp
        ///     to the IQT Var that must be resolved to a CQT DbExpression. In such cases the DbExpression that logically publishes
        ///     the IQT Var will have a record return Type.
        ///     The required property names are tracked, in order, in the PropertyPath property of this class.
        /// The PrependProperty method is used to update the DbPropertyExpression path required to reach
        /// the DbVariableReferenceExpression when the referenced Variable becomes part of such a record-typed output.
        /// </summary>
        private class VarInfo
        {
            #region Private Member Variables
            private Var _var;
            private List<string> _propertyChain = new List<string>();
            #endregion

            /// <summary>
            /// Gets the Var tracked by this VarInfo instance
            /// </summary>
            internal Var Var { get { return _var; } }

            /// <summary>
            /// Gets the names, in order of use, that should be used to build DbPropertyExpression around an initial DbVariableReferenceExpression in order to build a DbExpression subtree that correctly references the tracked IQT Var
            /// </summary>
            internal List<string> PropertyPath { get { return _propertyChain; } }

            /// <summary>
            /// Constructs a new VarInfo instance that tracks the specified Var.
            /// </summary>
            /// <param name="target">The IQT Var that this VarInfo instance should track.</param>
            internal VarInfo(Var target)
            {
                _var = target;
            }

            /// <summary>
            /// Adds a property name to the beginning of the property path for this VarInfo instance.
            /// Each time a new record structure is constructed on top of the expression that logically
            /// publishes this var, another DbPropertyExpression is required around the DbVariableReferenceExpression used
            /// to reach the Var in the CQT. Each new DbPropertyExpression must be added immediately around the
            /// DbVariableReferenceExpression, with previous PropertyExpressions now referring to the new DbPropertyExpression.
            /// Therefore the new property name added by this method is inserted at the start of the property path.
            /// See the Visit methods for the Join/ApplyOps for examples of using this method to adjust the property path.
            /// </summary>
            /// <param name="propName">The new property name to insert at the start of the property path for the Var tracked by this VarInfo instance</param>
            internal void PrependProperty(string propName)
            {
                _propertyChain.Insert(0, propName);
            }
        }

        /// <summary>
        /// Groups a set of VarInfo instances together and allows certain operations (Bind/Unbind/PrependProperty)
        /// to be performed on all instances in the VarInfoList with a single call.
        /// </summary>
        private class VarInfoList : List<VarInfo>
        {
            /// <summary>
            /// Constructs a new, empty VarInfoList.
            /// </summary>
            internal VarInfoList() : base() { }

            /// <summary>
            /// Constructs a new VarInfoList that contains the specified VarInfo instances.
            /// </summary>
            /// <param name="elements"></param>
            internal VarInfoList(IEnumerable<VarInfo> elements) : base(elements) { }

            /// <summary>
            /// Prepends the specified property name to the property path of all VarInfo instances in this list.
            /// </summary>
            /// <param name="propName"></param>
            internal void PrependProperty(string propName)
            {
                foreach (VarInfo vInf in this)
                {
                    vInf.PropertyPath.Insert(0, propName);
                }
            }

            /// <summary>
            /// Attempts to retrieve the VarInfo instance that tracks the specified IQT Var, if it is contained by this VarInfoList.
            /// </summary>
            /// <param name="targetVar">The required IQT Var</param>
            /// <param name="varInfo">Contains the VarInfo instance that tracks the specified Var if this method returns true</param>
            /// <returns>True if this list contains a VarInfo instance that tracks the specified Var; otherwise false</returns>
            internal bool TryGetInfo(Var targetVar, out VarInfo varInfo)
            {
                varInfo = null;
                foreach (VarInfo info in this)
                {
                    if (info.Var == targetVar)
                    {
                        varInfo = info;
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// IqtVarScope is used to represent one or more IQT Vars that are currently in scope and can be mapped to a corresponding CQT DbExpression subtree.
        /// </summary>
        private abstract class IqtVarScope
        {
            /// <summary>
            /// Attempts to resolve the specified IQT Var by building or mapping to a CQT DbExpression subtree. Overridden in derived classes.
            /// </summary>
            /// <param name="targetVar">The IQT Var to resolve</param>
            /// <param name="resultExpr">If the methods returns true, the DbExpression to which the Var was resolved; otherwise null</param>
            /// <returns>True if the specified Var was successfully resolved; otherwise false</returns>
            internal abstract bool TryResolveVar(Var targetVar, out DbExpression resultExpr);
        }

        private abstract class BindingScope : IqtVarScope
        {
            private readonly VarInfoList _definedVars;

            internal BindingScope(IEnumerable<VarInfo> boundVars)
            {
                _definedVars = new VarInfoList(boundVars);
            }

            /// <summary>
            /// Information (current binding name, property path) about the Vars logically published by the Publisher expression
            /// </summary>
            internal VarInfoList PublishedVars { get { return _definedVars; } }
            
            /// <summary>
            /// Implements the abstract IqtVarScope.TryResolveVar method. If the specified Var was published by this scope's DbExpression, it is mapped to a CQT DbExpression by calling CreateExpression on the VarInfo used to track it.
            /// </summary>
            /// <param name="targetVar">The Var to resolve</param>
            /// <param name="resultExpr">If the method returns true, the DbExpression to which the Var was resolved; otherwise null</param>
            /// <returns>True if the specified Var was successfully resolved; otherwise false</returns>
            internal override bool TryResolveVar(Var targetVar, out DbExpression resultExpr)
            {
                resultExpr = null;
                VarInfo foundInfo = null;
                if (_definedVars.TryGetInfo(targetVar, out foundInfo))
                {
                    resultExpr = this.BindingReference;
                    foreach (string propName in foundInfo.PropertyPath)
                    {
                        resultExpr = resultExpr.Property(propName);
                    }

                    return true;
                }

                return false;
            }

            protected abstract DbVariableReferenceExpression BindingReference { get; }
        }

        /// <summary>
        /// Represents a collection of IQT Vars that were brought into scope by a DbExpression used in a DbExpressionBinding. This class is also used to associate those Vars with that DbExpression, which is considered the logical 'publisher' of the Vars.
        /// </summary>
        private class RelOpInfo : BindingScope
        {
            private readonly DbExpressionBinding _binding;
            
            internal RelOpInfo(string bindingName, DbExpression publisher, IEnumerable<VarInfo> publishedVars)
                : base(publishedVars)
            {
                PlanCompiler.Assert(TypeSemantics.IsCollectionType(publisher.ResultType), "non-collection type used as RelOpInfo publisher");
                _binding = publisher.BindAs(bindingName);
            }

            /// <summary>
            /// The unique name assigned to the CQT DbExpression that logically publishes the PublishedVars. Used primarily in ExpressionBindings that contain that DbExpression
            /// </summary>
            internal string PublisherName
            {
                get { return _binding.VariableName; }
            }

            /// <summary>
            /// The CQT DbExpression that logically publishes the PublishedVars
            /// </summary>
            internal DbExpression Publisher { get { return _binding.Expression; } }
                        
            /// <summary>
            /// Creates a new DbExpressionBinding that binds the publisher DbExpression under the binding name
            /// </summary>
            /// <returns>The new DbExpressionBinding</returns>
            internal DbExpressionBinding CreateBinding()
            {
                return _binding;
            }

            protected override DbVariableReferenceExpression BindingReference
            {
                get { return _binding.Variable; }
            }
        }

        /// <summary>
        /// Represents a collection of IQT Vars that were brought into scope by a DbExpression used in a DbGroupExpressionBinding. 
        /// </summary>
        private class GroupByScope : BindingScope
        {
            private readonly DbGroupExpressionBinding _binding;
            private bool _referenceGroup;

            internal GroupByScope(DbGroupExpressionBinding binding, IEnumerable<VarInfo> publishedVars)
                : base(publishedVars)
            {
                _binding = binding;
            }
                
            /// <summary>
            /// Returns the DbGroupExpressionBinding that backs this group-by scope
            /// </summary>
            /// <returns>The new DbExpressionBinding</returns>
            internal DbGroupExpressionBinding Binding { get { return _binding; } }

            internal void SwitchToGroupReference()
            {
                PlanCompiler.Assert(!_referenceGroup, "SwitchToGroupReference called more than once on the same GroupByScope?");
                _referenceGroup = true;
            }

            protected override DbVariableReferenceExpression BindingReference
            {
                get { return (_referenceGroup ? _binding.GroupVariable : _binding.Variable); }
            }
        }

        /// <summary>
        /// Represents a collection of IQT Vars that are in scope because they are defined locally (by VarDefOps) to an IQT Op that is being visited.
        /// </summary>
        private class VarDefScope : IqtVarScope
        {
            private Dictionary<Var, DbExpression> _definedVars;

            internal VarDefScope(Dictionary<Var, DbExpression> definedVars)
            {
                _definedVars = definedVars;
            }

            /// <summary>
            /// Implements the abstract IqtVarScope.TryResolveVar method. If the specified Var exists in this scope, it is resolved by mapping it to the DbExpression that was produced by converting the IQT child Node of the VarDefOp that defines it to a CQT DbExpression subtree.
            /// </summary>
            /// <param name="targetVar">The Var to resolve</param>
            /// <param name="resultExpr">If the method returns true, the DbExpression to which the Var was resolved; otherwise null</param>
            /// <returns>True if the specified Var was successfully resolved; otherwise false</returns>
            internal override bool TryResolveVar(Var targetVar, out DbExpression resultExpr)
            {
                resultExpr = null;
                DbExpression foundExpr = null;
                if (_definedVars.TryGetValue(targetVar, out foundExpr))
                {
                    resultExpr = foundExpr;
                    return true;
                }

                return false;
            }
        }

        #endregion

        #region Private Instance Members

        private Command _iqtCommand;
        private DbQueryCommandTree _queryTree;
        private Dictionary<ParameterVar, DbParameterReferenceExpression> _addedParams = new Dictionary<ParameterVar, DbParameterReferenceExpression>();
        private Stack<IqtVarScope> _bindingScopes = new Stack<IqtVarScope>();
        private Stack<VarDefScope> _varScopes = new Stack<VarDefScope>();
        private Dictionary<DbExpression, RelOpInfo> _relOpState = new Dictionary<DbExpression, RelOpInfo>();

        private AliasGenerator _applyAliases = new AliasGenerator("Apply");
        private AliasGenerator _distinctAliases = new AliasGenerator("Distinct");
        private AliasGenerator _exceptAliases = new AliasGenerator("Except");
        private AliasGenerator _extentAliases = new AliasGenerator("Extent");
        private AliasGenerator _filterAliases = new AliasGenerator("Filter");
        private AliasGenerator _groupByAliases = new AliasGenerator("GroupBy");
        private AliasGenerator _intersectAliases = new AliasGenerator("Intersect");
        private AliasGenerator _joinAliases = new AliasGenerator("Join");
        private AliasGenerator _projectAliases = new AliasGenerator("Project");
        private AliasGenerator _sortAliases = new AliasGenerator("Sort");
        private AliasGenerator _unionAllAliases = new AliasGenerator("UnionAll");
        private AliasGenerator _elementAliases = new AliasGenerator("Element");
        private AliasGenerator _singleRowTableAliases = new AliasGenerator("SingleRowTable");
        private AliasGenerator _limitAliases = new AliasGenerator("Limit");
        private AliasGenerator _skipAliases = new AliasGenerator("Skip");

        #endregion

        #region (pseudo) Public API
        internal static DbCommandTree Generate(Command itree, Node toConvert)
        {
            CTreeGenerator treeGenerator = new CTreeGenerator(itree, toConvert);
            return treeGenerator._queryTree;
        }
        #endregion

        #region Constructors (private)
        private CTreeGenerator(Command itree, Node toConvert)
        {
            _iqtCommand = itree;
            DbExpression queryExpression = VisitNode(toConvert);
            _queryTree = DbQueryCommandTree.FromValidExpression(itree.MetadataWorkspace, DataSpace.SSpace, queryExpression);
        }
        #endregion

        #region RelOp Helpers and PublishedVar State Maintenance

        /// <summary>
        /// Asserts that the specified DbExpression is a 'RelOp' DbExpression, i.e. it is considered the publisher of one or more (IQT) RelVars.
        /// </summary>
        /// <param name="expr">The DbExpression on which to Assert</param>
        private void AssertRelOp(DbExpression expr)
        {
            PlanCompiler.Assert(_relOpState.ContainsKey(expr),"not a relOp expression?");
        }

        /// <summary>
        /// Update the DbExpression to RelOpInfo map to indicate that the specified DbExpression logically publishes the Vars
        /// tracked in VarInfoList and that they should be bound under the specified name.
        /// </summary>
        /// <param name="name">The name under which the Vars tracked in VarInfoList are initially considered bound. This will be a unique name based on what kind of RelOp the specified DbExpression (the publisher) corresponds to</param>
        /// <param name="expr">The DbExpression that is considered the logical publisher of the Vars tracked in publishedVars</param>
        /// <param name="publishedVars">A VarInfoList that contains VarInfo instances that track the IQT Vars that are logically published by the specified DbExpression</param>
        /// <returns>A new RelOpInfo instance that associates the given binding name and published Vars with the specified DbExpression. This RelOpInfo is also added to the DbExpression to RelOpInfo map</returns>
        private RelOpInfo PublishRelOp(string name, DbExpression expr, VarInfoList publishedVars)
        {
            RelOpInfo retInfo = new RelOpInfo(name, expr, publishedVars);
            _relOpState.Add(expr, retInfo);
            return retInfo;
        }

        /// <summary>
        /// Removes an entry in the DbExpression to RelOpInfo map, 'consuming' it so that it is not visible higher in the converted CQT.
        /// </summary>
        /// <param name="expr">The DbExpression for which the corresponding RelOpEntry should be removed</param>
        /// <returns>The RelOpInfo that was removed from the DbExpression to RelOpInfo map</returns>
        private RelOpInfo ConsumeRelOp(DbExpression expr)
        {
            AssertRelOp(expr);
            RelOpInfo retInfo = _relOpState[expr];
            _relOpState.Remove(expr);
            return retInfo;
        }

        private RelOpInfo VisitAsRelOp(Node inputNode)
        {
            // Assert that the Op is actually a RelOp before attempting to use it
            PlanCompiler.Assert(inputNode.Op is RelOp, "Non-RelOp used as DbExpressionBinding Input");

            //
            // Visit the Op. This Visit method of this class that actually processes the Op will
            // publish the Vars produced by the resulting DbExpression in the DbExpression to RelOpInfo
            // map, then return that DbExpression.
            //
            DbExpression inputExpr = VisitNode(inputNode);

            //
            // Retrieve the RelOpInfo for the DbExpression, that was published as part of the above call.
            // ConsumeRelOp is called to both retrieve and remove the RelOpInfo instance since it is being
            // used here in a DbExpressionBinding.
            //
            return ConsumeRelOp(inputExpr);
        }

        #endregion

        #region Var Scope Maintenance
        private void PushExpressionBindingScope(RelOpInfo inputState)
        {
            PlanCompiler.Assert(inputState != null && inputState.PublisherName != null && inputState.PublishedVars != null , "Invalid RelOpInfo produced by DbExpressionBinding Input");
            _bindingScopes.Push(inputState);
        }

        /// <summary>
        /// Visit a Node that will be used as the basis of a DbExpressionBinding, optionally pushing the
        /// Vars that are logically published by the DbExpression produced from the Node's Op onto the expression binding scopes stack.
        /// </summary>
        /// <param name="inputNode">The Node to Visit</param>
        /// <param name="pushScope">Indicates whether or not the Vars published by the converted form of the Node's Op should be brought into scope before this method returns</param>
        /// <returns>The RelOpInfo that corresponds to the given Node, which details the DbExpression it was converted to, the Vars that are logically published by that DbExpression, and the unique name under which those Vars should be bound</returns>
        private RelOpInfo EnterExpressionBindingScope(Node inputNode, bool pushScope)
        {
            RelOpInfo inputInfo = VisitAsRelOp(inputNode);

            //
            // If the pushScope flag is set, push the RelOpInfo onto the binding scopes stack to bring
            // the Vars it tracks into scope.
            //
            if (pushScope)
            {
                PushExpressionBindingScope(inputInfo);
            }

            //
            // Return the RelOpInfo that was produced by the input Node to the caller, providing access to the
            // DbExpression that the Node's Op was converted to, the Vars that are logically published by that DbExpression,
            // and the unique binding name that the Vars are considered bound under - that name should be used in
            // the DbExpressionBinding that uses the DbExpression.
            //
            return inputInfo;
        }

        private RelOpInfo EnterExpressionBindingScope(Node inputNode)
        {
            return EnterExpressionBindingScope(inputNode, true);
        }

        private void ExitExpressionBindingScope(RelOpInfo scope, bool wasPushed)
        {
            if (wasPushed)
            {
                PlanCompiler.Assert(_bindingScopes.Count > 0, "ExitExpressionBindingScope called on empty ExpressionBindingScope stack");

                RelOpInfo bindingScope = (RelOpInfo)_bindingScopes.Pop();

                PlanCompiler.Assert(bindingScope == scope, "ExitExpressionBindingScope called on incorrect expression");
            }
        }

        private void ExitExpressionBindingScope(RelOpInfo scope)
        {
            ExitExpressionBindingScope(scope, true);
        }

        private GroupByScope EnterGroupByScope(Node inputNode)
        {
            RelOpInfo inputInfo = VisitAsRelOp(inputNode);

            // The current binding name is saved for use later as the VarName in the DbGroupExpressionBinding.
            string varName = inputInfo.PublisherName;

            // Generate the GroupVarName, and rebind the Input Vars under that name
            string groupVarName = string.Format(CultureInfo.InvariantCulture, "{0}Group", varName);
            
            DbGroupExpressionBinding newBinding = inputInfo.CreateBinding().Expression.GroupBindAs(varName, groupVarName);
            GroupByScope newScope = new GroupByScope(newBinding, inputInfo.PublishedVars);
            _bindingScopes.Push(newScope);
            return newScope;
        }

        private void ExitGroupByScope(GroupByScope scope)
        {
            PlanCompiler.Assert(_bindingScopes.Count > 0, "ExitGroupByScope called on empty ExpressionBindingScope stack");

            GroupByScope groupScope = (GroupByScope)_bindingScopes.Pop();

            PlanCompiler.Assert(groupScope == scope, "ExitGroupByScope called on incorrect expression");
        }

        /// <summary>
        /// Converts a list of VarDefOp Nodes into Expressions, builds a map of Var to DbExpression for each
        /// defined Var, and pushes a new VarDefScope containing the map onto the stack of 'in scope' Vars.
        /// </summary>
        /// <param name="varDefNodes">A list of Nodes. Each Node in the list must reference a VarDefOp</param>
        private void EnterVarDefScope(List<Node> varDefNodes)
        {
            //
            // Create a new dictionary to act as the Var to DbExpression map
            //
            Dictionary<Var, DbExpression> varDefs = new Dictionary<Var, DbExpression>();

            //
            // For each Node in the list:
            // 1. Assert that the Node is actually referencing a VarDefOp
            // 2. Assert that the Var defined by the VarDefOp is actually a ComputedVar
            // 3. Visit the Child0 Node of the Node to produce the CQT DbExpression that defines the Var
            // 4. Add the returned DbExpression to the Var to DbExpression map.
            foreach (Node childNode in varDefNodes)
            {
                VarDefOp defOp = childNode.Op as VarDefOp;
                PlanCompiler.Assert(defOp != null, "VarDefListOp contained non-VarDefOp child node");
                PlanCompiler.Assert(defOp.Var is ComputedVar, "VarDefOp defined non-Computed Var");

                varDefs.Add(defOp.Var, VisitNode(childNode.Child0));
            }

            //
            // Finally, construct and push a new VarDefScope based on the Var to DbExpression map onto the stack
            // of locally 'in scope' IQT ComputedVars. All of the Vars defined in the original list are brought into scope by
            // this final step, and are not in scope until this is done. Therefore it is not valid for any Var
            // in the original list to refer to a Var that occurs previously in the list (left-correlation).
            //
            _varScopes.Push(new VarDefScope(varDefs));
        }

        /// <summary>
        /// A convenience method to create a new VarDefScope from the specified VarDefListOp Node
        /// </summary>
        /// <param name="varDefListNode">The Node that references the VarDefListOp. Its children will be used as the basis of the new VarDefScope</param>
        private void EnterVarDefListScope(Node varDefListNode)
        {
            PlanCompiler.Assert(varDefListNode.Op is VarDefListOp, "EnterVarDefListScope called with non-VarDefListOp");
            EnterVarDefScope(varDefListNode.Children);
        }
        
        /// <summary>
        /// Asserts that the top of the scope stack is actually a VarDefScope, and then pops it to remove the locally defined Vars from scope.
        /// </summary>
        private void ExitVarDefScope()
        {
            PlanCompiler.Assert(_varScopes.Count > 0, "ExitVarDefScope called on empty VarDefScope stack");
            _varScopes.Pop();
        }

        /// <summary>
        /// Resolves an IQT Var to a CQT DbExpression.
        /// There are 3 possible ways for an IQT Var to resolve to a valid reference expressed as a CQT DbExpression:
        /// 1. The specified Var is a valid ParameterVar in the IQT Command being converted:
        ///     This resolves simply to ParameterRefExpression. A Parameter that corresponds to the ParameterVar
        ///     is declared on the CQT DbCommandTree is this has not already been done.
        /// 2. The specified Var is a ComputedVar that is defined locally to the Op being visited. In this case
        ///     The DbExpression produced by converting the VarDefOp that defines the Var is returned.
        /// 3. Otherwise, the Var must have been brought into scope because the DbExpression that logically produces it is
        ///     being used in a DbExpressionBinding which is currently in scope. Each RelOpInfo on the ExpressionBindingScopes stack
        ///     is asked to resolve the Var, if one of the RelOpInfo scopes is tracking the Var it will construct an appropriate combination
        ///     of DbVariableReferenceExpression and PropertyRefExpressions that are sufficient to logically reference the Var.
        /// If none of the 3 above conditions are satisfied then the Var is unresolvable in the CQT being constructed and
        /// the original IQT Command must be considered invalid for the purposes of this conversion.
        /// </summary>
        /// <param name="referencedVar">The IQT Var to resolve</param>
        /// <returns>The CQT DbExpression to which the specified Var resolves</returns>
        private DbExpression ResolveVar(Var referencedVar)
        {
            DbExpression retExpr = null;
            ParameterVar paramVar = referencedVar as ParameterVar;
            if (paramVar != null)
            {
                //
                // If there is already a parameter expression that corresponds to this parameter Var, reuse it.
                //
                DbParameterReferenceExpression paramRef;
                if (!_addedParams.TryGetValue(paramVar, out paramRef))
                {
                    paramRef = DbExpressionBuilder.Parameter(paramVar.Type, paramVar.ParameterName);
                    _addedParams[paramVar] = paramRef;
                }
                retExpr = paramRef;
            }
            else
            {
                ComputedVar compVar = referencedVar as ComputedVar;
                if (compVar != null)
                {
                    //
                    // If this is a ComputedVar, first check if it is defined locally to the Node of the Op being visited.
                    // Such local ComputedVars are only directly accessible from the Op being converted, so only the topmost
                    // ComputedVar scope on the stack should be considered.
                    //
                    if (_varScopes.Count > 0)
                    {
                        if (!_varScopes.Peek().TryResolveVar(compVar, out retExpr))
                        {
                            retExpr = null;
                        }
                    }
                }

                if (null == retExpr)
                {
                    //
                    // If the Var was not resolved as a locally defined ComputedVar, then it must now be a Var that was brought
                    // into scope by a DbExpressionBinding in order to be considered valid. Each DbExpressionBinding scope (represented as a RelOpInfo)
                    // on the binding scopes stack from top to bottom is asked in turn to resolve the Var, breaking if the Var is successfully resolved.
                    //
                    DbExpression foundExpr = null;
                    foreach (IqtVarScope scope in _bindingScopes)
                    {
                        if (scope.TryResolveVar(referencedVar, out foundExpr))
                        {
                            retExpr = foundExpr;
                            break;
                        }
                    }
                }
            }

            PlanCompiler.Assert(retExpr != null, string.Format(CultureInfo.InvariantCulture, "Unresolvable Var used in Command: VarType={0}, Id={1}", Enum.GetName(typeof(VarType), referencedVar.VarType), referencedVar.Id));
            return retExpr;
        }
        #endregion

        #region Visitor Helpers

        /// <summary>
        /// Asserts that the specified Node has exactly 2 child Nodes
        /// </summary>
        /// <param name="n">The Node on which to Assert</param>
        private static void AssertBinary(Node n)
        {
            PlanCompiler.Assert(2 == n.Children.Count, string.Format(CultureInfo.InvariantCulture, "Non-Binary {0} encountered", n.Op.GetType().Name));
        }

        private DbExpression VisitChild(Node n, int index)
        {
            PlanCompiler.Assert(n.Children.Count > index, "VisitChild called with invalid index");
            return VisitNode(n.Children[index]);
        }

        private new List<DbExpression> VisitChildren(Node n)
        {
            List<DbExpression> retList = new List<DbExpression>();
            foreach (Node argNode in n.Children)
            {
                retList.Add(VisitNode(argNode));
            }

            return retList;
        }

        #endregion

        #region IOpVisitor<DbExpression> Members

        #region ScalarOp Conversions
        protected override DbExpression VisitConstantOp(ConstantBaseOp op, Node n)
        {
            //
            // Simple conversion using the same constant value as the ConstantBaseOp in a CQT DbConstantExpression
            //
            return DbExpressionBuilder.Constant(op.Type, op.Value);
        }

        public override DbExpression Visit(ConstantOp op, Node n)
        {
            return VisitConstantOp(op, n);
        }

        public override DbExpression Visit(InternalConstantOp op, Node n)
        {
            return VisitConstantOp(op, n);
        }

        public override DbExpression Visit(NullOp op, Node n)
        {
            return DbExpressionBuilder.Null(op.Type);
        }

        public override DbExpression Visit(NullSentinelOp op, Node n)
        {
            return VisitConstantOp(op, n);
        }

        public override DbExpression Visit(ConstantPredicateOp op, Node n)
        {
            //
            // Create a "true=true" for "true" predicates,
            // Create a "true=false" expression for false predicates
            //
            return DbExpressionBuilder.True.Equal(op.IsTrue ? DbExpressionBuilder.True : DbExpressionBuilder.False);
        }

        public override DbExpression Visit(FunctionOp op, Node n)
        {
            //
            // FunctionOp becomes DbFunctionExpression that references the same EdmFunction metadata and
            // with argument Expressions produced by converting the child nodes of the FunctionOp's Node
            //
            return op.Function.Invoke(VisitChildren(n));
        }

        public override DbExpression Visit(PropertyOp op, Node n)
        {
            // We should never see this Op - should have been eliminated in NTE
            throw EntityUtil.NotSupported();
        }

        public override DbExpression Visit(RelPropertyOp op, Node n)
        {
            // should have been eliminated in NTE
            throw EntityUtil.NotSupported();
        }

        public override DbExpression Visit(ArithmeticOp op, Node n)
        {
            //
            // ArithmeticOp converts to a DbArithmeticExpression with an appropriate DbExpressionKind.
            //
            DbExpression resultExpr = null;
            if (OpType.UnaryMinus == op.OpType)
            {
                // If the OpType is Unary minus, only 1 child Node is required.
                resultExpr = VisitChild(n, 0).UnaryMinus();
            }
            else
            {
                // Otherwise this is a binary operator, so visit the left and right child Nodes
                // and convert to CQT DbExpression based on the OpType.
                DbExpression left = VisitChild(n, 0);
                DbExpression right = VisitChild(n, 1);

                switch (op.OpType)
                {
                    case OpType.Divide:
                        {
                            resultExpr = left.Divide(right);
                        }
                        break;

                    case OpType.Minus:
                        {
                            resultExpr = left.Minus(right);
                        }
                        break;

                    case OpType.Modulo:
                        {
                            resultExpr = left.Modulo(right);
                        }
                        break;

                    case OpType.Multiply:
                        {
                            resultExpr = left.Multiply(right);
                        }
                        break;

                    case OpType.Plus:
                        {
                            resultExpr = left.Plus(right);
                        }
                        break;

                    default:
                        {
                            resultExpr = null;
                        }
                        break;
                }
            }

            // The result DbExpression will only be null if a new OpType is added and this code is not updated
            PlanCompiler.Assert(resultExpr != null, string.Format(CultureInfo.InvariantCulture, "ArithmeticOp OpType not recognized: {0}", Enum.GetName(typeof(OpType), op.OpType)));
            return resultExpr;
        }

        public override DbExpression Visit(CaseOp op, Node n)
        {
            //
            // CaseOp converts directly to DbCaseExpression.
            // If no 'Else' Node is present a new DbNullExpression typed to the result Type of the CaseOp is used as the DbCaseExpression's Else expression.
            // Otherwise the converted form of the 'Else' Node is used.
            // This method assumes that the child Nodes of the CaseOp's Node are as follows:
            // When1, Then1[..., WhenN, ThenN][, Else]
            // that is, at least one When/Then pair MUST be present, subsequent When/Then pairs and the final Else are optional.

            // This is the count of Nodes that contribute to the When/Then pairs, NOT the count of those pairs.
            int caseCount = n.Children.Count;

            // Verify the assumption made that at least one case is present.
            PlanCompiler.Assert(caseCount > 1, "Invalid CaseOp: At least 2 child Nodes (1 When/Then pair) must be present");

            List<DbExpression> whens = new List<DbExpression>();
            List<DbExpression> thens = new List<DbExpression>();
            DbExpression elseExpr = null;

            if(0 == n.Children.Count % 2)
            {
                // If the number of child Nodes is divisible by 2, it is assumed that they are When/Then pairs without the optional Else Node.
                // The Else DbExpression defaults to a properly typed DbNullExpression.
                elseExpr = DbExpressionBuilder.Null(op.Type);
            }
            else
            {
                // Otherwise, an Else Node is present as the last child Node. It's CQT DbExpression form is used as the Else DbExpression.
                // The count of child Nodes that contribute to the When/Then pairs must now be reduced by 1.
                caseCount = caseCount - 1;
                elseExpr = VisitChild(n, n.Children.Count - 1);
            }

            // Convert the When/Then Nodes in pairs until the number of converted Nodes is equal to the number of Nodes that contribute to the When/Then pairs.
            for(int idx = 0; idx < caseCount; idx += 2)
            {
                whens.Add(VisitChild(n, idx));
                thens.Add(VisitChild(n, idx + 1));
            }

            // Create and return a new DbCaseExpression using the When and Then DbExpression lists and the (converted or DbNullExpression) Else DbExpression.
            return DbExpressionBuilder.Case(whens, thens, elseExpr);
        }

        public override DbExpression Visit(ComparisonOp op, Node n)
        {
            //
            // ComparisonOp converts to a DbComparisonExpression with an appropriate DbExpressionKind.
            // The ComparisonOp is only convertible to a DbComparisonExpression if it has 2 child Nodes
            //
            AssertBinary(n);

            DbExpression left = VisitChild(n, 0);
            DbExpression right = VisitChild(n, 1);

            DbExpression compExpr = null;

            switch (op.OpType)
            {
                case OpType.EQ:
                    {
                        compExpr = left.Equal(right);
                    }
                    break;

                case OpType.NE:
                    {
                        compExpr = left.NotEqual(right);
                    }
                    break;

                case OpType.LT:
                    {
                        compExpr = left.LessThan(right);
                    }
                    break;

                case OpType.GT:
                    {
                        compExpr = left.GreaterThan(right);
                    }
                    break;

                case OpType.LE:
                    {
                        compExpr = left.LessThanOrEqual(right);
                    }
                    break;

                case OpType.GE:
                    {
                        compExpr = left.GreaterThanOrEqual(right);
                    }
                    break;

                default:
                    {
                        compExpr = null;
                    }
                    break;
            }

            // The result DbExpression will only be null if a new OpType is added and this code is not updated
            PlanCompiler.Assert(compExpr != null, string.Format(CultureInfo.InvariantCulture, "ComparisonOp OpType not recognized: {0}", Enum.GetName(typeof(OpType), op.OpType)));
            return compExpr;
        }

        public override DbExpression Visit(ConditionalOp op, Node n)
        {
            //
            // Boolean ConditionalOps convert to the corresponding And/Or/DbNotExpression. The IsNull ConditionalOp converts to DbIsNullExpression.
            // In all cases the OpType is used to determine what kind of DbExpression to create.
            //

            // There will always be at least one argument (IsNull, Not) and should be at most 2 (And, Or).
            DbExpression left = VisitChild(n, 0);
            DbExpression condExpr = null;
            switch (op.OpType)
            {
                case OpType.IsNull:
                    {
                        condExpr = left.IsNull();
                    }
                    break;

                case OpType.And:
                    {
                        condExpr = left.And(VisitChild(n, 1));
                    }
                    break;

                case OpType.Or:
                    {
                        condExpr = left.Or(VisitChild(n, 1));
                    }
                    break;

                case OpType.Not:
                    {
                        // Convert Not(Not(<expression>)) to just <expression>. This is taken into account here
                        // because LeftSemi/AntiJoin conversions generate intermediate Not(Exists(<Op>)) IQT Nodes,
                        // which would then be converted to Not(Not(IsEmpty(<expression>)) if the following code were not present.
                        DbNotExpression notExpr = left as DbNotExpression;
                        if (notExpr != null)
                        {
                            condExpr = notExpr.Argument;
                        }
                        else
                        {
                            condExpr = left.Not();
                        }
                    }
                    break;

                default:
                    {
                        condExpr = null;
                    }
                    break;
            }

            // The result DbExpression will only be null if a new OpType is added and this code is not updated
            PlanCompiler.Assert(condExpr != null, string.Format(CultureInfo.InvariantCulture, "ConditionalOp OpType not recognized: {0}", Enum.GetName(typeof(OpType), op.OpType)));
            return condExpr;
        }

        public override DbExpression Visit(LikeOp op, Node n)
        {
            //
            // LikeOp converts to DbLikeExpression, with the conversions of the
            // Node's first, second and third child nodes providing the
            // Input, Pattern and Escape expressions.
            //
            return DbExpressionBuilder.Like(
                    VisitChild(n, 0),
                    VisitChild(n, 1),
                    VisitChild(n, 2)
                );
        }

        public override DbExpression Visit(AggregateOp op, Node n)
        {
            // AggregateOp may only occur as the immediate child of a VarDefOp that is itself the
            // child of a VarDefListOp used as the 'Aggregates' collection of a GroupByOp.
            // As such, aggregates are handled directly during processing of GroupByOp.
            // If this method is called an AggregateOp was encountered at some other (invalid) point in the IQT.
            PlanCompiler.Assert(false, "AggregateOp encountered outside of GroupByOp");
            throw EntityUtil.NotSupported(System.Data.Entity.Strings.Iqt_CTGen_UnexpectedAggregate);
        }
        public override DbExpression Visit(NavigateOp op, Node n)
        {
            // we should never see this Op
            throw EntityUtil.NotSupported();
        }

        public override DbExpression Visit(NewEntityOp op, Node n)
        {
            // We should never see this Op - should have been eliminated in NTE
            throw EntityUtil.NotSupported();
        }
        public override DbExpression Visit(NewInstanceOp op, Node n)
        {
            // We should never see this Op - should have been eliminated in NTE
            throw EntityUtil.NotSupported();
        }

        public override DbExpression Visit(DiscriminatedNewEntityOp op, Node n)
        {
            // We should never see this Op -  should have been eliminated in NTE
            throw EntityUtil.NotSupported();
        }

        public override DbExpression Visit(NewMultisetOp op, Node n)
        {
            // We should never see this Op
            throw EntityUtil.NotSupported();
        }

        public override DbExpression Visit(NewRecordOp op, Node n)
        {
            // We should never see this Op
            throw EntityUtil.NotSupported();
        }

        public override DbExpression Visit(RefOp op, Node n)
        {
            // We should never see this Op
            throw EntityUtil.NotSupported();
        }

        public override DbExpression Visit(VarRefOp op, Node n)
        {
            return ResolveVar(op.Var);
        }

        public override DbExpression Visit(TreatOp op, Node n)
        {
            // We should never see this Op
            throw EntityUtil.NotSupported();
        }

        public override DbExpression Visit(CastOp op, Node n)
        {
            // Direct conversion to DbCastExpression with the same Type and converted argument DbExpression
            return VisitChild(n, 0).CastTo(op.Type);
        }

        /// <summary>
        /// A SoftCastOp is intended to be used only for promotion (and/or equivalence)
        /// and should be ignored in the CTree
        /// </summary>
        /// <param name="op">the softcast Op</param>
        /// <param name="n">the node</param>
        /// <returns></returns>
        public override DbExpression Visit(SoftCastOp op, Node n)
        {
            // Microsoft 9/21/06 - temporarily removing check here 
            //  because the assert wrongly fails in some cases where the types are promotable,
            //  but the facets are not.  Put this back when that issue is solved.
            //
            // PlanCompiler.Assert(TypeSemantics.IsEquivalentOrPromotableTo(n.Child0.Op.Type, op.Type),
            //    "Invalid use of SoftCastOp: Type " + n.Child0.Op.Type.Identity + " is not promotable to " + op.Type);
            return VisitChild(n, 0);
        }

        public override DbExpression Visit(IsOfOp op, Node n)
        {
            // Direct conversion to DbIsOfExpression (with DbExpressionKind.IsOf) with the same Type and converted argument DbExpression
            if (op.IsOfOnly)
                return VisitChild(n, 0).IsOfOnly(op.IsOfType);
            else
                return VisitChild(n, 0).IsOf(op.IsOfType);
        }

        public override DbExpression Visit(ExistsOp op, Node n)
        {
            //
            // Exists requires a RelOp input set
            //
            DbExpression inputExpr = VisitNode(n.Child0);

            //
            // Information about the Vars published by the RelOp argument does not need to be maintained
            // since they may not now be used higher in the CQT.
            //
            ConsumeRelOp(inputExpr);

            //
            // Exists --> Not(IsEmpty(Input set)) via DbExpressionBuilder.Exists
            //
            return inputExpr.IsEmpty().Not();
        }

        public override DbExpression Visit(ElementOp op, Node n)
        {
            // We create this op when turning ApplyOp into a scalar subquery
            DbExpression inputExpr = VisitNode(n.Child0);
            AssertRelOp(inputExpr);
            ConsumeRelOp(inputExpr);

            DbElementExpression elementExpr = DbExpressionBuilder.CreateElementExpressionUnwrapSingleProperty(inputExpr);
            return elementExpr;
        }

        public override DbExpression Visit(GetRefKeyOp op, Node n)
        {
            // We should never see this Op
            throw EntityUtil.NotSupported();
        }

        public override DbExpression Visit(GetEntityRefOp op, Node n)
        {
            // We should never see this Op
            throw EntityUtil.NotSupported();
        }

        public override DbExpression Visit(CollectOp op, Node n)
        {
            // We should never get here
            throw EntityUtil.NotSupported();
        }
        #endregion

        #region RelOp Conversions

        /// <summary>
        /// Generates a name for the specified Var.
        /// If the Var has a name (TryGetName), then we use the name to look up
        /// the right alias generator, and get a column name from the alias generator
        /// Otherwise, we simply get a name from the default alias generator
        /// </summary>
        /// <param name="projectedVar">the var in question</param>
        /// <param name="aliasMap">map to identify the appropriate alias generator</param>
        /// <param name="defaultAliasGenerator">the default alias generator</param>
        /// <param name="alreadyUsedNames">list of already used names</param>
        /// <returns></returns>
        private static string GenerateNameForVar(Var projectedVar, Dictionary<string, AliasGenerator> aliasMap,
            AliasGenerator defaultAliasGenerator, Dictionary<string, string> alreadyUsedNames)
        {
            string columnName;
            AliasGenerator aliasGenerator;

            if (projectedVar.TryGetName(out columnName))
            {
                if (!aliasMap.TryGetValue(columnName, out aliasGenerator))
                {
                    //
                    // No existing column in the current row with the same name. Create
                    // an alias-generator for future use
                    //
                    aliasGenerator = new AliasGenerator(columnName);
                    aliasMap[columnName] = aliasGenerator;
                }
                else
                {
                    //
                    // Column name collides with another name in the same row. 
                    // Use the alias-generator to generate a new name
                    //
                    columnName = aliasGenerator.Next();
                }
            }
            else
            {
                //
                // Must be a computed column or some such. Use the default alias generator
                //
                aliasGenerator = defaultAliasGenerator;
                columnName = aliasGenerator.Next();
            }

            // Check to see if I've used this name already
            while (alreadyUsedNames.ContainsKey(columnName))
            {
                columnName = aliasGenerator.Next();
            }

            alreadyUsedNames[columnName] = columnName;
            return columnName;
        }

        /// <summary>
        /// Called by both Visit(ProjectOp) and VisitSetOpArgument to create a DbProjectExpression
        /// based on the RelOpInfo of the projection input and the set of projected Vars.
        /// Note:
        /// The projected Vars must have already been brought into scope (by one of the
        /// methods such as EnterExpressionBinding, EnterVarDefScope, etc) before this method
        /// is called, or the projected Vars will not be successfully resolved.
        /// Both Visit(ProjectOp) and VisitSetOpArgument do this"
        /// 1. Visit(ProjectOp) takes both DbExpressionBinding and VarDef based Vars into account
        /// 2. The Vars produced by a SetOpArgument projection are only allowed to be DbExpressionBinding
        ///    based and are brought into scope when the original SetOp argument Node is visited.
        /// </summary>
        /// <param name="sourceInfo"></param>
        /// <param name="outputVars"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1309:UseOrdinalStringComparison", MessageId = "System.Collections.Generic.Dictionary`2<System.String,System.String>.#ctor(System.Collections.Generic.IEqualityComparer`1<System.String>)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1309:UseOrdinalStringComparison", MessageId = "System.Collections.Generic.Dictionary`2<System.String,System.Data.Common.Utils.AliasGenerator>.#ctor(System.Collections.Generic.IEqualityComparer`1<System.String>)")]
        private DbExpression CreateProject(RelOpInfo sourceInfo, IEnumerable<Var> outputVars)
        {
            //
            // For each Var produced by the ProjectOp, call ResolveVar to retrieve the correct CQT DbExpression.
            // This will either be a DbExpression that references the CQT Var under which the IQT is currently
            // bound (if it is in scope) or it will be a copy of the DbExpression subtree that defines the Var,
            // if the Var is a ComputedVar defined by a local VarDefOp.
            // A new column name is generated to project the DbExpression, and the VarInfoList produced
            // by this conversion is updated to include a new VarInfo indicating that the projected Var can be
            // reached in the DbProjectExpression returned from this method via a property reference to the generated column name.
            // This is the only path element required since the Vars are an immediate product of the ProjectOp, which also hides any Vars below it.
            // Hence a new VarInfoList is constructed and published by the new DbProjectExpression.
            // The list of column name/DbExpression pairs is built to use later when constructing the projection expression.
            //
            VarInfoList projectedInfo = new VarInfoList();
            List<KeyValuePair<string, DbExpression>> projectedCols = new List<KeyValuePair<string, DbExpression>>();
            AliasGenerator colGen = new AliasGenerator("C");
            Dictionary<string, AliasGenerator> aliasMap = new Dictionary<string, AliasGenerator>(StringComparer.InvariantCultureIgnoreCase);
            Dictionary<string, string> alreadyUsedAliases = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (Var projectedVar in outputVars)
            {
                string columnName = GenerateNameForVar(projectedVar, aliasMap, colGen, alreadyUsedAliases);

                DbExpression columnValue = ResolveVar(projectedVar);
                projectedCols.Add(new KeyValuePair<string, DbExpression>(columnName, columnValue));

                VarInfo colInfo = new VarInfo(projectedVar);
                colInfo.PrependProperty(columnName);
                projectedInfo.Add(colInfo);
            }

            //
            // Create a new DbProjectExpression with the converted Input and a new row (DbNewInstanceExpression) projection using the
            // previously constructed column names and Expressions to define the shape of the resulting row. The Input is bound
            // under the Var publisher name specified by its RelOpInfo, which will be a unique name based on the type of RelOp it was converted from.
            //
            DbExpression retExpr = sourceInfo.CreateBinding().Project(DbExpressionBuilder.NewRow(projectedCols));
                
            //
            // Publish the Vars produced by the new DbProjectExpression:
            // PublisherName: The next Project alias.
            // PublishedVars: The PublishedVars of the Project are those specified in the VarSet of the ProjectOp, reachable using the generated column names.
            //
            PublishRelOp(_projectAliases.Next(), retExpr, projectedInfo);

            return retExpr;
        }

        /// <summary>
        /// Called by both ScanTableOp and UnnestOp Visitor pattern methods to determine
        /// the shape of the output of the converted form of those Ops, in terms of the
        /// IQT Vars that are published by the resulting DbExpression and how those Vars should
        /// be reached.
        /// </summary>
        /// <param name="targetTable">The table that is logically produced by the Op. For non-record sourceTypes, this should consist of a single column that logically constitutes the entire 'table'</param>
        /// <returns>A VarInfoList containing VarInfo instances that correctly track the Var or Vars produced by the targetTable, in accordance with the shape of the sourceType</returns>
        private static VarInfoList GetTableVars(Table targetTable)
        {
            VarInfoList outputVars = new VarInfoList();

            if (targetTable.TableMetadata.Flattened)
            {
                // For a flat table, one Var per table column must be produced.
                // There should be a ColumnVar in the targetTable's Columns collection for each
                // column in the record type (for the table), and the VarInfo instances created here will track the
                // fact that each ColumnVar should be reached via DbPropertyExpression of the record column's name
                for (int idx = 0; idx < targetTable.Columns.Count; idx++)
                {
                    VarInfo colInfo = new VarInfo(targetTable.Columns[idx]);
                    colInfo.PrependProperty(targetTable.TableMetadata.Columns[idx].Name);
                    outputVars.Add(colInfo);
                }
            }
            else
            {
                // Otherwise, a single Var must be produced, which is immediately reachable
                outputVars.Add(new VarInfo(targetTable.Columns[0]));
            }

            return outputVars;
        }

        public override DbExpression Visit(ScanTableOp op, Node n)
        {
            //
            // Currently 2 different types of 'Table' (i.e. Extent) are supported:
            // Record Extents (for example an S-Space table or view)
            // Entity Extents (for example a C-Space EntitySet)
            // These extents produce results of different shapes - the EntitySet case produces simply
            // A collection of Entities, not a collection of records with a single Entity-typed column
            // This distinction is handled in the common GetTableVars method shared by ScanTableOp and
            // UnnestOp Visitor pattern methods.
            //
            PlanCompiler.Assert(op.Table.TableMetadata.Extent != null, "Invalid TableMetadata used in ScanTableOp - no Extent specified");

            //
            // We don't expect to see any view expressions here
            //
            PlanCompiler.Assert(!n.HasChild0, "views are not expected here");

            VarInfoList outputVars = GetTableVars(op.Table);

            // ScanTable converts to ExtentExpression
            DbExpression retExpr = op.Table.TableMetadata.Extent.Scan();
            
            //
            // Publish the Vars that are logically produced by the ExtentExpression:
            // PublisherName: The next Extent alias
            // PublishedVars: The single Var (for an Entity extent) or multiple column-bound Vars (for a structured type extent)
            //  that are logically published by the ExtentExpression.
            //
            PublishRelOp(_extentAliases.Next(), retExpr, outputVars);

            // Return the ExtentExpression
            return retExpr;
        }

        public override DbExpression Visit(ScanViewOp op, Node n)
        {
            // We should never see this Op
            throw EntityUtil.NotSupported();
        }

        /// <summary>
        /// Translate UnnestOp which is assumed (at this stage) to wrap a native ScalarOp
        /// that returns a collection (e.g. a table-valued function node).
        /// </summary>
        public override DbExpression Visit(UnnestOp op, Node n)
        {
            // support Unnest(VarDef(input)) -> input
            // where input is presumed to have a collection type (e.g. TVF)
            PlanCompiler.Assert(n.Child0.Op.OpType == OpType.VarDef, 
                "an unnest's child must be a VarDef");

            // get input (first child of VarDef)
            Node input = n.Child0.Child0;

            // translate input
            DbExpression expr = input.Op.Accept(this, input);

            // verify that the result is actually a collection
            PlanCompiler.Assert(expr.ResultType.EdmType.BuiltInTypeKind == BuiltInTypeKind.CollectionType,
                "the input to unnest must yield a collection after plan compilation");

            // collect table vars for the unnest
            VarInfoList outputVars = GetTableVars(op.Table);
            PublishRelOp(_extentAliases.Next(), expr, outputVars);

            return expr;
        }

        /// <summary>
        /// Builds up an "empty" projection over the input node. Well, in reality, we build
        /// up a dummy projection node - which simply selects out some constant (which
        /// is never used). This is useful in scenarios where the outputs are
        /// uninteresting, but the input row count is
        /// </summary>
        /// <param name="relOpNode">the relOp node</param>
        /// <returns></returns>
        private RelOpInfo BuildEmptyProjection(Node relOpNode)
        {
            //
            // Ignore the projectOp at the root - if any
            //
            if (relOpNode.Op.OpType == OpType.Project)
            {
                relOpNode = relOpNode.Child0;
            }

            //
            // Visit the Input RelOp, bring its Var(s) into scope, and retrieve and consume the RelOpInfo that describes its published Vars
            //
            RelOpInfo sourceInfo = EnterExpressionBindingScope(relOpNode);

            //
            // Create a new DbProjectExpression with the converted Input and a new row (DbNewInstanceExpression) projection using the
            // previously constructed column names and Expressions to define the shape of the resulting row. The Input is bound
            // under the Var publisher name specified by its RelOpInfo, which will be a unique name based on the type of RelOp it was converted from.
            //
            DbExpression constExpr = DbExpressionBuilder.Constant(1);
            List<KeyValuePair<string, DbExpression>> projectedCols = new List<KeyValuePair<string, DbExpression>>();
            projectedCols.Add(new KeyValuePair<string, DbExpression>("C0", constExpr));

            DbExpression retExpr = sourceInfo.CreateBinding().Project(DbExpressionBuilder.NewRow(projectedCols));
                
            // Publish the Vars produced by the new DbProjectExpression:
            // PublisherName: The next Project alias.
            // PublishedVars: The PublishedVars of the Project are those specified in the VarSet of the ProjectOp, reachable using the generated column names.
            //
            PublishRelOp(_projectAliases.Next(), retExpr, new VarInfoList());

            // remove the Input's Vars from scope, unbinding the Input's VarInfos. 
            ExitExpressionBindingScope(sourceInfo);

            RelOpInfo relOpInfo = ConsumeRelOp(retExpr);
            return relOpInfo;
        }

        /// <summary>
        /// Build up a Project Op with exactly the Vars that we want. If the input is 
        /// a Project already, piggyback on it, and get the Vars we want. Otherwise, 
        /// create a new ProjectOp, and define the specified Vars
        /// 
        /// Note that the ProjectOp's output (element) type will be a record with the fields
        /// in exactly the order specified by the projectionVars argument
        /// 
        /// </summary>
        /// <param name="relOpNode">the input relOpNode to cap with a Project</param>
        /// <param name="projectionVars">List of vars we are interested in</param>
        /// <returns>A ProjectOp that produces the right set of Vars</returns>
        private RelOpInfo BuildProjection(Node relOpNode, IEnumerable<Var> projectionVars)
        {
            DbExpression retExpr = null;

            //
            // If the input is a ProjectOp, then simply invoke the ProjectOp handler, but 
            // use the requested Vars instead
            //
            ProjectOp projectOp = relOpNode.Op as ProjectOp;
            if (projectOp != null)
            {
                retExpr = VisitProject(projectOp, relOpNode, projectionVars);
            }
            else
            {
                //
                // Otherwise, treat it in a very similar fashion to a normal projectOp. The
                // only difference being that we have no VarDefList argument
                //

                //
                // Visit the Input RelOp, bring its Var(s) into scope, and retrieve and consume the RelOpInfo that describes its published Vars
                //
                RelOpInfo sourceInfo = EnterExpressionBindingScope(relOpNode);

                //
                // Call CreateProject to convert resolve the projected Vars,
                // create the DbProjectExpression, and associate the projected Vars
                // with the new DbProjectExpression in the DbExpression -> RelOpInfo map.
                //
                retExpr = CreateProject(sourceInfo, projectionVars);

                // remove the Input's Vars from scope, unbinding the Input's VarInfos. 
                ExitExpressionBindingScope(sourceInfo);
            }

            RelOpInfo relOpInfo = ConsumeRelOp(retExpr);
            return relOpInfo;
        }

        DbExpression VisitProject(ProjectOp op, Node n, IEnumerable<Var> varList)
        {
            //
            // Visit the Input RelOp, bring its Var(s) into scope, and retrieve and consume the RelOpInfo that describes its published Vars
            //
            RelOpInfo sourceInfo = EnterExpressionBindingScope(n.Child0);

            //
            // With the Input in scope, visit the VarDefs from the ProjectOp's VarDefList and bring their ComputedVars into scope
            //
            if (n.Children.Count > 1)
            {
                EnterVarDefListScope(n.Child1);
            }

            //
            // Call CreateProject to convert resolve the projected Vars,
            // create the DbProjectExpression, and associate the projected Vars
            // with the new DbProjectExpression in the DbExpression -> RelOpInfo map.
            //
            DbExpression retExpr = CreateProject(sourceInfo, varList);

            // Take the local ComputedVars from the ProjectOp's VarDefList out of scope.
            if (n.Children.Count > 1)
            {
                ExitVarDefScope();
            }

            // remove the Input's Vars from scope, unbinding the Input's VarInfos.
            ExitExpressionBindingScope(sourceInfo);

            return retExpr;
        }

        public override DbExpression Visit(ProjectOp op, Node n)
        {
            return VisitProject(op, n, op.Outputs);
        }

        public override DbExpression Visit(FilterOp op, Node n)
        {
            //
            // Visit the Input RelOp, bring its Var(s) into scope, and retrieve and consume the RelOpInfo that describes its published Vars
            //
            RelOpInfo inputInfo = EnterExpressionBindingScope(n.Child0);

            //
            // Visit the Predicate with the Input Var(s) in scope and assert that the predicate is valid
            //
            DbExpression predicateExpr = VisitNode(n.Child1);
            PlanCompiler.Assert(TypeSemantics.IsPrimitiveType(predicateExpr.ResultType, PrimitiveTypeKind.Boolean), "Invalid FilterOp Predicate (non-ScalarOp or non-Boolean result)");

            //
            // Create a new DbFilterExpression with the converted Input and Predicate.
            // The RelOpState produced from the Input (above) indicates the name that should be used
            // in the DbExpressionBinding for the Input expression (this is the name that the
            // Input's Vars were brought into scope with in EnterExpressionBindingScope).
            //
            DbExpression retExpr = inputInfo.CreateBinding().Filter(predicateExpr);

            //
            // Remove the Input's Var(s) from scope and unbind the Input's VarInfo(s)
            //
            ExitExpressionBindingScope(inputInfo);

            //
            // Update the tracked RelOpInfo for the new DbFilterExpression. This consists of:
            // PublisherName: The next Filter alias.
            // PublishedVars: The PublishedVars of the Filter are the same (now unbound) PublishedVars of its Input.
            //
            PublishRelOp(_filterAliases.Next(), retExpr, inputInfo.PublishedVars);

            return retExpr;
        }

        private List<DbSortClause> VisitSortKeys(IList<InternalTrees.SortKey> sortKeys)
        {
            VarVec sortVars = _iqtCommand.CreateVarVec();
            List<DbSortClause> sortClauses = new List<DbSortClause>();
            foreach (InternalTrees.SortKey sortKey in sortKeys)
            {
                //
                // If we've already seen the same Var, then ignore it
                //
                if (sortVars.IsSet(sortKey.Var))
                {
                    continue;
                }
                else
                {
                    sortVars.Set(sortKey.Var);
                }

                DbSortClause sortClause = null;
                DbExpression keyExpression = ResolveVar(sortKey.Var);
                if (!string.IsNullOrEmpty(sortKey.Collation))
                {

                    sortClause = (sortKey.AscendingSort ? keyExpression.ToSortClause(sortKey.Collation) : keyExpression.ToSortClauseDescending(sortKey.Collation));
                }
                else
                {
                    sortClause = (sortKey.AscendingSort ? keyExpression.ToSortClause() : keyExpression.ToSortClauseDescending());
                }

                sortClauses.Add(sortClause);
            }

            return sortClauses;
        }

        public override DbExpression Visit(SortOp op, Node n)
        {
            //
            // Visit the Input RelOp, bring its Var(s) into scope, and retrieve and consume the RelOpInfo that describes its published Vars
            //
            RelOpInfo inputInfo = EnterExpressionBindingScope(n.Child0);
            PlanCompiler.Assert(!n.HasChild1, "SortOp can have only one child");
            
            //
            // Visit the SortKeys with the Input's Vars in scope and create the DbSortExpression
            //
            DbExpression retExpr = inputInfo.CreateBinding().Sort(VisitSortKeys(op.Keys));
            
            //
            // Remove the Input's Vars from scope
            //
            ExitExpressionBindingScope(inputInfo);

            //
            // Update the tracked RelOpInfo for the new DbSortExpression. This consists of:
            // PublisherName: The next Sort alias.
            // PublishedVars: The PublishedVars of the Sort are the same as its Input.
            //
            PublishRelOp(_sortAliases.Next(), retExpr, inputInfo.PublishedVars);
            return retExpr;
        }

        private DbExpression CreateLimitExpression(DbExpression argument, DbExpression limit, bool withTies)
        {
            PlanCompiler.Assert(!withTies, "Limit with Ties is not currently supported");
            return argument.Limit(limit);
        }

        public override DbExpression Visit(ConstrainedSortOp op, Node n)
        {
            DbExpression retExpr = null;
            RelOpInfo inputInfo = null;
            string alias = null;
            bool nullSkip = (OpType.Null == n.Child1.Op.OpType);
            bool nullLimit = (OpType.Null == n.Child2.Op.OpType);
            PlanCompiler.Assert(!nullSkip || !nullLimit, "ConstrainedSortOp with no Skip Count and no Limit?");
            if (op.Keys.Count == 0)
            {
                // Without SortKeys, this ConstrainedSortOp must represent a Limit operation applied to the input.
                PlanCompiler.Assert(nullSkip, "ConstrainedSortOp without SortKeys cannot have Skip Count");
                
                //
                // Visit the input Node and retrieve its RelOpInfo
                //
                DbExpression inputExpr = this.VisitNode(n.Child0);
                inputInfo = ConsumeRelOp(inputExpr);

                //
                // Create the DbLimitExpression using the converted form of the input Node's Child2 Node (the Limit Node)
                // together with the input DbExpression created above.
                //
                retExpr = this.CreateLimitExpression(inputExpr, this.VisitNode(n.Child2), op.WithTies);
                alias = _limitAliases.Next();
            }
            else
            {
                //
                // Bring the Input into scope and visit the SortKeys to produce the equivalent SortClauses,
                // then remove the Input's Vars from scope.
                //
                inputInfo = EnterExpressionBindingScope(n.Child0);
                List<DbSortClause> sortOrder = VisitSortKeys(op.Keys);
                ExitExpressionBindingScope(inputInfo);

                //
                // SortKeys are present, so one of the following cases must be true:
                // - Child1 (Skip Count) is non-NullOp, Child2 (Limit) is non-NullOp    => Limit(Skip(input))
                // - Child1 (Skip Count) is non-NullOp, Child2 (Limit) is NullOp        => Skip(input)
                // - Child1 (Skip Count) is NullOp,     Child2 (Limit) is non-NullOp    => Limit(Sort(input))
                //
                if (!nullSkip && !nullLimit)
                {
                    // Limit(Skip(input))
                    retExpr =
                        this.CreateLimitExpression(
                            inputInfo.CreateBinding().Skip(sortOrder, VisitChild(n, 1)),
                            VisitChild(n, 2),
                            op.WithTies
                        );
                    alias = _limitAliases.Next();
                }
                else if (!nullSkip && nullLimit)
                {
                    // Skip(input)
                    retExpr = inputInfo.CreateBinding().Skip(sortOrder, VisitChild(n, 1));
                    alias = _skipAliases.Next();
                }
                else if (nullSkip && !nullLimit)
                {
                    // Limit(Sort(input))
                    retExpr =
                        this.CreateLimitExpression(
                            inputInfo.CreateBinding().Sort(sortOrder), 
                            VisitChild(n, 2),
                            op.WithTies
                        );
                    alias = _limitAliases.Next();
                }
            }

            //
            // Update the tracked RelOpInfo for the new expression. This consists of:
            // PublisherName: The next Skip or Limit alias depending on which expression is topmost.
            // PublishedVars: The PublishedVars of the Skip/Limit are the same as its Input.
            //
            PublishRelOp(alias, retExpr, inputInfo.PublishedVars);
            return retExpr;
        }

        public override DbExpression Visit(GroupByOp op, Node n)
        {
            //
            // Track the Vars that are logically published by this GroupBy. These will be
            //
            VarInfoList publishedVars = new VarInfoList();

            //
            // Visit the Input, publish its Vars and bring them into scope under a new binding name
            //
            GroupByScope inputInfo = EnterGroupByScope(n.Child0);

            //
            // With the Input in scope, visit the VarDefs for the Keys and build the Var to DbExpression map for them
            //
            EnterVarDefListScope(n.Child1);

            //
            // With the mappings for the Key Vars in scope, build the Name/DbExpression key column pairs by
            // generating a new column alias (prefixed with 'K') and resolving the Var, for each Var in the Keys Var list.
            // The list of output Vars that represent aggregates is also built here, by starting with a list
            // of all output Vars and removing each Key Var as it is processed.
            //
            AliasGenerator keyAliases = new AliasGenerator("K");
            List<KeyValuePair<string, DbExpression>> keyExprs = new List<KeyValuePair<string, DbExpression>>();
            List<Var> outputAggVars = new List<Var>(op.Outputs);
            foreach (Var keyVar in op.Keys)
            {
                //
                // Generate the alias and resolve the Key Var. This will find and retrieve the DbExpression to which
                // the Key Var maps, which is most likely the VarDef scope that was just entered by visiting the Key VarDefListOp Node.
                // Track the Name/DbExpression pairs for use later in CreateGroupByExpression.
                //
                string keyColName = keyAliases.Next();
                keyExprs.Add(new KeyValuePair<string, DbExpression>(keyColName, ResolveVar(keyVar)));

                //
                // Create a new VarInfo to track this key Var. To begin with it is reachable through
                // a property reference of the column under which it is bound, i.e. using the column alias
                // generated above, so the VarInfo property path is set up to contain just that name
                // (the VarInfo has no binding name until later in this method when PublishRelOp is called).
                //
                VarInfo keyColInfo = new VarInfo(keyVar);
                keyColInfo.PrependProperty(keyColName);
                publishedVars.Add(keyColInfo);

                //
                // Remove the Key Var from the list of Aggregate Outputs
                //
                outputAggVars.Remove(keyVar);
            }

            //
            // After this point, no Vars in the output Vars list of the GroupBy may be defined by the
            // VarDefOps from the Keys VarDefListOp Node, so the Keys VarDefScope should be removed from the scope stack.
            //
            ExitVarDefScope();

            //
            // The Vars published by the Input are currently in scope under the binding name that was generated for the Input (Extent0, Filter3, etc).
            // This is correct while the Keys are processed, however it is not correct for the aggregates. While the aggregates have access to exactly
            // the same Vars, they must be bound under a different name, which will become the GroupVarName used later in this method when a DbGroupExpressionBinding
            // is constructed. The GroupVarName is generated simply by appending 'Group' to the (already unique) binding name that was generated for the Input (to yield Extent0Group, Filter3Group, etc).

            // In aggregate arguments, the GroupBy input's Vars must be accessed using the Group variable
            inputInfo.SwitchToGroupReference();

            // Build the map of Var to Aggregate. The Aggregates VarDefListOp Node child of the GroupByOp's Node is
            // processed here to build the map. This is the only location in an IQT Command where an AggregateOp is valid.
            Dictionary<Var, DbAggregate> aggMap = new Dictionary<Var,DbAggregate>();
            Node aggRootNode = n.Child2;
            PlanCompiler.Assert(aggRootNode.Op is VarDefListOp, "Invalid Aggregates VarDefListOp Node encountered in GroupByOp");
            foreach (Node aggVarDefNode in aggRootNode.Children)
            {
                VarDefOp aggVarDef = aggVarDefNode.Op as VarDefOp;
                PlanCompiler.Assert(aggVarDef != null, "Non-VarDefOp Node encountered as child of Aggregates VarDefListOp Node");

                Var aggVar = aggVarDef.Var;
                PlanCompiler.Assert(aggVar is ComputedVar, "Non-ComputedVar encountered in Aggregate VarDefOp");

                Node aggOpNode = aggVarDefNode.Child0;
                DbExpression aggDef = VisitNode(aggOpNode.Child0);
                AggregateOp funcAggOp = aggOpNode.Op as AggregateOp;
                PlanCompiler.Assert(funcAggOp != null, "Non-Aggregate Node encountered as child of Aggregate VarDefOp Node");
                DbFunctionAggregate newFuncAgg;
                if (funcAggOp.IsDistinctAggregate)
                {
                    newFuncAgg = funcAggOp.AggFunc.AggregateDistinct(aggDef);
                }
                else
                {
                    newFuncAgg = funcAggOp.AggFunc.Aggregate(aggDef);
                }

                PlanCompiler.Assert(outputAggVars.Contains(aggVar), "Defined aggregate Var not in Output Aggregate Vars list?");

                aggMap.Add(aggVar, newFuncAgg);

            }

            //
            // The Vars published by the Input should no longer be considered in scope, so call ExitExpressionBindingScope to pop them off the scope stack.
            //
            ExitGroupByScope(inputInfo);

            //
            // Process the list of Aggregate Vars using the Var to Aggregate map created in the code above.
            // Note that since there is no dedicated Aggregates VarSet on the GroupByOp, it is necessary to
            // process the Vars in the OutputVars set, but beginning with the Var that immediately follows
            // the last Key Var.
            // Each Var is mapped to a previously created Aggregate using the Var to Aggregate map. The end
            // result is a list of name CQT Aggregates that can be used in the call to CreateGroupByExpression.
            //
            AliasGenerator aggAliases = new AliasGenerator("A");
            List<KeyValuePair<string, DbAggregate>> aggregates = new List<KeyValuePair<string, DbAggregate>>();
            foreach(Var aggVar in outputAggVars)
            {
                // Generate a new column name for the Aggregate that will be prefixed with 'A'.
                string aggColName = aggAliases.Next();

                // Map the Var to an Aggregate and add it to the list under the newly generated column name
                aggregates.Add(new KeyValuePair<string, DbAggregate>(aggColName, aggMap[aggVar]));

                // Create a new VarInfo that will track the Aggregate Var up the CQT. Its property path is
                // initialized to the newly generated column name to indicate that the Var must be reached
                // with a DbPropertyExpression of the column name.
                VarInfo aggColInfo = new VarInfo(aggVar);
                aggColInfo.PrependProperty(aggColName);

                // Add the Aggregate VarInfo to the list of VarInfos that are tracking the Vars that are
                // logically published by the DbGroupByExpression that will result from this method.
                publishedVars.Add(aggColInfo);
            }

            //
            // Create the DbGroupByExpression. The binding name of the input is used together with the
            // generated group name and the input DbExpression to create a DbGroupExpressionBinding.
            // The list of named Keys and Aggregates built above are specified in the call to CreateGroupExpressionBinding.
            //
            DbExpression retExpr = inputInfo.Binding.GroupBy(keyExprs, aggregates);

            PublishRelOp(_groupByAliases.Next(), retExpr, publishedVars);

            return retExpr;
        }

        public override DbExpression Visit(GroupByIntoOp op, Node n)
        {
            // We should never see this Op
            throw EntityUtil.NotSupported();
        }

        #region JoinOp Conversions - CrossJoinOp, InnerJoinOp, FullOuterJoinOp, LeftOuterJoinOp
        /// <summary>
        /// Massages the input to a join node.
        /// 
        /// If the input is a Filter(ScanTable), we throw in a dummy project over
        /// this input. This projectOp simply looks at the "referenced" columns of
        /// the table, and uses those as the projection Vars
        /// Otherwise, sqlgen does not really know which columns are referenced, and
        /// ends up adding a projection with all columns of the table.
        /// 
        /// NOTE: We may want to do this for Apply as well
        /// </summary>
        /// <param name="joinInputNode">one of the inputs to the join node</param>
        /// <returns>RelopInfo for the transformed input</returns>
        private RelOpInfo VisitJoinInput(Node joinInputNode)
        {
            RelOpInfo relOpInfo;

            if (joinInputNode.Op.OpType == OpType.Filter && joinInputNode.Child0.Op.OpType == OpType.ScanTable)
            {
                ScanTableOp scanTableOp = (ScanTableOp)joinInputNode.Child0.Op;
                //
                // #479385: Handle "empty" projection lists
                //
                if (scanTableOp.Table.ReferencedColumns.IsEmpty)
                {
                    relOpInfo = BuildEmptyProjection(joinInputNode);
                }
                else
                {
                    relOpInfo = BuildProjection(joinInputNode, scanTableOp.Table.ReferencedColumns);
                }
            }
            else
            {
                relOpInfo = EnterExpressionBindingScope(joinInputNode, false);
            }

            return relOpInfo;
        }

        /// <summary>
        /// Called by all Visitor pattern method that handle binary JoinOps (Inner, FullOuter, LeftOuter)
        /// </summary>
        /// <param name="joinNode">The IQT Node that references the JoinOp</param>
        /// <param name="joinKind">The CQT DbExpressionKind that represents the type of join to create</param>
        /// <returns></returns>
        private DbExpression VisitBinaryJoin(Node joinNode, DbExpressionKind joinKind)
        {
            //
            // Visit and retrieve RelOpInfo for the left Input, but do not bring its published Vars
            // into scope. Passing the value false as the 'pushScope' argument indicates that
            // EnterExpressionBindingScope should visit the specified Node, retrieve (and remove)
            // its RelOpInfo from the DbExpression to RelOpInfo map, but not push that RelOpInfo onto
            // the scope stack.
            // The Vars are not brought into scope in order to prevent Left-correlation - the Vars of
            // the left Input should not be visible to the right Input of the same join.
            //
            RelOpInfo leftInfo = VisitJoinInput(joinNode.Child0);
            
            // Do the same for the right Input to the join.
            RelOpInfo rightInfo = VisitJoinInput(joinNode.Child1);

            bool scopesPushed = false;
            DbExpression joinCond = null;
            if (joinNode.Children.Count > 2)
            {
                // If a Join condition Node is present, bring the Vars from the left and right arguments into scope
                // and visit the Join condition Node's Op to convert the join condition. The scopesPushed flag is updated
                // to true to indicate that the Var scopes should be removed from the scope stack when this method completes.
                scopesPushed = true;

                PushExpressionBindingScope(leftInfo);
                PushExpressionBindingScope(rightInfo);

                joinCond = VisitNode(joinNode.Child2);
            }
            else
            {
                // There is no join condition, so the default condition - DbConstantExpression(True) - is used.
                // The Vars from the left and right Inputs to the join need not be brought into scope, so the scopesPushed flag is not updated.
                joinCond = DbExpressionBuilder.True;
            }

            // Create a new DbJoinExpression using bindings created by the RelOpInfos of the left and right Inputs,
            // the specified Join type, and the converted or default Join condition DbExpression.
            DbExpression retExpr = DbExpressionBuilder.CreateJoinExpressionByKind(
                joinKind,
                joinCond,
                leftInfo.CreateBinding(),
                rightInfo.CreateBinding()
            );

            // Create a new VarInfoList to hold the output Vars that are logically published by the new DbJoinExpression
            VarInfoList outputVars = new VarInfoList();

            //
            // Remove the right argument from scope. If the scopesPushed flag is true then the RelOpInfo
            // will be popped from the scope stack.
            //
            ExitExpressionBindingScope(rightInfo, scopesPushed);

            // In the record type that results from the join, the Vars published by the left argument
            // must now be accessed using an additional DbPropertyExpression that specifies the column name
            // used in the join (which was also the binding name used in the DbExpressionBinding created as part of the join)
            // PrependProperty is called on the published Vars of the right argument to reflect this, then they
            // are added to the overall set of Vars that are logically published by the new DbJoinExpression.
            // Note that calling ExitExpressionBindingScope has already unbound these Vars, making them
            // ready for use by the consumer of the new DbJoinExpression.
            rightInfo.PublishedVars.PrependProperty(rightInfo.PublisherName);
            outputVars.AddRange(rightInfo.PublishedVars);

            // Repeat the above steps for the left argument to the join
            ExitExpressionBindingScope(leftInfo, scopesPushed);
            leftInfo.PublishedVars.PrependProperty(leftInfo.PublisherName);
            outputVars.AddRange(leftInfo.PublishedVars);

            //
            // Update the tracked RelOpInfo for the new DbJoinExpression. This consists of:
            // PublisherName: The next Join alias.
            // PublishedVars: The PublishedVars of the Join are the (now unbound) PublishedVars of both Inputs, with appropriate column names prepended to their property paths.
            //
            PublishRelOp(_joinAliases.Next(), retExpr, outputVars);

            return retExpr;
        }

        public override DbExpression Visit(CrossJoinOp op, Node n)
        {
            // Create a new list of DbExpressionBinding to track the bindings that will be used in the new DbJoinExpression
            List<DbExpressionBinding> inputBindings = new List<DbExpressionBinding>();

            // Create a new VarInfoList to track the Vars that will be logically published by the new DbJoinExpression
            VarInfoList outputVars = new VarInfoList();

            //
            // For each Input Node:
            // 1. Visit and retrieve RelOpInfo for the Node, but do not bring it's Vars into scope
            //      (again to avoid Left-correlation between join Inputs).
            // 2. Use the RelOpInfo to create a correct Expressionbinding and add it to the list of ExpressionBindings
            // 3. Call ExitExpressionBinding, indicating that the RelOpInfo was not originally pushed onto the scope stack
            //      and so its published Vars should simply be unbound and the attempt should not be made to pop it from the scope stack.
            // 4. Update the property path for the Vars published by the Input to start with the same column name as was just used in the DbExpressionBinding for the Input
            // 5. Add the Vars published by the Input to the overall set of Vars that will be logically published by the new DbJoinExpression (created below).
            //
            foreach (Node inputNode in n.Children)
            {
                RelOpInfo inputInfo = VisitJoinInput(inputNode);
                inputBindings.Add(inputInfo.CreateBinding());
                ExitExpressionBindingScope(inputInfo, false);
                inputInfo.PublishedVars.PrependProperty(inputInfo.PublisherName);
                outputVars.AddRange(inputInfo.PublishedVars);
            }

            // Create a new DbJoinExpression from the list of DbExpressionBinding (implicitly creating a CrossJoin)
            DbExpression retExpr = DbExpressionBuilder.CrossJoin(inputBindings);

            // Update the DbExpression to RelOpInfo map to indicate that the overall set of Vars collected above are logically published by the new DbJoinExpression
            // PublisherName will be the next Join alias.
            PublishRelOp(_joinAliases.Next(), retExpr, outputVars);

            // Return the new DbJoinExpression
            return retExpr;
        }

        public override DbExpression Visit(InnerJoinOp op, Node n)
        {
            // Use common handling for binary Join Ops
            return VisitBinaryJoin(n, DbExpressionKind.InnerJoin);
        }

        public override DbExpression Visit(LeftOuterJoinOp op, Node n)
        {
            // Use common handling for binary Join Ops
            return VisitBinaryJoin(n, DbExpressionKind.LeftOuterJoin);
        }

        public override DbExpression Visit(FullOuterJoinOp op, Node n)
        {
            // Use common handling for binary Join Ops
            return VisitBinaryJoin(n, DbExpressionKind.FullOuterJoin);
        }

        #endregion

        #region ApplyOp Conversions - CrossApplyOp, OuterApplyOp

        /// <summary>
        /// Called by both CrossApply and OuterApply visitor pattern methods - command handling of both types of Apply operation
        /// </summary>
        /// <param name="applyNode">The Node that references the ApplyOp</param>
        /// <param name="applyKind">The CQT DbExpressionKind that corresponds to the ApplyOp (DbExpressionKind.CrossApply for CrossApplyOp, DbExpressionKind.OuterApply for OuterApplyOp)</param>
        /// <returns>A new CqtResult containing a DbApplyExpression with the correct ApplyType</returns>
        private DbExpression VisitApply(Node applyNode, DbExpressionKind applyKind)
        {            
            //
            // Visit the Input and bring its Vars into scope for the Apply
            //
            RelOpInfo inputInfo = EnterExpressionBindingScope(applyNode.Child0);

            //
            // Visit the Apply - there is no need to bring its Vars into scope
            //
            RelOpInfo applyInfo = EnterExpressionBindingScope(applyNode.Child1, false);

            DbExpression retExpr = DbExpressionBuilder.CreateApplyExpressionByKind(
                applyKind,
                inputInfo.CreateBinding(),
                applyInfo.CreateBinding());

            //
            // Unbind the Apply Vars by calling ExitExpressionBindingScope and indicating that the specified scope was not pushed onto the scope stack.
            //
            ExitExpressionBindingScope(applyInfo, false);

            //
            // Remove the Input Vars from scope and unbind them
            //
            ExitExpressionBindingScope(inputInfo);

            //
            // Update the property path to the Input and Apply vars appropriately based on the names used in their ExpressionBindings, which will then form the column names in the record output type of the AppyExpression
            //
            inputInfo.PublishedVars.PrependProperty(inputInfo.PublisherName);
            applyInfo.PublishedVars.PrependProperty(applyInfo.PublisherName);

            //
            // Build and publish the set of IQT Vars logically published by the DbApplyExpression
            // PublisherName: The next Apply alias.
            // PublishedVars: The PublishedVars of the Apply consists of the Vars published by the input plus those published by the apply.
            //
            VarInfoList outputVars = new VarInfoList();
            outputVars.AddRange(inputInfo.PublishedVars);
            outputVars.AddRange(applyInfo.PublishedVars);

            PublishRelOp(_applyAliases.Next(), retExpr, outputVars);

            return retExpr;
        }

        public override DbExpression Visit(CrossApplyOp op, Node n)
        {
            // Use common handling for Apply Ops
            return VisitApply(n, DbExpressionKind.CrossApply);
        }

        public override DbExpression Visit(OuterApplyOp op, Node n)
        {
            // Use common handling for Apply Ops
            return VisitApply(n, DbExpressionKind.OuterApply);
        }
        #endregion

        #region SetOp Conversions - ExceptOp, IntersectOp, UnionAllOp

        /// <summary>
        /// Called by VisitSetOp to convert each argument.
        /// Determines whether a column-reordering projection should be applied to
        /// the argument, and applies that projection if necessary during conversion
        /// to a DbExpression. A different projection is applied if no Nodes higher in
        /// the IQT consume the vars produced by the SetOp argument.
        /// </summary>
        /// <param name="argNode">
        /// A Node that provides one of the arguments to the SetOp
        /// </param>
        /// <param name="outputVars">
        /// Defines the expected order of the Output Vars of the SetOp
        /// </param>
        /// <param name="argVars">
        /// The VarMap for the SetOp argument represented by the node.
        /// This specifies the Output (SetOp-produced) Var to Input (Argument-produced)
        /// Var mappings for the Vars in the outputVars enumerable.
        /// </param>
        /// <returns>
        /// A DbExpression that is the converted form of the argument
        /// (with an appropriate column-reording projection applied if necessary)
        /// </returns>
        private DbExpression VisitSetOpArgument(Node argNode, VarVec outputVars, VarMap argVars)
        {
            RelOpInfo sourceInfo;

            List<Var> projectionVars = new List<Var>();

            //
            // If the list of output vars is empty, no higher Nodes required the vars produced by
            // this SetOp argument. A projection must therefore be applied that performs the equivalent
            // of 'SELECT true FROM <SetOp Argument>'.
            //
            if (outputVars.IsEmpty)
            {
                sourceInfo = BuildEmptyProjection(argNode);
            }
            else
            {
                //
                // Build up the list of Vars that we want as the output for this argument.
                // The "outputVars" argument defines the order in which we need the outputs
                // 
                foreach (Var v in outputVars)
                {
                    projectionVars.Add(argVars[v]);
                }

                //
                // Build up a ProjectOp over the input that produces the required Output vars
                //
                sourceInfo = BuildProjection(argNode, projectionVars);
            }

            return sourceInfo.Publisher;
        }

        /// <summary>
        /// Called by UnionAll, Intersect and Except (SetOp) visitor pattern methods
        /// </summary>
        /// <param name="op">The visited SetOp</param>
        /// <param name="n">The Node that references the SetOp</param>
        /// <param name="alias">Alias to use when publishing the SetOp's Vars</param>
        /// <param name="setOpBuilder">Callback to construct the SetOp DbExpression from the left and right arguments</param>
        /// <returns>The DbExpression equivalent of the SetOp</returns>
        private DbExpression VisitSetOp(SetOp op, Node n, AliasGenerator alias, Func<DbExpression, DbExpression, DbExpression> setOpExpressionBuilder)
        {
            //
            // To be convertible to a CQT Except/Intersect/DbUnionAllExpression, the SetOp must have exactly 2 arguments.
            //
            AssertBinary(n);

            //
            // Convert the left and right arguments to expressions.
            //
            DbExpression left = VisitSetOpArgument(n.Child0, op.Outputs, op.VarMap[0]);
            DbExpression right = VisitSetOpArgument(n.Child1, op.Outputs, op.VarMap[1]);

            //
            // If the output of the SetOp is a collection of records then the Vars produced
            // by the SetOp must be prepended with the names of the record type's columns as
            // they are tracked up the tree by VarInfo instances.
            //
            CollectionType outputType = TypeHelpers.GetEdmType<CollectionType>(TypeHelpers.GetCommonTypeUsage(left.ResultType, right.ResultType));
            IEnumerator<EdmProperty> properties = null;
            RowType outputElementType = null;
            if (TypeHelpers.TryGetEdmType<RowType>(outputType.TypeUsage, out outputElementType))
            {
                properties = outputElementType.Properties.GetEnumerator();
            }
            
            //
            // The published Vars of the DbExpression produced from the SetOp must be its Output Vars.
            // These Output Vars are mapped to the Vars of each of the SetOp's arguments using an array
            // of VarMaps (one for each argument) on the SetOp.
            // A VarInfo instance is added to the published Vars list for each Output Var of the SetOp.
            // If the output type of the SetOp is a collection of a record type then each Var's PropertyPath
            // is updated to be the name of the corresponding record column.
            //
            VarInfoList publishedVars = new VarInfoList();
            foreach (Var outputVar in op.Outputs)
            {
                VarInfo newVarInfo = new VarInfo(outputVar);
                // Prepend a property name to this var, if the output is a record type
                if (outputElementType != null)
                {
                    if (!properties.MoveNext())
                    {
                        PlanCompiler.Assert(false, "Record columns don't match output vars");
                    }
                    newVarInfo.PrependProperty(properties.Current.Name);
                }
                publishedVars.Add(newVarInfo);
            }

            DbExpression retExpr = setOpExpressionBuilder(left, right);
            PublishRelOp(alias.Next(), retExpr, publishedVars);

            return retExpr;
        }

        public override DbExpression Visit(UnionAllOp op, Node n)
        {
            return VisitSetOp(op, n, _unionAllAliases, DbExpressionBuilder.UnionAll);
        }

        public override DbExpression Visit(IntersectOp op, Node n)
        {
            return VisitSetOp(op, n, _intersectAliases, DbExpressionBuilder.Intersect);
        }

        public override DbExpression Visit(ExceptOp op, Node n)
        {
            return VisitSetOp(op, n, _exceptAliases, DbExpressionBuilder.Except);
        }
        #endregion

        public override DbExpression Visit(DerefOp op, Node n)
        {
            throw EntityUtil.NotSupported();
        }

        public override DbExpression Visit(DistinctOp op, Node n)
        {
            //
            // Build a projection above the input that gets the "keys" of the 
            // DistinctOp.
            //
            RelOpInfo sourceInfo = BuildProjection(n.Child0, op.Keys);

            //
            // Build the Distinct expression now
            //
            DbExpression distinctExpr = sourceInfo.Publisher.Distinct();

            //
            // Publish the DbDistinctExpression's Vars:
            // PublisherName: The next Distinct alias
            // PublishedVars: The PublishedVars of the Distinct are the same (rebound) Vars published by its input
            //
            PublishRelOp(_distinctAliases.Next(), distinctExpr, sourceInfo.PublishedVars);
            
            return distinctExpr;
        }

        /// <summary>
        /// Convert SRO(e) => NewMultiset(Element(e'))
        ///   where e' is the CTree version of e
        /// Add a Project over e, if it does not already have a ProjectOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override DbExpression Visit(SingleRowOp op, Node n)
        {
            RelOpInfo inputInfo;
            DbExpression inputExpr;

            //
            // Build a Projection over the child - sqlgen gets very confused otherwise
            //
            if (n.Child0.Op.OpType != OpType.Project)
            {
                ExtendedNodeInfo childNodeInfo = _iqtCommand.GetExtendedNodeInfo(n.Child0);
                //
                // #484757: Handle "empty" projection lists due to projection pruning
                //
                if (childNodeInfo.Definitions.IsEmpty)
                {
                    inputInfo = BuildEmptyProjection(n.Child0);
                }
                else
                {
                    inputInfo = BuildProjection(n.Child0, childNodeInfo.Definitions);
                }
                inputExpr = inputInfo.Publisher;
            }
            else
            {
                inputExpr = VisitNode(n.Child0);
                AssertRelOp(inputExpr);
                inputInfo = ConsumeRelOp(inputExpr);
            }

            DbElementExpression elementExpr = inputExpr.Element();
            List<DbExpression> collectionElements = new List<DbExpression>();
            collectionElements.Add(elementExpr);
            DbNewInstanceExpression collectionExpr = DbExpressionBuilder.NewCollection(collectionElements);
            PublishRelOp(_elementAliases.Next(), collectionExpr, inputInfo.PublishedVars);

            return collectionExpr;
        }

        /// <summary>
        /// Convert SingleRowTableOp into NewMultisetOp(1) - a single element
        /// collection. The element type of the collection doesn't really matter
        /// </summary>
        /// <param name="op">SingleRowTableOp</param>
        /// <param name="n">current subtree</param>
        /// <returns>CQT expression</returns>
        public override DbExpression Visit(SingleRowTableOp op, Node n)
        {
            DbNewInstanceExpression collectionExpr = DbExpressionBuilder.NewCollection(new [] { DbExpressionBuilder.Constant(1) });
            PublishRelOp(_singleRowTableAliases.Next(), collectionExpr, new VarInfoList());
            return collectionExpr;
        }

        #endregion

        #region Variable Definition Ops
        public override DbExpression Visit(VarDefOp op, Node n)
        {
            //
            // VarDef and VarDefList are handled in the conversion of the Ops in which they are valid (by calls to EnterVarDefScope/EnterVarDefListScope).
            // If this method is called a VarDefOp exists in an invalid location in the IQT
            //
            PlanCompiler.Assert(false, "Unexpected VarDefOp");
            throw EntityUtil.NotSupported(System.Data.Entity.Strings.Iqt_CTGen_UnexpectedVarDef);
        }

        public override DbExpression Visit(VarDefListOp op, Node n)
        {
            //
            // VarDef and VarDefList are handled in the conversion of the Ops in which they are valid (by calls to EnterVarDefScope/EnterVarDefListScope).
            // If this method is called a VarDefListOp exists in an invalid location in the IQT
            //
            PlanCompiler.Assert(false, "Unexpected VarDefListOp");
            throw EntityUtil.NotSupported(System.Data.Entity.Strings.Iqt_CTGen_UnexpectedVarDefList);
        }
        #endregion

        #region PhysicalOps
        /// <summary>
        /// Translates the PhysicalProjectOp. Handles two cases. If the child is a ProjectOp,
        /// then we simply piggyback on the ProjectOp method, but with our list of Vars.
        ///
        /// Otherwise, we visit the child, and then create a DbProjectExpression above it.
        ///
        /// The reason we special case the first scenario is because we do not want to add
        /// an extra Project over a Project-over-Sort expression tree. This causes bad
        /// problems later down the line
        /// </summary>
        /// <param name="op">the PhysicalProjectOp</param>
        /// <param name="n">current subtree</param>
        /// <returns>the CQT expression corresponding to this subtree</returns>
        public override DbExpression Visit(PhysicalProjectOp op, Node n)
        {
            PlanCompiler.Assert(n.Children.Count == 1, "more than one input to physicalProjectOp?");

            //
            // Prune the output vars from the PhysicalProjectOp
            //
            VarList prunedOutputs = new VarList();
            foreach (Var v in op.Outputs)
            {
                if (!prunedOutputs.Contains(v))
                {
                    prunedOutputs.Add(v);
                }
            }
            op.Outputs.Clear();
            op.Outputs.AddRange(prunedOutputs);

            //
            // Build a Projection over the input with exactly the Vars that we want
            // 
            RelOpInfo sourceInfo = BuildProjection(n.Child0, op.Outputs);

            return sourceInfo.Publisher;
        }

        public override DbExpression Visit(SingleStreamNestOp op, Node n)
        {
            throw EntityUtil.NotSupported();
        }
        public override DbExpression Visit(MultiStreamNestOp op, Node n)
        {
            throw EntityUtil.NotSupported();
        }
        #endregion

        #endregion
    }
}
