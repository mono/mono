//------------------------------------------------------------------------------
// <copyright file="RecordStateScratchpad.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace System.Data.Common.Internal.Materialization
{
    /// <summary>
    /// Used in the Translator to aggregate information about a (nested) record
    /// state.  After the translator visits the columnMaps, it will compile
    /// the recordState(s) which produces an immutable RecordStateFactory that 
    /// can be shared amongst many query instances.
    /// </summary>
    internal class RecordStateScratchpad
    {
        private int _stateSlotNumber;
        internal int StateSlotNumber
        {
            get { return _stateSlotNumber; }
            set { _stateSlotNumber = value; }
        }

        private int _columnCount;
        internal int ColumnCount
        {
            get { return _columnCount; }
            set { _columnCount = value; }
        }

        private DataRecordInfo _dataRecordInfo;
        internal DataRecordInfo DataRecordInfo
        {
            get { return _dataRecordInfo; }
            set { _dataRecordInfo = value; }
        }

        private Expression _gatherData;
        internal Expression GatherData
        {
            get { return _gatherData; }
            set { _gatherData = value; }
        }

        private string[] _propertyNames;
        internal string[] PropertyNames
        {
            get { return _propertyNames; }
            set { _propertyNames = value; }
        }
        private TypeUsage[] _typeUsages;
        internal TypeUsage[] TypeUsages
        {
            get { return _typeUsages; }
            set { _typeUsages = value; }
        }

        private List<RecordStateScratchpad> _nestedRecordStateScratchpads = new List<RecordStateScratchpad>();

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal RecordStateFactory Compile()
        {
            RecordStateFactory[] nestedRecordStateFactories = new RecordStateFactory[_nestedRecordStateScratchpads.Count];
            for (int i = 0; i < nestedRecordStateFactories.Length; i++)
            {
                nestedRecordStateFactories[i] = _nestedRecordStateScratchpads[i].Compile();
            }

            RecordStateFactory result = (RecordStateFactory)Activator.CreateInstance(typeof(RecordStateFactory), new object[] {
                                                            this.StateSlotNumber, 
                                                            this.ColumnCount,
                                                            nestedRecordStateFactories,
                                                            this.DataRecordInfo,
                                                            this.GatherData,
                                                            this.PropertyNames,
                                                            this.TypeUsages
                                                            });
            return result;
        }
    }
}
