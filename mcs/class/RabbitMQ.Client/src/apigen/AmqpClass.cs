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
using System.Xml;

namespace RabbitMQ.Client.Apigen {
    public class AmqpClass: AmqpEntity {
        public ArrayList Methods;
        public ArrayList Fields;

        public AmqpClass(XmlNode n)
            : base(n)
        {
            Methods = new ArrayList();
            foreach (XmlNode m in n.SelectNodes("method")) {
                Methods.Add(new AmqpMethod(m));
            }
            Fields = new ArrayList();
            foreach (XmlNode f in n.SelectNodes("field")) {
                Fields.Add(new AmqpField(f));
            }
        }

        public int Index {
            get {
                return GetInt("@index");
            }
        }

        public bool NeedsProperties {
            get {
                foreach (AmqpMethod m in Methods) {
                    if (m.HasContent) return true;
                }
                return false;
            }
        }

        public AmqpMethod MethodNamed(string name) {
            foreach (AmqpMethod m in Methods) {
                if (m.Name == name) {
                    return m;
                }
            }
            return null;
        }
    }
}
