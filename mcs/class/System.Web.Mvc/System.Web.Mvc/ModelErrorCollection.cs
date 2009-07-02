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
    using System.Collections.ObjectModel;

    [Serializable]
    public class ModelErrorCollection : Collection<ModelError> {

        public void Add(Exception exception) {
            Add(new ModelError(exception));
        }

        public void Add(string errorMessage) {
            Add(new ModelError(errorMessage));
        }
    }
}
