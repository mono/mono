//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Claims
{
    public static class Rights
    {
        const string rightNamespace = XsiConstants.Namespace + "/right";

        const string identity = rightNamespace + "/identity";
        const string possessProperty = rightNamespace + "/possessproperty";

        static public string Identity { get { return identity; } }
        static public string PossessProperty { get { return possessProperty; } }

    }
}
