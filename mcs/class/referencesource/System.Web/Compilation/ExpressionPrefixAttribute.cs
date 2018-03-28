//------------------------------------------------------------------------------
// <copyright file="ExpressionPrefixAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Compilation {

    using System;
    using System.ComponentModel;
    using System.Security.Permissions;


    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public sealed class ExpressionPrefixAttribute : Attribute {

        private string _expressionPrefix;


        public ExpressionPrefixAttribute(string expressionPrefix) {
            if (String.IsNullOrEmpty(expressionPrefix)) {
                throw new ArgumentNullException("expressionPrefix");
            }

            _expressionPrefix = expressionPrefix;
        }


        public string ExpressionPrefix {
            get {
                return _expressionPrefix;
            }
        }
    }
}

