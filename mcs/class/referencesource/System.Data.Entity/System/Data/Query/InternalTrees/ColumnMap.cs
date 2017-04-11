//---------------------------------------------------------------------
// <copyright file="ColumnMap.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Text;
using System.Collections.Generic;
using md = System.Data.Metadata.Edm;
using mp = System.Data.Mapping;
using System.Globalization;
using System.Diagnostics;
using System.Data.Common.Utils;

// A ColumnMap is a data structure that maps columns from the C space to
// the corresponding columns from one or more underlying readers.
//
// ColumnMaps are used by the ResultAssembly phase to assemble results in the
// desired shape (as requested by the caller) from a set of underlying
// (usually) flat readers. ColumnMaps are produced as part of the PlanCompiler
// module of the bridge, and are consumed by the Execution phase of the bridge.
//
// * Simple (scalar) columns (and UDTs) are represented by a SimpleColumnMap
// * Record type columns are represented by a RecordColumnMap
// * A nominal type instance (that supports inheritance) is usually represented
//     by a PolymorphicColumnMap - this polymorphicColumnMap contains information
//     about the type discriminator (assumed to be a simple column), and a mapping
//     from type-discriminator value to the column map for the specific type
// * The specific type for nominal types is represented by ComplexTypeColumnMap
//     for complextype columns, and EntityColumnMap for entity type columns.
//     EntityColumnMaps additionally have an EntityIdentity that describes
//     the entity identity. The entity identity is logically just a set of keys
//     (and the column maps), plus a column map that helps to identify the
//     the appropriate entity set for the entity instance
// * Refs are represented by a RefColumnMap. The RefColumnMap simply contains an
//   EntityIdentity
// * Collections are represented by either a SimpleCollectionColumnMap or a
//     DiscriminatedCollectionColumnMap. Both of these contain a column map for the
//     element type, and an optional list of simple columns (the keys) that help
//     demarcate the elements of a specific collection instance.
//     The DiscriminatedCollectionColumnMap is used in scenarios when the containing
//     row has multiple collections, and the different collection properties must be
//     differentiated. This differentiation is achieved via a Discriminator column
//     (a simple column), and a Discriminator value. The value of the Discriminator
//     column is read and compared with the DiscriminatorValue stored in this map
//     to determine if we're dealing with the current collection.
//
// NOTE:
//  * Key columns are assumed to be SimpleColumns. There may be more than one key
//      column (applies to EntityColumnMap and *CollectionColumnMap)
//  * TypeDiscriminator and Discriminator columns are also considered to be
//      SimpleColumns. There are singleton columns.
//
// It is the responsibility of the PlanCompiler phase to produce the right column
// maps.
//
// The result of a query is always assumed to be a collection. The ColumnMap that we
// return as part of plan compilation refers to the element type of this collection
// - the element type is usually a structured type, but may also be a simple type
//   or another collection type. How does the DbRecord framework handle these cases?
//
//
namespace System.Data.Query.InternalTrees
{
    /// <summary>
    /// Represents a column
    /// </summary>
    internal abstract class ColumnMap
    {
        private md.TypeUsage m_type; // column datatype
        private string m_name; // name of the column

        /// <summary>
        /// Default Column Name; should not be set until CodeGen once we're done 
        /// with all our transformations that might give us a good name, but put 
        /// here for ease of finding it.
        /// </summary>
        internal const string DefaultColumnName = "Value";

        /// <summary>
        /// Simple constructor - just needs the name and type of the column
        /// </summary>
        /// <param name="type">column type</param>
        /// <param name="name">column name</param>
        internal ColumnMap(md.TypeUsage type, string name)
        {
            Debug.Assert(type != null, "Unspecified type");
            m_type = type; 
            m_name = name;
        }

        /// <summary>
        /// Get the column's datatype
        /// </summary>
        internal md.TypeUsage Type { get { return m_type; } }

        /// <summary>
        /// Get the column name
        /// </summary>
        internal string Name 
        {
            get { return m_name; }
            set 
            {
                Debug.Assert(!String.IsNullOrEmpty(value), "invalid name?");
                m_name = value; 
            } 
        }

