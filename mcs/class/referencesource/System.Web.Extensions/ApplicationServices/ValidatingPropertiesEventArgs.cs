//------------------------------------------------------------------------------
// <copyright file="ValidatingPropertiesEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.ApplicationServices
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Web;

    public class ValidatingPropertiesEventArgs: EventArgs{
        private IDictionary<string,object> _properties;
        public IDictionary<string,object> Properties{
            get{
                    return _properties;
                }
            }

        private Collection<string> _failedProperties;
        public Collection<string> FailedProperties{
            get{
                    return _failedProperties;
                }
        }
        internal ValidatingPropertiesEventArgs(){}

        internal ValidatingPropertiesEventArgs(IDictionary<string,object> properties){
            _properties = properties;
            _failedProperties = new Collection<string>();
        }
    }
}
    
