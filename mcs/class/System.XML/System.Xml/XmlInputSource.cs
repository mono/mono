// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Xml.XmlInputSource.cs
//	port of Open Xml TXmlInputSource class
//
// Author:
//   Daniel Weber (daniel-weber@austin.rr.com
//
// (C) 2001 Daniel Weber
//		
//

using System;
using System.IO;

namespace System.Xml
{
	internal class XmlInputSource
	{
		private DomEncodingType Fencoding;

		private string FpublicID;
		private string FsystemID;
		public string FrootName;

		private Stream FStream;

		private bool FLastCharWasCR;

		// locator
		int FColumnNumber;
		int FStartColumnNumber;
		int FStartLineNumber;
		bool FLastWCharWasLF;
		int FLineNumber;
		bool FPieceEndSet;

		// Buffer storage for UTF-8 surrogates
		// see http://www.ietf.org/rfc/rfc2279.txt for a complete description of UTF-8 encoding
		private int FLastUcs4;				


		//FLocator: TdomStandardLocator;

		// public properties
		//===========================================================================
		public DomEncodingType encoding
		{
			get
			{
				return Fencoding;
			}
		}

		//property locator: TdomStandardLocator read FLocator;
		public string publicId
		{
			get
			{
				return FpublicID;
			}
		}

		public string rootName 
		{
			get
			{
				return FrootName;
			}
		}

		public Stream stream
		{
			get
			{
				return FStream;
			}
		}

		public string streamAsWideString
		{
			get
			{
				return string.Empty;
				//wideString read getStreamAsWideString;
			}
		}

		public string systemId
		{
			get 
			{
				return FsystemID;
			}
		}

		public int columnNumber
		{
			get { return FColumnNumber; }
		}

		public int lineNumber
		{
			get { return FLineNumber; }
		}

		public int startColumnNumber
		{
			get { return FStartColumnNumber; }
		}
		
		public int startLineNumber
		{
			get { return FStartLineNumber; }
		}

