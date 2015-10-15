//---------------------------------------------------------------------
// <copyright file="NominalTypeEliminator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
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

using System.Globalization;
using System.Linq;
using System.Data.Common;
using md = System.Data.Metadata.Edm;
using System.Data.Query.InternalTrees;
using System.Data.Query.PlanCompiler;


namespace System.Data.Query.PlanCompiler
{
    /// <summary>
    /// The goal of this module is to eliminate all references to nominal types
    /// in the tree. Additionally, all structured types are replaced by "flat"
    /// record types - where every field of the structured type is a scalar type.
    /// Note that UDTs are not considered to be structured types.
    /// 
    /// At the end of this phase,
    /// * there are no more nominal types in the tree
    /// * there are no more nested record types in the tree
    /// * No Var in the tree is of an structured type
    /// * Additionally (and these follow from the statements above)
    ///   * There are no NewInstanceOp constructors in the tree
    ///   * There are no PropertyOp operators where the result is a structured type
    /// 
    /// This module uses information from the PropertyPushdown phase to "optimize"
    /// structured type elimination. Essentially, if we can avoid producing pieces
    /// of information that will be discarded later, then lets do that.
    /// 
    /// The general mechanism of type elimination is as follows. We walk up the tree
    /// in a bottom up fashion, and try to convert all structured types into flattened
    /// record types - type constructors are first converted into flat record constructors
    /// and then dismantled etc. The barrier points - Vars - are all converted into
    /// scalar types, and all intermediate stages will be eliminated in transition.
    /// 
    /// The output from this phase includes a ColumnMap - which is used later by
    /// the execution model to produce results in the right form from an otherwise
    /// flat query
    /// 
    /// Notes: This phase could be combined later with the PropertyPushdown phase
    /// 
    /// </summary>
    internal class NominalTypeEliminator : BasicOpVisitorOfNode
    {

        #region Nested Classes
        /// <summary>
        /// Describes an operation kind - for various property extractions
        /// </summary>
        internal enum OperationKind
        {
            /// <summary>
            /// Comparing two instances for equality
            /// </summary>
            Equality,

            /// <summary>
            /// Checking to see if an instance is null
            /// </summary>
            IsNull,

            /// <summary>
            /// Getting the "identity" of an entity
            /// </summary>
            GetIdentity,

            /// <summary>
            /// Getting the keys of an entity
            /// </summary>
            GetKeys,

            /// <summary>
            /// All properties of an entity
            /// </summary>
            All
        }
        #endregion

        #region private state

        private readonly Dictionary<Var, PropertyRefList> m_varPropertyMap;
        private readonly Dictionary<Node, PropertyRefList> m_nodePropertyMap;
        private readonly VarInfoMap m_varInfoMap;
        private readonly PlanCompiler m_compilerState;
        private Command m_command { get { return m_compilerState.Command; } }
        private readonly StructuredTypeInfo m_typeInfo;
        private readonly Dictionary<md.EdmFunction, md.EdmProperty[]> m_tvfResultKeys;
        private Dictionary<md.TypeUsage, md.TypeUsage> m_typeToNewTypeMap;
        private const string PrefixMatchCharacter = "%"; // This is ANSI-SQL defined, but it should probably be configurable.

        #endregion

        #region constructors

        private NominalTypeEliminator(PlanCompiler compilerState,
            StructuredTypeInfo typeInfo,
            Dictionary<Var, PropertyRefList> varPropertyMap,
            Dictionary<Node, PropertyRefList> nodePropertyMap,
            Dictionary<md.EdmFunction, md.EdmProperty[]> tvfResultKeys)
        {
            m_compilerState = compilerState;
            m_typeInfo = typeInfo;
            m_varPropertyMap = varPropertyMap;
            m_nodePropertyMap = nodePropertyMap;
            m_varInfoMap = new VarInfoMap();
            m_tvfResultKeys = tvfResultKeys;
            m_typeToNewTypeMap = new Dictionary<md.TypeUsage, md.TypeUsage>(TypeUsageEqualityComparer.Instance);
        }

        #endregion

        #region Process Driver

        /// <summary>
        /// Eliminates all structural types from the query
        /// </summary>
        /// <param name="compilerState">current compiler state</param>
        /// <param name="structuredTypeInfo"></param>
        /// <param name="tvfResultKeys">inferred s-space keys for TVFs that are mapped to entities</param>
        internal static void Process(
            PlanCompiler compilerState,
            StructuredTypeInfo structuredTypeInfo,
            Dictionary<md.EdmFunction, md.EdmProperty[]> tvfResultKeys)
        {
#if DEBUG
            //string phase0 = Dump.ToXml(compilerState.Command);
            Validator.Validate(compilerState);
#endif

            // Phase 1: Top-down property pushdown
            Dictionary<Var, PropertyRefList> varPropertyMap;
            Dictionary<Node, PropertyRefList> nodePropertyMap;
            PropertyPushdownHelper.Process(compilerState.Command, structuredTypeInfo, out varPropertyMap, out nodePropertyMap);

#if DEBUG
            //string phase1 = Dump.ToXml(compilerState.Command);
            Validator.Validate(compilerState);
#endif

            // Phase 2: actually eliminate nominal types
            NominalTypeEliminator nte = new NominalTypeEliminator(
                compilerState, structuredTypeInfo, varPropertyMap, nodePropertyMap, tvfResultKeys);
            nte.Process();

#if DEBUG
            //string phase2 = Dump.ToXml(compilerState.Command);
            Validator.Validate(compilerState);
#endif

#if DEBUG
            //To avoid garbage collection
            //int size = phase0.Length;
            //size = phase1.Length;
            //size = phase2.Length;
#endif
        }


        /// <summary>
        /// The real driver. Invokes the visitor to traverse the tree bottom-up,
        /// and modifies the tree along the way.
        /// </summary>
        private void Process()
        {
            // Replace command enum parameters with a counterpart whose type is the underlying enum type of the original parameter
            // Replace command strongly typed spatial parameters with a counterpart whose type is the underlying spatial union type of the original parameter
            foreach (var paramVar in m_command.Vars.OfType<ParameterVar>().Where(v => md.TypeSemantics.IsEnumerationType(v.Type) || md.TypeSemantics.IsStrongSpatialType(v.Type)).ToArray())
            {
                ParameterVar newVar = md.TypeSemantics.IsEnumerationType(paramVar.Type)
                    ? m_command.ReplaceEnumParameterVar(paramVar)
                    : m_command.ReplaceStrongSpatialParameterVar(paramVar);
                m_varInfoMap.CreatePrimitiveTypeVarInfo(paramVar, newVar);
            }

            Node rootNode = m_command.Root;
            PlanCompiler.Assert(rootNode.Op.OpType == OpType.PhysicalProject, "root node is not PhysicalProjectOp?");
            // invoke the visitor on the root node
            rootNode.Op.Accept(this, rootNode);
        }

        #endregion

        #region type utilities

        /// <summary>
        /// The datatype of the typeid property
        /// </summary>
        private md.TypeUsage DefaultTypeIdType
        {
            get { return m_command.StringType; }
        }

        /// <summary>
        /// Get the "new" type corresponding to the input type. 
        /// For structured types, we simply look up the typeInfoMap
        /// For collection types, we create a new collection type based on the 
        ///   "new" element type.
        /// For enums we return the underlying type of the enum type.
        /// For strong spatial types we return the union type that includes the strong spatial type.
        /// For all other types, we simply return the input type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private md.TypeUsage GetNewType(md.TypeUsage type)
        {
            md.TypeUsage newType;

            if (m_typeToNewTypeMap.TryGetValue(type, out newType))
            {
                return newType;
            }

            md.CollectionType collectionType;
            if (TypeHelpers.TryGetEdmType<md.CollectionType>(type, out collectionType))
            {
                // If this is a collection type, then clone a new collection type
                md.TypeUsage newElementType = GetNewType(collectionType.TypeUsage);
                newType = TypeUtils.CreateCollectionType(newElementType);
            }
            else if (TypeUtils.IsStructuredType(type))
            {
                // structured type => we've already calculated the input
                newType = m_typeInfo.GetTypeInfo(type).FlattenedTypeUsage;
            }
            else if (md.TypeSemantics.IsEnumerationType(type))
            {
                newType = TypeHelpers.CreateEnumUnderlyingTypeUsage(type);
            }
            else if (md.TypeSemantics.IsStrongSpatialType(type))
            {
                newType = TypeHelpers.CreateSpatialUnionTypeUsage(type);
            }
            else
            {
                // "simple" type => return the input type
                newType = type;
            }

            // Add this information to the map
            m_typeToNewTypeMap[type] = newType;
            return newType;
        }

        #endregion

        #region misc utilities

        /// <summary>
        /// This function builds a "property accessor" over the input expression.  It
        /// can produce one of three results:
        /// 
        ///   - It can return "null", if it is convinced that the input has no 
        ///     such expression
        ///   - It can return a subnode of the input, if that subnode represents
        ///     the property
        ///   - Or, it can build a PropertyOp explicitly
        /// 
        /// Assertion: the property is not a structured type
        /// </summary>
        /// <param name="input">The input expression</param>
        /// <param name="property">The desired property</param>
        /// <returns></returns>
        private Node BuildAccessor(Node input, md.EdmProperty property)
        {
            Op inputOp = input.Op;

            // Special handling if the input is a NewRecordOp
            NewRecordOp newRecordOp = inputOp as NewRecordOp;
            if (null != newRecordOp)
            {
                int fieldPos;
                // Identify the specific property we're interested in.
                if (newRecordOp.GetFieldPosition(property, out fieldPos))
                {
                    return Copy(input.Children[fieldPos]);
                }
                else
                {
                    return null;
                }
            }

            // special handling if the input is a null
            if (inputOp.OpType == OpType.Null)
            {
                return null;
            }

            // The default case: Simply return a new PropertyOp
            PropertyOp newPropertyOp = m_command.CreatePropertyOp(property);
            return m_command.CreateNode(newPropertyOp, this.Copy(input));
        }

        /// <summary>
        /// A BuildAccessor variant. If the appropriate property was not found, then
        /// build up a null constant instead
        /// </summary>
        /// <param name="input"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private Node BuildAccessorWithNulls(Node input, md.EdmProperty property)
        {
            Node newNode = this.BuildAccessor(input, property);
            if (newNode == null)
            {
                newNode = CreateNullConstantNode(md.Helper.GetModelTypeUsage(property));
            }
            return newNode;
        }

        /// <summary>
        /// Builds up an accessor to the typeid property. If the type has no typeid
        /// property, then we simply create a constantOp with the corresponding 
        /// typeid value for the type
        /// </summary>
        /// <param name="input">the input expression</param>
        /// <param name="typeInfo">the original type of the input expression</param>
        /// <returns></returns>
        private Node BuildTypeIdAccessor(Node input, TypeInfo typeInfo)
        {
            Node result;

            if (typeInfo.HasTypeIdProperty)
            {
                result = BuildAccessorWithNulls(input, typeInfo.TypeIdProperty);
            }
            else
            {
                result = CreateTypeIdConstant(typeInfo);
            }

            return result;
        }

        /// <summary>
        /// Builds a SoftCast operator over the input - if one is necessary.
        /// </summary>
        /// <param name="node">the input expression to "cast"</param>
        /// <param name="targetType">the target type</param>
        /// <returns>the "cast"ed expression</returns>
        private Node BuildSoftCast(Node node, md.TypeUsage targetType)
        {
            PlanCompiler.Assert(node.Op.IsScalarOp, "Attempting SoftCast around non-ScalarOp?");
            if (Command.EqualTypes(node.Op.Type, targetType))
            {
                return node;
            }
            // Skip any castOps we may have created already
            while (node.Op.OpType == OpType.SoftCast)
            {
                node = node.Child0;
            }
            Node newNode = m_command.CreateNode(m_command.CreateSoftCastOp(targetType), node);
            return newNode;
        }

        /// <summary>
        /// Clones a subtree.
        /// This is used by the "BuildAccessor" routines to build a property-accessor
        /// over some input. If we're reusing the input, the input must be cloned.
        /// </summary>
        /// <param name="n">The subtree to copy</param>
        /// <returns></returns>
        private Node Copy(Node n)
        {
            return OpCopier.Copy(m_command, n);
        }

        /// <summary>
        /// Returns a node for a null constant of the desired type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private Node CreateNullConstantNode(md.TypeUsage type)
        {
            return m_command.CreateNode(m_command.CreateNullOp(type));
        }

        /// <summary>
        /// Create a node to represent nullability.
        /// </summary>
        /// <returns>Node for the typeid constant</returns>
        private Node CreateNullSentinelConstant()
        {
            NullSentinelOp op = m_command.CreateNullSentinelOp();
            return m_command.CreateNode(op);
        }

        /// <summary>
        /// Create a node to represent the exact value of the typeid constant
        /// </summary>
        /// <param name="typeInfo">The current type</param>
        /// <returns>Node for the typeid constant</returns>
        private Node CreateTypeIdConstant(TypeInfo typeInfo)
        {
            object value = typeInfo.TypeId;
            md.TypeUsage typeIdType;
            if (typeInfo.RootType.DiscriminatorMap != null)
            {
                typeIdType = md.Helper.GetModelTypeUsage(typeInfo.RootType.DiscriminatorMap.DiscriminatorProperty);
            }
            else
            {
                typeIdType = DefaultTypeIdType;
            }
            InternalConstantOp op = m_command.CreateInternalConstantOp(typeIdType, value);
            return m_command.CreateNode(op);
        }

        /// <summary>
        /// Create a node to represent a typeid constant for a prefix match. 
        /// If the typeid value were "123X", then we would generate a constant 
        /// like "123X%"
        /// </summary>
        /// <param name="typeInfo">the current type</param>
        /// <returns>Node for the typeid constant</returns>
        private Node CreateTypeIdConstantForPrefixMatch(TypeInfo typeInfo)
        {
            string value = typeInfo.TypeId + PrefixMatchCharacter;
            InternalConstantOp op = m_command.CreateInternalConstantOp(DefaultTypeIdType, value);
            return m_command.CreateNode(op);
        }

