// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  SatelliteContractVersionAttribute
** 
** <OWNER>Microsoft</OWNER>
**
**
** Purpose: Specifies which version of a satellite assembly 
**          the ResourceManager should ask for.
**
**
===========================================================*/

namespace System.Resources {
    using System;
    using System.Diagnostics.Contracts;
    
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=false)]  
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SatelliteContractVersionAttribute : Attribute 
    {
        private String _version;

        public SatelliteContractVersionAttribute(String version)
        {
            if (version == null)
                throw new ArgumentNullException("version");
            Contract.EndContractBlock();
            _version = version;
        }

        public String Version {
            get { return _version; }
        }
    }
}
