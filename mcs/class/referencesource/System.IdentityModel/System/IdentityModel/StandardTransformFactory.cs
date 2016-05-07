//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.IdentityModel.Tokens;
    using System.Security.Cryptography;

    class StandardTransformFactory : TransformFactory
    {
        static StandardTransformFactory instance = new StandardTransformFactory();

        protected StandardTransformFactory() { }

        internal static StandardTransformFactory Instance
        {
            get { return instance; }
        }

        public override Transform CreateTransform(string transformAlgorithmUri)
        {
            if (transformAlgorithmUri == SecurityAlgorithms.ExclusiveC14n)
            {
                return new ExclusiveCanonicalizationTransform();
            }
            else if (transformAlgorithmUri == SecurityAlgorithms.ExclusiveC14nWithComments)
            {
                return new ExclusiveCanonicalizationTransform(false, true);
            }
            else if (transformAlgorithmUri == SecurityAlgorithms.StrTransform)
            {
                return new StrTransform();
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.UnsupportedTransformAlgorithm)));
            }
        }
    }
}
