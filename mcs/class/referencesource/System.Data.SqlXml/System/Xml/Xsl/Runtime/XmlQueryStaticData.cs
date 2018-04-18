//------------------------------------------------------------------------------
// <copyright file="XmlQueryStaticData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml.Xsl.IlGen;
using System.Xml.Xsl.Qil;

namespace System.Xml.Xsl.Runtime {
    /// <summary>
    /// Contains all static data that is used by the runtime.
    /// </summary>
    internal class XmlQueryStaticData {
        // Name of the field to serialize to
        public const string DataFieldName   = "staticData";
        public const string TypesFieldName  = "ebTypes";

        // Format version marker to support versioning: (major << 8) | minor
        private const int CurrentFormatVersion = (0 << 8) | 0;

        private XmlWriterSettings defaultWriterSettings;
        private IList<WhitespaceRule> whitespaceRules;
        private string[] names;
        private StringPair[][] prefixMappingsList;
        private Int32Pair[] filters;
        private XmlQueryType[] types;
        private XmlCollation[] collations;
        private string[] globalNames;
        private EarlyBoundInfo[] earlyBound;

        /// <summary>
        /// Constructor.
        /// </summary>
        public XmlQueryStaticData(XmlWriterSettings defaultWriterSettings, IList<WhitespaceRule> whitespaceRules, StaticDataManager staticData) {
            Debug.Assert(defaultWriterSettings != null && staticData != null);
            this.defaultWriterSettings = defaultWriterSettings;
            this.whitespaceRules = whitespaceRules;
            this.names = staticData.Names;
            this.prefixMappingsList = staticData.PrefixMappingsList;
            this.filters = staticData.NameFilters;
            this.types = staticData.XmlTypes;
            this.collations = staticData.Collations;
            this.globalNames = staticData.GlobalNames;
            this.earlyBound = staticData.EarlyBound;

        #if DEBUG
            // Round-trip check
            byte[] data;
            Type[] ebTypes;
            this.GetObjectData(out data, out ebTypes);
            XmlQueryStaticData copy = new XmlQueryStaticData(data, ebTypes);

            this.defaultWriterSettings = copy.defaultWriterSettings;
            this.whitespaceRules = copy.whitespaceRules;
            this.names = copy.names;
            this.prefixMappingsList = copy.prefixMappingsList;
            this.filters = copy.filters;
            this.types = copy.types;
            this.collations = copy.collations;
            this.globalNames = copy.globalNames;
            this.earlyBound = copy.earlyBound;
        #endif
        }

