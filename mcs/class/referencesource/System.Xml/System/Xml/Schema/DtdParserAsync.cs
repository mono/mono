
using System;
using System.IO;
using System.Text;
using System.Xml.Schema;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

#if SILVERLIGHT
using BufferBuilder=System.Xml.BufferBuilder;
#else
using BufferBuilder = System.Text.StringBuilder;
#endif

using System.Threading.Tasks;

namespace System.Xml {

    internal partial class DtdParser : IDtdParser {

//
// IDtdParser interface
//
#region IDtdParser Members

        async Task< IDtdInfo > IDtdParser.ParseInternalDtdAsync(IDtdParserAdapter adapter, bool saveInternalSubset) {
            Initialize(adapter);
            await ParseAsync(saveInternalSubset).ConfigureAwait(false);
            return schemaInfo;
        }

        async Task< IDtdInfo > IDtdParser.ParseFreeFloatingDtdAsync(string baseUri, string docTypeName, string publicId, string systemId, string internalSubset, IDtdParserAdapter adapter) {
            InitializeFreeFloatingDtd(baseUri, docTypeName, publicId, systemId, internalSubset, adapter);
            await ParseAsync(false).ConfigureAwait(false);
            return schemaInfo;
        }
#endregion

//
// Parsing methods
//

        private async Task ParseAsync( bool saveInternalSubset ) {
            if ( freeFloatingDtd ) {
                await ParseFreeFloatingDtdAsync().ConfigureAwait(false);
            }
            else {
                await ParseInDocumentDtdAsync( saveInternalSubset ).ConfigureAwait(false);
            }

            schemaInfo.Finish();

#if !SILVERLIGHT
            // check undeclared forward references
            if ( validate && undeclaredNotations != null ) {
                foreach ( UndeclaredNotation un in undeclaredNotations.Values ) {
                    UndeclaredNotation tmpUn = un;
                    while ( tmpUn != null ) {
                        SendValidationEvent( XmlSeverityType.Error, new XmlSchemaException( Res.Sch_UndeclaredNotation, un.name, BaseUriStr, (int)un.lineNo, (int)un.linePos ) );
                        tmpUn = tmpUn.next;
                    }
                }
            }
#endif
        }

        private async Task ParseInDocumentDtdAsync( bool saveInternalSubset ) {
            LoadParsingBuffer();

            scanningFunction = ScanningFunction.QName;
            nextScaningFunction = ScanningFunction.Doctype1;

            // doctype name
            if ( await GetTokenAsync( false ).ConfigureAwait(false) != Token.QName ) {
                OnUnexpectedError();
            }   
            schemaInfo.DocTypeName = GetNameQualified( true );

            // SYSTEM or PUBLIC id
            Token token = await GetTokenAsync( false ).ConfigureAwait(false);
            if ( token == Token.SYSTEM || token == Token.PUBLIC ) {

                var tuple_0 = await ParseExternalIdAsync( token,  Token.DOCTYPE).ConfigureAwait(false);
                publicId = tuple_0.Item1;
                systemId = tuple_0.Item2;

                token = await GetTokenAsync( false).ConfigureAwait(false);
            }

            switch ( token ) {
                case Token.LeftBracket:
                    if ( saveInternalSubset ) {
                        SaveParsingBuffer(); // this will cause saving the internal subset right from the point after '['
                        internalSubsetValueSb = new BufferBuilder();
                    }
                    await ParseInternalSubsetAsync().ConfigureAwait(false);
                    break;
                case Token.GreaterThan:
                    break;
                default:
                    OnUnexpectedError();
                    break;
            }
            SaveParsingBuffer();

            if ( systemId != null && systemId.Length > 0 ) {
                await ParseExternalSubsetAsync().ConfigureAwait(false);
            }
        }

        private async Task ParseFreeFloatingDtdAsync() {
            if ( hasFreeFloatingInternalSubset ) {
                LoadParsingBuffer();
                await ParseInternalSubsetAsync().ConfigureAwait(false);
                SaveParsingBuffer();
            }

            if ( systemId != null && systemId.Length > 0 ) {
                await ParseExternalSubsetAsync().ConfigureAwait(false);
            }
        }

        private Task ParseInternalSubsetAsync() {
            Debug.Assert( ParsingInternalSubset );
            return ParseSubsetAsync();
        }

        private async Task ParseExternalSubsetAsync() {
            Debug.Assert( externalEntitiesDepth == 0 );

            // push external subset
            if ( !await readerAdapter.PushExternalSubsetAsync( systemId, publicId ).ConfigureAwait(false) ) {
                return;
            }

            Uri baseUri = readerAdapter.BaseUri;
            if ( baseUri != null ) {
                externalDtdBaseUri = baseUri.ToString();
            }

            externalEntitiesDepth++;
            LoadParsingBuffer();

            // parse
            await ParseSubsetAsync().ConfigureAwait(false);

#if DEBUG
            Debug.Assert( readerAdapter.EntityStackLength == 0 ||
                         ( freeFloatingDtd && readerAdapter.EntityStackLength == 1 ) );
#endif
        }

        private async Task ParseSubsetAsync() {
            int startTagEntityId;
            for (;;) {
                Token token = await GetTokenAsync( false ).ConfigureAwait(false);
                startTagEntityId = currentEntityId;
                switch ( token ) {
                    case Token.AttlistDecl:
                        await ParseAttlistDeclAsync().ConfigureAwait(false);
                        break;

                    case Token.ElementDecl:
                        await ParseElementDeclAsync().ConfigureAwait(false);
                        break;

                    case Token.EntityDecl:
                        await ParseEntityDeclAsync().ConfigureAwait(false);
                        break;

                    case Token.NotationDecl:
                        await ParseNotationDeclAsync().ConfigureAwait(false);
                        break;

                    case Token.Comment:
                        await ParseCommentAsync().ConfigureAwait(false);
                        break;

                    case Token.PI:
                        await ParsePIAsync().ConfigureAwait(false);
                        break;

                    case Token.CondSectionStart:
                        if ( ParsingInternalSubset ) {
                            Throw( curPos - 3, Res.Xml_InvalidConditionalSection ); // 3==strlen("<![")
                        }
                        await ParseCondSectionAsync().ConfigureAwait(false);
                        startTagEntityId = currentEntityId;
                        break;
                    case Token.CondSectionEnd:
                        if ( condSectionDepth > 0 ) {
                            condSectionDepth--;
#if !SILVERLIGHT
                            if ( validate && currentEntityId != condSectionEntityIds[condSectionDepth] ) {
                                SendValidationEvent( curPos, XmlSeverityType.Error, Res.Sch_ParEntityRefNesting, string.Empty );
                            }
#endif
                        }
                        else {  
                            Throw( curPos - 3, Res.Xml_UnexpectedCDataEnd );
                        }
                        break;
                    case Token.RightBracket:
                        if ( ParsingInternalSubset ) {
                            if ( condSectionDepth != 0 ) {
                                Throw( curPos, Res.Xml_UnclosedConditionalSection );
                            }
                            // append the rest to internal subset value but not the closing ']'
                            if ( internalSubsetValueSb != null ) {
                                Debug.Assert( curPos > 0 && chars[curPos-1] == ']' );
                                SaveParsingBuffer( curPos - 1 );
                                schemaInfo.InternalDtdSubset = internalSubsetValueSb.ToString();
                                internalSubsetValueSb = null;
                            }
                            // check '>'
                            if ( await GetTokenAsync( false ).ConfigureAwait(false) != Token.GreaterThan ) {
                                ThrowUnexpectedToken( curPos, ">" );
                            }
#if DEBUG
                            // check entity nesting
                            Debug.Assert( readerAdapter.EntityStackLength == 0 || 
                                          ( freeFloatingDtd && readerAdapter.EntityStackLength == 1 ) );
#endif
                        }
                        else {
                            Throw( curPos, Res.Xml_ExpectDtdMarkup );
                        }
                        return;
                    case Token.Eof:
                        if ( ParsingInternalSubset && !freeFloatingDtd ) {
                            Throw( curPos, Res.Xml_IncompleteDtdContent );
                        }
                        if ( condSectionDepth != 0 ) {
                            Throw( curPos, Res.Xml_UnclosedConditionalSection );
                        }
                        return;
                    default:
                        Debug.Assert( false );
                        break;
                }

                Debug.Assert( scanningFunction == ScanningFunction.SubsetContent );

                if ( currentEntityId != startTagEntityId ) {
#if SILVERLIGHT
                    Throw(curPos, Res.Sch_ParEntityRefNesting);
#else
                    if ( validate ) {
                        SendValidationEvent( curPos, XmlSeverityType.Error, Res.Sch_ParEntityRefNesting, string.Empty );
                    }
                    else {
                        if ( !v1Compat ) {
                            Throw( curPos, Res.Sch_ParEntityRefNesting );
                        }
                    }
#endif
                }
            }
        }

        private async Task ParseAttlistDeclAsync() {
            if (await GetTokenAsync(true).ConfigureAwait(false) != Token.QName) {
                goto UnexpectedError;
            }

            // element name
            XmlQualifiedName elementName = GetNameQualified(true);
            SchemaElementDecl elementDecl;
            if (!schemaInfo.ElementDecls.TryGetValue(elementName, out elementDecl)) {
                if (!schemaInfo.UndeclaredElementDecls.TryGetValue(elementName, out elementDecl)) {
                    elementDecl = new SchemaElementDecl(elementName, elementName.Namespace);
                    schemaInfo.UndeclaredElementDecls.Add(elementName, elementDecl);
                }
            }

            SchemaAttDef attrDef = null;
            for (; ; ) {
                switch (await GetTokenAsync(false).ConfigureAwait(false)) {
                    case Token.QName:
                        XmlQualifiedName attrName = GetNameQualified(true);
                        attrDef = new SchemaAttDef(attrName, attrName.Namespace);
                        attrDef.IsDeclaredInExternal = !ParsingInternalSubset;
                        attrDef.LineNumber = (int)LineNo;
                        attrDef.LinePosition = (int)LinePos - (curPos - tokenStartPos);
                        break;
                    case Token.GreaterThan:
#if !SILVERLIGHT
                        if ( v1Compat ) {
                            // check xml:space and xml:lang
                            // 


                            if ( attrDef != null && attrDef.Prefix.Length > 0 && attrDef.Prefix.Equals( "xml" ) && attrDef.Name.Name == "space" ) {
                                attrDef.Reserved = SchemaAttDef.Reserve.XmlSpace;
                                if ( attrDef.Datatype.TokenizedType != XmlTokenizedType.ENUMERATION ) {
                                    Throw( Res.Xml_EnumerationRequired, string.Empty, attrDef.LineNumber, attrDef.LinePosition );
                                }
                                if ( validate ) {
                                    attrDef.CheckXmlSpace( readerAdapterWithValidation.ValidationEventHandling );
                                }
                            }
                        }
#endif
                        return;
                    default:
                        goto UnexpectedError;
                }

                bool attrDefAlreadyExists = (elementDecl.GetAttDef(attrDef.Name) != null);

                await ParseAttlistTypeAsync(attrDef, elementDecl, attrDefAlreadyExists).ConfigureAwait(false);
                await ParseAttlistDefaultAsync(attrDef, attrDefAlreadyExists).ConfigureAwait(false);

                // check xml:space and xml:lang
                if (attrDef.Prefix.Length > 0 && attrDef.Prefix.Equals("xml")) {
                    if ( attrDef.Name.Name == "space" ) {
#if !SILVERLIGHT
                        if ( v1Compat ) {
                            // 



                            string val = attrDef.DefaultValueExpanded.Trim();
                            if ( val.Equals( "preserve" ) || val.Equals( "default" ) ) {
                                attrDef.Reserved = SchemaAttDef.Reserve.XmlSpace;
                            }
                        }
                        else {
#endif
                            attrDef.Reserved = SchemaAttDef.Reserve.XmlSpace;
                            if ( attrDef.TokenizedType != XmlTokenizedType.ENUMERATION ) {
                                Throw( Res.Xml_EnumerationRequired, string.Empty, attrDef.LineNumber, attrDef.LinePosition );
                            }
#if !SILVERLIGHT
                            if ( validate ) {
                                attrDef.CheckXmlSpace( readerAdapterWithValidation.ValidationEventHandling );
                            }
                        }
#endif
                    }
                    else if ( attrDef.Name.Name == "lang" ) {
                        attrDef.Reserved = SchemaAttDef.Reserve.XmlLang;
                    }
                }

                // add attribute to element decl
                if (!attrDefAlreadyExists) {
                    elementDecl.AddAttDef(attrDef);
                }
            }

        UnexpectedError:
            OnUnexpectedError();
        }

