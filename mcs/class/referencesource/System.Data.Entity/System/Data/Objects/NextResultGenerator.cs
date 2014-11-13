//------------------------------------------------------------------------------
// <copyright file="NextResultGenerator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">WillA</owner>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Common.Utils;
using System.Data.EntityClient;
using System.Data.Common.Internal.Materialization;
using System.Data.Metadata.Edm;
using System.Linq;
using System.Text;


namespace System.Data.Objects
{
    internal class NextResultGenerator 
    {
        EntityCommand _entityCommand;
        ReadOnlyMetadataCollection<EntitySet> _entitySets;
        ObjectContext _context;
        EdmType[] _edmTypes;
        int _resultSetIndex;
        MergeOption _mergeOption;

        internal NextResultGenerator(ObjectContext context, EntityCommand entityCommand, EdmType[] edmTypes, ReadOnlyMetadataCollection<EntitySet> entitySets, MergeOption mergeOption, int resultSetIndex)
        {
            _context = context;
            _entityCommand = entityCommand;
            _entitySets = entitySets;
            _edmTypes = edmTypes;
            _resultSetIndex = resultSetIndex;
            _mergeOption = mergeOption;
        }

        internal ObjectResult<TElement> GetNextResult<TElement>(DbDataReader storeReader)
        {
            bool isNextResult = false;
            try
            {
                isNextResult = storeReader.NextResult();
            }
            catch (Exception e)
            {
                if (EntityUtil.IsCatchableExceptionType(e))
                {
                    throw EntityUtil.CommandExecution(System.Data.Entity.Strings.EntityClient_StoreReaderFailed, e);
                }
                throw;
            }

            if (isNextResult)
            {
                EdmType edmType = _edmTypes[_resultSetIndex];
                MetadataHelper.CheckFunctionImportReturnType<TElement>(edmType, _context.MetadataWorkspace);
                return _context.MaterializedDataRecord<TElement>(_entityCommand, storeReader, _resultSetIndex, _entitySets, _edmTypes, _mergeOption);
            }
            else
            {
                return null; 
            }
        }
    }
}
