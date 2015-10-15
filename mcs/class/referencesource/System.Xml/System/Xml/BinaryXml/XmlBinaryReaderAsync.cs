//------------------------------------------------------------------------------
// <copyright file="XmlBinaryWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Schema;
using System.Threading.Tasks;

namespace System.Xml {

    internal sealed partial class XmlSqlBinaryReader : XmlReader, IXmlNamespaceResolver {

        public override Task<string> GetValueAsync() {
            throw new NotSupportedException();
        }

        public override Task<bool> ReadAsync() {
            throw new NotSupportedException();
        }

        public override Task<object> ReadContentAsObjectAsync() {
            throw new NotSupportedException();
        }

        public override Task<object> ReadContentAsAsync(Type returnType, IXmlNamespaceResolver namespaceResolver) {
            throw new NotSupportedException();
        }

        public override Task<XmlNodeType> MoveToContentAsync() {
            throw new NotSupportedException();
        }

        public override Task<string> ReadContentAsStringAsync() {
            throw new NotSupportedException();
        }

        public override Task<int> ReadContentAsBase64Async(byte[] buffer, int index, int count) {
            throw new NotSupportedException();
        }

        public override Task<object> ReadElementContentAsAsync(Type returnType, IXmlNamespaceResolver namespaceResolver) {
            throw new NotSupportedException();
        }

        public override Task<object> ReadElementContentAsObjectAsync() {
            throw new NotSupportedException();
        }

        public override Task<int> ReadElementContentAsBinHexAsync(byte[] buffer, int index, int count) {
            throw new NotSupportedException();
        }

        public override Task<string> ReadInnerXmlAsync() {
            throw new NotSupportedException();
        }

        public override Task<string> ReadOuterXmlAsync() {
            throw new NotSupportedException();
        }

        public override Task<int> ReadValueChunkAsync(char[] buffer, int index, int count) {
            throw new NotSupportedException();
        }

        public override Task SkipAsync() {
            throw new NotSupportedException();
        }

        public override Task<string> ReadElementContentAsStringAsync() {
            throw new NotSupportedException();
        }

    }
}