        /// <summary>
        /// Identify the list of property refs for comparison and isnull semantics
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <param name="opKind"></param>
        /// <returns></returns>
        private IEnumerable<PropertyRef> GetPropertyRefsForComparisonAndIsNull(TypeInfo typeInfo, OperationKind opKind)
        {
            PlanCompiler.Assert(opKind == OperationKind.IsNull || opKind == OperationKind.Equality,
                "Unexpected opKind: " + opKind + "; Can only handle IsNull and Equality");

            md.TypeUsage currentType = typeInfo.Type;

            md.RowType recordType = null;
            if (TypeHelpers.TryGetEdmType<md.RowType>(currentType, out recordType))
            {
                if (opKind == OperationKind.IsNull && typeInfo.HasNullSentinelProperty)
                {
                    yield return NullSentinelPropertyRef.Instance;
                }
                else
                    foreach (md.EdmProperty m in recordType.Properties)
                    {
                        if (!TypeUtils.IsStructuredType(md.Helper.GetModelTypeUsage(m)))
                        {
                            yield return new SimplePropertyRef(m);
                        }
                        else
                        {
                            TypeInfo nestedTypeInfo = m_typeInfo.GetTypeInfo(md.Helper.GetModelTypeUsage(m));
                            foreach (PropertyRef p in GetPropertyRefs(nestedTypeInfo, opKind))
                            {
                                PropertyRef nestedPropertyRef = p.CreateNestedPropertyRef(m);
                                yield return nestedPropertyRef;
                            }
                        }
                    }
                yield break;
            }

            md.EntityType entityType = null;
            if (TypeHelpers.TryGetEdmType<md.EntityType>(currentType, out entityType))
            {
                if (opKind == OperationKind.Equality ||
                    (opKind == OperationKind.IsNull && !typeInfo.HasTypeIdProperty))
                {
                    foreach (PropertyRef p in typeInfo.GetIdentityPropertyRefs())
                    {
                        yield return p;
                    }
                }
                else
                {
                    yield return TypeIdPropertyRef.Instance;
                }
                yield break;
            }

            md.ComplexType complexType = null;
            if (TypeHelpers.TryGetEdmType<md.ComplexType>(currentType, out complexType))
            {
                PlanCompiler.Assert(opKind == OperationKind.IsNull, "complex types not equality-comparable");
                PlanCompiler.Assert(typeInfo.HasNullSentinelProperty, "complex type with no null sentinel property: can't handle isNull");
                yield return NullSentinelPropertyRef.Instance;
                yield break;
            }

            md.RefType refType = null;
            if (TypeHelpers.TryGetEdmType<md.RefType>(currentType, out refType))
            {
                foreach (PropertyRef p in typeInfo.GetAllPropertyRefs())
                {
                    yield return p;
                }
                yield break;
            }

            PlanCompiler.Assert(false, "Unknown type");
        }

        /// <summary>
        /// Get the list of "desired" propertyrefs for the specified type and operation
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <param name="opKind"></param>
        /// <returns></returns>
        private IEnumerable<PropertyRef> GetPropertyRefs(TypeInfo typeInfo, OperationKind opKind)
        {
            PlanCompiler.Assert(opKind != OperationKind.All, "unexpected attempt to GetPropertyRefs(...,OperationKind.All)");
            if (opKind == OperationKind.GetKeys)
            {
                return typeInfo.GetKeyPropertyRefs();
            }
            else if (opKind == OperationKind.GetIdentity)
            {
                return typeInfo.GetIdentityPropertyRefs();
            }
            else
            {
                return GetPropertyRefsForComparisonAndIsNull(typeInfo, opKind);
            }
        }

        /// <summary>
        /// Get a list of "desired" properties for each operationKind (specified by the opKind
        /// parameter). The OpKinds we support are 
        /// 
        ///  * GetKeys
        ///    Applies only to entity and ref types - gets the key properties (more specifically
        ///      the flattened equivalents)
        ///  * GetIdentity
        ///    Applies only to entity and ref types - gets the entityset id property first, and then the
        ///      the Key properties
        ///  * All
        ///    Gets all properties of the flattened type
        /// 
        ///  * Equality
        ///    Scalar types - the entire instance
        ///    Entity - the identity properties
        ///    Ref - all properties (= identity properties)
        ///    Complex/Collection - Not supported
        ///    Record - recurse over each property
        /// 
        ///  * IsNull
        ///    Scalar types - entire instance
        ///    Entity - typeid property, if it exists; otherwise, the key properties
        ///    ComplexType - typeid property
        ///    Ref - all properties
        ///    Collection - not supported
        ///    Record - recurse over each property
        /// </summary>
        /// <param name="typeInfo">Type information for the current op</param>
        /// <param name="opKind">Current operation kind</param>
        /// <returns>List of desired properties</returns>
        private IEnumerable<md.EdmProperty> GetProperties(TypeInfo typeInfo, OperationKind opKind)
        {
            if (opKind == OperationKind.All)
            {
                foreach (md.EdmProperty p in typeInfo.GetAllProperties())
                {
                    yield return p;
                }
            }
            else
            {
                foreach (PropertyRef p in GetPropertyRefs(typeInfo, opKind))
                {
                    yield return typeInfo.GetNewProperty(p);
                }
            }
        }

        /// <summary>
        /// Get a list of properties and value (expressions) for each desired property of the
        /// input. The list of desired properties is based on the opKind parameter. 
        /// The ignoreMissingProperties indicates if we should create a null constant, in case
        /// the input cannot produce the specified property
        /// </summary>
        /// <param name="typeInfo">typeinfo for the input</param>
        /// <param name="opKind">Current operation kind</param>
        /// <param name="input">The input expression tree</param>
        /// <param name="ignoreMissingProperties">Should we ignore missing properties</param>
        /// <param name="properties">Output: list of properties</param>
        /// <param name="values">Output: correspondng list of values</param>
        private void GetPropertyValues(TypeInfo typeInfo, OperationKind opKind, Node input, bool ignoreMissingProperties,
            out List<md.EdmProperty> properties, out List<Node> values)
        {

            values = new List<Node>();
            properties = new List<md.EdmProperty>();
            foreach (md.EdmProperty prop in GetProperties(typeInfo, opKind))
            {
                KeyValuePair<md.EdmProperty, Node> kv = GetPropertyValue(input, prop, ignoreMissingProperties);
                if (kv.Value != null)
                {
                    properties.Add(kv.Key);
                    values.Add(kv.Value);
                }
            }
        }

        /// <summary>
        /// Build up a key-value pair of (property, expression) to represent 
        /// the extraction of the appropriate property from the input expression
        /// </summary>
        /// <param name="input">The input (structured type) expression</param>
        /// <param name="property">The property in question</param>
        /// <param name="ignoreMissingProperties">should we ignore missing properties</param>
        /// <returns></returns>
        private KeyValuePair<md.EdmProperty, Node> GetPropertyValue(Node input, md.EdmProperty property, bool ignoreMissingProperties)
        {
            Node n = null;

            if (!ignoreMissingProperties)
            {
                n = BuildAccessorWithNulls(input, property);
            }
            else
            {
                n = BuildAccessor(input, property);
            }
            return new KeyValuePair<md.EdmProperty, Node>(property, n);
        }

        /// <summary>
        /// Walk the SortKeys, and expand out 
        /// any Structured type Var references 
        /// If any of the sort keys is expanded to include a var representing a null sentinel,
        /// set PlanCompiler.HasSortingOnNullSentinels to true.
        /// </summary>
        /// <param name="keys">The list of input keys</param>
        /// <returns>An expanded list of keys. If there is nothing to expand it returns the original list.</returns>
        private List<InternalTrees.SortKey> HandleSortKeys(List<InternalTrees.SortKey> keys)
        {
            List<InternalTrees.SortKey> newSortKeys = new List<InternalTrees.SortKey>();
            bool modified = false;
            foreach (InternalTrees.SortKey k in keys)
            {
                VarInfo varInfo;
                if (!m_varInfoMap.TryGetVarInfo(k.Var, out varInfo))
                {
                    newSortKeys.Add(k);
                }
                else
                {
                    StructuredVarInfo structuredVarInfo = varInfo as StructuredVarInfo;
                    if (structuredVarInfo != null && structuredVarInfo.NewVarsIncludeNullSentinelVar)
                    {
                        m_compilerState.HasSortingOnNullSentinels = true;
                    }

                    foreach (Var v in varInfo.NewVars)
                    {
                        InternalTrees.SortKey newKey = Command.CreateSortKey(v, k.AscendingSort, k.Collation);
                        newSortKeys.Add(newKey);
                    }
                    modified = true;
                }
            }

            List<InternalTrees.SortKey> result = modified ? newSortKeys : keys;
            return result;
        }

        /// <summary>
        /// Project properties of <paramref name="unnestOpTableTypeInfo"/> that represents the flattened type of the <paramref name="unnestNode"/>.
        /// The <paramref name="unnestNode"/> contains a TVF call. 
        /// Return new node with ProjectOp and <paramref name="newVars"/> representing the projection outputs.
        /// </summary>
        private Node CreateTVFProjection(Node unnestNode, List<Var> unnestOpTableColumns, TypeInfo unnestOpTableTypeInfo, out List<Var> newVars)
        {
            md.RowType originalRowType = unnestOpTableTypeInfo.Type.EdmType as md.RowType;
            PlanCompiler.Assert(originalRowType != null, "Unexpected TVF return type (must be row): " + unnestOpTableTypeInfo.Type.ToString());

            List<Var> convertToFlattenedTypeVars = new List<Var>();
            List<Node> convertToFlattenedTypeVarDefs = new List<Node>();
            PropertyRef[] propRefs = unnestOpTableTypeInfo.PropertyRefList.ToArray();

            Dictionary<md.EdmProperty, PropertyRef> flattenedTypePropertyToPropertyRef = new Dictionary<md.EdmProperty, PropertyRef>();
            foreach (var propRef in propRefs)
            {
                flattenedTypePropertyToPropertyRef.Add(unnestOpTableTypeInfo.GetNewProperty(propRef), propRef);
            }

            foreach (var flattenedTypeProperty in unnestOpTableTypeInfo.FlattenedType.Properties)
            {
                var propRef = flattenedTypePropertyToPropertyRef[flattenedTypeProperty];

                Var var = null;
                SimplePropertyRef simplePropRef = propRef as SimplePropertyRef;
                if (simplePropRef != null)
                {
                    // Find the corresponding column in the TVF output and build a var ref to it.
                    int columnIndex = originalRowType.Members.IndexOf(simplePropRef.Property);
                    PlanCompiler.Assert(columnIndex >= 0, "Can't find a column in the TVF result type");
                    convertToFlattenedTypeVarDefs.Add(m_command.CreateVarDefNode(m_command.CreateNode(m_command.CreateVarRefOp(unnestOpTableColumns[columnIndex])), out var));
                }
                else
                {
                    NullSentinelPropertyRef nullSentinelPropRef = propRef as NullSentinelPropertyRef;
                    if (nullSentinelPropRef != null)
                    {
                        // Null sentinel does not exist in the TVF output, so build a new null sentinel expression.
                        convertToFlattenedTypeVarDefs.Add(m_command.CreateVarDefNode(CreateNullSentinelConstant(), out var));
                    }
                }
                PlanCompiler.Assert(var != null, "TVFs returning a collection of rows with non-primitive properties are not supported");

                convertToFlattenedTypeVars.Add(var);
            }

            // Make sure unnestTableColumnVar is mapped to the ProjectOp outputs.
            newVars = convertToFlattenedTypeVars;

            // Create Project(Unnest(Func()))
            return m_command.CreateNode(m_command.CreateProjectOp(m_command.CreateVarVec(convertToFlattenedTypeVars)),
                                        unnestNode,
                                        m_command.CreateNode(m_command.CreateVarDefListOp(), convertToFlattenedTypeVarDefs));
        }
        #endregion

        #region Visitor methods

        #region AncillaryOp Visitors

        /// <summary>
        /// VarDefListOp
        /// 
        /// Walks each VarDefOp child, and "expands" it out if the Var is a 
        /// structured type. If the Var is of enum type it replaces the var 
        /// with a var whose type is the underlying type of the enum type from
        /// the original Var.  If the Var is of strong spatial type it replaces the var 
        /// with a var whose type is the spatial union type that contains the strong spatial type of
        /// the original Var.
        /// 
        /// For each Var that is expanded, a new expression is created to compute
        /// its value (from the original computed expression)
        /// A new VarDefListOp is created to hold all the "expanded" Varlist
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(VarDefListOp op, Node n)
        {
            VisitChildren(n);

            List<Node> newChildren = new List<Node>();

            foreach (Node chi in n.Children)
            {
                PlanCompiler.Assert(chi.Op is VarDefOp, "VarDefOp expected");

                VarDefOp varDefOp = (VarDefOp)chi.Op;

                if (TypeUtils.IsStructuredType(varDefOp.Var.Type) || TypeUtils.IsCollectionType(varDefOp.Var.Type))
                {
                    List<Node> newChiList;
                    md.TypeUsage x;

                    FlattenComputedVar((ComputedVar)varDefOp.Var, chi, out newChiList, out x);

                    foreach (Node newChi in newChiList)
                    {
                        newChildren.Add(newChi);
                    }
                }
                else if (md.TypeSemantics.IsEnumerationType(varDefOp.Var.Type) || md.TypeSemantics.IsStrongSpatialType(varDefOp.Var.Type))
                {
                    newChildren.Add(FlattenEnumOrStrongSpatialVar(varDefOp, chi.Child0));
                }
                else
                {
                    newChildren.Add(chi);
                }
            }
            Node newVarDefListNode = m_command.CreateNode(n.Op, newChildren);
            return newVarDefListNode;
        }

