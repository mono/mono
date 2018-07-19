
using System;
using System.IO;
using System.Text;
using System.Xml.Schema;
using System.Collections;
using System.Diagnostics;

using System.Threading.Tasks;

namespace System.Xml {

    //
    // XmlCharCheckingWriter
    //
    internal partial class XmlCharCheckingWriter : XmlWrappingWriter {

        public override Task WriteDocTypeAsync( string name, string pubid, string sysid, string subset ) { 
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
            return writer.WriteDocTypeAsync(name, pubid, sysid, subset);
        }

        public override Task WriteStartElementAsync( string prefix, string localName, string ns ) {
            if ( checkNames ) {
                if ( localName == null || localName.Length == 0 ) {
                    throw new ArgumentException( Res.GetString( Res.Xml_EmptyLocalName ) );
                }
                ValidateNCName( localName );

                if ( prefix != null && prefix.Length > 0 ) {
                    ValidateNCName( prefix );
                }
            }
            return writer.WriteStartElementAsync(prefix, localName, ns);
        }

        protected internal override Task WriteStartAttributeAsync( string prefix, string localName, string ns ) {
            if ( checkNames ) {
                if ( localName == null || localName.Length == 0 ) {
                    throw new ArgumentException( Res.GetString( Res.Xml_EmptyLocalName ) );
                }
                ValidateNCName( localName );

                if ( prefix != null && prefix.Length > 0 ) {
                    ValidateNCName( prefix );
                }
            }
            return writer.WriteStartAttributeAsync(prefix, localName, ns);
        }

        public override async Task WriteCDataAsync( string text ) {
            if ( text != null ) {
                if ( checkValues ) {
                    CheckCharacters( text );
                }
                if ( replaceNewLines ) {
                    text = ReplaceNewLines( text );
                }
                int i;
                while ( ( i = text.IndexOf( "]]>", StringComparison.Ordinal ) ) >= 0 ) {
                    await writer.WriteCDataAsync( text.Substring( 0, i + 2 ) ).ConfigureAwait(false);
                    text = text.Substring( i + 2 );
                }
            }
            await writer.WriteCDataAsync( text ).ConfigureAwait(false);
        }

        public override Task WriteCommentAsync( string text ) {
            if ( text != null ) {
                if ( checkValues ) {
                    CheckCharacters( text );
                    text = InterleaveInvalidChars( text, '-', '-' );
                }
                if ( replaceNewLines ) {
                    text = ReplaceNewLines( text );
                }
            }
            return writer.WriteCommentAsync( text );
        }

        public override Task WriteProcessingInstructionAsync( string name, string text ) {
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
            return writer.WriteProcessingInstructionAsync(name, text);
        }

        public override Task WriteEntityRefAsync( string name ) {
            if ( checkNames ) {
                ValidateQName( name );
            }
            return writer.WriteEntityRefAsync(name);
        }

        public override Task WriteWhitespaceAsync( string ws ) {
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
            return writer.WriteWhitespaceAsync(ws);
        }

        public override Task WriteStringAsync( string text ) {
            if ( text != null ) {
                if ( checkValues ) {
                    CheckCharacters( text );
                }
                if ( replaceNewLines && WriteState != WriteState.Attribute ) {
                    text = ReplaceNewLines( text );
                }
            }
            return writer.WriteStringAsync(text);
        }

        public override Task WriteSurrogateCharEntityAsync( char lowChar, char highChar ) {
            return writer.WriteSurrogateCharEntityAsync(lowChar, highChar);
        }

        public override Task WriteCharsAsync( char[] buffer, int index, int count ) {
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
                    return WriteStringAsync(text);
                }
            }
            return writer.WriteCharsAsync( buffer, index, count );
        }

        public override Task WriteNmTokenAsync( string name ) { 
            if ( checkNames ) {
                if ( name == null || name.Length == 0 ) {
                    throw new ArgumentException( Res.GetString( Res.Xml_EmptyName ) );
                }
                XmlConvert.VerifyNMTOKEN( name );
            }
            return writer.WriteNmTokenAsync(name);
        }

        public override Task WriteNameAsync( string name ) {
            if ( checkNames ) {
                XmlConvert.VerifyQName( name, ExceptionType.XmlException );
            }
            return writer.WriteNameAsync(name);
        }

        public override Task WriteQualifiedNameAsync( string localName, string ns ) {
            if ( checkNames ) {
                ValidateNCName( localName );
            }
            return writer.WriteQualifiedNameAsync(localName, ns);
        }

    }
}

