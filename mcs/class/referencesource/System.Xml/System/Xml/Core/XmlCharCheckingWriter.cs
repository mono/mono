

//------------------------------------------------------------------------------
// <copyright file="XmlCharCheckingWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------






















using System;
using System.IO;
using System.Text;
using System.Xml.Schema;
using System.Collections;
using System.Diagnostics;






namespace System.Xml {

    //
    // XmlCharCheckingWriter
    //
    internal partial class XmlCharCheckingWriter : XmlWrappingWriter {


//
// Fields
//
        bool checkValues;
        bool checkNames;
        bool replaceNewLines;
        string newLineChars;

        XmlCharType xmlCharType;
// 
// Constructor
//
        internal XmlCharCheckingWriter( XmlWriter baseWriter, bool checkValues, bool checkNames, bool replaceNewLines, string newLineChars ) 
            : base( baseWriter ) {

            Debug.Assert( checkValues || replaceNewLines );
            this.checkValues = checkValues;
            this.checkNames = checkNames;
            this.replaceNewLines = replaceNewLines;
            this.newLineChars = newLineChars;

            if ( checkValues ) {
                xmlCharType = XmlCharType.Instance;
            }
        }
        
//
// XmlWriter implementation
//

        public override XmlWriterSettings Settings { 
            get {
                XmlWriterSettings s = base.writer.Settings;
                s = ( s != null) ? (XmlWriterSettings)s.Clone() : new XmlWriterSettings();

                if ( checkValues ) {
                    s.CheckCharacters = true;
                }
                if ( replaceNewLines ) {
                    s.NewLineHandling = NewLineHandling.Replace;
                    s.NewLineChars = newLineChars;
                }
                s.ReadOnly = true;
                return s;
            }
        }


        public override void WriteDocType( string name, string pubid, string sysid, string subset ) { 
            if ( checkNames ) {
                ValidateQName( name );
            }
            if ( checkValues ) {
                if ( pubid != null ) {
                    int i;
                    if ( ( i = xmlCharType.IsPublicId( pubid ) ) >= 0 ) {
                        throw XmlConvert.CreateInvalidCharException( pubid, i );
                    }
                }
                if ( sysid != null ) {
                    CheckCharacters( sysid );
                }
                if ( subset != null ) {
                    CheckCharacters( subset );
                }
            }
            if ( replaceNewLines ) {
                sysid = ReplaceNewLines( sysid );
                pubid = ReplaceNewLines( pubid );
                subset = ReplaceNewLines( subset );
            }
            writer.WriteDocType( name, pubid, sysid, subset );
        }

        public override void WriteStartElement( string prefix, string localName, string ns ) {
            if ( checkNames ) {
                if ( localName == null || localName.Length == 0 ) {
                    throw new ArgumentException( Res.GetString( Res.Xml_EmptyLocalName ) );
                }
                ValidateNCName( localName );

                if ( prefix != null && prefix.Length > 0 ) {
                    ValidateNCName( prefix );
                }
            }
            writer.WriteStartElement( prefix, localName, ns );
        }

        public override void WriteStartAttribute( string prefix, string localName, string ns ) {
            if ( checkNames ) {
                if ( localName == null || localName.Length == 0 ) {
                    throw new ArgumentException( Res.GetString( Res.Xml_EmptyLocalName ) );
                }
                ValidateNCName( localName );

                if ( prefix != null && prefix.Length > 0 ) {
                    ValidateNCName( prefix );
                }
            }
            writer.WriteStartAttribute( prefix, localName, ns );
        }

        public override void WriteCData( string text ) {
            if ( text != null ) {
                if ( checkValues ) {
                    CheckCharacters( text );
                }
                if ( replaceNewLines ) {
                    text = ReplaceNewLines( text );
                }
                int i;
                while ( ( i = text.IndexOf( "]]>", StringComparison.Ordinal ) ) >= 0 ) {
                    writer.WriteCData( text.Substring( 0, i + 2 ) );
                    text = text.Substring( i + 2 );
                }
            }
            writer.WriteCData( text );
        }

        public override void WriteComment( string text ) {
            if ( text != null ) {
                if ( checkValues ) {
                    CheckCharacters( text );
                    text = InterleaveInvalidChars( text, '-', '-' );
                }
                if ( replaceNewLines ) {
                    text = ReplaceNewLines( text );
                }
            }
            writer.WriteComment( text );
        }

