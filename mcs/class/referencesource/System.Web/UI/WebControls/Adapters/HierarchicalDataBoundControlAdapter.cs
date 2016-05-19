//------------------------------------------------------------------------------
// <copyright file="HierarchicalDataBoundControlAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.Adapters {

    public class HierarchicalDataBoundControlAdapter : WebControlAdapter {

        protected new HierarchicalDataBoundControl Control {
            get {
                return (HierarchicalDataBoundControl)base.Control;
            }
        }

        protected internal virtual void PerformDataBinding() {
            Control.PerformDataBinding();
        }
    }
}