		// private methods
		//===========================================================================
		/// <summary>
		/// Analyze the first bytes of an XML document to try and determine encoding
		/// </summary>
		/// <returns>Determined encoding type, defaults to UTF-8</returns>
		private void setEncodingType()
		{
			try
			{
				byte[] buf = new byte[4];

				FStream.Seek(0, SeekOrigin.Begin);
				FStream.Read(buf, 0, 4);

				// UTF-16 code streams should begin with 0xfeff for big-endian systems
				//	or 0xfffe for little endian systems.
				// check that first....
				if ( (buf[0] == 0xfe) & (buf[1] == 0xff) )
					Fencoding = DomEncodingType.etUTF16BE;
				else if ( (buf[0] == 0xff) & (buf[1] == 0xfe) )
					Fencoding = DomEncodingType.etUTF16LE;
				else
				{
					// assume utf-8, look for encoding in <?xml version="1.0" encoding="ISO-8859-6"> tag, eg
					Fencoding = DomEncodingType.etUTF8;

					// Check if the stream begins with <?[X|x][M|m][L|l]
					if ( (buf[0] == 0x3c) & (buf[1] == 0x3f) &			// "<?"    
						 ((buf[2] == 0x78) | (buf[2] ==0x58) ) &		// "x" or "X"
						( (buf[2] == 0x6d) | (buf[3] ==0x4d) ) &		// "m" or "M"
						( (buf[2] == 0x6c) | (buf[2] ==0x4c) ) )		// "l" or "L"
					{
						
						string tag = "";
						while (FStream.Position != FStream.Length)
						{
							char c = getNextChar();
							tag += c;

							if (c  == '>')
								break;
						}

						// start from the location of "encoding", and scan for quotes
						string encodeString = readEncodingAttrFromTag(tag);
						encodeString = encodeString.ToUpper();

						if ( (encodeString.IndexOf("ISO-8859-1") != -1) |
								(encodeString.IndexOf("LATIN-1") != -1) )
							Fencoding = DomEncodingType.etLatin1;
						else if ( (encodeString.IndexOf("ISO-8859-2") != -1) |
							(encodeString.IndexOf("LATIN-2") != -1) )
							Fencoding = DomEncodingType.etLatin2;
						else if ( (encodeString.IndexOf("ISO-8859-3") != -1) |
							(encodeString.IndexOf("LATIN-3") != -1) )
							Fencoding = DomEncodingType.etLatin3;
						else if ( (encodeString.IndexOf("ISO-8859-4") != -1) |
							(encodeString.IndexOf("LATIN-4") != -1) )
							Fencoding = DomEncodingType.etLatin4;
						else if ( (encodeString.IndexOf("ISO-8859-5") != -1) |
							(encodeString.IndexOf("CYRILLIC") != -1) )
							Fencoding = DomEncodingType.etCyrillic;
						else if ( (encodeString.IndexOf("ISO-8859-6") != -1) |
							(encodeString.IndexOf("ARABIC") != -1) )
							Fencoding = DomEncodingType.etArabic;
						else if ( (encodeString.IndexOf("ISO-8859-7") != -1) |
							(encodeString.IndexOf("GREEK") != -1) )
							Fencoding = DomEncodingType.etGreek;
						else if ( (encodeString.IndexOf("ISO-8859-8") != -1) |
							(encodeString.IndexOf("HEBREW") != -1) )
							Fencoding = DomEncodingType.etHebrew;
						else if ( (encodeString.IndexOf("ISO-8859-9") != -1) |
							(encodeString.IndexOf("LATIN-5") != -1) )
							Fencoding = DomEncodingType.etLatin5;
						else if ( (encodeString.IndexOf("ISO-8859-10") != -1) |
							(encodeString.IndexOf("LATIN-6") != -1) )
							Fencoding = DomEncodingType.etLatin6;
						else if ( (encodeString.IndexOf("ISO-8859-13") != -1) |
							(encodeString.IndexOf("LATIN-7") != -1) )
							Fencoding = DomEncodingType.etLatin7;
						else if ( (encodeString.IndexOf("ISO-8859-14") != -1) |
							(encodeString.IndexOf("LATIN-8") != -1) )
							Fencoding = DomEncodingType.etLatin8;
						else if ( (encodeString.IndexOf("ISO-8859-15") != -1) |
							(encodeString.IndexOf("LATIN-9") != -1) )
							Fencoding = DomEncodingType.etLatin9;
						else if (encodeString.IndexOf("KOI8-R") != -1)
							Fencoding = DomEncodingType.etKOI8R;
						else if (encodeString.IndexOf("CP10000_MACROMAN") != -1)
							Fencoding = DomEncodingType.etcp10000_MacRoman;
						else if ( (encodeString.IndexOf("Windows-1250") != -1) |
									(encodeString.IndexOf("CP1250") != -1) )
							Fencoding = DomEncodingType.etcp1250;
						else if ( (encodeString.IndexOf("Windows-1251") != -1) |
									(encodeString.IndexOf("CP1251") != -1) )
							Fencoding = DomEncodingType.etcp1251;
						else if ( (encodeString.IndexOf("Windows-1252") != -1) |
									(encodeString.IndexOf("CP1252") != -1) )
							Fencoding = DomEncodingType.etcp1252;
						}
					}

				}
			catch
			{
				Fencoding = DomEncodingType.etUTF8;
			}

			FStream.Seek(0, SeekOrigin.Begin);
		}

