//------------------------------------------------------------------------------
// <copyright file="GeneratedCodeAttribute.cs" company="Microsoft">
// 
// <OWNER>Microsoft</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.CodeDom.Compiler {
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public sealed class GeneratedCodeAttribute : Attribute {
        private readonly string tool;
        private readonly string version;

        public GeneratedCodeAttribute(string tool, string version) {
            this.tool = tool;
            this.version = version;
        }

        public string Tool {
            get { return this.tool; }
        }

        public string Version {
            get { return this.version; }
        }
    }
}
