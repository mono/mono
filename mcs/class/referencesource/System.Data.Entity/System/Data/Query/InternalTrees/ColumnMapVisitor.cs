//---------------------------------------------------------------------
// <copyright file="ColumnMapVisitorWithResults.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Query.InternalTrees
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Common;
    using System.Data.Common.CommandTrees;
    using System.Data.Metadata.Edm;
    using System.Data.Query.PlanCompiler;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    /// <summary>
    /// Basic Visitor Design Pattern support for ColumnMap hierarchy;
    /// 
    /// This visitor class will walk the entire hierarchy, but does not
    /// return results; it's useful for operations such as printing and
    /// searching.
    /// </summary>
    /// <typeparam name="TArgType"></typeparam>
    internal abstract class ColumnMapVisitor<TArgType>
    {
        #region visitor helpers

        /// <summary>
        /// Common List(ColumnMap) code
        /// </summary>
        /// <param name="columnMaps"></param>
        /// <param name="arg"></param>
        protected void VisitList<TListType>(TListType[] columnMaps, TArgType arg)
                   where TListType : ColumnMap
        {
            foreach (TListType columnMap in columnMaps)
            {
                columnMap.Accept(this, arg);
            }
        }

        #endregion

        #region EntityIdentity handling

        protected void VisitEntityIdentity(EntityIdentity entityIdentity, TArgType arg)
        {
            DiscriminatedEntityIdentity dei = entityIdentity as DiscriminatedEntityIdentity;
            if (null != dei)
            {
                VisitEntityIdentity(dei, arg);
            }
            else
            {
                VisitEntityIdentity((SimpleEntityIdentity)entityIdentity, arg);
            }
        }

        protected virtual void VisitEntityIdentity(DiscriminatedEntityIdentity entityIdentity, TArgType arg)
        {
            entityIdentity.EntitySetColumnMap.Accept(this, arg);
            foreach (SimpleColumnMap columnMap in entityIdentity.Keys)
            {
                columnMap.Accept(this, arg);
            }
        }

        protected virtual void VisitEntityIdentity(SimpleEntityIdentity entityIdentity, TArgType arg)
        {
            foreach (SimpleColumnMap columnMap in entityIdentity.Keys)
            {
                columnMap.Accept(this, arg);
            }
        }

        #endregion

        #region Visitor methods

        internal virtual void Visit(ComplexTypeColumnMap columnMap, TArgType arg)
        {
            ColumnMap nullSentinel = columnMap.NullSentinel;
            if (null != nullSentinel) 
            {
                nullSentinel.Accept(this, arg);
            }
            foreach (ColumnMap p in columnMap.Properties)
            {
                p.Accept(this, arg);
            }
        }

        internal virtual void Visit(DiscriminatedCollectionColumnMap columnMap, TArgType arg)
        {
            columnMap.Discriminator.Accept(this, arg);
            foreach (SimpleColumnMap fk in columnMap.ForeignKeys)
            {
                fk.Accept(this, arg);
            }
            foreach (SimpleColumnMap k in columnMap.Keys)
            {
                k.Accept(this, arg);
            }
            columnMap.Element.Accept(this, arg);
        }

        internal virtual void Visit(EntityColumnMap columnMap, TArgType arg)
        {
            VisitEntityIdentity(columnMap.EntityIdentity, arg);
            foreach (ColumnMap p in columnMap.Properties)
            {
                p.Accept(this, arg);
            }
        }

        internal virtual void Visit(SimplePolymorphicColumnMap columnMap, TArgType arg)
        {
            columnMap.TypeDiscriminator.Accept(this, arg);
            foreach (ColumnMap cm in columnMap.TypeChoices.Values)
            {
                cm.Accept(this, arg);
            }
            foreach (ColumnMap p in columnMap.Properties)
            {
                p.Accept(this, arg);
            }
        }

        internal virtual void Visit(MultipleDiscriminatorPolymorphicColumnMap columnMap, TArgType arg)
        {
            foreach (var typeDiscriminator in columnMap.TypeDiscriminators)
            {
                typeDiscriminator.Accept(this, arg);
            }
            foreach (var typeColumnMap in columnMap.TypeChoices.Values)
            {
                typeColumnMap.Accept(this, arg);
            }
            foreach (var property in columnMap.Properties)
            {
                property.Accept(this, arg);
            }
        }

        internal virtual void Visit(RecordColumnMap columnMap, TArgType arg)
        {
            ColumnMap nullSentinel = columnMap.NullSentinel;
            if (null != nullSentinel) 
            {
                nullSentinel.Accept(this, arg);
            }
            foreach (ColumnMap p in columnMap.Properties)
            {
                p.Accept(this, arg);
            }
        }

        internal virtual void Visit(RefColumnMap columnMap, TArgType arg)
        {
            VisitEntityIdentity(columnMap.EntityIdentity, arg);
        }

        internal virtual void Visit(ScalarColumnMap columnMap, TArgType arg)
        {
        }

        internal virtual void Visit(SimpleCollectionColumnMap columnMap, TArgType arg)
        {
            foreach (SimpleColumnMap fk in columnMap.ForeignKeys)
            {
                fk.Accept(this, arg);
            }
            foreach (SimpleColumnMap k in columnMap.Keys)
            {
                k.Accept(this, arg);
            }
            columnMap.Element.Accept(this, arg);
        }

        internal virtual void Visit(VarRefColumnMap columnMap, TArgType arg)
        {
        }

        #endregion
    }

    /// <summary>
    /// Basic Visitor Design Pattern support for ColumnMap hierarchy; 
    /// 
    /// This visitor class allows you to return results; it's useful for operations
    /// that copy or manipulate the hierarchy.
    /// </summary>
    /// <typeparam name="TArgType"></typeparam>
    /// <typeparam name="TResultType"></typeparam>
    internal abstract class ColumnMapVisitorWithResults<TResultType, TArgType>
    {

        #region EntityIdentity handling

        protected EntityIdentity VisitEntityIdentity(EntityIdentity entityIdentity, TArgType arg)
        {
            DiscriminatedEntityIdentity dei = entityIdentity as DiscriminatedEntityIdentity;
            if (null != dei)
            {
                return VisitEntityIdentity(dei, arg);
            }
            else
            {
                return VisitEntityIdentity((SimpleEntityIdentity)entityIdentity, arg);
            }
        }

        protected virtual EntityIdentity VisitEntityIdentity(DiscriminatedEntityIdentity entityIdentity, TArgType arg)
        {
            return entityIdentity;
        }

        protected virtual EntityIdentity VisitEntityIdentity(SimpleEntityIdentity entityIdentity, TArgType arg)
        {
            return entityIdentity;
        }

        #endregion

        #region Visitor methods

        internal abstract TResultType Visit(ComplexTypeColumnMap columnMap, TArgType arg);

        internal abstract TResultType Visit(DiscriminatedCollectionColumnMap columnMap, TArgType arg);

        internal abstract TResultType Visit(EntityColumnMap columnMap, TArgType arg);

        internal abstract TResultType Visit(SimplePolymorphicColumnMap columnMap, TArgType arg);

        internal abstract TResultType Visit(RecordColumnMap columnMap, TArgType arg);

        internal abstract TResultType Visit(RefColumnMap columnMap, TArgType arg);

        internal abstract TResultType Visit(ScalarColumnMap columnMap, TArgType arg);

        internal abstract TResultType Visit(SimpleCollectionColumnMap columnMap, TArgType arg);

        internal abstract TResultType Visit(VarRefColumnMap columnMap, TArgType arg);

        internal abstract TResultType Visit(MultipleDiscriminatorPolymorphicColumnMap columnMap, TArgType arg);

        #endregion
    }

}
