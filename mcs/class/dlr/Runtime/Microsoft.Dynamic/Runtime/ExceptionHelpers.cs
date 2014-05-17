/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
#if FEATURE_CORE_DLR
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Runtime {
    public static class ExceptionHelpers {
#if FEATURE_STACK_TRACE
        private const string prevStackTraces = "PreviousStackTraces";

        /// <summary>
        /// Updates an exception before it's getting re-thrown so
        /// we can present a reasonable stack trace to the user.
        /// </summary>
        public static Exception UpdateForRethrow(Exception rethrow) {
#if !SILVERLIGHT
            List<StackTrace> prev;

            // we don't have any dynamic stack trace data, capture the data we can
            // from the raw exception object.
            StackTrace st = new StackTrace(rethrow, true);

            if (!TryGetAssociatedStackTraces(rethrow, out prev)) {
                prev = new List<StackTrace>();
                AssociateStackTraces(rethrow, prev);
            }

            prev.Add(st);

#endif
            return rethrow;
        }

        /// <summary>
        /// Returns all the stack traces associates with an exception
        /// </summary>
        public static IList<StackTrace> GetExceptionStackTraces(Exception rethrow) {
            List<StackTrace> result;
            return TryGetAssociatedStackTraces(rethrow, out result) ? result : null;
        }

        private static void AssociateStackTraces(Exception e, List<StackTrace> traces) {
            e.Data[prevStackTraces] = traces;
        }

        private static bool TryGetAssociatedStackTraces(Exception e, out List<StackTrace> traces) {
            traces = e.Data[prevStackTraces] as List<StackTrace>;
            return traces != null;
        }        
#else
        public static Exception UpdateForRethrow(Exception rethrow) {
            return rethrow;
        }
#endif
    }
}
