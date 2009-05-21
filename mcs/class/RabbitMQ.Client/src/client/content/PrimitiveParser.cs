// This source code is dual-licensed under the Apache License, version
// 2.0, and the Mozilla Public License, version 1.1.
//
// The APL v2.0:
//
//---------------------------------------------------------------------------
//   Copyright (C) 2007-2009 LShift Ltd., Cohesive Financial
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
//   Portions created by LShift Ltd are Copyright (C) 2007-2009 LShift
//   Ltd. Portions created by Cohesive Financial Technologies LLC are
//   Copyright (C) 2007-2009 Cohesive Financial Technologies
//   LLC. Portions created by Rabbit Technologies Ltd are Copyright
//   (C) 2007-2009 Rabbit Technologies Ltd.
//
//   All Rights Reserved.
//
//   Contributor(s): ______________________________________.
//
//---------------------------------------------------------------------------
using System;
using System.Net;
using System.Text;
using System.IO;

using RabbitMQ.Client;
using RabbitMQ.Util;

namespace RabbitMQ.Client.Content {
    ///<summary>Utility class for extracting typed values from strings.</summary>
    public class PrimitiveParser {
        ///<summary>Causes ProtocolViolationException to be thrown;
        ///called by the various "Parse*" methods when a syntax error
        ///is detected.</summary>
        ///<exception cref="ProtocolViolationException"/>
        public static void InvalidConversion(string target, object source) {
            throw new ProtocolViolationException(string.Format("Invalid conversion to {0}: {1}",
                                                               target, source));
        }

        ///<summary>Attempt to parse a string representation of a bool.</summary>
        ///<exception cref="ProtocolViolationException"/>
        public static bool ParseBool(string value) {
            try {
                return bool.Parse(value);
            } catch {
                InvalidConversion("bool", value);
                return false;
            }
        }

        ///<summary>Attempt to parse a string representation of an int.</summary>
        ///<exception cref="ProtocolViolationException"/>
        public static int ParseInt(string value) {
            try {
                return int.Parse(value);
            } catch {
                InvalidConversion("int", value);
                return 0;
            }
        }

        ///<summary>Attempt to parse a string representation of a short.</summary>
        ///<exception cref="ProtocolViolationException"/>
        public static short ParseShort(string value) {
            try {
                return short.Parse(value);
            } catch {
                InvalidConversion("short", value);
                return 0;
            }
        }

        ///<summary>Attempt to parse a string representation of a byte.</summary>
        ///<exception cref="ProtocolViolationException"/>
        public static byte ParseByte(string value) {
            try {
                return byte.Parse(value);
            } catch {
                InvalidConversion("byte", value);
                return 0;
            }
        }

        ///<summary>Attempt to parse a string representation of a long.</summary>
        ///<exception cref="ProtocolViolationException"/>
        public static long ParseLong(string value) {
            try {
                return long.Parse(value);
            } catch {
                InvalidConversion("long", value);
                return 0;
            }
        }

        ///<summary>Attempt to parse a string representation of a float.</summary>
        ///<exception cref="ProtocolViolationException"/>
        public static float ParseFloat(string value) {
            try {
                return float.Parse(value);
            } catch {
                InvalidConversion("float", value);
                return 0;
            }
        }

        ///<summary>Attempt to parse a string representation of a double.</summary>
        ///<exception cref="ProtocolViolationException"/>
        public static double ParseDouble(string value) {
            try {
                return double.Parse(value);
            } catch {
                InvalidConversion("double", value);
                return 0;
            }
        }
    }
}
