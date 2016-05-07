//------------------------------------------------------------------------------
// <copyright file="TagPrefixAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;
    using System.ComponentModel;
    using System.Web.Util;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=true)]
    public sealed class TagPrefixAttribute : Attribute {

        private string namespaceName;
        private string tagPrefix;


        public TagPrefixAttribute(string namespaceName, string tagPrefix) {
            if (String.IsNullOrEmpty(namespaceName)) {
                throw ExceptionUtil.ParameterNullOrEmpty("namespaceName");
            }
            if (String.IsNullOrEmpty(tagPrefix)) {
                throw ExceptionUtil.ParameterNullOrEmpty("tagPrefix");
            }

            this.namespaceName = namespaceName;
            this.tagPrefix = tagPrefix;
        }


        public string NamespaceName {
            get {
                return namespaceName;
            }
        }


        public string TagPrefix {
            get {
                return tagPrefix;
            }
        }
    }
}

