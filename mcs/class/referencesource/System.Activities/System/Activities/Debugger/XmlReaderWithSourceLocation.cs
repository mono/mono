// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Debugger
{
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;

    internal class XmlReaderWithSourceLocation : XmlWrappingReader
    {
        private Dictionary<DocumentLocation, DocumentRange> attributeValueRanges;
        private Dictionary<DocumentLocation, DocumentRange> emptyElementRanges;
        private Dictionary<DocumentLocation, DocumentRange> contentValueRanges;
        private Dictionary<DocumentLocation, DocumentLocation> startElementLocations;
        private Dictionary<DocumentLocation, DocumentLocation> endElementLocations;
        private CharacterSpottingTextReader characterSpottingTextReader;
        private Stack<DocumentLocation> contentStartLocationStack;

        public XmlReaderWithSourceLocation(TextReader underlyingTextReader)
        {
            UnitTestUtility.Assert(underlyingTextReader != null, "CharacterSpottingTextReader cannot be null and should be ensured by caller.");
            CharacterSpottingTextReader characterSpottingTextReader = new CharacterSpottingTextReader(underlyingTextReader);
            this.BaseReader = XmlReader.Create(characterSpottingTextReader);
            UnitTestUtility.Assert(this.BaseReaderAsLineInfo != null, "The XmlReader created by XmlReader.Create should ensure this.");
            UnitTestUtility.Assert(this.BaseReaderAsLineInfo.HasLineInfo(), "The XmlReader created by XmlReader.Create should ensure this.");
            this.characterSpottingTextReader = characterSpottingTextReader;
            this.contentStartLocationStack = new Stack<DocumentLocation>();
        }

        public Dictionary<DocumentLocation, DocumentRange> AttributeValueRanges
        {
            get
            {
                if (this.attributeValueRanges == null)
                {
                    this.attributeValueRanges = new Dictionary<DocumentLocation, DocumentRange>();
                }

                return this.attributeValueRanges;
            }
        }

        public Dictionary<DocumentLocation, DocumentRange> ContentValueRanges
        {
            get
            {
                if (this.contentValueRanges == null)
                {
                    this.contentValueRanges = new Dictionary<DocumentLocation, DocumentRange>();
                }

                return this.contentValueRanges;
            }
        }

        public Dictionary<DocumentLocation, DocumentRange> EmptyElementRanges
        {
            get
            {
                if (this.emptyElementRanges == null)
                {
                    this.emptyElementRanges = new Dictionary<DocumentLocation, DocumentRange>();
                }

                return this.emptyElementRanges;
            }
        }

        public Dictionary<DocumentLocation, DocumentLocation> StartElementLocations
        {
            get
            {
                if (this.startElementLocations == null)
                {
                    this.startElementLocations = new Dictionary<DocumentLocation, DocumentLocation>();
                }

                return this.startElementLocations;
            }
        }

        public Dictionary<DocumentLocation, DocumentLocation> EndElementLocations
        {
            get
            {
                if (this.endElementLocations == null)
                {
                    this.endElementLocations = new Dictionary<DocumentLocation, DocumentLocation>();
                }

                return this.endElementLocations;
            }
        }

        private DocumentLocation CurrentLocation
        {
            get
            {
                return new DocumentLocation(this.BaseReaderAsLineInfo.LineNumber, this.BaseReaderAsLineInfo.LinePosition);
            }
        }

        public override bool Read()
        {
            bool result = base.Read();
            if (this.NodeType == Xml.XmlNodeType.Element)
            {
                DocumentLocation elementLocation = this.CurrentLocation;
                if (this.IsEmptyElement)
                {
                    DocumentRange emptyElementRange = this.FindEmptyElementRange(elementLocation);
                    this.EmptyElementRanges.Add(elementLocation, emptyElementRange);
                }
                else
                {
                    DocumentLocation startElementBracket = this.FindStartElementBracket(elementLocation);
                    this.StartElementLocations.Add(elementLocation, startElementBracket);

                    // Push a null as a place holder. In XmlNodeType.Text part, we replace this
                    // null with real data. Why not pushing real data only without this place holder?
                    // Because in XmlNodeType.EndElement, we need to know whether there is Text. Think 
                    // about situation like <a>Text1<b><c>Text2</c></b>Text3</a>
                    // So, each time an Element starts, we push a place holder in the stack so that Start
                    // and End don't mis-match.
                    this.contentStartLocationStack.Push(null);
                }

                int attributeCount = this.AttributeCount;
                if (attributeCount > 0)
                {
                    for (int i = 0; i < attributeCount; i++)
                    {
                        this.MoveToAttribute(i);
                        DocumentLocation memberLocation = this.CurrentLocation;
                        DocumentRange attributeValueRange = this.FindAttributeValueLocation(memberLocation);
                        this.AttributeValueRanges.Add(memberLocation, attributeValueRange);
                    }

                    this.MoveToElement();
                }
            }
            else if (this.NodeType == Xml.XmlNodeType.EndElement)
            {
                DocumentLocation endElementLocation = this.CurrentLocation;
                DocumentLocation endElementBracket = this.FindEndElementBracket(endElementLocation);
                this.EndElementLocations.Add(endElementLocation, endElementBracket);
                UnitTestUtility.Assert(
                    this.contentStartLocationStack.Count > 0, 
                    "The stack should contain at least a null we pushed in StartElement.");
                DocumentLocation contentStartLocation = this.contentStartLocationStack.Pop();
                if (contentStartLocation != null)
                {
                    DocumentLocation contentEnd = this.FindContentEndBefore(endElementLocation);
                    this.ContentValueRanges.Add(endElementLocation, new DocumentRange(contentStartLocation, contentEnd));
                }
            }
            else if (this.NodeType == Xml.XmlNodeType.Text)
            {
                UnitTestUtility.Assert(this.contentStartLocationStack.Count > 0, "Adding Text with out StartElement?");
                if (this.contentStartLocationStack.Peek() == null)
                { 
                    // no text was added since the last StartElement.
                    // This is the start of the content of this Element.
                    // <a>ABCDE</a>
                    // Sometimes, xml reader gives the text by ABC and DE in 
                    // two times.
                    this.contentStartLocationStack.Pop();
                    this.contentStartLocationStack.Push(this.CurrentLocation);    
                }
            }

            return result;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (this.characterSpottingTextReader != null)
                {
                    ((IDisposable)this.characterSpottingTextReader).Dispose();
                }

                this.characterSpottingTextReader = null;
            }
        }

        private DocumentLocation FindStartElementBracket(DocumentLocation elementLocation)
        {
            return this.characterSpottingTextReader.FindCharacterStrictlyBefore('<', elementLocation);
        }

        private DocumentLocation FindEndElementBracket(DocumentLocation elementLocation)
        {
            return this.characterSpottingTextReader.FindCharacterStrictlyAfter('>', elementLocation);
        }

        private DocumentRange FindEmptyElementRange(DocumentLocation elementLocation)
        {
            DocumentLocation startBracket = this.FindStartElementBracket(elementLocation);
            DocumentLocation endBracket = this.FindEndElementBracket(elementLocation);
            UnitTestUtility.Assert(startBracket != null, "XmlReader should guarantee there must be a start angle bracket.");
            UnitTestUtility.Assert(endBracket != null, "XmlReader should guarantee there must be an end angle bracket.");
            DocumentRange emptyElementRange = new DocumentRange(startBracket, endBracket);
            return emptyElementRange;
        }

        private DocumentRange FindAttributeValueLocation(DocumentLocation memberLocation)
        {
            UnitTestUtility.Assert(this.characterSpottingTextReader != null, "Ensured by constructor.");
            DocumentLocation attributeStart = this.characterSpottingTextReader.FindCharacterStrictlyAfter(this.QuoteChar, memberLocation);
            UnitTestUtility.Assert(attributeStart != null, "Read should ensure the two quote characters exists");
            DocumentLocation attributeEnd = this.characterSpottingTextReader.FindCharacterStrictlyAfter(this.QuoteChar, attributeStart);
            UnitTestUtility.Assert(attributeEnd != null, "Read should ensure the two quote characters exists");
            return new DocumentRange(attributeStart, attributeEnd);
        }

        private DocumentLocation FindContentEndBefore(DocumentLocation location)
        {
            DocumentLocation contentEnd = this.FindStartElementBracket(location);
            int linePosition = contentEnd.LinePosition.Value - 1;

            // Line position is 1-based
            if (linePosition < 1)
            {
                return this.characterSpottingTextReader.FindCharacterStrictlyBefore('\n', contentEnd);
            }
            else
            {
                return new DocumentLocation(contentEnd.LineNumber.Value, linePosition);
            }
        }
    }
}