        private async Task ParseAttlistTypeAsync( SchemaAttDef attrDef, SchemaElementDecl elementDecl, bool ignoreErrors ) {
            Token token = await GetTokenAsync( true ).ConfigureAwait(false);

            if ( token != Token.CDATA ) {
                elementDecl.HasNonCDataAttribute = true;
            }
            
            if ( IsAttributeValueType( token ) ) {
                attrDef.TokenizedType = (XmlTokenizedType)(int)token;
#if !SILVERLIGHT
                attrDef.SchemaType = XmlSchemaType.GetBuiltInSimpleType( attrDef.Datatype.TypeCode );
#endif

                switch ( token ) {
                    case Token.NOTATION:
                        break;
                    case Token.ID:
#if !SILVERLIGHT
                        if ( validate && elementDecl.IsIdDeclared ) {
                            SchemaAttDef idAttrDef = elementDecl.GetAttDef( attrDef.Name );
                            if ( ( idAttrDef == null || idAttrDef.Datatype.TokenizedType != XmlTokenizedType.ID ) && !ignoreErrors ) {
                                SendValidationEvent( XmlSeverityType.Error, Res.Sch_IdAttrDeclared, elementDecl.Name.ToString() );
                            }
                        }
#endif
                        elementDecl.IsIdDeclared = true;
                        return;
                    default:
                        return;
                }
#if !SILVERLIGHT
                // check notation constrains
                if ( validate ) {
                    if (elementDecl.IsNotationDeclared && !ignoreErrors ) {
                        SendValidationEvent( curPos - 8, XmlSeverityType.Error, Res.Sch_DupNotationAttribute, elementDecl.Name.ToString() ); // 8 == strlen("NOTATION")
                    }
                    else {
                        if ( elementDecl.ContentValidator != null && 
                            elementDecl.ContentValidator.ContentType == XmlSchemaContentType.Empty &&
                            !ignoreErrors ) {
                            SendValidationEvent( curPos - 8, XmlSeverityType.Error, Res.Sch_NotationAttributeOnEmptyElement, elementDecl.Name.ToString() );// 8 == strlen("NOTATION")
                        }
                        elementDecl.IsNotationDeclared = true;
                    }
                }
#endif

                if ( await GetTokenAsync( true ).ConfigureAwait(false) != Token.LeftParen ) {
                    goto UnexpectedError;
                }

                // parse notation list
                if ( await GetTokenAsync( false ).ConfigureAwait(false) != Token.Name ) {
                    goto UnexpectedError;
                }
                for (;;) {
                    string notationName = GetNameString();
#if !SILVERLIGHT
                    if ( !schemaInfo.Notations.ContainsKey(notationName) ) {
                        AddUndeclaredNotation(notationName);
                    }
                    if ( validate && !v1Compat && attrDef.Values != null && attrDef.Values.Contains( notationName ) && !ignoreErrors ) {
                        SendValidationEvent( XmlSeverityType.Error, new XmlSchemaException( Res.Xml_AttlistDuplNotationValue, notationName, BaseUriStr, (int)LineNo, (int)LinePos ) );
                    }
                    attrDef.AddValue( notationName );
#endif

                    switch ( await GetTokenAsync( false ).ConfigureAwait(false) ) {
                        case Token.Or:
                            if ( await GetTokenAsync( false ).ConfigureAwait(false) != Token.Name ) {
                                goto UnexpectedError;
                            }
                            continue;
                        case Token.RightParen:
                            return;
                        default:
                            goto UnexpectedError;
                    }
                }
            }
            else if ( token == Token.LeftParen ) {
                attrDef.TokenizedType = XmlTokenizedType.ENUMERATION;
#if !SILVERLIGHT
                attrDef.SchemaType = XmlSchemaType.GetBuiltInSimpleType( attrDef.Datatype.TypeCode );
#endif

                // parse nmtoken list
                if ( await GetTokenAsync( false ).ConfigureAwait(false) != Token.Nmtoken ) 
                    goto UnexpectedError;
#if !SILVERLIGHT
                attrDef.AddValue( GetNameString() );
#endif

                for (;;) {
                    switch ( await GetTokenAsync( false ).ConfigureAwait(false) ) {
                        case Token.Or:
                            if ( await GetTokenAsync( false ).ConfigureAwait(false) != Token.Nmtoken ) 
                                goto UnexpectedError;
                            string nmtoken = GetNmtokenString();
#if !SILVERLIGHT
                            if ( validate && !v1Compat && attrDef.Values != null && attrDef.Values.Contains( nmtoken ) && !ignoreErrors ) {
                                SendValidationEvent( XmlSeverityType.Error, new XmlSchemaException( Res.Xml_AttlistDuplEnumValue, nmtoken, BaseUriStr, (int)LineNo, (int)LinePos ) );
                            }
                            attrDef.AddValue( nmtoken );
#endif
                            break;
                        case Token.RightParen:
                            return;
                        default:
                            goto UnexpectedError;
                    }
                }
            }
            else {
                goto UnexpectedError;
            }

        UnexpectedError:
            OnUnexpectedError();
        }

        private async Task ParseAttlistDefaultAsync( SchemaAttDef attrDef, bool ignoreErrors ) {
            switch ( await GetTokenAsync( true ).ConfigureAwait(false) ) {
                case Token.REQUIRED:
                    attrDef.Presence = SchemaDeclBase.Use.Required;
                    return;
                case Token.IMPLIED:
                    attrDef.Presence = SchemaDeclBase.Use.Implied;
                    return;
                case Token.FIXED:
                    attrDef.Presence = SchemaDeclBase.Use.Fixed;
                    if ( await GetTokenAsync( true ).ConfigureAwait(false) != Token.Literal ) {
                        goto UnexpectedError;
                    }
                    break;
                case Token.Literal:
                    break;
                default:
                    goto UnexpectedError;
            }

#if !SILVERLIGHT
            if ( validate && attrDef.Datatype.TokenizedType == XmlTokenizedType.ID&& !ignoreErrors ) {
                SendValidationEvent( curPos, XmlSeverityType.Error, Res.Sch_AttListPresence, string.Empty );
            }
#endif

            if ( attrDef.TokenizedType != XmlTokenizedType.CDATA ) {
                // non-CDATA attribute type normalization - strip spaces
                attrDef.DefaultValueExpanded = GetValueWithStrippedSpaces();
            }
            else {
                attrDef.DefaultValueExpanded = GetValue();
            }
            attrDef.ValueLineNumber = (int)literalLineInfo.lineNo;
            attrDef.ValueLinePosition = (int)literalLineInfo.linePos + 1;

#if !SILVERLIGHT
            DtdValidator.SetDefaultTypedValue( attrDef, readerAdapter );
#endif
            return;

        UnexpectedError:
            OnUnexpectedError();
        }

        private async Task ParseElementDeclAsync() {
            // element name
            if ( await GetTokenAsync( true ).ConfigureAwait(false) != Token.QName ) {
                goto UnexpectedError;
            }

            // get schema decl for element
            SchemaElementDecl elementDecl = null;
            XmlQualifiedName name = GetNameQualified( true );

            if (schemaInfo.ElementDecls.TryGetValue(name, out elementDecl)) {
#if !SILVERLIGHT
                if ( validate ) {
                    SendValidationEvent( curPos - name.Name.Length, XmlSeverityType.Error, Res.Sch_DupElementDecl, GetNameString() );
                }
#endif
            }
            else {
                if ( schemaInfo.UndeclaredElementDecls.TryGetValue(name, out elementDecl ) ) {
                    schemaInfo.UndeclaredElementDecls.Remove( name );
                }
                else {
                    elementDecl = new SchemaElementDecl( name, name.Namespace );
                }
                schemaInfo.ElementDecls.Add( name, elementDecl );
            }
            elementDecl.IsDeclaredInExternal = !ParsingInternalSubset;

            // content spec
#if SILVERLIGHT
            switch ( await GetTokenAsync( true ).ConfigureAwait(false) ) {
                case Token.EMPTY:
                case Token.ANY:
                    break;
                case Token.LeftParen:
                    switch ( await GetTokenAsync( false ).ConfigureAwait(false) ) {
                        case Token.PCDATA:
                            await ParseElementMixedContentNoValidationAsync().ConfigureAwait(false);
                            break;
                        case Token.None:
                            await ParseElementOnlyContentNoValidationAsync().ConfigureAwait(false);
                            break;
                        default:
                            goto UnexpectedError;
                    }
                    break;
                default:
                    goto UnexpectedError;
            }
#else
            switch ( await GetTokenAsync( true ).ConfigureAwait(false) ) {
                case Token.EMPTY:
                    elementDecl.ContentValidator = ContentValidator.Empty;
                    break;
                case Token.ANY:
                    elementDecl.ContentValidator = ContentValidator.Any;
                    break;
                case Token.LeftParen:
                    int startParenEntityId = currentEntityId;
                    switch ( await GetTokenAsync( false ).ConfigureAwait(false) ) {
                        case Token.PCDATA:
                        {
                            ParticleContentValidator pcv = new ParticleContentValidator( XmlSchemaContentType.Mixed );
                            pcv.Start();
                            pcv.OpenGroup();

                            await ParseElementMixedContentAsync( pcv, startParenEntityId ).ConfigureAwait(false);

                            elementDecl.ContentValidator = pcv.Finish( true );
                            break;
                        }
                        case Token.None:
                        {
                            ParticleContentValidator pcv = null;
                            pcv = new ParticleContentValidator( XmlSchemaContentType.ElementOnly );
                            pcv.Start();
                            pcv.OpenGroup();

                            await ParseElementOnlyContentAsync( pcv, startParenEntityId ).ConfigureAwait(false);

                            elementDecl.ContentValidator = pcv.Finish( true );
                            break;
                        }
                        default:
                            goto UnexpectedError;
                    }
                    break;
                default:
                    goto UnexpectedError;
            }
#endif
            if ( await GetTokenAsync( false ).ConfigureAwait(false) != Token.GreaterThan ) {
                ThrowUnexpectedToken( curPos, ">" );
            }
            return;

        UnexpectedError:
            OnUnexpectedError();
        }

#if SILVERLIGHT // Element content model parsing methods without validation

