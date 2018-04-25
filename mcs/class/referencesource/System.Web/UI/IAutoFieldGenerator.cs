//------------------------------------------------------------------------------
// <copyright file="IDynamicDataManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.UI {
    using System.Collections;
    using System.Web.UI.WebControls;

    public interface IAutoFieldGenerator {
        ICollection GenerateFields(Control control);
    }

}
