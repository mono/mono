//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    class ExtendedTransformFactory : StandardTransformFactory
    {
        static ExtendedTransformFactory instance = new ExtendedTransformFactory();

        ExtendedTransformFactory() { }

        internal new static ExtendedTransformFactory Instance
        {
            get { return instance; }
        }

        public override Transform CreateTransform(string transformAlgorithmUri)
        {
            if (transformAlgorithmUri == XD.XmlSignatureDictionary.EnvelopedSignature.Value)
            {
                return new EnvelopedSignatureTransform();
            }
            else
            {
                return base.CreateTransform(transformAlgorithmUri);
            }
        }
    }
}
