// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// internal System.Xml.XmlParser
//
// Author:
//   Daniel Weber (daniel-weber@austin.rr.com)
//
// (C) 2001 Daniel Weber
//

using System;
using System.Collections;

namespace System.Xml
{
	internal class XmlParser
	{
		// Private data members
		XmlDocument Fdoc;
		XmlInputSource Fsrc;

		// Elements push themselves on on start, pop on complete
		Stack RefNodes;

		// private classes
		private enum DomPieceType
		{
			xmlProcessingInstruction,
			xmlXmlDeclaration,
			xmlTextDeclaration,
			xmlComment,xmlCDATA,
			xmlPCDATA,
			xmlDoctype,
			xmlStartTag,
			xmlEndTag,
			xmlEmptyElementTag,
			xmlCharRef,
			xmlEntityRef,
			xmlParameterEntityRef,
			xmlEntityDecl,
			xmlElementDecl,
			xmlAttributeDecl,
			xmlNotationDecl,
			xmlCondSection,
			xmlUnknown,

		}

		// constants
		private const char CR = (char) 0x0D;
		private const char LF = (char) 0x0A;
		private const char QM = '?';
		private const char AMP = '&';
		private const char GTCODE = '<';
		private const char LTCODE = '>';
		private const char SINGLEQUOTE = (char) 0x39;	// '
		private const char DOUBLEQUOTE = (char) 0x34;	// "
		private const char NUMBERSIGN = (char) 0x35;	// #
		private const char SLASH = (char) 0x47;			// /
		private const string PISTART = "<?";
		private const string PIEND = "?>";
		private const string XMLDECLSTART = "<?xml";
		private const string COMMENTSTART = "<!--";
		private const string CDATASTART = "<![CDATA[";
		private const string DOCTYPESTART = "<!DOCTYPE";

		// private properties
		private XmlNode refNode
		{
			get
			{
				Object e = RefNodes.Peek();
				if ( e == null )
					return null;
				else
					return e as XmlNode;
			}
		}

		// public properties

