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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class ModelValidatorProviderCollection : Collection<ModelValidatorProvider> {

        public ModelValidatorProviderCollection() {
        }

        public ModelValidatorProviderCollection(IList<ModelValidatorProvider> list)
            : base(list) {
        }

        protected override void InsertItem(int index, ModelValidatorProvider item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, ModelValidatorProvider item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }
            base.SetItem(index, item);
        }

        public IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context) {
            return this.SelectMany(provider => provider.GetValidators(metadata, context));
        }

    }
}
