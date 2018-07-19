//---------------------------------------------------------------------
// <copyright file="EntityFrameworkVersionsUtil.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------

using System.Diagnostics;

namespace System.Data.Entity.Design
{
    internal static class EntityFrameworkVersionsUtil
    {
        public static readonly Version Version1 = new Version(1, 0, 0, 0);
        public static readonly Version Version2 = new Version(2, 0, 0, 0);
        public static readonly Version Version3 = new Version(3, 0, 0, 0);
        internal static Version EdmVersion1_1 { get { return new Version(1, 1, 0, 0); } }

        internal static Version ConvertToVersion(double runtimeVersion)
        {
            if (runtimeVersion == 1.0 || runtimeVersion == 0.0)
            {
                return Version1;
            }
            else if (runtimeVersion == 1.1)
            {
                // this is not a valid EntityFramework version, 
                // but only a valid EdmVersion
                return EdmVersion1_1;
            }
            else if (runtimeVersion == 2.0)
            {
                return Version2;
            }
            else
            {
                Debug.Assert(runtimeVersion == 3.0, "Did you add a new version?");
                return Version3;
            }
        }
    }
}