		/// <summary>
		/// Helper function to try and find the encoding attribute value in 
		/// declaration tag.  Does not do well-formedness checks.
		/// </summary>
		/// <param name="tag">string to scan</param>
		/// <exception cref="InvalidOperationException">If bad encoding char found, mis-matched quotes, or no equals sign.</exception>
		/// <returns>encoding, or string.Empty if it is not found.</returns>
		private string readEncodingAttrFromTag( string tag )
		{
			int encodeIndex = tag.IndexOf("encoding");
			if ( encodeIndex == -1)
				return string.Empty;
			else
			{
				int curIndex = encodeIndex + "encoding".Length;
				bool firstQuoteFound = false;
				bool equalsFound = false;
				char quoteChar = (char) 0xffff;			// c# insists on initialization...
				string encoding = "";

				while ( curIndex != tag.Length )
				{
					char c = tag[curIndex];
					curIndex++;

					if ( c == '=')
					{
						equalsFound = true;
						continue;
					}

					if ( (c== '\"') | (c=='\'') )
					{
						if ( !firstQuoteFound & !equalsFound)
							throw new InvalidOperationException("No equals sign found in encoding attribute");
						else if ( firstQuoteFound )
						{
							if (c == quoteChar)
								return encoding;
							else
								throw new InvalidOperationException("non-matching quotes in attribute value");
						}
						else
						{
							firstQuoteFound = true;
							quoteChar = c;
							continue;
						}
					}
					else if (firstQuoteFound)
					{
						if ( ( c >= 'a') & ( c <= 'z'))			encoding += c;
						else if ( ( c >= 'A') & ( c <= 'Z'))	encoding += c;
						else if ( ( c >= '0') & ( c <= '9'))	encoding += c;
						else if ( c == '_' )					encoding += c;
						else if ( c == '-')						encoding += c;
						else if (c == '.')						encoding += c;
						else
							throw new InvalidOperationException("invalid character in encoding attribute");
					}
				}
				return string.Empty;
			}
		}
		
		private void evaluate(char c)
		{
			if (FLastWCharWasLF)
			{
				FLineNumber++;
				FLastWCharWasLF = false;
				FColumnNumber = 1;
			}
			else
				FColumnNumber++;

			if (c == (char) 10 )
				FLastWCharWasLF = true;

			if (FPieceEndSet)
				pieceStart();
		}

		public void pieceEnd()
		{
			FPieceEndSet = true;
		}

		public void pieceStart()
		{
			FStartColumnNumber = FColumnNumber;
			FStartLineNumber =   FLineNumber;
			FPieceEndSet = false;
		}

		/// <summary>
		/// Return true if input stream is at EOF.
		/// </summary>
		/// <returns></returns>
		public bool atEOF()
		{
			return (FStream.Length == FStream.Position);
		}
	
		/// <summary>
		/// Sets the internal root name by analyzing the tags at the beginning of the stream.
		/// root name is:
		/// - the element tag of the first element found
		/// - the root name listed in a !DOCTYPE tag
		/// - empty if a parse error occurs, or no applicable tags are found.
		/// Does not do well-formedness checks - skips comments and proc. instructions
		/// </summary>
		private void getRootName()
		{
			reset();
			FrootName = string.Empty;
			
			while ( ! atEOF() )
			{
				string tag = "<";
				char c = getNextChar();

				// skip whitespace to first tag
				while ( !atEOF() && (XmlNames_1_0.IsXmlWhiteSpace( c )) )
					c = getNextChar();
				if ( (c != '<') | atEOF() ) break;

				while ( !atEOF() & ( c != '>' ) )
				{
					c = getNextChar();
					tag += c;
				}
				if ( atEOF() ) break;

				// Only allow 1) comments, 2) processing instructions before <!DOCTYPE ...>
				if ( tag.StartsWith("<?") )				// Processing instruction
					continue;
				else if ( tag.StartsWith("<--") )		// comment
					continue;
				else if ( tag.StartsWith("<!DOCTYPE") )	// what we're looking for...
				{
					setRootName( tag );
					break;
				}
				// no DOCTYPE tag?  Use the first element tag as the root
				else if ( tag.StartsWith( "<" ) )
					setRootName( tag );
				// we hit a non-comment, processing instruction or declaration, we ain't gonna get it
				else
				{
					FrootName = string.Empty;
					break;
				}
			}
		 }

