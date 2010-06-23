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
using System.Collections;
using System.Threading;

namespace RabbitMQ.Util {
    ///<summary>Used internally by class Either.</summary>
    public enum EitherAlternative {
        Left,
        Right
    }

    ///<summary>Models the disjoint union of two alternatives, a
    ///"left" alternative and a "right" alternative.</summary>
    ///<remarks>Borrowed from ML, Haskell etc.</remarks>
    public class Either {
        ///<summary>Records which alternative this instance represents.</summary>
        private EitherAlternative m_alternative;
        ///<summary>Holds the value this instance carries.</summary>
        private object m_value;

        ///<summary>Private constructor. Use the static methods Left, Right instead.</summary>
        private Either(EitherAlternative alternative, object value) {
            m_alternative = alternative;
            m_value = value;
        }

        ///<summary>Constructs an Either instance representing a Left alternative.</summary>
        public static Either Left(object value) {
            return new Either(EitherAlternative.Left, value);
        }

        ///<summary>Constructs an Either instance representing a Right alternative.</summary>
        public static Either Right(object value) {
            return new Either(EitherAlternative.Right, value);
        }

        ///<summary>Retrieve the alternative represented by this instance.</summary>
        public EitherAlternative Alternative {
            get {
                return m_alternative;
            }
        }

        ///<summary>Retrieve the value carried by this instance.</summary>
        public object Value {
            get {
                return m_value;
            }
        }
    }
}