        /// <summary>
        /// Helps flatten out a computedVar expression
        /// </summary>
        /// <param name="v">The Var</param>
        /// <param name="node">Subtree rooted at the VarDefOp expression</param>
        /// <param name="newNodes">list of new nodes produced</param>
        /// <param name="newType"></param>
        /// <returns>VarInfo for this var</returns>
        private void FlattenComputedVar(ComputedVar v, Node node, out List<Node> newNodes, out md.TypeUsage newType)
        {
            newNodes = new List<Node>();
            Node definingExprNode = node.Child0; // defining expression for the VarDefOp
            newType = null;

            if (TypeUtils.IsCollectionType(v.Type))
            {
                PlanCompiler.Assert(definingExprNode.Op.OpType != OpType.Function, "Flattening of TVF output is not allowed.");
                newType = GetNewType(v.Type);
                Var newVar;
                Node newVarDefNode = m_command.CreateVarDefNode(definingExprNode, out newVar);
                newNodes.Add(newVarDefNode);
                m_varInfoMap.CreateCollectionVarInfo(v, newVar);
                return;
            }

            // Get the "new" type for the Var
            TypeInfo typeInfo = m_typeInfo.GetTypeInfo(v.Type);
            // Get a list of properties that we think are necessary 
            PropertyRefList desiredProperties = m_varPropertyMap[v];
            List<Var> newVars = new List<Var>();
            List<md.EdmProperty> newProps = new List<md.EdmProperty>();
            newNodes = new List<Node>();
            var hasNullSentinelVar = false;
            foreach (PropertyRef p in typeInfo.PropertyRefList)
            {
                // do I care for this property?
                if (!desiredProperties.Contains(p))
                {
                    continue;
                }

                md.EdmProperty newProperty = typeInfo.GetNewProperty(p);

                //
                // #479467 - Make sure that we build Vars for all properties - if
                // we are asked to produce all properties. This is extremely important
                // for the top-level Vars
                // 
                Node propAccessor = null;
                if (desiredProperties.AllProperties)
                {
                    propAccessor = BuildAccessorWithNulls(definingExprNode, newProperty);
                }
                else
                {
                    propAccessor = BuildAccessor(definingExprNode, newProperty);
                    if (propAccessor == null)
                    {
                        continue;
                    }
                }

                // Add the new property
                newProps.Add(newProperty);

                // Create a new VarDefOp. 
                Var newVar;
                Node newVarDefNode = m_command.CreateVarDefNode(propAccessor, out newVar);
                newNodes.Add(newVarDefNode);
                newVars.Add(newVar);

                // Check if it is a null sentinel var
                if (!hasNullSentinelVar && IsNullSentinelPropertyRef(p))
                {
                    hasNullSentinelVar = true;
                }
            }
            m_varInfoMap.CreateStructuredVarInfo(v, typeInfo.FlattenedType, newVars, newProps, hasNullSentinelVar);
            return;
        }

        /// <summary>
        /// Is the given propertyRef representing a null sentinel
        /// It is if:
        ///  - it is a NullSentinelPropertyRef
        ///  - it is a NestedPropertyRef with the outer property being a NullSentinelPropertyRef
        /// </summary>
        /// <param name="propertyRef"></param>
        /// <returns></returns>
        private bool IsNullSentinelPropertyRef(PropertyRef propertyRef)
        {
            if (propertyRef is NullSentinelPropertyRef)
            {
                return true;
            }
            NestedPropertyRef nestedPropertyRef = propertyRef as NestedPropertyRef;
            if (nestedPropertyRef == null)
            {
                return false;
            }
            return nestedPropertyRef.OuterProperty is NullSentinelPropertyRef;
        }

        /// <summary>
        /// Helps flatten out an enum or strong spatial Var
        /// </summary>
        /// <param name="varDefOp">Var definition expression. Must not be null.</param>
        /// <param name="node">Subtree rooted at the VarDefOp expression. Must not be null.</param>
        /// <returns>VarDefNode referencing the newly created Var.</returns>
        private Node FlattenEnumOrStrongSpatialVar(VarDefOp varDefOp, Node node)
        {
            System.Diagnostics.Debug.Assert(varDefOp != null, "varDefOp != null");
            System.Diagnostics.Debug.Assert(node != null, "node != null");

            Var newVar;
            Node newVarDefNode = m_command.CreateVarDefNode(node, out newVar);
            m_varInfoMap.CreatePrimitiveTypeVarInfo(varDefOp.Var, newVar);

            return newVarDefNode;
        }

        #endregion

        #region PhysicalOp Visitors

        /// <summary>
        /// PhysicalProjectOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(PhysicalProjectOp op, Node n)
        {
            // visit my children
            VisitChildren(n);

            // flatten out the varset
            VarList newVarList = FlattenVarList(op.Outputs);
            // reflect changes into my column map
            SimpleCollectionColumnMap newColumnMap = ExpandColumnMap(op.ColumnMap);
            PhysicalProjectOp newOp = m_command.CreatePhysicalProjectOp(newVarList, newColumnMap);
            n.Op = newOp;

            return n;
        }

        private SimpleCollectionColumnMap ExpandColumnMap(SimpleCollectionColumnMap columnMap)
        {
            VarRefColumnMap varRefColumnMap = columnMap.Element as VarRefColumnMap;
            PlanCompiler.Assert(varRefColumnMap != null, "Encountered a SimpleCollectionColumnMap element that is not VarRefColumnMap when expanding a column map in NominalTypeEliminator.");

            // see if this var has changed in some fashion
            VarInfo varInfo;
            if (!m_varInfoMap.TryGetVarInfo(varRefColumnMap.Var, out varInfo))
            {
                return columnMap; // no changes
            }

            //
            // Ensure that we get the right number of Vars - we need one Var for
            // each scalar property
            //
            if (TypeUtils.IsStructuredType(varRefColumnMap.Var.Type))
            {
                TypeInfo typeInfo = m_typeInfo.GetTypeInfo(varRefColumnMap.Var.Type);
                PlanCompiler.Assert(typeInfo.RootType.FlattenedType.Properties.Count == varInfo.NewVars.Count,
                    "Var count mismatch; Expected " + typeInfo.RootType.FlattenedType.Properties.Count + "; got " + varInfo.NewVars.Count + " instead.");
            }

            // "Process" this columnMap
            ColumnMapProcessor processor = new ColumnMapProcessor(varRefColumnMap, varInfo, m_typeInfo);
            ColumnMap newColumnMap = processor.ExpandColumnMap();

            //Wrap it with a collection
            SimpleCollectionColumnMap resultColumnMap = new SimpleCollectionColumnMap(TypeUtils.CreateCollectionType(newColumnMap.Type), newColumnMap.Name, newColumnMap, columnMap.Keys, columnMap.ForeignKeys);

            return resultColumnMap;
        }

        #endregion

        #region RelOp Visitors

        /// <summary>
        /// Walk the input var sequence, flatten each var, and return the new sequence of
        /// Vars
        /// </summary>
        /// <param name="vars">input Var sequence</param>
        /// <returns>flattened output var sequence</returns>
        private IEnumerable<Var> FlattenVars(IEnumerable<Var> vars)
        {
            foreach (Var v in vars)
            {
                VarInfo varInfo;

                if (!m_varInfoMap.TryGetVarInfo(v, out varInfo))
                {
                    yield return v;
                }
                else
                {
                    foreach (Var newVar in varInfo.NewVars)
                    {
                        yield return newVar;
                    }
                }
            }
        }

        /// <summary>
        /// Probe the current VarSet for "structured" Vars - replace these with the
        /// corresponding sets of flattened Vars
        /// </summary>
        /// <param name="varSet">current set of vars</param>
        /// <returns>an "expanded" varset</returns>
        private VarVec FlattenVarSet(VarVec varSet)
        {
            VarVec newVarSet = m_command.CreateVarVec(FlattenVars(varSet));
            return newVarSet;
        }

        /// <summary>
        /// Build up a new varlist, where each structured var has been replaced by its
        /// corresponding flattened vars
        /// </summary>
        /// <param name="varList">the varlist to flatten</param>
        /// <returns>the new flattened varlist</returns>
        private VarList FlattenVarList(VarList varList)
        {
            VarList newVarList = Command.CreateVarList(FlattenVars(varList));
            return newVarList;
        }

        /// <summary>
        /// Simply flatten out every var in the keys, and return a new DistinctOp
        /// </summary>
        /// <param name="op">DistinctOp</param>
        /// <param name="n">Current subtree</param>
        /// <returns></returns>
        public override Node Visit(DistinctOp op, Node n)
        {
            VisitChildren(n);

            // Simply flatten out all the Vars
            VarVec newKeys = FlattenVarSet(op.Keys);
            n.Op = m_command.CreateDistinctOp(newKeys);
            return n;
        }

        /// <summary>
        /// GroupBy
        /// 
        /// Again, VisitChildren - for the Keys and Properties VarDefList nodes - does
        /// the real work. 
        /// 
        /// The "Keys" and the "OutputVars" varsets are updated to flatten out 
        /// references to any structured Vars.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(GroupByOp op, Node n)
        {
            VisitChildren(n);

            // update the output Vars and the key vars with the right sets
            VarVec newKeys = FlattenVarSet(op.Keys);
            VarVec newOutputs = FlattenVarSet(op.Outputs);

            if (newKeys != op.Keys || newOutputs != op.Outputs)
            {
                n.Op = m_command.CreateGroupByOp(newKeys, newOutputs);
            }

            return n;
        }

        /// <summary>
        /// GroupByInto
        /// 
        /// Again, VisitChildren - for the Keys and Properties VarDefList nodes - does
        /// the real work. 
        /// 
        /// The "Keys", "InputVars" and "OutputVars" varsets are updated to flatten out 
        /// references to any structured Vars.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(GroupByIntoOp op, Node n)
        {
            VisitChildren(n);

            // update the output Vars and the key vars with the right sets
            VarVec newKeys = FlattenVarSet(op.Keys);
            VarVec newInputs = FlattenVarSet(op.Inputs);
            VarVec newOutputs = FlattenVarSet(op.Outputs);

            if (newKeys != op.Keys || newInputs != op.Inputs || newOutputs != op.Outputs)
            {
                n.Op = m_command.CreateGroupByIntoOp(newKeys, newInputs, newOutputs);
            }

            return n;
        }

        /// <summary>
        /// ProjectOp
        /// 
        /// The computedVars (the VarDefList) are processed via the VisitChildren() call
        /// We then try to update the "Vars" property to flatten out any structured
        /// type Vars - if a new VarSet is produced, then the ProjectOp is cloned
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns>new subtree</returns>
        public override Node Visit(ProjectOp op, Node n)
        {
            VisitChildren(n);

            // update the output Vars with the right set of information 
            VarVec newVars = FlattenVarSet(op.Outputs);

            if (op.Outputs != newVars)
            {
                // If the set of vars is empty, that means we didn;t need any of the Vars
                if (newVars.IsEmpty)
                {
                    return n.Child0;
                }
                n.Op = m_command.CreateProjectOp(newVars);
            }
            return n;
        }

        /// <summary>
        /// ScanTableOp
        /// 
        /// Visit a scanTable Op. Flatten out the table's record into one column
        /// for each field. Additionally, set up the VarInfo map appropriately
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns>new subtree</returns>
        public override Node Visit(ScanTableOp op, Node n)
        {

            Var columnVar = op.Table.Columns[0];
            TypeInfo typeInfo = m_typeInfo.GetTypeInfo(columnVar.Type);
            md.RowType newRowType = typeInfo.FlattenedType;

            List<md.EdmProperty> properties = new List<System.Data.Metadata.Edm.EdmProperty>();
            List<md.EdmMember> keyProperties = new List<System.Data.Metadata.Edm.EdmMember>();
            HashSet<string> declaredProps = new HashSet<string>();
            foreach (md.EdmProperty p in TypeHelpers.GetAllStructuralMembers(columnVar.Type.EdmType))
            {
                declaredProps.Add(p.Name);
            }
            foreach (md.EdmProperty p in newRowType.Properties)
            {
                if (declaredProps.Contains(p.Name))
                {
                    properties.Add(p);
                }
            }
            foreach (PropertyRef pref in typeInfo.GetKeyPropertyRefs())
            {
                md.EdmProperty p = typeInfo.GetNewProperty(pref);
                keyProperties.Add(p);
            }

            //
            // Create a flattened table definition, and a table with that definiton;
            //
            TableMD newTableMD = m_command.CreateFlatTableDefinition(properties, keyProperties, op.Table.TableMetadata.Extent);
            Table newTable = m_command.CreateTableInstance(newTableMD);

            VarInfo varInfo = m_varInfoMap.CreateStructuredVarInfo(columnVar, newRowType, newTable.Columns, properties);

            n.Op = m_command.CreateScanTableOp(newTable);
            return n;
        }

        /// <summary>
        /// Get the *single" var produced by the subtree rooted at this node. 
        /// Returns null, if the node produces more than one var, or less than one
        /// </summary>
        /// <param name="n">the node</param>
        /// <returns>the single var produced by the node</returns>
        internal static Var GetSingletonVar(Node n)
        {
            switch (n.Op.OpType)
            {
                case OpType.Project:
                    {
                        ProjectOp projectOp = (ProjectOp)n.Op;
                        return (projectOp.Outputs.Count == 1) ? projectOp.Outputs.First : null;
                    }
                case OpType.ScanTable:
                    {
                        ScanTableOp tableOp = (ScanTableOp)n.Op;
                        return (tableOp.Table.Columns.Count == 1) ? tableOp.Table.Columns[0] : null;
                    }

                case OpType.Filter:
                case OpType.SingleRow:
                case OpType.Sort:
                case OpType.ConstrainedSort:
                    return GetSingletonVar(n.Child0);

                case OpType.UnionAll:
                case OpType.Intersect:
                case OpType.Except:
                    {
                        SetOp setOp = (SetOp)n.Op;
                        return (setOp.Outputs.Count == 1) ? setOp.Outputs.First : null;
                    }

                case OpType.Unnest:
                    {
                        UnnestOp unnestOp = (UnnestOp)n.Op;
                        return unnestOp.Table.Columns.Count == 1 ? unnestOp.Table.Columns[0] : null;
                    }

                case OpType.Distinct:
                    {
                        DistinctOp distinctOp = (DistinctOp)n.Op;
                        return (distinctOp.Keys.Count == 1) ? distinctOp.Keys.First : null;
                    }

                default:
                    return null;
            }
        }

