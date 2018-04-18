//------------------------------------------------------------------------------
// <copyright file="Util.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

using System.Data.Metadata.Edm;
using System.Data.Mapping;
namespace System.Data.Common.Internal.Materialization
{
    static class Util
    {
        /// <summary>
        /// Retrieves a mapping to CLR type for the given EDM type. Assumes the MetadataWorkspace has no    
        /// </summary>
        internal static ObjectTypeMapping GetObjectMapping(EdmType type, MetadataWorkspace workspace)
        {
            // Check if the workspace has cspace item collection registered with it. If not, then its a case
            // of public materializer trying to create objects from PODR or EntityDataReader with no context.
            ItemCollection collection;
            if (workspace.TryGetItemCollection(DataSpace.CSpace, out collection))
            {
                return (ObjectTypeMapping)workspace.GetMap(type, DataSpace.OCSpace);
            }
            else
            {
                EdmType ospaceType;
                EdmType cspaceType;
                // If its a case of EntityDataReader with no context, the typeUsage which is passed in must contain
                // a cspace type. We need to look up an OSpace type in the ospace item collection and then create
                // ocMapping
                if (type.DataSpace == DataSpace.CSpace)
                {
                    // if its a primitive type, then the names will be different for CSpace type and OSpace type
                    if (Helper.IsPrimitiveType(type))
                    {
                        ospaceType = workspace.GetMappedPrimitiveType(((PrimitiveType)type).PrimitiveTypeKind, DataSpace.OSpace);
                    }
                    else
                    {
                        // Metadata will throw if there is no item with this identity present.
                        // Is this exception fine or does object materializer code wants to wrap and throw a new exception
                        ospaceType = workspace.GetItem<EdmType>(type.FullName, DataSpace.OSpace);
                    }
                    cspaceType = type;
                }
                else
                {
                    // In case of PODR, there is no cspace at all. We must create a fake ocmapping, with ospace types
                    // on both the ends
                    ospaceType = type;
                    cspaceType = type;
                }

                // This condition must be hit only when someone is trying to materialize a legacy data reader and we
                // don't have the CSpace metadata.
                if (!Helper.IsPrimitiveType(ospaceType) && !Helper.IsEntityType(ospaceType) && !Helper.IsComplexType(ospaceType))
                {
                    throw EntityUtil.MaterializerUnsupportedType();
                }

                ObjectTypeMapping typeMapping;

                if (Helper.IsPrimitiveType(ospaceType))
                {
                    typeMapping = new ObjectTypeMapping(ospaceType, cspaceType);
                }
                else
                {
                    typeMapping = DefaultObjectMappingItemCollection.LoadObjectMapping(cspaceType, ospaceType, null);
                }

                return typeMapping;
            }
        }
    }
}
