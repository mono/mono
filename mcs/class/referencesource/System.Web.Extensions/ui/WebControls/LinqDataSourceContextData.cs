//------------------------------------------------------------------------------
// <copyright file="LinqDataSourceContextData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

 #if ORYX_VNEXT
namespace Microsoft.Web.Data.UI.WebControls {
#else
namespace System.Web.UI.WebControls {
    using System.Security.Permissions;
#endif
    public class ContextDataSourceContextData {
        public ContextDataSourceContextData() {
        }

         public ContextDataSourceContextData(object context) {
            Context = context;
        }

         public object Context {
            get;
            set;
        }

         public object EntitySet {
            get;
            set;
        }

     }
}