        private async Task ParseElementOnlyContentNoValidationAsync() {
            Stack<ParseElementOnlyContentNoValidation_LocalFrame> localFrames = 
                new Stack<ParseElementOnlyContentNoValidation_LocalFrame>();
            ParseElementOnlyContentNoValidation_LocalFrame currentFrame =
                new ParseElementOnlyContentNoValidation_LocalFrame();
            localFrames.Push(currentFrame);

        RecursiveCall:
            
        Loop:

            switch ( await GetTokenAsync( false ).ConfigureAwait(false) ) {
                case Token.QName:
                    GetNameQualified(true);
                    await ParseHowManyNoValidationAsync().ConfigureAwait(false);
                    break;
                case Token.LeftParen:
                    // We could just do this:
                    // ParseElementOnlyContentNoValidation();
                    //
                    // But that would be a recursion - so we will simulate the call using our localFrames stack
                    //   instead.
                    currentFrame =
                        new ParseElementOnlyContentNoValidation_LocalFrame();
                    localFrames.Push(currentFrame);
                    goto RecursiveCall;
                    // And we should return here when we return from the recursion
                    //   but it's the samea s returning after the switch statement

                case Token.GreaterThan:
                    Throw( curPos, Res.Xml_InvalidContentModel );
                    goto Return;
                default:
                    goto UnexpectedError;
            }

        ReturnFromRecursiveCall:

            switch ( await GetTokenAsync( false ).ConfigureAwait(false) ) {
                case Token.Comma:
                    if ( currentFrame.parsingSchema == Token.Or ) {
                        Throw( curPos, Res.Xml_InvalidContentModel );
                    }
                    currentFrame.parsingSchema = Token.Comma;
                    break;
                case Token.Or:
                    if ( currentFrame.parsingSchema == Token.Comma ) {
                        Throw( curPos, Res.Xml_InvalidContentModel );
                    }
                    currentFrame.parsingSchema = Token.Or;
                    break;
                case Token.RightParen:
                    await ParseHowManyNoValidationAsync().ConfigureAwait(false);
                    goto Return;
                case Token.GreaterThan:
                    Throw( curPos, Res.Xml_InvalidContentModel );
                    goto Return;
                default:
                    goto UnexpectedError;
            }
            goto Loop;

        UnexpectedError:
            OnUnexpectedError();

        Return:
            // This is equivalent to return; statement
            //   we simulate it using our localFrames stack
            localFrames.Pop();
            if (localFrames.Count > 0) {
                currentFrame = localFrames.Peek();
                goto ReturnFromRecursiveCall;
            }
            else {
                return;
            }
        }

        private Task ParseHowManyNoValidationAsync() {
            return GetTokenAsync( false );
        }

        private async Task ParseElementMixedContentNoValidationAsync() {
            bool hasNames = false;

            for (;;) {
                switch ( await GetTokenAsync( false ).ConfigureAwait(false) ) {
                    case Token.RightParen:
                        if ( await GetTokenAsync( false ).ConfigureAwait(false) != Token.Star && hasNames ) {
                            ThrowUnexpectedToken( curPos, "*" );
                        }
                        return;
                    case Token.Or:
                        if ( !hasNames ) {
                            hasNames = true;
                        }
                        if ( await GetTokenAsync( false ).ConfigureAwait(false) != Token.QName ) {
                            goto default;
                        }
                        GetNameQualified( true );
                        continue;
                    default:
                        OnUnexpectedError();
                        break;
                }
            }
        }

#else // Element content model parsing methods with validation support

        private async Task ParseElementOnlyContentAsync( ParticleContentValidator pcv, int startParenEntityId ) {
            Stack<ParseElementOnlyContent_LocalFrame> localFrames = new Stack<ParseElementOnlyContent_LocalFrame>();
            ParseElementOnlyContent_LocalFrame currentFrame = new ParseElementOnlyContent_LocalFrame(startParenEntityId);
            localFrames.Push(currentFrame);
            
        RecursiveCall:
            
        Loop:
            switch ( await GetTokenAsync( false ).ConfigureAwait(false) ) {
                case Token.QName:
                    pcv.AddName( GetNameQualified(true), null );
                    await ParseHowManyAsync( pcv ).ConfigureAwait(false);
                    break;
                case Token.LeftParen:
                    pcv.OpenGroup();

                    // We could just do this:
                    // ParseElementOnlyContent( pcv, currentEntityId );
                    // 
                    // But that would be recursion - so we will simulate the call using our localFrames stack 
                    //   instead. 
                    currentFrame = 
                        new ParseElementOnlyContent_LocalFrame(currentEntityId); 
                    localFrames.Push(currentFrame); 
                    goto RecursiveCall; 
                    // And we should return here when we return from recursion call 
                    //   but it's the same as returning after the switch statement 

                case Token.GreaterThan:
                    Throw( curPos, Res.Xml_InvalidContentModel );
                    goto Return;
                default:
                    goto UnexpectedError;
            }

        ReturnFromRecursiveCall:
            switch ( await GetTokenAsync( false ).ConfigureAwait(false) ) {
                case Token.Comma:
                    if ( currentFrame.parsingSchema == Token.Or ) {
                        Throw( curPos, Res.Xml_InvalidContentModel );
                    }
                    pcv.AddSequence();
                    currentFrame.parsingSchema = Token.Comma;
                    break;
                case Token.Or:
                    if (currentFrame.parsingSchema == Token.Comma) {
                        Throw( curPos, Res.Xml_InvalidContentModel );
                    }
                    pcv.AddChoice();
                    currentFrame.parsingSchema = Token.Or;
                    break;
                case Token.RightParen:
                    pcv.CloseGroup();
                    if (validate && currentEntityId != currentFrame.startParenEntityId) {
                        SendValidationEvent( curPos, XmlSeverityType.Error, Res.Sch_ParEntityRefNesting, string.Empty );
                    }
                    await ParseHowManyAsync( pcv ).ConfigureAwait(false);
                    goto Return;
                case Token.GreaterThan:
                    Throw( curPos, Res.Xml_InvalidContentModel );
                    goto Return;
                default:
                    goto UnexpectedError;
            }
            goto Loop;

        UnexpectedError:
            OnUnexpectedError();

        Return:
            // This is equivalent to return; statement
            //   we simlate it using our localFrames stack
            localFrames.Pop(); 
            if (localFrames.Count > 0) { 
                currentFrame = (ParseElementOnlyContent_LocalFrame)localFrames.Peek(); 
                goto ReturnFromRecursiveCall; 
            } 
            else { 
                return; 
            }
        }

        private async Task ParseHowManyAsync( ParticleContentValidator pcv ) {
            switch ( await GetTokenAsync( false ).ConfigureAwait(false) ) {
                case Token.Star:
                    pcv.AddStar();
                    return;
                case Token.QMark:
                    pcv.AddQMark();
                    return;
                case Token.Plus:
                    pcv.AddPlus();
                    return;
                default:
                    return;
            }
        }

        private async Task ParseElementMixedContentAsync( ParticleContentValidator pcv, int startParenEntityId ) {
            bool hasNames = false;
            int connectorEntityId = -1;
            int contentEntityId = currentEntityId;

            for (;;) {
                switch ( await GetTokenAsync( false ).ConfigureAwait(false) ) {
                    case Token.RightParen:
                        pcv.CloseGroup();
                        if ( validate && currentEntityId != startParenEntityId ) {
                            SendValidationEvent( curPos, XmlSeverityType.Error, Res.Sch_ParEntityRefNesting, string.Empty );
                        }
                        if ( await GetTokenAsync( false ).ConfigureAwait(false) == Token.Star && hasNames ) {
                            pcv.AddStar();
                        }
                        else if ( hasNames ) {
                            ThrowUnexpectedToken( curPos, "*" );
                        }
                        return;
                    case Token.Or:
                        if ( !hasNames ) {
                            hasNames = true;
                        }
                        else {
                            pcv.AddChoice();
                        }
                        if ( validate ) { 
                            connectorEntityId = currentEntityId;
                            if ( contentEntityId < connectorEntityId ) {  // entity repl.text starting with connector
                                SendValidationEvent( curPos, XmlSeverityType.Error, Res.Sch_ParEntityRefNesting, string.Empty );
                            }
                        }

                        if ( await GetTokenAsync( false ).ConfigureAwait(false) != Token.QName ) {
                            goto default;
                        }
                        
                        XmlQualifiedName name = GetNameQualified( true );
                        if ( pcv.Exists( name ) && validate ) {
                            SendValidationEvent( XmlSeverityType.Error, Res.Sch_DupElement, name.ToString() );
                        }
                        pcv.AddName( name, null );

                        if ( validate ) {
                            contentEntityId = currentEntityId;
                            if ( contentEntityId < connectorEntityId ) { // entity repl.text ending with connector
                                SendValidationEvent( curPos, XmlSeverityType.Error, Res.Sch_ParEntityRefNesting, string.Empty );
                            }
                        }
                        continue;
                    default:
                        OnUnexpectedError();
                        break;
                }
            }
        }
#endif // Element content model parsing methods with validation support

        private async Task ParseEntityDeclAsync() {
            bool isParamEntity = false;
            SchemaEntity entity = null;

            // get entity name and type
            switch ( await GetTokenAsync( true ).ConfigureAwait(false) ) {
                case Token.Percent:
                    isParamEntity = true;
                    if ( await GetTokenAsync( true ).ConfigureAwait(false) != Token.Name ) {
                        goto UnexpectedError;
                    }
                    goto case Token.Name;
                case Token.Name:
                    // create entity object
                    XmlQualifiedName entityName = GetNameQualified( false );
                    entity = new SchemaEntity( entityName, isParamEntity );
                    
                    entity.BaseURI = BaseUriStr;
                    entity.DeclaredURI = ( externalDtdBaseUri.Length == 0 ) ? documentBaseUri : externalDtdBaseUri;

                    if ( isParamEntity ) {
                        if ( !schemaInfo.ParameterEntities.ContainsKey( entityName ) ) {
                            schemaInfo.ParameterEntities.Add( entityName, entity );
                        }
                    }
                    else {
                        if ( !schemaInfo.GeneralEntities.ContainsKey( entityName ) ) {
                            schemaInfo.GeneralEntities.Add( entityName, entity );
                        }
                    }
                    entity.DeclaredInExternal = !ParsingInternalSubset;
                    entity.ParsingInProgress = true;
                    break;
                default:
                    goto UnexpectedError;
            }

            Token token = await GetTokenAsync( true ).ConfigureAwait(false);
            switch ( token ) {
                case Token.PUBLIC:
                case Token.SYSTEM:
                    string systemId;
                    string publicId;

                    var tuple_1 = await ParseExternalIdAsync( token,  Token.EntityDecl).ConfigureAwait(false);
                    publicId = tuple_1.Item1;
                    systemId = tuple_1.Item2;

                    entity.IsExternal = true;
                    entity.Url = systemId;
                    entity.Pubid = publicId;

                    if ( await GetTokenAsync( false ).ConfigureAwait(false) == Token.NData ) {
                        if ( isParamEntity ) {
                            ThrowUnexpectedToken( curPos - 5, ">" ); // 5 == strlen("NDATA")
                        }
                        if ( !whitespaceSeen ) { 
                            Throw( curPos - 5, Res.Xml_ExpectingWhiteSpace, "NDATA" );
                        }

                        if ( await GetTokenAsync( true ).ConfigureAwait(false) != Token.Name ) {
                            goto UnexpectedError;
                        }
                        
                        entity.NData = GetNameQualified( false );
#if !SILVERLIGHT
                        string notationName = entity.NData.Name;
                        if ( !schemaInfo.Notations.ContainsKey(notationName) ) {
                            AddUndeclaredNotation(notationName);
                        }
#endif
                    }
                    break;
                case Token.Literal:
                    entity.Text = GetValue();
                    entity.Line = (int)literalLineInfo.lineNo;
                    entity.Pos = (int)literalLineInfo.linePos;
                    break;
                default:
                    goto UnexpectedError;
            }

            if ( await GetTokenAsync( false ).ConfigureAwait(false) == Token.GreaterThan ) {
                entity.ParsingInProgress = false; 
                return;
            }

        UnexpectedError:
            OnUnexpectedError();
        }

