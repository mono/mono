//------------------------------------------------------------------------------
// <copyright file="LinqDataSourceEditData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System.Security.Permissions;

    public class QueryableDataSourceEditData {

        private object _newDataObject;
        private object _originalDataObject;

        public object NewDataObject {
            get {
                return _newDataObject;
            }
            set {
                _newDataObject = value;
            }
        }

        public object OriginalDataObject {
            get {
                return _originalDataObject;
            }
            set {
                _originalDataObject = value;
            }
        }	

    }
}

