// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace Microsoft.Activities.Presentation.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    // NOTE: (x, y) denotes line x column y, where x and y are 0 based, 
    // while the line/column in SourceLocation and LineNumberPair is 1 based.
    internal class SourceTextScanner
    {
        private const char NewLine = '\n';

        private const char Return = '\r';

        private string source;

        // <start index, length>
        // say a line is "abc\n"
        // the length is 4, with '\n' included.
        private List<Tuple<int, int>> indexCache;

        internal SourceTextScanner(string source)
        {
            SharedFx.Assert(source != null, "source != null");
            this.source = source;
            this.indexCache = new List<Tuple<int, int>>();
        }

        internal Tuple<LineColumnPair, char> SearchCharAfter(LineColumnPair startPoint, params char[] charsToSearch)
        {
            SharedFx.Assert(startPoint != null, "startPoint != null");

            int line = startPoint.LineNumber - 1;
            int column = startPoint.ColumnNumber - 1;

            HashSet<char> charsToSearchSet = new HashSet<char>(charsToSearch);
            int index = this.GetIndex(line, column);
            if (index < 0)
            {
                return null;
            }

            bool firstLoop = true;
            foreach (Tuple<char, int> currentPair in this.Scan(index))
            {
                if (firstLoop)
                {
                    firstLoop = false;
                }
                else
                {
                    if (charsToSearchSet.Contains(currentPair.Item1))
                    {
                        LineColumnPair location = this.GetLocation(currentPair.Item2);
                        SharedFx.Assert(location != null, "invalid location");
                        return Tuple.Create(location, currentPair.Item1);
                    }
                }
            }

            return null;
        }

        private LineColumnPair GetLocation(int index)
        {
            SharedFx.Assert(index >= 0 && index < this.source.Length, "index out of range");

            while (!this.IsIndexInScannedLine(index))
            {
                this.TryScanNextLine();
            }
            
            int line = this.indexCache.Count - 1;
            for (; line >= 0; --line)
            {
                if (index >= this.indexCache[line].Item1)
                {
                    break;
                }
            }
            
            SharedFx.Assert(line >= 0, "line < this.indexCache.Count");
            int column = index - this.indexCache[line].Item1;
            SharedFx.Assert(column < this.indexCache[line].Item2, "Should Not Happen");

            return new LineColumnPair(line + 1, column + 1);
        }
        
        private int GetIndex(int line, int column)
        {
            while (this.indexCache.Count <= line)
            {
                if (!this.TryScanNextLine())
                {
                    break;
                }
            }

            if (this.indexCache.Count <= line)
            {
                SharedFx.Assert(string.Format(CultureInfo.CurrentCulture, "line out of range:({0},{1})", line + 1, column + 1));
                return -1;
            }

            if (column >= this.indexCache[line].Item2)
            {
                SharedFx.Assert(string.Format(CultureInfo.CurrentCulture, "column out of range:({0},{1})", line + 1, column + 1));
                return -1;
            }

            return this.indexCache[line].Item1 + column;
        }

        private bool IsIndexInScannedLine(int index)
        {
            SharedFx.Assert(index >= 0 && index < this.source.Length, "invalid index");

            int last = this.indexCache.Count - 1;
            return last >= 0 && index < this.indexCache[last].Item1 + this.indexCache[last].Item2;
        }

        // return created
        private bool TryScanNextLine()
        {
            int startIndex = 0;
            if (this.indexCache.Count > 0)
            {
                int tail = this.indexCache.Count - 1;
                startIndex = this.indexCache[tail].Item1 + this.indexCache[tail].Item2;
            }

            if (startIndex >= this.source.Length)
            {
                return false;
            }

            int lastIndex = -1;
            foreach (Tuple<char, int> currentPair in this.Scan(startIndex))
            {
                lastIndex = currentPair.Item2;
                if (currentPair.Item1 == NewLine)
                {
                    break;
                }
            }

            if (lastIndex < 0)
            {
                SharedFx.Assert("lastIndex < 0");
                return false;
            }

            int lineLength = lastIndex - startIndex + 1;
            this.indexCache.Add(Tuple.Create(startIndex, lineLength));
            return true;
        }

        // Tuple<current charactor, charactor index>
        // this Scan will replace \r\n=>\n \r=>\n
        // \r\n return: <\n, \n's index>
        // \r   return: <\n, \r's index>
        private IEnumerable<Tuple<char, int>> Scan(int index)
        {
            if (index < 0 || index >= this.source.Length)
            {
                SharedFx.Assert("index < 0 || index >= this.source.Length");
                yield break;
            }

            while (index < this.source.Length)
            {
                char currentChar = this.source[index];

                if (currentChar == Return)
                {
                    if (index + 1 < this.source.Length && this.source[index + 1] == NewLine)
                    {
                        ++index;
                    }

                    currentChar = NewLine;
                }

                yield return Tuple.Create(currentChar, index);
                ++index;
            }
        }
    }
}
