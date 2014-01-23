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

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Scripting.Utils {
    public static class ExceptionUtils {
        public static ArgumentOutOfRangeException MakeArgumentOutOfRangeException(string paramName, object actualValue, string message) {
#if SILVERLIGHT || WP75 // ArgumentOutOfRangeException ctor overload
            throw new ArgumentOutOfRangeException(paramName, string.Format("{0} (actual value is '{1}')", message, actualValue));
#else
            throw new ArgumentOutOfRangeException(paramName, actualValue, message);
#endif
        }

        public static ArgumentNullException MakeArgumentItemNullException(int index, string arrayName) {
            return new ArgumentNullException(String.Format("{0}[{1}]", arrayName, index));
        }

#if FEATURE_REMOTING
        public static object GetData(this Exception e, object key) {
            return e.Data[key];
        }

        public static void SetData(this Exception e, object key, object data) {
            e.Data[key] = data;
        }

        public static void RemoveData(this Exception e, object key) {
            e.Data.Remove(key);
        }
#else

#if WP75
        private static WeakDictionary<Exception, List<KeyValuePair<object, object>>> _exceptionData;
#else
        private static ConditionalWeakTable<Exception, List<KeyValuePair<object, object>>> _exceptionData;
#endif

        public static void SetData(this Exception e, object key, object value) {
            if (_exceptionData == null) {
#if WP75
                Interlocked.CompareExchange(ref _exceptionData, new WeakDictionary<Exception, List<KeyValuePair<object, object>>>(), null);
#else
                Interlocked.CompareExchange(ref _exceptionData, new ConditionalWeakTable<Exception, List<KeyValuePair<object, object>>>(), null);
#endif
            }

            lock (_exceptionData) {
                var data = _exceptionData.GetOrCreateValue(e);
            
                int index = data.FindIndex(entry => entry.Key == key);
                if (index >= 0) {
                    data[index] = new KeyValuePair<object, object>(key, value);
                } else {
                    data.Add(new KeyValuePair<object, object>(key, value));
                }
            }
        }

        public static object GetData(this Exception e, object key) {
            if (_exceptionData == null) {
                return null;
            }

            lock (_exceptionData) {
                List<KeyValuePair<object, object>> data;
                if (!_exceptionData.TryGetValue(e, out data)) {
                    return null;
                }

                return data.FirstOrDefault(entry => entry.Key == key).Value;
            }
        }

        public static void RemoveData(this Exception e, object key) {
            if (_exceptionData == null) {
                return;
            }

            lock (_exceptionData) {
                List<KeyValuePair<object, object>> data;
                if (!_exceptionData.TryGetValue(e, out data)) {
                    return;
                }

                int index = data.FindIndex(entry => entry.Key == key);
                if (index >= 0) {
                    data.RemoveAt(index);
                }
            }
        }
#endif
    }
}
