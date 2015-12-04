//------------------------------------------------------------------------------
// <copyright file="StaticDataManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="false">[....]</owner>
//------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Xsl.Qil;
using System.Xml.Xsl.Runtime;

namespace System.Xml.Xsl.IlGen {

    /// <summary>
    /// This internal class maintains a list of unique values.  Each unique value is assigned a unique ID, which can
    /// be used to quickly access the value, since it corresponds to the value's position in the list.
    /// </summary>
    internal class UniqueList<T> {
        private Dictionary<T, int> lookup = new Dictionary<T, int>();
        private List<T> list = new List<T>();

        /// <summary>
        /// If "value" is already in the list, do not add it.  Return the unique ID of the value in the list.
        /// </summary>
        public int Add(T value) {
            int id;

            if (!this.lookup.ContainsKey(value)) {
                // The value does not yet exist, so add it to the list
                id = list.Count;
                this.lookup.Add(value, id);
                this.list.Add(value);
            }
            else {
                id = this.lookup[value];
            }

            return id;
        }

        /// <summary>
        /// Return an array of the unique values.
        /// </summary>
        public T[] ToArray() {
            return list.ToArray();
        }
    }


    /// <summary>
    /// Manages all static data that is used by the runtime.  This includes:
    ///   1. All NCName and QName atoms that will be used at run-time
    ///   2. All QName filters that will be used at run-time
    ///   3. All Xml types that will be used at run-time
    ///   4. All global variables and parameters
    /// </summary>
    internal class StaticDataManager {
        private UniqueList<string> uniqueNames;
        private UniqueList<Int32Pair> uniqueFilters;
        private List<StringPair[]> prefixMappingsList;
        private List<string> globalNames;
        private UniqueList<EarlyBoundInfo> earlyInfo;
        private UniqueList<XmlQueryType> uniqueXmlTypes;
        private UniqueList<XmlCollation> uniqueCollations;

        /// <summary>
        /// Add "name" to the list of unique names that are used by this query.  Return the index of
        /// the unique name in the list.
        /// </summary>
        public int DeclareName(string name) {
            if (this.uniqueNames == null)
                this.uniqueNames = new UniqueList<string>();

            return this.uniqueNames.Add(name);
        }

        /// <summary>
        /// Return an array of all names that are used by the query (null if no names).
        /// </summary>
        public string[] Names {
            get { return (this.uniqueNames != null) ? this.uniqueNames.ToArray() : null; }
        }

        /// <summary>
        /// Add a name filter to the list of unique filters that are used by this query.  Return the index of
        /// the unique filter in the list.
        /// </summary>
        public int DeclareNameFilter(string locName, string nsUri) {
            if (this.uniqueFilters == null)
                this.uniqueFilters = new UniqueList<Int32Pair>();

            return this.uniqueFilters.Add(new Int32Pair(DeclareName(locName), DeclareName(nsUri)));
        }

        /// <summary>
        /// Return an array of all name filters, where each name filter is represented as a pair of integer offsets (localName, namespaceUri)
        /// into the Names array (null if no name filters).
        /// </summary>
        public Int32Pair[] NameFilters {
            get { return (this.uniqueFilters != null) ? this.uniqueFilters.ToArray() : null; }
        }

        /// <summary>
        /// Add a list of QilExpression NamespaceDeclarations to an array of strings (prefix followed by namespace URI).
        /// Return index of the prefix mappings within this array.
        /// </summary>
        public int DeclarePrefixMappings(IList<QilNode> list) {
            StringPair[] prefixMappings;

            // Fill mappings array
            prefixMappings = new StringPair[list.Count];
            for (int i = 0; i < list.Count; i++) {
                // Each entry in mappings array must be a constant NamespaceDeclaration
                QilBinary ndNmspDecl = (QilBinary) list[i];
                Debug.Assert(ndNmspDecl != null);
                Debug.Assert(ndNmspDecl.Left is QilLiteral && ndNmspDecl.Right is QilLiteral);

                prefixMappings[i] = new StringPair((string) (QilLiteral) ndNmspDecl.Left, (string) (QilLiteral) ndNmspDecl.Right);
            }

            // Add mappings to list and return index
            if (this.prefixMappingsList == null)
                this.prefixMappingsList = new List<StringPair[]>();

            this.prefixMappingsList.Add(prefixMappings);
            return this.prefixMappingsList.Count - 1;
        }

        /// <summary>
        /// Return an array of all prefix mappings that are used by the query to compute names (null if no mappings).
        /// </summary>
        public StringPair[][] PrefixMappingsList {
            get { return (this.prefixMappingsList != null) ? this.prefixMappingsList.ToArray() : null; }
        }

        /// <summary>
        /// Declare a new global variable or parameter.
        /// </summary>
        public int DeclareGlobalValue(string name) {
            int idx;

            if (this.globalNames == null)
                this.globalNames = new List<string>();

            idx = this.globalNames.Count;
            this.globalNames.Add(name);
            return idx;
        }

        /// <summary>
        /// Return an array containing the names of all global variables and parameters.
        /// </summary>
        public string[] GlobalNames {
            get { return (this.globalNames != null) ? this.globalNames.ToArray() : null; }
        }

        /// <summary>
        /// Add early bound information to a list that is used by this query.  Return the index of
        /// the early bound information in the list.
        /// </summary>
        public int DeclareEarlyBound(string namespaceUri, Type ebType) {
            if (this.earlyInfo == null)
                this.earlyInfo = new UniqueList<EarlyBoundInfo>();

            return this.earlyInfo.Add(new EarlyBoundInfo(namespaceUri, ebType));
        }

        /// <summary>
        /// Return an array of all early bound information that is used by the query (null if none is used).
        /// </summary>
        public EarlyBoundInfo[] EarlyBound {
            get {
                if (this.earlyInfo != null)
                    return this.earlyInfo.ToArray();

                return null;
            }
        }

        /// <summary>
        /// Add "type" to the list of unique types that are used by this query.  Return the index of
        /// the unique type in the list.
        /// </summary>
        public int DeclareXmlType(XmlQueryType type) {
            if (this.uniqueXmlTypes == null)
                this.uniqueXmlTypes = new UniqueList<XmlQueryType>();

            XmlQueryTypeFactory.CheckSerializability(type);
            return this.uniqueXmlTypes.Add(type);
        }

        /// <summary>
        /// Return an array of all types that are used by the query (null if no names).
        /// </summary>
        public XmlQueryType[] XmlTypes {
            get { return (this.uniqueXmlTypes != null) ? this.uniqueXmlTypes.ToArray() : null; }
        }

        /// <summary>
        /// Add "collation" to the list of unique collations that are used by this query.  Return the index of
        /// the unique collation in the list.
        /// </summary>
        public int DeclareCollation(string collation) {
            if (this.uniqueCollations == null)
                this.uniqueCollations = new UniqueList<XmlCollation>();

            return this.uniqueCollations.Add(XmlCollation.Create(collation));
        }

        /// <summary>
        /// Return an array of all collations that are used by the query (null if no names).
        /// </summary>
        public XmlCollation[] Collations {
            get { return (this.uniqueCollations != null) ? this.uniqueCollations.ToArray() : null; }
        }
    }
}
