//------------------------------------------------------------------------------
// <copyright file="XmlNamespaceManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Xml {

    using System;
    using System.IO;
    using System.Collections;
    using System.Diagnostics;
    using System.Collections.Generic;

    public class XmlNamespaceManager : IXmlNamespaceResolver, IEnumerable {
#if !SILVERLIGHT // EmptyResolver is not used in Silverlight
        static volatile IXmlNamespaceResolver s_EmptyResolver;
#endif

        struct NamespaceDeclaration {
            public string prefix;
            public string uri;
            public int    scopeId;
            public int    previousNsIndex;

            public void Set( string prefix, string uri, int scopeId, int previousNsIndex ) {
                this.prefix = prefix;
                this.uri = uri;
                this.scopeId = scopeId;
                this.previousNsIndex = previousNsIndex;
            }
        }

        // array with namespace declarations
        NamespaceDeclaration[] nsdecls;

        // index of last declaration
        int lastDecl = 0;

        // name table
        XmlNameTable nameTable;

        // ID (depth) of the current scope
        int scopeId;

        // hash table for faster lookup when there is lots of namespaces
        Dictionary<string,int> hashTable;
        bool useHashtable;

        // atomized prefixes for "xml" and "xmlns"
        string xml;
        string xmlNs;

        // Constants
        const int MinDeclsCountForHashtable = 16;

#if !SILVERLIGHT // EmptyResolver is not used in Silverlight
        internal static IXmlNamespaceResolver EmptyResolver {
            get {
                if ( s_EmptyResolver == null ) {
                    // no locking; the empty resolver is immutable so it's not a problem that it may get initialized more than once
                    s_EmptyResolver = new XmlNamespaceManager( new NameTable() );
                }
                return s_EmptyResolver;
            }
        }
#endif

#if !SILVERLIGHT // This constructor is not used in Silverlight
        internal XmlNamespaceManager() {
        }
#endif

        public XmlNamespaceManager( XmlNameTable nameTable ) {
            this.nameTable = nameTable;
            xml = nameTable.Add("xml");
            xmlNs = nameTable.Add("xmlns");

            nsdecls = new NamespaceDeclaration[8];
            string emptyStr = nameTable.Add( string.Empty );
            nsdecls[0].Set( emptyStr, emptyStr, -1, -1 );
            nsdecls[1].Set( xmlNs, nameTable.Add( XmlReservedNs.NsXmlNs ), -1, -1 );
            nsdecls[2].Set( xml, nameTable.Add( XmlReservedNs.NsXml ), 0, -1 );
            lastDecl = 2;
            scopeId = 1;
        }

        public virtual XmlNameTable NameTable { 
            get { 
                return nameTable; 
            }
        }

        public virtual string DefaultNamespace {
            get { 
                string defaultNs = LookupNamespace( string.Empty );
                return ( defaultNs == null ) ? string.Empty : defaultNs;
            }
        }

        public virtual void PushScope() {
            scopeId++;
        }

        public virtual bool PopScope() {
            int decl = lastDecl;
            if ( scopeId == 1 ) {
                return false;
            }
            while( nsdecls[decl].scopeId == scopeId ) {
                if ( useHashtable ) {
                    hashTable[nsdecls[decl].prefix] = nsdecls[decl].previousNsIndex;
                }
                decl--;
                Debug.Assert( decl >= 2 );
            }
            lastDecl = decl;
            scopeId--;
            return true;
        }

        public virtual void AddNamespace( string prefix, string uri ) {
            if ( uri == null )
                throw new ArgumentNullException( "uri" );

            if ( prefix == null )
                throw new ArgumentNullException( "prefix" );

            prefix = nameTable.Add( prefix );
            uri = nameTable.Add( uri );
            
            if ( ( Ref.Equal( xml, prefix ) && !uri.Equals( XmlReservedNs.NsXml ) ) ) {
                throw new ArgumentException( Res.GetString( Res.Xml_XmlPrefix ) );
            }
            if ( Ref.Equal( xmlNs, prefix ) ) {
                throw new ArgumentException( Res.GetString( Res.Xml_XmlnsPrefix ) );
            }
            
            int declIndex = LookupNamespaceDecl( prefix );
            int previousDeclIndex = -1;
            if ( declIndex != -1 ) {
                if ( nsdecls[declIndex].scopeId == scopeId ) {
                    // redefine if in the same scope
                    nsdecls[declIndex].uri = uri;
                    return;
                }
                else {
                    // othewise link
                    previousDeclIndex = declIndex;
                }
            }

            // set new namespace declaration
            if ( lastDecl == nsdecls.Length - 1 ) {
                NamespaceDeclaration[] newNsdecls = new NamespaceDeclaration[nsdecls.Length * 2];
                Array.Copy( nsdecls, 0, newNsdecls, 0, nsdecls.Length );
                nsdecls = newNsdecls;
            }
            
            nsdecls[++lastDecl].Set( prefix, uri, scopeId, previousDeclIndex );

            // add to hashTable
            if ( useHashtable ) {
                hashTable[prefix] = lastDecl;
            }
            // or create a new hashTable if the threashold has been reached
            else if ( lastDecl >= MinDeclsCountForHashtable ) {
                // add all to hash table
                Debug.Assert( hashTable == null );
                hashTable = new Dictionary<string,int>( lastDecl );
                for ( int i = 0; i <= lastDecl; i++ ) {
                    hashTable[nsdecls[i].prefix] = i;
                }
                useHashtable = true;
            }
        }

        public virtual void RemoveNamespace( string prefix, string uri ) {
            if ( uri == null ) {
                throw new ArgumentNullException( "uri" );
            }
            if ( prefix == null ) {
                throw new ArgumentNullException( "prefix" );
            }

            int declIndex = LookupNamespaceDecl( prefix );
            while ( declIndex != -1 ) {
                if ( String.Equals( nsdecls[declIndex].uri, uri ) && nsdecls[declIndex].scopeId == scopeId ) {
                    nsdecls[declIndex].uri = null;
                }
                declIndex = nsdecls[declIndex].previousNsIndex;
            }
        }

        public virtual IEnumerator GetEnumerator() {
            Dictionary<string, string> prefixes = new Dictionary<string, string>(lastDecl + 1);
            for( int thisDecl = 0; thisDecl <= lastDecl; thisDecl ++ ) {
                if ( nsdecls[thisDecl].uri != null ) {
                    prefixes[nsdecls[thisDecl].prefix] = nsdecls[thisDecl].prefix;
                }
            }
            return prefixes.Keys.GetEnumerator();
        }

// This pragma disables a warning that the return type is not CLS-compliant, but generics are part of CLS in Whidbey. 
#pragma warning disable 3002
        public virtual IDictionary<string,string> GetNamespacesInScope( XmlNamespaceScope scope ) {
#pragma warning restore 3002
            int i = 0;
            switch ( scope ) {
                case XmlNamespaceScope.All:
                    i = 2;
                    break;
                case XmlNamespaceScope.ExcludeXml:
                    i = 3;
                    break;
                case XmlNamespaceScope.Local:
                    i = lastDecl;
                    while ( nsdecls[i].scopeId == scopeId ) {
                        i--;
                        Debug.Assert( i >= 2 );
                    }
                    i++;
                    break;
            }

            Dictionary<string,string> dict = new Dictionary<string, string>( lastDecl - i + 1 );
            for( ; i <= lastDecl; i++ ) {
                string prefix = nsdecls[i].prefix;
                string uri = nsdecls[i].uri;
                Debug.Assert( prefix != null );

                if ( uri != null ) {
                    if ( uri.Length > 0 || prefix.Length > 0 || scope == XmlNamespaceScope.Local ) {
                        dict[prefix] = uri;
                    }
                    else {
                        // default namespace redeclared to "" -> remove from list for all scopes other than local
                        dict.Remove( prefix );
                    }
                }
            }
            return dict;
        }

        public virtual string LookupNamespace( string prefix ) {
            int declIndex = LookupNamespaceDecl( prefix );
            return ( declIndex == -1  ) ? null : nsdecls[declIndex].uri;
        }

        private int LookupNamespaceDecl( string prefix ) {
            if ( useHashtable ) {
                int declIndex;
                if ( hashTable.TryGetValue( prefix, out declIndex ) ) {
                    while ( declIndex != -1 && nsdecls[declIndex].uri == null ) {
                        declIndex = nsdecls[declIndex].previousNsIndex;
                    }
                    return declIndex;
                }
                return -1;
            }
            else {
                // First assume that prefix is atomized
                for( int thisDecl = lastDecl; thisDecl >= 0; thisDecl -- ) {
                    if ( (object)nsdecls[thisDecl].prefix == (object)prefix && nsdecls[thisDecl].uri != null ) {
                        return thisDecl;
                    }
                }

                // Non-atomized lookup
                for( int thisDecl = lastDecl; thisDecl >= 0; thisDecl -- ) {
                    if ( String.Equals( nsdecls[thisDecl].prefix, prefix ) && nsdecls[thisDecl].uri != null ) {
                        return thisDecl;
                    }
                }
            }
            return -1;
        }

        public virtual string LookupPrefix( string uri ) {
            // Don't assume that prefix is atomized
            for( int thisDecl = lastDecl; thisDecl >= 0; thisDecl -- ) {
                if ( String.Equals( nsdecls[thisDecl].uri, uri ) ) {
                    string prefix = nsdecls[thisDecl].prefix;
                    if ( String.Equals( LookupNamespace( prefix ), uri ) ) {
                        return prefix;
                    }
                }
            }
            return null;
        }

        public virtual bool HasNamespace( string prefix ) {
            // Don't assume that prefix is atomized
            for( int thisDecl = lastDecl; nsdecls[thisDecl].scopeId == scopeId; thisDecl-- ) {
                if ( String.Equals( nsdecls[thisDecl].prefix, prefix ) && nsdecls[thisDecl].uri != null ) {
                    if ( prefix.Length > 0 || nsdecls[thisDecl].uri.Length > 0 ) {
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }

#if !SILVERLIGHT // This method is not used in Silverlight
        internal bool GetNamespaceDeclaration( int idx, out string prefix, out string uri ) {
            idx = lastDecl - idx;
            if ( idx < 0 ) {
                prefix = uri = null;
                return false;
            }

            prefix = nsdecls[idx].prefix;
            uri = nsdecls[idx].uri;

            return true;
        }
#endif
   } //XmlNamespaceManager
}
