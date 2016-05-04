
using System;
using System.Xml;
using System.Xml.Schema;
using System.Diagnostics;
using System.Collections;

using System.Threading.Tasks;

namespace System.Xml {

    internal partial class XmlWrappingReader : XmlReader, IXmlLineInfo {

        public override Task<string> GetValueAsync() {
            return reader.GetValueAsync();
        }

        public override Task< bool > ReadAsync() {
            return reader.ReadAsync();
        }

        public override Task SkipAsync() {
            return reader.SkipAsync();
        }

    }
}
    