        private async Task ParseNotationDeclAsync() {
            // notation name
            if ( await GetTokenAsync( true ).ConfigureAwait(false) != Token.Name ) {
                OnUnexpectedError();
            }

            XmlQualifiedName notationName = GetNameQualified( false ); 
#if !SILVERLIGHT
            SchemaNotation notation = null;
            if ( !schemaInfo.Notations.ContainsKey( notationName.Name ) ) {
                if ( undeclaredNotations != null ) {
                    undeclaredNotations.Remove( notationName.Name );
                }
                notation = new SchemaNotation( notationName );
                schemaInfo.Notations.Add(notation.Name.Name, notation);
                
            }
            else {
                // duplicate notation
                if ( validate ) {
                    SendValidationEvent( curPos - notationName.Name.Length, XmlSeverityType.Error, Res.Sch_DupNotation, notationName.Name );
                }
            }
#endif

            // public / system id
            Token token = await GetTokenAsync( true ).ConfigureAwait(false);
            if ( token == Token.SYSTEM || token == Token.PUBLIC ) {
                string notationPublicId, notationSystemId;

                var tuple_2 = await ParseExternalIdAsync( token,  Token.NOTATION).ConfigureAwait(false);
                notationPublicId = tuple_2.Item1;
                notationSystemId = tuple_2.Item2;

#if !SILVERLIGHT
                if ( notation != null ) {
                    notation.SystemLiteral = notationSystemId;
                    notation.Pubid = notationPublicId;
                }
#endif
            }
            else {
                OnUnexpectedError();
            }

            if ( await GetTokenAsync( false ).ConfigureAwait(false) != Token.GreaterThan ) 
                OnUnexpectedError();
        }

        private async Task ParseCommentAsync() {
            SaveParsingBuffer();
#if !SILVERLIGHT
            try {
#endif
                if ( SaveInternalSubsetValue ) {
                    await readerAdapter.ParseCommentAsync( internalSubsetValueSb ).ConfigureAwait(false);
                    internalSubsetValueSb.Append( "-->" );
                }
                else {
                    await readerAdapter.ParseCommentAsync( null ).ConfigureAwait(false);
                }
#if !SILVERLIGHT
            }
            catch (XmlException e) {
                if ( e.ResString == Res.Xml_UnexpectedEOF && currentEntityId != 0 ) {
                    SendValidationEvent( XmlSeverityType.Error, Res.Sch_ParEntityRefNesting, null );
                }
                else {
                    throw;
                }
            }
#endif
            LoadParsingBuffer();
        }

        private async Task ParsePIAsync() {
            SaveParsingBuffer();
            if ( SaveInternalSubsetValue ) {
                await readerAdapter.ParsePIAsync( internalSubsetValueSb ).ConfigureAwait(false);
                internalSubsetValueSb.Append( "?>" );
            }
            else {
                await readerAdapter.ParsePIAsync( null ).ConfigureAwait(false);
            }
            LoadParsingBuffer();
        }

        private async Task ParseCondSectionAsync() {
            int csEntityId = currentEntityId;

            switch ( await GetTokenAsync( false ).ConfigureAwait(false) ) {
                case Token.INCLUDE:
                    if ( await GetTokenAsync( false ).ConfigureAwait(false) != Token.LeftBracket ) {
                        goto default;
                    }
#if !SILVERLIGHT
                    if ( validate && csEntityId != currentEntityId ) {
                        SendValidationEvent( curPos, XmlSeverityType.Error, Res.Sch_ParEntityRefNesting, string.Empty );
                    }
                    if ( validate ) {
                        if ( condSectionEntityIds == null ) {
                            condSectionEntityIds = new int[CondSectionEntityIdsInitialSize];
                        }
                        else if ( condSectionEntityIds.Length == condSectionDepth ) {
                            int[] tmp = new int[condSectionEntityIds.Length*2];
                            Array.Copy( condSectionEntityIds, 0, tmp, 0, condSectionEntityIds.Length );
                            condSectionEntityIds = tmp;
                        }
                        condSectionEntityIds[condSectionDepth] = csEntityId;
                    }
#endif
                    condSectionDepth++;
                    break;
                case Token.IGNORE:
                    if ( await GetTokenAsync( false ).ConfigureAwait(false) != Token.LeftBracket ) {
                        goto default;
                    }
#if !SILVERLIGHT
                    if ( validate && csEntityId != currentEntityId ) {
                        SendValidationEvent( curPos, XmlSeverityType.Error, Res.Sch_ParEntityRefNesting, string.Empty );
                    }
#endif
                    // the content of the ignore section is parsed & skipped by scanning function
                    if ( await GetTokenAsync( false ).ConfigureAwait(false) != Token.CondSectionEnd ) {
                        goto default;
                    }
#if !SILVERLIGHT
                    if ( validate && csEntityId != currentEntityId ) {
                        SendValidationEvent( curPos, XmlSeverityType.Error, Res.Sch_ParEntityRefNesting, string.Empty );
                    }
#endif
                    break;
                default:
                    OnUnexpectedError();
                    break;
            }
        }

        private async Task< Tuple<string, string> > ParseExternalIdAsync(Token idTokenType, Token declType) {
            Tuple<string, string> tuple;
            string publicId;
            string systemId;

            LineInfo keywordLineInfo = new LineInfo( LineNo, LinePos - 6 );
            publicId = null;
            systemId = null;

            if ( await GetTokenAsync( true ).ConfigureAwait(false) != Token.Literal ) {
                ThrowUnexpectedToken( curPos, "\"", "'" );
            }

            if ( idTokenType == Token.SYSTEM ) {
                systemId = GetValue();

                if ( systemId.IndexOf( '#' ) >= 0 ) {
                    Throw( curPos - systemId.Length - 1, Res.Xml_FragmentId, new string[] { systemId.Substring( systemId.IndexOf( '#' ) ), systemId } );
                }

                if ( declType == Token.DOCTYPE && !freeFloatingDtd ) {
                    literalLineInfo.linePos++;
                    readerAdapter.OnSystemId( systemId, keywordLineInfo, literalLineInfo );
                }
            }
            else {
                Debug.Assert( idTokenType == Token.PUBLIC );
                publicId = GetValue();

                // verify if it contains chars valid for public ids
                int i;
                if ( ( i = xmlCharType.IsPublicId( publicId ) ) >= 0 ) {
                    ThrowInvalidChar( curPos - 1 - publicId.Length + i, publicId, i );
                }

                if ( declType == Token.DOCTYPE && !freeFloatingDtd ) {
                    literalLineInfo.linePos++;
                    readerAdapter.OnPublicId( publicId, keywordLineInfo, literalLineInfo );

                    if ( await GetTokenAsync( false ).ConfigureAwait(false) == Token.Literal ) {
                        if ( !whitespaceSeen ) {
                            Throw( Res.Xml_ExpectingWhiteSpace, new string( literalQuoteChar, 1 ), (int)literalLineInfo.lineNo, (int)literalLineInfo.linePos );
                        }
                        systemId = GetValue();
                        literalLineInfo.linePos++;
                        readerAdapter.OnSystemId( systemId, keywordLineInfo, literalLineInfo );
                    }
                    else {
                        ThrowUnexpectedToken( curPos, "\"", "'" );
                    }
                }
                else {
                    if ( await GetTokenAsync( false ).ConfigureAwait(false) == Token.Literal ) {
                        if ( !whitespaceSeen ) {
                            Throw( Res.Xml_ExpectingWhiteSpace, new string( literalQuoteChar, 1 ), (int)literalLineInfo.lineNo, (int)literalLineInfo.linePos );
                        }
                        systemId = GetValue();
                    }
                    else if ( declType != Token.NOTATION ) {
                        ThrowUnexpectedToken( curPos, "\"", "'" );
                    } 
                }
            }

        tuple = new Tuple<string, string>(publicId, systemId);
        return tuple;

        }
//
// Scanning methods - works directly with parsing buffer
//
        private async Task< Token > GetTokenAsync( bool needWhiteSpace ) {
           whitespaceSeen = false;
            for (;;) {
                switch ( chars[curPos] ) {
                    case (char)0:
                        if ( curPos == charsUsed ) {
                            goto ReadData;
                        }
                        else {
                            ThrowInvalidChar( chars, charsUsed, curPos );
                        }
                        break;
                    case (char)0xA:
                        whitespaceSeen = true;
                        curPos++;
                        readerAdapter.OnNewLine( curPos );
                        continue;
                    case (char)0xD:
                        whitespaceSeen = true;
                        if ( chars[curPos+1] == (char)0xA ) {
                            if ( Normalize ) {
                                SaveParsingBuffer();          // EOL normalization of 0xD 0xA
                                readerAdapter.CurrentPosition++;
                            }
                            curPos += 2;
                        }
                        else if ( curPos+1 < charsUsed || readerAdapter.IsEof ) {
                            chars[curPos] = (char)0xA;             // EOL normalization of 0xD
                            curPos++;
                        }
                        else {
                            goto ReadData;
                        }
                        readerAdapter.OnNewLine( curPos );
                        continue;
                    case (char)0x9:
                    case (char)0x20:
                        whitespaceSeen = true;
                        curPos++;
                        continue;
                    case '%':
                        if ( charsUsed - curPos < 2 ) {
                            goto ReadData;
                        }
                        if ( !xmlCharType.IsWhiteSpace( chars[curPos+1] ) ) {
                            if ( IgnoreEntityReferences ) {
                                curPos++;
                            }
                            else {
                                await HandleEntityReferenceAsync( true, false, false ).ConfigureAwait(false);
                            }
                            continue;
                        }
                        goto default;
                    default:
                        if ( needWhiteSpace && !whitespaceSeen && scanningFunction != ScanningFunction.ParamEntitySpace ) { 
                            Throw( curPos, Res.Xml_ExpectingWhiteSpace, ParseUnexpectedToken( curPos ) );
                        }
                        tokenStartPos = curPos;
                    SwitchAgain:
                        switch ( scanningFunction ) {
                            case ScanningFunction.Name:              return await ScanNameExpectedAsync().ConfigureAwait(false);
                            case ScanningFunction.QName:             return await ScanQNameExpectedAsync().ConfigureAwait(false);
                            case ScanningFunction.Nmtoken:           return await ScanNmtokenExpectedAsync().ConfigureAwait(false);
                            case ScanningFunction.SubsetContent:     return await ScanSubsetContentAsync().ConfigureAwait(false);
                            case ScanningFunction.Doctype1:          return await ScanDoctype1Async().ConfigureAwait(false);
                            case ScanningFunction.Doctype2:          return ScanDoctype2();
                            case ScanningFunction.Element1:          return await ScanElement1Async().ConfigureAwait(false);
                            case ScanningFunction.Element2:          return await ScanElement2Async().ConfigureAwait(false);
                            case ScanningFunction.Element3:          return await ScanElement3Async().ConfigureAwait(false);
                            case ScanningFunction.Element4:          return ScanElement4();
                            case ScanningFunction.Element5:          return ScanElement5();
                            case ScanningFunction.Element6:          return ScanElement6();
                            case ScanningFunction.Element7:          return ScanElement7();
                            case ScanningFunction.Attlist1:          return await ScanAttlist1Async().ConfigureAwait(false);
                            case ScanningFunction.Attlist2:          return await ScanAttlist2Async().ConfigureAwait(false);
                            case ScanningFunction.Attlist3:          return ScanAttlist3();
                            case ScanningFunction.Attlist4:          return ScanAttlist4();
                            case ScanningFunction.Attlist5:          return ScanAttlist5();
                            case ScanningFunction.Attlist6:          return await ScanAttlist6Async().ConfigureAwait(false);
                            case ScanningFunction.Attlist7:          return ScanAttlist7();
                            case ScanningFunction.Notation1:         return await ScanNotation1Async().ConfigureAwait(false);
                            case ScanningFunction.SystemId:          return await ScanSystemIdAsync().ConfigureAwait(false);
                            case ScanningFunction.PublicId1:         return await ScanPublicId1Async().ConfigureAwait(false);
                            case ScanningFunction.PublicId2:         return await ScanPublicId2Async().ConfigureAwait(false);
                            case ScanningFunction.Entity1:           return await ScanEntity1Async().ConfigureAwait(false);
                            case ScanningFunction.Entity2:           return await ScanEntity2Async().ConfigureAwait(false);
                            case ScanningFunction.Entity3:           return await ScanEntity3Async().ConfigureAwait(false);
                            case ScanningFunction.CondSection1:      return await ScanCondSection1Async().ConfigureAwait(false);
                            case ScanningFunction.CondSection2:      return ScanCondSection2();
                            case ScanningFunction.CondSection3:      return await ScanCondSection3Async().ConfigureAwait(false);
                            case ScanningFunction.ClosingTag:        return ScanClosingTag();
                            case ScanningFunction.ParamEntitySpace:
                                whitespaceSeen = true;
                                scanningFunction = savedScanningFunction;
                                goto SwitchAgain;
                            default:
                                Debug.Assert( false );
                                return Token.None;
                        }
                }
            ReadData:
                if ( readerAdapter.IsEof || await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                    if ( HandleEntityEnd( false ) ) {
                        continue;
                    }
                    if ( scanningFunction == ScanningFunction.SubsetContent ) {
                        return Token.Eof;
                    }
                    else {
                        Throw( curPos, Res.Xml_IncompleteDtdContent );
                    }
                }
            }
        }