        public override void WriteProcessingInstruction( string name, string text ) {
            if ( checkNames ) {
                ValidateNCName( name );
            }
            if ( text != null ) {
                if ( checkValues ) {
                    CheckCharacters( text );
                    text = InterleaveInvalidChars( text, '?', '>' );
                }
                if ( replaceNewLines ) {
                    text = ReplaceNewLines( text );
                }
            }
            writer.WriteProcessingInstruction( name, text );
        }

        public override void WriteEntityRef( string name ) {
            if ( checkNames ) {
                ValidateQName( name );
            }
            writer.WriteEntityRef( name );
        }

        public override void WriteWhitespace( string ws ) {
            if ( ws == null ) {
                ws = string.Empty;
            }
            // "checkNames" is intentional here; if false, the whitespaces are checked in XmlWellformedWriter
            if ( checkNames ) {
                int i;
                if ( ( i = xmlCharType.IsOnlyWhitespaceWithPos( ws ) ) != -1 ) {
                    throw new ArgumentException( Res.GetString( Res.Xml_InvalidWhitespaceCharacter, XmlException.BuildCharExceptionArgs( ws, i ) ) );
                }
            }
            if ( replaceNewLines ) {
                ws = ReplaceNewLines( ws );
            }
            writer.WriteWhitespace( ws );
        }

        public override void WriteString( string text ) {
            if ( text != null ) {
                if ( checkValues ) {
                    CheckCharacters( text );
                }
                if ( replaceNewLines && WriteState != WriteState.Attribute ) {
                    text = ReplaceNewLines( text );
                }
            }
            writer.WriteString( text );
        }

        public override void WriteSurrogateCharEntity( char lowChar, char highChar ) {
            writer.WriteSurrogateCharEntity( lowChar, highChar );
        }

        public override void WriteChars( char[] buffer, int index, int count ) {
            if (buffer == null) {
                throw new ArgumentNullException("buffer");
            }
            if (index < 0) {
                throw new ArgumentOutOfRangeException("index");
            }
            if (count < 0) {
                throw new ArgumentOutOfRangeException("count");
            }
            if (count > buffer.Length - index) {
                throw new ArgumentOutOfRangeException("count");
            }

            if (checkValues) {
                CheckCharacters( buffer, index, count );
            }
            if ( replaceNewLines && WriteState != WriteState.Attribute ) {
                string text = ReplaceNewLines( buffer, index, count );
                if ( text != null ) {
                    WriteString( text );
                    return;
                }
            }
            writer.WriteChars( buffer, index, count );
        }

        public override void WriteNmToken( string name ) { 
            if ( checkNames ) {
                if ( name == null || name.Length == 0 ) {
                    throw new ArgumentException( Res.GetString( Res.Xml_EmptyName ) );
                }
                XmlConvert.VerifyNMTOKEN( name );
            }
            writer.WriteNmToken( name );
        }

        public override void WriteName( string name ) {
            if ( checkNames ) {
                XmlConvert.VerifyQName( name, ExceptionType.XmlException );
            }
            writer.WriteName( name );
        }

        public override void WriteQualifiedName( string localName, string ns ) {
            if ( checkNames ) {
                ValidateNCName( localName );
            }
            writer.WriteQualifiedName( localName, ns );
        }


//
//  Private methods
//
        private void CheckCharacters( string str ) {
            XmlConvert.VerifyCharData( str, ExceptionType.ArgumentException );
        }

        private void CheckCharacters( char[] data, int offset, int len ) {
            XmlConvert.VerifyCharData( data, offset, len, ExceptionType.ArgumentException );
        }

        private void ValidateNCName( string ncname ) {
            if ( ncname.Length == 0 ) {
                throw new ArgumentException( Res.GetString( Res.Xml_EmptyName ) );
            }
            int len = ValidateNames.ParseNCName( ncname, 0 );
            if ( len != ncname.Length ) {
                throw new ArgumentException(Res.GetString(len == 0 ? Res.Xml_BadStartNameChar : Res.Xml_BadNameChar, XmlException.BuildCharExceptionArgs(ncname, len)));
            }
        }

        private void ValidateQName( string name ) {
            if ( name.Length == 0 ) {
                throw new ArgumentException( Res.GetString( Res.Xml_EmptyName ) );
            }
            int colonPos;
            int len = ValidateNames.ParseQName( name, 0, out colonPos );
            if ( len != name.Length ) {
                string res = ( len == 0 || ( colonPos > -1 && len == colonPos + 1 ) ) ? Res.Xml_BadStartNameChar : Res.Xml_BadNameChar;
                throw new ArgumentException( Res.GetString( res, XmlException.BuildCharExceptionArgs( name, len ) ) );
            }
        }

