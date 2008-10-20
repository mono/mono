// This source code is dual-licensed under the Apache License, version
// 2.0, and the Mozilla Public License, version 1.1.
//
// The APL v2.0:
//
//---------------------------------------------------------------------------
//   Copyright (C) 2007, 2008 LShift Ltd., Cohesive Financial
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
//   The Initial Developers of the Original Code are LShift Ltd.,
//   Cohesive Financial Technologies LLC., and Rabbit Technologies Ltd.
//
//   Portions created by LShift Ltd., Cohesive Financial Technologies
//   LLC., and Rabbit Technologies Ltd. are Copyright (C) 2007, 2008
//   LShift Ltd., Cohesive Financial Technologies LLC., and Rabbit
//   Technologies Ltd.;
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
    ///<summary>A classic counting semaphore.</summary>
    ///<remarks>
    /// The .NET framework does not introduce a counting semaphore
    /// until framework release 2.0. Consequently, we implement one
    /// here, for the benefit of .NET 1.1.
    ///</remarks>
    public class Semaphore {
        private int m_count;

        ///<summary>Create a Semaphore, with its counter initialized
        ///to 1.</summary>
        public Semaphore()
            : this(1)
        {}

        ///<summary>Create a Semaphore, with its counter initialized
        ///to the value passed in.</summary>
        public Semaphore(int initialCount)
        {
            m_count = initialCount;
        }

        ///<summary>Acquire a single resource, decrementing the count by one.</summary>
        ///<remarks>
        /// Not interruptable - will retry forever until a resource comes available.
        ///</remarks>
        public void Wait() {
            lock (this) {
                while (true) {
                    if (m_count > 0) {
                        m_count--;
                        return;
                    } else {
                        Monitor.Wait(this);
                    }
                }
            }
        }

        ///<summary>Acquire a single resource, decrementing the count
        ///by one and returning true, if a resource is available;
        ///otherwise, return false immediately.</summary>
        public bool TryWait() {
            lock (this) {
                if (m_count > 0) {
                    m_count--;
                    return true;
                } else {
                    return false;
                }
            }
        }

        ///<summary>Release a single resource, incrementing the count
        ///by one.</summary>
        public void Release() {
            lock (this) {
                m_count++;
                Monitor.Pulse(this);
            }
        }

        ///<summary>Retrieve the current semaphore value.</summary>
        ///<remarks>
        /// Consider carefully whether this property is actually what
        /// you want - usually, using this property is wrong, and
        /// either Wait() or TryWait() is the correct choice.
        ///</remarks>
        public int Value {
            get {
                lock (this) {
                    return m_count;
                }
            }
        }
    }
}
