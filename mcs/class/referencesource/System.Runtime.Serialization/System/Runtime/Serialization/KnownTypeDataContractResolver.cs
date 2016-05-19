//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization
{
    using System.Xml;

#if USE_REFEMIT
    public sealed class KnownTypeDataContractResolver : DataContractResolver
#else
    internal sealed class KnownTypeDataContractResolver : DataContractResolver
#endif
    {
        XmlObjectSerializerContext context;

        internal KnownTypeDataContractResolver(XmlObjectSerializerContext context)
        {
            Fx.Assert(context != null, "KnownTypeDataContractResolver should not be instantiated with a null context");
            this.context = context;
        }

        public override bool TryResolveType(Type type, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
        {
            if (type == null)
            {
                typeName = null;
                typeNamespace = null;
                return false;
            }
            if (declaredType != null && declaredType.IsInterface && CollectionDataContract.IsCollectionInterface(declaredType))
            {
                typeName = null;
                typeNamespace = null;
                return true;
            }

            DataContract contract = DataContract.GetDataContract(type);
            if (context.IsKnownType(contract, contract.KnownDataContracts, declaredType))
            {
                typeName = contract.Name;
                typeNamespace = contract.Namespace;
                return true;
            }
            else
            {
                typeName = null;
                typeNamespace = null;
                return false;
            }
        }

        public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
        {
            if (typeName == null || typeNamespace == null)
                return null;
            return context.ResolveNameFromKnownTypes(new XmlQualifiedName(typeName, typeNamespace));
        }

    }

}
