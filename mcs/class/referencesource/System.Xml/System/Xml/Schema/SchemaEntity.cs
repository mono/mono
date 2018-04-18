//------------------------------------------------------------------------------
// <copyright file="SchemaEntity.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>   
// <owner current="true" primary="true">Microsoft</owner>                                                                                                                            
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    using System;
    using System.Diagnostics;

    internal sealed class SchemaEntity : IDtdEntityInfo {
        private XmlQualifiedName qname;      // Name of entity
        private String url;                  // Url for external entity (system id)
        private String pubid;                // Pubid for external entity
        private String text;                 // Text for internal entity
        private XmlQualifiedName  ndata = XmlQualifiedName.Empty; // NDATA identifier
        private int    lineNumber;           // line number
        private int    linePosition;         // character postion
        private bool   isParameter;          // parameter entity flag
        private bool   isExternal;           // external entity flag
        private bool parsingInProgress;      // whether entity is being parsed (DtdParser infinite recursion check)
        private bool isDeclaredInExternal; // declared in external markup or not
        private string baseURI;
        private string declaredURI;

//
// Constructor
//
        internal SchemaEntity(XmlQualifiedName qname, bool isParameter) {
            this.qname = qname;
            this.isParameter = isParameter;
        }

//
// IDtdEntityInfo interface
//
#region IDtdEntityInfo Members

        string IDtdEntityInfo.Name {
            get { return this.Name.Name; }
        }

        bool IDtdEntityInfo.IsExternal {
            get { return ((SchemaEntity)this).IsExternal;}
        }

        bool IDtdEntityInfo.IsDeclaredInExternal {
            get { return this.DeclaredInExternal; }
        }

        bool IDtdEntityInfo.IsUnparsedEntity {
            get { return !this.NData.IsEmpty; }
        }

        bool IDtdEntityInfo.IsParameterEntity {
            get { return isParameter; }
        }

        string IDtdEntityInfo.BaseUriString {
            get { return this.BaseURI; }
        }

        string IDtdEntityInfo.DeclaredUriString {
            get { return this.DeclaredURI; }
        }

        string IDtdEntityInfo.SystemId {
            get { return this.Url; }
        }

        string IDtdEntityInfo.PublicId {
            get { return this.Pubid; }
        }

        string IDtdEntityInfo.Text {
            get { return ((SchemaEntity)this).Text; }
        }

        int IDtdEntityInfo.LineNumber {
            get { return this.Line; }
        }

        int IDtdEntityInfo.LinePosition {
            get { return this.Pos; }
        }

#endregion

//
// Internal methods and properties
//
#if !SILVERLIGHT
        internal static bool IsPredefinedEntity(String n) {
            return(n == "lt" ||
                   n == "gt" ||
                   n == "amp" ||
                   n == "apos" ||
                   n == "quot");
        }
#endif

        internal XmlQualifiedName Name {
            get { return qname; }
        }

        internal String Url {
            get { return url;}
            set { url = value; isExternal = true;} 
        }

        internal String Pubid {
            get { return pubid;}
            set { pubid = value;}
        }

        internal bool IsExternal {
            get { return isExternal; }
            set { isExternal = value; }
        }

        internal bool DeclaredInExternal {
            get { return isDeclaredInExternal; }
            set { isDeclaredInExternal = value; }
        }

        internal XmlQualifiedName NData {
            get { return ndata;}
            set { ndata = value;}
        }

        internal String Text {
            get { return text;}
            set { text = value; isExternal = false;}
        }

        internal int Line {
            get { return lineNumber;}
            set { lineNumber = value;}    
        }

        internal int Pos {
            get { return linePosition;}
            set { linePosition = value;}
        }

        internal String BaseURI {
            get { return (baseURI == null) ? String.Empty : baseURI; }
            set { baseURI = value; }
        }

        internal bool ParsingInProgress {
            get { return parsingInProgress; }
            set { parsingInProgress = value; }
        }

        internal String DeclaredURI {
            get { return (declaredURI == null) ? String.Empty : declaredURI; }
            set { declaredURI = value; }
        }
    };

}
