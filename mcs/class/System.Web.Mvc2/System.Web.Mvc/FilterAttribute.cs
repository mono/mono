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
    using System.Web.Mvc.Resources;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public abstract class FilterAttribute : Attribute {

        private int _order = -1;

        public int Order {
            get {
                return _order;
            }
            set {
                if (value < -1) {
                    throw new ArgumentOutOfRangeException("value",
                        MvcResources.FilterAttribute_OrderOutOfRange);
                }
                _order = value;
            }
        }
    }
}
