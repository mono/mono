//---------------------------------------------------------------------
// <copyright file="VarInfo.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
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

using System.Data.Common;
using md = System.Data.Metadata.Edm;
using System.Data.Query.InternalTrees;
using System.Data.Query.PlanCompiler;

namespace System.Data.Query.PlanCompiler {

    /// <summary>
    /// Kind of VarInfo
    /// </summary>
    internal enum VarInfoKind
    {   
        /// <summary>
        /// The VarInfo is of <see cref="PrimitiveTypeVarInfo"/> type.
        /// </summary>
        PrimitiveTypeVarInfo,

        /// <summary>
        /// The VarInfo is of <see cref="StructuredVarInfo"/> type.
        /// </summary>
        StructuredTypeVarInfo,

        /// <summary>
        /// The VarInfo is of <see cref="CollectionVarInfo"/> type.
        /// </summary>
        CollectionVarInfo 
    }

    /// <summary>
    /// Information about a Var and its replacement
    /// </summary>
    internal abstract class VarInfo {

        /// <summary>
        /// Gets <see cref="VarInfoKind"/> for this <see cref="VarInfo"/>.
        /// </summary>
        internal abstract VarInfoKind Kind { get; }

        /// <summary>
        /// Get the list of new Vars introduced by this VarInfo
        /// </summary>
        internal virtual List<Var> NewVars { get { return null; } }
    }

    /// <summary>
    /// Represents information about a collection typed Var. 
    /// Each such Var is replaced by a Var with a new "mapped" type - the "mapped" type
    /// is simply a collection type where the element type has been "mapped"
    /// </summary>
    internal class CollectionVarInfo : VarInfo {
        private List<Var> m_newVars; // always a singleton list

        /// <summary>
        /// Create a CollectionVarInfo
        /// </summary>
        /// <param name="newVar"></param>
        internal CollectionVarInfo(Var newVar) {
            m_newVars = new List<Var>();
            m_newVars.Add(newVar);
        }

        /// <summary>
        /// Get the newVar
        /// </summary>
        internal Var NewVar { get { return m_newVars[0]; } }

        /// <summary>
        /// Gets <see cref="VarInfoKind"/> for this <see cref="VarInfo"/>. Always <see cref="VarInfoKind.CollectionVarInfo"/>.
        /// </summary>
        internal override VarInfoKind Kind { get { return VarInfoKind.CollectionVarInfo; } }

        /// <summary>
        /// Get the list of all NewVars - just one really
        /// </summary>
        internal override List<Var> NewVars { get { return m_newVars; } }
    }

    /// <summary>
    /// The StructuredVarInfo class contains information about a structured type Var 
    /// and how it can be replaced. This is targeted towards Vars of complex/record/
    /// entity/ref types, and the goal is to replace all such Vars in this module. 
    /// </summary>
    internal class StructuredVarInfo : VarInfo {

        private Dictionary<md.EdmProperty, Var> m_propertyToVarMap;
        List<Var> m_newVars;
        bool m_newVarsIncludeNullSentinelVar;
        List<md.EdmProperty> m_newProperties;
        md.RowType m_newType;
        md.TypeUsage m_newTypeUsage;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="newType">new "flat" record type corresponding to the Var's datatype</param>
        /// <param name="newVars">List of vars to replace current Var</param>
        /// <param name="newTypeProperties">List of properties in the "flat" record type</param>
        /// <param name="newVarsIncludeNullSentinelVar">Do the new vars include a var that represents a null sentinel either for this type or for any nested type</param>
        internal StructuredVarInfo(md.RowType newType, List<Var> newVars, List<md.EdmProperty> newTypeProperties, bool newVarsIncludeNullSentinelVar)
        {
            PlanCompiler.Assert(newVars.Count == newTypeProperties.Count, "count mismatch");
            // I see a few places where this is legal
            // PlanCompiler.Assert(newVars.Count > 0, "0 vars?");
            m_newVars = newVars;
            m_newProperties = newTypeProperties;
            m_newType = newType;
            m_newVarsIncludeNullSentinelVar = newVarsIncludeNullSentinelVar;
            m_newTypeUsage = md.TypeUsage.Create(newType);
        }