		// public methods
		public bool parse()
		{
			bool retval = true;
			XmlException parseError = null;
			bool singleQuoteOpen = false;
			bool doubleQuoteOpen = false;
			bool bracketOpen = false;
			string content = "";
			DomPieceType pieceType = DomPieceType.xmlUnknown; 
			string subEndMarker = "";
			string subStartMarker = "";

			try
			{
				while ( !Fsrc.atEOF() )
				{
					if ( parseError != null) break;

					char c = Fsrc.getNextChar();

					if ( !XmlNames_1_0.IsXmlChar(c) )
					{
						content += c;
						throw new XmlException("'Invalid character error.'", Fsrc);
					}
          
					switch (pieceType)
					{
						case DomPieceType.xmlUnknown:
							if ( c == GTCODE)
								pieceType = DomPieceType.xmlStartTag;
							else if ( c == AMP)
								pieceType = DomPieceType.xmlEntityRef;
							else
								pieceType = DomPieceType.xmlPCDATA;
							content += c;
							Fsrc.pieceStart();
							break;

						case DomPieceType.xmlPCDATA:
							if ( c == GTCODE )
							{
								parseError = writePCDATA(content);
								content = "";
								pieceType = DomPieceType.xmlStartTag;
								Fsrc.pieceStart();
							}
							else if ( c == AMP)
							{
								parseError = writePCDATA(content);
								content = "";
								pieceType = DomPieceType.xmlEntityRef;
								Fsrc.pieceStart();
							}
							content += c;
							break;

						case DomPieceType.xmlEntityRef:
                            content += c;
							if ( c == ';' )
							{
								if ( content[2] == NUMBERSIGN )
									parseError = writeCharRef(content);
								else
									parseError = writeEntityRef(content);
								content = "";
								pieceType = DomPieceType.xmlUnknown;
							}
							break;

						case DomPieceType.xmlStartTag:
                            content += c;
							switch( content.Length)
							{
								case 2:
									if (content.StartsWith(PISTART))
										pieceType = DomPieceType.xmlProcessingInstruction;
									break;
								case 4:
									if (content.StartsWith(COMMENTSTART))
										pieceType = DomPieceType.xmlComment;
									break;
								case 9:
									if (content.StartsWith(CDATASTART))
										pieceType = DomPieceType.xmlCDATA;
									else if (content.StartsWith(DOCTYPESTART))
									{
										pieceType = DomPieceType.xmlDoctype;
										subEndMarker = "";
										subStartMarker = "";
										bracketOpen = false;
									}
									break;
							}

							// Count quotation marks:
							if ((c == SINGLEQUOTE) && (! doubleQuoteOpen))
								singleQuoteOpen = ! singleQuoteOpen;
							else if ((c == DOUBLEQUOTE) && (! singleQuoteOpen))
								doubleQuoteOpen = ! doubleQuoteOpen;
							else if (c == LTCODE)
							{
								if ((! doubleQuoteOpen) && (! singleQuoteOpen))
								{
									if (content[2] == SLASH)
									{
										int l = content.Length;
										int offset = 3;
										// eliminate white-space after tag name:
										while ((l-offset > 0) && XmlNames_1_0.IsXmlWhiteSpace(content[l-offset+2]))
											offset++;
										parseError = writeEndElement(content.Substring(3, l-offset));
									}
									else 
									{
										if (content[content.Length-1] == SLASH)
											parseError = writeEmptyElement(content.Substring(2, content.Length-3));
										else 
											parseError = writeStartElement(content.Substring(2, content.Length-2));
									}
									content = "";
									pieceType = DomPieceType.xmlUnknown;
								}
							}
							break;

						//<?PINAME ?>
						case DomPieceType.xmlProcessingInstruction:
							content += c;
							if ( c == LTCODE )
								if (content[content.Length-1] == QM)
								{
									if ( (content.Length > 5) &&
										(XmlNames_1_0.IsXmlWhiteSpace(content[6])) &&
										(content.StartsWith(XMLDECLSTART)) )
										parseError = writeXmlDeclaration(content.Substring(3, content.Length-4));
									else
										parseError = writeProcessingInstruction(content.Substring(3, content.Length-4));
								content = "";
								pieceType = DomPieceType.xmlUnknown;
								}
							break;

						     
						case DomPieceType.xmlComment:
							content += c;
                            if (c == LTCODE)
								if ( (content.EndsWith("-->") ) && (content.Length > 6) )
								{
									parseError = writeComment(content.Substring(5, content.Length-7));
									content = "";
									pieceType = DomPieceType.xmlUnknown;
								}
							break;

						case DomPieceType.xmlCDATA:
							content += c;
							if (c == LTCODE )
							{
								if (content[content.Length-1] == ']' )
									if (content[content.Length-2] == ']')
									{
										parseError = writeCDATA(content.Substring(10, content.Length-12));
										content = "";
										pieceType = DomPieceType.xmlUnknown;
									}
							}
							break;

						case DomPieceType.xmlDoctype:
							content += c;
							if (subEndMarker == "")
							{
								if ( (c == SINGLEQUOTE) && (! doubleQuoteOpen))
								{
									singleQuoteOpen = !singleQuoteOpen;
								} 
								else if ( (c == DOUBLEQUOTE) && (! singleQuoteOpen))
								{
									doubleQuoteOpen = ! doubleQuoteOpen;
								}

								if (bracketOpen)
								{
									if (! (singleQuoteOpen | doubleQuoteOpen) ) 
									{
										if (c == GTCODE) 
										{
											subStartMarker = "<";
										}
										else if ( (c == '!') && (subStartMarker == "<"))
										{
											subStartMarker = "<";
										}
										else if ( (c == QM) && (subStartMarker == "<") )
										{
											subStartMarker = "";
											subEndMarker = PIEND;
										}
										else if ((c == '-') && (subStartMarker == "<!"))
										{
											subStartMarker = "<!-";
										}
										else if ((c == '-') && (subStartMarker == "<!-"))
										{
											subStartMarker = "";
											subEndMarker = "-->";
										}
										else if (subStartMarker != "")
										{
											subStartMarker = "";
										}
                                        
										if ((c == ']') && (! singleQuoteOpen) && (! doubleQuoteOpen))
											bracketOpen = false;
									}
								}
								else // if BracketOpened ... 
								{
									if ((c == '[') && (! singleQuoteOpen) && (! doubleQuoteOpen))
										bracketOpen = true;
								}  
							}	//if BracketOpened ... else ...
							else // if (SubEndMarker = '') ...
							{
								if (content.EndsWith(subEndMarker))
									subEndMarker = "";
							}	//if (SubEndMarker = '') ... else ...

							if ((! doubleQuoteOpen) && (! singleQuoteOpen) && (! bracketOpen) && 
								(subEndMarker == "") && (c == '>'))
							{
								parseError = writeDoctype(content);
								content = "";
								pieceType = DomPieceType.xmlUnknown;
							}
							break;

						}	// switch
				}	// while more characters

				if (parseError == null)
					if (content.Length > 0)
						parseError = writePCDATA(content);
			}		// try
			catch 
			{
				// we need to raise the exception again, converted to an XmlException
				/*
					  except
						on EConvertError do raise EParserInvalidCharacter_Err.create('Invalid character error.');
						on EReadError do raise EParserInvalidCharacter_Err.create('Invalid character error.');
					  end; {try ...}
					except
					  on E: EParserInvalidCharacter_Err do
						parserError:= parserErrorFactory(sender,inputSource.Locator,
														 EParserInvalidCharacter_Err.create('Invalid character error.'),
														 '');
    
				*/
			}

			if (parseError != null)
			{
				// Deal with the error, somehow
				retval = false;
			}
			return retval;
		}