        /// <summary>
        /// ScanViewOp
        /// 
        /// Flatten out the view definition, and return that after 
        /// the appropriate remapping
        /// </summary>
        /// <param name="op">the ScanViewOp</param>
        /// <param name="n">current subtree</param>
        /// <returns>the flattened view definition</returns>
        public override Node Visit(ScanViewOp op, Node n)
        {
            //
            // Get the "single" var produced by the input
            //
            Var inputVar = GetSingletonVar(n.Child0);
            PlanCompiler.Assert(inputVar != null, "cannot identify Var for the input node to the ScanViewOp");
            // and the table should have exactly one column
            PlanCompiler.Assert(op.Table.Columns.Count == 1, "table for scanViewOp has more than on column?");
            Var columnVar = op.Table.Columns[0];

            Node definingNode = VisitNode(n.Child0);

            VarInfo varInfo;
            if (!m_varInfoMap.TryGetVarInfo(inputVar, out varInfo))
            {
                PlanCompiler.Assert(false, "didn't find inputVar for scanViewOp?");
            }
            // we must be dealing with a structured column here
            StructuredVarInfo svarInfo = (StructuredVarInfo)varInfo;

            TypeInfo typeInfo = m_typeInfo.GetTypeInfo(columnVar.Type);

            // if this view does not represent an entityset, then we're pretty much
            // done. We simply add a mapping from the columnVar to the list of flattened
            // vars produced by the underlying projectOp
            m_varInfoMap.CreateStructuredVarInfo(columnVar, svarInfo.NewType, svarInfo.NewVars, svarInfo.Fields);
            return definingNode;
        }

        /// <summary>
        /// Convert a SortOp. Specifically, walk the SortKeys, and expand out 
        /// any Structured type Var references 
        /// </summary>
        /// <param name="op">the sortOp</param>
        /// <param name="n">the current node</param>
        /// <returns>new subtree</returns>
        public override Node Visit(SortOp op, Node n)
        {
            VisitChildren(n);

            List<InternalTrees.SortKey> newSortKeys = HandleSortKeys(op.Keys);

            if (newSortKeys != op.Keys)
            {
                n.Op = m_command.CreateSortOp(newSortKeys);
            }
            return n;
        }

        /// <summary>
        /// UnnestOp
        /// 
        /// Converts an UnnestOp to the right shape. 
        /// - Visits UnnestOp input node and then rebuilds the Table instance according to the new flattened output of the input node.
        /// - In the case of a TVF call represented by Unnest(Func()) builds another projection that converts raw TVF output to a collection of flattened rows:
        ///   Unnest(Func()) -> Project(Unnest(Func()))
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns>new subtree</returns>
        public override Node Visit(UnnestOp op, Node n)
        {
            // Visit the children first
            VisitChildren(n);

            Var newUnnestVar = null;
            md.EdmFunction processingTVF = null;

            if (n.HasChild0)
            {
                Node chi = n.Child0;
                VarDefOp varDefOp = chi.Op as VarDefOp;

                if (null != varDefOp)
                {
                    if (TypeUtils.IsCollectionType(varDefOp.Var.Type))
                    {
                        ComputedVar computedVar = (ComputedVar)varDefOp.Var;

                        if (chi.HasChild0 && chi.Child0.Op.OpType == OpType.Function)
                        {
                            // For a TVF function call use the original non-flattened variable:
                            // the function will not return properties described in the flattened type, there would be no null sentinel and 
                            // row prop names will be as declared in the function signature.
                            // The mismatch between the flattened type and the orignial type is fixed by wrapping into a ProjectOp produced by CreateTVFProjection(...).
                            newUnnestVar = computedVar;
                            processingTVF = ((FunctionOp)chi.Child0.Op).Function;
                        }
                        else
                        {
                            // Flatten the computer var and add it to m_varInfoMap.
                            List<Node> newChildren = new List<Node>();
                            md.TypeUsage newType;
                            FlattenComputedVar(computedVar, chi, out newChildren, out newType);
                            PlanCompiler.Assert(newChildren.Count == 1, "Flattening unnest var produced more than one Var.");
                            n.Child0 = newChildren[0];
                        }
                    }
                }
            }

            if (processingTVF != null)
            {
                PlanCompiler.Assert(newUnnestVar != null, "newUnnestVar must be initialized in the TVF case.");
            }
            else
            {
                // Fetch the new unnestVar that should have been prepared inside the FlattenComputedVar call above.
                // If the new var info is not ready then the shape of the unnest or the variable type is incorrect.
                VarInfo unnestVarInfo;
                if (m_varInfoMap.TryGetVarInfo(op.Var, out unnestVarInfo) && unnestVarInfo.Kind == VarInfoKind.CollectionVarInfo)
                {
                    newUnnestVar = ((CollectionVarInfo)unnestVarInfo).NewVar;
                }
                else
                {
                    throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.WrongVarType);
                }
            }

            // If the type of table column var representing the collection element is not structured, simply update the n.Op with the new var and return n.
            // Otherwise rebuild the UnnestOp based on the new flattened type of the new input var fetched above.
            // If the input var represents a TVF call, then wrap the newly rebuilt UnnestOp into a ProjectOp (see below for more details).
            Var unnestTableColumnVar = op.Table.Columns[0];
            if (!TypeUtils.IsStructuredType(unnestTableColumnVar.Type))
            {
                PlanCompiler.Assert(processingTVF == null, "TVFs returning a collection of values of a non-structured type are not supported");

                if (md.TypeSemantics.IsEnumerationType(unnestTableColumnVar.Type) || md.TypeSemantics.IsStrongSpatialType(unnestTableColumnVar.Type))
                {
                    UnnestOp unnestOp = m_command.CreateUnnestOp(newUnnestVar);
                    m_varInfoMap.CreatePrimitiveTypeVarInfo(unnestTableColumnVar, unnestOp.Table.Columns[0]);
                    n.Op = unnestOp;
                }
                else
                {
                    // Update the current unnest node with the new UnnestOp based on the newUnnestVar and the old table.
                    n.Op = m_command.CreateUnnestOp(newUnnestVar, op.Table);
                }
            }
            else
            {
                //
                // 1. Flatten out the table to be used in the new UnnestOp.
                //    If processingTVF use the typeInfo.FlattenedType for the new table structure,
                //    otherwise use the original type of the unnestTableColumnVar representing precisely the fields returned by the TVF.
                //
                // 2. Create the new UnnestOp using the new unnest input var and the new table.
                //    Note that if processingTVF, the new unnest input var is not flattened (see code above for more info).
                //
                // 3. If processingTVF, create a ProjectOp and wrap the new UnnestOp into it.
                //    The new ProjectOp projects fields of the typeInfo.FlattenedType. The values of the projected fields
                //    are taken from the corresponding variables of the new UnnestOp. 
                //    The new ProjectOp also projects a null sentinenel if the flattened type has one.
                //
                // 4. Update m_varInfoMap with the new new entry that maps the old unnestTableColumnVar to the list of new flattened vars:
                //    If processingTVF, the new flattended vars are the outputs of the ProjectOp, 
                //    otherwise the new flattened vars are the columns on the new UnnestOp.Table.
                //

                //
                // Get the flattened representation of the table column var type.
                //
                TypeInfo typeInfo = m_typeInfo.GetTypeInfo(unnestTableColumnVar.Type);

                TableMD newTableMetadata;
                if (processingTVF != null)
                {
                    // For the direct function call use the original non-flattened type.
                    // The function will not return values according to the flattened type:
                    // there would be no null sentinel and row prop names will be as declared in the function signature.
                    // In the code below we create a projection over the function call that produces the flattened.
                    md.RowType tvfReturnType = TypeHelpers.GetTvfReturnType(processingTVF);
                    PlanCompiler.Assert(Command.EqualTypes(tvfReturnType, unnestTableColumnVar.Type.EdmType), "Unexpected TVF return type (row type is expected).");
                    newTableMetadata = m_command.CreateFlatTableDefinition(tvfReturnType.Properties, GetTvfResultKeys(processingTVF), null);
                }
                else
                {
                    newTableMetadata = m_command.CreateFlatTableDefinition(typeInfo.FlattenedType);
                }
                Table newTable = m_command.CreateTableInstance(newTableMetadata);

                // Update the current unnest node with the new UnnestOp based on the newUnnestVar and newTable.
                n.Op = m_command.CreateUnnestOp(newUnnestVar, newTable);
                List<Var> newVars;
                if (processingTVF != null)
                {
                    // Replace the current Unnest(Func()) with the new Project(Unnest(Func()))
                    n = CreateTVFProjection(n, newTable.Columns, typeInfo, out newVars);
                }
                else
                {
                    newVars = newTable.Columns;
                }

                // Map the unnestTableColumnVar to the list of the new flattened vars.
                m_varInfoMap.CreateStructuredVarInfo(unnestTableColumnVar,
                                                     typeInfo.FlattenedType,
                                                     newVars,
                                                     typeInfo.FlattenedType.Properties.ToList<md.EdmProperty>());
            }

            return n;
        }

        private IEnumerable<md.EdmProperty> GetTvfResultKeys(md.EdmFunction tvf)
        {
            md.EdmProperty[] keys;
            if (m_tvfResultKeys.TryGetValue(tvf, out keys))
            {
                return keys;
            }
            return Enumerable.Empty<md.EdmProperty>();
        }

        #region SetOps

        /// <summary>
        /// SetOp
        /// 
        /// Converts all SetOps - union/intersect/except. 
        /// Calls VisitChildren() to do the bulk of the work. After that, the VarMaps
        /// need to be updated to reflect the removal of any structured Vars
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns>new subtree</returns>
        protected override Node VisitSetOp(SetOp op, Node n)
        {
            VisitChildren(n);

            // Now walk through the first VarMap, and identify the Vars that are needed
            for (int i = 0; i < op.VarMap.Length; i++)
            {
                List<ComputedVar> newComputedVars;
                op.VarMap[i] = FlattenVarMap(op.VarMap[i], out newComputedVars);
                if (newComputedVars != null)
                {
                    n.Children[i] = FixupSetOpChild(n.Children[i], op.VarMap[i], newComputedVars);
                }
            }

            // now get the set of Vars that we will actually need
            op.Outputs.Clear();
            foreach (Var v in op.VarMap[0].Keys)
            {
                op.Outputs.Set(v);
            }
            return n;
        }

        /// <summary>
        /// Fixes up a SetOp child.
        /// As part of Var flattening, it may so happen that the outer var in the VarMap
        /// may require a property that has no corresponding analog in the inner Var
        /// This logically implies that the corresponding inner property is null. H
        /// What we do here is to throw an additional projectOp over the setOp child to
        /// add computed Vars (whose defining expressions are null constants) for each
        /// of those missing properties
        /// </summary>
        /// <param name="setOpChild">one child of the setop</param>
        /// <param name="varMap">the varmap for this child</param>
        /// <param name="newComputedVars">list of new Vars produced</param>
        /// <returns>new node for the setOpchild (if any)</returns>
        private Node FixupSetOpChild(Node setOpChild, VarMap varMap, List<ComputedVar> newComputedVars)
        {
            PlanCompiler.Assert(null != setOpChild, "null setOpChild?");
            PlanCompiler.Assert(null != varMap, "null varMap?");
            PlanCompiler.Assert(null != newComputedVars, "null newComputedVars?");

            // Walk through the list of Vars that have no inner analog, and create 
            // a computed Var for each of them
            VarVec newVarSet = m_command.CreateVarVec();
            foreach (KeyValuePair<Var, Var> kv in varMap)
            {
                newVarSet.Set(kv.Value);
            }

            List<Node> varDefOpNodes = new List<Node>();
            foreach (Var v in newComputedVars)
            {
                VarDefOp varDefOp = m_command.CreateVarDefOp(v);
                Node varDefOpNode = m_command.CreateNode(varDefOp, CreateNullConstantNode(v.Type));
                varDefOpNodes.Add(varDefOpNode);
            }
            Node varDefListNode = m_command.CreateNode(m_command.CreateVarDefListOp(), varDefOpNodes);
            ProjectOp projectOp = m_command.CreateProjectOp(newVarSet);
            Node projectNode = m_command.CreateNode(projectOp, setOpChild, varDefListNode);
            return projectNode;
        }

        /// <summary>
        /// Flattens out a VarMap. 
        /// 
        /// Any structured type Vars are expanded out; and collection type Vars 
        /// are replaced by new Vars that reflect the new collection types.
        /// 
        /// There is one special case when dealing with Structured type Vars -
        /// the output and input vars may no longer be 1-1; specifically, there 
        /// may be no input Var corresponding to an output var. In such cases, we 
        /// build up a new ComputedVar (with an expected value of null), and use that
        /// in place of the inner var. A subsequent stage will inspect the list of 
        /// new ComputedVars, and perform the appropriate fixups
        /// </summary>
        /// <param name="varMap">The VarMap to fixup</param>
        /// <param name="newComputedVars">list of any new computedVars that are created</param>
        /// <returns>a new VarMap</returns>
        private VarMap FlattenVarMap(VarMap varMap, out List<ComputedVar> newComputedVars)
        {
            newComputedVars = null;

            VarMap newVarMap = new VarMap();
            foreach (KeyValuePair<Var, Var> kv in varMap)
            {
                VarInfo innerVarInfo;
                VarInfo outerVarInfo;
                // Does the inner var have a Varinfo - if not, simply add it 
                // to the VarMap, and continue.
                // Otherwise, the Outer var must have a VarInfo too
                if (!m_varInfoMap.TryGetVarInfo(kv.Value, out innerVarInfo))
                {
                    newVarMap.Add(kv.Key, kv.Value);
                }
                else
                {
                    // get my own var info
                    if (!m_varInfoMap.TryGetVarInfo(kv.Key, out outerVarInfo))
                    {
                        outerVarInfo = FlattenSetOpVar((SetOpVar)kv.Key);
                    }

                    // If this Var represents a collection type, then simply 
                    // replace the singleton Var
                    if (outerVarInfo.Kind == VarInfoKind.CollectionVarInfo)
                    {
                        newVarMap.Add(((CollectionVarInfo)outerVarInfo).NewVar, ((CollectionVarInfo)innerVarInfo).NewVar);
                    }
                    else if (outerVarInfo.Kind == VarInfoKind.PrimitiveTypeVarInfo)
                    {
                        newVarMap.Add(((PrimitiveTypeVarInfo)outerVarInfo).NewVar, ((PrimitiveTypeVarInfo)innerVarInfo).NewVar);
                    }
                    else
                    { // structured type 
                        System.Diagnostics.Debug.Assert(outerVarInfo.Kind == VarInfoKind.StructuredTypeVarInfo, "StructuredVarInfo expected");

                        StructuredVarInfo outerSvarInfo = (StructuredVarInfo)outerVarInfo;
                        StructuredVarInfo innerSvarInfo = (StructuredVarInfo)innerVarInfo;

                        // walk through each property, and find the innerVar corresponding
                        // to that property
                        foreach (md.EdmProperty prop in outerSvarInfo.Fields)
                        {
                            Var outerVar;
                            Var innerVar;
                            bool ret = outerSvarInfo.TryGetVar(prop, out outerVar);
                            PlanCompiler.Assert(ret, "Could not find VarInfo for prop " + prop.Name);

                            if (!innerSvarInfo.TryGetVar(prop, out innerVar))
                            {
                                // we didn't find a corresponding innerVar. 
                                innerVar = m_command.CreateComputedVar(outerVar.Type);
                                if (newComputedVars == null)
                                {
                                    newComputedVars = new List<ComputedVar>();
                                }
                                newComputedVars.Add((ComputedVar)innerVar);
                            }
                            newVarMap.Add(outerVar, innerVar);
                        }
                    }
                }
            }
            return newVarMap;
        }

