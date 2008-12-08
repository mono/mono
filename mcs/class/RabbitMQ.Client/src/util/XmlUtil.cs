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
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace RabbitMQ.Util {
    ///<summary>Miscellaneous helpful XML utilities.</summary>
    public class XmlUtil {
        ///<summary>Private constructor - this class has no instances</summary>
        private XmlUtil() {}

        ///<summary>Serializes an arbitrary serializable object to an
        ///XML document.</summary>
        public static XmlDocument SerializeObject(Type serializationType, object obj) {
            StringWriter writer = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(serializationType);
            serializer.Serialize(writer, obj);
            XmlDocument doc = new XmlDocument();
            doc.Load(new StringReader(writer.ToString()));
            return doc;
        }

        ///<summary>Constructs an indenting XmlTextWriter that writes to a
        ///fresh MemoryStream.</summary>
        public static XmlTextWriter CreateIndentedXmlWriter() {
            return CreateIndentedXmlWriter(new MemoryStream());
        }

        ///<summary>Constructs an indenting XmlTextWriter that writes to
        ///the supplied stream.</summary>
        public static XmlTextWriter CreateIndentedXmlWriter(Stream stream) {
            XmlTextWriter w = new XmlTextWriter(stream, Encoding.UTF8);
            w.Formatting = Formatting.Indented;
            return w;
        }

        ///<summary>Constructs an indenting XmlTextWriter that writes to
        ///the supplied file name.</summary>
        public static XmlTextWriter CreateIndentedXmlWriter(string path) {
            XmlTextWriter w = new XmlTextWriter(path, Encoding.UTF8);
            w.Formatting = Formatting.Indented;
            return w;
        }
    }
}
