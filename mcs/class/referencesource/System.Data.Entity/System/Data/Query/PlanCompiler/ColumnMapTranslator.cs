//---------------------------------------------------------------------
// <copyright file="ColumnMapTranslator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data.Query.InternalTrees;
using System.Data.Query.PlanCompiler;
using System.Linq;
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

    /// <summary>
    /// Delegate pattern that the ColumnMapTranslator uses to find its replacement
    /// columnMaps.  Given a columnMap, return it's replacement.
    /// </summary>
    /// <param name="columnMap"></param>
    /// <returns></returns>
    internal delegate ColumnMap ColumnMapTranslatorTranslationDelegate(ColumnMap columnMap);

    /// <summary>
    /// ColumnMapTranslator visits the ColumnMap hiearchy and runs the translation delegate
    /// you specify;  There are some static methods to perform common translations, but you
    /// can bring your own translation if you desire.
    /// 
    /// This visitor only creates new ColumnMap objects when necessary; it attempts to 
    /// replace-in-place, except when that is not possible because the field is not
    /// writable.
    /// 
    /// NOTE: over time, we should be able to modify the ColumnMaps to have more writable
    ///       fields;
    /// </summary>
    internal class ColumnMapTranslator : ColumnMapVisitorWithResults<ColumnMap, ColumnMapTranslatorTranslationDelegate>
    {

        #region Constructors

        /// <summary>
        /// Singleton instance for the "public" methods to use;
        /// </summary>
        static private ColumnMapTranslator Instance = new ColumnMapTranslator();

        /// <summary>
        /// Constructor; no one should use this.
        /// </summary>
        private ColumnMapTranslator()
        {
        }

        #endregion

        #region Visitor Helpers

        /// <summary>
        /// Returns the var to use in the copy, either the original or the
        /// replacement.  Note that we will follow the chain of replacements, in
        /// case the replacement was also replaced.
        /// </summary>
        /// <param name="originalVar"></param>
        /// <param name="replacementVarMap"></param>
        /// <returns></returns>
        private static Var GetReplacementVar(Var originalVar, Dictionary<Var, Var> replacementVarMap)
        {
            // SQLBUDT #478509: Follow the chain of mapped vars, don't
            //                  just stop at the first one
            Var replacementVar = originalVar;

            while (replacementVarMap.TryGetValue(replacementVar, out originalVar))
            {
                if (originalVar == replacementVar)
                {
                    break;
                }
                replacementVar = originalVar;
            }
            return replacementVar;
        }

        #endregion

        #region "Public" surface area

        /// <summary>
        /// Bring-Your-Own-Replacement-Delegate method.
        /// </summary>
        /// <param name="columnMap"></param>
        /// <param name="translationDelegate"></param>
        /// <returns></returns>
        internal static ColumnMap Translate(ColumnMap columnMap, ColumnMapTranslatorTranslationDelegate translationDelegate)
        {
            return columnMap.Accept(ColumnMapTranslator.Instance, translationDelegate);
        }
        
        /// <summary>
        /// Replace VarRefColumnMaps with the specified ColumnMap replacement
        /// </summary>
        /// <param name="columnMapToTranslate"></param>
        /// <param name="varToColumnMap"></param>
        /// <returns></returns>
        internal static ColumnMap Translate(ColumnMap columnMapToTranslate, Dictionary<Var, ColumnMap> varToColumnMap)
        {
            ColumnMap result = Translate(columnMapToTranslate,
                                            delegate(ColumnMap columnMap)
                                            {
                                                VarRefColumnMap varRefColumnMap = columnMap as VarRefColumnMap;
                                                if (null != varRefColumnMap)
                                                {
                                                    if (varToColumnMap.TryGetValue(varRefColumnMap.Var, out columnMap))
                                                    {
                                                        // perform fixups; only allow name changes when the replacement isn't
                                                        // already named (and the original is named...)
                                                        if (!columnMap.IsNamed && varRefColumnMap.IsNamed)
                                                        {
                                                            columnMap.Name = varRefColumnMap.Name;
                                                        }
                                                    }
                                                    else 
                                                    {
                                                        columnMap = varRefColumnMap;
                                                    }
                                                }
                                                return columnMap;
                                            }
                                            );
            return result;
        }

        /// <summary>
        /// Replace VarRefColumnMaps with new VarRefColumnMaps with the specified Var
        /// </summary>
        /// <param name="columnMapToTranslate"></param>
        /// <param name="varToVarMap"></param>
        /// <returns></returns>
        internal static ColumnMap Translate(ColumnMap columnMapToTranslate, Dictionary<Var, Var> varToVarMap)
        {
            ColumnMap result = Translate(columnMapToTranslate,
                                            delegate(ColumnMap columnMap)
                                            {
                                                VarRefColumnMap varRefColumnMap = columnMap as VarRefColumnMap;
                                                if (null != varRefColumnMap)
                                                {
                                                    Var replacementVar = GetReplacementVar(varRefColumnMap.Var, varToVarMap);
                                                    if (varRefColumnMap.Var != replacementVar)
                                                    {
                                                        columnMap = new VarRefColumnMap(varRefColumnMap.Type, varRefColumnMap.Name, replacementVar);
                                                    }
                                                }
                                                return columnMap;
                                            }
                                            );

            return result;
        }

        /// <summary>
        /// Replace VarRefColumnMaps with ScalarColumnMaps referring to the command and column
        /// </summary>
        /// <param name="columnMapToTranslate"></param>
        /// <param name="varToCommandColumnMap"></param>
        /// <returns></returns>
        internal static ColumnMap Translate(ColumnMap columnMapToTranslate, Dictionary<Var, KeyValuePair<int, int>> varToCommandColumnMap)
        {
            ColumnMap result = Translate(columnMapToTranslate,
                                            delegate(ColumnMap columnMap)
                                            {
                                                VarRefColumnMap varRefColumnMap = columnMap as VarRefColumnMap;
                                                if (null != varRefColumnMap)
                                                {
                                                    KeyValuePair<int, int> commandAndColumn;

                                                    if (!varToCommandColumnMap.TryGetValue(varRefColumnMap.Var, out commandAndColumn))
                                                    {
                                                        throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.UnknownVar, 1, varRefColumnMap.Var.Id); // shouldn't have gotten here without having a resolveable var
                                                    }
                                                    columnMap = new ScalarColumnMap(varRefColumnMap.Type, varRefColumnMap.Name, commandAndColumn.Key, commandAndColumn.Value);
                                                }

                                                // While we're at it, we ensure that all columnMaps are named; we wait
                                                // until this point, because we don't want to assign names until after
                                                // we've gone through the transformations; 
                                                if (!columnMap.IsNamed)
                                                {
                                                    columnMap.Name = ColumnMap.DefaultColumnName;
                                                }
                                                return columnMap;
                                            }
                                            );

            return result;
        }

        #endregion

        #region Visitor methods

        #region List handling

        /// <summary>
        /// List(ColumnMap)
        /// </summary>
        /// <typeparam name="TResultType"></typeparam>
        /// <param name="tList"></param>
        /// <param name="translationDelegate"></param>
        private void VisitList<TResultType>(TResultType[] tList, ColumnMapTranslatorTranslationDelegate translationDelegate)
                where TResultType : ColumnMap
        {
            for (int i = 0; i < tList.Length; i++)
            {
                tList[i] = (TResultType)tList[i].Accept(this, translationDelegate);
            }
        }

        #endregion

        #region EntityIdentity handling

        /// <summary>
        /// DiscriminatedEntityIdentity
        /// </summary>
        /// <param name="entityIdentity"></param>
        /// <param name="translationDelegate"></param>
        /// <returns></returns>
        protected override EntityIdentity VisitEntityIdentity(DiscriminatedEntityIdentity entityIdentity, ColumnMapTranslatorTranslationDelegate translationDelegate)
        {
            ColumnMap newEntitySetColumnMap = entityIdentity.EntitySetColumnMap.Accept(this, translationDelegate);
            VisitList(entityIdentity.Keys, translationDelegate);

            if (newEntitySetColumnMap != entityIdentity.EntitySetColumnMap)
            {
                entityIdentity = new DiscriminatedEntityIdentity((SimpleColumnMap)newEntitySetColumnMap, entityIdentity.EntitySetMap, entityIdentity.Keys);
            }
            return entityIdentity;
        }

        /// <summary>
        /// SimpleEntityIdentity
        /// </summary>
        /// <param name="entityIdentity"></param>
        /// <param name="translationDelegate"></param>
        /// <returns></returns>
        protected override EntityIdentity VisitEntityIdentity(SimpleEntityIdentity entityIdentity, ColumnMapTranslatorTranslationDelegate translationDelegate)
        {
            VisitList(entityIdentity.Keys, translationDelegate);
            return entityIdentity;
        }

        #endregion

        /// <summary>
        /// ComplexTypeColumnMap
        /// </summary>
        /// <param name="columnMap"></param>
        /// <param name="translationDelegate"></param>
        /// <returns></returns>
        internal override ColumnMap Visit(ComplexTypeColumnMap columnMap, ColumnMapTranslatorTranslationDelegate translationDelegate)
        {
            SimpleColumnMap newNullSentinel = columnMap.NullSentinel;
            if (null != newNullSentinel) 
            {
                newNullSentinel = (SimpleColumnMap)translationDelegate(newNullSentinel);
            }

            VisitList(columnMap.Properties, translationDelegate);

            if (columnMap.NullSentinel != newNullSentinel)
            {
                columnMap = new ComplexTypeColumnMap(columnMap.Type, columnMap.Name, columnMap.Properties, newNullSentinel);
            }
            return translationDelegate(columnMap);
        }

        /// <summary>
        /// DiscriminatedCollectionColumnMap
        /// </summary>
        /// <param name="columnMap"></param>
        /// <param name="translationDelegate"></param>
        /// <returns></returns>
        internal override ColumnMap Visit(DiscriminatedCollectionColumnMap columnMap, ColumnMapTranslatorTranslationDelegate translationDelegate)
        {
            ColumnMap newDiscriminator = columnMap.Discriminator.Accept(this, translationDelegate);
            VisitList(columnMap.ForeignKeys, translationDelegate);
            VisitList(columnMap.Keys, translationDelegate);
            ColumnMap newElement = columnMap.Element.Accept(this, translationDelegate);

            if (newDiscriminator != columnMap.Discriminator || newElement != columnMap.Element)
            {
                columnMap = new DiscriminatedCollectionColumnMap(columnMap.Type, columnMap.Name, newElement, columnMap.Keys, columnMap.ForeignKeys,(SimpleColumnMap)newDiscriminator, columnMap.DiscriminatorValue);
            }
            return translationDelegate(columnMap);
        }

        /// <summary>
        /// EntityColumnMap
        /// </summary>
        /// <param name="columnMap"></param>
        /// <param name="translationDelegate"></param>
        /// <returns></returns>
        internal override ColumnMap Visit(EntityColumnMap columnMap, ColumnMapTranslatorTranslationDelegate translationDelegate)
        {
            EntityIdentity newEntityIdentity = VisitEntityIdentity(columnMap.EntityIdentity, translationDelegate);
            VisitList(columnMap.Properties, translationDelegate);

            if (newEntityIdentity != columnMap.EntityIdentity)
            {
                columnMap = new EntityColumnMap(columnMap.Type, columnMap.Name, columnMap.Properties, newEntityIdentity);
            }
            return translationDelegate(columnMap);
        }

        /// <summary>
        /// SimplePolymorphicColumnMap
        /// </summary>
        /// <param name="columnMap"></param>
        /// <param name="translationDelegate"></param>
        /// <returns></returns>
        internal override ColumnMap Visit(SimplePolymorphicColumnMap columnMap, ColumnMapTranslatorTranslationDelegate translationDelegate)
        {
            ColumnMap newTypeDiscriminator = columnMap.TypeDiscriminator.Accept(this, translationDelegate);

            // NOTE: we're using Copy-On-Write logic to avoid allocation if we don't 
            //       need to change things.
            Dictionary<object, TypedColumnMap> newTypeChoices = columnMap.TypeChoices;
            foreach (KeyValuePair<object, TypedColumnMap> kv in columnMap.TypeChoices)
            {
                TypedColumnMap newTypeChoice = (TypedColumnMap)kv.Value.Accept(this, translationDelegate);

                if (newTypeChoice != kv.Value)
                {
                    if (newTypeChoices == columnMap.TypeChoices)
                    {
                        newTypeChoices = new Dictionary<object, TypedColumnMap>(columnMap.TypeChoices);
                    }
                    newTypeChoices[kv.Key] = newTypeChoice;
                }
            }
            VisitList(columnMap.Properties, translationDelegate);

            if (newTypeDiscriminator != columnMap.TypeDiscriminator || newTypeChoices != columnMap.TypeChoices) 
            {
                columnMap = new SimplePolymorphicColumnMap(columnMap.Type, columnMap.Name, columnMap.Properties, (SimpleColumnMap)newTypeDiscriminator, newTypeChoices);
            }
            return translationDelegate(columnMap);
        }

        /// <summary>
        /// MultipleDiscriminatorPolymorphicColumnMap
        /// </summary>
        internal override ColumnMap Visit(MultipleDiscriminatorPolymorphicColumnMap columnMap, ColumnMapTranslatorTranslationDelegate translationDelegate)
        {
            // At this time, we shouldn't ever see this type here; it's for SPROCS which don't use
            // the plan compiler.
            System.Data.Query.PlanCompiler.PlanCompiler.Assert(false, "unexpected MultipleDiscriminatorPolymorphicColumnMap in ColumnMapTranslator");
            return null;
        }

        /// <summary>
        /// RecordColumnMap
        /// </summary>
        /// <param name="columnMap"></param>
        /// <param name="translationDelegate"></param>
        /// <returns></returns>
        internal override ColumnMap Visit(RecordColumnMap columnMap, ColumnMapTranslatorTranslationDelegate translationDelegate)
        {
            SimpleColumnMap newNullSentinel = columnMap.NullSentinel;
            if (null != newNullSentinel) 
            {
                newNullSentinel = (SimpleColumnMap)translationDelegate(newNullSentinel);
            }

            VisitList(columnMap.Properties, translationDelegate);

            if (columnMap.NullSentinel != newNullSentinel)
            {
                columnMap = new RecordColumnMap(columnMap.Type, columnMap.Name, columnMap.Properties, newNullSentinel);
            }
            return translationDelegate(columnMap);
        }

        /// <summary>
        /// RefColumnMap
        /// </summary>
        /// <param name="columnMap"></param>
        /// <param name="translationDelegate"></param>
        /// <returns></returns>
        internal override ColumnMap Visit(RefColumnMap columnMap, ColumnMapTranslatorTranslationDelegate translationDelegate)
        {
            EntityIdentity newEntityIdentity = VisitEntityIdentity(columnMap.EntityIdentity, translationDelegate);

            if (newEntityIdentity != columnMap.EntityIdentity)
            {
                columnMap = new RefColumnMap(columnMap.Type, columnMap.Name, newEntityIdentity);
            }
            return translationDelegate(columnMap);
        }

        /// <summary>
        /// ScalarColumnMap
        /// </summary>
        /// <param name="columnMap"></param>
        /// <param name="translationDelegate"></param>
        /// <returns></returns>
        internal override ColumnMap Visit(ScalarColumnMap columnMap, ColumnMapTranslatorTranslationDelegate translationDelegate)
        {
            return translationDelegate(columnMap);
        }

        /// <summary>
        /// SimpleCollectionColumnMap
        /// </summary>
        /// <param name="columnMap"></param>
        /// <param name="translationDelegate"></param>
        /// <returns></returns>
        internal override ColumnMap Visit(SimpleCollectionColumnMap columnMap, ColumnMapTranslatorTranslationDelegate translationDelegate)
        {
            VisitList(columnMap.ForeignKeys, translationDelegate);
            VisitList(columnMap.Keys, translationDelegate);
            ColumnMap newElement = columnMap.Element.Accept(this, translationDelegate);

            if (newElement != columnMap.Element)
            {
                columnMap = new SimpleCollectionColumnMap(columnMap.Type, columnMap.Name, newElement, columnMap.Keys, columnMap.ForeignKeys);
            }
            return translationDelegate(columnMap);
        }

        /// <summary>
        /// VarRefColumnMap
        /// </summary>
        /// <param name="columnMap"></param>
        /// <param name="translationDelegate"></param>
        /// <returns></returns>
        internal override ColumnMap Visit(VarRefColumnMap columnMap, ColumnMapTranslatorTranslationDelegate translationDelegate)
        {
            return translationDelegate(columnMap);
        }

        #endregion
    }
}
