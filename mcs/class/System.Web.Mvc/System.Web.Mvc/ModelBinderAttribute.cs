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
    using System.Globalization;
    using System.Web.Mvc.Resources;

    [AttributeUsage(ValidTargets, AllowMultiple = false, Inherited = false)]
    public sealed class ModelBinderAttribute : CustomModelBinderAttribute {

        public ModelBinderAttribute(Type binderType) {
            if (binderType == null) {
                throw new ArgumentNullException("binderType");
            }
            if (!typeof(IModelBinder).IsAssignableFrom(binderType)) {
                string message = String.Format(CultureInfo.CurrentUICulture,
                    MvcResources.ModelBinderAttribute_TypeNotIModelBinder, binderType.FullName);
                throw new ArgumentException(message, "binderType");
            }

            BinderType = binderType;
        }

        public Type BinderType {
            get;
            private set;
        }

        public override IModelBinder GetBinder() {
            try {
                return (IModelBinder)Activator.CreateInstance(BinderType);
            }
            catch (Exception ex) {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentUICulture,
                        MvcResources.ModelBinderAttribute_ErrorCreatingModelBinder,
                        BinderType.FullName),
                    ex);
            }
        }

    }
}
