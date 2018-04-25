//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.Xml;

    abstract class Transform
    {
        protected Transform()
        {
        }

        public abstract string Algorithm
        {
            get;
        }

        public virtual bool NeedsInclusiveContext
        {
            get { return false; }
        }

        public abstract object Process(object input, SignatureResourcePool resourcePool, DictionaryManager dictionaryManager);

        public abstract byte[] ProcessAndDigest(object input, SignatureResourcePool resourcePool, string digestAlgorithm, DictionaryManager dictionaryManager);

        public abstract void ReadFrom(XmlDictionaryReader reader, DictionaryManager dictionaryManager, bool preserveComments);

        public abstract void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager);
    }
}
