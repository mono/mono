/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;

    public class ByteArrayModelBinder : IModelBinder {
        public virtual object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext) {
            if (bindingContext == null) {
                throw new ArgumentNullException("bindingContext");
            }

            ValueProviderResult valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            // case 1: there was no <input ... /> element containing this data
            if (valueResult == null) {
                return null;
            }

            string value = valueResult.AttemptedValue;

            // case 2: there was an <input ... /> element but it was left blank
            if (String.IsNullOrEmpty(value)) {
                return null;
            }

            // Future proofing. If the byte array is actually an instance of System.Data.Linq.Binary
            // then we need to remove these quotes put in place by the ToString() method.
            string realValue = value.Replace("\"", String.Empty);
            return Convert.FromBase64String(realValue);
        }
    }

}