        private async Task< Token > ScanSubsetContentAsync() {
            for (;;) {
                switch ( chars[curPos] ) {
                    case '<':
                        switch ( chars[curPos+1] ) {
                            case '!':
                                switch ( chars[curPos+2] ) {
                                    case 'E':
                                        if ( chars[curPos+3] == 'L' ) {
                                            if ( charsUsed - curPos < 9 ) {
                                                goto ReadData;
                                            }
                                            if ( chars[curPos+4] != 'E' || chars[curPos+5] != 'M' ||
                                                 chars[curPos+6] != 'E' || chars[curPos+7] != 'N' || 
                                                 chars[curPos+8] != 'T' ) {
                                                Throw( curPos, Res.Xml_ExpectDtdMarkup );
                                            }
                                            curPos += 9;
                                            scanningFunction = ScanningFunction.QName;
                                            nextScaningFunction = ScanningFunction.Element1;
                                            return Token.ElementDecl;
                                        }
                                        else if ( chars[curPos+3] == 'N' ) {
                                            if ( charsUsed - curPos < 8 ) {
                                                goto ReadData;
                                            }
                                            if ( chars[curPos+4] != 'T' || chars[curPos+5] != 'I' || 
                                                 chars[curPos+6] != 'T' || chars[curPos+7] != 'Y' ) {
                                                Throw( curPos, Res.Xml_ExpectDtdMarkup );
                                            }
                                            curPos += 8;
                                            scanningFunction = ScanningFunction.Entity1;
                                            return Token.EntityDecl;
                                        }
                                        else {
                                            if ( charsUsed - curPos < 4 ) {
                                                goto ReadData;
                                            }
                                            Throw( curPos, Res.Xml_ExpectDtdMarkup );
                                            return Token.None;
                                        }

                                    case 'A':
                                        if ( charsUsed - curPos < 9 ) {
                                            goto ReadData;
                                        }
                                        if ( chars[curPos+3] != 'T' || chars[curPos+4] != 'T' || 
                                             chars[curPos+5] != 'L' || chars[curPos+6] != 'I' || 
                                             chars[curPos+7] != 'S' || chars[curPos+8] != 'T' ) {
                                            Throw( curPos, Res.Xml_ExpectDtdMarkup );
                                        }
                                        curPos += 9;
                                        scanningFunction = ScanningFunction.QName;
                                        nextScaningFunction = ScanningFunction.Attlist1;
                                        return Token.AttlistDecl;

                                    case 'N':
                                        if ( charsUsed - curPos < 10 ) {
                                            goto ReadData;
                                        }
                                        if ( chars[curPos+3] != 'O' || chars[curPos+4] != 'T' || 
                                             chars[curPos+5] != 'A' || chars[curPos+6] != 'T' || 
                                             chars[curPos+7] != 'I' || chars[curPos+8] != 'O' ||
                                             chars[curPos+9] != 'N' ) {
                                            Throw( curPos, Res.Xml_ExpectDtdMarkup );
                                        }
                                        curPos += 10;
                                        scanningFunction = ScanningFunction.Name;
                                        nextScaningFunction = ScanningFunction.Notation1;
                                        return Token.NotationDecl;

                                    case '[':
                                        curPos += 3;
                                        scanningFunction = ScanningFunction.CondSection1;
                                        return Token.CondSectionStart;
                                    case '-':
                                        if ( chars[curPos+3] == '-' ) {
                                            curPos += 4;
                                            return Token.Comment;
                                        }
                                        else {
                                            if ( charsUsed - curPos < 4 ) {
                                                goto ReadData;
                                            }
                                            Throw( curPos, Res.Xml_ExpectDtdMarkup );
                                            break;
                                        }
                                    default:
                                        if ( charsUsed - curPos < 3 ) {
                                            goto ReadData;
                                        }
                                        Throw( curPos + 2, Res.Xml_ExpectDtdMarkup );
                                        break;
                                }
                                break;
                            case '?':
                                curPos += 2;
                                return Token.PI;
                            default:
                                if ( charsUsed - curPos < 2 ) {
                                    goto ReadData;
                                }
                                Throw( curPos, Res.Xml_ExpectDtdMarkup );
                                return Token.None;
                        }
                        break;
                    case ']':
                        if ( charsUsed - curPos < 2 && !readerAdapter.IsEof ) {
                            goto ReadData;
                        }
                        if ( chars[curPos+1] != ']' ) {
                            curPos++;  
                            scanningFunction = ScanningFunction.ClosingTag;
                            return Token.RightBracket;
                        }
                        if ( charsUsed - curPos < 3 && !readerAdapter.IsEof ) {
                            goto ReadData;
                        }
                        if ( chars[curPos+1] == ']' && chars[curPos+2] == '>' ) {
                            curPos += 3;
                            return Token.CondSectionEnd;
                        }
                        goto default;
                    default:
                        if ( charsUsed - curPos == 0 ) {
                            goto ReadData;
                        }
                        Throw( curPos, Res.Xml_ExpectDtdMarkup );
                        break;
                }
            ReadData:
                if ( await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                    Throw( charsUsed, Res.Xml_IncompleteDtdContent );
                }
            }
        }

        private async Task< Token > ScanNameExpectedAsync() {
            await ScanNameAsync().ConfigureAwait(false);
            scanningFunction = nextScaningFunction;
            return Token.Name;
        }

        private async Task< Token > ScanQNameExpectedAsync() {
            await ScanQNameAsync().ConfigureAwait(false);
            scanningFunction = nextScaningFunction;
            return Token.QName;
        }

        private async Task< Token > ScanNmtokenExpectedAsync() {
            await ScanNmtokenAsync().ConfigureAwait(false);
            scanningFunction = nextScaningFunction;
            return Token.Nmtoken;
        }

        private async Task< Token > ScanDoctype1Async() {
            switch ( chars[curPos] ) {
                case 'P':
                    if ( !await EatPublicKeywordAsync().ConfigureAwait(false) ) {
                        Throw( curPos, Res.Xml_ExpectExternalOrClose );
                    }
                    nextScaningFunction = ScanningFunction.Doctype2;
                    scanningFunction = ScanningFunction.PublicId1;
                    return Token.PUBLIC;
                case 'S':
                    if ( !await EatSystemKeywordAsync().ConfigureAwait(false) ) {
                        Throw( curPos, Res.Xml_ExpectExternalOrClose );
                    }
                    nextScaningFunction = ScanningFunction.Doctype2;
                    scanningFunction = ScanningFunction.SystemId;
                    return Token.SYSTEM;
                case '[':
                    curPos++;
                    scanningFunction = ScanningFunction.SubsetContent;
                    return Token.LeftBracket;
                case '>':
                    curPos++;
                    scanningFunction = ScanningFunction.SubsetContent;
                    return Token.GreaterThan;
                default:
                    Throw( curPos, Res.Xml_ExpectExternalOrClose );
                    return Token.None;
            }
        }

        private async Task< Token > ScanElement1Async() {
            for (;;) {
                switch ( chars[curPos] ) {
                    case '(':
                        scanningFunction = ScanningFunction.Element2;
                        curPos++;
                        return Token.LeftParen;
                    case 'E':
                        if ( charsUsed - curPos < 5 ) {
                            goto ReadData;
                        }
                        if ( chars[curPos+1] == 'M' && chars[curPos+2] == 'P' &&
                             chars[curPos+3] == 'T' && chars[curPos+4] == 'Y' ) {
                            curPos += 5;
                            scanningFunction = ScanningFunction.ClosingTag;
                            return Token.EMPTY;
                        }
                        goto default;
                    case 'A':
                        if ( charsUsed - curPos < 3 ) {
                            goto ReadData;
                        }
                        if ( chars[curPos+1] == 'N' && chars[curPos+2] == 'Y' ) {
                            curPos += 3;
                            scanningFunction = ScanningFunction.ClosingTag;
                            return Token.ANY;
                        }
                        goto default;
                    default:
                        Throw( curPos, Res.Xml_InvalidContentModel );
                        break;
                }
            ReadData:
                if ( await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                    Throw( curPos, Res.Xml_IncompleteDtdContent );
                }
            }
        }
        
