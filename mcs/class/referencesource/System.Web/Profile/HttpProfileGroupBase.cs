//------------------------------------------------------------------------------
// <copyright file="ProfileGroupBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * ProfileGroupBase
 *
 * Copyright (c) 2002 Microsoft Corporation
 */

namespace System.Web.Profile {
    using  System.Security.Principal;
    using  System.Security.Permissions;
    using  System.Collections;
    using  System.Collections.Specialized;
    using  System.Web.Configuration;
    using  System.Web.Util;
    using  System.Web.Security;

    public class ProfileGroupBase {


        public object this[string propertyName] { get { return _Parent[_MyName + propertyName];} set { _Parent[_MyName + propertyName] = value; } }


        public object GetPropertyValue(string propertyName) {
            return _Parent[_MyName + propertyName];
        }

        public void SetPropertyValue(string propertyName, object propertyValue) {
            _Parent[_MyName + propertyName] = propertyValue;
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////

        public ProfileGroupBase() {
            _Parent = null;
            _MyName = null;
        }

        public void Init(ProfileBase parent, string myName) {
            if (_Parent == null) {
                _Parent = parent;
                _MyName = myName + ".";
            }
        }
        private string _MyName;
        private ProfileBase _Parent;
    }
}

