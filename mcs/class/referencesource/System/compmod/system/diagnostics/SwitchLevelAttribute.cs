//------------------------------------------------------------------------------
// <copyright file="SwitchLevelAttribute .cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Reflection;
using System.Collections;

namespace System.Diagnostics {

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SwitchLevelAttribute : Attribute {
        private Type type;
        
    	public SwitchLevelAttribute (Type switchLevelType) {
    	    SwitchLevelType = switchLevelType;
        }

    	public Type SwitchLevelType { 
    	    get { return type;}
    	    set { 
    	        if (value == null)
    	            throw new ArgumentNullException("value");
    	        
    	        type = value;
	        }
        }
    }
}
