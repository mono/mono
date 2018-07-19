//---------------------------------------------------------------------
// <copyright file="ITreeGenerator.cs" company="Microsoft">
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
    using System.Data.Entity;
    using System.Data.Entity.Util;
    using System.Data.Metadata.Edm;
    using System.Data.Query.InternalTrees;
    using System.Linq;

    internal class ITreeGenerator : DbExpressionVisitor<Node>
    {
        #region Nested Types
        /// <summary>
        /// Abstract base class for both DbExpressionBinding and LambdaFunction scopes
        /// </summary>
        private abstract class CqtVariableScope
        {
            internal abstract bool Contains(string varName);
            internal abstract Node this[string varName] { get; }
            /// <summary>
            /// Returns true if it is a lambda variable representing a predicate expression.
            /// </summary>
            internal abstract bool IsPredicate(string varName);
        }

        /// <summary>
        /// Represents a variable scope introduced by a CQT DbExpressionBinding, and therefore contains a single variable.
        /// </summary>
        private class ExpressionBindingScope : CqtVariableScope
        {
            private Command _tree;
            private string _varName;
            private Var _var;

            internal ExpressionBindingScope(Command iqtTree, string name, Var iqtVar)
            {
                _tree = iqtTree;
                _varName = name;
                _var = iqtVar;
            }

            internal override bool Contains(string name) { return (_varName == name); }
            internal override Node this[string name]
            {
                get
                {
                    PlanCompiler.Assert(name == _varName,"huh?");
                    return _tree.CreateNode(_tree.CreateVarRefOp(_var));
                }
            }
            internal override bool IsPredicate(string varName)
            {
                return false;
            }

            internal Var ScopeVar { get { return _var; } }
        }

        /// <summary>
        /// Represents a variable scope introduced by a LambdaFunction.
        /// </summary>
        private sealed class LambdaScope : CqtVariableScope
        {
            private readonly ITreeGenerator _treeGen;
            private readonly Command _command;
            /// <summary>
            /// varName : [node, IsPredicate]
            /// </summary>
            private readonly Dictionary<string, Tuple<Node, bool>> _arguments;
            private readonly Dictionary<Node, bool> _referencedArgs;

            internal LambdaScope(ITreeGenerator treeGen, Command command, Dictionary<string, Tuple<Node, bool>> args)
            {
                _treeGen = treeGen;
                _command = command;
                _arguments = args;
                _referencedArgs = new Dictionary<Node, bool>(_arguments.Count);
            }

            internal override bool Contains(string name) { return (_arguments.ContainsKey(name)); }
            internal override Node this[string name]
            {
                get
                {
                    PlanCompiler.Assert(_arguments.ContainsKey(name), "LambdaScope indexer called for invalid Var");
                    
                    Node argNode = _arguments[name].Item1;
                    if (_referencedArgs.ContainsKey(argNode))
                    {
                        // The specified argument has already been substituted into the
                        // IQT and so this substitution requires a copy of the argument.
                        VarMap mappedVars = null;

                        // This is a 'deep copy' operation that clones the entire subtree rooted at the node.
                        Node argCopy = OpCopier.Copy(_command, argNode, out mappedVars);

                        // If any Nodes in the copy of the argument produce Vars then the
                        // Node --> Var map must be updated to include them.
                        if (mappedVars.Count > 0)
                        {
                            List<Node> sources = new List<Node>(1);
                            sources.Add(argNode);

                            List<Node> copies = new List<Node>(1);
                            copies.Add(argCopy);

                            MapCopiedNodeVars(sources, copies, mappedVars);
                        }

                        argNode = argCopy;
                    }
                    else
                    {
                        // This is the first reference of the lambda argument, so the Node itself
                        // can be returned rather than a copy, but the dictionary that tracks
                        // whether or not an argument has been referenced needs to be updated.
                        _referencedArgs[argNode] = true;
                    }

                    return argNode;
                }
            }

            internal override bool IsPredicate(string name)
            {
                PlanCompiler.Assert(_arguments.ContainsKey(name), "LambdaScope indexer called for invalid Var");
                return _arguments[name].Item2;
            }

            private void MapCopiedNodeVars(IList<Node> sources, IList<Node> copies, Dictionary<Var, Var> varMappings)
            {
                PlanCompiler.Assert(sources.Count == copies.Count, "Source/Copy Node count mismatch");

                //
                // For each Source/Copy Node in the two lists:
                // - Recursively update the Node --> Var map for any child nodes
                // - If the Source Node is mapped to a Var, then retrieve the new Var
                //   produced by the Op copier that corresponds to that Source Var, and
                //   add an entry to the Node --> Var map that maps the Copy Node to the
                //   new Var.
                //
                for (int idx = 0; idx < sources.Count; idx++)
                {
                    Node sourceNode = sources[idx];
                    Node copyNode = copies[idx];

                    if (sourceNode.Children.Count > 0)
                    {
                        MapCopiedNodeVars(sourceNode.Children, copyNode.Children, varMappings);
                    }

                    Var sourceVar = null;
                    if (_treeGen.VarMap.TryGetValue(sourceNode, out sourceVar))
                    {
                        PlanCompiler.Assert(varMappings.ContainsKey(sourceVar), "No mapping found for Var in Var to Var map from OpCopier");
                        this._treeGen.VarMap[copyNode] = varMappings[sourceVar];
                    }
                }
            }
        }
        #endregion

        private static Dictionary<DbExpressionKind, OpType> s_opMap = InitializeExpressionKindToOpTypeMap();

        private readonly Command _iqtCommand;
        private readonly Stack<CqtVariableScope> _varScopes = new Stack<CqtVariableScope>();
        private readonly Dictionary<Node, Var> _varMap = new Dictionary<Node, Var>();
        private readonly Stack<EdmFunction> _functionExpansions = new Stack<EdmFunction>();
        /// <summary>
        /// Maintained for lambda and model-defined function applications (DbLambdaExpression and DbFunctionExpression).
        /// </summary>
        private readonly Dictionary<DbExpression, bool> _functionsIsPredicateFlag = new Dictionary<DbExpression, bool>();

        // Used to track which IsOf type filter expressions have already been processed
        private readonly HashSet<DbFilterExpression> _processedIsOfFilters = new HashSet<DbFilterExpression>();
        private readonly HashSet<DbTreatExpression> _fakeTreats = new HashSet<DbTreatExpression>();

        // leverage discriminator metadata in the top-level project when translating query mapping views...
        private readonly System.Data.Mapping.ViewGeneration.DiscriminatorMap _discriminatorMap;
        private readonly DbProjectExpression _discriminatedViewTopProject;


        /// <summary>
        /// Initialize the DbExpressionKind --> OpType mappings for DbComparisonExpression and DbArithmeticExpression
        /// </summary>
        private static Dictionary<DbExpressionKind, OpType> InitializeExpressionKindToOpTypeMap()
        {
            Dictionary<DbExpressionKind, OpType> opMap = new Dictionary<DbExpressionKind, OpType>(12);

            //
            // Arithmetic operators
            //
            opMap[DbExpressionKind.Plus] = OpType.Plus;
            opMap[DbExpressionKind.Minus] = OpType.Minus;
            opMap[DbExpressionKind.Multiply] = OpType.Multiply;
            opMap[DbExpressionKind.Divide] = OpType.Divide;
            opMap[DbExpressionKind.Modulo] = OpType.Modulo;
            opMap[DbExpressionKind.UnaryMinus] = OpType.UnaryMinus;

            //
            // Comparison operators
            //
            opMap[DbExpressionKind.Equals] = OpType.EQ;
            opMap[DbExpressionKind.NotEquals] = OpType.NE;
            opMap[DbExpressionKind.LessThan] = OpType.LT;
            opMap[DbExpressionKind.GreaterThan] = OpType.GT;
            opMap[DbExpressionKind.LessThanOrEquals] = OpType.LE;
            opMap[DbExpressionKind.GreaterThanOrEquals] = OpType.GE;

            return opMap;
        }

        internal Dictionary<Node, Var> VarMap { get { return _varMap; } }

        public static Command Generate(DbQueryCommandTree ctree)
        {
            return Generate(ctree, null);
        }

        /// <summary>
        /// Generate an IQT given a query command tree and discriminator metadata (available for certain query mapping views)
        /// </summary>
        internal static Command Generate(DbQueryCommandTree ctree, System.Data.Mapping.ViewGeneration.DiscriminatorMap discriminatorMap)
        {
            ITreeGenerator treeGenerator = new ITreeGenerator(ctree, discriminatorMap);
            return treeGenerator._iqtCommand;
        }

        private ITreeGenerator(DbQueryCommandTree ctree, System.Data.Mapping.ViewGeneration.DiscriminatorMap discriminatorMap)
        {
            //
            // Create a new IQT Command instance that uses the same metadata workspace as the incoming command tree
            //
            _iqtCommand = new Command(ctree.MetadataWorkspace);

            //
            // When translating a query mapping view matching the TPH discrimination pattern, remember the top level discriminator map 
            // (leveraged to produced a DiscriminatedNewInstanceOp for the top-level projection in the view)
            //
            if (null != discriminatorMap)
            {
                _discriminatorMap = discriminatorMap;
                // see System.Data.Mapping.ViewGeneration.DiscriminatorMap
                PlanCompiler.Assert(ctree.Query.ExpressionKind == DbExpressionKind.Project, 
                    "top level QMV expression must be project to match discriminator pattern");
                _discriminatedViewTopProject = (DbProjectExpression)ctree.Query;
            }

            //
            // For each Parameter declared by the command tree, add a ParameterVar to the set of parameter vars maintained by the conversion visitor.
            // Each ParameterVar has the same name and type as the corresponding parameter on the command tree.
            //
            foreach (KeyValuePair<string, TypeUsage> paramInfo in ctree.Parameters)
            {
                if (!ValidateParameterType(paramInfo.Value))
                {
                    throw EntityUtil.NotSupported(System.Data.Entity.Strings.ParameterTypeNotSupported(paramInfo.Key, paramInfo.Value.ToString()));
                }
                _iqtCommand.CreateParameterVar(paramInfo.Key, paramInfo.Value);
            }

            // Convert into an ITree
            _iqtCommand.Root = VisitExpr(ctree.Query);

            //
            // If the root of the tree is not a relop, build up a fake project over a
            // a singlerowtableOp.
            //   "s" => Project(SingleRowTableOp, "s")
            //
            if (!_iqtCommand.Root.Op.IsRelOp)
            {
                Node scalarExpr = ConvertToScalarOpTree(_iqtCommand.Root, ctree.Query);
                Node singletonTableNode = _iqtCommand.CreateNode(_iqtCommand.CreateSingleRowTableOp());
                Var newVar;
                Node varDefListNode = _iqtCommand.CreateVarDefListNode(scalarExpr, out newVar);
                ProjectOp projectOp = _iqtCommand.CreateProjectOp(newVar);


                Node newRoot = _iqtCommand.CreateNode(projectOp, singletonTableNode, varDefListNode);

                if (TypeSemantics.IsCollectionType(_iqtCommand.Root.Op.Type))
                {
                    UnnestOp unnestOp = _iqtCommand.CreateUnnestOp(newVar);
                    newRoot = _iqtCommand.CreateNode(unnestOp, varDefListNode.Child0);
                    newVar = unnestOp.Table.Columns[0];
                }

                _iqtCommand.Root = newRoot;
                _varMap[_iqtCommand.Root] = newVar;

            }

            //
            // Ensure that the topmost portion of the query is capped by a
            // PhysicalProject expression
            //
            _iqtCommand.Root = CapWithPhysicalProject(_iqtCommand.Root);
        }

        private static bool ValidateParameterType(TypeUsage paramType)
        {
            return (paramType != null && paramType.EdmType != null &&
                (TypeSemantics.IsPrimitiveType(paramType) || paramType.EdmType is EnumType));
        }

        #region DbExpressionVisitor Helpers

        private static RowType ExtractElementRowType(TypeUsage typeUsage)
        {
            return TypeHelpers.GetEdmType<RowType>(TypeHelpers.GetEdmType<CollectionType>(typeUsage).TypeUsage);
        }

#if DEBUG
        private static bool IsCollectionOfRecord(TypeUsage typeUsage)
        {
            CollectionType collectionType;
            return (TypeHelpers.TryGetEdmType<CollectionType>(typeUsage, out collectionType) &&
                    collectionType != null &&
                    TypeSemantics.IsRowType(collectionType.TypeUsage));
        }
#endif

        /// <summary>
        /// Is the current expression a predicate?
        /// </summary>
        /// <param name="expr">expr to check</param>
        /// <returns>true, if the expression is a predicate</returns>
        private bool IsPredicate(DbExpression expr)
        {
            if (TypeSemantics.IsPrimitiveType(expr.ResultType, PrimitiveTypeKind.Boolean))
            {
                switch (expr.ExpressionKind)
                {
                    case DbExpressionKind.Equals:
                    case DbExpressionKind.NotEquals:
                    case DbExpressionKind.LessThan:
                    case DbExpressionKind.LessThanOrEquals:
                    case DbExpressionKind.GreaterThan:
                    case DbExpressionKind.GreaterThanOrEquals:
                    case DbExpressionKind.And:
                    case DbExpressionKind.Or:
                    case DbExpressionKind.Not:
                    case DbExpressionKind.Like:
                    case DbExpressionKind.IsEmpty:
                    case DbExpressionKind.IsNull:
                    case DbExpressionKind.IsOf:
                    case DbExpressionKind.IsOfOnly:
                    case DbExpressionKind.Any:
                    case DbExpressionKind.All:
                        return true;
                    case DbExpressionKind.VariableReference:
                        var varRef = (DbVariableReferenceExpression)expr;
                        return ResolveScope(varRef).IsPredicate(varRef.VariableName);
                    case DbExpressionKind.Lambda:
                        {
                            // 
                            bool isPredicateFunction;
                            if (_functionsIsPredicateFlag.TryGetValue(expr, out isPredicateFunction))
                            {
                                return isPredicateFunction;
                            }
                            else
                            {
                                // It is important that IsPredicate is called after the expression has been visited, otherwise 
                                // _functionsIsPredicateFlag map will not contain an entry for the lambda
                                PlanCompiler.Assert(false, "IsPredicate must be called on a visited lambda expression");
                                return false;
                            }
                        }
                    case DbExpressionKind.Function:
                        {
                            // 
                            EdmFunction edmFunction = ((DbFunctionExpression)expr).Function;
                            if (edmFunction.HasUserDefinedBody)
                            {
                                bool isPredicateFunction;
                                if (_functionsIsPredicateFlag.TryGetValue(expr, out isPredicateFunction))
                                {
                                    return isPredicateFunction;
                                }
                                else
                                {
                                    // It is important that IsPredicate is called after the expression has been visited, otherwise 
                                    // _functionsIsPredicateFlag map will not contain an entry for the function with a definition
                                    PlanCompiler.Assert(false, "IsPredicate must be called on a visited function expression");
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                    default:
                        return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Callback to process an expression
        /// </summary>
        /// <param name="e">The expression to convert</param>
        /// <returns></returns>
        private delegate Node VisitExprDelegate(DbExpression e);

        private Node VisitExpr(DbExpression e)
        {
            if (e == null)
            {
                return null;
            }
            else
            {
                return e.Accept<Node>(this);
            }
        }

        /// <summary>
        /// Convert this expression into a "scalar value" ITree expression. There are two main
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        private Node VisitExprAsScalar(DbExpression expr)
        {
            if (expr == null)
            {
                return null;
            }

            Node node = VisitExpr(expr); // the real work
            node = ConvertToScalarOpTree(node, expr);
            return node;
        }

        /// <summary>
        /// Convert an Itree node into a scalar op tree
        /// </summary>
        /// <param name="node">the subtree</param>
        /// <param name="expr">the original CQT expression</param>
        /// <returns>the converted subtree</returns>
        private Node ConvertToScalarOpTree(Node node, DbExpression expr)
        {
            //
            // If the current expression is a collection, and we've simply produced a RelOp
            // then we need to add a CollectOp above a PhysicalProjectOp above the RelOp
            //
            if (node.Op.IsRelOp)
            {
                node = ConvertRelOpToScalarOpTree(node, expr.ResultType);
            }
            //
            // If the current expression is a boolean, and it is really a predicate, then
            // scalarize the predicate (ie) convert it into a "case when <predicate> then 'true' else 'false' end" expression
            // SQLBUDT #431406: handle 3-valued logic for all predicates except IsNull
            // Convert boolean predicate p into
            //    case when p then true when not(p) then false else null end
            //
            else if (IsPredicate(expr))
            {
                node = ConvertPredicateToScalarOpTree(node, expr);
            }

            return node;
        }

        /// <summary>
        /// Convert a rel op Itree node into a scalar op tree
        /// </summary>
        /// <param name="node"></param>
        /// <param name="resultType"></param>
        /// <returns></returns>
        private Node ConvertRelOpToScalarOpTree(Node node, TypeUsage resultType)
        {
            PlanCompiler.Assert(TypeSemantics.IsCollectionType(resultType), "RelOp with non-Collection result type");
            CollectOp collectOp = _iqtCommand.CreateCollectOp(resultType);
            //
            // I'm not thrilled about having to build a PhysicalProjectOp here - this
            // is definitely something I will need to revisit soon
            //
            Node projectNode = CapWithPhysicalProject(node);
            node = _iqtCommand.CreateNode(collectOp, projectNode);

            return node;
        }

        /// <summary>
        /// Scalarize the predicate (x = y) by converting it into a "case when x = y then 'true' else 'false' end" expression.
        /// </summary>
        private Node ConvertPredicateToScalarOpTree(Node node, DbExpression expr)
        {
            CaseOp caseOp = _iqtCommand.CreateCaseOp(_iqtCommand.BooleanType);

            //For 2-valued logic there are 3 arguments, for 3-valued there are 5
            List<Node> arguments = new List<Node>((expr.ExpressionKind == DbExpressionKind.IsNull) ? 3 : 5);

            //Add the original as the first when
            arguments.Add(node);

            //Add the first then, the true node
            arguments.Add(_iqtCommand.CreateNode(_iqtCommand.CreateInternalConstantOp(_iqtCommand.BooleanType, true)));

            //If the expression has 3-valued logic, add a second when
            if (expr.ExpressionKind != DbExpressionKind.IsNull)
            {
                Node predCopy = VisitExpr(expr);
                arguments.Add(_iqtCommand.CreateNode(_iqtCommand.CreateConditionalOp(OpType.Not), predCopy));
            }

            //Add the false node: for 3 valued logic this is the second then, for 2 valued the else
            arguments.Add(_iqtCommand.CreateNode(_iqtCommand.CreateInternalConstantOp(_iqtCommand.BooleanType, false)));

            //The null node, it is the else-clause for 3-valued logic
            if (expr.ExpressionKind != DbExpressionKind.IsNull)
            {
                arguments.Add(_iqtCommand.CreateNode(_iqtCommand.CreateNullOp(_iqtCommand.BooleanType)));
            }

            node = _iqtCommand.CreateNode(caseOp, arguments);
            return node;
        }

        /// <summary>
        /// Convert an expression into an iqt predicate
        /// </summary>
        /// <param name="expr">the expression to process</param>
        /// <returns></returns>
        private Node VisitExprAsPredicate(DbExpression expr)
        {
            if (expr == null)
            {
                return null;
            }

            Node node = VisitExpr(expr);

            //
            // If the current expression is not a predicate, then we need to make it one, by
            // comparing it with the constant 'true'
            //
            if (!IsPredicate(expr))
            {
                ComparisonOp comparisonOp = _iqtCommand.CreateComparisonOp(OpType.EQ);
                Node trueNode = _iqtCommand.CreateNode(_iqtCommand.CreateInternalConstantOp(_iqtCommand.BooleanType, true));
                node = _iqtCommand.CreateNode(comparisonOp, node, trueNode);
            }
            else
            {
                PlanCompiler.Assert(!node.Op.IsRelOp, "unexpected relOp as predicate?");
            }

            return node;
        }

        /// <summary>
        /// Process a list of expressions, and apply the delegate to each of the expressions
        /// </summary>
        /// <param name="exprs">list of cqt expressions to process</param>
        /// <param name="exprDelegate">the callback to apply</param>
        /// <returns>a list of IQT expressions</returns>
        private static IList<Node> VisitExpr(IList<DbExpression> exprs, VisitExprDelegate exprDelegate)
        {
            List<Node> nodeList = new List<Node>();
            for(int idx = 0; idx < exprs.Count; idx++)
            {
                nodeList.Add(exprDelegate(exprs[idx]));
            }
            return nodeList;
        }

        /// <summary>
        /// Process a set of cqt expressions - and convert them into scalar iqt expressions
        /// </summary>
        /// <param name="exprs">list of cqt expressions</param>
        /// <returns>list of iqt expressions</returns>
        private IList<Node> VisitExprAsScalar(IList<DbExpression> exprs)
        {
            return VisitExpr(exprs, VisitExprAsScalar);
        }

        private Node VisitUnary(DbUnaryExpression e, Op op, VisitExprDelegate exprDelegate)
        {
            return _iqtCommand.CreateNode(op, exprDelegate(e.Argument));
        }

        private Node VisitBinary(DbBinaryExpression e, Op op, VisitExprDelegate exprDelegate)
        {
            return _iqtCommand.CreateNode(op, exprDelegate(e.Left), exprDelegate(e.Right));
        }
        
        /// <summary>
        /// Ensures that an input op is a RelOp. If the specified Node's Op is not a RelOp then it is wrapped in an Unnest to create a synthetic RelOp. This is only possible if the input Op produces a collection.
        /// </summary>
        /// <param name="inputNode">The input Node/Op pair</param>
        /// <returns>A Node with an Op that is guaranteed to be a RelOp (this may be the original Node or a new Node created to perform the Unnest)</returns>
        private Node EnsureRelOp(Node inputNode)
        {
            //
            // Input node = N1
            //
            Op inputOp = inputNode.Op;

            //
            // If the Op is already a RelOp then simply return its Node
            //
            if (inputOp.IsRelOp)
            {
                return inputNode;
            }

            //
            // Assert that the input is a ScalarOp (CQT expressions should only ever produce RelOps or ScalarOps)
            //
            ScalarOp scalar = inputOp as ScalarOp;
            PlanCompiler.Assert(scalar != null, "An expression in a CQT produced a non-ScalarOp and non-RelOp output Op");
            
            //
            // Assert that the ScalarOp has a collection result type. EnsureRelOp is called to ensure that arguments to
            // RelOps are either also RelOps or are ScalarOps that produce a collection, which can be wrapped in an
            // unnest to produce a RelOp.
            //
            PlanCompiler.Assert(TypeSemantics.IsCollectionType(scalar.Type), "An expression used as a RelOp argument was neither a RelOp or a collection");

            //
            // If the ScalarOp represents the nesting of an existing RelOp, simply return that RelOp instead.
            // CollectOp(PhysicalProjectOp(x)) => x
            //
            CollectOp collect = inputOp as CollectOp;
            if (collect != null)
            {
                PlanCompiler.Assert(inputNode.HasChild0, "CollectOp without argument");
                if (inputNode.Child0.Op as PhysicalProjectOp != null)
                {
                    PlanCompiler.Assert(inputNode.Child0.HasChild0, "PhysicalProjectOp without argument");
                    PlanCompiler.Assert(inputNode.Child0.Child0.Op.IsRelOp, "PhysicalProjectOp applied to non-RelOp input");

                    //
                    // The structure of the Input is Collect(PhysicalProject(x)), so return x
                    //
                    return inputNode.Child0.Child0;
                }
            }

            //
            // Create a new VarDefOp that defines the computed var that represents the ScalarOp collection.
            // This var is the input to the UnnestOp.
            // varDefNode = N2
            //
            Var inputCollectionVar;
            Node varDefNode = _iqtCommand.CreateVarDefNode(inputNode, out inputCollectionVar);

            //
            // Create an UnnestOp that references the computed var created above. The VarDefOp that defines the var
            // using the original input Node/Op pair becomes a child of the UnnestOp.
            //
            UnnestOp unnest = _iqtCommand.CreateUnnestOp(inputCollectionVar);
            PlanCompiler.Assert(unnest.Table.Columns.Count == 1, "Unnest of collection ScalarOp produced unexpected number of columns (1 expected)");

            //
            // Create the unnest node, N3
            // The UnnestOp produces a new Var, the single ColumnVar produced by the table that results from the Unnest.
            //
            Node unnestNode = _iqtCommand.CreateNode(unnest, varDefNode);
            _varMap[unnestNode] = unnest.Table.Columns[0];

            //
            // Create a Project node above the Unnest, so we can simplify the work to eliminate
            // the Unnest later.  That means we need to create a VarRef to the column var in the
            // table, a VarDef to define it, and a VarDefList to hold it, then a Project node, N4,
            // which we return.
            //
            Var projectVar;
            Node varRefNode = _iqtCommand.CreateNode(_iqtCommand.CreateVarRefOp(unnest.Table.Columns[0]));
            Node varDefListNode = _iqtCommand.CreateVarDefListNode(varRefNode, out projectVar);

            ProjectOp projectOp = _iqtCommand.CreateProjectOp(projectVar);
            Node projectNode = _iqtCommand.CreateNode(projectOp, unnestNode, varDefListNode);
            
            _varMap[projectNode] = projectVar;
            
            return projectNode;
        }

        /// <summary>
        /// Cap a RelOp with a ProjectOp. The output var of the Project is the
        /// output var from the input
        /// </summary>
        /// <param name="input">the input relop tree</param>
        /// <returns>the relop tree with a projectNode at the root</returns>
        private Node CapWithProject(Node input)
        {
            PlanCompiler.Assert(input.Op.IsRelOp, "unexpected non-RelOp?");
            if (input.Op.OpType == OpType.Project)
            {
                return input;
            }

            // Get the Var from the input; and build up a Project above it
            Var inputVar = _varMap[input];
            ProjectOp projectOp = _iqtCommand.CreateProjectOp(inputVar);
            Node projectNode = _iqtCommand.CreateNode(projectOp, input,
               _iqtCommand.CreateNode(_iqtCommand.CreateVarDefListOp()));
            _varMap[projectNode] = inputVar;

            return projectNode;
        }

        /// <summary>
        /// Cap a relop tree with a PhysicalProjectOp. The Vars of the PhysicalProjectOp
        /// are the vars from the RelOp tree
        /// </summary>
        /// <param name="input">the input relop tree</param>
        /// <returns>relop tree capped by a PhysicalProjectOp</returns>
        private Node CapWithPhysicalProject(Node input)
        {
            PlanCompiler.Assert(input.Op.IsRelOp, "unexpected non-RelOp?");            

            // Get the Var from the input; and build up a Project above it
            Var inputVar = _varMap[input];
            PhysicalProjectOp projectOp = _iqtCommand.CreatePhysicalProjectOp(inputVar);
            Node projectNode = _iqtCommand.CreateNode(projectOp, input);

            return projectNode;
        }

        /// <summary>
        /// Creates a new variable scope that is based on a CQT DbExpressionBinding and pushes it onto the variable scope stack. The scope defines a single variable based on the DbExpressionBinding's VarName and DbExpression.
        /// </summary>
        /// <param name="binding">The DbExpressionBinding that defines the scope</param>
        /// <returns>The Node produced by converting the binding's DbExpression</returns>
        private Node EnterExpressionBinding(DbExpressionBinding binding)
        {
            return VisitBoundExpressionPushBindingScope(binding.Expression, binding.VariableName);
        }

        /// <summary>
        /// Creates a new variable scope that is based on a CQT DbGroupExpressionBinding and pushes it onto the variable scope stack. The scope defines a single variable based on the DbExpressionBinding's VarName and DbExpression.
        /// This method does not bring the GroupVarName into scope. Note that ExitExpressionBinding and NOT ExitGroupExpressionBinding should be used to remove this scope from the stack.
        /// </summary>
        /// <param name="binding">The DbGroupExpressionBinding that defines the scope</param>
        /// <returns>The Node produced by converting the binding's DbExpression</returns>
        private Node EnterGroupExpressionBinding(DbGroupExpressionBinding binding)
        {
            return VisitBoundExpressionPushBindingScope(binding.Expression, binding.VariableName);
        }

        /// <summary>
        /// Common implementation method called by both EnterExpressionBinding and EnterGroupExpressionBinding
        /// </summary>
        /// <param name="boundExpression">The DbExpression that defines the binding</param>
        /// <param name="bindingName">The name of the binding variable</param>
        /// <returns></returns>
        private Node VisitBoundExpressionPushBindingScope(DbExpression boundExpression, string bindingName)
        {
            Var boundVar;
            Node inputNode = VisitBoundExpression(boundExpression, out boundVar);
            PushBindingScope(boundVar, bindingName);
            return inputNode;
        }

        /// <summary>
        /// Common implementation method called by both VisitBoundExpressionPushBindingScope and VisitJoin
        /// </summary>
        /// <param name="boundExpression">The DbExpression that defines the binding</param>
        /// <param name="boundVar">Var representing the RelOp produced for the <paramref name="boundExpression"/></param>
        /// <returns></returns>
        private Node VisitBoundExpression(DbExpression boundExpression, out Var boundVar)
        {
            //
            // Visit the expression binding's DbExpression to convert it to a Node/Op pair
            //
            Node inputNode = VisitExpr(boundExpression);
            PlanCompiler.Assert(inputNode != null, "DbExpressionBinding.Expression produced null conversion");

            //
            // Call EnsureRelOp on the converted Node and set inputNode equal to the result
            //
            inputNode = EnsureRelOp(inputNode);

            //
            // Retrieve the Var produced by the RelOp from the Node --> Var map
            //
            boundVar = _varMap[inputNode];
            PlanCompiler.Assert(boundVar != null, "No Var found for Input Op");

            return inputNode;
        }

        /// <summary>
        /// Common implementation method called by both VisitBoundExpressionPushBindingScope and VisitJoin
        /// </summary>
        /// <param name="boundVar">The Var produced by the RelOp from DbExpression that defines the binding</param>
        /// <param name="bindingName">The name of the binding variable</param>
        /// <returns></returns>
        private void PushBindingScope(Var boundVar, string bindingName)
        {
            //
            // Create a new ExpressionBindingScope using the VarName from the DbExpressionBinding and
            // the Var associated with the Input RelOp, and push the new scope onto the variable scope stack.
            //
            _varScopes.Push(new ExpressionBindingScope(_iqtCommand, bindingName, boundVar));
        }

        /// <summary>
        /// Removes a variable scope created based on a DbExpressionBinding from the top of the variable scope stack, verifying that it is in fact an ExpressionBindingScope.
        /// </summary>
        /// <returns>The removed ExpressionBindingScope</returns>
        private ExpressionBindingScope ExitExpressionBinding()
        {
            //
            // Pop the scope from the variable scope stack, assert that it is a DbExpressionBinding scope, and return it.
            //
            ExpressionBindingScope retScope = _varScopes.Pop() as ExpressionBindingScope;
            PlanCompiler.Assert(retScope != null, "ExitExpressionBinding called without ExpressionBindingScope on top of scope stack");
            return retScope;
        }

        /// <summary>
        /// Removes a variable scope created based on a DbGroupExpressionBinding from the top of the variable scope stack, verifying that it is in fact an ExpressionBindingScope.
        /// Should only be called after visiting the Aggregates of a DbGroupByExpression in Visit(DbGroupByExpression).
        /// The sequence (in Visit(GroupExpression e) is:
        /// 1. EnterGroupExpressionBinding
        /// 2.     Visit e.Keys
        /// 3. ExitExpressionBinding
        /// 4. (Push new scope with GroupVarName instead of VarName)
        /// 5.     Visit e.Aggregates
        /// 6. ExitGroupExpressionBinding
        /// </summary>
        private void ExitGroupExpressionBinding()
        {
            ExpressionBindingScope retScope = _varScopes.Pop() as ExpressionBindingScope;
            PlanCompiler.Assert(retScope != null, "ExitGroupExpressionBinding called without ExpressionBindingScope on top of scope stack");
        }

        /// <summary>
        /// Creates a new variable scope that is based on a CQT DbLambda and pushes it onto the variable scope stack.
        /// </summary>
        /// <param name="function">The DbLambda that defines the scope</param>
        /// <param name="argumentValues">A list of Nodes and IsPredicate bits produced by converting the CQT Expressions that provide the arguments to the Lambda function</param>
        /// <param name="expandingEdmFunction">an edm function for which the current lambda represents the generated body, otherwise null</param>
        private void EnterLambdaFunction(DbLambda lambda, List<Tuple<Node, bool>> argumentValues, EdmFunction expandingEdmFunction)
        {
            IList<DbVariableReferenceExpression> lambdaParams = lambda.Variables;

            var args = new Dictionary<string, Tuple<Node, bool>>();
            int idx = 0;
            foreach (var argumentValue in argumentValues)
            {
                args.Add(lambdaParams[idx].VariableName, argumentValue);
                idx++;
            }

            //
            // If lambda represents an edm function body then check for a possible recursion in the function definition.
            // 
            if (expandingEdmFunction != null)
            {
                //
                // Check if we are already inside the function body.
                //
                if (_functionExpansions.Contains(expandingEdmFunction))
                {
                    throw EntityUtil.CommandCompilation(Strings.Cqt_UDF_FunctionDefinitionWithCircularReference(expandingEdmFunction.FullName), null);
                }
                //
                // Push the function before processing its body
                //
                _functionExpansions.Push(expandingEdmFunction);
            }

            _varScopes.Push(new LambdaScope(this, _iqtCommand, args));
        }

        /// <summary>
        /// Removes a variable scope created based on a Lambda function from the top of the variable scope stack, verifying that it is in fact a LambdaScope.
        /// </summary>
        /// <param name="expandingEdmFunction">an edm function for which the current lambda represents the generated body, otherwise null</param>
        private LambdaScope ExitLambdaFunction(EdmFunction expandingEdmFunction)
        {
            //
            // Pop the scope from the variable scope stack, assert that it is a Lambda scope, and return it.
            //
            LambdaScope retScope = _varScopes.Pop() as LambdaScope;
            PlanCompiler.Assert(retScope != null, "ExitLambdaFunction called without LambdaScope on top of scope stack");

            //
            // If lambda represents an edm function body then pop the function from the expansion stack and make sure it is the expected one.
            //
            if (expandingEdmFunction != null)
            {
                EdmFunction edmFunction = _functionExpansions.Pop();
                PlanCompiler.Assert(edmFunction == expandingEdmFunction, "Function expansion stack corruption: unexpected function at the top of the stack");
            }

            return retScope;
        }

        /// <summary>
        /// Constructs a NewRecordOp on top of a multi-Var-producing Op, resulting in a RelOp that produces a single Var.
        /// </summary>
        /// <param name="inputNode">The Node that references the multi-Var-producing Op. This Node will become the first child node of the new ProjectOp's Node</param>
        /// <param name="recType">Type metadata that describes the output record type</param>
        /// <param name="colVars">A list of Vars that provide the output columns of the projection</param>
        /// <returns>A new ProjectOp that projects a new record of the specified type from the specified Vars over the original input Op/Node</returns>
        private Node ProjectNewRecord(Node inputNode, RowType recType, IEnumerable<Var> colVars)
        {
            //
            // Create a list of VarRefOp Nodes that provide the column values for the new record
            //
            List<Node> recordColumns = new List<Node>();
            foreach (Var colVar in colVars)
            {
                recordColumns.Add(_iqtCommand.CreateNode(_iqtCommand.CreateVarRefOp(colVar)));
            }

            //
            // Create the NewRecordOp Node using the record column nodes as its child nodes
            //
            Node newRecordNode = _iqtCommand.CreateNode(_iqtCommand.CreateNewRecordOp(recType), recordColumns);

            //
            // Create a new ComputedVar and a VarDefOp that uses the NewRecordOp Node to define it
            //
            Var newRecordVar;
            Node varDefNode = _iqtCommand.CreateVarDefListNode(newRecordNode, out newRecordVar);

            //
            // Create a ProjectOp with the single Computed Var defined by the new record construction
            //
            ProjectOp projection = _iqtCommand.CreateProjectOp(newRecordVar);
            Node projectionNode = _iqtCommand.CreateNode(projection, inputNode, varDefNode);
            _varMap[projectionNode] = newRecordVar;

            return projectionNode;
        }
        #endregion

        #region DbExpressionVisitor<Node> Members

        public override Node Visit(DbExpression e)
        {
            throw EntityUtil.NotSupported(System.Data.Entity.Strings.Cqt_General_UnsupportedExpression(e.GetType().FullName));
        }

        public override Node Visit(DbConstantExpression e)
        {
            // Don't use CreateInternalConstantOp - respect user-intent
            //
            // Note that it is only safe to call GetValue and access the 
            // constant value directly because any immutable values (byte[])
            // will be cloned as the result expression is built in CTreeGenerator,
            // during the call to DbExpressionBuilder.Constant in VisitConstantOp.
            ConstantBaseOp op = _iqtCommand.CreateConstantOp(e.ResultType, e.GetValue());
            return _iqtCommand.CreateNode(op);
        }

        public override Node Visit(DbNullExpression e)
        {
            NullOp op = _iqtCommand.CreateNullOp(e.ResultType);
            return _iqtCommand.CreateNode(op);
        }

        public override Node Visit(DbVariableReferenceExpression e)
        {
            Node varNode = ResolveScope(e)[e.VariableName];
            return varNode;
        }

        private CqtVariableScope ResolveScope(DbVariableReferenceExpression e)
        {
            //
            // Search the stack of variables scopes, top-down,
            // until the first one is found that defines a variable with the specified name.
            //
            foreach (CqtVariableScope scope in _varScopes)
            {
                if (scope.Contains(e.VariableName))
                {
                    return scope;
                }
            }

            //
            // If the variable name was not resolved then either:
            // 1. The original CQT was invalid (should not be allowed into the ITreeGenerator).
            // 2. The variable scope stack itself is invalid.
            //
            PlanCompiler.Assert(false, "CQT VarRef could not be resolved in the variable scope stack");
            return null;
        }

        public override Node Visit(DbParameterReferenceExpression e)
        {
            Op op = _iqtCommand.CreateVarRefOp(_iqtCommand.GetParameter(e.ParameterName));
            return _iqtCommand.CreateNode(op);
        }

        public override Node Visit(DbFunctionExpression e)
        {
            Node retNode = null;

            if (e.Function.IsModelDefinedFunction)
            {
                // This is a user-defined CSpace function with a body definition. 
                // Try expanding it:
                //  - replace the function call with the call to the body lambda,
                //  - visit the lambda call expression.

                // Get/generate the body lambda. Wrap body generation exceptions.
                DbLambda lambda;
                try
                {
                    lambda = _iqtCommand.MetadataWorkspace.GetGeneratedFunctionDefinition(e.Function);
                }
                catch (Exception exception)
                {
                    if (EntityUtil.IsCatchableExceptionType(exception))
                    {
                        throw EntityUtil.CommandCompilation(Strings.Cqt_UDF_FunctionDefinitionGenerationFailed(e.Function.FullName), exception);
                    }
                    throw;
                }

                // Visit the lambda call expression. 
                // Argument types should be validated by now, hence the visitor should not throw under normal conditions.
                retNode = VisitLambdaExpression(lambda, e.Arguments, e, e.Function);
            }
            else // a provider-manifest-defined or store function call - no expansion needed 
            {
                List<Node> argNodes = new List<Node>(e.Arguments.Count);
                for (int idx = 0; idx < e.Arguments.Count; idx++)
                {
                    // Ensure that any argument with a result type that does not exactly match the type of
                    // the corresponding function parameter is enclosed in a SoftCastOp.
                    argNodes.Add(BuildSoftCast(VisitExprAsScalar(e.Arguments[idx]), e.Function.Parameters[idx].TypeUsage));
                }

                retNode = _iqtCommand.CreateNode(_iqtCommand.CreateFunctionOp(e.Function), argNodes);
            }
            
            return retNode;
        }

        public override Node Visit(DbLambdaExpression e)
        {
            return VisitLambdaExpression(e.Lambda, e.Arguments, e, null);
        }

        private Node VisitLambdaExpression(DbLambda lambda, IList<DbExpression> arguments, DbExpression applicationExpr, EdmFunction expandingEdmFunction)
        {
            Node retNode = null;

            var argNodes = new List<Tuple<Node, bool>>(arguments.Count);
            foreach (DbExpression argExpr in arguments)
            {
                // #484709: Lambda function parameters should not have enclosing SoftCastOps.
                argNodes.Add(Tuple.Create(VisitExpr(argExpr), IsPredicate(argExpr)));
            }

            EnterLambdaFunction(lambda, argNodes, expandingEdmFunction);
            retNode = VisitExpr(lambda.Body);

            // Check the body to see if the current lambda yields a predicate.
            _functionsIsPredicateFlag[applicationExpr] = IsPredicate(lambda.Body);

            ExitLambdaFunction(expandingEdmFunction);

            return retNode;
        }

#if METHOD_EXPRESSION
        public override Node Visit(MethodExpression e)
        {
            throw EntityUtil.NotSupported();
        }
#endif
        #region SoftCast Helpers
        /// <summary>
        /// This method builds a "soft"Cast operator over the input node (if necessary) to (soft)
        /// cast it to the desired type (targetType)
        /// 
        /// If the input is a scalarOp, then we simply add on the SoftCastOp 
        /// directly (if it is needed, of course). If the input is a RelOp, we create a 
        /// new ProjectOp above the input, add a SoftCast above the Var of the
        /// input, and then return the new ProjectOp
        /// 
        /// The "need to cast" is determined by the Command.EqualTypes function. All type
        /// equivalence in the plan compiler is determined by this function
        /// </summary>
        /// <param name="node">the expression to soft-cast</param>
        /// <param name="targetType">the desired type to cast to</param>
        /// <returns></returns>
        private Node BuildSoftCast(Node node, TypeUsage targetType)
        {
            //
            // If the input is a RelOp (say X), and the Var of the input is "x",
            // we convert this into 
            //   Project(X, softCast(x, t))
            // where t is the element type of the desired target type
            // 
            if (node.Op.IsRelOp)
            {
                CollectionType targetCollectionType = TypeHelpers.GetEdmType<CollectionType>(targetType);
                targetType = targetCollectionType.TypeUsage;

                Var nodeVar = _varMap[node];
                // Do we need a cast at all?
                if (Command.EqualTypes(targetType, nodeVar.Type))
                {
                    return node;
                }

                // Build up the projectOp
                Var projectVar;
                Node varRefNode = _iqtCommand.CreateNode(_iqtCommand.CreateVarRefOp(nodeVar));
                Node castNode = _iqtCommand.CreateNode(_iqtCommand.CreateSoftCastOp(targetType), varRefNode);
                Node varDefListNode = _iqtCommand.CreateVarDefListNode(castNode, out projectVar);

                ProjectOp projectOp = _iqtCommand.CreateProjectOp(projectVar);
                Node projectNode = _iqtCommand.CreateNode(projectOp, node, varDefListNode);

                _varMap[projectNode] = projectVar;
                return projectNode;
            }
            else
            {
                PlanCompiler.Assert(node.Op.IsScalarOp, "I want a scalar op");
                if (Command.EqualTypes(node.Op.Type, targetType))
                {
                    return node;
                }
                else
                {
                    SoftCastOp castOp = _iqtCommand.CreateSoftCastOp(targetType);
                    return _iqtCommand.CreateNode(castOp, node);
                }
            }
        }

        /// <summary>
        /// A variant of the function above. Works with an EdmType instead
        /// of a TypeUsage, but leverages all the work above
        /// </summary>
        /// <param name="node">the node to "cast"</param>
        /// <param name="targetType">the desired type</param>
        /// <returns>the transformed expression</returns>
        private Node BuildSoftCast(Node node, EdmType targetType)
        {
            return BuildSoftCast(node, TypeUsage.Create(targetType));
        }

        private Node BuildEntityRef(Node arg, TypeUsage entityType)
        {
            TypeUsage refType = TypeHelpers.CreateReferenceTypeUsage((EntityType)entityType.EdmType);
            return _iqtCommand.CreateNode(_iqtCommand.CreateGetEntityRefOp(refType), arg);      
        }

        #endregion

        /// <summary>
        /// We simplify the property instance where the user is accessing a key member of 
        /// a reference navigation. The instance becomes simply the reference key in such
        /// cases.
        ///
        /// For instance, product.Category.CategoryID becomes Ref(product.Category).CategoryID,
        /// which gives us a chance of optimizing the query (using foreign keys rather than joins) 
        /// </summary>
        /// <param name="propertyExpression">The original property expression that specifies the member and instance</param>
        /// <param name="rewritten">'Simplified' instance. If the member is a key and the instance is a navigation
        /// the rewritten expression's instance is a reference navigation rather than the full entity.</param>
        /// <returns><c>true</c> if the property expression was rewritten, in which case <paramref name="rewritten"/> will be non-null,
        /// otherwise <c>false</c>, in which case <paramref name="rewritten"/> will be null.</returns>    
        private bool TryRewriteKeyPropertyAccess(DbPropertyExpression propertyExpression, out DbExpression rewritten)
        {
            // if we're accessing a key member of a navigation, collapse the structured instance
            // to the key reference.
            if (propertyExpression.Instance.ExpressionKind == DbExpressionKind.Property &&
                Helper.IsEntityType(propertyExpression.Instance.ResultType.EdmType))
            {
                EntityType instanceType = (EntityType)propertyExpression.Instance.ResultType.EdmType;
                DbPropertyExpression instanceExpression = (DbPropertyExpression)propertyExpression.Instance;
                if (Helper.IsNavigationProperty(instanceExpression.Property) &&
                    instanceType.KeyMembers.Contains(propertyExpression.Property))
                {
                    // modify the property expression so that it merely retrieves the reference
                    // not the entire entity
                    NavigationProperty navigationProperty = (NavigationProperty)instanceExpression.Property;

                    DbExpression navigationSource = instanceExpression.Instance.GetEntityRef();
                    DbExpression navigationExpression = navigationSource.Navigate(navigationProperty.FromEndMember, navigationProperty.ToEndMember);
                    rewritten = navigationExpression.GetRefKey();
                    rewritten = rewritten.Property(propertyExpression.Property.Name);
                    
                    return true;
                }
            }

            rewritten = null;
            return false;
        }

        public override Node Visit(DbPropertyExpression e)
        {
            // Only Properties, Relationship End and NavigationProperty members are supported.
            if (BuiltInTypeKind.EdmProperty != e.Property.BuiltInTypeKind &&
                BuiltInTypeKind.AssociationEndMember != e.Property.BuiltInTypeKind &&
                BuiltInTypeKind.NavigationProperty != e.Property.BuiltInTypeKind)
            {
                throw EntityUtil.NotSupported();
            }

            PlanCompiler.Assert(e.Instance != null, "Static properties are not supported");
            
            Node retNode = null;
            DbExpression rewritten;
            if (TryRewriteKeyPropertyAccess(e, out rewritten))
            {
                retNode = this.VisitExpr(rewritten);
            }
            else
            {
                Node instance = VisitExpr(e.Instance);

                //
                // Retrieving a property from a new instance constructor can be
                // simplified to just the node that provides the corresponding property.
                // For example, Property(Row(A = x, B = y), 'A') => x
                // All structured types (including association types) are considered.
                //
                if (e.Instance.ExpressionKind == DbExpressionKind.NewInstance &&
                    Helper.IsStructuralType(e.Instance.ResultType.EdmType))
                {
                    // Retrieve the 'structural' members of the instance's type.
                    // For Association types this should be only Association End members,
                    // while for Complex, Entity or Row types is should be only Properties.
                    System.Collections.IList propertyOrEndMembers = Helper.GetAllStructuralMembers(e.Instance.ResultType.EdmType);

                    // Find the position of the member with the same name as the retrieved
                    // member in the list of structural members. 
                    int memberIdx = -1;
                    for (int idx = 0; idx < propertyOrEndMembers.Count; idx++)
                    {
                        if (string.Equals(e.Property.Name, ((EdmMember)propertyOrEndMembers[idx]).Name, StringComparison.Ordinal))
                        {
                            memberIdx = idx;
                            break;
                        }
                    }

                    PlanCompiler.Assert(memberIdx > -1, "The specified property was not found");

                    // If the member was found, return the corresponding argument value
                    // to the new instance op.
                    retNode = instance.Children[memberIdx];

                    // Make sure the argument value has been "cast" to the return type
                    // of the property, if necessary.
                    retNode = BuildSoftCast(retNode, e.ResultType);
                }
                else
                {
                    Op op = _iqtCommand.CreatePropertyOp(e.Property);

                    // Make sure that the input has been "cast" to the right type
                    instance = BuildSoftCast(instance, e.Property.DeclaringType);
                    retNode = _iqtCommand.CreateNode(op, instance);
                }
            }

            return retNode;
        }

        public override Node Visit(DbComparisonExpression e)
        {
            Op op = _iqtCommand.CreateComparisonOp(s_opMap[e.ExpressionKind]);

            Node leftArg = VisitExprAsScalar(e.Left);
            Node rightArg = VisitExprAsScalar(e.Right);

            TypeUsage commonType = TypeHelpers.GetCommonTypeUsage(e.Left.ResultType, e.Right.ResultType);

            // Make sure that the inputs have been cast to the right types
            if (!Command.EqualTypes(e.Left.ResultType, e.Right.ResultType))
            {    
                leftArg = BuildSoftCast(leftArg, commonType);
                rightArg = BuildSoftCast(rightArg, commonType);
            }

            if (TypeSemantics.IsEntityType(commonType) &&
                (e.ExpressionKind == DbExpressionKind.Equals || e.ExpressionKind == DbExpressionKind.NotEquals))
            {
                // Entity (in)equality is implemented as ref (in)equality
                leftArg = BuildEntityRef(leftArg, commonType);
                rightArg = BuildEntityRef(rightArg, commonType);
            }

            return _iqtCommand.CreateNode(op, leftArg, rightArg);
        }

        public override Node Visit(DbLikeExpression e)
        {
            return _iqtCommand.CreateNode(
                _iqtCommand.CreateLikeOp(),
                VisitExpr(e.Argument),
                VisitExpr(e.Pattern),
                VisitExpr(e.Escape)
            );
        }

        private Node CreateLimitNode(Node inputNode, Node limitNode, bool withTies)
        {
            //
            // Limit(Skip(x)) - which becomes ConstrainedSortOp - and Limit(Sort(x)) are special cases
            //
            Node retNode = null;
            if (OpType.ConstrainedSort == inputNode.Op.OpType &&
                OpType.Null == inputNode.Child2.Op.OpType)
            {
                //
                // The input was a DbSkipExpression which is now represented
                // as a ConstrainedSortOp with a NullOp Limit. The Limit from
                // this DbLimitExpression can be merged into the input ConstrainedSortOp
                // rather than creating a new ConstrainedSortOp.
                //
                inputNode.Child2 = limitNode;

                // If this DbLimitExpression specifies WithTies, the input ConstrainedSortOp must be
                // updated to reflect this (DbSkipExpression always produces a ConstrainedSortOp with
                // WithTies equal to false).
                if (withTies)
                {
                    ((ConstrainedSortOp)inputNode.Op).WithTies = true;
                }

                retNode = inputNode;
            }
            else if (OpType.Sort == inputNode.Op.OpType)
            {
                //
                // This DbLimitExpression is applying a limit to a DbSortExpression.
                // The two expressions can be merged into a single ConstrainedSortOp
                // rather than creating a new ConstrainedSortOp over the input SortOp.
                //
                // The new ConstrainedSortOp has the same SortKeys as the input SortOp.
                // The returned Node will have the following children:
                // - The input to the Sort
                // - A NullOp to indicate no Skip operation is specified
                // - The limit Node from the DbLimitExpression
                //
                retNode =
                    _iqtCommand.CreateNode(
                        _iqtCommand.CreateConstrainedSortOp(((SortOp)inputNode.Op).Keys, withTies),
                        inputNode.Child0,
                        _iqtCommand.CreateNode(_iqtCommand.CreateNullOp(_iqtCommand.IntegerType)),
                        limitNode
                    );
            }
            else
            {
                //
                // The input to the Limit is neither ConstrainedSortOp or SortOp.
                // A new ConstrainedSortOp must be created with an empty list of keys
                // and the following children:
                // - The input to the DbLimitExpression
                // - a NullOp to indicate that no Skip operation is specified
                // - The limit Node from the DbLimitExpression
                //
                retNode =
                    _iqtCommand.CreateNode(
                        _iqtCommand.CreateConstrainedSortOp(new List<SortKey>(), withTies),
                        inputNode,
                        _iqtCommand.CreateNode(_iqtCommand.CreateNullOp(_iqtCommand.IntegerType)),
                        limitNode
                    );
            }

            return retNode;
        }

        public override Node Visit(DbLimitExpression expression)
        {
            //
            // Visit the Argument and retrieve its Var
            //
            Node inputNode = EnsureRelOp(VisitExpr(expression.Argument));
            Var inputVar = _varMap[inputNode];

            //
            // Visit the Limit ensuring that it is a scalar
            //
            Node limitNode = VisitExprAsScalar(expression.Limit);

            Node retNode;
            if (OpType.Project == inputNode.Op.OpType
                && (!AppSettings.SimplifyLimitOperations
                    || (OpType.Sort == inputNode.Child0.Op.OpType
                        || OpType.ConstrainedSort == inputNode.Child0.Op.OpType)))
            {
                //
                // If the input to the DbLimitExpression is a projection, then apply the Limit operation to the
                // input to the ProjectOp instead. This allows  Limit(Project(Skip(x))) and Limit(Project(Sort(x)))
                // to be treated in the same way as Limit(Skip(x)) and Limit(Sort(x)).
                // Note that even if the input to the projection is not a ConstrainedSortOp or SortOp, the
                // Limit operation is still pushed under the Project when the SimplifyLimitOperations AppSetting
                // is set to false. SimplifyLimitOperations is false by default.
                //
                inputNode.Child0 = CreateLimitNode(inputNode.Child0, limitNode, expression.WithTies);
                retNode = inputNode;
            }
            else
            {
                //
                // Otherwise, apply the Limit operation directly to the input.
                //
                retNode = CreateLimitNode(inputNode, limitNode, expression.WithTies);
            }

            //
            // The output Var of the resulting Node is the same as the output Var of its input Node.
            // If the input node is being returned (either because the Limit was pushed under a Project
            // or because the input was a ConstrainedSortOp that was simply updated with the Limit value)
            // then the Node -> Var map does not need to be updated.
            //
            if(!object.ReferenceEquals(retNode, inputNode))
            {
                _varMap[retNode] = inputVar;
            }
            
            return retNode;
        }

        public override Node Visit(DbIsNullExpression e)
        {
            // SQLBUDT #484294: We need to recognize and simplify IsNull - IsNull and IsNull - Not - IsNull
            // This is the latest point where such patterns can be easily recognized. 
            // After this the input predicate would get translated into a case statement.
            bool isAlwaysFalse = false;  //true if IsNull - IsNull and IsNull - Not - IsNull is recognized

            if (e.Argument.ExpressionKind == DbExpressionKind.IsNull)
            {
                isAlwaysFalse = true;
            }
            else if (e.Argument.ExpressionKind == DbExpressionKind.Not)
            {
                DbNotExpression notExpression = (DbNotExpression)e.Argument;
                if (notExpression.Argument.ExpressionKind == DbExpressionKind.IsNull)
                {
                    isAlwaysFalse = true;
                }
            }

            Op op = _iqtCommand.CreateConditionalOp(OpType.IsNull);

            //If we have recognized that the result is always false, return IsNull(true), to still have predicate as output. 
            //This gets further simplified by transformation rules.
            if (isAlwaysFalse)
            {
                return _iqtCommand.CreateNode(op, _iqtCommand.CreateNode(_iqtCommand.CreateInternalConstantOp(_iqtCommand.BooleanType, true)));
            }

            Node argNode = VisitExprAsScalar(e.Argument);
            if (TypeSemantics.IsEntityType(e.Argument.ResultType))
            {
                argNode = BuildEntityRef(argNode, e.Argument.ResultType);
            }

            return _iqtCommand.CreateNode(op, argNode);
        }

        public override Node Visit(DbArithmeticExpression e)
        {
            Op op = _iqtCommand.CreateArithmeticOp(s_opMap[e.ExpressionKind], e.ResultType);
            // Make sure that the inputs have been "cast" to the result type
            // Assumption: The input type must be the same as the result type. Is this always true?
            List<Node> children = new List<Node>();
            foreach (DbExpression arg in e.Arguments)
            {
                Node child = VisitExprAsScalar(arg);
                children.Add(BuildSoftCast(child, e.ResultType));
            }
            return _iqtCommand.CreateNode(op, children);
        }

        public override Node Visit(DbAndExpression e)
        {
            Op op = _iqtCommand.CreateConditionalOp(OpType.And);
            return VisitBinary(e, op, VisitExprAsPredicate);
        }

        public override Node Visit(DbOrExpression e)
        {
            Op op = _iqtCommand.CreateConditionalOp(OpType.Or);
            return VisitBinary(e, op, VisitExprAsPredicate);
        }

        public override Node Visit(DbNotExpression e)
        {
            Op op = _iqtCommand.CreateConditionalOp(OpType.Not);
            return VisitUnary(e, op, VisitExprAsPredicate);
        }

        public override Node Visit(DbDistinctExpression e)
        {
            Node inputSetNode = EnsureRelOp(VisitExpr(e.Argument));
            Var inputVar = _varMap[inputSetNode];
            Op distinctOp = _iqtCommand.CreateDistinctOp(inputVar);
            Node distinctNode = _iqtCommand.CreateNode(distinctOp, inputSetNode);
            _varMap[distinctNode] = inputVar;
            return distinctNode;
        }

        public override Node Visit(DbElementExpression e)
        {
            Op elementOp = _iqtCommand.CreateElementOp(e.ResultType);
            Node inputSetNode = EnsureRelOp(VisitExpr(e.Argument));
            
            // Add a soft cast if needed
            inputSetNode = BuildSoftCast(inputSetNode, TypeHelpers.CreateCollectionTypeUsage(e.ResultType));
            
            Var inputVar = _varMap[inputSetNode];

            //
            // Add a singleRowOp enforcer, as we are not guaranteed that the input
            // collection produces at most one row
            //
            inputSetNode = _iqtCommand.CreateNode(_iqtCommand.CreateSingleRowOp(), inputSetNode);
            _varMap[inputSetNode] = inputVar;

            // add a fake projectNode
            inputSetNode = CapWithProject(inputSetNode);
            return _iqtCommand.CreateNode(elementOp, inputSetNode);
        }

        public override Node Visit(DbIsEmptyExpression e)
        {
            //
            // IsEmpty(input set) --> Not(Exists(input set))
            //
            Op existsOp = _iqtCommand.CreateExistsOp();
            Node inputSetNode = EnsureRelOp(VisitExpr(e.Argument));

            return _iqtCommand.CreateNode(
                _iqtCommand.CreateConditionalOp(OpType.Not),
                _iqtCommand.CreateNode(existsOp, inputSetNode)
            );
        }
        
        /// <summary>
        /// Encapsulates the logic required to convert a SetOp (Except, Intersect, UnionAll) expression
        /// into an IQT Node/Op pair.
        /// </summary>
        /// <param name="expression">The DbExceptExpression, DbIntersectExpression or DbUnionAllExpression to convert, as an instance of DbBinaryExpression</param>
        /// <returns>A new IQT Node that references the ExceptOp, IntersectOp or UnionAllOp created based on the expression</returns>
        private Node VisitSetOpExpression(DbBinaryExpression expression)
        {
            PlanCompiler.Assert(DbExpressionKind.Except == expression.ExpressionKind ||
                         DbExpressionKind.Intersect == expression.ExpressionKind ||
                         DbExpressionKind.UnionAll == expression.ExpressionKind,
                         "Non-SetOp DbExpression used as argument to VisitSetOpExpression");

            PlanCompiler.Assert(TypeSemantics.IsCollectionType(expression.ResultType), "SetOp DbExpression does not have collection result type?");

            // Visit the left and right collection arguments
            Node leftNode = EnsureRelOp(VisitExpr(expression.Left));
            Node rightNode = EnsureRelOp(VisitExpr(expression.Right));

            //
            // Now the hard part. "Normalize" the left and right sides to
            // match the result type.
            //
            leftNode = BuildSoftCast(leftNode, expression.ResultType);
            rightNode = BuildSoftCast(rightNode, expression.ResultType);

            // The SetOp produces a single Var of the same type as the element type of the expression's collection result type
            Var outputVar = _iqtCommand.CreateSetOpVar(TypeHelpers.GetEdmType<CollectionType>(expression.ResultType).TypeUsage);

            // Create VarMaps for the left and right arguments that map the output Var to the Var produced by the corresponding argument
            VarMap leftMap = new VarMap();
            leftMap.Add(outputVar, _varMap[leftNode]);

            VarMap rightMap = new VarMap();
            rightMap.Add(outputVar, _varMap[rightNode]);

            // Create a SetOp that corresponds to the operation specified by the expression's DbExpressionKind
            Op setOp = null;
            switch(expression.ExpressionKind)
            {
                case DbExpressionKind.Except:
                    setOp = _iqtCommand.CreateExceptOp(leftMap, rightMap);
                    break;

                case DbExpressionKind.Intersect:
                    setOp = _iqtCommand.CreateIntersectOp(leftMap, rightMap);
                    break;

                case DbExpressionKind.UnionAll:
                    setOp = _iqtCommand.CreateUnionAllOp(leftMap, rightMap);
                    break;
            }

            // Create a new Node that references the SetOp
            Node setOpNode = _iqtCommand.CreateNode(setOp, leftNode, rightNode);

            // Update the Node => Var map with an entry that maps the new Node to the output Var
            _varMap[setOpNode] = outputVar;

            // Return the newly created SetOp Node
            return setOpNode;
        }

        public override Node Visit(DbUnionAllExpression e)
        {
            return VisitSetOpExpression(e);
        }

        public override Node Visit(DbIntersectExpression e)
        {
            return VisitSetOpExpression(e);
        }

        public override Node Visit(DbExceptExpression e)
        {
            return VisitSetOpExpression(e);
        }

        public override Node Visit(DbTreatExpression e)
        {
            Op op;
            if (_fakeTreats.Contains(e))
            {
                op = _iqtCommand.CreateFakeTreatOp(e.ResultType);
            }
            else
            {
                op = _iqtCommand.CreateTreatOp(e.ResultType);
            }
            return VisitUnary(e, op, VisitExprAsScalar);
        }

        public override Node Visit(DbIsOfExpression e)
        {
            Op op = null;
            if (DbExpressionKind.IsOfOnly == e.ExpressionKind)
            {
                op = _iqtCommand.CreateIsOfOnlyOp(e.OfType);
            }
            else
            {
                op = _iqtCommand.CreateIsOfOp(e.OfType);
            }
            return VisitUnary(e, op, VisitExprAsScalar);
        }

        public override Node Visit(DbCastExpression e)
        {
            Op op = _iqtCommand.CreateCastOp(e.ResultType);
            return VisitUnary(e, op, VisitExprAsScalar);
        }

        public override Node Visit(DbCaseExpression e)
        {
            List<Node> childNodes = new List<Node>();
            for (int idx = 0; idx < e.When.Count; idx++)
            {
                childNodes.Add(VisitExprAsPredicate(e.When[idx]));
                // Make sure that each then-clause is the same type as the result
                childNodes.Add(BuildSoftCast(VisitExprAsScalar(e.Then[idx]), e.ResultType));
            }

            // Make sure that the else-clause is the same type as the result
            childNodes.Add(BuildSoftCast(VisitExprAsScalar(e.Else), e.ResultType));
            return _iqtCommand.CreateNode(_iqtCommand.CreateCaseOp(e.ResultType), childNodes);
        }
            
        /// <summary>
        /// Represents one or more type filters that should be AND'd together to produce an aggregate IsOf filter expression
        /// </summary>
        private class IsOfFilter
        {
            /// <summary>
            /// The type that elements of the filtered input set must be to satisfy this IsOf filter
            /// </summary>
            private readonly TypeUsage requiredType;

            /// <summary>
            /// Indicates whether elements of the filtered input set may be of a subtype (IsOf) of the required type
            /// and still satisfy the IsOfFilter, or must be exactly of the required type (IsOfOnly) to do so.
            /// </summary>
            private readonly bool isExact;

            /// <summary>
            /// The next IsOfFilter in the AND chain.
            /// </summary>
            private IsOfFilter next;

            internal IsOfFilter(DbIsOfExpression template)
            {
                this.requiredType = template.OfType;
                this.isExact = (template.ExpressionKind == DbExpressionKind.IsOfOnly);
            }

            internal IsOfFilter(DbOfTypeExpression template)
            {
                this.requiredType = template.OfType;
                this.isExact = (template.ExpressionKind == DbExpressionKind.OfTypeOnly);
            }

            private IsOfFilter(TypeUsage required, bool exact)
            {
                this.requiredType = required;
                this.isExact = exact;
            }

            private IsOfFilter Merge(TypeUsage otherRequiredType, bool otherIsExact)
            {
                // Can the two type filters be merged? In general, a more specific
                // type filter can replace a less specific type filter.
                IsOfFilter result;
                bool typesEqual = this.requiredType.EdmEquals(otherRequiredType);

                // The simplest case - the filters are equivalent
                if (typesEqual && this.isExact == otherIsExact)
                {
                    result = this;
                }

                // Next simplest - two IsOfOnly filters can never be merged if the types are different
                // (and if the types were equal the above condition would have been satisfied).
                // SC_
                else if (this.isExact && otherIsExact)
                {
                    result = new IsOfFilter(otherRequiredType, otherIsExact);
                    result.next = this;
                }

                // Two IsOf filters can potentially be adjusted - the more specific type filter should be kept, if present
                else if (!this.isExact && !otherIsExact)
                {
                    // At this point the types cannot be equal. If one filter specifies a type that is a subtype of the other,
                    // then the subtype filter is the one that should remain
                    if (otherRequiredType.IsSubtypeOf(this.requiredType))
                    {
                        result = new IsOfFilter(otherRequiredType, false);
                        result.next = this.next;
                    }
                    else if (this.requiredType.IsSubtypeOf(otherRequiredType))
                    {
                        result = this;
                    }
                    else
                    {
                        // The types are not related and the filters cannot be merged
                        // Note that this case may not be possible since IsOf and OfType
                        // both require an argument with a compatible type to the IsOf type.
                        result = new IsOfFilter(otherRequiredType, otherIsExact);
                        result.next = this;
                    }
                }

                // One filter is an IsOf filter while the other is an IsOfOnly filter
                else
                {
                    // For IsOf(T) AND IsOfOnly(T), the IsOf filter can be dropped
                    if (typesEqual)
                    {
                        result = new IsOfFilter(otherRequiredType, true);
                        result.next = this.next;
                    }
                    else
                    {
                        // Decide which is the 'IsOfOnly' type and which is the 'IsOf' type
                        TypeUsage isOfOnlyType = (this.isExact ? this.requiredType : otherRequiredType);
                        TypeUsage isOfType = (this.isExact ? otherRequiredType : this.requiredType);

                        // IsOf(Super) && IsOfOnly(Sub) => IsOfOnly(Sub)
                        // In all other cases, both filters remain - even though the IsOfOnly(Super) and IsOf(Sub) is obviously a contradiction.
                        // SC_
                        if (isOfOnlyType.IsSubtypeOf(isOfType))
                        {
                            if (object.ReferenceEquals(isOfOnlyType, this.requiredType) && this.isExact)
                            {
                                result = this;
                            }
                            else
                            {
                                result = new IsOfFilter(isOfOnlyType, true);
                                result.next = this.next;
                            }
                        }
                        else
                        {
                            result = new IsOfFilter(otherRequiredType, otherIsExact);
                            result.next = this;
                        }
                    }
                }

                return result;
            }

            internal IsOfFilter Merge(DbIsOfExpression other)
            {
                return Merge(other.OfType, (other.ExpressionKind == DbExpressionKind.IsOfOnly));
            }

            internal IsOfFilter Merge(DbOfTypeExpression other)
            {
                return Merge(other.OfType, (other.ExpressionKind == DbExpressionKind.OfTypeOnly));
            }

            internal IEnumerable<KeyValuePair<TypeUsage, bool>> ToEnumerable()
            {
                IsOfFilter currentFilter = this;
                while (currentFilter != null)
                {
                    yield return new KeyValuePair<TypeUsage, bool>(currentFilter.requiredType, currentFilter.isExact);
                    currentFilter = currentFilter.next;
                }
            }
        }

        private DbFilterExpression CreateIsOfFilterExpression(DbExpression input, IsOfFilter typeFilter)
        {
            // Create a filter expression based on the IsOf/IsOfOnly operations specified by typeFilter
            DbExpressionBinding resultBinding = input.Bind();
            List<DbExpression> predicates = new List<DbExpression>(
                typeFilter.ToEnumerable().Select(tf => tf.Value ? resultBinding.Variable.IsOfOnly(tf.Key) : resultBinding.Variable.IsOf(tf.Key)).ToList()
            );
            DbExpression predicate = Helpers.BuildBalancedTreeInPlace(predicates, (left, right) => left.And(right));
            DbFilterExpression result = resultBinding.Filter(predicate);

            // Track the fact that this IsOfFilter was created by the ITreeGenerator itself and should
            // simply be converted to an ITree Node when it is encountered again by the visitor pass.
            _processedIsOfFilters.Add(result);
            return result;
        }

        private bool IsIsOfFilter(DbFilterExpression filter)
        {
            if(filter.Predicate.ExpressionKind != DbExpressionKind.IsOf &&
               filter.Predicate.ExpressionKind != DbExpressionKind.IsOfOnly)
            {
                return false;
            }
            
            DbExpression isOfArgument = ((DbIsOfExpression)filter.Predicate).Argument;
            return (isOfArgument.ExpressionKind == DbExpressionKind.VariableReference &&
                   ((DbVariableReferenceExpression)isOfArgument).VariableName == filter.Input.VariableName);
        }

        private DbExpression ApplyIsOfFilter(DbExpression current, IsOfFilter typeFilter)
        {
            // An IsOf filter can be safely pushed down through the following expressions:
            //
            // Distinct
            // Filter - may be merged if the Filter is also an OfType filter
            // OfType - converted to Project(Filter(input, IsOf(T)), TreatAs(T)) and the Filter may be merged
            // Project - only for identity project
            //           SC_








            DbExpression result;
            switch(current.ExpressionKind)
            {
                case DbExpressionKind.Distinct:
                    {
                        result = ApplyIsOfFilter(((DbDistinctExpression)current).Argument, typeFilter).Distinct();
                    }
                    break;

                case DbExpressionKind.Filter:
                    {
                        DbFilterExpression filter = (DbFilterExpression)current;
                        if (IsIsOfFilter(filter))
                        {
                            // If this is an IsOf filter, examine the interaction with the current filter we are trying to apply
                            DbIsOfExpression isOfExp = (DbIsOfExpression)filter.Predicate;
                            typeFilter = typeFilter.Merge(isOfExp);
                            result = ApplyIsOfFilter(filter.Input.Expression, typeFilter);
                        }
                        else
                        {
                            // Otherwise, push the current IsOf filter under this filter
                            DbExpression rewritten = ApplyIsOfFilter(filter.Input.Expression, typeFilter);
                            result = rewritten.BindAs(filter.Input.VariableName).Filter(filter.Predicate);
                        }
                    }
                    break;
                                    
                case DbExpressionKind.OfType:
                case DbExpressionKind.OfTypeOnly:
                    {
                        // Examine the interaction of this nested OfType filter with the OfType filter we are trying to apply
                        // and construct an aggregated type filter (where possible)
                        DbOfTypeExpression ofTypeExp = (DbOfTypeExpression)current;
                        typeFilter = typeFilter.Merge(ofTypeExp);
                        DbExpression rewrittenIsOf = ApplyIsOfFilter(ofTypeExp.Argument, typeFilter);
                        DbExpressionBinding treatBinding = rewrittenIsOf.Bind();
                        DbTreatExpression treatProjection = treatBinding.Variable.TreatAs(ofTypeExp.OfType);
                        _fakeTreats.Add(treatProjection);
                        result = treatBinding.Project(treatProjection);                        
                    }
                    break;

                case DbExpressionKind.Project:
                    {
                        DbProjectExpression project = (DbProjectExpression)current;
                        if(project.Projection.ExpressionKind == DbExpressionKind.VariableReference &&
                           ((DbVariableReferenceExpression)project.Projection).VariableName == project.Input.VariableName)
                        {
                            // If this is an identity-project, remove it by visiting the input expression
                            result = ApplyIsOfFilter(project.Input.Expression, typeFilter);
                        }
                        else
                        {
                            // Otherwise, the projection is opaque to the IsOf rewrite
                            result = CreateIsOfFilterExpression(current, typeFilter);
                        }
                    }
                    break;
                                    
                case DbExpressionKind.Sort:
                    {
                        // The IsOf filter is applied to the Sort input, then the sort keys are reapplied to create a new Sort expression.
                        DbSortExpression sort = (DbSortExpression)current;
                        DbExpression sortInput = ApplyIsOfFilter(sort.Input.Expression, typeFilter);
                        result = sortInput.BindAs(sort.Input.VariableName).Sort(sort.SortOrder);
                    }
                    break;
                               
                default:
                    {
                        // This is not a recognized case, so simply apply the type filter to the expression.
                        result = CreateIsOfFilterExpression(current, typeFilter);
                    }
                    break;
            }
            return result;
        }
                
        /// <summary>
        /// Build the equivalent of an OfTypeExpression over the input (ie) produce the set of values from the
        /// input that are of the desired type (exactly of the desired type, if the "includeSubtypes" parameter is false).
        /// 
        /// Further more, "update" the result element type to be the desired type.
        /// 
        /// We accomplish this by first building a FilterOp with an IsOf (or an IsOfOnly) predicate for the desired 
        /// type. We then build out a ProjectOp over the FilterOp, where we introduce a "Fake" TreatOp over the input
        /// element to cast it to the right type. The "Fake" TreatOp is only there for "compile-time" typing reasons,
        /// and will be ignored in the rest of the plan compiler
        /// </summary>
        // <param name="inputNode">the input collection</param>
        // <param name="inputVar">the single Var produced by the input collection</param>
        // <param name="desiredType">the desired element type </param>
        // <param name="includeSubtypes">do we include subtypes of the desired element type</param>
        // <param name="resultNode">the result subtree</param>
        // <param name="resultVar">the single Var produced by the result subtree</param>
        public override Node Visit(DbOfTypeExpression e)
        {
            //
            // The argument to OfType must be a collection
            //
            PlanCompiler.Assert(TypeSemantics.IsCollectionType(e.Argument.ResultType), "Non-Collection Type Argument in DbOfTypeExpression");

            DbExpression rewrittenIsOfFilter = ApplyIsOfFilter(e.Argument, new IsOfFilter(e));

            //
            // Visit the collection argument and ensure that it is a RelOp suitable for subsequent use in the Filter/Project used to convert OfType.
            //
            Node inputNode = EnsureRelOp(VisitExpr(rewrittenIsOfFilter));

            //
            // Retrieve the Var produced by the RelOp input.
            //
            Var inputVar = _varMap[inputNode];

            //
            // Build the Treat part of the OfType expression tree - note that this is a 'fake'
            // Treat because the underlying IsOf filter makes it unnecessary (as far as the
            // plan compiler is concerned).
            //
            Var resultVar;
            Node resultNode = _iqtCommand.BuildFakeTreatProject(inputNode, inputVar, e.OfType, out resultVar);

            //
            // Add the node-var mapping, and return
            //
            _varMap[resultNode] = resultVar;
            return resultNode;
        }

        public override Node Visit(DbNewInstanceExpression e)
        {
            Op newInstOp = null;
            List<Node> relPropertyExprs = null;
            if (TypeSemantics.IsCollectionType(e.ResultType))
            {
                newInstOp = _iqtCommand.CreateNewMultisetOp(e.ResultType);
            }
            else if (TypeSemantics.IsRowType(e.ResultType))
            {
                newInstOp = _iqtCommand.CreateNewRecordOp(e.ResultType);
            }
            else if (TypeSemantics.IsEntityType(e.ResultType))
            {
                List<RelProperty> relPropertyList = new List<RelProperty>();
                relPropertyExprs = new List<Node>();
                if (e.HasRelatedEntityReferences)
                {
                    foreach (DbRelatedEntityRef targetRef in e.RelatedEntityReferences)
                    {
                        RelProperty relProperty = new RelProperty((RelationshipType)targetRef.TargetEnd.DeclaringType, targetRef.SourceEnd, targetRef.TargetEnd);
                        relPropertyList.Add(relProperty);
                        Node relPropertyNode = VisitExprAsScalar(targetRef.TargetEntityReference);
                        relPropertyExprs.Add(relPropertyNode);
                    }
                }
                newInstOp = _iqtCommand.CreateNewEntityOp(e.ResultType, relPropertyList);
            }
            else
            {
                newInstOp = _iqtCommand.CreateNewInstanceOp(e.ResultType);
            }

            // 
            // Build up the list of arguments. Make sure that they match 
            // the expected types (and add "soft" casts, if needed)
            //
            List<Node> newArgs = new List<Node>();
            if (TypeSemantics.IsStructuralType(e.ResultType))
            {
                StructuralType resultType = TypeHelpers.GetEdmType<StructuralType>(e.ResultType);
                int i = 0;
                foreach (EdmMember m in TypeHelpers.GetAllStructuralMembers(resultType))
                {
                    Node newArg = BuildSoftCast(VisitExprAsScalar(e.Arguments[i]), Helper.GetModelTypeUsage(m));
                    newArgs.Add(newArg);
                    i++;
                }
            }
            else
            {
                CollectionType resultType = TypeHelpers.GetEdmType<CollectionType>(e.ResultType);
                TypeUsage elementTypeUsage = resultType.TypeUsage;
                foreach (DbExpression arg in e.Arguments)
                {
                    Node newArg = BuildSoftCast(VisitExprAsScalar(arg), elementTypeUsage);
                    newArgs.Add(newArg);
                }
            }

            if (relPropertyExprs != null)
            {
                newArgs.AddRange(relPropertyExprs);
            }
            Node node = _iqtCommand.CreateNode(newInstOp, newArgs);

            return node;
        }

        public override Node Visit(DbRefExpression e)
        {
            // SQLBUDT #502617: Creating a collection of refs throws an Assert
            // A SoftCastOp may be required if the argument to the RefExpression is only promotable
            // to the row type produced from the key properties of the referenced Entity type. Since
            // this row type is not actually represented anywhere in the tree it must be built here in
            // order to determine whether or not the SoftCastOp should be applied.
            //
            Op op = _iqtCommand.CreateRefOp(e.EntitySet, e.ResultType);
            Node newArg = BuildSoftCast(VisitExprAsScalar(e.Argument), TypeHelpers.CreateKeyRowType(e.EntitySet.ElementType)); 
            return _iqtCommand.CreateNode(op, newArg);
        }

        public override Node Visit(DbRelationshipNavigationExpression e)
        {
            RelProperty relProperty = new RelProperty(e.Relationship, e.NavigateFrom, e.NavigateTo);
            Op op = _iqtCommand.CreateNavigateOp(e.ResultType, relProperty);
            Node arg = VisitExprAsScalar(e.NavigationSource);
            return _iqtCommand.CreateNode(op, arg);
        }

        public override Node Visit(DbDerefExpression e)
        {
            Op op = _iqtCommand.CreateDerefOp(e.ResultType);
            return VisitUnary(e, op, VisitExprAsScalar);
        }

        public override Node Visit(DbRefKeyExpression e)
        {
            Op op = _iqtCommand.CreateGetRefKeyOp(e.ResultType);
            return VisitUnary(e, op, VisitExprAsScalar);
        }

        public override Node Visit(DbEntityRefExpression e)
        {
            Op op = _iqtCommand.CreateGetEntityRefOp(e.ResultType);
            return VisitUnary(e, op, VisitExprAsScalar);
        }

        public override Node Visit(DbScanExpression e)
        {
            // Create a new table definition
            TableMD tableMetadata = Command.CreateTableDefinition(e.Target);
            
            // Create a scan table operator
            ScanTableOp op = _iqtCommand.CreateScanTableOp(tableMetadata);

            // Map the ScanTableOp to the ColumnVar of the Table's single column of the Extent's element type
            Node node = _iqtCommand.CreateNode(op);
            Var singleColumn = op.Table.Columns[0];
            _varMap[node] = singleColumn;

            return node;
        }

        public override Node Visit(DbFilterExpression e)
        {
            if (!IsIsOfFilter(e) || _processedIsOfFilters.Contains(e))
            {
                //
                // Visit the Predicate with the Input binding's variable in scope
                //
                Node inputSetNode = EnterExpressionBinding(e.Input);
                Node predicateNode = VisitExprAsPredicate(e.Predicate);
                ExitExpressionBinding();

                Op filtOp = _iqtCommand.CreateFilterOp();

                // Update the Node --> Var mapping. Filter maps to the same Var as its input.
                Node filtNode = _iqtCommand.CreateNode(filtOp, inputSetNode, predicateNode);
                _varMap[filtNode] = _varMap[inputSetNode];

                return filtNode;
            }
            else
            {
                DbIsOfExpression isOfPredicate = (DbIsOfExpression)e.Predicate;
                DbExpression processed = ApplyIsOfFilter(e.Input.Expression, new IsOfFilter(isOfPredicate));
                return this.VisitExpr(processed);
            }
        }

        public override Node Visit(DbProjectExpression e)
        {
            // check if this is the discriminated projection for a query mapping view
            if (e == this._discriminatedViewTopProject)
            {
                return GenerateDiscriminatedProject(e);
            }
            else
            {
                return GenerateStandardProject(e);
            }
        }

        private Node GenerateDiscriminatedProject(DbProjectExpression e)
        {
            PlanCompiler.Assert(null != _discriminatedViewTopProject, "if a project matches the pattern, there must be a corresponding discriminator map");

            // convert the input to the top level projection
            Node source = EnterExpressionBinding(e.Input);

            List<RelProperty> relPropertyList = new List<RelProperty>();
            List<Node> relPropertyExprs = new List<Node>();
            foreach (KeyValuePair<RelProperty, DbExpression> kv in _discriminatorMap.RelPropertyMap)
            {
                relPropertyList.Add(kv.Key);
                relPropertyExprs.Add(VisitExprAsScalar(kv.Value));
            }

            // construct a DiscriminatedNewInstanceOp
            DiscriminatedNewEntityOp newInstOp = _iqtCommand.CreateDiscriminatedNewEntityOp(e.Projection.ResultType,
                new ExplicitDiscriminatorMap(_discriminatorMap), _discriminatorMap.EntitySet, relPropertyList);

            // args include all projected properties and discriminator and the relProperties
            List<Node> newArgs = new List<Node>(_discriminatorMap.PropertyMap.Count + 1);
            newArgs.Add(CreateNewInstanceArgument(_discriminatorMap.Discriminator.Property, _discriminatorMap.Discriminator));
            foreach (var propertyMap in _discriminatorMap.PropertyMap)
            {
                DbExpression value = propertyMap.Value;
                EdmProperty property = propertyMap.Key;
                Node newArg = CreateNewInstanceArgument(property, value);
                newArgs.Add(newArg);
            }
            newArgs.AddRange(relPropertyExprs);

            Node newInstNode = _iqtCommand.CreateNode(newInstOp, newArgs);
            ExitExpressionBinding();

            Var sourceVar;
            Node varDefListNode = _iqtCommand.CreateVarDefListNode(newInstNode, out sourceVar);

            ProjectOp projOp = _iqtCommand.CreateProjectOp(sourceVar);
            Node projNode = _iqtCommand.CreateNode(projOp, source, varDefListNode);
            _varMap[projNode] = sourceVar;

            return projNode;
        }

        private Node CreateNewInstanceArgument(EdmMember property, DbExpression value)
        {
            Node newArg = BuildSoftCast(VisitExprAsScalar(value), Helper.GetModelTypeUsage(property));
            return newArg;
        }

        private Node GenerateStandardProject(DbProjectExpression e)
        {
            Node projectedSetNode = EnterExpressionBinding(e.Input);
            Node projectionNode = VisitExprAsScalar(e.Projection);
            ExitExpressionBinding();

            Var projectionVar;
            Node varDefListNode = _iqtCommand.CreateVarDefListNode(projectionNode, out projectionVar);

            ProjectOp projOp = _iqtCommand.CreateProjectOp(projectionVar);
            Node projNode = _iqtCommand.CreateNode(projOp, projectedSetNode, varDefListNode);
            _varMap[projNode] = projectionVar;

            return projNode;
        }

        public override Node Visit(DbCrossJoinExpression e)
        {
            return VisitJoin(e, e.Inputs, null);
        }

        public override Node Visit(DbJoinExpression e)
        {
            List<DbExpressionBinding> inputs = new List<DbExpressionBinding>();
            inputs.Add(e.Left);
            inputs.Add(e.Right);

            return VisitJoin(e, inputs, e.JoinCondition);
        }

        private Node VisitJoin(DbExpression e, IList<DbExpressionBinding> inputs, DbExpression joinCond)
        {
            //
            // Assert that the JoinType is covered. If JoinTypes are added to CQT then the
            // switch statement that constructs the JoinOp must be updated, along with this assert.
            //
            PlanCompiler.Assert(DbExpressionKind.CrossJoin == e.ExpressionKind ||
                            DbExpressionKind.InnerJoin == e.ExpressionKind ||
                            DbExpressionKind.LeftOuterJoin == e.ExpressionKind ||
                            DbExpressionKind.FullOuterJoin == e.ExpressionKind,
                            "Unrecognized JoinType specified in DbJoinExpression");

#if DEBUG
            //
            // Assert that the DbJoinExpression is producing a collection result with a record element type.
            // !!! IsCollectionOfRecord() is defined only in DEBUG  !!!
            PlanCompiler.Assert(IsCollectionOfRecord(e.ResultType), "Invalid Type returned by DbJoinExpression");
#endif

            //
            // Visit Join inputs, track their nodes and vars.
            //
            List<Node> inputNodes = new List<Node>();
            List<Var> inputVars = new List<Var>();
            for(int idx = 0; idx < inputs.Count; idx++)
            {
                Var boundVar;
                Node inputNode = VisitBoundExpression(inputs[idx].Expression, out boundVar);
                inputNodes.Add(inputNode);
                inputVars.Add(boundVar);
            }

            //
            // Bring the variables for the Join inputs into scope.
            //
            for (int scopeCount = 0; scopeCount < inputNodes.Count; scopeCount++)
            {
                PushBindingScope(inputVars[scopeCount], inputs[scopeCount].VariableName);
            }

            //
            // Visit join condition, if present.
            //
            Node joinCondNode = VisitExprAsPredicate(joinCond);

            //
            // Remove the input variables from scope after visiting the Join condition.
            //
            for (int scopeCount = 0; scopeCount < inputNodes.Count; scopeCount++)
            {
                ExitExpressionBinding();
            }

            //
            // Create an appropriate JoinOp based on the JoinType specified in the DbJoinExpression.
            //
            JoinBaseOp joinOp = null;
            switch (e.ExpressionKind)
            {
                case DbExpressionKind.CrossJoin:
                    {
                        joinOp = _iqtCommand.CreateCrossJoinOp();
                    }
                    break;

                case DbExpressionKind.InnerJoin:
                    {
                        joinOp = _iqtCommand.CreateInnerJoinOp();
                    }
                    break;

                case DbExpressionKind.LeftOuterJoin:
                    {
                        joinOp = _iqtCommand.CreateLeftOuterJoinOp();
                    }
                    break;

                case DbExpressionKind.FullOuterJoin:
                    {
                        joinOp = _iqtCommand.CreateFullOuterJoinOp();
                    }
                    break;
            }

            //
            // Assert that a JoinOp was produced. This check is again in case a new JoinType is introduced to CQT and this method is not updated.
            //
            PlanCompiler.Assert(joinOp != null, "Unrecognized JoinOp specified in DbJoinExpression, no JoinOp was produced");

            //
            // If the Join condition was present then add its converted form to the list of child nodes for the new Join node.
            //
            if (e.ExpressionKind != DbExpressionKind.CrossJoin)
            {
                PlanCompiler.Assert(joinCondNode != null, "Non CrossJoinOps must specify a join condition");
                inputNodes.Add(joinCondNode);
            }

            //
            // Create and return a new projection that unifies the multiple vars produced by the Join columns into a single record constructor.
            //
            return ProjectNewRecord(
                _iqtCommand.CreateNode(joinOp, inputNodes),
                ExtractElementRowType(e.ResultType),
                inputVars
            );
        }

        public override Node Visit(DbApplyExpression e)
        {
#if DEBUG
            //
            // Assert that the DbJoinExpression is producing a collection result with a record element type.
            // !!! IsCollectionOfRecord() is defined only in DEBUG  !!!
            PlanCompiler.Assert(IsCollectionOfRecord(e.ResultType), "Invalid Type returned by DbApplyExpression");
#endif

            //
            // Bring the Input set's variable into scope
            //
            Node inputNode = EnterExpressionBinding(e.Input);

            //
            // Visit the Apply expression with the Input's variable in scope.
            // This is done via EnterExpressionBinding, which is allowable only because
            // it will only bring the Apply variable into scope *after* visiting the Apply expression
            // (which means that the Apply expression cannot validly reference its own binding variable)
            //
            Node applyNode = EnterExpressionBinding(e.Apply);

            //
            // Remove the Apply and Input variables from scope
            //
            ExitExpressionBinding(); // for the Apply
            ExitExpressionBinding(); // for the Input

            //
            // The ApplyType should only be either CrossApply or OuterApply.
            //
            PlanCompiler.Assert(DbExpressionKind.CrossApply == e.ExpressionKind || DbExpressionKind.OuterApply == e.ExpressionKind, "Unrecognized DbExpressionKind specified in DbApplyExpression");

            //
            // Create a new Node with the correct ApplyOp as its Op and the input and apply nodes as its child nodes.
            //
            ApplyBaseOp applyOp = null;
            if (DbExpressionKind.CrossApply == e.ExpressionKind)
            {
                applyOp = _iqtCommand.CreateCrossApplyOp();
            }
            else
            {
                applyOp = _iqtCommand.CreateOuterApplyOp();
            }

            Node retNode = _iqtCommand.CreateNode(applyOp, inputNode, applyNode);

            //
            // Create and return a new projection that unifies the vars produced by the input and apply columns into a single record constructor.
            //
            return ProjectNewRecord(
                retNode,
                ExtractElementRowType(e.ResultType),
                new Var[] { _varMap[inputNode], _varMap[applyNode] }
            );
        }

        public override Node Visit(DbGroupByExpression e)
        {
#if DEBUG
            // !!! IsCollectionOfRecord() is defined only in DEBUG  !!!
            PlanCompiler.Assert(IsCollectionOfRecord(e.ResultType), "DbGroupByExpression has invalid result Type (not record collection)");
#endif

            //
            // Process the input and the keys
            //
            VarVec keyVarSet = _iqtCommand.CreateVarVec();
            VarVec outputVarSet = _iqtCommand.CreateVarVec();
            Node inputNode;
            List<Node> keyVarDefNodes;
            ExpressionBindingScope scope;
            ExtractKeys(e, keyVarSet, outputVarSet, out inputNode, out keyVarDefNodes, out scope);

            // Get the index of the group aggregate if any
            int groupAggregateIndex = -1;
            for (int i = 0; i < e.Aggregates.Count; i++)
            {
                if (e.Aggregates[i].GetType() == typeof(DbGroupAggregate))
                {
                    groupAggregateIndex = i;
                    break;
                }
            }

            //
            //If there is a group aggregate, create a copy of the input
            //
            Node copyOfInput = null;
            List<Node> copyOfKeyVarDefNodes = null;
            VarVec copyOutputVarSet = _iqtCommand.CreateVarVec();
            VarVec copyKeyVarSet = _iqtCommand.CreateVarVec();
            if (groupAggregateIndex >= 0)
            {
                ExpressionBindingScope copyOfScope; //not needed
                ExtractKeys(e, copyKeyVarSet, copyOutputVarSet, out copyOfInput, out copyOfKeyVarDefNodes, out copyOfScope);
            }

            //
            // Bring the Input variable from the DbGroupByExpression into scope
            //
            scope = new ExpressionBindingScope(_iqtCommand, e.Input.GroupVariableName, scope.ScopeVar);
            _varScopes.Push(scope);

            //
            // Process the Aggregates: For each DbAggregate, produce the corresponding IQT conversion depending on whether the DbAggregate is a DbFunctionAggregate or DbGroupAggregate.
            // The converted Node is then used as the child node of a VarDefOp Node that is added to a list of Aggregate VarDefs or Group Aggregate VarDefs correspondingly.
            // The Var defined by the converted DbAggregate is added only to the overall list of Vars produced by the GroupBy (not the list of Keys).
            //
            List<Node> aggVarDefNodes = new List<Node>();
            Node groupAggDefNode = null;
            for(int idx = 0; idx < e.Aggregates.Count; idx++)
            {
                DbAggregate agg = e.Aggregates[idx];
                Var aggVar;

                //
                // Produce the converted form of the Arguments to the aggregate
                //
                IList<Node> argNodes = VisitExprAsScalar(agg.Arguments);
              
                //
                // Handle if it is DbFunctionAggregate
                //
                if (idx != groupAggregateIndex)
                {
                    DbFunctionAggregate funcAgg = agg as DbFunctionAggregate;
                    PlanCompiler.Assert(funcAgg != null, "Unrecognized DbAggregate used in DbGroupByExpression");

                    aggVarDefNodes.Add(ProcessFunctionAggregate(funcAgg, argNodes, out aggVar));
                }
                //
                // Handle if it is DbGroupAggregate
                //
                else
                {
                    groupAggDefNode = ProcessGroupAggregate(keyVarDefNodes, copyOfInput, copyOfKeyVarDefNodes, copyKeyVarSet, e.Input.Expression.ResultType, out aggVar);
                }

                outputVarSet.Set(aggVar);
            }

            //
            // The Aggregates have now been processed, so remove the group variable from scope.
            //
            ExitGroupExpressionBinding();

            //
            // Construct the GroupBy. This consists of a GroupByOp (or GroupByIntoOp) with 3 (or 4) children:
            // 1. The Node produced from the Input set
            // 2. A VarDefListOp Node that uses the Key VarDefs to define the Key Vars (created above)
            // 3. A VarDefListOp Node that uses the Aggregate VarDefs to define the Aggregate Vars (created above)
            // 4. For a GroupByIntoOp a verDefLIstOp Node with a single var def node that defines the group aggregate
            //
            List<Node> groupByChildren = new List<Node>();
            groupByChildren.Add(inputNode);  // The Node produced from the Input set
            groupByChildren.Add(        // The Key VarDefs
                _iqtCommand.CreateNode(
                    _iqtCommand.CreateVarDefListOp(),
                    keyVarDefNodes
                ));
            groupByChildren.Add(        // The Aggregate VarDefs
                _iqtCommand.CreateNode(
                    _iqtCommand.CreateVarDefListOp(),
                    aggVarDefNodes
                ));

            GroupByBaseOp op;
            if (groupAggregateIndex >= 0)
            {
                groupByChildren.Add(    // The GroupAggregate VarDef
                    _iqtCommand.CreateNode(
                        _iqtCommand.CreateVarDefListOp(),
                        groupAggDefNode
                    ));
                op = _iqtCommand.CreateGroupByIntoOp(keyVarSet, this._iqtCommand.CreateVarVec(_varMap[inputNode]), outputVarSet);
            }
            else
            {
                op = _iqtCommand.CreateGroupByOp(keyVarSet, outputVarSet);
            }
                
            Node groupByNode = _iqtCommand.CreateNode(
                op, groupByChildren);

            //
            // Create and return a projection that unifies the multiple output vars of the GroupBy into a single record constructor.
            //
            return ProjectNewRecord(
                groupByNode,
                ExtractElementRowType(e.ResultType),
                outputVarSet     //todo: it is not correct to pass a varvec where an ordered list is expected
            );
        }

        private void ExtractKeys(DbGroupByExpression e, VarVec keyVarSet, VarVec outputVarSet, out Node inputNode, out List<Node> keyVarDefNodes, out ExpressionBindingScope scope)
        {
            inputNode = EnterGroupExpressionBinding(e.Input);

            //
            // Process the Keys: For each Key, produce the corresponding IQT conversion.
            // The converted Node is then used as the child node of a VarDefOp Node that is
            // added to a list of Key VarDefs. The Var defined by the converted Key expression
            // is added to both the overall list of Vars produced by the GroupBy and the list of Key vars produced by the GroupBy.
            //
            keyVarDefNodes = new List<Node>();
            for (int idx = 0; idx < e.Keys.Count; idx++)
            {
                DbExpression keyExpr = e.Keys[idx];

                Node keyNode = VisitExprAsScalar(keyExpr);
                ScalarOp keyOp = keyNode.Op as ScalarOp;

                //
                // In a valid CQT, each group key expressions will result in a ScalarOp since they
                // must be of an equality comparable type.
                //
                PlanCompiler.Assert(keyOp != null, "GroupBy Key is not a ScalarOp");

                //
                // Create a ComputedVar with the same type as the Key and add it to both the set of output Vars produced by the GroupBy and the set of Key vars.
                //
                Var keyVar;
                //
                // Create a VarDefOp that uses the converted form of the Key to define the ComputedVar and add it to the list of Key VarDefs.
                //
                keyVarDefNodes.Add(_iqtCommand.CreateVarDefNode(keyNode, out keyVar));
                outputVarSet.Set(keyVar);
                keyVarSet.Set(keyVar);
            }

            //
            // Before the Aggregates are processed, the Input variable must be taken out of scope and the 'group' variable introduced into scope in its place
            // This is done as follows:
            // 1. Pop the current ExpressionBindingScope from the stack
            // 2. Create a new ExpressionBindingScope using the same Var but the name of the 'group' variable from the DbGroupByExpression's DbGroupExpressionBinding
            // 3. Push this new scope onto the variable scope stack.
            //
            scope = ExitExpressionBinding();
        }
        
        private Node ProcessFunctionAggregate(DbFunctionAggregate funcAgg, IList<Node> argNodes, out Var aggVar)
        {
            Node aggNode = _iqtCommand.CreateNode(
                _iqtCommand.CreateAggregateOp(funcAgg.Function, funcAgg.Distinct),
                argNodes
            );

            //
            // Create a VarDefOp that uses the converted form of the DbAggregate to define the ComputedVar
            //
            return _iqtCommand.CreateVarDefNode(aggNode, out aggVar);
        }

        /// <summary>
        /// Translation for GroupAggregate
        ///
        /// Create the translation as :  
        /// 
        ///  Collect
        ///     |
        ///  PhysicalProject
        ///     |
        ///  GroupNodeDefinition
        /// 
        /// Here, GroupNodeDefinition is:  
        ///    1. If there are no keys:  copyOfInput;
        ///    2. If there are keys: 
        ///  
        ///  Filter (keyDef1 = copyOfKeyDef1 or keyDef1 is null and copyOfKeyDef1 is null) and ... and (keyDefn = copyOfKeyDefn or keyDefn is null and copyOfKeyDefn is null)
        ///    |
        ///  Project (copyOfInput, copyOfKeyDef1, copyOfKeyDef1, ... copyOfKeyDefn) 
        ///    |
        ///  copyOfInput
        /// 
        /// </summary>
        /// <param name="keyVarDefNodes"></param>
        /// <param name="copyOfInput"></param>
        /// <param name="copyOfkeyVarDefNodes"></param>
        /// <param name="copyKeyVarSet"></param>
        /// <param name="inputResultType"></param>
        /// <param name="groupAggVar"></param>
        /// <returns></returns>
        private Node ProcessGroupAggregate(List<Node> keyVarDefNodes, Node copyOfInput, List<Node> copyOfkeyVarDefNodes, VarVec copyKeyVarSet, TypeUsage inputResultType, out Var groupAggVar)
        {
            Var inputVar = this._varMap[copyOfInput];
            Node groupDefNode = copyOfInput;

            if (keyVarDefNodes.Count > 0)
            {
                VarVec projectOutpus = _iqtCommand.CreateVarVec();
                projectOutpus.Set(inputVar);
                projectOutpus.Or(copyKeyVarSet);

                Node projectNodeWithKeys = _iqtCommand.CreateNode(
                    _iqtCommand.CreateProjectOp(projectOutpus),
                    groupDefNode,                   //the input
                    _iqtCommand.CreateNode(         //the key var defs
                        _iqtCommand.CreateVarDefListOp(),
                        copyOfkeyVarDefNodes
                    ));

                List<Node> flattentedKeys = new List<Node>();
                List<Node> copyFlattenedKeys = new List<Node>();

                for (int i = 0; i < keyVarDefNodes.Count; i++)
                {
                    Node keyVarDef = keyVarDefNodes[i];
                    Node copyOfKeyVarDef = copyOfkeyVarDefNodes[i];

                    Var keyVar = ((VarDefOp)keyVarDef.Op).Var;
                    Var copyOfKeyVar = ((VarDefOp)copyOfKeyVarDef.Op).Var;

                    //
                    // The keys of type row need to be flattened, because grouping by a row means grouping by its individual 
                    // members and thus we have to check the individual members whether they are null. 
                    // IsNull(x) where x is a row type does not mean whether the individual properties of x are null,
                    // but rather whether the entire row is null. 
                    //
                    FlattenProperties(_iqtCommand.CreateNode(_iqtCommand.CreateVarRefOp(keyVar)), flattentedKeys);
                    FlattenProperties(_iqtCommand.CreateNode(_iqtCommand.CreateVarRefOp(copyOfKeyVar)), copyFlattenedKeys);
                }

                PlanCompiler.Assert(flattentedKeys.Count == copyFlattenedKeys.Count, "The flattened keys lists should have the same nubmer of elements");
                
                Node filterPredicateNode = null;

                for(int j = 0; j< flattentedKeys.Count; j++)
                {
                    Node keyNode = flattentedKeys[j];
                    Node copyKeyNode = copyFlattenedKeys[j];

                    //
                    // Create the predicate for a single key
                    // keyVar = copyOfKeyVar or keyVar is null and copyOfKeyVar is null
                    // 
                    Node predicate = _iqtCommand.CreateNode(
                                _iqtCommand.CreateConditionalOp(OpType.Or),
                                _iqtCommand.CreateNode(
                                    _iqtCommand.CreateComparisonOp(OpType.EQ), keyNode, copyKeyNode),
                                _iqtCommand.CreateNode(
                                    _iqtCommand.CreateConditionalOp(OpType.And),
                                        _iqtCommand.CreateNode(
                                            _iqtCommand.CreateConditionalOp(OpType.IsNull),
                                            OpCopier.Copy(_iqtCommand, keyNode)),
                                        _iqtCommand.CreateNode(
                                            _iqtCommand.CreateConditionalOp(OpType.IsNull),
                                            OpCopier.Copy(_iqtCommand, copyKeyNode))));
                    
                    if (filterPredicateNode == null)
                    {
                        filterPredicateNode = predicate;
                    }
                    else
                    {
                        filterPredicateNode = _iqtCommand.CreateNode(
                                _iqtCommand.CreateConditionalOp(OpType.And),
                                filterPredicateNode, predicate);
                    }
                }

                Node filterNode = _iqtCommand.CreateNode(
                                    _iqtCommand.CreateFilterOp(), projectNodeWithKeys, filterPredicateNode);

                groupDefNode = filterNode;
            }

            //Cap with Collect over PhysicalProject
            _varMap[groupDefNode] = inputVar;
            groupDefNode = ConvertRelOpToScalarOpTree(groupDefNode, inputResultType);

            Node result = _iqtCommand.CreateVarDefNode(groupDefNode, out groupAggVar);
            return result;
        }

        /// <summary>
        /// If the return type of the input node is a RowType it flattens its individual non-row properties.
        /// The produced nodes are added to the given flattenedProperties list
        /// </summary>
        /// <param name="input"></param>
        /// <param name="flattenedProperties"></param>
        private void FlattenProperties(Node input, IList<Node> flattenedProperties)
        {
            if (input.Op.Type.EdmType.BuiltInTypeKind == BuiltInTypeKind.RowType)
            {
                IList<EdmProperty> properties = TypeHelpers.GetProperties(input.Op.Type);
                PlanCompiler.Assert(properties.Count != 0, "No nested properties for RowType");

                for (int i = 0; i < properties.Count; i++)
                {
                    Node newInput = (i == 0) ? input : OpCopier.Copy(_iqtCommand, input);
                    FlattenProperties(_iqtCommand.CreateNode(_iqtCommand.CreatePropertyOp(properties[i]), newInput), flattenedProperties);
                }
            }
            else
            {
                flattenedProperties.Add(input);
            }
        }

        /// <summary>
        /// Common processing for the identical input and sort order arguments to the unrelated
        /// DbSkipExpression and DbSortExpression types.
        /// </summary>
        /// <param name="input">The input DbExpressionBinding from the DbSkipExpression or DbSortExpression</param>
        /// <param name="sortOrder">The list of SortClauses from the DbSkipExpression or DbSortExpression</param>
        /// <param name="sortKeys">A list to contain the converted SortKeys produced from the SortClauses</param>
        /// <param name="inputVar">The Var produced by the input to the DbSkipExpression or DbSortExpression</param>
        /// <returns>
        ///     The converted form of the input to the DbSkipExpression or DbSortExpression, capped by a 
        ///     ProjectOp that defines and Vars referenced by the SortKeys.
        /// </returns>
        private Node VisitSortArguments(DbExpressionBinding input, IList<DbSortClause> sortOrder, List<SortKey> sortKeys, out Var inputVar)
        {
            //
            // Skip/DbSortExpression conversion first produces a ProjectOp over the original input.
            // This is done to ensure that the new (Constrained)SortOp itself does not
            // contain any local variable definitions (in the form of a VarDefList child node)
            // which makes it simpler to pull SortOps over ProjectOps later in the PlanCompiler
            // (specifically the PreProcessor).
            // The new ProjectOp projects the output Var of the input along with any Vars referenced
            // by the SortKeys, and its VarDefList child defines those Vars.
            
            //
            // Bring the variable defined by the DbSortExpression's input set into scope
            // and retrieve it from the Node => Var map for later use.
            //
            Node inputNode = EnterExpressionBinding(input);
            inputVar = _varMap[inputNode];

            //
            // Convert the SortClauses, building a new VarDefOp Node for each one.
            //
            VarVec projectedVars = _iqtCommand.CreateVarVec();
            projectedVars.Set(inputVar);

            List<Node> sortVarDefs = new List<Node>();
            PlanCompiler.Assert(sortKeys.Count == 0, "Non-empty SortKey list before adding converted SortClauses");
            for (int idx = 0; idx < sortOrder.Count; idx++)
            {
                DbSortClause clause = sortOrder[idx];

                //
                // Convert the DbSortClause DbExpression to a Node/Op pair
                //
                Node exprNode = VisitExprAsScalar(clause.Expression);

                //
                // In a valid CQT, DbSortClause expressions must have a result of an OrderComparable Type,
                // and such expressions will always convert to ScalarOps.
                //
                ScalarOp specOp = exprNode.Op as ScalarOp;
                PlanCompiler.Assert(specOp != null, "DbSortClause Expression converted to non-ScalarOp");

                //
                // Create a new ComputedVar with the same Type as the result Type of the DbSortClause DbExpression
                //
                Var specVar;

                //
                // Create a new VarDefOp Node that defines the ComputedVar and add it both to the
                // list of VarDefs and the VarVec of produced Vars that will be used to create a
                // SortKey-defining ProjectOp over the Sort input.
                //
                sortVarDefs.Add(_iqtCommand.CreateVarDefNode(exprNode, out specVar));
                projectedVars.Set(specVar);

                //
                // Create a new IQT SortKey that references the ComputedVar and has the same
                // Ascending and Collation as the original DbSortClause, then add it to the list of SortKeys.
                //
                SortKey sortKey = null;
                if (string.IsNullOrEmpty(clause.Collation))
                {
                    sortKey = Command.CreateSortKey(specVar, clause.Ascending);
                }
                else
                {
                    sortKey = Command.CreateSortKey(specVar, clause.Ascending, clause.Collation);
                }
                sortKeys.Add(sortKey);
            }

            //
            // Now that the SortClauses have been converted, remove the Input set's variable from scope.
            //
            ExitExpressionBinding();

            //
            // Cap the Input with a ProjectOp that pushes the sort key VarDefs down to that projection.
            //
            inputNode =
                _iqtCommand.CreateNode(
                    _iqtCommand.CreateProjectOp(projectedVars),
                    inputNode,
                    _iqtCommand.CreateNode(
                        _iqtCommand.CreateVarDefListOp(),
                        sortVarDefs
                    )
                );

            return inputNode;
        }

        public override Node Visit(DbSkipExpression expression)
        {
            //
            // Invoke common processing of Skip/DbSortExpression arguments.
            //
            Var inputVar;
            List<SortKey> sortKeys = new List<SortKey>();
            Node inputNode = VisitSortArguments(expression.Input, expression.SortOrder, sortKeys, out inputVar);

            //
            // Visit the Skip Count
            //
            Node countNode = VisitExprAsScalar(expression.Count);

            //
            // Create a new Node that has a new ConstrainedSortOp based on the SortKeys as its Op
            // and the following children:
            // - The Input node from VisitSortArguments
            // - The converted form of the skip count
            // - A NullOp of type Int64 to indicate that no limit operation is applied
            //
            Node skipNode = 
                _iqtCommand.CreateNode(
                    _iqtCommand.CreateConstrainedSortOp(sortKeys),
                    inputNode,
                    countNode,
                    _iqtCommand.CreateNode(_iqtCommand.CreateNullOp(_iqtCommand.IntegerType))
                );

            // Update the Node --> Var mapping for the new ConstrainedSort Node.
            // ConstrainedSortOp maps to the same Op that its RelOp input maps to.
            _varMap[skipNode] = inputVar;

            return skipNode;
        }

        public override Node Visit(DbSortExpression e)
        {
            //
            // Invoke common processing of Skip/DbSortExpression arguments.
            //
            Var inputVar;
            List<SortKey> sortKeys = new List<SortKey>();
            Node inputNode = VisitSortArguments(e.Input, e.SortOrder, sortKeys, out inputVar);

            //
            // Create a new SortOp that uses the constructed SortKeys.
            //
            SortOp newSortOp = _iqtCommand.CreateSortOp(sortKeys);

            //
            // Create a new SortOp Node that has the new SortOp as its Op the Key-defining ProjectOp Node as its only child.
            //
            Node newSortNode = _iqtCommand.CreateNode(newSortOp, inputNode);

            // Update the Node --> Var mapping for the new Sort Node.
            // SortOp maps to the same Op that its RelOp input maps to.
            _varMap[newSortNode] = inputVar;

            return newSortNode;
        }

        public override Node Visit(DbQuantifierExpression e)
        {
            Node retNode = null;

            //
            // Any converts to Exists(Filter(Input, Predicate))
            // All converts to Not(Exists(Filter(Input, Or(Not(Predicate), IsNull(Predicate)))))
            //
            PlanCompiler.Assert(DbExpressionKind.Any == e.ExpressionKind || DbExpressionKind.All == e.ExpressionKind, "Invalid DbExpressionKind in DbQuantifierExpression");

            //
            // Bring the input's variable into scope
            //
            Node inputNode = EnterExpressionBinding(e.Input);

            //
            // Convert the predicate
            //
            Node predicateNode = VisitExprAsPredicate(e.Predicate);

            //
            // If the quantifier is All then the predicate must become 'Not(Predicate) Or IsNull(Predicate)',
            // since the converted form of the predicate should exclude a member of the input set if and only if
            // the predicate evaluates to False - filtering only with the negated predicate would also exclude members
            // for which that negated predicate evaluates to null, possibly resulting in an erroneous empty result set
            // and causing the quantifier to produce a false positive result.
            //
            if (DbExpressionKind.All == e.ExpressionKind)
            {
                // Create the 'Not(Predicate)' branch of the Or.
                predicateNode = _iqtCommand.CreateNode(
                    _iqtCommand.CreateConditionalOp(OpType.Not),
                    predicateNode
                );

                // Visit the original predicate for use in the 'IsNull(Predicate)' branch of the Or.
                // Note that this is treated as a scalar value rather than a Boolean predicate.
                Node predicateCopy = VisitExprAsScalar(e.Predicate);

                // Create the 'IsNull(Predicate)' branch of the Or.
                predicateCopy = _iqtCommand.CreateNode(
                    _iqtCommand.CreateConditionalOp(OpType.IsNull),
                    predicateCopy
                );

                // Finally, combine the branches with a Boolean 'Or' Op to create the updated predicate node.
                predicateNode = _iqtCommand.CreateNode(
                    _iqtCommand.CreateConditionalOp(OpType.Or),
                    predicateNode,
                    predicateCopy
                );
            }

            //
            // Remove the input's variable from scope
            //
            ExitExpressionBinding();
            
            //
            // Create a FilterOp around the original input set and map the FilterOp to the Var produced by the original input set.
            //
            Var inputVar = _varMap[inputNode];
            inputNode = _iqtCommand.CreateNode(_iqtCommand.CreateFilterOp(), inputNode, predicateNode);
            _varMap[inputNode] = inputVar;

            //
            // Create an ExistsOp around the filtered set to perform the quantifier operation.
            //
            retNode = _iqtCommand.CreateNode(_iqtCommand.CreateExistsOp(), inputNode);

            //
            // For All, the exists operation as currently built must now be negated.
            //
            if (DbExpressionKind.All == e.ExpressionKind)
            {
                retNode = _iqtCommand.CreateNode(_iqtCommand.CreateConditionalOp(OpType.Not), retNode);
            }

            return retNode;
        }

        #endregion
    }
}