        /// <summary>
        /// Flattens a SetOpVar (used in SetOps). Simply produces a list of 
        /// properties corresponding to each desired property
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private VarInfo FlattenSetOpVar(SetOpVar v)
        {
            if (TypeUtils.IsCollectionType(v.Type))
            {
                md.TypeUsage newType = GetNewType(v.Type);
                Var newVar = m_command.CreateSetOpVar(newType);
                return m_varInfoMap.CreateCollectionVarInfo(v, newVar);
            }
            else if (md.TypeSemantics.IsEnumerationType(v.Type) || md.TypeSemantics.IsStrongSpatialType(v.Type))
            {
                md.TypeUsage newType = GetNewType(v.Type);
                Var newVar = m_command.CreateSetOpVar(newType);
                return m_varInfoMap.CreatePrimitiveTypeVarInfo(v, newVar);
            }

            // Get the "new" type for the Var
            TypeInfo typeInfo = m_typeInfo.GetTypeInfo(v.Type);
            // Get a list of properties that we think are necessary 
            PropertyRefList desiredProperties = m_varPropertyMap[v];
            List<Var> newVars = new List<Var>();
            List<md.EdmProperty> newProps = new List<md.EdmProperty>();
            bool hasNullSentinelVar = false;
            foreach (PropertyRef p in typeInfo.PropertyRefList)
            {
                if (!desiredProperties.Contains(p))
                {
                    continue;
                }
                md.EdmProperty newProperty = typeInfo.GetNewProperty(p);
                newProps.Add(newProperty);
                SetOpVar newVar = m_command.CreateSetOpVar(md.Helper.GetModelTypeUsage(newProperty));
                newVars.Add(newVar);
             
                // Check if it is a null sentinel var
                if (!hasNullSentinelVar && IsNullSentinelPropertyRef(p))
                {
                    hasNullSentinelVar = true;
                }
            }
            VarInfo varInfo = m_varInfoMap.CreateStructuredVarInfo(v, typeInfo.FlattenedType, newVars, newProps, hasNullSentinelVar);
            return varInfo;
        }

        #endregion

        #region DML RelOps

        //
        // DML RelOps are technically very simple - we should simply visit the 
        // children. However, I will defer this to when we actually support DML
        // so for now, the default implementation in the basicVisitor is to throw
        // unimplemented and that's good enough.
        //

        #endregion

        #endregion

        #region ScalarOp Visitors

        /// <summary>
        /// SoftCastOp
        /// 
        /// Visit the children first.
        /// 
        /// If this is an entity type, complextype or ref type, simply return the
        ///   visited child. (Rationale: These must be in the same type hierarchy; or
        ///   the earlier stages of query would have failed. And, we end up
        ///   using the same "flat" type for every type in the hierarchy)
        /// 
        /// If this is a scalar type, then simply return the current node 
        /// 
        /// If this is a collection type, then create a new softcastOp over the input
        ///  (the collection type may have changed)
        /// 
        /// Otherwise, we're dealing with a record type. Since our earlier 
        /// definitions of equivalence required that equivalent record types must 
        /// have the same number of fields, with "promotable" types, and in the same
        /// order; *and* since we asked for all properties (see PropertyPushdownHelper),
        /// the input must be a NewRecordOp, whose fields line up 1-1 with our fields. 
        /// Build up a new NewRecordOp based on the arguments to the input NewRecordOp, 
        /// and build up SoftCastOps for any field whose type does not match
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(SoftCastOp op, Node n)
        {
            md.TypeUsage inputTypeUsage = n.Child0.Op.Type;
            md.TypeUsage oldType = op.Type;

            // Always think of your children first
            VisitChildren(n);

            md.TypeUsage newType = GetNewType(oldType);

            if (md.TypeSemantics.IsRowType(oldType))
            {
                PlanCompiler.Assert(n.Child0.Op.OpType == OpType.NewRecord, "Expected a record constructor here. Found " + n.Child0.Op.OpType + " instead");

                TypeInfo inputTypeInfo = m_typeInfo.GetTypeInfo(inputTypeUsage);
                TypeInfo outputTypeInfo = m_typeInfo.GetTypeInfo(op.Type);

                NewRecordOp newOp = m_command.CreateNewRecordOp(newType);

                List<Node> newArgs = new List<Node>();

                // We have to adjust for when we're supposed to add/remove null sentinels; 
                // it is entirely possible that we may need to add multiple null sentinel
                // columns (See SQLBUDT #549068 for an example).  
                IEnumerator<md.EdmProperty> outputs = newOp.Properties.GetEnumerator();
                int outputPropertyCount = newOp.Properties.Count;
                outputs.MoveNext();

                IEnumerator<Node> inputs = n.Child0.Children.GetEnumerator();
                int inputPropertyCount = n.Child0.Children.Count;
                inputs.MoveNext();

                // We know that all Null Sentinels are added on the left side, so we'll
                // just keep adding them until we have the same number of properties on
                // both the input and the output...
                while (inputPropertyCount < outputPropertyCount)
                {
                    PlanCompiler.Assert(outputTypeInfo.HasNullSentinelProperty && !inputTypeInfo.HasNullSentinelProperty, "NullSentinelProperty mismatch on input?");

                    // make up a null sentinel; the output requires it.
                    newArgs.Add(CreateNullSentinelConstant());
                    outputs.MoveNext();
                    outputPropertyCount--;
                }

                // Likewise, we'll just drop any null sentinel columns from the input until
                // we have the same number of columns...
                while (inputPropertyCount > outputPropertyCount)
                {
                    PlanCompiler.Assert(!outputTypeInfo.HasNullSentinelProperty && inputTypeInfo.HasNullSentinelProperty, "NullSentinelProperty mismatch on output?");

                    // remove the null sentinel; the output doesn't require it.
                    inputs.MoveNext();
                    inputPropertyCount--;
                }

                do
                {
                    md.EdmProperty p = outputs.Current;
                    Node arg = BuildSoftCast(inputs.Current, md.Helper.GetModelTypeUsage(p));
                    newArgs.Add(arg);
                    outputs.MoveNext();
                }
                while (inputs.MoveNext());

                Node newNode = m_command.CreateNode(newOp, newArgs);
                return newNode;
            }
            else if (md.TypeSemantics.IsCollectionType(oldType))
            {
                //
                // Our collection type may have changed - 'coz the 
                // element type of the collection may have changed.
                // Simply build up a new castOp (if necessary)
                //
                return BuildSoftCast(n.Child0, newType);
            }
            else if (md.TypeSemantics.IsPrimitiveType(oldType))
            {
                // How primitive! Well, the Prime Directive prohibits me
                // from doing much with these. 
                return n;
            }
            else
            {
                PlanCompiler.Assert(md.TypeSemantics.IsNominalType(oldType) ||
                    md.TypeSemantics.IsReferenceType(oldType),
                    "Gasp! Not a nominal type or even a reference type");
                // I'm dealing with a nominal type (entity, complex type) or
                // a reference type here. Every type in the same hierarchy 
                // must have been rationalized into the same type, and so, we
                // won't need to do anything special
                PlanCompiler.Assert(Command.EqualTypes(newType, n.Child0.Op.Type),
                    "Types are not equal");
                return n.Child0;
            }
        }

        /// <summary>
        /// Removes or rewrites cast to enum or spatial type.
        /// </summary>
        /// <param name="op"><see cref="CastOp"/> operator.</param>
        /// <param name="n">Current node.</param>
        /// <returns>Visited, possible rewritten <paramref name="n"/>.</returns>
        public override Node Visit(CastOp op, Node n)
        {
            // Visit children first to get rid of all the nominal types (including enums) in the subtree. 
            VisitChildren(n);

            // if casting to enum (e.g. (Color)3) - get rid of the cast if underlying type of the enum is the same
            // as the type of the cast argument. If they are not the same rewrite the cast so that the argument 
            // is casted to the underlying enum type. 
            if (md.TypeSemantics.IsEnumerationType(op.Type))
            {
                // We visited subtree so the result type of the cast argument should be now primitive even if it originally was not (e.g. enum). 
                PlanCompiler.Assert(md.TypeSemantics.IsPrimitiveType(n.Child0.Op.Type), "Primitive type expected.");
                var underlyingType = md.Helper.GetUnderlyingEdmTypeForEnumType(op.Type.EdmType);
                return RewriteAsCastToUnderlyingType(underlyingType, op, n);
 
            }
            if (md.TypeSemantics.IsSpatialType(op.Type))
            {
                // We visited subtree so the result type of the cast argument should now be a union spatial type even if it was originally strong). 
                PlanCompiler.Assert(md.TypeSemantics.IsPrimitiveType(n.Child0.Op.Type, md.PrimitiveTypeKind.Geography) || md.TypeSemantics.IsPrimitiveType(n.Child0.Op.Type, md.PrimitiveTypeKind.Geometry), "Union spatial type expected.");
                var underlyingType = md.Helper.GetSpatialNormalizedPrimitiveType(op.Type.EdmType);
                return RewriteAsCastToUnderlyingType(underlyingType, op, n);
            }

            // children visited so it's OK just to return the node
            return n;
        }

        private Node RewriteAsCastToUnderlyingType(md.PrimitiveType underlyingType, CastOp op, Node n)
        {
            // if type of the argument and the underlying type match we can strip the Cast entirely
            if (underlyingType.PrimitiveTypeKind == ((md.PrimitiveType)n.Child0.Op.Type.EdmType).PrimitiveTypeKind)
            {
                return n.Child0;
            }
            else
            {
                return m_command.CreateNode(m_command.CreateCastOp(md.TypeUsage.Create(underlyingType, op.Type.Facets)), n.Child0);
            }
        }

        /// <summary>
        /// Converts Constant enum value to its underlying type.   Converts strong spatial constants to be union typed
        /// The node is processed only if it represents enum or strong spatial constant.
        /// </summary>
        /// <param name="op"><see cref="ConstantOp"/> operator.</param>
        /// <param name="n">Current node.</param>
        /// <returns>Possible rewritten <paramref name="n"/>.</returns>
        public override Node Visit(ConstantOp op, Node n)
        {
            PlanCompiler.Assert(n.Children.Count == 0, "Constant operations don't have children.");
            PlanCompiler.Assert(op.Value != null, "Value must not be null");

            // No need to visit children as none are expected

            if (md.TypeSemantics.IsEnumerationType(op.Type))
            {
                // For enums the value can be specified either as enum (e.g. Color.Yellow) or as a number.
                // We need the numeric value only so if it was not specified as a number we need to cast it to the 
                // underlying enum type.
                object constValue = op.Value.GetType().IsEnum ?
                    Convert.ChangeType(op.Value, op.Value.GetType().GetEnumUnderlyingType(), CultureInfo.InvariantCulture) :
                    op.Value;                   

                return m_command.CreateNode(
                        m_command.CreateConstantOp(
                            TypeHelpers.CreateEnumUnderlyingTypeUsage(op.Type), constValue));
            }
            if (md.TypeSemantics.IsStrongSpatialType(op.Type))
            {
                op.Type = TypeHelpers.CreateSpatialUnionTypeUsage(op.Type);
            }

            // ConstantOp has no children so there is nothing to visit - just return the original node.
            return n;
        }

        /// <summary>
        /// CaseOp
        /// 
        /// Special handling
        /// 
        /// If the case statement is of one of the following two shapes:
        ///     (1) case when X then NULL else Y, or
        ///     (2) case when X then Y else NULL,
        /// where Y is of row type and the types of the input CaseOp, the NULL and Y are the same,
        /// it gets rewritten into:  Y', where Y's null sentinel N' is:
        ///     (1) case when X then NULL else N, or
        /// where N is Y's null sentinel.
        /// </summary>
        /// <param name="op">the CaseOp</param>
        /// <param name="n">corresponding node</param>
        /// <returns>new subtree</returns>
        public override Node Visit(CaseOp op, Node n)
        {
            // Before visiting the children, check whether the case statment can be optimized 
            bool thenClauseIsNull;
            bool canSimplifyPrecheck = PlanCompilerUtil.IsRowTypeCaseOpWithNullability(op, n, out thenClauseIsNull);

            VisitChildren(n);

            if (canSimplifyPrecheck)
            {
                Node rewrittenNode;
                if (TryRewriteCaseOp(n, thenClauseIsNull, out rewrittenNode))
                {
                    return rewrittenNode;
                }
            }

            //
            // If the CaseOp returns a simple type, then we don't need to do 
            // anything special.
            //
            // 



            // If the CaseOp returns a collection, then we need to create a
            // new CaseOp of the new and improved collection type. Similarly
            // for enums we need to convert the result of the operation from 
            // the enum type to the underlying type of the enum type, and
            // for spatial types we must convert it to the underlying spatial union type.
            if (TypeUtils.IsCollectionType(op.Type) || md.TypeSemantics.IsEnumerationType(op.Type) || md.TypeSemantics.IsStrongSpatialType(op.Type))
            {
                md.TypeUsage newType = GetNewType(op.Type);

                n.Op = m_command.CreateCaseOp(newType);
                return n;
            }
            else if (TypeUtils.IsStructuredType(op.Type))
            {
                // We've got a structured type, so the CaseOp is flattened out into 
                // a NewRecordOp via the FlattenCaseOp method.
                PropertyRefList desiredProperties = m_nodePropertyMap[n];
                Node newNode = FlattenCaseOp(op, n, m_typeInfo.GetTypeInfo(op.Type), desiredProperties);
                return newNode;
            }
            else
            {
                return n;
            }
        }