        private async Task< Token > ScanElement2Async() {
            if ( chars[curPos] == '#' ) {
                while ( charsUsed - curPos < 7 ) {
                    if ( await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                        Throw( curPos, Res.Xml_IncompleteDtdContent );
                    }
                }
                if ( chars[curPos+1] == 'P' && chars[curPos+2] == 'C' &&
                     chars[curPos+3] == 'D' && chars[curPos+4] == 'A' &&
                     chars[curPos+5] == 'T' && chars[curPos+6] == 'A' ) {
                    curPos += 7;
                    scanningFunction = ScanningFunction.Element6;
                    return Token.PCDATA;
                }
                else {
                    Throw( curPos + 1, Res.Xml_ExpectPcData );
                }
            }

            scanningFunction = ScanningFunction.Element3;
            return Token.None;
        }

        private async Task< Token > ScanElement3Async() {
            switch ( chars[curPos] ) {
                case '(':
                    curPos++;
                    return Token.LeftParen;
                case '>':
                    curPos++;
                    scanningFunction = ScanningFunction.SubsetContent;
                    return Token.GreaterThan;
                default:
                    await ScanQNameAsync().ConfigureAwait(false);
                    scanningFunction = ScanningFunction.Element4;
                    return Token.QName;
            }
        }

        private async Task< Token > ScanAttlist1Async() {
            switch ( chars[curPos] ) {
                case '>':
                    curPos++;
                    scanningFunction = ScanningFunction.SubsetContent;
                    return Token.GreaterThan;
                default:
                    if ( !whitespaceSeen ) {
                        Throw( curPos, Res.Xml_ExpectingWhiteSpace, ParseUnexpectedToken( curPos ) );
                    }
                    await ScanQNameAsync().ConfigureAwait(false);
                    scanningFunction = ScanningFunction.Attlist2;
                    return Token.QName;
            }
        }

        private async Task< Token > ScanAttlist2Async() {
            for (;;) {
                switch ( chars[curPos] ) {
                    case '(':
                        curPos++;
                        scanningFunction = ScanningFunction.Nmtoken;
                        nextScaningFunction = ScanningFunction.Attlist5;
                        return Token.LeftParen;
                    case 'C':
                        if ( charsUsed - curPos < 5 )
                            goto ReadData;
                        if ( chars[curPos+1] != 'D' || chars[curPos+2] != 'A' ||
                             chars[curPos+3] != 'T' || chars[curPos+4] != 'A' ) {
                            Throw( curPos, Res.Xml_InvalidAttributeType1 );
                        }
                        curPos += 5;
                        scanningFunction = ScanningFunction.Attlist6;
                        return Token.CDATA;
                    case 'E':
                        if ( charsUsed - curPos < 9 )
                            goto ReadData;
                        scanningFunction = ScanningFunction.Attlist6;
                        if ( chars[curPos+1] != 'N' || chars[curPos+2] != 'T' ||
                             chars[curPos+3] != 'I' || chars[curPos+4] != 'T' ) {
                            Throw( curPos, Res.Xml_InvalidAttributeType );
                        }
                        switch ( chars[curPos+5] ) {
                            case 'I':
                                if ( chars[curPos+6] != 'E' || chars[curPos+7] != 'S' ) {
                                    Throw( curPos, Res.Xml_InvalidAttributeType );
                                }
                                curPos += 8;
                                return Token.ENTITIES;
                            case 'Y':
                                curPos += 6;
                                return Token.ENTITY;
                            default:
                                Throw( curPos, Res.Xml_InvalidAttributeType );
                                break;
                        }
                        break;
                    case 'I':
                        if ( charsUsed - curPos < 6 )
                            goto ReadData;
                        scanningFunction = ScanningFunction.Attlist6;
                        if ( chars[curPos+1] != 'D' ) {
                            Throw( curPos, Res.Xml_InvalidAttributeType );
                        }

                        if ( chars[curPos+2] != 'R' ) {
                            curPos += 2;
                            return Token.ID;
                        }

                        if ( chars[curPos+3] != 'E' || chars[curPos+4] != 'F' ) {
                            Throw( curPos, Res.Xml_InvalidAttributeType );
                        }

                        if ( chars[curPos+5] != 'S' ) {
                            curPos += 5;
                            return Token.IDREF;
                        }
                        else {
                            curPos += 6;
                            return Token.IDREFS;
                        }
                    case 'N':
                        if ( charsUsed - curPos < 8 && !readerAdapter.IsEof ) {
                            goto ReadData;
                        }
                        switch ( chars[curPos+1] ) {
                            case 'O':
                                if ( chars[curPos+2] != 'T' || chars[curPos+3] != 'A' || 
                                     chars[curPos+4] != 'T' || chars[curPos+5] != 'I' || 
                                     chars[curPos+6] != 'O' || chars[curPos+7] != 'N' ) {
                                    Throw( curPos, Res.Xml_InvalidAttributeType );
                                }
                                curPos += 8;
                                scanningFunction = ScanningFunction.Attlist3;
                                return Token.NOTATION;
                            case 'M':
                                if ( chars[curPos+2] != 'T' || chars[curPos+3] != 'O' || 
                                     chars[curPos+4] != 'K' || chars[curPos+5] != 'E' || 
                                    chars[curPos+6] != 'N' ) {
                                    Throw( curPos, Res.Xml_InvalidAttributeType );
                                }
                                scanningFunction = ScanningFunction.Attlist6;

                                if ( chars[curPos+7] == 'S' ) {
                                    curPos += 8;
                                    return Token.NMTOKENS;
                                }
                                else {
                                    curPos += 7;
                                    return Token.NMTOKEN;
                                }
                            default:
                                Throw( curPos, Res.Xml_InvalidAttributeType );
                                break;
                        }
                        break;
                    default:
                        Throw( curPos, Res.Xml_InvalidAttributeType );
                        break;
                }

            ReadData:
                if ( await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                    Throw( curPos, Res.Xml_IncompleteDtdContent );
                }
            }
        }

        private async Task< Token > ScanAttlist6Async() {
            for (;;) {
                switch ( chars[curPos] ) {
                    case '"':
                    case '\'':
                        await ScanLiteralAsync( LiteralType.AttributeValue ).ConfigureAwait(false);
                        scanningFunction = ScanningFunction.Attlist1;
                        return Token.Literal;
                    case '#':
                        if ( charsUsed - curPos < 6 )
                            goto ReadData;
                        switch ( chars[curPos+1] ) {
                            case 'R':
                                if ( charsUsed - curPos < 9 )
                                    goto ReadData;
                                if ( chars[curPos+2] != 'E' || chars[curPos+3] != 'Q' ||
                                     chars[curPos+4] != 'U' || chars[curPos+5] != 'I' ||
                                     chars[curPos+6] != 'R' || chars[curPos+7] != 'E' ||
                                     chars[curPos+8] != 'D' ) {
                                    Throw( curPos, Res.Xml_ExpectAttType );
                                }
                                curPos += 9;
                                scanningFunction = ScanningFunction.Attlist1;
                                return Token.REQUIRED;
                            case 'I':
                                if ( charsUsed - curPos < 8 )
                                    goto ReadData;
                                if ( chars[curPos+2] != 'M' || chars[curPos+3] != 'P' ||
                                     chars[curPos+4] != 'L' || chars[curPos+5] != 'I' ||
                                     chars[curPos+6] != 'E' || chars[curPos+7] != 'D' ) {
                                    Throw( curPos, Res.Xml_ExpectAttType );
                                }
                                curPos += 8;
                                scanningFunction = ScanningFunction.Attlist1;
                                return Token.IMPLIED;
                            case 'F':
                                if ( chars[curPos+2] != 'I' || chars[curPos+3] != 'X' ||
                                     chars[curPos+4] != 'E' || chars[curPos+5] != 'D' ) {
                                    Throw( curPos, Res.Xml_ExpectAttType );
                                }
                                curPos += 6;
                                scanningFunction = ScanningFunction.Attlist7;
                                return Token.FIXED;
                            default:
                                Throw( curPos, Res.Xml_ExpectAttType );
                                break;
                        }
                        break;
                    default:
                        Throw( curPos, Res.Xml_ExpectAttType );
                        break;
                }
            ReadData:
                if ( await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                    Throw( curPos, Res.Xml_IncompleteDtdContent );
                }
            }
        }

