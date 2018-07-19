//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization
{
    using System;
    using System.Xml;
    using DataContractDictionary = System.Collections.Generic.Dictionary<System.Xml.XmlQualifiedName, DataContract>;

    struct ScopedKnownTypes
    {
        internal DataContractDictionary[] dataContractDictionaries;
        int count;
        internal void Push(DataContractDictionary dataContractDictionary)
        {
            if (dataContractDictionaries == null)
                dataContractDictionaries = new DataContractDictionary[4];
            else if (count == dataContractDictionaries.Length)
                Array.Resize<DataContractDictionary>(ref dataContractDictionaries, dataContractDictionaries.Length * 2);
            dataContractDictionaries[count++] = dataContractDictionary;
        }

        internal void Pop()
        {
            count--;
        }

        internal DataContract GetDataContract(XmlQualifiedName qname)
        {
            for (int i = (count - 1); i >= 0; i--)
            {
                DataContractDictionary dataContractDictionary = dataContractDictionaries[i];
                DataContract dataContract;
                if (dataContractDictionary.TryGetValue(qname, out dataContract))
                    return dataContract;
            }
            return null;
        }

    }

}