        /// <summary>
        /// Deserialize XmlQueryStaticData object from a byte array.
        /// </summary>
        public XmlQueryStaticData(byte[] data, Type[] ebTypes) {
            MemoryStream dataStream = new MemoryStream(data, /*writable:*/false);
            XmlQueryDataReader dataReader = new XmlQueryDataReader(dataStream);
            int length;

            // Read a format version
            int formatVersion = dataReader.ReadInt32Encoded();

            // Changes in the major part of version are not supported
            if ((formatVersion & ~0xff) > CurrentFormatVersion)
                throw new NotSupportedException();

            // XmlWriterSettings defaultWriterSettings;
            defaultWriterSettings = new XmlWriterSettings(dataReader);

            // IList<WhitespaceRule> whitespaceRules;
            length = dataReader.ReadInt32();
            if (length != 0) {
                this.whitespaceRules = new WhitespaceRule[length];
                for (int idx = 0; idx < length; idx++) {
                    this.whitespaceRules[idx] = new WhitespaceRule(dataReader);
                }
            }

            // string[] names;
            length = dataReader.ReadInt32();
            if (length != 0) {
                this.names = new string[length];
                for (int idx = 0; idx < length; idx++) {
                    this.names[idx] = dataReader.ReadString();
                }
            }

            // StringPair[][] prefixMappingsList;
            length = dataReader.ReadInt32();
            if (length != 0) {
                this.prefixMappingsList = new StringPair[length][];
                for (int idx = 0; idx < length; idx++) {
                    int length2 = dataReader.ReadInt32();
                    this.prefixMappingsList[idx] = new StringPair[length2];
                    for (int idx2 = 0; idx2 < length2; idx2++) {
                        this.prefixMappingsList[idx][idx2] = new StringPair(dataReader.ReadString(), dataReader.ReadString());
                    }
                }
            }

            // Int32Pair[] filters;
            length = dataReader.ReadInt32();
            if (length != 0) {
                this.filters = new Int32Pair[length];
                for (int idx = 0; idx < length; idx++) {
                    this.filters[idx] = new Int32Pair(dataReader.ReadInt32Encoded(), dataReader.ReadInt32Encoded());
                }
            }

            // XmlQueryType[] types;
            length = dataReader.ReadInt32();
            if (length != 0) {
                this.types = new XmlQueryType[length];
                for (int idx = 0; idx < length; idx++) {
                    this.types[idx] = XmlQueryTypeFactory.Deserialize(dataReader);
                }
            }

            // XmlCollation[] collations;
            length = dataReader.ReadInt32();
            if (length != 0) {
                this.collations = new XmlCollation[length];
                for (int idx = 0; idx < length; idx++) {
                    this.collations[idx] = new XmlCollation(dataReader);
                }
            }

            // string[] globalNames;
            length = dataReader.ReadInt32();
            if (length != 0) {
                this.globalNames = new string[length];
                for (int idx = 0; idx < length; idx++) {
                    this.globalNames[idx] = dataReader.ReadString();
                }
            }

            // EarlyBoundInfo[] earlyBound;
            length = dataReader.ReadInt32();
            if (length != 0) {
                this.earlyBound = new EarlyBoundInfo[length];
                for (int idx = 0; idx < length; idx++) {
                    this.earlyBound[idx] = new EarlyBoundInfo(dataReader.ReadString(), ebTypes[idx]);
                }
            }

            Debug.Assert(formatVersion != CurrentFormatVersion || dataReader.Read() == -1, "Extra data at the end of the stream");
            dataReader.Close();
        }

        /// <summary>
        /// Serialize XmlQueryStaticData object into a byte array.
        /// </summary>
        public void GetObjectData(out byte[] data, out Type[] ebTypes) {
            MemoryStream dataStream = new MemoryStream(4096);
            XmlQueryDataWriter dataWriter = new XmlQueryDataWriter(dataStream);

            // First put the format version
            dataWriter.WriteInt32Encoded(CurrentFormatVersion);

            // XmlWriterSettings defaultWriterSettings;
            defaultWriterSettings.GetObjectData(dataWriter);

            // IList<WhitespaceRule> whitespaceRules;
            if (this.whitespaceRules == null) {
                dataWriter.Write(0);
            }
            else {
                dataWriter.Write(this.whitespaceRules.Count);
                foreach (WhitespaceRule rule in this.whitespaceRules) {
                    rule.GetObjectData(dataWriter);
                }
            }

            // string[] names;
            if (this.names == null) {
                dataWriter.Write(0);
            }
            else {
                dataWriter.Write(this.names.Length);
                foreach (string name in this.names) {
                    dataWriter.Write(name);
                }
            }

            // StringPair[][] prefixMappingsList;
            if (this.prefixMappingsList == null) {
                dataWriter.Write(0);
            }
            else {
                dataWriter.Write(this.prefixMappingsList.Length);
                foreach (StringPair[] mappings in this.prefixMappingsList) {
                    dataWriter.Write(mappings.Length);
                    foreach (StringPair mapping in mappings) {
                        dataWriter.Write(mapping.Left);
                        dataWriter.Write(mapping.Right);
                    }
                }
            }

            // Int32Pair[] filters;
            if (this.filters == null) {
                dataWriter.Write(0);
            }
            else {
                dataWriter.Write(this.filters.Length);
                foreach (Int32Pair filter in this.filters) {
                    dataWriter.WriteInt32Encoded(filter.Left);
                    dataWriter.WriteInt32Encoded(filter.Right);
                }
            }

            // XmlQueryType[] types;
            if (this.types == null) {
                dataWriter.Write(0);
            }
            else {
                dataWriter.Write(this.types.Length);
                foreach (XmlQueryType type in this.types) {
                    XmlQueryTypeFactory.Serialize(dataWriter, type);                    
                }
            }

            // XmlCollation[] collations;
            if (collations == null) {
                dataWriter.Write(0);
            }
            else {
                dataWriter.Write(this.collations.Length);
                foreach (XmlCollation collation in this.collations) {
                    collation.GetObjectData(dataWriter);
                }
            }

            // string[] globalNames;
            if (this.globalNames == null) {
                dataWriter.Write(0);
            }
            else {
                dataWriter.Write(this.globalNames.Length);
                foreach (string name in this.globalNames) {
                    dataWriter.Write(name);
                }
            }

            // EarlyBoundInfo[] earlyBound;
            if (this.earlyBound == null) {
                dataWriter.Write(0);
                ebTypes = null;
            }
            else {
                dataWriter.Write(this.earlyBound.Length);
                ebTypes = new Type[this.earlyBound.Length];
                int idx = 0;
                foreach (EarlyBoundInfo info in this.earlyBound) {
                    dataWriter.Write(info.NamespaceUri);
                    ebTypes[idx++] = info.EarlyBoundType;
                }
            }

            dataWriter.Close();
            data = dataStream.ToArray();
        }

