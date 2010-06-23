// This source code is dual-licensed under the Apache License, version
// 2.0, and the Mozilla Public License, version 1.1.
//
// The APL v2.0:
//
//---------------------------------------------------------------------------
//   Copyright (C) 2007-2010 LShift Ltd., Cohesive Financial
//   Technologies LLC., and Rabbit Technologies Ltd.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//---------------------------------------------------------------------------
//
// The MPL v1.1:
//
//---------------------------------------------------------------------------
//   The contents of this file are subject to the Mozilla Public License
//   Version 1.1 (the "License"); you may not use this file except in
//   compliance with the License. You may obtain a copy of the License at
//   http://www.rabbitmq.com/mpl.html
//
//   Software distributed under the License is distributed on an "AS IS"
//   basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//   License for the specific language governing rights and limitations
//   under the License.
//
//   The Original Code is The RabbitMQ .NET Client.
//
//   The Initial Developers of the Original Code are LShift Ltd,
//   Cohesive Financial Technologies LLC, and Rabbit Technologies Ltd.
//
//   Portions created before 22-Nov-2008 00:00:00 GMT by LShift Ltd,
//   Cohesive Financial Technologies LLC, or Rabbit Technologies Ltd
//   are Copyright (C) 2007-2008 LShift Ltd, Cohesive Financial
//   Technologies LLC, and Rabbit Technologies Ltd.
//
//   Portions created by LShift Ltd are Copyright (C) 2007-2010 LShift
//   Ltd. Portions created by Cohesive Financial Technologies LLC are
//   Copyright (C) 2007-2010 Cohesive Financial Technologies
//   LLC. Portions created by Rabbit Technologies Ltd are Copyright
//   (C) 2007-2010 Rabbit Technologies Ltd.
//
//   All Rights Reserved.
//
//   Contributor(s): ______________________________________.
//
//---------------------------------------------------------------------------
using System;
using System.Collections;
using System.Threading;

namespace RabbitMQ.Util {
    ///<summary>A thread-safe single-assignment reference cell.</summary>
    ///<remarks>
    ///A fresh BlockingCell holds no value (is empty). Any thread
    ///reading the Value property when the cell is empty will block
    ///until a value is made available by some other thread. The Value
    ///property can only be set once - on the first call, the
    ///BlockingCell is considered full, and made immutable. Further
    ///attempts to set Value result in a thrown
    ///InvalidOperationException.
    ///</remarks>
    public class BlockingCell {
        private bool m_valueSet = false;
        private object m_value = null;

        ///<summary>Construct an empty BlockingCell.</summary>
        public BlockingCell() {}

        ///<summary>Retrieve the cell's value, blocking if none exists
        ///at present, or supply a value to an empty cell, thereby
        ///filling it.</summary>
        /// <exception cref="InvalidOperationException" />
        public object Value {
            get {
                lock (this) {
                    while (!m_valueSet) {
                        Monitor.Wait(this);
                    }
                    return m_value;
                }
            }

            set {
                lock (this) {
                    if (m_valueSet) {
                        throw new InvalidOperationException("Setting BlockingCell value twice forbidden");
                    }
                    m_value = value;
                    m_valueSet = true;
                    Monitor.PulseAll(this);
                }
            }
        }
        
        ///<summary>Retrieve the cell's value, waiting for the given
        ///timeout if no value is immediately available.</summary>
        ///<remarks>
        ///<para>
        /// If a value is present in the cell at the time the call is
        /// made, the call will return immediately. Otherwise, the
        /// calling thread blocks until either a value appears, or
        /// millisecondsTimeout milliseconds have elapsed.
        ///</para>
        ///<para>
        /// Returns true in the case that the value was available
        /// before the timeout, in which case the out parameter
        /// "result" is set to the value itself.
        ///</para>
        ///<para>
        /// If no value was available before the timeout, returns
        /// false, and sets "result" to null.
        ///</para>
        ///<para>
        /// A timeout of -1 (i.e. System.Threading.Timeout.Infinite)
        /// will be interpreted as a command to wait for an
        /// indefinitely long period of time for the cell's value to
        /// become available. See the MSDN documentation for
        /// System.Threading.Monitor.Wait(object,int).
        ///</para>
        ///</remarks>
        public bool GetValue(int millisecondsTimeout, out object result)
        {

            
            lock (this) {
                if (!m_valueSet) {
                    Monitor.Wait(this, validatedTimeout(millisecondsTimeout));
                    if (!m_valueSet) {
                        result = null;
                        return false;
                    }
                }
                result = m_value;
                return true;
            }
        }
        
        ///<summary>Return valid timeout value</summary>
        ///<remarks>If value of the parameter is less then zero, return 0
        ///to mean infinity</remarks>
        public static int validatedTimeout(int timeout)
        {
            return (timeout != Timeout.Infinite)
                && (timeout < 0) ? 0 : timeout;
        }
    }
}
