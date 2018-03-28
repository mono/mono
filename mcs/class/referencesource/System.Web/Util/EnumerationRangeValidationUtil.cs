//------------------------------------------------------------------------------
// <copyright file="EnumerationRangeValidationUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Helper class for performing common enumeration range checks.
 * 
 * Copyright (c) 2009 Microsoft Corporation
 */

namespace System.Web.Util {

    using System.Web.UI.WebControls;

    internal static class EnumerationRangeValidationUtil {

        public static void ValidateRepeatLayout(RepeatLayout value) {
            if (value < RepeatLayout.Table || value > RepeatLayout.OrderedList) {
                throw new ArgumentOutOfRangeException("value");
            }
        }
    }
}
