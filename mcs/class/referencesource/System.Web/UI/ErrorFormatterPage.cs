//------------------------------------------------------------------------------
// <copyright file="ErrorFormatterPage.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * This is a page class that is used for adaptive error formatting for mobile
 * devices.
 *
 * NOTE: We explicitly override the state management methods because if the
 *       normal page class is used, an exception is thrown for mobile devices.
 *       For example, WmlPageAdapter uses SessionPageStatePersister
 *       for persisting view state.  SessionPageStatePersister requires
 *       Context.Session to be available.  Otherwise, it would throw in the
 *       constructor.  However, when an error occurred, Context.Session is
 *       removed by SessionStateModule before the error is being formatted and
 *       rendered.  Hence the methods are overridden below and ignored to avoid
 *       the exception since there is no need to persist any view state for the
 *       adaptive error page which is created dynamically during error handling.
 *
 * Copyright (c) 2003 Microsoft Corporation
 */

namespace System.Web.UI {
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.MSInternal", "CA910:AlwaysSetViewStateUserKeyToUniqueValue",
        Justification = "This page does not allow access to sensitive information.")]
    internal sealed class ErrorFormatterPage : Page {
        protected internal override void SavePageStateToPersistenceMedium(Object viewState) {
            // Override and ignore. No need to save view state for this page.
        }

        protected internal override Object LoadPageStateFromPersistenceMedium() {
            // Override and ignore. No view state to load for this page.
            return null;
        }
    }
}
