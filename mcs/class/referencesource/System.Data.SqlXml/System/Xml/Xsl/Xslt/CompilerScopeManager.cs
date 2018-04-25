//------------------------------------------------------------------------------
// <copyright file="CompilerScopeManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

using System.Diagnostics;

namespace System.Xml.Xsl.Xslt {
    using QilName = System.Xml.Xsl.Qil.QilName;

    // Compiler scope manager keeps track of
    //   Variable declarations
    //   Namespace declarations
    //   Extension and excluded namespaces
    internal sealed class CompilerScopeManager<V> {
        public enum ScopeFlags {
            BackwardCompatibility = 0x1,
            ForwardCompatibility = 0x2,
            CanHaveApplyImports = 0x4,
            NsDecl   = 0x10,                   // NS declaration
            NsExcl   = 0x20,                   // NS Extencion (null for ExcludeAll)
            Variable = 0x40,

            CompatibilityFlags = BackwardCompatibility | ForwardCompatibility,
            InheritedFlags = CompatibilityFlags | CanHaveApplyImports,
            ExclusiveFlags = NsDecl | NsExcl | Variable
        }

        public struct ScopeRecord {
            public int    scopeCount;
            public ScopeFlags flags;
            public string ncName;     // local-name for variable, prefix for namespace, null for extension or excluded namespace
            public string nsUri;      // namespace uri
            public V      value;      // value for variable, null for namespace

            // Exactly one of these three properties is true for every given record
            public bool IsVariable      { get { return (flags & ScopeFlags.Variable) != 0; } }
            public bool IsNamespace     { get { return (flags & ScopeFlags.NsDecl  ) != 0; } }
//          public bool IsExNamespace   { get { return (flags & ScopeFlags.NsExcl  ) != 0; } }
        }

        // Number of predefined records minus one
        private const int       LastPredefRecord = 0;

        private ScopeRecord[]   records = new ScopeRecord[32];
        private int             lastRecord = LastPredefRecord;

        // This is cache of records[lastRecord].scopeCount field;
        // most often we will have PushScope()/PopScope pare over the same record.
        // It has sence to avoid adresing this field through array access.
        private int             lastScopes = 0;

        public CompilerScopeManager() {
            // The prefix 'xml' is by definition bound to the namespace name http://www.w3.org/XML/1998/namespace
            records[0].flags  = ScopeFlags.NsDecl;
            records[0].ncName = "xml";
            records[0].nsUri  = XmlReservedNs.NsXml;
        }

        public CompilerScopeManager(KeywordsTable atoms) {
            records[0].flags  = ScopeFlags.NsDecl;
            records[0].ncName = atoms.Xml;
            records[0].nsUri  = atoms.UriXml;
        }

        public void EnterScope() {
            lastScopes++;
        }

        public void ExitScope() {
            if (0 < lastScopes) {
                lastScopes--;
            } else {
                while (records[--lastRecord].scopeCount == 0) {
                }
                lastScopes = records[lastRecord].scopeCount;
                lastScopes--;
            }
        }

        [Conditional("DEBUG")]
        public void CheckEmpty() {
            ExitScope();
            Debug.Assert(lastRecord == 0 && lastScopes == 0, "PushScope() and PopScope() calls are unbalanced");
        }

        // returns true if ns decls was added to scope
        public bool EnterScope(NsDecl nsDecl) {
            lastScopes++;

            bool hasNamespaces = false;
            bool excludeAll    = false;
            for (; nsDecl != null;  nsDecl = nsDecl.Prev) {
                if (nsDecl.NsUri == null) {
                    Debug.Assert(nsDecl.Prefix == null, "NS may be null only when prefix is null where it is used for extension-element-prefixes='#all'");
                    excludeAll = true;
                } else if (nsDecl.Prefix == null) {
                    AddExNamespace(nsDecl.NsUri);
                } else {
                    hasNamespaces = true;
                    AddNsDeclaration(nsDecl.Prefix, nsDecl.NsUri);
                }
            }
            if (excludeAll) {
                // #all should be on the top of the stack, becase all NSs on this element should be excluded as well
                AddExNamespace(null);
            }
            return hasNamespaces;
        }

