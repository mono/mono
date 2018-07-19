//------------------------------------------------------------------------------
// <copyright file="XmlAttributeCache.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.Runtime {
    using System;
    using System.Diagnostics;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Schema;


    /// <summary>
    /// This writer supports only writer methods which write attributes.  Attributes are stored in a
    /// data structure until StartElementContent() is called, at which time the attributes are flushed
    /// to the wrapped writer.  In the case of duplicate attributes, the last attribute's value is used.
    /// </summary>
    internal sealed class XmlAttributeCache : XmlRawWriter, IRemovableWriter {
        private XmlRawWriter wrapped;
        private OnRemoveWriter onRemove;        // Event handler that is called when cached attributes are flushed to wrapped writer
        private AttrNameVal[] arrAttrs;         // List of cached attribute names and value parts
        private int numEntries;                 // Number of attributes in the cache
        private int idxLastName;                // The entry containing the name of the last attribute to be cached
        private int hashCodeUnion;              // Set of hash bits that can quickly guarantee a name is not a duplicate

        /// <summary>
        /// Initialize the cache.  Use this method instead of a constructor in order to reuse the cache.
        /// </summary>
        public void Init(XmlRawWriter wrapped) {
            SetWrappedWriter(wrapped);

            // Clear attribute list
            this.numEntries = 0;
            this.idxLastName = 0;
            this.hashCodeUnion = 0;
        }

        /// <summary>
        /// Return the number of cached attributes.
        /// </summary>
        public int Count {
            get { return this.numEntries; }
        }


        //-----------------------------------------------
        // IRemovableWriter interface
        //-----------------------------------------------

        /// <summary>
        /// This writer will raise this event once cached attributes have been flushed in order to signal that the cache
        /// no longer needs to be part of the pipeline.
        /// </summary>
        public OnRemoveWriter OnRemoveWriterEvent {
            get { return this.onRemove; }
            set { this.onRemove = value; }
        }

        /// <summary>
        /// The wrapped writer will callback on this method if it wishes to remove itself from the pipeline.
        /// </summary>
        private void SetWrappedWriter(XmlRawWriter writer) {
            // If new writer might remove itself from pipeline, have it callback on this method when its ready to go
            IRemovableWriter removable = writer as IRemovableWriter;
            if (removable != null)
                removable.OnRemoveWriterEvent = SetWrappedWriter;

            this.wrapped = writer;
        }


        //-----------------------------------------------
        // XmlWriter interface
        //-----------------------------------------------

        /// <summary>
        /// Add an attribute to the cache.  If an attribute if the same name already exists, replace it.
        /// </summary>
        public override void WriteStartAttribute(string prefix, string localName, string ns) {
            int hashCode;
            int idx = 0;
            Debug.Assert(localName != null && localName.Length != 0 && prefix != null && ns != null);

            // Compute hashcode based on first letter of the localName
            hashCode = (1 << ((int) localName[0] & 31));

            // If the hashcode is not in the union, then name will not be found by a scan
            if ((this.hashCodeUnion & hashCode) != 0) {
                // The name may or may not be present, so scan for it
                Debug.Assert(this.numEntries != 0);

                do {
                    if (this.arrAttrs[idx].IsDuplicate(localName, ns, hashCode))
                        break;

                    // Next attribute name
                    idx = this.arrAttrs[idx].NextNameIndex;
                }
                while (idx != 0);
            }
            else {
                // Insert hashcode into union
                this.hashCodeUnion |= hashCode;
            }

            // Insert new attribute; link attribute names together in a list
            EnsureAttributeCache();
            if (this.numEntries != 0)
                this.arrAttrs[this.idxLastName].NextNameIndex = this.numEntries;
            this.idxLastName = this.numEntries++;
            this.arrAttrs[this.idxLastName].Init(prefix, localName, ns, hashCode);
        }

        /// <summary>
        /// No-op.
        /// </summary>
        public override void WriteEndAttribute() {
        }

        /// <summary>
        /// Pass through namespaces to underlying writer.  If any attributes have been cached, flush them.
        /// </summary>
        internal override void WriteNamespaceDeclaration(string prefix, string ns) {
            FlushAttributes();
            this.wrapped.WriteNamespaceDeclaration(prefix, ns);
        }

        /// <summary>
        /// Add a block of text to the cache.  This text block makes up some or all of the untyped string
        /// value of the current attribute.
        /// </summary>
        public override void WriteString(string text) {
            Debug.Assert(text != null);
            Debug.Assert(this.arrAttrs != null && this.numEntries != 0);
            EnsureAttributeCache();
            this.arrAttrs[this.numEntries++].Init(text);
        }

        /// <summary>
        /// All other WriteValue methods are implemented by XmlWriter to delegate to WriteValue(object) or WriteValue(string), so
        /// only these two methods need to be implemented.
        /// </summary>
        public override void WriteValue(object value) {
            Debug.Assert(value is XmlAtomicValue, "value should always be an XmlAtomicValue, as XmlAttributeCache is only used by XmlQueryOutput");
            Debug.Assert(this.arrAttrs != null && this.numEntries != 0);
            EnsureAttributeCache();
            this.arrAttrs[this.numEntries++].Init((XmlAtomicValue) value);
        }

        public override void WriteValue(string value) {
            WriteValue(value);
        }

        /// <summary>
        /// Send cached, non-overriden attributes to the specified writer.  Calling this method has
        /// the side effect of clearing the attribute cache.
        /// </summary>
        internal override void StartElementContent() {
            FlushAttributes();

            // Call StartElementContent on wrapped writer
            this.wrapped.StartElementContent();
        }

        public override void WriteStartElement(string prefix, string localName, string ns) {
            Debug.Assert(false, "Should never be called on XmlAttributeCache.");
        }
        internal override void WriteEndElement(string prefix, string localName, string ns) {
            Debug.Assert(false, "Should never be called on XmlAttributeCache.");
        }
        public override void WriteComment(string text) {
            Debug.Assert(false, "Should never be called on XmlAttributeCache.");
        }
        public override void WriteProcessingInstruction(string name, string text) {
            Debug.Assert(false, "Should never be called on XmlAttributeCache.");
        }
        public override void WriteEntityRef(string name) {
            Debug.Assert(false, "Should never be called on XmlAttributeCache.");
        }

        /// <summary>
        /// Forward call to wrapped writer.
        /// </summary>
        public override void Close() {
            this.wrapped.Close();
        }

        /// <summary>
        /// Forward call to wrapped writer.
        /// </summary>
        public override void Flush() {
            this.wrapped.Flush();
        }
 

        //-----------------------------------------------
        // Helper methods
        //-----------------------------------------------

        private void FlushAttributes() {
            int idx = 0, idxNext;
            string localName;

            while (idx != this.numEntries) {
                // Get index of next attribute's name (0 if this is the last attribute)
                idxNext = this.arrAttrs[idx].NextNameIndex;
                if (idxNext == 0)
                    idxNext = this.numEntries;

                // If localName is null, then this is a duplicate attribute that has been marked as "deleted"
                localName = this.arrAttrs[idx].LocalName;
                if (localName != null) {
                    string prefix = this.arrAttrs[idx].Prefix;
                    string ns = this.arrAttrs[idx].Namespace;

                    this.wrapped.WriteStartAttribute(prefix, localName, ns);

                    // Output all of this attribute's text or typed values
                    while (++idx != idxNext) {
                        string text = this.arrAttrs[idx].Text;

                        if (text != null)
                            this.wrapped.WriteString(text);
                        else
                            this.wrapped.WriteValue(this.arrAttrs[idx].Value);
                    }

                    this.wrapped.WriteEndAttribute();
                }
                else {
                    // Skip over duplicate attributes
                    idx = idxNext;
                }
            }

            // Notify event listener that attributes have been flushed
            if (this.onRemove != null)
                this.onRemove(this.wrapped);
        }

        private struct AttrNameVal {
            private string localName;
            private string prefix;
            private string namespaceName;
            private string text;
            private XmlAtomicValue value;
            private int hashCode;
            private int nextNameIndex;

            public string LocalName { get { return this.localName; } }
            public string Prefix { get { return this.prefix; } }
            public string Namespace { get { return this.namespaceName; } }
            public string Text { get { return this.text; } }
            public XmlAtomicValue Value { get { return this.value; } }
            public int NextNameIndex { get { return this.nextNameIndex; } set { this.nextNameIndex = value; } }

            /// <summary>
            /// Cache an attribute's name and type.
            /// </summary>
            public void Init(string prefix, string localName, string ns, int hashCode) {
                this.localName = localName;
                this.prefix = prefix;
                this.namespaceName = ns;
                this.hashCode = hashCode;
                this.nextNameIndex = 0;
            }

            /// <summary>
            /// Cache all or part of the attribute's string value.
            /// </summary>
            public void Init(string text) {
                this.text = text;
                this.value = null;
            }

            /// <summary>
            /// Cache all or part of the attribute's typed value.
            /// </summary>
            public void Init(XmlAtomicValue value) {
                this.text = null;
                this.value = value;
            }

            /// <summary>
            /// Returns true if this attribute has the specified name (and thus is a duplicate).
            /// </summary>
            public bool IsDuplicate(string localName, string ns, int hashCode) {
                // If attribute is not marked as deleted
                if (this.localName != null) {
                    // And if hash codes match,
                    if (this.hashCode == hashCode) {
                        // And if local names match,
                        if (this.localName.Equals(localName)) {
                            // And if namespaces match,
                            if (this.namespaceName.Equals(ns)) {
                                // Then found duplicate attribute, so mark the attribute as deleted
                                this.localName = null;
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }

    #if DEBUG
        private const int DefaultCacheSize = 2;
    #else
        private const int DefaultCacheSize = 32;
    #endif

        /// <summary>
        /// Ensure that attribute array has been created and is large enough for at least one
        /// additional entry.
        /// </summary>
        private void EnsureAttributeCache() {
            if (this.arrAttrs == null) {
                // Create caching array
                this.arrAttrs = new AttrNameVal[DefaultCacheSize];
            }
            else if (this.numEntries >= this.arrAttrs.Length) {
                // Resize caching array
                Debug.Assert(this.numEntries == this.arrAttrs.Length);
                AttrNameVal[] arrNew = new AttrNameVal[this.numEntries * 2];
                Array.Copy(this.arrAttrs, arrNew, this.numEntries);
                this.arrAttrs = arrNew;
            }
        }
    }
}