        /// <summary>
        /// Gets <see cref="VarInfoKind"/> for this <see cref="VarInfo"/>. Always <see cref="VarInfoKind.StructuredTypeVarInfo"/>.
        /// </summary>
        internal override VarInfoKind Kind
        {
            get { return VarInfoKind.StructuredTypeVarInfo; }
        }

        /// <summary>
        /// The NewVars property of the VarInfo is a list of the corresponding 
        /// "scalar" Vars that can be used to replace the current Var. This is 
        /// mainly intended for use by other RelOps that maintain lists of Vars
        /// - for example, the "Vars" property of ProjectOp and other similar 
        /// locations.
        /// </summary>
        internal override List<Var> NewVars { get { return m_newVars; } }

        /// <summary>
        /// The Fields property is matched 1-1 with the NewVars property, and
        /// specifies the properties of the record type corresponding to the 
        /// original VarType
        /// </summary>
        internal List<md.EdmProperty> Fields { get { return m_newProperties; } }

        /// <summary>
        /// Indicates whether any of the vars in NewVars 'derives'
        /// from a null sentinel. For example, for a type that is a Record with two
        /// nested records, if any has a null sentinel, it would be set to true. 
        /// It is used when expanding sort keys, to be able to indicate that there is a
        /// sorting operation that includes null sentinels. This indication is later 
        /// used by transformation rules. 
        /// </summary>
        internal bool NewVarsIncludeNullSentinelVar { get { return m_newVarsIncludeNullSentinelVar; } }

        /// <summary>
        /// Get the Var corresponding to a specific property
        /// </summary>
        /// <param name="p">the requested property</param>
        /// <param name="v">the corresponding Var</param>
        /// <returns>true, if the Var was found</returns>
        internal bool TryGetVar(md.EdmProperty p, out Var v) {
            if (m_propertyToVarMap == null) {
                InitPropertyToVarMap();
            }
            return m_propertyToVarMap.TryGetValue(p, out v);
        }

        /// <summary>
        /// The NewType property describes the new "flattened" record type
        /// that is a replacement for the original type of the Var
        /// </summary>
        internal md.RowType NewType { get { return m_newType; } }

        /// <summary>
        /// Returns the NewType wrapped in a TypeUsage
        /// </summary>
        internal md.TypeUsage NewTypeUsage { get { return m_newTypeUsage; } }

        /// <summary>
        /// Initialize mapping from properties to the corresponding Var
        /// </summary>
        private void InitPropertyToVarMap() {
            if (m_propertyToVarMap == null) {
                m_propertyToVarMap = new Dictionary<md.EdmProperty, Var>();
                IEnumerator<Var> newVarEnumerator = m_newVars.GetEnumerator();
                foreach (md.EdmProperty prop in m_newProperties) {
                    newVarEnumerator.MoveNext();
                    m_propertyToVarMap.Add(prop, newVarEnumerator.Current);
                }
                newVarEnumerator.Dispose();
            }
        }
    }

    /// <summary>
    /// Represents information about a primitive typed Var and how it can be replaced. 
    /// </summary>
    internal class PrimitiveTypeVarInfo : VarInfo
    {
        private List<Var> m_newVars; // always a singleton list

        /// <summary>
        /// Initializes a new instance of <see cref="PrimitiveTypeVarInfo"/> class.
        /// </summary>
        /// <param name="newVar">
        /// New <see cref="Var"/> that replaces current <see cref="Var"/>.
        /// </param>
        internal PrimitiveTypeVarInfo(Var newVar)
        {
            System.Diagnostics.Debug.Assert(newVar != null, "newVar != null");
            m_newVars = new List<Var>() { newVar };
        }

        /// <summary>
        /// Gets the newVar.
        /// </summary>
        internal Var NewVar { get { return m_newVars[0]; } }

        /// <summary>
        /// Gets <see cref="VarInfoKind"/> for this <see cref="VarInfo"/>. Always <see cref="VarInfoKind.CollectionVarInfo"/>.
        /// </summary>
        internal override VarInfoKind Kind
        {
            get { return VarInfoKind.PrimitiveTypeVarInfo; }
        }

