//------------------------------------------------------------------------------
// <copyright file="IDataBindingsAccessor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;

    /// <devdoc>
    ///   <para>Allows designer functionality to access information about a UserControl, that is
    ///     applicable at design-time only.
    ///   </para>
    /// </devdoc>
    public interface IUserControlDesignerAccessor {


        string InnerText {
            get;
            set;
        }


        string TagName {
            get;
            set;
        }
    }
}