        /// <summary>
        /// Returns whether the column already has a name;
        /// </summary>
        internal bool IsNamed 
        {
            get { return m_name != null; }
        }

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TArgType"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        [DebuggerNonUserCode]
        internal abstract void Accept<TArgType>(ColumnMapVisitor<TArgType> visitor, TArgType arg);

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TResultType"></typeparam>
        /// <typeparam name="TArgType"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        [DebuggerNonUserCode]
        internal abstract TResultType Accept<TResultType, TArgType>(ColumnMapVisitorWithResults<TResultType, TArgType> visitor, TArgType arg);
    }

    /// <summary>
    /// Base class for simple column maps; can be either a VarRefColumnMap or 
    /// ScalarColumnMap; the former is used pretty much throughout the PlanCompiler,
    /// while the latter will only be used once we generate the final Plan.
    /// </summary>
    internal abstract class SimpleColumnMap : ColumnMap
    { 
        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="type">datatype for this column</param>
        /// <param name="name">column name</param>
        internal SimpleColumnMap(md.TypeUsage type, string name)
            : base(type, name)
        {
        }
    }

    /// <summary>
    /// Column map for a scalar column - maps 1-1 with a column from a 
    /// row of the underlying reader
    /// </summary>
    internal class ScalarColumnMap : SimpleColumnMap
    {
        private int m_commandId;
        private int m_columnPos;

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="type">datatype for this column</param>
        /// <param name="name">column name</param>
        /// <param name="commandId">Underlying command to locate this column</param>
        /// <param name="columnPos">Position in underlying reader</param>
        internal ScalarColumnMap(md.TypeUsage type, string name, int commandId, int columnPos)
            : base(type, name)
        {
            Debug.Assert(commandId >= 0, "invalid command id");
            Debug.Assert(columnPos >= 0, "invalid column position");
            m_commandId = commandId;
            m_columnPos = columnPos;
        }

        /// <summary>
        /// The command (reader, really) to get this column value from
        /// </summary>
        internal int CommandId { get { return m_commandId; } }
        /// <summary>
        /// Column position within the reader of the command
        /// </summary>
        internal int ColumnPos { get { return m_columnPos; } }

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TArgType"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="arg"></param>
        [DebuggerNonUserCode]
        internal override void Accept<TArgType>(ColumnMapVisitor<TArgType> visitor, TArgType arg)
        {
            visitor.Visit(this, arg);
        }

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TResultType"></typeparam>
        /// <typeparam name="TArgType"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="arg"></param>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType, TArgType>(ColumnMapVisitorWithResults<TResultType, TArgType> visitor, TArgType arg)
        {
            return visitor.Visit(this, arg);
        }