        /// <summary>
        /// Return the default writer settings.
        /// </summary>
        public XmlWriterSettings DefaultWriterSettings {
            get { return this.defaultWriterSettings; }
        }

        /// <summary>
        /// Return the rules used for whitespace stripping/preservation.
        /// </summary>
        public IList<WhitespaceRule> WhitespaceRules {
            get { return this.whitespaceRules; }
        }

        /// <summary>
        /// Return array of names used by this query.
        /// </summary>
        public string[] Names {
            get { return this.names; }
        }

        /// <summary>
        /// Return array of prefix mappings used by this query.
        /// </summary>
        public StringPair[][] PrefixMappingsList {
            get { return this.prefixMappingsList; }
        }

        /// <summary>
        /// Return array of name filter specifications used by this query.
        /// </summary>
        public Int32Pair[] Filters {
            get { return this.filters; }
        }

        /// <summary>
        /// Return array of types used by this query.
        /// </summary>
        public XmlQueryType[] Types {
            get { return this.types; }
        }

        /// <summary>
        /// Return array of collations used by this query.
        /// </summary>
        public XmlCollation[] Collations {
            get { return this.collations; }
        }

        /// <summary>
        /// Return names of all global variables and parameters used by this query.
        /// </summary>
        public string[] GlobalNames {
            get { return this.globalNames; }
        }

        /// <summary>
        /// Return array of early bound object information related to this query.
        /// </summary>
        public EarlyBoundInfo[] EarlyBound {
            get { return this.earlyBound; }
        }
    }

    /// <summary>
    /// Subclass of BinaryReader used to serialize query static data.
    /// </summary>
    internal class XmlQueryDataReader : BinaryReader {
        public XmlQueryDataReader(Stream input) : base(input) { }

        /// <summary>
        /// Read in a 32-bit integer in compressed format.
        /// </summary>
        public int ReadInt32Encoded() {
            return Read7BitEncodedInt();
        }

        /// <summary>
        /// Read a string value from the stream. Value can be null.
        /// </summary>
        public string ReadStringQ() {
            return ReadBoolean() ? ReadString() : null;
        }

        /// <summary>
        /// Read a signed byte value from the stream and check if it belongs to the given diapason.
        /// </summary>
        public sbyte ReadSByte(sbyte minValue, sbyte maxValue) {
            sbyte value = ReadSByte();
            if (value < minValue)
                throw new ArgumentOutOfRangeException("minValue");
            if (maxValue < value)
                throw new ArgumentOutOfRangeException("maxValue");

            return value;
        }
    }

    /// <summary>
    /// Subclass of BinaryWriter used to deserialize query static data.
    /// </summary>
    internal class XmlQueryDataWriter : BinaryWriter {
        public XmlQueryDataWriter(Stream output) : base(output) { }

        /// <summary>
        /// Write a 32-bit integer in a compressed format.
        /// </summary>
        public void WriteInt32Encoded(int value) {
            Write7BitEncodedInt(value);
        }

        /// <summary>
        /// Write a string value to the stream. Value can be null.
        /// </summary>
        public void WriteStringQ(string value) {
            Write(value != null);
            if (value != null) {
                Write(value);
            }
        }
    }
}