        private void AddRecord() {
            // Store cached fields:
            records[lastRecord].scopeCount = lastScopes;
            // Extend record buffer:
            if (++lastRecord == records.Length) {
                ScopeRecord[] newRecords = new ScopeRecord[lastRecord * 2];
                Array.Copy(records, 0, newRecords, 0, lastRecord);
                records = newRecords;
            }
            // reset scope count:
            lastScopes = 0;
        }

        private void AddRecord(ScopeFlags flag, string ncName, string uri, V value) {
            Debug.Assert(flag == (flag & ScopeFlags.ExclusiveFlags) && (flag & (flag - 1)) == 0 && flag != 0, "One exclusive flag");
            Debug.Assert(uri != null || ncName == null, "null, null means exclude '#all'");

            ScopeFlags flags = records[lastRecord].flags;
            bool canReuseLastRecord = (lastScopes == 0) && (flags & ScopeFlags.ExclusiveFlags) == 0;
            if (!canReuseLastRecord) {
                AddRecord();
                flags &= ScopeFlags.InheritedFlags;
            }

            records[lastRecord].flags   = flags | flag;
            records[lastRecord].ncName  = ncName;
            records[lastRecord].nsUri   = uri;
            records[lastRecord].value   = value;
        }

        private void SetFlag(ScopeFlags flag, bool value) {
            Debug.Assert(flag == (flag & ScopeFlags.InheritedFlags) && (flag & (flag - 1)) == 0 && flag != 0, "one inherited flag");
            ScopeFlags flags = records[lastRecord].flags;
            if (((flags & flag) != 0) != value) {
                // lastScopes == records[lastRecord].scopeCount;          // we know this because we are cashing it.
                bool canReuseLastRecord = lastScopes == 0;                // last record is from last scope
                if (!canReuseLastRecord) {
                    AddRecord();
                    flags &= ScopeFlags.InheritedFlags;
                }
                if (flag == ScopeFlags.CanHaveApplyImports) {
                    flags ^= flag;
                } else {
                    flags &= ~ScopeFlags.CompatibilityFlags;
                    if (value) {
                        flags |= flag;
                    }
                }
                records[lastRecord].flags = flags;
            }
            Debug.Assert((records[lastRecord].flags & ScopeFlags.CompatibilityFlags) != ScopeFlags.CompatibilityFlags,
                "BackwardCompatibility and ForwardCompatibility flags are mutually exclusive"
            );
        }

        // Add variable to the current scope.  Returns false in case of duplicates.
        public void AddVariable(QilName varName, V value) {
            Debug.Assert(varName.LocalName != null && varName.NamespaceUri != null);
            AddRecord(ScopeFlags.Variable, varName.LocalName, varName.NamespaceUri, value);
        }

        // Since the prefix might be redefined in an inner scope, we search in descending order in [to, from]
        // If interval is empty (from < to), the function returns null.
        private string LookupNamespace(string prefix, int from, int to) {
            Debug.Assert(prefix != null);
            for (int record = from; to <= record; --record) {
                string recPrefix, recNsUri;
                ScopeFlags flags = GetName(ref records[record], out recPrefix, out recNsUri);
                if (
                    (flags & ScopeFlags.NsDecl) != 0 &&
                    recPrefix == prefix
                ) {
                    return recNsUri;
                }
            }
            return null;
        }

        public string LookupNamespace(string prefix) {
            return LookupNamespace(prefix, lastRecord, 0);
        }

        private static ScopeFlags GetName(ref ScopeRecord re, out string prefix, out string nsUri) {
            prefix = re.ncName;
            nsUri  = re.nsUri;
            return re.flags;
        }

        public void AddNsDeclaration(string prefix, string nsUri) {
            AddRecord(ScopeFlags.NsDecl, prefix, nsUri, default(V));
        }

        public void AddExNamespace(string nsUri) {
            AddRecord(ScopeFlags.NsExcl, null, nsUri, default(V));
        }

