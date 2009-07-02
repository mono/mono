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
    using System.Collections.Generic;

    public abstract class ParameterBindingInfo {

        public virtual IModelBinder Binder {
            get {
                return null;
            }
        }

        public virtual ICollection<string> Exclude {
            get {
                return new string[0];
            }
        }

        public virtual ICollection<string> Include {
            get {
                return new string[0];
            }
        }

        public virtual string Prefix {
            get {
                return null;
            }
        }

    }
}
