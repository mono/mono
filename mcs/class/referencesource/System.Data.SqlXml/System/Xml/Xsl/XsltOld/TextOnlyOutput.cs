//------------------------------------------------------------------------------
// <copyright file="TextOnlyOutput.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.XsltOld {
    using Res = System.Xml.Utils.Res;
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;
    using System.Text;
    using System.Collections;

    internal class TextOnlyOutput : RecordOutput {
        private     Processor  processor;
        private     TextWriter writer;

        internal XsltOutput Output {
            get { return this.processor.Output; }
        }

		public TextWriter Writer {
			get { return this.writer; }
		}

        //
        // Constructor
        //

        internal TextOnlyOutput(Processor processor, Stream stream) {
            if (stream == null) {
                throw new ArgumentNullException("stream");
            }

            this.processor  = processor;
            this.writer     = new StreamWriter(stream, Output.Encoding);
        }

        internal TextOnlyOutput(Processor processor, TextWriter writer) {
            if (writer == null) {
                throw new ArgumentNullException("writer");
            }

            this.processor  = processor;
            this.writer     = writer;
        }

        //
        // RecordOutput interface method implementation
        //

        public Processor.OutputResult RecordDone(RecordBuilder record) {
            BuilderInfo mainNode       = record.MainNode;

            switch (mainNode.NodeType) {
            case XmlNodeType.Text:
            case XmlNodeType.Whitespace:
            case XmlNodeType.SignificantWhitespace:
                this.writer.Write(mainNode.Value);
                break;
            default:
                break;
            }

            record.Reset();
            return Processor.OutputResult.Continue;
        }

        public void TheEnd() {
            this.writer.Flush();
        }
    }
}