        public bool IsExNamespace(string nsUri) {
            Debug.Assert(nsUri != null);
            int exAll = 0;
            for (int record = lastRecord; 0 <= record; record--) {
                string recPrefix, recNsUri;
                ScopeFlags flags = GetName(ref records[record], out recPrefix, out recNsUri);
                if ((flags & ScopeFlags.NsExcl) != 0) {
                    Debug.Assert(recPrefix == null);
                    if (recNsUri == nsUri) {
                        return true;               // This namespace is excluded
                    }
                    if (recNsUri == null) {
                        exAll = record;            // #all namespaces below are excluded
                    }
                } else if (
                    exAll != 0 &&
                    (flags & ScopeFlags.NsDecl) != 0 &&
                    recNsUri == nsUri
                ) {
                    // We need to check that this namespace wasn't undefined before last "#all"
                    bool undefined = false;
                    for (int prev = record + 1; prev < exAll; prev++) {
                        string prevPrefix, prevNsUri;
                        ScopeFlags prevFlags = GetName(ref records[prev], out prevPrefix, out prevNsUri);
                        if (
                            (flags & ScopeFlags.NsDecl) != 0 &&
                            prevPrefix == recPrefix
                        ) {
                            // We don't care if records[prev].nsUri == records[record].nsUri.
                            // In this case the namespace was already undefined above.
                            undefined = true;
                            break;
                        }
                    }
                    if (!undefined) {
                        return true;
                    }
                }
            }
            return false;
        }

        private int SearchVariable(string localName, string uri) {
            Debug.Assert(localName != null);
            for (int record = lastRecord; 0 <= record; --record) {
                string recLocal, recNsUri;
                ScopeFlags flags = GetName(ref records[record], out recLocal, out recNsUri);
                if (
                    (flags & ScopeFlags.Variable) != 0 &&
                    recLocal == localName &&
                    recNsUri == uri
                ) {
                    return record;
                }
            }
            return -1;
        }

        public V LookupVariable(string localName, string uri) {
            int record = SearchVariable(localName, uri);
            return (record < 0) ? default(V) : records[record].value;
        }

        public bool IsLocalVariable(string localName, string uri) {
            int record = SearchVariable(localName, uri);
            while (0 <= --record) {
                if (records[record].scopeCount != 0) {
                    return true;
                }
            }
            return false;
        }

        public bool ForwardCompatibility {
            get { return (records[lastRecord].flags & ScopeFlags.ForwardCompatibility) != 0; }
            set { SetFlag(ScopeFlags.ForwardCompatibility, value); }
        }

        public bool BackwardCompatibility {
            get { return (records[lastRecord].flags & ScopeFlags.BackwardCompatibility) != 0; }
            set { SetFlag(ScopeFlags.BackwardCompatibility, value); }
        }

        public bool CanHaveApplyImports {
            get { return (records[lastRecord].flags & ScopeFlags.CanHaveApplyImports) != 0; }
            set { SetFlag(ScopeFlags.CanHaveApplyImports, value); }
        }

        internal System.Collections.Generic.IEnumerable<ScopeRecord> GetActiveRecords() {
            int currentRecord = this.lastRecord + 1;
            // This logic comes from NamespaceEnumerator.MoveNext but also returns variables
            while (LastPredefRecord < --currentRecord) {
                if (records[currentRecord].IsNamespace) {
                    // This is a namespace declaration
                    if (LookupNamespace(records[currentRecord].ncName, lastRecord, currentRecord + 1) != null) {
                        continue;
                    }
                    // Its prefix has not been redefined later in [currentRecord + 1, lastRecord]
                }
                yield return records[currentRecord];
            }
        }

        public NamespaceEnumerator GetEnumerator() {
            return new NamespaceEnumerator(this);
        }

        internal struct NamespaceEnumerator {
            CompilerScopeManager<V> scope;
            int                     lastRecord;
            int                     currentRecord;

            public NamespaceEnumerator(CompilerScopeManager<V> scope) {
                this.scope      = scope;
                this.lastRecord = scope.lastRecord;
                this.currentRecord = lastRecord + 1;
            }

            public void Reset() {
                currentRecord = lastRecord + 1;
            }

            public bool MoveNext() {
                while (LastPredefRecord < --currentRecord) {
                    if (scope.records[currentRecord].IsNamespace) {
                        // This is a namespace declaration
                        if (scope.LookupNamespace(scope.records[currentRecord].ncName, lastRecord, currentRecord + 1) == null) {
                            // Its prefix has not been redefined later in [currentRecord + 1, lastRecord]
                            return true;
                        }
                    }
                }
                return false;
            }

            public ScopeRecord Current {
                get {
                    Debug.Assert(LastPredefRecord <= currentRecord && currentRecord <= scope.lastRecord, "MoveNext() either was not called or returned false");
                    Debug.Assert(scope.records[currentRecord].IsNamespace);
                    return scope.records[currentRecord];
                }
            }
        }
    }
}
