//------------------------------------------------------------------------------
// <copyright file="DiagnosticsSwitches.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Xml {
    using System.Diagnostics;

#if DEBUG
    public static class DiagnosticsSwitches {
#else
    internal static class DiagnosticsSwitches {
#endif
        private static volatile BooleanSwitch xmlSchemaContentModel;
        private static volatile TraceSwitch xmlSchema;
        private static volatile BooleanSwitch keepTempFiles;
        private static volatile BooleanSwitch pregenEventLog;
        private static volatile TraceSwitch xmlSerialization;
        private static volatile TraceSwitch xslTypeInference;
        private static volatile BooleanSwitch nonRecursiveTypeLoading;

        public static BooleanSwitch XmlSchemaContentModel {
            get {
                if (xmlSchemaContentModel == null) {
                    xmlSchemaContentModel = new BooleanSwitch("XmlSchemaContentModel", "Enable tracing for the XmlSchema content model.");
                }
                return xmlSchemaContentModel;
            }
        }

        public static TraceSwitch XmlSchema {
            get {
                if (xmlSchema == null) {
                    xmlSchema = new TraceSwitch("XmlSchema", "Enable tracing for the XmlSchema class.");
                }
                return xmlSchema;
            }
        }

        public static BooleanSwitch KeepTempFiles {
            get {
                if (keepTempFiles == null) {
                    keepTempFiles = new BooleanSwitch("XmlSerialization.Compilation", "Keep XmlSerialization generated (temp) files.");
                }
                return keepTempFiles;
            }
        }

        public static BooleanSwitch PregenEventLog {
            get {
                if (pregenEventLog == null) {
                    pregenEventLog = new BooleanSwitch("XmlSerialization.PregenEventLog", "Log failures while loading pre-generated XmlSerialization assembly.");
                }
                return pregenEventLog;
            }
        }

        public static TraceSwitch XmlSerialization {
            get {
                if (xmlSerialization == null) {
                    xmlSerialization = new TraceSwitch("XmlSerialization", "Enable tracing for the System.Xml.Serialization component.");
                }
                return xmlSerialization;
            }
        }

        public static TraceSwitch XslTypeInference {
            get {
                if (xslTypeInference == null) {
                    xslTypeInference = new TraceSwitch("XslTypeInference", "Enable tracing for the XSLT type inference algorithm.");
                }
                return xslTypeInference;
            }
        }
        public static BooleanSwitch NonRecursiveTypeLoading {
            get {
                if (nonRecursiveTypeLoading == null) {
                    nonRecursiveTypeLoading = new BooleanSwitch("XmlSerialization.NonRecursiveTypeLoading", "Turn on non-recursive algorithm generating XmlMappings for CLR types.");
                }
                return nonRecursiveTypeLoading;
            }
        }
    }
}
