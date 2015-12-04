//---------------------------------------------------------------------
// <copyright file="StorageConditionPropertyMapping.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Data.Metadata.Edm;

namespace System.Data.Mapping {
    /// <summary>
    /// Mapping metadata for Conditional property mapping on a type.
    /// Condition Property Mapping specifies a Condition either on the C side property or S side property.
    /// </summary>
    /// <example>
    /// For Example if conceptually you could represent the CS MSL file as following
    /// --Mapping 
    ///   --EntityContainerMapping ( CNorthwind-->SNorthwind )
    ///     --EntitySetMapping
    ///       --EntityTypeMapping
    ///         --MappingFragment
    ///           --EntityKey
    ///             --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///           --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///           --ConditionProperyMap ( constant value-->SMemberMetadata )
    ///       --EntityTypeMapping
    ///         --MappingFragment
    ///           --EntityKey
    ///             --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///           --ComplexPropertyMap
    ///             --ComplexTypeMap
    ///               --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///               --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    ///           --ConditionProperyMap ( constant value-->SMemberMetadata )
    ///     --AssociationSetMapping 
    ///       --AssociationTypeMapping
    ///         --MappingFragment
    ///           --EndPropertyMap
    ///             --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///             --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    ///           --EndPropertyMap
    ///             --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// This class represents the metadata for all the condition property map elements in the 
    /// above example.
    /// </example>
    internal class StorageConditionPropertyMapping : StoragePropertyMapping {
        #region Constructors
        /// <summary>
        /// Construct a new condition Property mapping object
        /// </summary>
        /// <param name="cdmMember"></param>
        /// <param name="columnMember"></param>
        /// <param name="value"></param>
        /// <param name="isNull"></param>
        internal StorageConditionPropertyMapping(EdmProperty cdmMember, EdmProperty columnMember
            , object value, Nullable<bool> isNull) : base(cdmMember) {
            Debug.Assert((cdmMember != null) || (columnMember != null), "Both CDM and Column Members can not be specified for Condition Mapping");
            Debug.Assert((cdmMember == null) || (columnMember == null), "Either CDM or Column Members has to be specified for Condition Mapping");
            Debug.Assert((isNull.HasValue) || (value != null), "Both Value and IsNull can not be specified on Condition Mapping");
            Debug.Assert(!(isNull.HasValue) || (value == null), "Either Value or IsNull has to be specified on Condition Mapping");
            this.m_columnMember = columnMember;
            this.m_value = value;
            this.m_isNull = isNull;
        }
        #endregion

        #region Fields
        /// <summary>
        /// Column EdmMember for which the condition is specified.
        /// </summary>
        EdmProperty m_columnMember;
        /// <summary>
        /// Value for the condition thats being mapped.
        /// </summary>
        object m_value;
        bool? m_isNull;
        #endregion

        #region Properties
        /// <summary>
        /// Value for the condition
        /// </summary>
        internal object Value {
            get {
                return this.m_value;
            }
        }

        /// <summary>
        /// Whether the property is being mapped to Null or NotNull
        /// </summary>
        internal Nullable<bool> IsNull {
            get {
                return this.m_isNull;
            }
        }


        /// <summary>
        /// ColumnMember for which the Condition Map is being specified
        /// </summary>
        internal EdmProperty ColumnProperty {
            get {
                return this.m_columnMember;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// This method is primarily for debugging purposes.
        /// Will be removed shortly.
        /// </summary>
        /// <param name="index"></param>
        internal override void Print(int index) {
            StorageEntityContainerMapping.GetPrettyPrintString(ref index);
            StringBuilder sb = new StringBuilder();
            sb.Append("ConditionPropertyMapping");
            sb.Append("   ");
            if (this.EdmProperty != null) {
                sb.Append("Name:");
                sb.Append(this.EdmProperty.Name);
                sb.Append("   ");
            }
            if (this.ColumnProperty != null) {
                sb.Append("Column Name:");
                sb.Append(this.ColumnProperty.Name);
                sb.Append("   ");
            }
            if (this.Value != null) {
                sb.Append("Value:");
                sb.Append("'" + Value + "'");
                sb.Append("   ");
                sb.Append("Value CLR Type:");
                sb.Append("'" + Value.GetType() + "'");
                sb.Append("   ");
            }
            sb.Append("Value TypeMetadata:");
            EdmType memberType = (ColumnProperty != null) ? ColumnProperty.TypeUsage.EdmType : null;
            if (memberType != null)
            {
                sb.Append("'" + memberType.FullName + "'");
                sb.Append("   ");
            }
            if (this.IsNull.HasValue) {
                sb.Append("IsNull:");
                sb.Append(this.IsNull);
                sb.Append("   ");
            }
            Console.WriteLine(sb.ToString());
        }
        #endregion
    }
}
