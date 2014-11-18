//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.Collections;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml;

    internal abstract class TransformFactory
    {
        public abstract Transform CreateTransform(string transformAlgorithmUri);
    }
}
