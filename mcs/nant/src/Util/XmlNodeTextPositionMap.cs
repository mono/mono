// NAnt - A .NET build tool
// Copyright (C) 2001 Gerry Shaw
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// Gerry Shaw (gerry_shaw@yahoo.com)

namespace SourceForge.NAnt {

    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.XPath;
    using System.Collections;

    public struct TextPosition {
        public static readonly TextPosition InvalidPosition = new TextPosition(-1,-1);

        public TextPosition(int line, int column) {
            Line = line;
            Column = column;
        }

        public int Line;
        public int Column;
    }

    /// <summary>
    /// Maps XML nodes to the text positions from their original source.
    /// </summary>
    public class XPathTextPositionMap {

        Hashtable _map = new Hashtable();

        public XPathTextPositionMap(string url) {
            string parentXPath = "/"; // default to root
            string previousXPath = "";
            int previousDepth = 0;

            // Load text reader
            XmlTextReader reader = new XmlTextReader(url);
            ArrayList indexAtDepth = new ArrayList();

            // Explicitly load document XPath
            _map.Add((object) "/", (object) new TextPosition(1, 1));

            // loop thru all nodes in the document
            while (reader.Read()) {
                // reader to Node ...
                if (   (reader.NodeType.ToString() != "Whitespace")  // Ignore those we aren't interested in
                    && (reader.NodeType.ToString() != "EndElement")
                    && (reader.NodeType.ToString() != "ProcessingInstruction")
                    && (reader.NodeType.ToString() != "XmlDeclaration")
                    ) {
                    int level = reader.Depth;
                    string currentXPath = "";

                    // If we arr higher than before
                    if (reader.Depth < previousDepth) {
                        // Clear vars for new depth
                        string[] list = parentXPath.Split('/');
                        string newXPath = ""; // once appended to / will be root node ...

                        for (int j = 1; j < level+1; j++) {
                            newXPath += "/" + list[j];
                        }

                        // higher than before so trim xpath\
                        parentXPath = newXPath; // one up from before

                        // clear indexes for depth greater than ours
                        indexAtDepth.RemoveRange(level+1, indexAtDepth.Count - (level+1));

                    } else if (reader.Depth > previousDepth) {
                        // we are lower
                        parentXPath = previousXPath;
                    }

                    // End depth setup
                    // Setup up index array
                    // add any needed extra items ( usually only 1 )
                    // would have uses array but not sure what maximum depth will be beforehand
                    for (int index = indexAtDepth.Count; index < level+1; index++) {
                        indexAtDepth.Add(0);
                    }
                    // Set child index
                    if ((int) indexAtDepth[level] == 0) {
                        // first time thru
                        indexAtDepth[level] = 1;
                    } else {
                        indexAtDepth[level] = (int) indexAtDepth[level] + 1; // lower so append to xpath
                    }

                    // Do actual XPath generation
                    if (parentXPath.EndsWith("/")) {
                        currentXPath = parentXPath;
                    } else {
                        currentXPath = parentXPath + "/"; // add seperator
                    }

                    // Set the final XPath
                    currentXPath += "child::node()[" + indexAtDepth[level] + "]";

                    // Add to our hash structures
                    _map.Add((object) currentXPath, (object) new TextPosition(reader.LineNumber, reader.LinePosition));

                    // setup up loop vars for next iteration
                    previousXPath = currentXPath;
                    previousDepth = reader.Depth;
                }
            }
        }

        public TextPosition GetTextPosition(XmlNode node) {
            string xpath = GetXPathFromNode(node);
            return GetTextPosition(xpath);
        }

        public TextPosition GetTextPosition(string xpath) {
            TextPosition pos;
            if (_map.ContainsKey(xpath)) {
                pos = (TextPosition) _map[xpath];
            } else {
                pos = TextPosition.InvalidPosition;
            }
            return pos;
        }

        private string GetXPathFromNode(XmlNode node) {
            XPathNavigator nav = node.CreateNavigator();

            string xpath = "";
            int index = 0;

            while (nav.NodeType.ToString() != "Root") {
                // loop thru children until we find ourselves
                XPathNavigator navParent = nav.Clone();
                navParent.MoveToParent();
                int parentIndex = 0;
                navParent.MoveToFirstChild();
                if (navParent.IsSamePosition(nav)) {
                    index = parentIndex;
                }
                while (navParent.MoveToNext()) {
                    parentIndex++;
                    if (navParent.IsSamePosition(nav)) {
                        index = parentIndex;
                    }
                }

                nav.MoveToParent(); // do loop condiditon here

                // if we are at doc and index = 0 then there is no xml proc instruction
                if ((nav.NodeType.ToString()) != "Root" || (index == 0)) {
                    index = index + 1; // special case at root to avoid processing instruction ..
                }

                string thisNode = "child::node()[" + index  + "]";

                if (xpath == "") {
                    xpath = thisNode;
                } else {
                    // build xpath string
                    xpath = thisNode + "/" + xpath;
                }
            }

            // prepend slash to ...
            xpath = "/" + xpath;

            return xpath;
        }
    }
}