        private async Task< Token > ScanLiteralAsync( LiteralType literalType ) {
            Debug.Assert( chars[curPos] == '"' || chars[curPos] == '\'' );
            
            char quoteChar = chars[curPos];
            char replaceChar = ( literalType == LiteralType.AttributeValue ) ? (char)0x20 : (char)0xA;
            int startQuoteEntityId = currentEntityId;

            literalLineInfo.Set( LineNo, LinePos );

            curPos++;
            tokenStartPos = curPos;

#if SILVERLIGHT
            stringBuilder.Clear();
#else
            stringBuilder.Length = 0;
#endif

            for (;;) {

#if SILVERLIGHT
                while ( xmlCharType.IsAttributeValueChar( chars[curPos] ) && chars[curPos] != '%' ) {
                    curPos++;
                }
#else
                unsafe {
                    while ((xmlCharType.charProperties[chars[curPos]] & XmlCharType.fAttrValue) != 0 && chars[curPos] != '%') {
                        curPos++;
                    }
                }
#endif

                if ( chars[curPos] == quoteChar && currentEntityId == startQuoteEntityId ) {
                    if ( stringBuilder.Length > 0 ) {
                        stringBuilder.Append( chars, tokenStartPos, curPos - tokenStartPos );
                    }
                    curPos++;
                    literalQuoteChar = quoteChar;
                    return Token.Literal;
                }

                int tmp1 = curPos - tokenStartPos;
                if ( tmp1 > 0 ) {
                    stringBuilder.Append( chars, tokenStartPos, tmp1 );
                    tokenStartPos = curPos;
                }

                switch ( chars[curPos] ) {
                    case '"':
                    case '\'':
                    case '>':
                        curPos++;
                        continue;
                    // eol
                    case (char)0xA:
                        curPos++;
                        if ( Normalize ) {
                            stringBuilder.Append( replaceChar );        // For attributes: CDATA normalization of 0xA
                            tokenStartPos = curPos;
                        }
                        readerAdapter.OnNewLine( curPos );
                        continue;
                    case (char)0xD:
                        if ( chars[curPos+1] == (char)0xA ) {
                            if ( Normalize ) {
                                if ( literalType == LiteralType.AttributeValue ) {
                                    stringBuilder.Append( readerAdapter.IsEntityEolNormalized ? "\u0020\u0020" : "\u0020" ); // CDATA normalization of 0xD 0xA
                                }
                                else {
                                    stringBuilder.Append( readerAdapter.IsEntityEolNormalized ? "\u000D\u000A" : "\u000A" ); // EOL normalization of 0xD 0xA                                    
                                }
                                tokenStartPos = curPos + 2;

                                SaveParsingBuffer();          // EOL normalization of 0xD 0xA in internal subset value
                                readerAdapter.CurrentPosition++;
                            }
                            curPos += 2;
                        }
                        else if ( curPos+1 == charsUsed ) {
                            goto ReadData;
                        }
                        else {
                            curPos++;
                            if ( Normalize ) {
                                stringBuilder.Append( replaceChar ); // For attributes: CDATA normalization of 0xD and 0xD 0xA
                                tokenStartPos = curPos;              // For entities:   EOL normalization of 0xD and 0xD 0xA
                            }
                        }
                        readerAdapter.OnNewLine( curPos );
                        continue;
                    // tab
                    case (char)0x9:
                        if ( literalType == LiteralType.AttributeValue && Normalize ) {
                            stringBuilder.Append( (char)0x20 );      // For attributes: CDATA normalization of 0x9
                            tokenStartPos++;
                        }
                        curPos++;
                        continue;
                    // attribute values cannot contain '<'
                    case '<':
                        if ( literalType == LiteralType.AttributeValue ) {
                            Throw( curPos, Res.Xml_BadAttributeChar, XmlException.BuildCharExceptionArgs( '<', '\0' ) );
                        }
                        curPos++;
                        continue;
                    // parameter entity reference
                    case '%':
                        if ( literalType != LiteralType.EntityReplText ) {
                            curPos++;
                            continue;
                        }
                        await HandleEntityReferenceAsync( true, true, literalType == LiteralType.AttributeValue ).ConfigureAwait(false);
                        tokenStartPos = curPos;
                        continue;
                    // general entity reference
                    case '&':
                        if ( literalType == LiteralType.SystemOrPublicID ) {
                            curPos++;
                            continue;
                        }
                        if ( curPos + 1 == charsUsed ) {
                            goto ReadData;
                        }
                        // numeric characters reference
                        if ( chars[curPos + 1] == '#' ) {
                            SaveParsingBuffer();
                            int endPos = await readerAdapter.ParseNumericCharRefAsync( SaveInternalSubsetValue ? internalSubsetValueSb : null ).ConfigureAwait(false);
                            LoadParsingBuffer();
                            stringBuilder.Append( chars, curPos, endPos - curPos );
                            readerAdapter.CurrentPosition = endPos;
                            tokenStartPos = endPos;
                            curPos = endPos;
                            continue;
                        }
                        else {
                            // general entity reference
                            SaveParsingBuffer();
                            if ( literalType == LiteralType.AttributeValue ) {
                                int endPos = await readerAdapter.ParseNamedCharRefAsync( true, SaveInternalSubsetValue ? internalSubsetValueSb : null ).ConfigureAwait(false);
                                LoadParsingBuffer();

                                if ( endPos >= 0 ) {
                                    stringBuilder.Append( chars, curPos, endPos - curPos );
                                    readerAdapter.CurrentPosition = endPos;
                                    tokenStartPos = endPos;
                                    curPos = endPos;
                                    continue;
                                }
                                else {
                                    await HandleEntityReferenceAsync( false, true, true ).ConfigureAwait(false);
                                    tokenStartPos = curPos;
                                }
                                continue;
                            }
                            else {
                                int endPos = await readerAdapter.ParseNamedCharRefAsync( false, null ).ConfigureAwait(false);
                                LoadParsingBuffer();
    
                                if ( endPos >= 0 ) {
                                    tokenStartPos = curPos;
                                    curPos = endPos;
                                    continue;
                                }
                                else {
                                    stringBuilder.Append( '&' );
                                    curPos++;
                                    tokenStartPos = curPos;
                                    // Bypass general entities in entity values
                                    XmlQualifiedName entityName = ScanEntityName();
                                    VerifyEntityReference( entityName, false, false, false );
                                }
                                continue;
                            }
                        }
                    default:
                        // end of buffer
                        if ( curPos == charsUsed ) {
                            goto ReadData;
                        }
                        // surrogate chars
                        else { 
                            char ch = chars[curPos];
                            if ( XmlCharType.IsHighSurrogate(ch) ) {
                                if ( curPos + 1 == charsUsed ) {
                                    goto ReadData;
                                }
                                curPos++;
                                if ( XmlCharType.IsLowSurrogate(chars[curPos]) ) {
                                    curPos++;
                                    continue;
                                }
                            }
                            ThrowInvalidChar( chars, charsUsed, curPos );
                            return Token.None;
                        }
                }

            ReadData:
                Debug.Assert( curPos - tokenStartPos == 0 );

                // read new characters into the buffer
                if ( readerAdapter.IsEof || await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                    if ( literalType == LiteralType.SystemOrPublicID || !HandleEntityEnd( true ) ) {
                        Throw( curPos, Res.Xml_UnclosedQuote );
                    }
                }
                tokenStartPos = curPos;
            }
        }

        private async Task< Token > ScanNotation1Async() {
            switch ( chars[curPos] ) {
                case 'P':
                    if ( !await EatPublicKeywordAsync().ConfigureAwait(false) ) {
                        Throw( curPos, Res.Xml_ExpectExternalOrClose );
                    }
                    nextScaningFunction = ScanningFunction.ClosingTag;
                    scanningFunction = ScanningFunction.PublicId1;
                    return Token.PUBLIC;
                case 'S':
                    if ( !await EatSystemKeywordAsync().ConfigureAwait(false) ) {
                        Throw( curPos, Res.Xml_ExpectExternalOrClose );
                    }
                    nextScaningFunction = ScanningFunction.ClosingTag;
                    scanningFunction = ScanningFunction.SystemId;
                    return Token.SYSTEM;
                default:
                    Throw( curPos, Res.Xml_ExpectExternalOrPublicId );
                    return Token.None;
            }
        }

        private async Task< Token > ScanSystemIdAsync() {
            if ( chars[curPos] != '"' && chars[curPos] != '\'' ) {
                ThrowUnexpectedToken( curPos, "\"", "'" );
            }

            await ScanLiteralAsync( LiteralType.SystemOrPublicID ).ConfigureAwait(false);

            scanningFunction = nextScaningFunction;
            return Token.Literal;
        }

        private async Task< Token > ScanEntity1Async() {
            if ( chars[curPos] == '%' ) {
                curPos++;
                nextScaningFunction = ScanningFunction.Entity2;
                scanningFunction = ScanningFunction.Name;
                return Token.Percent;
            }
            else {
                await ScanNameAsync().ConfigureAwait(false);
                scanningFunction = ScanningFunction.Entity2;
                return Token.Name;
            }
        }

        private async Task< Token > ScanEntity2Async() {
            switch ( chars[curPos] ) {
                case 'P':
                    if ( !await EatPublicKeywordAsync().ConfigureAwait(false) ) {
                        Throw( curPos, Res.Xml_ExpectExternalOrClose );
                    }
                    nextScaningFunction = ScanningFunction.Entity3;
                    scanningFunction = ScanningFunction.PublicId1;
                    return Token.PUBLIC;
                case 'S':
                    if ( !await EatSystemKeywordAsync().ConfigureAwait(false) ) {
                        Throw( curPos, Res.Xml_ExpectExternalOrClose );
                    }
                    nextScaningFunction = ScanningFunction.Entity3;
                    scanningFunction = ScanningFunction.SystemId;
                    return Token.SYSTEM;

                case '"':
                case '\'':
                    await ScanLiteralAsync( LiteralType.EntityReplText ).ConfigureAwait(false);
                    scanningFunction = ScanningFunction.ClosingTag;
                    return Token.Literal;
                default:
                    Throw( curPos, Res.Xml_ExpectExternalIdOrEntityValue );
                    return Token.None;
            }
        }

        private async Task< Token > ScanEntity3Async() {
            if ( chars[curPos] == 'N' ) {
                while ( charsUsed - curPos < 5 ) {
                    if ( await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                        goto End;
                    }
                }
                if ( chars[curPos+1] == 'D' && chars[curPos+2] == 'A' && 
                     chars[curPos+3] == 'T' && chars[curPos+4] == 'A' ) {
                    curPos += 5;
                    scanningFunction = ScanningFunction.Name;
                    nextScaningFunction = ScanningFunction.ClosingTag;
                    return Token.NData;
                }
            }
        End:
            scanningFunction = ScanningFunction.ClosingTag;
            return Token.None;
        }

        private async Task< Token > ScanPublicId1Async() {
            if ( chars[curPos] != '"' && chars[curPos] != '\'' ) {
                ThrowUnexpectedToken( curPos, "\"", "'" );
            }

            await ScanLiteralAsync( LiteralType.SystemOrPublicID ).ConfigureAwait(false);

            scanningFunction = ScanningFunction.PublicId2;
            return Token.Literal;
        }

        private async Task< Token > ScanPublicId2Async() {
            if ( chars[curPos] != '"' && chars[curPos] != '\'' ) {
                scanningFunction = nextScaningFunction;
                return Token.None;
            }

            await ScanLiteralAsync( LiteralType.SystemOrPublicID ).ConfigureAwait(false);
            scanningFunction = nextScaningFunction;

            return Token.Literal;
        }

        private async Task< Token > ScanCondSection1Async() {
            if ( chars[curPos] != 'I' ) {
                Throw( curPos, Res.Xml_ExpectIgnoreOrInclude );
            }
            curPos++;

            for (;;) {
                if ( charsUsed - curPos < 5 ) { 
                    goto ReadData;
                }
                switch ( chars[curPos] ) {
                    case 'N':
                        if ( charsUsed - curPos < 6 ) { 
                            goto ReadData;
                        }
                        if ( chars[curPos+1] != 'C' || chars[curPos+2] != 'L' ||
                             chars[curPos+3] != 'U' || chars[curPos+4] != 'D' || 
                             chars[curPos+5] != 'E' || xmlCharType.IsNameSingleChar( chars[curPos+6] ) 
#if XML10_FIFTH_EDITION
                             || xmlCharType.IsNCNameHighSurrogateChar( chars[curPos+6] ) 
#endif
                            ) {
                            goto default;
                        }
                        nextScaningFunction = ScanningFunction.SubsetContent;
                        scanningFunction = ScanningFunction.CondSection2;
                        curPos += 6;
                        return Token.INCLUDE;
                    case 'G':
                        if ( chars[curPos+1] != 'N' || chars[curPos+2] != 'O' ||
                             chars[curPos+3] != 'R' || chars[curPos+4] != 'E' ||
                             xmlCharType.IsNameSingleChar( chars[curPos + 5] ) 
#if XML10_FIFTH_EDITION
                            ||xmlCharType.IsNCNameHighSurrogateChar( chars[curPos+5] ) 
#endif
                            ) {
                            goto default;
                        }
                        nextScaningFunction = ScanningFunction.CondSection3;
                        scanningFunction = ScanningFunction.CondSection2;
                        curPos += 5;
                        return Token.IGNORE;
                    default:
                        Throw( curPos - 1, Res.Xml_ExpectIgnoreOrInclude );
                        return Token.None;
                }
            ReadData:
                if ( await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                    Throw( curPos, Res.Xml_IncompleteDtdContent );
                }
            }
        }

