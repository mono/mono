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

    public class FilterInfo {

        private List<IActionFilter> _actionFilters = new List<IActionFilter>();
        private List<IAuthorizationFilter> _authorizationFilters = new List<IAuthorizationFilter>();
        private List<IExceptionFilter> _exceptionFilters = new List<IExceptionFilter>();
        private List<IResultFilter> _resultFilters = new List<IResultFilter>();

        public IList<IActionFilter> ActionFilters {
            get {
                return _actionFilters;
            }
        }

        public IList<IAuthorizationFilter> AuthorizationFilters {
            get {
                return _authorizationFilters;
            }
        }

        public IList<IExceptionFilter> ExceptionFilters {
            get {
                return _exceptionFilters;
            }
        }

        public IList<IResultFilter> ResultFilters {
            get {
                return _resultFilters;
            }
        }

    }
}
