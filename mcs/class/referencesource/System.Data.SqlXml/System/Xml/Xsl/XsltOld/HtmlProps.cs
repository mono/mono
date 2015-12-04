//------------------------------------------------------------------------------
// <copyright file="HtmlProps.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.XsltOld {
    using Res = System.Xml.Utils.Res;
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Xml;
    using System.Globalization;

    internal class HtmlElementProps {
        private bool   empty;
        private bool   abrParent;
        private bool   uriParent;
        private bool   noEntities;
        private bool   blockWS;
        private bool   head;
        private bool   nameParent;

        static public HtmlElementProps Create(bool empty, bool abrParent, bool uriParent, bool noEntities, bool blockWS, bool head, bool nameParent) {
            HtmlElementProps props = new HtmlElementProps();
            props.empty      = empty     ;
            props.abrParent  = abrParent ;
            props.uriParent  = uriParent ;
            props.noEntities = noEntities;
            props.blockWS    = blockWS   ;
            props.head       = head      ;
            props.nameParent = nameParent;
            return props;
        }
        public bool   Empty      { get { return this.empty     ; } }
        public bool   AbrParent  { get { return this.abrParent ; } }
        public bool   UriParent  { get { return this.uriParent ; } }
        public bool   NoEntities { get { return this.noEntities; } }
        public bool   BlockWS    { get { return this.blockWS   ; } }
        public bool   Head       { get { return this.head      ; } }
        public bool   NameParent { get { return this.nameParent; } }


        static private Hashtable         s_table = CreatePropsTable();
//        static private HtmlElementProps  s_otherElements = Create(false, false, false, false, false, false, false);

        static public  HtmlElementProps GetProps(string name) {
            HtmlElementProps result = (HtmlElementProps) s_table[name];
            return result;
            // We can do this but in case of Xml/Html mixed output this doesn't have big sence.
//            return result != null ?  result : s_otherElements;            
        }

        static Hashtable CreatePropsTable() {
            bool o = false, X = true;
            Hashtable table = new Hashtable(71, StringComparer.OrdinalIgnoreCase); {
                //                                EMPTY    ABR     URI    NO_ENT  NO_WS   HEAD   NAME
                table.Add( "a"          , Create(   o   ,   o   ,   X   ,   o   ,   o  ,   o  ,   X   ) );
                table.Add( "address"    , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "applet"     , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "area"       , Create(   X   ,   X   ,   X   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "base"       , Create(   X   ,   o   ,   X   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "basefont"   , Create(   X   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "blockquote" , Create(   o   ,   o   ,   X   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "body"       , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "br"         , Create(   X   ,   o   ,   o   ,   o   ,   o  ,   o  ,   o   ) );
                table.Add( "button"     , Create(   o   ,   X   ,   o   ,   o   ,   o  ,   o  ,   o   ) );
                table.Add( "caption"    , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "center"     , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "col"        , Create(   X   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "colgroup"   , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "dd"         , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "del"        , Create(   o   ,   o   ,   X   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "dir"        , Create(   o   ,   X   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "div"        , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "dl"         , Create(   o   ,   X   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "dt"         , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "fieldset"   , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "font"       , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "form"       , Create(   o   ,   o   ,   X   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "frame"      , Create(   X   ,   X   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "frameset"   , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "h1"         , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "h2"         , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "h3"         , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "h4"         , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "h5"         , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "h6"         , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "head"       , Create(   o   ,   o   ,   X   ,   o   ,   X  ,   X  ,   o   ) );
                table.Add( "hr"         , Create(   X   ,   X   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "html"       , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "iframe"     , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "img"        , Create(   X   ,   X   ,   X   ,   o   ,   o  ,   o  ,   o   ) );
                table.Add( "input"      , Create(   X   ,   X   ,   X   ,   o   ,   o  ,   o  ,   o   ) );
                table.Add( "ins"        , Create(   o   ,   o   ,   X   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "isindex"    , Create(   X   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "legend"     , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "li"         , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "link"       , Create(   X   ,   o   ,   X   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "map"        , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "menu"       , Create(   o   ,   X   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "meta"       , Create(   X   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "noframes"   , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "noscript"   , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "object"     , Create(   o   ,   X   ,   X   ,   o   ,   o  ,   o  ,   o   ) );
                table.Add( "ol"         , Create(   o   ,   X   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "optgroup"   , Create(   o   ,   X   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "option"     , Create(   o   ,   X   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "p"          , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "param"      , Create(   X   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "pre"        , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "q"          , Create(   o   ,   o   ,   X   ,   o   ,   o  ,   o  ,   o   ) );
                table.Add( "s"          , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "script"     , Create(   o   ,   X   ,   X   ,   X   ,   o  ,   o  ,   o   ) );
                table.Add( "select"     , Create(   o   ,   X   ,   o   ,   o   ,   o  ,   o  ,   o   ) );
                table.Add( "strike"     , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "style"      , Create(   o   ,   o   ,   o   ,   X   ,   X  ,   o  ,   o   ) );
                table.Add( "table"      , Create(   o   ,   o   ,   X   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "tbody"      , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "td"         , Create(   o   ,   X   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "textarea"   , Create(   o   ,   X   ,   o   ,   o   ,   o  ,   o  ,   o   ) );
                table.Add( "tfoot"      , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "th"         , Create(   o   ,   X   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "thead"      , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "title"      , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "tr"         , Create(   o   ,   o   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "ul"         , Create(   o   ,   X   ,   o   ,   o   ,   X  ,   o  ,   o   ) );
                table.Add( "xmp"        , Create(   o   ,   o   ,   o   ,   o   ,   o  ,   o  ,   o   ) );
            }
            return table;
        }
    }

    internal class HtmlAttributeProps {
        private bool   abr;
        private bool   uri;
        private bool   name;

        static public HtmlAttributeProps Create(bool abr, bool uri, bool name) {
            HtmlAttributeProps props = new HtmlAttributeProps();
            props.abr  = abr;
            props.uri  = uri;
            props.name = name;
            return props;
        }
        public bool   Abr  { get { return this.abr ; } }
        public bool   Uri  { get { return this.uri ; } }
        public bool   Name { get { return this.name; } }

        static private Hashtable         s_table = CreatePropsTable();
//      static private HtmlElementProps  s_otherAttributes = Create(false, false, false);

        static public  HtmlAttributeProps GetProps(string name) {
            HtmlAttributeProps result = (HtmlAttributeProps) s_table[name];
            return result;
            // We can do this but in case of Xml/Html mixed output this doesn't have big sence.
//          return result != null ?  result : s_otherElements;            
        }

        static Hashtable CreatePropsTable() {
            bool o = false, X = true;
            Hashtable table = new Hashtable(26, StringComparer.OrdinalIgnoreCase); {
                //                               ABR     URI    NAME 
                table.Add( "action"   , Create(   o   ,   X  ,   o   ) );
                table.Add( "checked"  , Create(   X   ,   o  ,   o   ) );
                table.Add( "cite"     , Create(   o   ,   X  ,   o   ) );
                table.Add( "classid"  , Create(   o   ,   X  ,   o   ) );
                table.Add( "codebase" , Create(   o   ,   X  ,   o   ) );
                table.Add( "compact"  , Create(   X   ,   o  ,   o   ) );
                table.Add( "data"     , Create(   o   ,   X  ,   o   ) );
                table.Add( "datasrc"  , Create(   o   ,   X  ,   o   ) );
                table.Add( "declare"  , Create(   X   ,   o  ,   o   ) );
                table.Add( "defer"    , Create(   X   ,   o  ,   o   ) );
                table.Add( "disabled" , Create(   X   ,   o  ,   o   ) );
                table.Add( "for"      , Create(   o   ,   X  ,   o   ) );
                table.Add( "href"     , Create(   o   ,   X  ,   o   ) );
                table.Add( "ismap"    , Create(   X   ,   o  ,   o   ) );
                table.Add( "longdesc" , Create(   o   ,   X  ,   o   ) );
                table.Add( "multiple" , Create(   X   ,   o  ,   o   ) );
                table.Add( "name"     , Create(   o   ,   o  ,   X   ) );
                table.Add( "nohref"   , Create(   X   ,   o  ,   o   ) );
                table.Add( "noresize" , Create(   X   ,   o  ,   o   ) );
                table.Add( "noshade"  , Create(   X   ,   o  ,   o   ) );
                table.Add( "nowrap"   , Create(   X   ,   o  ,   o   ) );
                table.Add( "profile"  , Create(   o   ,   X  ,   o   ) );
                table.Add( "readonly" , Create(   X   ,   o  ,   o   ) );
                table.Add( "selected" , Create(   X   ,   o  ,   o   ) );
                table.Add( "src"      , Create(   o   ,   X  ,   o   ) );
                table.Add( "usemap"   , Create(   o   ,   X  ,   o   ) );
            }
            return table;
        }
    }
}
