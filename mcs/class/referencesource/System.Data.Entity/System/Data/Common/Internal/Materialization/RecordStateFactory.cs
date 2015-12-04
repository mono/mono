//------------------------------------------------------------------------------
// <copyright file="recordstatefactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Linq;
using System.Linq.Expressions;

namespace System.Data.Common.Internal.Materialization
{
    /// <summary>
    /// An immutable class used to generate new RecordStates, which are used
    /// at runtime to produce value-layer (aka DataReader) results.  
    /// 
    /// Contains static information collected by the Translator visitor.  The 
    /// expressions produced by the Translator are compiled.  The RecordStates
    /// will refer to this object for all static information.
    /// 
    /// This class is cached in the query cache as part of the CoordinatorFactory.
    /// </summary>
    internal class RecordStateFactory
    {
        #region state

        /// <summary>
        /// Indicates which state slot in the Shaper.State is expected to hold the
        /// value for this record state.  Each unique record shape has it's own state
        /// slot.
        /// </summary>
        internal readonly int StateSlotNumber;

        /// <summary>
        /// How many column values we have to reserve space for in this record.
        /// </summary>
        internal readonly int ColumnCount;

        /// <summary>
        /// The DataRecordInfo we must return for this record.  If the record represents
        /// an entity, this will be used to construct a unique EntityRecordInfo with the
        /// EntityKey and EntitySet for the entity.
        /// </summary>
        internal readonly DataRecordInfo DataRecordInfo;

        /// <summary>
        /// A function that will gather the data for the row and store it on the record state.
        /// </summary>
        internal readonly Func<Shaper, bool> GatherData;

        /// <summary>
        /// Collection of nested records for this record, such as a complex type that is
        /// part of an entity.  This does not include records that are part of a nested
        /// collection, however.
        /// </summary>
        internal readonly System.Collections.ObjectModel.ReadOnlyCollection<RecordStateFactory> NestedRecordStateFactories;

        /// <summary>
        /// The name for each column.
        /// </summary>
        internal readonly System.Collections.ObjectModel.ReadOnlyCollection<string> ColumnNames;

        /// <summary>
        /// The type usage information for each column.
        /// </summary>
        internal readonly System.Collections.ObjectModel.ReadOnlyCollection<TypeUsage> TypeUsages;

        /// <summary>
        /// Tracks which columns might need special handling (nested readers/records)
        /// </summary>
        internal readonly System.Collections.ObjectModel.ReadOnlyCollection<bool> IsColumnNested;

        /// <summary>
        /// Tracks whether there are ANY columns that need special handling.
        /// </summary>
        internal readonly bool HasNestedColumns;

        /// <summary>
        /// A helper class to make the translation from name->ordinal.
        /// </summary>
        internal readonly FieldNameLookup FieldNameLookup;

        /// <summary>
        /// Description of this RecordStateFactory, used for debugging only; while this
        /// is not  needed in retail code, it is pretty important because it's the only 
        /// description we'll have once we compile the Expressions; debugging a problem 
        /// with retail bits would be pretty hard without this.
        /// </summary>
        private readonly string Description;

        #endregion

        #region constructor

        public RecordStateFactory(int stateSlotNumber, int columnCount, RecordStateFactory[] nestedRecordStateFactories, DataRecordInfo dataRecordInfo, Expression gatherData, string[] propertyNames, TypeUsage[] typeUsages)
        {
            this.StateSlotNumber = stateSlotNumber;
            this.ColumnCount = columnCount;
            this.NestedRecordStateFactories = new System.Collections.ObjectModel.ReadOnlyCollection<RecordStateFactory>(nestedRecordStateFactories);
            this.DataRecordInfo = dataRecordInfo;
            this.GatherData = Translator.Compile<bool>(gatherData);
            this.Description = gatherData.ToString();
            this.ColumnNames = new System.Collections.ObjectModel.ReadOnlyCollection<string>(propertyNames);
            this.TypeUsages = new System.Collections.ObjectModel.ReadOnlyCollection<TypeUsage>(typeUsages);

            this.FieldNameLookup = new FieldNameLookup(this.ColumnNames, -1);

            // pre-compute the nested objects from typeUsage, for performance
            bool[] isColumnNested = new bool[columnCount];

            for (int ordinal = 0; ordinal < columnCount; ordinal++)
            {
                switch (typeUsages[ordinal].EdmType.BuiltInTypeKind)
                {
                    case BuiltInTypeKind.EntityType:
                    case BuiltInTypeKind.ComplexType:
                    case BuiltInTypeKind.RowType:
                    case BuiltInTypeKind.CollectionType:
                        isColumnNested[ordinal] = true;
                        this.HasNestedColumns = true;
                        break;
                    default:
                        isColumnNested[ordinal] = false;
                        break;
                }
            }
            this.IsColumnNested = new System.Collections.ObjectModel.ReadOnlyCollection<bool>(isColumnNested);
        }

        #endregion

        #region "public" surface area

        /// <summary>
        /// It's GO time, create the record state.
        /// </summary>
        internal RecordState Create(CoordinatorFactory coordinatorFactory)
        {
            return new RecordState(this, coordinatorFactory);
        }

        #endregion
    }

}
