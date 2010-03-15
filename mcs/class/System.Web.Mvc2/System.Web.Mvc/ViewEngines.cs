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

    public static class ViewEngines {

        private readonly static ViewEngineCollection _engines = new ViewEngineCollection {
            new WebFormViewEngine() 
        };

        public static ViewEngineCollection Engines {
            get {
                return _engines;
            }
        }
    }
}