        /// <summary>
        /// Given a case statement of one of the following two shapes:
        ///     (1) case when X then NULL else Y, or
        ///     (2) case when X then Y else NULL,
        /// where Y is of row type and the types of the input CaseOp, the NULL and Y are the same,
        /// it rewrittes into:  Y', where Y's null sentinel N' is:
        ///     (1) case when X then NULL else N, or
        /// where N is Y's null sentinel.
        /// 
        /// The rewrite only happens if: 
        ///     (1) Y has null sentinel, and
        ///     (2) Y is a NewRecordOp.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="thenClauseIsNull"></param>
        /// <param name="rewrittenNode"></param>
        /// <returns>Whether a rewrite was done</returns>
        private bool TryRewriteCaseOp(Node n, bool thenClauseIsNull, out Node rewrittenNode)
        {
            rewrittenNode = n;

            //If the type of the case op does not have a null sentinel, we can't do the rewrite.
            if (!m_typeInfo.GetTypeInfo(n.Op.Type).HasNullSentinelProperty)
            {
                return false;
            }

            Node resultNode = thenClauseIsNull ? n.Child2 : n.Child1;
            if (resultNode.Op.OpType != OpType.NewRecord)
            {
                return false;
            }

            //Rewrite the null sentinel, which is the first child of the resultNode
            Node currentNullSentinel = resultNode.Child0;
            md.TypeUsage integerType = this.m_command.IntegerType;
            PlanCompiler.Assert(currentNullSentinel.Op.Type.EdmEquals(integerType), "Column that is expected to be a null sentinel is not of Integer type.");

            CaseOp newCaseOp = m_command.CreateCaseOp(integerType);
            List<Node> children = new List<Node>(3);

            //The the 'when' from the case statement
            children.Add(n.Child0);

            Node nullSentinelNullNode = m_command.CreateNode(m_command.CreateNullOp(integerType));
            Node nullSentinelThenNode = thenClauseIsNull ? nullSentinelNullNode : currentNullSentinel;
            Node nullSentinelElseNode = thenClauseIsNull ? currentNullSentinel : nullSentinelNullNode;
            children.Add(nullSentinelThenNode);
            children.Add(nullSentinelElseNode);

            //Use the case op as a new null sentinel
            resultNode.Child0 = m_command.CreateNode(newCaseOp, children);

            rewrittenNode = resultNode;
            return true;
        }

        /// <summary>
        /// Flattens a CaseOp - Specifically, if the CaseOp returns a structuredtype,
        /// then the CaseOp is broken up so that we build up a "flat" record constructor
        /// for that structured type, with each argument to the record constructor being 
        /// a (scalar) CaseOp.  For example:
        /// 
        ///     Case when b1 then e1 else e2 end
        /// 
        /// gets translated into:
        /// 
        ///     RecordOp(case when b1 then e1.a else e2.a end,
        ///              case when b1 then e1.b else e2.b end,
        ///              ...)
        /// 
        /// The property extraction is optimized by producing only those properties 
        /// that have actually been requested.
        /// </summary>
        /// <param name="op">the CaseOp</param>
        /// <param name="n">Node corresponding to the CaseOp</param>
        /// <param name="typeInfo">Information about the type</param>
        /// <param name="desiredProperties">Set of properties desired</param>
        /// <returns></returns>
        private Node FlattenCaseOp(CaseOp op, Node n, TypeInfo typeInfo, PropertyRefList desiredProperties)
        {
            // Build up a type constructor - with only as many fields filled in 
            // as are desired. 
            List<md.EdmProperty> fieldTypes = new List<md.EdmProperty>();
            List<Node> fieldValues = new List<Node>();

            foreach (PropertyRef pref in typeInfo.PropertyRefList)
            {
                // Is this property desired later?
                if (!desiredProperties.Contains(pref))
                {
                    continue;
                }
                md.EdmProperty property = typeInfo.GetNewProperty(pref);

                // Build up an accessor for this property across each when/then clause
                List<Node> caseChildren = new List<Node>();
                for (int i = 0; i < n.Children.Count - 1; )
                {
                    Node whenNode = Copy(n.Children[i]);
                    caseChildren.Add(whenNode);
                    i++;

                    Node propNode = BuildAccessorWithNulls(n.Children[i], property);
                    caseChildren.Add(propNode);
                    i++;
                }
                Node elseNode = BuildAccessorWithNulls(n.Children[n.Children.Count - 1], property);
                caseChildren.Add(elseNode);

                Node caseNode = m_command.CreateNode(m_command.CreateCaseOp(md.Helper.GetModelTypeUsage(property)), caseChildren);

                fieldTypes.Add(property);
                fieldValues.Add(caseNode);
            }

            NewRecordOp newRec = m_command.CreateNewRecordOp(typeInfo.FlattenedTypeUsage, fieldTypes);
            return m_command.CreateNode(newRec, fieldValues);
        }

        /// <summary>
        /// CollectOp
        /// 
        /// Nothing much to do - simply update the result type
        /// </summary>
        /// <param name="op">the NestOp</param>
        /// <param name="n">corresponding node</param>
        /// <returns>new subtree</returns>
        public override Node Visit(CollectOp op, Node n)
        {
            VisitChildren(n);
            // simply update the desired type
            n.Op = m_command.CreateCollectOp(GetNewType(op.Type));
            return n;
        }

        /// <summary>
        /// ComparisonOp
        /// 
        /// If the inputs to the comparisonOp are Refs/records/entitytypes, then
        /// we need to flatten these out. Of course, the only reasonable comparisons
        /// should be EQ and NE
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(ComparisonOp op, Node n)
        {
            md.TypeUsage child0Type = ((ScalarOp)n.Child0.Op).Type;
            md.TypeUsage child1Type = ((ScalarOp)n.Child1.Op).Type;

            if (!TypeUtils.IsStructuredType(child0Type))
            {
                return VisitScalarOpDefault(op, n);
            }

            VisitChildren(n); // visit the children first

            // We're now dealing with a structured type
            PlanCompiler.Assert(!(md.TypeSemantics.IsComplexType(child0Type) || md.TypeSemantics.IsComplexType(child1Type)), "complex type?"); // cannot be a complex type
            PlanCompiler.Assert(op.OpType == OpType.EQ || op.OpType == OpType.NE, "non-equality comparison of structured types?");

            //
            // Strictly speaking, we should be able to use the typeinfo of either of the arguments. 
            // However, as things stand today, we do have scenarios where the types on the 
            // two sides (records mainly) are equivalent, but not identical. This non-identicality
            // may involve the field types being different, the field names being different etc. - but
            // we may be assured that the order of the field types is fixed.
            //
            TypeInfo child0TypeInfo = m_typeInfo.GetTypeInfo(child0Type);
            TypeInfo child1TypeInfo = m_typeInfo.GetTypeInfo(child1Type);
            List<md.EdmProperty> properties1;
            List<md.EdmProperty> properties2;
            List<Node> values1;
            List<Node> values2;

            // get a list of the relevant properties and values from each of the children

            GetPropertyValues(child0TypeInfo, OperationKind.Equality, n.Child0, false, out properties1, out values1);
            GetPropertyValues(child1TypeInfo, OperationKind.Equality, n.Child1, false, out properties2, out values2);

            PlanCompiler.Assert((properties1.Count == properties2.Count) && (values1.Count == values2.Count), "different shaped structured types?");

            // Build up an and-chain of comparison ops on the property values
            Node andNode = null;
            for (int i = 0; i < values1.Count; i++)
            {
                ComparisonOp newCompOp = m_command.CreateComparisonOp(op.OpType);
                Node newCompNode = m_command.CreateNode(newCompOp, values1[i], values2[i]);
                if (null == andNode)
                    andNode = newCompNode;
                else
                    andNode = m_command.CreateNode(m_command.CreateConditionalOp(OpType.And), andNode, newCompNode);
            }
            return andNode;
        }

        /// <summary>
        /// ConditionalOp
        /// 
        /// IsNull requires special handling.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(ConditionalOp op, Node n)
        {
            if (op.OpType != OpType.IsNull)
            {
                return VisitScalarOpDefault(op, n);
            }

            //
            // Special handling for IS NULL ops on structured types
            //
            // For structured types, we simply convert this into an AND chain of
            // IS NULL predicates, one for each property. There are a couple of
            // optimizations that we perform. 
            //
            // For entity types, we simply perfom the IS NULL operations on the 
            // key attributes alone. 
            //
            // Complex types must have a typeid property - the isnull is pushed to the
            // typeid property
            //
            // We do NOT support IsNull for Collections
            //

            md.TypeUsage childOpType = ((ScalarOp)n.Child0.Op).Type;

            // Special cases are for structured types only 
            if (!TypeUtils.IsStructuredType(childOpType))
            {
                return VisitScalarOpDefault(op, n);
            }

            // visit the children first
            VisitChildren(n);

            TypeInfo typeInfo = m_typeInfo.GetTypeInfo(childOpType);

            // Otherwise, build up an and-chain of is null checks for each appropriate
            // property - which should consist only of key properties for Entity types.
            List<md.EdmProperty> properties = null;
            List<Node> values = null;
            GetPropertyValues(typeInfo, OperationKind.IsNull, n.Child0, false, out properties, out values);

            PlanCompiler.Assert(properties.Count == values.Count && properties.Count > 0, "No properties returned from GetPropertyValues(IsNull)?");

            Node andNode = null;
            foreach (Node propertyValue in values)
            {
                Node isNullNode = m_command.CreateNode(m_command.CreateConditionalOp(OpType.IsNull), propertyValue);
                if (andNode == null)
                    andNode = isNullNode;
                else
                    andNode = m_command.CreateNode(m_command.CreateConditionalOp(OpType.And), andNode, isNullNode);
            }
            return andNode;
        }

        /// <summary>
        /// Convert a ConstrainedSortOp. Specifically, walk the SortKeys, and expand out 
        /// any Structured type Var references 
        /// </summary>
        /// <param name="op">the constrainedSortOp</param>
        /// <param name="n">the current node</param>
        /// <returns>new subtree</returns>
        public override Node Visit(ConstrainedSortOp op, Node n)
        {
            VisitChildren(n);

            List<InternalTrees.SortKey> newSortKeys = HandleSortKeys(op.Keys);

            if (newSortKeys != op.Keys)
            {
                n.Op = m_command.CreateConstrainedSortOp(newSortKeys, op.WithTies);
            }
            return n;
        }

        /// <summary>
        /// GetEntityKeyOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(GetEntityRefOp op, Node n)
        {
            return FlattenGetKeyOp(op, n);
        }

        /// <summary>
        /// GetRefKeyOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(GetRefKeyOp op, Node n)
        {
            return FlattenGetKeyOp(op, n);
        }

        /// <summary>
        /// GetEntityKeyOp/GetRefKeyOp common handling
        /// 
        /// In either case, get the "key" properties from the input entity/ref, and 
        /// build up a record constructor from these values
        /// </summary>
        /// <param name="op">the GetRefKey/GetEntityKey op</param>
        /// <param name="n">current subtree</param>
        /// <returns>new expression subtree</returns>
        private Node FlattenGetKeyOp(ScalarOp op, Node n)
        {
            PlanCompiler.Assert(op.OpType == OpType.GetEntityRef || op.OpType == OpType.GetRefKey, "Expecting GetEntityRef or GetRefKey ops");

            TypeInfo inputTypeInfo = m_typeInfo.GetTypeInfo(((ScalarOp)n.Child0.Op).Type);
            TypeInfo outputTypeInfo = m_typeInfo.GetTypeInfo(op.Type);

            // Visit the child - will flatten out the input ref/entity
            VisitChildren(n);

            // Get "key" properties (and the corresponding values) from the input
            List<md.EdmProperty> inputFieldTypes;
            List<Node> inputFieldValues;

            // Get the key properties for GetRefKey; get the Identity properties
            // for GetEntityRef
            if (op.OpType == OpType.GetRefKey)
            {
                GetPropertyValues(inputTypeInfo, OperationKind.GetKeys, n.Child0, false /* ignore missing props */, out inputFieldTypes, out inputFieldValues);
            }
            else
            {
                PlanCompiler.Assert(op.OpType == OpType.GetEntityRef,
                    "Expected OpType.GetEntityRef: Found " + op.OpType);
                GetPropertyValues(inputTypeInfo, OperationKind.GetIdentity, n.Child0, false, out inputFieldTypes, out inputFieldValues);
            }

            if (outputTypeInfo.HasNullSentinelProperty && !inputTypeInfo.HasNullSentinelProperty)
            {
                // Add a null sentinel column, the input doesn't have one but the output requires it.
                inputFieldValues.Insert(0, CreateNullSentinelConstant());
            }

            // create an appropriate record constructor
            List<md.EdmProperty> outputFieldTypes = new List<md.EdmProperty>(outputTypeInfo.FlattenedType.Properties);
            PlanCompiler.Assert(inputFieldValues.Count == outputFieldTypes.Count, "fieldTypes.Count mismatch?");

            NewRecordOp rec = m_command.CreateNewRecordOp(outputTypeInfo.FlattenedTypeUsage, outputFieldTypes);
            Node newNode = m_command.CreateNode(rec, inputFieldValues);
            return newNode;
        }

