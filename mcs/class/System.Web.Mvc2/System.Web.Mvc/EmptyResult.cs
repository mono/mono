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

    // represents a result that doesn't do anything, like a controller action returning null
    public class EmptyResult : ActionResult {

        private static readonly EmptyResult _singleton = new EmptyResult();

        internal static EmptyResult Instance {
            get {
                return _singleton;
            }
        }

        public override void ExecuteResult(ControllerContext context) {
        }
    }
}
