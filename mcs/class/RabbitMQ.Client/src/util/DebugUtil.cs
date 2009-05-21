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
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections;

namespace RabbitMQ.Util {
    ///<summary>Miscellaneous debugging and development utilities.</summary>
    ///<remarks>
    ///Not part of the public API.
    ///</remarks>
    public class DebugUtil {
        ///<summary>Private constructor - this class has no instances</summary>
        private DebugUtil() {}

        ///<summary>Print a hex dump of the supplied bytes to stdout.</summary>
        public static void Dump(byte[] bytes) {
            Dump(bytes, Console.Out);
        }

        ///<summary>Print a hex dump of the supplied bytes to the supplied TextWriter.</summary>
        public static void Dump(byte[] bytes, TextWriter writer) {
            int rowlen = 16;

            for (int count = 0; count < bytes.Length; count += rowlen) {
                int thisRow = Math.Min(bytes.Length - count, rowlen);

                writer.Write(String.Format("{0:X8}: ", count));
                for (int i = 0; i < thisRow; i++) {
                    writer.Write(String.Format("{0:X2}", bytes[count + i]));
                }
                for (int i = 0; i < (rowlen - thisRow); i++) {
                    writer.Write("  ");
                }
                writer.Write("  ");
                for (int i = 0; i < thisRow; i++) {
                    if (bytes[count + i] >= 32 &&
                        bytes[count + i] < 128)
                        {
                            writer.Write((char) bytes[count + i]);
                        } else {
                            writer.Write('.');
                        }
                }
                writer.WriteLine();
            }
            if (bytes.Length % 16 != 0) {
                writer.WriteLine(String.Format("{0:X8}: ", bytes.Length));
            }
        }

	///<summary>Prints an indented key/value pair; used by DumpProperties()</summary>
        ///<remarks>Recurses into the value using DumpProperties().</remarks>
	public static void DumpKeyValue(string key, object value, TextWriter writer, int indent) {
	    string prefix = new String(' ', indent + 2) + key + ": ";
	    writer.Write(prefix);
	    DumpProperties(value, writer, indent + 2);
	}

        ///<summary>Dump properties of objects to the supplied writer.</summary>
        public static void DumpProperties(object value, TextWriter writer, int indent) {
            if (value == null) {
                writer.WriteLine("(null)");
            } else if (value is string) {
                writer.WriteLine("\"" + ((string) value).Replace("\"", "\\\"") + "\"");
            } else if (value is byte[]) {
                writer.WriteLine("byte[]");
                Dump((byte[]) value);
            } else if (value is ValueType) {
                writer.WriteLine(value);
	    } else if (value is IDictionary) {
                Type t = value.GetType();
                writer.WriteLine(t.FullName);
		foreach (DictionaryEntry entry in ((IDictionary) value)) {
		    DumpKeyValue((string) entry.Key, entry.Value, writer, indent);
		}
            } else if (value is IEnumerable) {
                writer.WriteLine("IEnumerable");
                int index = 0;
                foreach (object v in ((IEnumerable) value)) {
                    DumpKeyValue(index.ToString(), v, writer, indent);
                    index++;
                }
            } else {
                Type t = value.GetType();
                writer.WriteLine(t.FullName);
                foreach (PropertyInfo pi in t.GetProperties(BindingFlags.Instance
                                                            | BindingFlags.Public
                                                            | BindingFlags.DeclaredOnly))
                {
                    if (pi.GetIndexParameters().Length == 0) {
			DumpKeyValue(pi.Name, pi.GetValue(value, new object[0]), writer, indent);
                    }
                }
            }
        }
    }
}
