//------------------------------------------------------------------------------
// <copyright file="ModuleConfigurationInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.Threading;
    using System.Configuration;
    using System.Web.Compilation;
    using System.Web.Util;

    // used by HttpApplication to keep track of configuration
    // info read by native config
    internal class ModuleConfigurationInfo {
        string _type;
        string _name;
        string _precondition;
        
        internal ModuleConfigurationInfo(string name, string type, string condition) {
            _type = type;
            _name = name;
            _precondition = condition;
        }

        internal string Type {
            get {
                return _type;
            }
        }

        internal string Name {
            get {
                return _name;
            }
        }

        internal string Precondition {
            get {
                return _precondition;
            }
        }
    }
}
