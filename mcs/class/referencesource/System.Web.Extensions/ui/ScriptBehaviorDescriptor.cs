//------------------------------------------------------------------------------
// <copyright file="ScriptBehaviorDescriptor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Web;
    using System.Web.Resources;

    public class ScriptBehaviorDescriptor : ScriptComponentDescriptor {
        private string _name;

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID")]
        public ScriptBehaviorDescriptor(string type, string elementID) : base(type, elementID) {
            RegisterDispose = false;
        }

        public override string ClientID {
            get {
                if (String.IsNullOrEmpty(ID)) {
                    Debug.Assert(!String.IsNullOrEmpty(ElementID), "Base ctor ensures ElementID is not null or empty");
                    return ElementID + "$" + Name;
                }
                else {
                    return ID;
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID")]
        public string ElementID {
            get {
                return base.ElementIDInternal;
            }
        }

        public string Name {
            get {
                if (String.IsNullOrEmpty(_name)) {
                    return GetTypeName(this.Type);
                }
                else {
                    return _name;
                }
            }
            set {
                _name = value;
            }
        }

        // Returns the short name of a possibly namespace-qualified type name.
        // Examples:
        //   "TestNS1.TestNS2.TestType" -> "TestType"
        //   "TestType" -> "TestType"
        private static string GetTypeName(string type) {
            int index = type.LastIndexOf('.');
            if (index == -1) {
                return type;
            }
            else {
                return type.Substring(index + 1);
            }
        }

        protected internal override string GetScript() {
            if (!String.IsNullOrEmpty(_name)) {
                AddProperty("name", _name);
            }
            return base.GetScript();
        }
    }
}
