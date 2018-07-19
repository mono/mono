//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    public abstract class BasicSecurityProfileVersion
    {
        internal BasicSecurityProfileVersion() { }

        public static BasicSecurityProfileVersion BasicSecurityProfile10
        {
            get { return BasicSecurityProfile10BasicSecurityProfileVersion.Instance; }
        }

        class BasicSecurityProfile10BasicSecurityProfileVersion : BasicSecurityProfileVersion
        {
            static BasicSecurityProfile10BasicSecurityProfileVersion instance = new BasicSecurityProfile10BasicSecurityProfileVersion();

            public static BasicSecurityProfile10BasicSecurityProfileVersion Instance { get { return instance; } }

            public override string ToString()
            {
                return "BasicSecurityProfile10";
            }
        }
    }
}
