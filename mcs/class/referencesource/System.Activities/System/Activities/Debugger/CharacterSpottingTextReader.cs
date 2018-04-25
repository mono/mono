// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Debugger
{
    using System.Collections.Generic;
    using System.IO;

    // 
    internal partial class CharacterSpottingTextReader : TextReader
    {
        // These 'special characters' couple with the fact that we are working on XML.
        private const char StartAngleBracket = '<';
        private const char EndAngleBracket = '>';
        private const char SingleQuote = '\'';
        private const char DoubleQuote = '"';
        private const char EndLine = '\n';
        private const char CarriageReturn = '\r';

        private TextReader underlyingReader;
        private int currentLine;
        private int currentPosition;
        private List<DocumentLocation> startAngleBrackets;
        private List<DocumentLocation> endAngleBrackets;
        private List<DocumentLocation> singleQuotes;
        private List<DocumentLocation> doubleQuotes;
        private List<DocumentLocation> endLines;

        public CharacterSpottingTextReader(TextReader underlyingReader)
        {
            UnitTestUtility.Assert(underlyingReader != null, "underlyingReader should not be null and should be ensured by caller.");
            this.underlyingReader = underlyingReader;
            this.currentLine = 1;
            this.currentPosition = 1;
            this.startAngleBrackets = new List<DocumentLocation>();
            this.endAngleBrackets = new List<DocumentLocation>();
            this.singleQuotes = new List<DocumentLocation>();
            this.doubleQuotes = new List<DocumentLocation>();
            this.endLines = new List<DocumentLocation>();
        }

        // CurrentLocation consists of the current line number and the current position on the line.
        // 
        // The current position is like a cursor moving along the line. For example, a string "abc" ending with "\r\n":
        // 
        //    abc\r\n
        // 
        // the current position, depicted as | below, moves from char to char:
        // 
        //    |a|b|c|\r|\n
        // 
        // When we are at the beginning of the line, the current position is 1. After we read the first char, 
        // we advance the current position to 2, and so on:
        // 
        //    1 2 3 4 
        //    |a|b|c|\r|\n
        // 
        // As we reach the end-of-line character on the line, which can be \r, \r\n or \n, we move to the next line and reset the current position to 1.
        private DocumentLocation CurrentLocation
        {
            get
            {
                return new DocumentLocation(this.currentLine, this.currentPosition);
            }
        }

        public override void Close()
        {
            this.underlyingReader.Close();
        }

        public override int Peek()
        {
            // This character is not consider read, therefore we don't need to analyze this.
            return this.underlyingReader.Peek();
        }

        public override int Read()
        {
            int result = this.underlyingReader.Read();
            if (result != -1)
            {
                result = this.AnalyzeReadData((char)result);
            }

            return result;
        }

        internal DocumentLocation FindCharacterStrictlyAfter(char c, DocumentLocation afterLocation)
        {
            List<DocumentLocation> locationList = this.GetLocationList(c);
            UnitTestUtility.Assert(locationList != null, "We should always find character for special characters only");

            // Note that this 'nextLocation' may not represent a real document location (we could hit an end line character here so that there is no next line
            // position. This is merely used for the search algorithm below:
            DocumentLocation nextLocation = new DocumentLocation(afterLocation.LineNumber, new OneBasedCounter(afterLocation.LinePosition.Value + 1));
            BinarySearchResult result = locationList.MyBinarySearch(nextLocation);
            if (result.IsFound)
            {
                // It is possible that the next location is a quote itself, or
                return nextLocation;
            }
            else if (result.IsNextIndexAvailable)
            {
                // Some other later position is the quote, or
                return locationList[result.NextIndex];
            }
            else
            {
                // in the worst case no quote can be found.
                return null;
            }
        }

        internal DocumentLocation FindCharacterStrictlyBefore(char c, DocumentLocation documentLocation)
        {
            List<DocumentLocation> locationList = this.GetLocationList(c);
            UnitTestUtility.Assert(locationList != null, "We should always find character for special characters only");

            BinarySearchResult result = locationList.MyBinarySearch(documentLocation);
            if (result.IsFound)
            {
                if (result.FoundIndex > 0)
                {
                    return locationList[result.FoundIndex - 1];
                }
                else
                {
                    return null;
                }
            }
            else if (result.IsNextIndexAvailable)
            {
                if (result.NextIndex > 0)
                {
                    return locationList[result.NextIndex - 1];
                }
                else
                {
                    return null;
                }
            }
            else if (locationList.Count > 0)
            {
                return locationList[locationList.Count - 1];
            }
            else
            {
                return null;
            }
        }

        private List<DocumentLocation> GetLocationList(char c)
        {
            switch (c)
            {
                case StartAngleBracket:
                    return this.startAngleBrackets;
                case EndAngleBracket:
                    return this.endAngleBrackets;
                case SingleQuote:
                    return this.singleQuotes;
                case DoubleQuote:
                    return this.doubleQuotes;
                case EndLine:
                    return this.endLines;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Process last character read, and canonicalize end line.
        /// </summary>
        /// <param name="lastCharacterRead">The last character read by the underlying reader</param>
        /// <returns>The last character processed</returns>
        private char AnalyzeReadData(char lastCharacterRead)
        {
            // XML specification requires end-of-line == '\n' or '\r' or "\r\n"
            // See http://www.w3.org/TR/2008/REC-xml-20081126/#sec-line-ends for details.
            if (lastCharacterRead == CarriageReturn)
            {
                // if reading \r and peek next char is \n, then process \n as well
                int nextChar = this.underlyingReader.Peek();
                if (nextChar == EndLine)
                {
                    lastCharacterRead = (char)this.underlyingReader.Read();
                }
            }

            if (lastCharacterRead == EndLine || lastCharacterRead == CarriageReturn)
            {
                this.endLines.Add(this.CurrentLocation);
                this.currentLine++;
                this.currentPosition = 1;

                // according to XML spec, both \r\n and \r should be translated to \n
                return EndLine;
            }
            else
            {
                List<DocumentLocation> locations = this.GetLocationList(lastCharacterRead);
                if (locations != null)
                {
                    locations.Add(this.CurrentLocation);
                }

                this.currentPosition++;
                return lastCharacterRead;
            }
        }
    }
}