		private void setRootName( string doctypeTag )
		{
			int start = doctypeTag.IndexOf("<DOCTYPE");
			if ( start == -1 ) 
				start = 1;				// set from element
			else
				start += "<DOCTYPE".Length;
			while ( ( start != doctypeTag.Length ) & XmlNames_1_0.IsXmlWhiteSpace( doctypeTag[start] ) )
				start++;
			
			string tmp = string.Empty;

			while ( ( start != doctypeTag.Length ) && 
					!XmlNames_1_0.IsXmlWhiteSpace(doctypeTag[start])  &&
					(doctypeTag[start] != '>') &&
					(doctypeTag[start] != '[') && 
					(doctypeTag[start] != '/') )
				tmp += doctypeTag[start];

			if (XmlNames_1_0.isXmlName(tmp) ) FrootName = tmp;
		}

		/// <summary>
		/// Read in the next character (either UTF-8 or UTF-16) and convert by charset
		/// Normalize CR/LF pairs to single CR.
		/// </summary>
		/// <returns></returns>
		public char getNextChar()
		{
			byte[] buf = new byte[2];
			char retval = (char) 0xffff;
			int bCount;

			switch(Fencoding)
			{
				case DomEncodingType.etLatin1:
					bCount = stream.Read(buf,0, 1);
					if (bCount == 1) 
						retval = XmlUtil.Iso8859_1ToUTF16Char(buf[0]);
					break;
				case DomEncodingType.etLatin2:
					bCount = stream.Read(buf, 0, 1);
					if (bCount == 1) 
						retval = XmlUtil.Iso8859_2ToUTF16Char(buf[0]);
					break;
				case DomEncodingType.etLatin3:
					bCount = stream.Read(buf, 0, 1);
					if (bCount == 1) 
						retval = XmlUtil.Iso8859_3ToUTF16Char(buf[0]);
					break;
				case DomEncodingType.etLatin4:
					bCount = stream.Read(buf, 0, 1);
					if (bCount == 1) 
						retval = XmlUtil.Iso8859_4ToUTF16Char(buf[0]);
					break;
				case DomEncodingType.etCyrillic:
					bCount = stream.Read(buf, 0, 1);
					if (bCount == 1) 
						retval = XmlUtil.Iso8859_5ToUTF16Char(buf[0]);
					break;
				case DomEncodingType.etArabic:
					bCount = stream.Read(buf, 0, 1);
					if (bCount == 1) 
						retval = XmlUtil.Iso8859_6ToUTF16Char(buf[0]);
					break;
				case DomEncodingType.etGreek:
					bCount = stream.Read(buf, 0, 1);
					if (bCount == 1) 
						retval = XmlUtil.Iso8859_7ToUTF16Char(buf[0]);
					break;
				case DomEncodingType.etHebrew:
					bCount = stream.Read(buf, 0, 1);
					if (bCount == 1) 
						retval = XmlUtil.Iso8859_8ToUTF16Char(buf[0]);
					break;
				case DomEncodingType.etLatin5:
					bCount = stream.Read(buf, 0, 1);
					if (bCount == 1) 
						retval = XmlUtil.Iso8859_9ToUTF16Char(buf[0]);
					break;
				case DomEncodingType.etLatin6:
					bCount = stream.Read(buf, 0, 1);
					if (bCount == 1) 
						retval = XmlUtil.Iso8859_10ToUTF16Char(buf[0]);
					break;
				case DomEncodingType.etLatin7:
					bCount = stream.Read(buf, 0, 1);
					if (bCount == 1) 
						retval = XmlUtil.Iso8859_13ToUTF16Char(buf[0]);
					break;
				case DomEncodingType.etLatin8:
					bCount = stream.Read(buf, 0, 1);
					if (bCount == 1) 
						retval = XmlUtil.Iso8859_14ToUTF16Char(buf[0]);
					break;
				case DomEncodingType.etLatin9:
					bCount = stream.Read(buf, 0, 1);
					if (bCount == 1) 
						retval = XmlUtil.Iso8859_15ToUTF16Char(buf[0]);
					break;
				case DomEncodingType.etKOI8R:
					bCount = stream.Read(buf, 0, 1);
					if (bCount == 1) 
						retval = XmlUtil.KOI8_RToUTF16Char(buf[0]);
					break;
				case DomEncodingType.etcp10000_MacRoman:
					bCount = stream.Read(buf, 0, 1);
					if (bCount == 1) 
						retval = XmlUtil.cp10000_MacRomanToUTF16Char(buf[0]);
					break;
				case DomEncodingType.etcp1250:
					bCount = stream.Read(buf, 0, 1);
					if (bCount == 1) 
						retval = XmlUtil.cp1250ToUTF16Char(buf[0]);
					break;
				case DomEncodingType.etcp1251:
					bCount = stream.Read(buf, 0, 1);
					if (bCount == 1) 
						retval = XmlUtil.cp1251ToUTF16Char(buf[0]);
					break;
				case DomEncodingType.etcp1252:
					bCount = stream.Read(buf, 0, 1);
					if (bCount == 1) 
						retval = XmlUtil.cp1252ToUTF16Char(buf[0]);
					break;
				case DomEncodingType.etUTF8:
					if ( FLastUcs4 >= 0x10000)
					{
						// Output low surrogate
						retval = XmlUtil.Utf16LowSurrogate(FLastUcs4);
						FLastUcs4 = 0;
					}
					else
					{
						FLastUcs4 = XmlUtil.ReadUTF8Char( stream );
						if ( FLastUcs4 >= 0x10000)
							retval = XmlUtil.Utf16HighSurrogate(FLastUcs4);
						else
							retval = (char) FLastUcs4;
					}
					break;
				case DomEncodingType.etUTF16BE:
					bCount = stream.Read(buf, 0, 2);
					if (bCount == 2)
						retval = System.Convert.ToChar( (buf[0] << 16) + buf[1] );
						break;
				case DomEncodingType.etUTF16LE:
					bCount = stream.Read(buf, 0, 2);
					if (bCount == 2)
						retval = System.Convert.ToChar( (buf[1] << 16) + buf[0] );
					break;
			}

			// normalize CRLF or a single CR to LF:
			if ( (retval == 0x000D) & FLastCharWasCR)		// 0x000d = CR
			{
				FLastCharWasCR = false;
				return getNextChar();
			}
			else if ( retval == 0x000A)						// 0x000a = LF
			{
				FLastCharWasCR = true;
				return (char) 0x000D;
			}
			else
				FLastCharWasCR = false;
			  
			evaluate(retval);
			return retval;
		}

