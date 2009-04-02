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

    public class ViewDataDictionary<TModel> : ViewDataDictionary where TModel : class {
        public ViewDataDictionary() :
            base() {
        }

        public ViewDataDictionary(TModel model) :
            base(model) {
        }

        public ViewDataDictionary(ViewDataDictionary viewDataDictionary) :
            base(viewDataDictionary) {
        }

        public new TModel Model {
            get {
                return (TModel)base.Model;
            }
            set {
                SetModel(value);
            }
        }

        protected override void SetModel(object value) {
            TModel model = value as TModel;

            // If there was a value but the cast failed, throw an exception
            if ((value != null) && (model == null)) {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentUICulture,
                        MvcResources.ViewDataDictionary_WrongTModelType, value.GetType(), typeof(TModel)));
            }

            base.SetModel(value);
        }
    }
}