        private async Task< Token > ScanCondSection3Async() {
            int ignoreSectionDepth = 0;

            // skip ignored part
            for (;;) {

#if SILVERLIGHT
                while ( xmlCharType.IsTextChar(chars[curPos]) && chars[curPos] != ']' ) {
                    curPos++;
                }
#else
                unsafe {
                    while ((xmlCharType.charProperties[chars[curPos]] & XmlCharType.fText) != 0 && chars[curPos] != ']') {
                        curPos++;
                    }
                }
#endif

                switch ( chars[curPos] ) {
                    case '"':
                    case '\'':
                    case (char)0x9:
                    case '&':
                        curPos++;
                        continue;
                    // eol
                    case (char)0xA:
                        curPos++;
                        readerAdapter.OnNewLine( curPos );
                        continue;
                    case (char)0xD:
                        if ( chars[curPos+1] == (char)0xA ) {
                            curPos += 2;
                        }
                        else if ( curPos+1 < charsUsed || readerAdapter.IsEof ) {
                            curPos++;
                        }
                        else {
                            goto ReadData;
                        }
                        readerAdapter.OnNewLine( curPos );
                        continue;
                    case '<':
                        if ( charsUsed - curPos < 3 ) {
                            goto ReadData;
                        }
                        if ( chars[curPos+1] != '!' || chars[curPos+2] != '[' ) {
                            curPos++;
                            continue;
                        }
                        ignoreSectionDepth++;
                        curPos += 3;
                        continue;
                    case ']':
                        if ( charsUsed - curPos < 3 ) {
                            goto ReadData;
                        }
                        if ( chars[curPos+1] != ']' || chars[curPos+2] != '>' ) {
                            curPos++;
                            continue;
                        }
                        if ( ignoreSectionDepth > 0 ) {
                            ignoreSectionDepth--;
                            curPos += 3;
                            continue;
                        }
                        else {
                            curPos += 3;
                            scanningFunction = ScanningFunction.SubsetContent;
                            return Token.CondSectionEnd;
                        }
                    default:
                        // end of buffer
                        if ( curPos == charsUsed ) {
                            goto ReadData;
                        }
                        // surrogate chars
                        else { 
                            char ch = chars[curPos];
                            if ( XmlCharType.IsHighSurrogate(ch) ) {
                                if ( curPos + 1 == charsUsed ) {
                                    goto ReadData;
                                }
                                curPos++;
                                if ( XmlCharType.IsLowSurrogate(chars[curPos])) {
                                    curPos++;
                                    continue;
                                }
                            }
                            ThrowInvalidChar( chars, charsUsed, curPos );
                            return Token.None;
                        }
                }

            ReadData:
                // read new characters into the buffer
                if ( readerAdapter.IsEof || await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                    if ( HandleEntityEnd( false ) ) {
                        continue;
                    }
                    Throw( curPos, Res.Xml_UnclosedConditionalSection );
                }
                tokenStartPos = curPos;
            }
        }

        private Task ScanNameAsync() {
            return ScanQNameAsync( false );
        }

        private Task ScanQNameAsync() {
            return ScanQNameAsync( SupportNamespaces );
        }

        private async Task ScanQNameAsync( bool isQName ) {
            tokenStartPos = curPos;
            int colonOffset = -1;

            for (;;) {

                //a tmp flag, used to avoid await keyword in unsafe context.
                bool awaitReadDataInNameAsync = false;
                unsafe {
#if SILVERLIGHT
                    if ( xmlCharType.IsStartNCNameSingleChar(chars[curPos]) || chars[curPos] == ':' ) {
#else
                    if ((xmlCharType.charProperties[chars[curPos]] & XmlCharType.fNCStartNameSC) != 0 || chars[curPos] == ':') { // if ( xmlCharType.IsStartNCNameSingleChar(chars[curPos]) || chars[curPos] == ':' ) {
#endif
                        curPos++;
                    }
#if XML10_FIFTH_EDITION
                    else if ( curPos + 1 < charsUsed && xmlCharType.IsNCNameSurrogateChar(chars[curPos+1], chars[curPos])) {
                        curPos += 2;
                    }
#endif
                    else {
                        if (curPos + 1 >= charsUsed) {
                            awaitReadDataInNameAsync = true;
                        }
                        else {
                            Throw(curPos, Res.Xml_BadStartNameChar, XmlException.BuildCharExceptionArgs(chars, charsUsed, curPos));
                        }
                    }
                }

                if (awaitReadDataInNameAsync) {
                    if (await ReadDataInNameAsync().ConfigureAwait(false)) {
                        continue;
                    }
                    Throw(curPos, Res.Xml_UnexpectedEOF, "Name");
                }

        ContinueName:
                unsafe {
                    for (; ; ) {
#if SILVERLIGHT
                        if ( xmlCharType.IsNCNameSingleChar( chars[curPos] ) ) {
#else
                        if ((xmlCharType.charProperties[chars[curPos]] & XmlCharType.fNCNameSC) != 0) { // while ( xmlCharType.IsNCNameSingleChar(chars[curPos]) ) {
#endif

                            curPos++;
                        }
#if XML10_FIFTH_EDITION
                        else if ( curPos + 1 < charsUsed && xmlCharType.IsNameSurrogateChar(chars[curPos + 1], chars[curPos]) ) {
                            curPos += 2;
                        }
#endif
                        else {
                            break;
                        }
                    }
                }

                if ( chars[curPos] == ':' ) {
                    if ( isQName ) {
                        if ( colonOffset != -1 ) {
                            Throw( curPos, Res.Xml_BadNameChar, XmlException.BuildCharExceptionArgs( ':', '\0' ));
                        }
                        colonOffset = curPos - tokenStartPos;
                        curPos++;
                        continue;
                    }
                    else {
                        curPos++;
                        goto ContinueName;
                    }
                }
                // end of buffer
                else if ( curPos == charsUsed 
#if XML10_FIFTH_EDITION
                    || ( curPos + 1 == charsUsed && xmlCharType.IsNCNameHighSurrogateChar( chars[curPos] ) ) 
#endif
                    ) {
                    if ( await ReadDataInNameAsync().ConfigureAwait(false) ) {
                        goto ContinueName;
                    }
                    if ( tokenStartPos == curPos ) {
                        Throw( curPos, Res.Xml_UnexpectedEOF, "Name" );
                    }
                }
                // end of name
                colonPos = ( colonOffset == -1 ) ? -1 : tokenStartPos + colonOffset;
                return;
            }
        }

        private async Task< bool > ReadDataInNameAsync() {
            int offset = curPos - tokenStartPos;
            curPos = tokenStartPos;
            bool newDataRead = ( await ReadDataAsync().ConfigureAwait(false) != 0 );
            tokenStartPos = curPos;
            curPos += offset;
            return newDataRead;
        }

        private async Task ScanNmtokenAsync() {
            tokenStartPos = curPos;

            for (;;) {

                unsafe {
                    for (; ; ) {
#if SILVERLIGHT
                        if ( xmlCharType.IsNCNameSingleChar(chars[curPos]) || chars[curPos] == ':' ) {
#else
                        if ((xmlCharType.charProperties[chars[curPos]] & XmlCharType.fNCNameSC) != 0 || chars[curPos] == ':') {  // if ( xmlCharType.IsNCNameChar(chars[curPos]) || chars[curPos] == ':' ) {
#endif
                            curPos++;
                        }
#if XML10_FIFTH_EDITION
                        else if (curPos + 1 < charsUsed && xmlCharType.IsNCNameSurrogateChar(chars[curPos + 1], chars[curPos])) {
                            curPos += 2;
                        }
#endif
                        else {
                            break;
                        }
                    }
                }

                if ( curPos < charsUsed 
#if XML10_FIFTH_EDITION
                    && ( !xmlCharType.IsNCNameHighSurrogateChar( chars[curPos] ) || curPos + 1 < charsUsed ) 
#endif
                    ) {
                    if ( curPos - tokenStartPos == 0 ) {
                        Throw( curPos, Res.Xml_BadNameChar, XmlException.BuildCharExceptionArgs( chars, charsUsed, curPos ) );
                    }
                    return;
                }

                int len = curPos - tokenStartPos;
                curPos = tokenStartPos;
                if ( await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                    if ( len > 0 ) {
                        tokenStartPos = curPos;
                        curPos += len;
                        return;
                    }
                    Throw( curPos, Res.Xml_UnexpectedEOF, "NmToken" );
                }
                tokenStartPos = curPos;
                curPos += len;
            }
        }

        private async Task< bool > EatPublicKeywordAsync() { 
            Debug.Assert( chars[curPos] == 'P' );
            while ( charsUsed - curPos < 6 ) {
                if ( await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                    return false;
                }
            }
            if ( chars[curPos+1] != 'U' || chars[curPos+2] != 'B' ||
                 chars[curPos+3] != 'L' || chars[curPos+4] != 'I' ||
                 chars[curPos+5] != 'C' ) {
                return false;
            }
            curPos += 6;
            return true;
        }

        private async Task< bool > EatSystemKeywordAsync() { 
            Debug.Assert( chars[curPos] == 'S' );
            while ( charsUsed - curPos < 6 ) {
                if ( await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                    return false;
                }
            }
            if ( chars[curPos+1] != 'Y' || chars[curPos+2] != 'S' ||
                 chars[curPos+3] != 'T' || chars[curPos+4] != 'E' ||
                 chars[curPos+5] != 'M' ) {
                return false;
            }
            curPos += 6;
            return true;
        }

//
// Parsing buffer maintainance methods
//
        async Task< int > ReadDataAsync() {
            SaveParsingBuffer();
            int charsRead = await readerAdapter.ReadDataAsync().ConfigureAwait(false);
            LoadParsingBuffer();
            return charsRead;
        }

//
// Entity handling
//
        private Task< bool > HandleEntityReferenceAsync( bool paramEntity, bool inLiteral, bool inAttribute ) {
            Debug.Assert( chars[curPos] == '&' || chars[curPos] == '%' );
            curPos++;

            return HandleEntityReferenceAsync( ScanEntityName(), paramEntity, inLiteral, inAttribute );

        }

        private async Task< bool > HandleEntityReferenceAsync( XmlQualifiedName entityName, bool paramEntity, bool inLiteral, bool inAttribute ) {
            Debug.Assert( chars[curPos-1] == ';' );

            SaveParsingBuffer();
            if ( paramEntity && ParsingInternalSubset && !ParsingTopLevelMarkup ) {
                Throw( curPos - entityName.Name.Length - 1, Res.Xml_InvalidParEntityRef );
            }

            SchemaEntity entity = VerifyEntityReference( entityName, paramEntity, true, inAttribute );
            if ( entity == null ) {
                return false;
            }
            if ( entity.ParsingInProgress ) {
                Throw( curPos - entityName.Name.Length - 1, paramEntity ? Res.Xml_RecursiveParEntity : Res.Xml_RecursiveGenEntity, entityName.Name );
            }

            int newEntityId;
            if ( entity.IsExternal ) {

                var tuple_3 = await readerAdapter.PushEntityAsync( entity).ConfigureAwait(false);
                newEntityId = tuple_3.Item1;

                if ( !tuple_3.Item2 ) {

                    return false;
                }
                externalEntitiesDepth++;
            }
            else {
                if ( entity.Text.Length == 0 ) {
                    return false;
                }

                var tuple_4 = await readerAdapter.PushEntityAsync( entity).ConfigureAwait(false);
                newEntityId = tuple_4.Item1;

                if ( !tuple_4.Item2 ) {

                    return false;
                }
            }
            currentEntityId = newEntityId;

            if ( paramEntity && !inLiteral && scanningFunction != ScanningFunction.ParamEntitySpace ) {
                savedScanningFunction = scanningFunction;
                scanningFunction = ScanningFunction.ParamEntitySpace;
            }

            LoadParsingBuffer();
            return true;
        }

    }
}