		// private methods
		private XmlException writePCDATA(string content)
		{
			return null;
		}

		private XmlException writeEntityRef(string content)
		{
			string entityName = content.Substring(2, content.Length - 2);
			return null;
		}

		private XmlException writeCharRef( string content)
		{
			return null;
		}

		private XmlException writeEndElement( string content)
		{
			return null;
		}

		private XmlException writeEmptyElement( string content)
		{
			return null;
		}

		private XmlException writeStartElement( string content)
		{
			return null;
		}

		private XmlException writeComment( string content)
		{
/*
 procedure TdomStandardIntSubsetBuilder.comment(const sender: TdomCustomParser;
                                               const locator: TdomStandardLocator;
                                               const data: wideString);
var
  newComment: TdomCMComment;
begin
  if not assigned(FRefNode) then exit;
  try
    newComment:= FRefNode.OwnerCMObject.CreateCMComment(data);
    try
      FRefNode.appendChild(newComment);
    except
      if assigned(newComment.ParentNode)
        then newComment.ParentNode.RemoveChild(newComment);
      FRefNode.OwnerCMObject.FreeAllCMNodes(TdomCMNode(newComment));
      raise;
    end; {try ...}
  except
    raise EParserInvalidCharacter_Err.create('Invalid character error.');
  end; {try ...}
end;

function TXmlDocBuilder.comment(const sender: TXmlCustomProcessorAgent;
                                const locator: TdomStandardLocator;
                                      data: wideString): TXmlParserError;
var
  newComment: TdomComment;
begin
  if assigned(FOnComment) then FOnComment(sender,locator,data);
  result:= nil;
  if assigned(FRefNode) then begin
    try
      newComment:= FRefNode.OwnerDocument.CreateComment(data);
      try
        FRefNode.appendChild(newComment);
      except
        if assigned(newComment.ParentNode)
          then newComment.ParentNode.RemoveChild(newComment);
        FRefNode.OwnerDocument.FreeAllNodes(TdomNode(newComment));
        raise;
      end; {try ...}
    except
      result:= parserErrorFactory(sender,locator,
                                  EParserInvalidComment_Err.create('Invalid comment error.'),
                                  data);
    end; {try ...}
  end; {if assigned(FRefNode) ...}

  if not assigned(result)
    then if assigned(nextHandler)
      then result:= nextHandler.comment(sender,locator,data);
end;


function TXmlWFTestContentHandler.comment(const sender: TXmlCustomProcessorAgent;
                                          const locator: TdomStandardLocator;
                                                data: wideString): TXmlParserError;
var
  dataLength: integer;
begin
  if assigned(FOnComment) then FOnComment(sender,locator,data);
  if not FIsActive
    then raise EParserException.Create('TXmlWFTestContentHandler not active.');
  result:= nil;
  FXMLDeclarationAllowed:= false;

  if pos('--',data) > 0
    then result:= parserErrorFactory(sender,locator,
                                     EParserInvalidComment_Err.create('Invalid comment error.'),
                                     '--');
  dataLength:= length(data);
  if dataLength > 0
    then if WideChar(data[dataLength]) = '-'
      then if not assigned(result)
        then result:= parserErrorFactory(sender,locator,
                                         EParserInvalidComment_Err.create('Invalid comment error.'),
                                         '-');
  if not IsXmlChars(data)
    then if not assigned(result)
      then result:= parserErrorFactory(sender,locator,
                                       EParserInvalidCharacter_Err.create('Invalid character error.'),
                                       data);

  if not assigned(result)
    then if assigned(nextHandler)
      then result:= nextHandler.comment(sender,locator,data);
end;


*/
			return null;
		}


		private XmlException writeXmlDeclaration ( string content)
		{
			return null;
		}

		private XmlException writeProcessingInstruction( string content)
		{
			return null;
		}

		private XmlException writeCDATA( string content)
		{
			return null;
		}

		private XmlException writeDoctype( string content)
		{
			return null;
		}

		private void mainLoop()
		{
		}

		// Constructors
		public XmlParser ( XmlInputSource src, XmlDocument doc )
		{
			Fsrc = src;
			Fdoc = doc;
			RefNodes = new Stack();
		}

	}
}