        /// <summary>
        /// Gets the list of all NewVars. The list contains always just one element.
        /// </summary>
        internal override List<Var> NewVars { get { return m_newVars; } }
    }

    /// <summary>
    /// The VarInfo map maintains a mapping from Vars to their corresponding VarInfo
    /// It is logically a Dictionary
    /// </summary>
    internal class VarInfoMap {
        private Dictionary<Var, VarInfo> m_map;

        /// <summary>
        /// Default constructor
        /// </summary>
        internal VarInfoMap() {
            m_map = new Dictionary<Var, VarInfo>();
        }

        /// <summary>
        /// Create a new VarInfo for a structured type Var
        /// </summary>
        /// <param name="v">The structured type Var</param>
        /// <param name="newType">"Mapped" type for v</param>
        /// <param name="newVars">List of vars corresponding to v</param>
        /// <param name="newProperties">Flattened Properties </param>
        /// <param name="newVarsIncludeNullSentinelVar">Do the new vars include a var that represents a null sentinel either for this type or for any nested type</param>
        /// <returns>the VarInfo</returns>
        internal VarInfo CreateStructuredVarInfo(Var v, md.RowType newType, List<Var> newVars, List<md.EdmProperty> newProperties, bool newVarsIncludeNullSentinelVar)
        {
            VarInfo varInfo = new StructuredVarInfo(newType, newVars, newProperties, newVarsIncludeNullSentinelVar);
            m_map.Add(v, varInfo);
            return varInfo;
        }

        /// <summary>
        /// Create a new VarInfo for a structured type Var where the newVars cannot include a null sentinel
        /// </summary>
        /// <param name="v">The structured type Var</param>
        /// <param name="newType">"Mapped" type for v</param>
        /// <param name="newVars">List of vars corresponding to v</param>
        /// <param name="newProperties">Flattened Properties </param>
        internal VarInfo CreateStructuredVarInfo(Var v, md.RowType newType, List<Var> newVars, List<md.EdmProperty> newProperties)
        {
            return CreateStructuredVarInfo(v, newType, newVars, newProperties, false);
        }

        /// <summary>
        /// Create a VarInfo for a collection typed Var
        /// </summary>
        /// <param name="v">The collection-typed Var</param>
        /// <param name="newVar">the new Var</param>
        /// <returns>the VarInfo</returns>
        internal VarInfo CreateCollectionVarInfo(Var v, Var newVar) {
            VarInfo varInfo = new CollectionVarInfo(newVar);
            m_map.Add(v, varInfo);
            return varInfo;
        }

        /// <summary>
        /// Creates a var info for var variables of primitive or enum type.
        /// </summary>
        /// <param name="v">Current variable of primitive or enum type.</param>
        /// <param name="newVar">The new variable replacing <paramref name="v"/>.</param>
        /// <returns><see cref="PrimitiveTypeVarInfo"/> for <paramref name="v"/>.</returns>
        internal VarInfo CreatePrimitiveTypeVarInfo(Var v, Var newVar)
        {
            System.Diagnostics.Debug.Assert(v != null, "v != null");
            System.Diagnostics.Debug.Assert(newVar != null, "newVar != null");

            PlanCompiler.Assert(md.TypeSemantics.IsScalarType(v.Type), "The current variable should be of primitive or enum type.");
            PlanCompiler.Assert(md.TypeSemantics.IsScalarType(newVar.Type), "The new variable should be of primitive or enum type.");

            VarInfo varInfo = new PrimitiveTypeVarInfo(newVar);
            m_map.Add(v, varInfo);
            return varInfo;
        }

        /// <summary>
        /// Return the VarInfo for the specified var (if one exists, of course)
        /// </summary>
        /// <param name="v">The Var</param>
        /// <param name="varInfo">the corresponding VarInfo</param>
        /// <returns></returns>
        internal bool TryGetVarInfo(Var v, out VarInfo varInfo) {
            return m_map.TryGetValue(v, out varInfo);
        }
    }
}