        /// <summary>
        /// Common handler for PropertyOp and RelPropertyOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <param name="propertyRef"></param>
        /// <param name="throwIfMissing">ignore missing properties</param>
        /// <returns></returns>
        private Node VisitPropertyOp(Op op, Node n, PropertyRef propertyRef, bool throwIfMissing)
        {
            PlanCompiler.Assert(op.OpType == OpType.Property || op.OpType == OpType.RelProperty,
                "Unexpected optype: " + op.OpType);

            md.TypeUsage inputType = n.Child0.Op.Type;
            md.TypeUsage outputType = op.Type;

            // First visit all my children
            VisitChildren(n);

            // If the instance is not a structured type (ie) it is a udt, then there 
            // is little for us to do. Simply return 
            if (TypeUtils.IsUdt(inputType))
            {
                return n;
            }

            Node newNode = null;
            TypeInfo inputTypeInfo = m_typeInfo.GetTypeInfo(inputType);

            if (TypeUtils.IsStructuredType(outputType))
            {
                TypeInfo outputTypeInfo = m_typeInfo.GetTypeInfo(outputType);
                List<md.EdmProperty> fieldTypes = new List<md.EdmProperty>();
                List<Node> fieldValues = new List<Node>();
                PropertyRefList expectedProperties = m_nodePropertyMap[n];

                foreach (PropertyRef npr in outputTypeInfo.PropertyRefList)
                {
                    // Is this a property that's desired by my consumers?
                    if (expectedProperties.Contains(npr))
                    {  
                        PropertyRef newPropRef = npr.CreateNestedPropertyRef(propertyRef);
                        md.EdmProperty newNestedProp;
                        
                        if (inputTypeInfo.TryGetNewProperty(newPropRef, throwIfMissing, out newNestedProp))
                        {
                            md.EdmProperty outputNestedProp = outputTypeInfo.GetNewProperty(npr);
                            Node field = BuildAccessor(n.Child0, newNestedProp);
                            if (null != field)
                            {
                                fieldTypes.Add(outputNestedProp);
                                fieldValues.Add(field);
                            }
                        }
                    }
                }
                Op newRecordOp = m_command.CreateNewRecordOp(outputTypeInfo.FlattenedTypeUsage, fieldTypes);
                newNode = m_command.CreateNode(newRecordOp, fieldValues);
            }
            else
            {
                md.EdmProperty newProp = inputTypeInfo.GetNewProperty(propertyRef);
                // Build an accessor over the new property
                newNode = this.BuildAccessorWithNulls(n.Child0, newProp);
            }
            return newNode;
        }

        /// <summary>
        /// PropertyOp
        /// 
        /// If this is a scalar/collection property, then simply get the appropriate
        /// field out.
        /// 
        /// Otherwise, build up a record constructor corresponding to the result
        /// type - optimize this by only getting those properties that are needed
        /// 
        /// If the instance is not a structured type (ie) it is a UDT, then simply return
        /// 
        /// </summary>
        /// <param name="op">the PropertyOp</param>
        /// <param name="n">the corresponding node</param>
        /// <returns>new subtree</returns>
        public override Node Visit(PropertyOp op, Node n)
        {
            return VisitPropertyOp(op, n, new SimplePropertyRef(op.PropertyInfo), throwIfMissing: true);
        }

        /// <summary>
        /// RelPropertyOp. Pick out the appropriate property from the child
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(RelPropertyOp op, Node n)
        {
            // DevDiv #7246: When the underlying source is "OF TYPE ONLY" query, the view does not have the
            // rel properties for the subtypes. However, relationship span may try to navigate to these properties, 
            // thus we need to ignore them (i.e. the nulls are produced)
            return VisitPropertyOp(op, n, new RelPropertyRef(op.PropertyInfo), throwIfMissing: false);
        }

        /// <summary>
        /// RefOp
        /// 
        /// Simply convert this into the corresponding record type - with one
        /// field for each key, and one for the entitysetid
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(RefOp op, Node n)
        {
            TypeInfo inputTypeInfo = m_typeInfo.GetTypeInfo(((ScalarOp)n.Child0.Op).Type);
            TypeInfo outputTypeInfo = m_typeInfo.GetTypeInfo(op.Type);

            // visit children now
            VisitChildren(n);

            // Get the list of fields and properties from the input (key) op
            List<md.EdmProperty> inputFields;
            List<Node> inputFieldValues;
            GetPropertyValues(inputTypeInfo, OperationKind.All, n.Child0, false, out inputFields, out inputFieldValues);

            // Get my property list
            List<md.EdmProperty> outputFields = new List<md.EdmProperty>(outputTypeInfo.FlattenedType.Properties);

            if (outputTypeInfo.HasEntitySetIdProperty)
            {
                PlanCompiler.Assert(outputFields[0] == outputTypeInfo.EntitySetIdProperty, "OutputField0 must be the entitySetId property");

                if (inputTypeInfo.HasNullSentinelProperty && !outputTypeInfo.HasNullSentinelProperty)
                {  // realistically, REFs can't have null sentinels, but I'm being pedantic...
                    PlanCompiler.Assert(outputFields.Count == inputFields.Count, "Mismatched field count: Expected " + inputFields.Count + "; Got " + outputFields.Count);
                    RemoveNullSentinel(inputTypeInfo, inputFields, inputFieldValues, outputFields);
                }
                else
                {
                    PlanCompiler.Assert(outputFields.Count == inputFields.Count + 1, "Mismatched field count: Expected " + (inputFields.Count + 1) + "; Got " + outputFields.Count);
                }

                // Now prepend a value for the entitysetid property and a value for this property
                int entitySetId = m_typeInfo.GetEntitySetId(op.EntitySet);
                inputFieldValues.Insert(0, m_command.CreateNode(m_command.CreateInternalConstantOp(md.Helper.GetModelTypeUsage(outputTypeInfo.EntitySetIdProperty), entitySetId)));
            }
            else
            {
                if (inputTypeInfo.HasNullSentinelProperty && !outputTypeInfo.HasNullSentinelProperty)
                { // realistically, REFs can't have null sentinels, but I'm being pedantic...
                    RemoveNullSentinel(inputTypeInfo, inputFields, inputFieldValues, outputFields);
                }

                PlanCompiler.Assert(outputFields.Count == inputFields.Count, "Mismatched field count: Expected " + inputFields.Count + "; Got " + outputFields.Count);
            }

            // now build up a NewRecordConstructor with the appropriate info
            NewRecordOp recOp = m_command.CreateNewRecordOp(outputTypeInfo.FlattenedTypeUsage, outputFields);
            Node newNode = m_command.CreateNode(recOp, inputFieldValues);

            return newNode;
        }

        // We have to adjust for when we're supposed to remove null sentinels; 
        // columns (See SQLBUDT #553534 for an example).  Note that we shouldn't
        // have to add null sentinels here, since reference types won't be expecting
        // them (the fact that the key is null is good enough...)
        private static void RemoveNullSentinel(TypeInfo inputTypeInfo, List<md.EdmProperty> inputFields, List<Node> inputFieldValues, List<md.EdmProperty> outputFields)
        {
            PlanCompiler.Assert(inputFields[0] == inputTypeInfo.NullSentinelProperty, "InputField0 must be the null sentinel property");
            inputFields.RemoveAt(0);
            inputFieldValues.RemoveAt(0);
        }

        /// <summary>
        /// VarRefOp
        /// 
        /// Replace a VarRef with a copy of the corresponding "Record" constructor.
        /// For collection and enum Var references replaces VarRef with the new Var
        /// stored in the VarInfo.
        /// </summary>
        /// <param name="op">the VarRefOp</param>
        /// <param name="n">the node</param>
        /// <returns>new subtree</returns>
        public override Node Visit(VarRefOp op, Node n)
        {
            // Lookup my VarInfo
            VarInfo varInfo;
            if (!m_varInfoMap.TryGetVarInfo(op.Var, out varInfo))
            {
                PlanCompiler.Assert(!TypeUtils.IsStructuredType(op.Type),
                    "No varInfo for a structured type var: Id = " + op.Var.Id + " Type = " + op.Type);

                return n;
            }

            if (varInfo.Kind == VarInfoKind.CollectionVarInfo)
            {
                n.Op = m_command.CreateVarRefOp(((CollectionVarInfo)varInfo).NewVar);
                return n;
            }
            else if (varInfo.Kind == VarInfoKind.PrimitiveTypeVarInfo)
            {
                n.Op = m_command.CreateVarRefOp(((PrimitiveTypeVarInfo)varInfo).NewVar);
                return n;                
            }
            else
            {
                // A very specialized record constructor mechanism for structured type Vars.
                // We look up the VarInfo corresponding to the Var - which has a set of fields
                // and the corresponding properties that we need to produce

                StructuredVarInfo structuredVarInfo = (StructuredVarInfo)varInfo;

                NewRecordOp newOp = m_command.CreateNewRecordOp(structuredVarInfo.NewTypeUsage, structuredVarInfo.Fields);
                List<Node> newNodeChildren = new List<Node>();
                foreach (Var v in varInfo.NewVars)
                {
                    VarRefOp newVarRefOp = m_command.CreateVarRefOp(v);
                    newNodeChildren.Add(m_command.CreateNode(newVarRefOp));
                }
                Node newNode = m_command.CreateNode(newOp, newNodeChildren);
                return newNode;
            }
        }

        #region record construction ops

        /// <summary>
        /// Handler for NewEntity
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(NewEntityOp op, Node n)
        {
            return FlattenConstructor(op, n);
        }

        /// <summary>
        /// NewInstanceOp
        /// </summary>
        /// <param name="op">the NewInstanceOp</param>
        /// <param name="n">corresponding node</param>
        /// <returns>new subtree</returns>
        public override Node Visit(NewInstanceOp op, Node n)
        {
            return FlattenConstructor(op, n);
        }

        /// <summary>
        /// DiscriminatedNewInstanceOp
        /// </summary>
        /// <param name="op">the DiscriminatedNewInstanceOp</param>
        /// <param name="n">corresponding node</param>
        /// <returns>new subtree</returns>
        public override Node Visit(DiscriminatedNewEntityOp op, Node n)
        {
            return FlattenConstructor(op, n);
        }

        /// <summary>
        /// Given an explicit discriminator value, map to normalized values. Essentially, this allows
        /// a discriminated new instance to coexist with free-floating entities, MEST, etc. which use
        /// general purpose ordpath type ids (e.g. '0X0X')
        /// 
        /// An example of the normalization is given:
        /// 
        /// CASE 
        ///     WHEN discriminator = 'Base' THEN '0X'
        ///     WHEN discriminator = 'Derived1' THEN '0X0X'
        ///     WHEN discriminator = 'Derived2' THEN '0X1X'
        ///     ELSE '0X2X' -- default case for 'Derived3'
        /// </summary>
        private Node NormalizeTypeDiscriminatorValues(DiscriminatedNewEntityOp op, Node discriminator)
        {
            TypeInfo typeInfo = m_typeInfo.GetTypeInfo(op.Type);

            CaseOp normalizer = m_command.CreateCaseOp(typeInfo.RootType.TypeIdProperty.TypeUsage);
            List<Node> children = new List<Node>(op.DiscriminatorMap.TypeMap.Count * 2 - 1);
            for (int i = 0; i < op.DiscriminatorMap.TypeMap.Count; i++)
            {
                object discriminatorValue = op.DiscriminatorMap.TypeMap[i].Key;
                md.EntityType type = op.DiscriminatorMap.TypeMap[i].Value;
                TypeInfo currentTypeInfo = m_typeInfo.GetTypeInfo(md.TypeUsage.Create(type));

                Node normalizedDiscriminatorConstant = CreateTypeIdConstant(currentTypeInfo);
                // for the last type, return the 'then' value
                if (i == op.DiscriminatorMap.TypeMap.Count - 1)
                {
                    // ELSE normalizedDiscriminatorValue
                    children.Add(normalizedDiscriminatorConstant);
                }
                else
                {
                    // WHEN discriminator = discriminatorValue THEN normalizedDiscriminatorValue
                    ConstantBaseOp discriminatorValueOp = m_command.CreateConstantOp(md.Helper.GetModelTypeUsage(op.DiscriminatorMap.DiscriminatorProperty.TypeUsage),
                                                                                     discriminatorValue);
                    Node discriminatorConstant = m_command.CreateNode(discriminatorValueOp);
                    ComparisonOp discriminatorPredicateOp = m_command.CreateComparisonOp(OpType.EQ);
                    Node discriminatorPredicate = m_command.CreateNode(discriminatorPredicateOp, discriminator, discriminatorConstant);
                    children.Add(discriminatorPredicate);
                    children.Add(normalizedDiscriminatorConstant);
                }
            }

            // swap discriminator with case op normalizing the discriminator
            discriminator = m_command.CreateNode(normalizer, children);
            return discriminator;
        }

        /// <summary>
        /// NewRecordOp
        /// </summary>
        /// <param name="op">the newRecordOp</param>
        /// <param name="n">corresponding node</param>
        /// <returns>new subtree</returns>
        public override Node Visit(NewRecordOp op, Node n)
        {
            return FlattenConstructor(op, n);
        }

        /// <summary>
        /// Build out an expression corresponding to the entitysetid 
        /// </summary>
        /// <param name="entitySetidProperty">the property corresponding to the entitysetid</param>
        /// <param name="op">the *NewEntity op</param>
        /// <returns></returns>
        private Node GetEntitySetIdExpr(md.EdmProperty entitySetIdProperty, NewEntityBaseOp op)
        {
            Node entitySetIdNode;
            md.EntitySet entitySet = op.EntitySet as md.EntitySet;
            if (entitySet != null)
            {
                int entitySetId = m_typeInfo.GetEntitySetId(entitySet);
                InternalConstantOp entitySetIdOp = m_command.CreateInternalConstantOp(md.Helper.GetModelTypeUsage(entitySetIdProperty), entitySetId);
                entitySetIdNode = m_command.CreateNode(entitySetIdOp);
            }
            else
            {
                //
                // Not in a view context; simply assume a null entityset
                //
                entitySetIdNode = CreateNullConstantNode(md.Helper.GetModelTypeUsage(entitySetIdProperty));
            }

            return entitySetIdNode;
        }