        /// <summary>
        /// Debugging support
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "S({0},{1})", this.CommandId, this.ColumnPos);
        }
    }

    #region Structured Columns
    /// <summary>
    /// Represents a column map for a structured column
    /// </summary>
    internal abstract class StructuredColumnMap : ColumnMap
    {
        private readonly ColumnMap[] m_properties;

        /// <summary>
        /// Structured columnmap constructor
        /// </summary>
        /// <param name="type">datatype for this column</param>
        /// <param name="name">column name</param>
        /// <param name="properties">list of properties</param>
        internal StructuredColumnMap(md.TypeUsage type, string name, ColumnMap[] properties)
            : base(type, name)
        {
            Debug.Assert(properties != null, "No properties (gasp!) for a structured type");
            m_properties = properties;
        }

        /// <summary>
        /// Get the null sentinel column, if any.  Virtual so only derived column map
        /// types that can have NullSentinel have to provide storage, etc.
        /// </summary>
        virtual internal SimpleColumnMap NullSentinel { get { return null; } }

        /// <summary>
        /// Get the list of properties that constitute this structured type
        /// </summary>
        internal ColumnMap[] Properties { get { return m_properties; } }

        /// <summary>
        /// Debugging support
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string separator = String.Empty;
            sb.Append("{");
            foreach (ColumnMap c in this.Properties)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}", separator, c);
                separator = ",";
            }
            sb.Append("}");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Represents a record (an untyped structured column)
    /// </summary>
    internal class RecordColumnMap : StructuredColumnMap
    {
        private SimpleColumnMap m_nullSentinel;

        /// <summary>
        /// Constructor for a record column map
        /// </summary>
        /// <param name="type">Datatype of this column</param>
        /// <param name="name">column name</param>
        /// <param name="properties">List of ColumnMaps - one for each property</param>
        internal RecordColumnMap(md.TypeUsage type, string name, ColumnMap[] properties, SimpleColumnMap nullSentinel)
            : base(type, name, properties)
        {
            m_nullSentinel = nullSentinel;
        }

        /// <summary>
        /// Get the type Nullability column
        /// </summary>
        internal override SimpleColumnMap NullSentinel { get { return m_nullSentinel; } }

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TArgType"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="arg"></param>
        [DebuggerNonUserCode]
        internal override void Accept<TArgType>(ColumnMapVisitor<TArgType> visitor, TArgType arg)
        {
            visitor.Visit(this, arg);
        }

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TResultType"></typeparam>
        /// <typeparam name="TArgType"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="arg"></param>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType, TArgType>(ColumnMapVisitorWithResults<TResultType, TArgType> visitor, TArgType arg)
        {
            return visitor.Visit(this, arg);
        }
    }

    /// <summary>
    /// Column map for a "typed" column
    /// - either an entity type or a complex type
    /// </summary>
    internal abstract class TypedColumnMap : StructuredColumnMap
    {
        /// <summary>
        /// Typed columnMap constructor
        /// </summary>
        /// <param name="type">Datatype of column</param>
        /// <param name="name">column name</param>
        /// <param name="properties">List of column maps - one for each property</param>
        internal TypedColumnMap(md.TypeUsage type, string name, ColumnMap[] properties)
            : base(type, name, properties) { }
    }

    /// <summary>
    /// Represents a polymorphic typed column - either an entity or
    /// a complex type.
    /// </summary>
    internal class SimplePolymorphicColumnMap : TypedColumnMap
    {
        private SimpleColumnMap m_typeDiscriminator;
        private Dictionary<object, TypedColumnMap> m_typedColumnMap;

        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="type">datatype of the column</param>
        /// <param name="name">column name</param>
        /// <param name="typeDiscriminator">column map for type discriminator column</param>
        /// <param name="baseTypeColumns">base list of fields common to all types</param>
        /// <param name="typeChoices">map from type discriminator value->columnMap</param>
        internal SimplePolymorphicColumnMap(md.TypeUsage type, 
            string name, 
            ColumnMap[] baseTypeColumns,
            SimpleColumnMap typeDiscriminator, 
            Dictionary<object, TypedColumnMap> typeChoices)
            : base(type, name, baseTypeColumns)
        {
            Debug.Assert(typeDiscriminator != null, "Must specify a type discriminator column");
            Debug.Assert(typeChoices != null, "No type choices for polymorphic column");
            m_typedColumnMap = typeChoices;
            m_typeDiscriminator = typeDiscriminator;
        }

        /// <summary>
        /// Get the type discriminator column
        /// </summary>
        internal SimpleColumnMap TypeDiscriminator { get { return m_typeDiscriminator; } }

        /// <summary>
        /// Get the type mapping
        /// </summary>
        internal Dictionary<object, TypedColumnMap> TypeChoices
        {
            get { return m_typedColumnMap; }
        }

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TArgType"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="arg"></param>
        [DebuggerNonUserCode]
        internal override void Accept<TArgType>(ColumnMapVisitor<TArgType> visitor, TArgType arg)
        {
            visitor.Visit(this, arg);
        }

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TResultType"></typeparam>
        /// <typeparam name="TArgType"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="arg"></param>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType, TArgType>(ColumnMapVisitorWithResults<TResultType, TArgType> visitor, TArgType arg)
        {
            return visitor.Visit(this, arg);
        }

        /// <summary>
        /// Debugging support
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string separator = String.Empty;

            sb.AppendFormat(CultureInfo.InvariantCulture, "P{{TypeId={0}, ", this.TypeDiscriminator);
            foreach (KeyValuePair<object, TypedColumnMap> kv in this.TypeChoices)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}({1},{2})", separator, kv.Key, kv.Value);
                separator = ",";
            }
            sb.Append("}");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Represents a function import column map.
    /// </summary>
    internal class MultipleDiscriminatorPolymorphicColumnMap : TypedColumnMap
    {
        private readonly SimpleColumnMap[] m_typeDiscriminators;
        private readonly Dictionary<md.EntityType, TypedColumnMap> m_typeChoices;
        private readonly Func<object[], md.EntityType> m_discriminate;

        /// <summary>
        /// Internal constructor
        /// </summary>
        internal MultipleDiscriminatorPolymorphicColumnMap(md.TypeUsage type,
            string name,
            ColumnMap[] baseTypeColumns,
            SimpleColumnMap[] typeDiscriminators,
            Dictionary<md.EntityType, TypedColumnMap> typeChoices,
            Func<object[], md.EntityType> discriminate)
            : base(type, name, baseTypeColumns)
        {
            Debug.Assert(typeDiscriminators != null, "Must specify type discriminator columns");
            Debug.Assert(typeChoices != null, "No type choices for polymorphic column");
            Debug.Assert(discriminate != null, "Must specify discriminate");

            m_typeDiscriminators = typeDiscriminators;
            m_typeChoices = typeChoices;
            m_discriminate = discriminate;
        }

        /// <summary>
        /// Get the type discriminator column
        /// </summary>
        internal SimpleColumnMap[] TypeDiscriminators { get { return m_typeDiscriminators; } }

        /// <summary>
        /// Get the type mapping
        /// </summary>
        internal Dictionary<md.EntityType, TypedColumnMap> TypeChoices
        {
            get { return m_typeChoices; }
        }

        /// <summary>
        /// Gets discriminator delegate
        /// </summary>
        internal Func<object[], md.EntityType> Discriminate
        {
            get { return m_discriminate; }
        }

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        [DebuggerNonUserCode]
        internal override void Accept<TArgType>(ColumnMapVisitor<TArgType> visitor, TArgType arg)
        {
            visitor.Visit(this, arg);
        }

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType, TArgType>(ColumnMapVisitorWithResults<TResultType, TArgType> visitor, TArgType arg)
        {
            return visitor.Visit(this, arg);
        }

        /// <summary>
        /// Debugging support
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string separator = String.Empty;

            sb.AppendFormat(CultureInfo.InvariantCulture, "P{{TypeId=<{0}>, ", StringUtil.ToCommaSeparatedString(this.TypeDiscriminators));
            foreach (var kv in this.TypeChoices)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}(<{1}>,{2})", separator, kv.Key, kv.Value);
                separator = ",";
            }
            sb.Append("}");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Represents a column map for a specific complextype
    /// </summary>
    internal class ComplexTypeColumnMap : TypedColumnMap
    {
        private SimpleColumnMap m_nullSentinel;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">column Datatype</param>
        /// <param name="name">column name</param>
        /// <param name="properties">list of properties</param>
        internal ComplexTypeColumnMap(md.TypeUsage type, string name, ColumnMap[] properties, SimpleColumnMap nullSentinel)
            : base(type, name, properties)
        {
            m_nullSentinel = nullSentinel;
        }

        /// <summary>
        /// Get the type Nullability column
        /// </summary>
        internal override SimpleColumnMap NullSentinel { get { return m_nullSentinel; } }

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TArgType"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="arg"></param>
        [DebuggerNonUserCode]
        internal override void Accept<TArgType>(ColumnMapVisitor<TArgType> visitor, TArgType arg)
        {
            visitor.Visit(this, arg);
        }

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TResultType"></typeparam>
        /// <typeparam name="TArgType"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="arg"></param>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType, TArgType>(ColumnMapVisitorWithResults<TResultType, TArgType> visitor, TArgType arg)
        {
            return visitor.Visit(this, arg);
        }

        /// <summary>
        /// Debugging support
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = String.Format(CultureInfo.InvariantCulture, "C{0}", base.ToString());
            return str;
        }

    }

    /// <summary>
    /// Represents a column map for a specific entity type
    /// </summary>
    internal class EntityColumnMap : TypedColumnMap
    {
        private EntityIdentity m_entityIdentity;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="type">column datatype</param>
        /// <param name="name">column name</param>
        /// <param name="entityIdentity">entity identity information</param>
        /// <param name="properties">list of properties</param>
        internal EntityColumnMap(md.TypeUsage type, string name, ColumnMap[] properties, EntityIdentity entityIdentity)
            : base(type, name, properties)
        {
            Debug.Assert(entityIdentity != null, "Must specify an entity identity");
            m_entityIdentity = entityIdentity;
        }

        /// <summary>
        /// Get the entity identity information
        /// </summary>
        internal EntityIdentity EntityIdentity { get { return m_entityIdentity; } }

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TArgType"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="arg"></param>
        [DebuggerNonUserCode]
        internal override void Accept<TArgType>(ColumnMapVisitor<TArgType> visitor, TArgType arg)
        {
            visitor.Visit(this, arg);
        }

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TResultType"></typeparam>
        /// <typeparam name="TArgType"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="arg"></param>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType, TArgType>(ColumnMapVisitorWithResults<TResultType, TArgType> visitor, TArgType arg)
        {
            return visitor.Visit(this, arg);
        }

        /// <summary>
        /// Debugging support
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = String.Format(CultureInfo.InvariantCulture, "E{0}", base.ToString());
            return str;
        }
    }

    /// <summary>
    /// A column map that represents a ref column.
    /// </summary>
    internal class RefColumnMap: ColumnMap
    {
        private EntityIdentity m_entityIdentity;

        /// <summary>
        /// Constructor for a ref column
        /// </summary>
        /// <param name="type">column datatype</param>
        /// <param name="name">column name</param>
        /// <param name="entityIdentity">identity information for this entity</param>
        internal RefColumnMap(md.TypeUsage type, string name,
            EntityIdentity entityIdentity)
            : base(type, name)
        {
            Debug.Assert(entityIdentity != null, "Must specify entity identity information");
            m_entityIdentity = entityIdentity;
        }

        /// <summary>
        /// Get the entity identity information for this ref
        /// </summary>
        internal EntityIdentity EntityIdentity { get { return m_entityIdentity; } }

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TArgType"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="arg"></param>
        [DebuggerNonUserCode]
        internal override void Accept<TArgType>(ColumnMapVisitor<TArgType> visitor, TArgType arg)
        {
            visitor.Visit(this, arg);
        }
        
        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TResultType"></typeparam>
        /// <typeparam name="TArgType"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="arg"></param>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType, TArgType>(ColumnMapVisitorWithResults<TResultType, TArgType> visitor, TArgType arg)
        {
            return visitor.Visit(this, arg);
        }
    }
    #endregion

    #region Collections
    /// <summary>
    /// Represents a column map for a collection column.
    /// The "element" represents the element of the collection - usually a Structured
    /// type, but occasionally a collection/simple type as well.
    /// The "ForeignKeys" property is optional (but usually necessary) to determine the 
    /// elements of the collection.
    /// </summary>
    internal abstract class CollectionColumnMap : ColumnMap
    {
        private readonly ColumnMap m_element;
        private readonly SimpleColumnMap[] m_foreignKeys;
        private readonly SimpleColumnMap[] m_keys;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">datatype of column</param>
        /// <param name="name">column name</param>
        /// <param name="elementMap">column map for collection element</param>
        /// <param name="keys">List of keys</param>
        /// <param name="foreignKeys">List of foreign keys</param>
        internal CollectionColumnMap(md.TypeUsage type, string name, ColumnMap elementMap, SimpleColumnMap[] keys, SimpleColumnMap[] foreignKeys)
            : base(type, name)
        {
            Debug.Assert(elementMap != null, "Must specify column map for element");

            m_element = elementMap;
            m_keys = keys ?? new SimpleColumnMap[0];
            m_foreignKeys = foreignKeys ?? new SimpleColumnMap[0];
        }

        /// <summary>
        /// Get the list of columns that may comprise the foreign key
        /// </summary>
        internal SimpleColumnMap[] ForeignKeys 
        { 
            get { return m_foreignKeys; } 
        }

        /// <summary>
        /// Get the list of columns that may comprise the key
        /// </summary>
        internal SimpleColumnMap[] Keys
        {
            get { return m_keys; }
        }

        /// <summary>
        /// Get the column map describing the collection element
        /// </summary>
        internal ColumnMap Element
        {
            get { return m_element; }
        }
    }

    /// <summary>
    /// Represents a "simple" collection map.
    /// </summary>
    internal class SimpleCollectionColumnMap : CollectionColumnMap
    {
        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="type">Column datatype</param>
        /// <param name="name">column name</param>
        /// <param name="elementMap">column map for the element of the collection</param>
        /// <param name="keys">list of key columns</param>
        /// <param name="foreignKeys">list of foreign key columns</param>
        internal SimpleCollectionColumnMap(md.TypeUsage type, string name,
            ColumnMap elementMap,
            SimpleColumnMap[] keys,
            SimpleColumnMap[] foreignKeys)
            : base(type, name, elementMap, keys, foreignKeys) { }

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TArgType"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="arg"></param>
        [DebuggerNonUserCode]
        internal override void Accept<TArgType>(ColumnMapVisitor<TArgType> visitor, TArgType arg)
        {
            visitor.Visit(this, arg);
        }

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TResultType"></typeparam>
        /// <typeparam name="TArgType"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="arg"></param>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType, TArgType>(ColumnMapVisitorWithResults<TResultType, TArgType> visitor, TArgType arg)
        {
            return visitor.Visit(this, arg);
        }
    }

    /// <summary>
    /// Represents a "discriminated" collection column.
    /// This represents a scenario when multiple collections are represented
    /// at the same level of the container row, and there is a need to distinguish
    /// between these collections
    /// </summary>
    internal class DiscriminatedCollectionColumnMap : CollectionColumnMap
    {
        private SimpleColumnMap m_discriminator;
        private object m_discriminatorValue;

        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="type">Column datatype</param>
        /// <param name="name">column name</param>
        /// <param name="elementMap">column map for collection element</param>
        /// <param name="keys">Keys for the collection</param>
        /// <param name="foreignKeys">Foreign keys for the collection</param>
        /// <param name="discriminator">Discriminator column map</param>
        /// <param name="discriminatorValue">Discriminator value</param>
        internal DiscriminatedCollectionColumnMap(md.TypeUsage type, string name,
            ColumnMap elementMap,
            SimpleColumnMap[] keys,
            SimpleColumnMap[] foreignKeys,
            SimpleColumnMap discriminator, 
            object discriminatorValue)
            : base(type, name, elementMap, keys, foreignKeys)
        {
            Debug.Assert(discriminator != null, "Must specify a column map for the collection discriminator");
            Debug.Assert(discriminatorValue != null, "Must specify a discriminator value");
            m_discriminator = discriminator;
            m_discriminatorValue = discriminatorValue;
        }

        /// <summary>
        /// Get the column that describes the discriminator
        /// </summary>
        internal SimpleColumnMap Discriminator { get { return m_discriminator; } }

        /// <summary>
        /// Get the discriminator value
        /// </summary>
        internal object DiscriminatorValue { get { return m_discriminatorValue; } }

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TArgType"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="arg"></param>
        [DebuggerNonUserCode]
        internal override void Accept<TArgType>(ColumnMapVisitor<TArgType> visitor, TArgType arg)
        {
            visitor.Visit(this, arg);
        }

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TResultType"></typeparam>
        /// <typeparam name="TArgType"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="arg"></param>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType, TArgType>(ColumnMapVisitorWithResults<TResultType, TArgType> visitor, TArgType arg)
        {
            return visitor.Visit(this, arg);
        }

        /// <summary>
        /// Debugging support
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = String.Format(CultureInfo.InvariantCulture, "M{{{0}}}", this.Element.ToString());
            return str;
        }
    }
    #endregion

    #region EntityIdentity
    /// <summary>
    /// Abstract base class representing entity identity. Used by both
    /// EntityColumnMap and RefColumnMap.
    /// An EntityIdentity captures two pieces of information - the list of keys
    /// that uniquely identify an entity within an entityset, and the the entityset
    /// itself.
    /// </summary>
    internal abstract class EntityIdentity
    {
        private readonly SimpleColumnMap[] m_keys; // list of keys

        /// <summary>
        /// Simple constructor - gets a list of key columns
        /// </summary>
        /// <param name="keyColumns"></param>
        internal EntityIdentity(SimpleColumnMap[] keyColumns)
        {
            Debug.Assert(keyColumns != null, "Must specify column maps for key columns");
            m_keys = keyColumns;
        }

        /// <summary>
        /// Get the key columns
        /// </summary>
        internal SimpleColumnMap[] Keys { get { return m_keys; } }
    }

    /// <summary>
    /// This class is a "simple" representation of the entity identity, where the
    /// entityset containing the entity is known a priori. This may be because
    /// there is exactly one entityset for the entity; or because it is inferrable
    /// from the query that only one entityset is relevant here
    /// </summary>
    internal class SimpleEntityIdentity : EntityIdentity
    {
        private md.EntitySet m_entitySet; // the entity set

        /// <summary>
        /// Basic constructor.
        /// Note: the entitySet may be null - in which case, we are referring to
        /// a transient entity
        /// </summary>
        /// <param name="entitySet">The entityset</param>
        /// <param name="keyColumns">key columns of the entity</param>
        internal SimpleEntityIdentity(md.EntitySet entitySet, SimpleColumnMap[] keyColumns)
            : base(keyColumns)
        {
            // the entityset may be null
            m_entitySet = entitySet;
        }

        /// <summary>
        /// The entityset containing the entity
        /// </summary>
        internal md.EntitySet EntitySet { get { return m_entitySet; } }

        /// <summary>
        /// Debugging support
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string separator = String.Empty;
            sb.AppendFormat(CultureInfo.InvariantCulture, "[(ES={0}) (Keys={", this.EntitySet.Name);
            foreach (SimpleColumnMap c in this.Keys)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}", separator, c);
                separator = ",";
            }
            sb.AppendFormat(CultureInfo.InvariantCulture, "})]");
            return sb.ToString();
        }
    }

    /// <summary>
    /// This class also represents entity identity. However, this class addresses
    /// those scenarios where the entityset for the entity is not uniquely known
    /// a priori. Instead, the query is annotated with information, and based on
    /// the resulting information, the appropriate entityset is identified.
    /// Specifically, the specific entityset is represented as a SimpleColumnMap
    /// in the query. The value of that column is used to look up a dictionary,
    /// and then identify the appropriate entity set.
    /// It is entirely possible that no entityset may be located for the entity
    /// instance - this represents a transient entity instance
    /// </summary>
    internal class DiscriminatedEntityIdentity : EntityIdentity
    {
        private SimpleColumnMap m_entitySetColumn;  // (optional) column map representing the entity set
        private md.EntitySet[] m_entitySetMap; // optional dictionary that maps values to entitysets

        /// <summary>
        /// Simple constructor
        /// </summary>
        /// <param name="entitySetColumn">column map representing the entityset</param>
        /// <param name="entitySetMap">Map from value -> the appropriate entityset</param>
        /// <param name="keyColumns">list of key columns</param>
        internal DiscriminatedEntityIdentity(SimpleColumnMap entitySetColumn, md.EntitySet[] entitySetMap,
            SimpleColumnMap[] keyColumns)
            : base(keyColumns)
        {
            Debug.Assert(entitySetColumn != null, "Must specify a column map to identify the entity set");
            Debug.Assert(entitySetMap != null, "Must specify a dictionary to look up entitysets");
            m_entitySetColumn = entitySetColumn;
            m_entitySetMap = entitySetMap;
        }

        /// <summary>
        /// Get the column map representing the entityset
        /// </summary>
        internal SimpleColumnMap EntitySetColumnMap { get { return m_entitySetColumn; } }

        /// <summary>
        /// Return the entityset map
        /// </summary>
        internal md.EntitySet[] EntitySetMap
        {
            get { return m_entitySetMap; }
        }

        /// <summary>
        /// Debugging support
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string separator = String.Empty;
            sb.AppendFormat(CultureInfo.InvariantCulture, "[(Keys={");
            foreach (SimpleColumnMap c in this.Keys)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}", separator, c);
                separator = ",";
            }
            sb.AppendFormat(CultureInfo.InvariantCulture, "})]");
            return sb.ToString();
        }
    }
    #endregion

    #region internal classes

    /// <summary>
    /// A VarRefColumnMap is our intermediate representation of a ColumnMap.
    /// Eventually, this gets translated into a regular ColumnMap - during the CodeGen phase
    /// </summary>
    internal class VarRefColumnMap : SimpleColumnMap
    {
        #region Public Methods
        /// <summary>
        /// Get the Var that produces this column's value
        /// </summary>
        internal InternalTrees.Var Var
        {
            get { return m_var; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Simple constructor
        /// </summary>
        /// <param name="type">datatype of this Var</param>
        /// <param name="name">the name of the column</param>
        /// <param name="v">the var producing the value for this column</param>
        internal VarRefColumnMap(md.TypeUsage type, string name, InternalTrees.Var v)
            : base(type, name)
        {
            m_var = v;
        }

        internal VarRefColumnMap(InternalTrees.Var v)
            : this(v.Type, null, v)
        {
        }

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TArgType"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="arg"></param>
        [DebuggerNonUserCode]
        internal override void Accept<TArgType>(ColumnMapVisitor<TArgType> visitor, TArgType arg)
        {
            visitor.Visit(this, arg);
        }

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TResultType"></typeparam>
        /// <typeparam name="TArgType"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="arg"></param>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType, TArgType>(ColumnMapVisitorWithResults<TResultType, TArgType> visitor, TArgType arg)
        {
            return visitor.Visit(this, arg);
        }

        /// <summary>
        /// Debugging support
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.IsNamed ? this.Name : String.Format(CultureInfo.InvariantCulture, "{0}", m_var.Id);
        }
        #endregion

        #region private state
        private InternalTrees.Var m_var;
        #endregion
    }
    #endregion
}