        private string ReplaceNewLines( string str ) {
            if ( str == null ) {
                return null;
            }

            StringBuilder sb = null;
            int start = 0;
            int i;
            for ( i = 0; i < str.Length; i++ ) {
                char ch;
                if ( ( ch = str[i] ) >= 0x20 ) {
                    continue;
                }
                if ( ch == '\n' ) {
                    if ( newLineChars == "\n" ) {
                        continue;
                    }
                    if ( sb == null ) {
                        sb = new StringBuilder( str.Length + 5 );
                    }
                    sb.Append( str, start, i - start );
                }
                else if ( ch == '\r' ) {
                    if ( i + 1 < str.Length && str[i+1] == '\n' ) {
                        if ( newLineChars == "\r\n" ) {
                            i++;
                            continue;
                        }
                        if ( sb == null ) {
                            sb = new StringBuilder( str.Length  + 5 );
                        }
                        sb.Append( str, start, i - start );
                        i++;
                    }
                    else {
                        if ( newLineChars == "\r" ) {
                            continue;
                        }
                        if ( sb == null ) {
                            sb = new StringBuilder( str.Length + 5 );
                        }
                        sb.Append( str, start, i - start );
                    }
                }
                else {
                    continue; 
                }
                sb.Append( newLineChars );
                start = i + 1;
            }

            if ( sb == null ) {
                return str;
            }
            else {
                sb.Append( str, start, i - start );
                return sb.ToString();
            }
        }

        private string ReplaceNewLines( char[] data, int offset, int len ) {
            if ( data == null ) {
                return null;
            }

            StringBuilder sb = null;
            int start = offset;
            int endPos = offset + len;
            int i;
            for ( i = offset; i < endPos; i++ ) {
                char ch;
                if ( ( ch = data[i] ) >= 0x20 ) {
                    continue;
                }
                if ( ch == '\n' ) {
                    if ( newLineChars == "\n" ) {
                        continue;
                    }
                    if ( sb == null ) {
                        sb = new StringBuilder( len + 5 );
                    }
                    sb.Append( data, start, i - start );
                }
                else if ( ch == '\r' ) {
                    if ( i + 1 < endPos && data[i+1] == '\n' ) {
                        if ( newLineChars == "\r\n" ) {
                            i++;
                            continue;
                        }
                        if ( sb == null ) {
                            sb = new StringBuilder( len + 5 );
                        }
                        sb.Append( data, start, i - start );
                        i++;
                    }
                    else {
                        if ( newLineChars == "\r" ) {
                            continue;
                        }
                        if ( sb == null ) {
                            sb = new StringBuilder( len + 5 );
                        }
                        sb.Append( data, start, i - start );
                    }
                }
                else {
                    continue; 
                }
                sb.Append( newLineChars );
                start = i + 1;
            }

            if ( sb == null ) {
                return null;
            }
            else {
                sb.Append( data, start, i - start );
                return sb.ToString();
            }
        }

        // Interleave 2 adjacent invalid chars with a space. This is used for fixing invalid values of comments and PIs. 
        // Any "--" in comment must be replaced with "- -" and any "-" at the end must be appended with " ".
        // Any "?>" in PI value must be replaced with "? >". 
        // This code has a bug SQL BU Defect Tracking #480848, which was triaged as Won't Fix because it is a breaking change
        private string InterleaveInvalidChars( string text, char invChar1, char invChar2 ) {
            StringBuilder sb = null;
            int start = 0;
            int i;
            for ( i = 0; i < text.Length; i++ ) {
                if ( text[i] != invChar2 ) {
                    continue;
                }
                if ( i > 0 && text[i-1] == invChar1 ) {
                    if ( sb == null ) {
                        sb = new StringBuilder( text.Length + 5 );
                    }
                    sb.Append( text, start, i - start );
                    sb.Append( ' ' );
                    start = i;
                }
            }

            // check last char & return
            if ( sb == null ) {
                return (i == 0 || text[i - 1] != invChar1) ? text : (text + ' ');
            }
            else {
                sb.Append( text, start, i - start );
                if (i > 0 && text[i - 1] == invChar1) {
                    sb.Append(' ');
                }
                return sb.ToString();
            }
        }

    }
}