        /// <summary>
        /// Flattens out a constructor into a "flat" record constructor. 
        /// The "flat" record type is looked up for the current constructor's type,
        /// and each property is filled out from the current constructor's fields
        /// </summary>
        /// <param name="op">The NewRecordOp/NewInstanceOp</param>
        /// <param name="n">The current subtree</param>
        /// <returns>the new subtree</returns>
        private Node FlattenConstructor(ScalarOp op, Node n)
        {
            PlanCompiler.Assert(op.OpType == OpType.NewInstance || op.OpType == OpType.NewRecord || op.OpType == OpType.DiscriminatedNewEntity || op.OpType == OpType.NewEntity,
                "unexpected op: " + op.OpType + "?");

            // First visit all my children
            VisitChildren(n);

            // Find the new type corresponding to the type
            TypeInfo typeInfo = m_typeInfo.GetTypeInfo(op.Type);
            md.RowType flatType = typeInfo.FlattenedType;
            NewEntityBaseOp newEntityOp = op as NewEntityBaseOp;

            // Identify the fields
            IEnumerable opFields = null;
            DiscriminatedNewEntityOp discriminatedNewInstanceOp = null;
            if (op.OpType == OpType.NewRecord)
            {
                // Get only those fields that I have values for 
                opFields = ((NewRecordOp)op).Properties;
            }
            else if (op.OpType == OpType.DiscriminatedNewEntity)
            {
                // Get all properties projected by the discriminated new instance op
                discriminatedNewInstanceOp = (DiscriminatedNewEntityOp)op;
                opFields = discriminatedNewInstanceOp.DiscriminatorMap.Properties;
            }
            else
            {
                // Children align with structural members of type for a standard NewInstanceOp
                opFields = TypeHelpers.GetAllStructuralMembers(op.Type);
            }

            // Next, walk through each of my field, and flatten out any field
            // that is structured.
            List<md.EdmProperty> newFields = new List<md.EdmProperty>();
            List<Node> newFieldValues = new List<Node>();

            //
            // NOTE: we expect the type id property and the entityset id properties
            //       to be at the start of the properties collection.
            //
            // Add a typeid property if we need one
            //
            if (typeInfo.HasTypeIdProperty)
            {
                newFields.Add(typeInfo.TypeIdProperty);
                if (null == discriminatedNewInstanceOp)
                {
                    newFieldValues.Add(CreateTypeIdConstant(typeInfo));
                }
                else
                {
                    // first child in DiscriminatedNewInstanceOp is discriminator/typeid
                    Node discriminator = n.Children[0];

                    if (null == typeInfo.RootType.DiscriminatorMap)
                    {
                        // if there are multiple sets (or free-floating constructors) for this type
                        // hierarchy, normalize the discriminator value to expose the standard
                        // '0X' style values
                        discriminator = NormalizeTypeDiscriminatorValues(discriminatedNewInstanceOp, discriminator);
                    }

                    newFieldValues.Add(discriminator);
                }
            }

            //
            // Add an entitysetid property if we need one
            //
            if (typeInfo.HasEntitySetIdProperty)
            {
                newFields.Add(typeInfo.EntitySetIdProperty);

                PlanCompiler.Assert(newEntityOp != null, "unexpected optype:" + op.OpType);
                Node entitySetIdNode = GetEntitySetIdExpr(typeInfo.EntitySetIdProperty, newEntityOp);

                // Get the entity-set-id of the "current" entityset
                newFieldValues.Add(entitySetIdNode);
            }

            // Add a nullability property if we need one
            if (typeInfo.HasNullSentinelProperty)
            {
                newFields.Add(typeInfo.NullSentinelProperty);
                newFieldValues.Add(CreateNullSentinelConstant());
            }

            //
            // first child of discriminatedNewInstanceOp is the typeId; otherwise, the first child is the first property
            //
            int childrenIndex = null == discriminatedNewInstanceOp ? 0 : 1;

            foreach (md.EdmMember opField in opFields)
            {
                Node fieldValue = n.Children[childrenIndex];
                if (TypeUtils.IsStructuredType(md.Helper.GetModelTypeUsage(opField)))
                {
                    // Flatten out nested type
                    md.RowType nestedFlatType = m_typeInfo.GetTypeInfo(md.Helper.GetModelTypeUsage(opField)).FlattenedType;

                    // Find offset of opField in top-level flat type
                    int nestedPropertyOffset = typeInfo.RootType.GetNestedStructureOffset(new SimplePropertyRef(opField));

                    foreach (md.EdmProperty nestedProperty in nestedFlatType.Properties)
                    {
                        // Try to build up an accessor for this property from the input
                        Node nestedPropertyValue = BuildAccessor(fieldValue, nestedProperty);

                        if (null != nestedPropertyValue)
                        {
                            newFields.Add(flatType.Properties[nestedPropertyOffset]);
                            newFieldValues.Add(nestedPropertyValue);
                        }

                        nestedPropertyOffset++;
                    }
                }
                else
                {
                    PropertyRef propRef = new SimplePropertyRef(opField);
                    md.EdmProperty outputTypeProp = typeInfo.GetNewProperty(propRef);

                    newFields.Add(outputTypeProp);

                    newFieldValues.Add(fieldValue);
                }

                childrenIndex++;
            }

            //
            // We've now handled all the regular properties. Now, walk through all the rel properties - 
            // obviously, this only applies for the *NewEntityOps
            //
            if (newEntityOp != null)
            {
                foreach (RelProperty relProp in newEntityOp.RelationshipProperties)
                {
                    Node fieldValue = n.Children[childrenIndex];
                    md.RowType nestedFlatType = m_typeInfo.GetTypeInfo(relProp.ToEnd.TypeUsage).FlattenedType;

                    // Find offset of opField in top-level flat type
                    int nestedPropertyOffset = typeInfo.RootType.GetNestedStructureOffset(new RelPropertyRef(relProp));

                    foreach (md.EdmProperty nestedProperty in nestedFlatType.Properties)
                    {
                        // Try to build up an accessor for this property from the input
                        Node nestedPropertyValue = BuildAccessor(fieldValue, nestedProperty);

                        if (null != nestedPropertyValue)
                        {
                            newFields.Add(flatType.Properties[nestedPropertyOffset]);
                            newFieldValues.Add(nestedPropertyValue);
                        }

                        nestedPropertyOffset++;
                    }
                    childrenIndex++;
                }
            }

            //
            // So, now we have the list of all fields that should make up the 
            // flat type.  Create a new node with them.
            //
            NewRecordOp newOp = m_command.CreateNewRecordOp(typeInfo.FlattenedTypeUsage, newFields);
            Node newNode = m_command.CreateNode(newOp, newFieldValues);

            return newNode;
        }

        /// <summary>
        /// NullOp
        /// 
        /// If the node represents a null of an entity type it 'flattens' it into a new record,
        /// with at most one non-null value: for the typeIdProperty, if one is needed.
        /// If the node represents an null of a non-entity type, no special work is done.
        /// </summary>
        /// <param name="op">The NullOp</param>
        /// <param name="n">The current subtree</param>
        /// <returns>the new subtree</returns>
        public override Node Visit(NullOp op, Node n)
        {
            if (!TypeUtils.IsStructuredType(op.Type))
            {
                if(md.TypeSemantics.IsEnumerationType(op.Type))
                {
                    op.Type = TypeHelpers.CreateEnumUnderlyingTypeUsage(op.Type);
                }
                else if (md.TypeSemantics.IsStrongSpatialType(op.Type))
                {
                    op.Type = TypeHelpers.CreateSpatialUnionTypeUsage(op.Type);
                }

                return n;
            }

            // Find the new type corresponding to the type
            TypeInfo typeInfo = m_typeInfo.GetTypeInfo(op.Type);

            List<md.EdmProperty> newFields = new List<md.EdmProperty>();
            List<Node> newFieldValues = new List<Node>();

            // Add a typeid property if we need one
            if (typeInfo.HasTypeIdProperty)
            {
                newFields.Add(typeInfo.TypeIdProperty);
                var typeIdType = md.Helper.GetModelTypeUsage(typeInfo.TypeIdProperty);
                newFieldValues.Add(CreateNullConstantNode(typeIdType));
            }

            NewRecordOp newRecordOp = new NewRecordOp(typeInfo.FlattenedTypeUsage, newFields);
            return m_command.CreateNode(newRecordOp, newFieldValues);
        }

        #endregion

        #region type comparison ops

        /// <summary>
        /// IsOf
        /// 
        /// Convert an IsOf operator into a typeid comparison:
        /// 
        ///     IsOfOnly(e, T) => e.TypeId == TypeIdValue(T)
        ///     IsOf(e, T)     => e.TypeId like TypeIdValue(T)% escape null
        /// 
        /// </summary>
        /// <param name="op">The IsOfOp to handle</param>
        /// <param name="n">current isof subtree</param>
        /// <returns>new subtree</returns>
        public override Node Visit(IsOfOp op, Node n)
        {
            // First visit all my children
            VisitChildren(n);

            if (!TypeUtils.IsStructuredType(op.IsOfType))
            {
                return n;
            }
            TypeInfo typeInfo = m_typeInfo.GetTypeInfo(op.IsOfType);
            Node newNode = CreateTypeComparisonOp(n.Child0, typeInfo, op.IsOfOnly);
            return newNode;
        }

        /// <summary>
        /// TreatOp
        /// 
        ///     TreatOp(e, T) => case when e.TypeId like TypeIdValue(T) then T else null end
        /// </summary>
        /// <param name="op">the TreatOp</param>
        /// <param name="n">the node</param>
        /// <returns>new subtree</returns>
        public override Node Visit(TreatOp op, Node n)
        {
            // First visit all my children
            VisitChildren(n);

            //
            // filter out useless treat operations
            // Treat(subtype-instance as superType)
            //
            ScalarOp arg = (ScalarOp)n.Child0.Op;
            if (op.IsFakeTreat ||
                md.TypeSemantics.IsStructurallyEqual(arg.Type, op.Type) ||
                md.TypeSemantics.IsSubTypeOf(arg.Type, op.Type))
            {
                return n.Child0;
            }

            // When we support UDTs
            if (!TypeUtils.IsStructuredType(op.Type))
            {
                return n;
            }

            //
            // First, convert this into a CaseOp:
            //   case when e.TypeId like TypeIdValue then e else null end
            //
            TypeInfo typeInfo = m_typeInfo.GetTypeInfo(op.Type);
            Node likeNode = CreateTypeComparisonOp(n.Child0, typeInfo, false);
            CaseOp caseOp = m_command.CreateCaseOp(typeInfo.FlattenedTypeUsage);
            Node caseNode = m_command.CreateNode(caseOp, likeNode, n.Child0, CreateNullConstantNode(caseOp.Type));

            //
            // Now "flatten" out this Op into a constructor. But only get the
            // desired properties
            //
            PropertyRefList desiredProperties = m_nodePropertyMap[n];
            Node flattenedCaseNode = FlattenCaseOp(caseOp, caseNode, typeInfo, desiredProperties);
            return flattenedCaseNode;
        }

        /// <summary>
        /// Create a typeid-comparison operator - more specifically, create an 
        /// operator that compares a typeid value with the typeid property of an 
        /// input structured type.
        /// The comparison may be "exact" - in which case we're looking for the exact
        /// type; otherwise, we're looking for any possible subtypes. 
        /// The "exact" variant is used by the IsOfOp (only); the other variant is
        /// used by IsOfOp and TreatOp
        /// </summary>
        /// <param name="input">The input structured type expression</param>
        /// <param name="typeInfo">Augmented type information for the type</param>
        /// <param name="isExact">Exact comparison?</param>
        /// <returns>New comparison expression</returns>
        private Node CreateTypeComparisonOp(Node input, TypeInfo typeInfo, bool isExact)
        {
            Node typeIdProperty = BuildTypeIdAccessor(input, typeInfo);
            Node newNode = null;

            if (isExact)
            {
                newNode = CreateTypeEqualsOp(typeInfo, typeIdProperty);
            }
            else
            {
                if (typeInfo.RootType.DiscriminatorMap != null)
                {
                    // where there are explicit discriminator values, LIKE '0X%' pattern does not work...
                    newNode = CreateDisjunctiveTypeComparisonOp(typeInfo, typeIdProperty);
                }
                else
                {
                    Node typeIdConstantNode = CreateTypeIdConstantForPrefixMatch(typeInfo);
                    LikeOp likeOp = m_command.CreateLikeOp();
                    newNode = m_command.CreateNode(likeOp, typeIdProperty, typeIdConstantNode, CreateNullConstantNode(DefaultTypeIdType));
                }
            }
            return newNode;
        }

        /// <summary>
        /// Create a filter matching all types in the given hierarchy (typeIdProperty IN typeInfo.Hierarchy) e.g.:
        /// 
        ///     typeIdProperty = 'Base' OR typeIdProperty = 'Derived1' ...
        ///     
        /// This is called only for types using DiscriminatorMap (explicit discriminator values)
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <param name="typeIdProperty"></param>
        /// <returns>type hierarchy check</returns>
        private Node CreateDisjunctiveTypeComparisonOp(TypeInfo typeInfo, Node typeIdProperty)
        {
            PlanCompiler.Assert(typeInfo.RootType.DiscriminatorMap != null, "should be used only for DiscriminatorMap type checks");
            // collect all non-abstract types in the given hierarchy
            IEnumerable<TypeInfo> types = typeInfo.GetTypeHierarchy().Where(t => !t.Type.EdmType.Abstract);

            // generate a disjunction
            Node current = null;
            foreach (TypeInfo type in types)
            {
                Node typeComparisonNode = CreateTypeEqualsOp(type, typeIdProperty);
                if (null == current)
                {
                    current = typeComparisonNode;
                }
                else
                {
                    current = m_command.CreateNode(m_command.CreateConditionalOp(OpType.Or), current, typeComparisonNode);
                }
            }
            if (null == current)
            {
                // only abstract types in this hierarchy... no values possible
                current = m_command.CreateNode(m_command.CreateFalseOp());
            }
            return current;
        }

        /// <summary>
        /// Generates a node of the form typeIdProperty = typeInfo.TypeId
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <param name="typeIdProperty"></param>
        /// <returns>type equality check</returns>
        private Node CreateTypeEqualsOp(TypeInfo typeInfo, Node typeIdProperty)
        {
            Node typeIdConstantNode = CreateTypeIdConstant(typeInfo);
            ComparisonOp eqCompOp = m_command.CreateComparisonOp(OpType.EQ);
            Node result = m_command.CreateNode(eqCompOp, typeIdProperty, typeIdConstantNode);
            return result;
        }

        #endregion

        #endregion

        #endregion
    }
}
