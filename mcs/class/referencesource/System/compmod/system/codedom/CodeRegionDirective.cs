//------------------------------------------------------------------------------
// <OWNER>[....]</OWNER>
// 
// <copyright file="CodeChecksumPragma.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.CodeDom {

    using System.Diagnostics;
    using System;
    using Microsoft.Win32;
    using System.Collections;
    using System.Runtime.InteropServices;

    [
        ClassInterface(ClassInterfaceType.AutoDispatch),
        ComVisible(true),
        Serializable,
    ]
    public class CodeRegionDirective: CodeDirective {
        private string regionText;
        private CodeRegionMode regionMode;

        public CodeRegionDirective() {
        }
        
        public CodeRegionDirective(CodeRegionMode regionMode, string regionText) {
            this.RegionText = regionText;
            this.regionMode = regionMode;
        }

        public string RegionText {
            get {
                return (regionText == null) ? string.Empty : regionText;
            }
            set {
                regionText = value;
            }
        }
                
        public CodeRegionMode RegionMode {
            get {
                return regionMode;
            }
            set {
                regionMode = value;
            }
        }
    }
}