		/// <summary>
		/// Reset the Input to the origin and clear internal variables.
		/// </summary>
		public void reset()
		{
			FLastUcs4 = 0;
            FLastCharWasCR = false;

			switch(Fencoding)
			{
				// skip the leading 0xfeff/oxfffe on UTF-16 streams
				case DomEncodingType.etUTF16BE:
					FStream.Seek(2, SeekOrigin.Begin);
					break;
				case DomEncodingType.etUTF16LE:
					FStream.Seek(2, SeekOrigin.Begin);
					break;
				default:
					FStream.Seek(0, SeekOrigin.Begin);
					break;
			}

			FColumnNumber =      0;
			FLineNumber =        0;
			FStartColumnNumber = 0;
			FStartLineNumber =   0;
			FLastWCharWasLF = true;
			pieceEnd();
		}
        
/*
 * private
    

  protected
    function getStreamAsWideString: wideString; virtual;
    procedure skipTextDecl(const locator: TdomStandardLocator); virtual;
  public
    constructor create(const stream: TStream;
                       const publicId,
                             systemId: wideString); virtual;
    destructor destroy; override;


*/
		// Constructor
		//===========================================================================
		XmlInputSource(Stream inputStream, string publicID, string systemID)
		{
			if (inputStream == null)
				throw new NullReferenceException("Null stream passed to XmlInputSource constructor");

			FStream = inputStream;
			FLastUcs4 = 0;
			FLastCharWasCR = false;
			FpublicID = publicID;
			FsystemID = systemID;
			setEncodingType();
			//FLocator:= TdomStandardLocator.create(self);
			getRootName();
		}
	}

}
