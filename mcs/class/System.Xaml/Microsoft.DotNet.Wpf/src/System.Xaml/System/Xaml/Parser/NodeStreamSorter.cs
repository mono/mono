// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Xaml.MS.Impl;
using System.Diagnostics;
using System.Xaml;
using MS.Internal.Xaml.Context;
using MS.Internal.Xaml.Parser;

namespace MS.Internal.Xaml
{
    internal class NodeStreamSorter: IEnumerator<XamlNode>
    {
        XamlParserContext _context;
        XamlXmlReaderSettings _settings;
        IEnumerator<XamlNode> _source;
        Queue<XamlNode> _buffer;
        XamlNode _current;

        ReorderInfo[] _sortingInfoArray;
        XamlNode[] _originalNodesInOrder;

        Dictionary<string, string> _xmlnsDictionary;

        class SeenCtorDirectiveFlags
        {
            public bool SeenInstancingProperty;
            public bool SeenOutOfOrderCtorDirective;
        }

        List<SeenCtorDirectiveFlags> _seenStack = new List<SeenCtorDirectiveFlags>();
        int _startObjectDepth = 0;

        List<int> _moveList;

        private void InitializeObjectFrameStack()
        {
            if (_seenStack.Count == 0)
            {
                _seenStack.Add(new SeenCtorDirectiveFlags());
            }
            _seenStack[0].SeenInstancingProperty = false;
            _seenStack[0].SeenOutOfOrderCtorDirective = false;
        }

        private void StartObjectFrame()
        {
            _startObjectDepth += 1;
            if(_seenStack.Count <=_startObjectDepth)
            {
                _seenStack.Add(new SeenCtorDirectiveFlags());
            }
            _seenStack[_startObjectDepth].SeenInstancingProperty = false;
            _seenStack[_startObjectDepth].SeenOutOfOrderCtorDirective = false;
        }

        private void EndObjectFrame()
        {
            _startObjectDepth -= 1;
        }

        bool HaveSeenInstancingProperty
        {
            get { return _seenStack[_startObjectDepth].SeenInstancingProperty; }
            set { _seenStack[_startObjectDepth].SeenInstancingProperty = value; }
        }

        bool HaveSeenOutOfOrderCtorDirective
        {
            get { return _seenStack[_startObjectDepth].SeenOutOfOrderCtorDirective; }
            set { _seenStack[_startObjectDepth].SeenOutOfOrderCtorDirective = value; }
        }

        struct ReorderInfo
        {
            public int Depth { get; set; }
            public int OriginalOrderIndex { get; set; }
            public XamlNodeType XamlNodeType { get; set; }

#if DEBUG
            public override string ToString()
            {
                return String.Format(TypeConverterHelper.InvariantEnglishUS, "Depth[{0}] {2}", this.Depth, this.XamlNodeType);
            }
#endif
        }

        public NodeStreamSorter(XamlParserContext context, XamlPullParser parser, XamlXmlReaderSettings settings, Dictionary<string, string> xmlnsDictionary)
        {
            _context = context;
            _settings = settings;
            _source = parser.Parse().GetEnumerator();
            _xmlnsDictionary = xmlnsDictionary;

            _buffer = new Queue<XamlNode>();
            _sortingInfoArray = null;

            StartNewNodeStreamWithSettingsPreamble();
            ReadAheadAndSortCtorProperties();
        }

        #region IEnumerator<XamlNode> Members

        public XamlNode Current
        {
            get { return _current; }
        }

        object System.Collections.IEnumerator.Current
        {
            get { return _current; }
        }

        public bool MoveNext()
        {
            do
            {
                if (_buffer.Count > 0)
                {
                    _current = _buffer.Dequeue();
                }
                else
                {
                    if (!_source.MoveNext())
                    {
                        return false;
                    }
                    _current = _source.Current;
                    if (_current.NodeType == XamlNodeType.StartObject)
                    {
                        // Out of order Ctor Directives could result in the Type of the Start Object
                        // being unresolved (UNKNOWN).  So provide the SO in the buffer for possible fixup
                        // and then read the possibly fixed Start Object back after the Sort.
                        _buffer.Enqueue(_current);
                        ReadAheadAndSortCtorProperties();
                        _current = _buffer.Dequeue();
                    }
                }
                // Skip over "End Of Attributes" nodes.
            } while (_current.IsEndOfAttributes);
            return true;
        }

        // required for the IEnumerable interface
        // but not used in the parser's usage.
        public void Reset()
        {
            throw new NotImplementedException();
        }

        // required for the IEnumerable interface
        // but not used in the parser's usage.
        // FxCop requires the call the SuppressFinalize().
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Preamble Methods

        // this does the initial load of the node stream buffer.
        // It also looks at the XamlReaderSettings and inserts any XmlNs
        // definitions and XML state required by the provided XamlReaderSettings.
        private void StartNewNodeStreamWithSettingsPreamble()
        {
            XamlNode node;
            bool foundFirstStartObject = false;

            while (!foundFirstStartObject)
            {
                _source.MoveNext();
                node = _source.Current;
                switch (node.NodeType)
                {
                case XamlNodeType.NamespaceDeclaration:
                    _buffer.Enqueue(node);
                    break;
                case XamlNodeType.StartObject:
                    foundFirstStartObject = true;
                    EnqueueInitialExtraXmlNses();
                    _buffer.Enqueue(node);
                    EnqueueInitialXmlState();
                    break;
                case XamlNodeType.None:
                    if (node.IsLineInfo)
                    {
                        _buffer.Enqueue(node);
                    }
                    break;
                default:
                    break;
                }
            }
        }

        private void EnqueueInitialExtraXmlNses()
        {
            if (_xmlnsDictionary != null)
            {
                foreach (string prefix in _xmlnsDictionary.Keys)
                {
                    // Skip any prefixes in the settings that were already defined
                    // in the XML text (on the root node)
                    if (_context.FindNamespaceByPrefixInParseStack(prefix) == null)
                    {
                        string uriString = _xmlnsDictionary[prefix];
                        XamlNode node = new XamlNode(XamlNodeType.NamespaceDeclaration, new NamespaceDeclaration(uriString, prefix));
                        _buffer.Enqueue(node);
                    }
                }
            }
        }

        // Enqueue nodes from the "settings".
        // I.e  move state from the setting into the node stream.
        private void EnqueueInitialXmlState()
        {
            string xmlNs = _context.FindNamespaceByPrefix(KnownStrings.XmlPrefix);
            XamlSchemaContext schemaContext = _context.SchemaContext;
            if (_settings.XmlSpacePreserve == true)
            {
                EnqueueOneXmlDirectiveProperty(XamlLanguage.Space, KnownStrings.Preserve);
            }
            if (!String.IsNullOrEmpty(_settings.XmlLang))
            {
                EnqueueOneXmlDirectiveProperty(XamlLanguage.Lang, _settings.XmlLang);
            }
            if (_settings.BaseUri != null)
            {
                EnqueueOneXmlDirectiveProperty(XamlLanguage.Base, _settings.BaseUri.ToString());
            }
        }

        private void EnqueueOneXmlDirectiveProperty(XamlMember xmlDirectiveProperty, string textValue)
        {
            var startProperty = new XamlNode(XamlNodeType.StartMember, xmlDirectiveProperty);
            // No lineinfo for nodes that don't really appear in the text.
            _buffer.Enqueue(startProperty);

            var textNode = new XamlNode(XamlNodeType.Value, textValue);
            // No lineinfo for nodes that don't really appear in the text.
            _buffer.Enqueue(textNode);

            // No lineinfo for nodes that don't really appear in the text.
            _buffer.Enqueue(new XamlNode(XamlNodeType.EndMember));
        }

        #endregion

        // Read until we have all the attributes and the first directive property elements.
        // We do this because some directive properties control Object creation and they
        // can occur anywhere before the first property element.
        // After we scoop all that up, reorder the Ctor directives to the front
        // so the Builder(s) (aka Object Writer) have them right away. And don't
        // have to look for them.
        private void ReadAheadAndSortCtorProperties()
        {
            InitializeObjectFrameStack();
            _moveList = null;

            ReadAheadToEndObjectOrFirstPropertyElement();

            // If we saw TypeArguments, Arguments, or FactoryMethod properties.
            // then dig in and correct the stream.
            //
            //if (HaveSeenOutOfOrderCtorDirective)
            if(_moveList != null)
            {
                SortContentsOfReadAheadBuffer();
            }
            return;
        }

        private void ReadAheadToEndObjectOrFirstPropertyElement()
        {
            ReadAheadToEndOfAttributes();
            ReadAheadToFirstInstancingProperty();
        }

        // This reads from immediately after a start object and queues all the
        // nodes to the end of the attributes.
        // returns true if read End of Attributes.
        // returns false if read End Object.
        private void ReadAheadToEndOfAttributes()
        {
            XamlNode node;
            int propertyDepth = 0;
            bool done = false;

            do
            {
                if (!_source.MoveNext())
                {
                    throw new InvalidOperationException("premature end of stream before EoA");
                }
                node = _source.Current;
                switch (node.NodeType)
                {
                case XamlNodeType.StartObject:
                    StartObjectFrame();
                    break;

                case XamlNodeType.EndObject:
                    EndObjectFrame();
                    if (propertyDepth == 0)
                    {
                        done = true;
                    }
                    break;

                case XamlNodeType.None:
                    if (node.IsEndOfAttributes)
                    {
                        if (propertyDepth == 0)
                        {
                            done = true;
                        }
                    }
                    break;

                case XamlNodeType.StartMember:
                    {
                        propertyDepth += 1;
                        if (!HaveSeenOutOfOrderCtorDirective)
                        {
                            CheckForOutOfOrderCtorDirectives(node);
                        }
                    }
                    break;

                case XamlNodeType.EndMember:
                    propertyDepth -= 1;
                    break;
                }
                _buffer.Enqueue(node);

            } while (!done);
        }

        // After the End of Attributes is found continue to buffer nodes
        // until the first real property.  Careful, there may be real objects
        // with real properties inside of Directives.
        private void ReadAheadToFirstInstancingProperty()
        {
            int propertyDepth = 0;

            bool done = false;
            do
            {
                if (!_source.MoveNext())
                {
                    throw new InvalidOperationException("premature end of stream after EoA");
                }
                XamlNode node = _source.Current;
                switch (node.NodeType)
                {
                case XamlNodeType.StartMember:
                    {
                        propertyDepth++;
                        bool isInstancingProperty = CheckForOutOfOrderCtorDirectives(node);
                        if (isInstancingProperty && propertyDepth == 1)
                        {
                            done = true;
                        }
                    }
                    break;

                case XamlNodeType.EndMember:
                    propertyDepth--;
                    break;

                case XamlNodeType.EndObject:
                    if (propertyDepth == 0)
                    {
                        // end of current object, no real properties but we are done.
                        // Exit loop normaly so we Enqueue the EndObject.
                        done = true;
                    }
                    break;
                }
                _buffer.Enqueue(node);
            } while (!done);
        }

        // This updates the state of instancing vs. construction controling
        // members.  It also returns if the current member is "instancing or not".
        private bool CheckForOutOfOrderCtorDirectives(XamlNode node)
        {
            XamlMember prop = node.Member;
            bool isInstancingProperty = false;
            if (IsCtorDirective(prop))
            {
                if (HaveSeenInstancingProperty)
                {
                    HaveSeenOutOfOrderCtorDirective = true;
                    if (_moveList == null)
                    {
                        _moveList = new List<int>();
                    }
                    _moveList.Add(_buffer.Count);  // mark the current position as needing fixup
                }
            }
            // Anything else except x:Key is an instancing member.
            else if (!(prop.IsDirective && prop == XamlLanguage.Key))
            {
                HaveSeenInstancingProperty = true;
                isInstancingProperty = true;
            }
            return isInstancingProperty;
        }

        private bool IsCtorDirective(XamlMember member)
        {
            if (!member.IsDirective)
            {
                return false;
            }
            if ((member == XamlLanguage.Initialization)
                  || (member == XamlLanguage.PositionalParameters)
                  || (member == XamlLanguage.FactoryMethod)
                  || (member == XamlLanguage.Arguments)
                  || (member == XamlLanguage.TypeArguments)
                  || (member == XamlLanguage.Base))
            {
                return true;
            }
            return false;
        }

        private bool IsInstancingMember(XamlMember member)
        {
            if (IsCtorDirective(member))
            {
                return false;
            }
            if (member.IsDirective && member == XamlLanguage.Key)
            {
                return false;
            }
            // Actually...
            // XamlLanguage.Uid, if the type has no UidProperty is not "instancing".
            // But it might be slower to track the current type and lookup the UidProperty
            // than to assume that Uid "might" be instancing.
            return true;
        }

        private void SortContentsOfReadAheadBuffer()
        {
            BuildSortingBuffer();

            // Do the reordering
            MoveList_Process();

            // Load the result back in to the Buffer.
            ReloadSortedBuffer();
        }

        private void BuildSortingBuffer()
        {
            _originalNodesInOrder = _buffer.ToArray();
            _buffer.Clear();

            // Build an array with the info we need
            _sortingInfoArray = new ReorderInfo[_originalNodesInOrder.Length];
            int depth = 0;
            ReorderInfo rInfo = new ReorderInfo();

            for (int i = 0; i < _originalNodesInOrder.Length; i++)
            {
                rInfo.Depth = depth;
                rInfo.OriginalOrderIndex = i;
                rInfo.XamlNodeType = _originalNodesInOrder[i].NodeType;

                switch (rInfo.XamlNodeType)
                {
                    case XamlNodeType.NamespaceDeclaration:
                    case XamlNodeType.EndMember:
                    case XamlNodeType.Value:
                        break;

                    case XamlNodeType.GetObject:
                    case XamlNodeType.StartObject:
                        rInfo.Depth = ++depth;
                        break;

                    case XamlNodeType.EndObject:
                        rInfo.Depth = depth--;
                        break;

                    case XamlNodeType.StartMember:
                        break;
                }
                _sortingInfoArray[i] = rInfo;
            }
        }

        private void ReloadSortedBuffer()
        {
            for (int idx = 0; idx < _sortingInfoArray.Length; idx++)
            {
                int xamlIndex = _sortingInfoArray[idx].OriginalOrderIndex;
                _buffer.Enqueue(_originalNodesInOrder[xamlIndex]);
            }
            _sortingInfoArray = null;
        }

        private void MoveList_Process()
        {
            int depth;
            int ctorDirectiveIdx;
            while (MoveList_RemoveStartMemberIndexWithGreatestDepth(out ctorDirectiveIdx, out depth))
            {
                int startObjectIdx;
                if (BackupTo(ctorDirectiveIdx, XamlNodeType.StartObject, depth, out startObjectIdx))
                {
                    int firstMemberIdx;
                    if (AdvanceTo(startObjectIdx, XamlNodeType.StartMember, depth, out firstMemberIdx))
                    {
                        SortMembers(firstMemberIdx);
                    }
                }
            }
        }

        private bool MoveList_RemoveStartMemberIndexWithGreatestDepth(out int deepestCtorIdx, out int deepestDepth)
        {
            deepestDepth = -1;
            deepestCtorIdx = -1;

            int deepestIdx = -1;
            if (_moveList.Count == 0)
            {
                return false;
            }
            for (int i = 0; i < _moveList.Count; i++)
            {
                int ctorIdx = _moveList[i];
                if (_sortingInfoArray[ctorIdx].Depth > deepestDepth)
                {
                    deepestDepth = _sortingInfoArray[ctorIdx].Depth;
                    deepestCtorIdx = ctorIdx;
                    deepestIdx = i;
                }
            }
            Debug.Assert(deepestIdx != -1);
            _moveList.RemoveAt(deepestIdx);
            return true;
        }

        private void SortMembers(int start)
        {
            int depth = _sortingInfoArray[start].Depth;
            Debug.Assert(_sortingInfoArray[start].XamlNodeType == XamlNodeType.StartMember);

            int idx = start;
            while (idx < _sortingInfoArray.Length
                   && _sortingInfoArray[idx].XamlNodeType == XamlNodeType.StartMember)
            {
                int propIdx;
                int ctorIdx;
                if (!AdvanceToNextInstancingMember(idx, depth, out propIdx))
                {
                    break;
                }

                if (!AdvanceToNextCtorDirective(propIdx, depth, out ctorIdx))
                {
                    break;
                }

                int ctorDirectivesLength = AdvanceOverNoninstancingDirectives(ctorIdx, depth);
                SwapRanges(propIdx, ctorIdx, ctorIdx + ctorDirectivesLength);
                idx = ctorIdx + ctorDirectivesLength;
            }
        }

        private bool AdvanceToNextInstancingMember(int current, int depth, out int end)
        {
            Debug.Assert(_sortingInfoArray[current].XamlNodeType == XamlNodeType.StartMember);

            end = current;
            int originalIdx = _sortingInfoArray[current].OriginalOrderIndex;
            XamlMember nextMember = _originalNodesInOrder[originalIdx].Member;
            while(!IsInstancingMember(nextMember))
            {
                if(!AdvanceTo(current, XamlNodeType.StartMember, depth, out end))
                {
                    return false;
                }
                current = end;
                originalIdx = _sortingInfoArray[current].OriginalOrderIndex;
                nextMember = _originalNodesInOrder[originalIdx].Member;
            }
            return true;
        }

        private bool AdvanceToNextCtorDirective(int current, int depth, out int end)
        {
            Debug.Assert(_sortingInfoArray[current].XamlNodeType == XamlNodeType.StartMember);

            end = current;
            int originalIdx = _sortingInfoArray[current].OriginalOrderIndex;
            XamlMember member = _originalNodesInOrder[originalIdx].Member;
            while (!IsCtorDirective(member))
            {
                if (!AdvanceTo(current, XamlNodeType.StartMember, depth, out end))
                {
                    return false;
                }
                current = end;
                originalIdx = _sortingInfoArray[current].OriginalOrderIndex;
                member = _originalNodesInOrder[originalIdx].Member;
            }
            return true;
        }

        private int AdvanceOverNoninstancingDirectives(int start, int depth)
        {
            int current = start;
            int end = current;
            int originalIdx = _sortingInfoArray[current].OriginalOrderIndex;
            XamlMember nextMember = _originalNodesInOrder[originalIdx].Member;
            while (!IsInstancingMember(nextMember))
            {
                if (!AdvanceTo(current, XamlNodeType.StartMember, depth, out end))
                {
                    if (AdvanceTo(current, XamlNodeType.EndObject, depth, out end))
                    {
                        return end - start;
                    }
                    else
                    {
                        Debug.Assert(false, "Missing End Object in node sorter");
                    }
                }
                current = end;
                originalIdx = _sortingInfoArray[current].OriginalOrderIndex;
                nextMember = _originalNodesInOrder[originalIdx].Member;
            }
            return end - start;
        }

        private void SwapRanges(int beginning, int middle, int end)
            {
            int length1 = middle - beginning;
            int length2 = end - middle;
            Debug.Assert(length1 > 0 && length2 > 0);

            ReorderInfo[] temp = new ReorderInfo[length1];
            
            // Copy first half into temp storage.
            //             srcArray,      srcIdx, destArray, destIdx, length
            Array.Copy(_sortingInfoArray, beginning, temp,       0,      length1);

            // Copy second half up where the first half was.
            //             srcArray,      srcIdx,    destArray,     destIdx,  length
            Array.Copy(_sortingInfoArray, middle, _sortingInfoArray, beginning,  length2); 

            // Copy first half out of temp storage in after the first half
            //        srcArray, srcIdx, destArray,        destIdx,         length
            Array.Copy(temp,      0,  _sortingInfoArray, beginning + length2, length1); 
        }

        private bool AdvanceTo(int start, XamlNodeType nodeType, int searchDepth, out int end)
        {
            for (int idx = start + 1; idx < _sortingInfoArray.Length; idx++)
            {
                XamlNodeType currentNodeType = _sortingInfoArray[idx].XamlNodeType;
                int nodeDepth = _sortingInfoArray[idx].Depth;
                if(nodeDepth == searchDepth)
                {
                    if (currentNodeType == nodeType)
                    {
                        end = idx;
                        return true;
                    }
                }
                else if (nodeDepth < searchDepth)
                {
                    end = idx;
                    return false;  // we have searched past the end of the current Object.
                }
            }
            end =_sortingInfoArray.Length;
            return false;
        }

        private bool BackupTo(int start, XamlNodeType nodeType, int searchDepth, out int end)
        {
            for (int idx = start - 1; idx >= 0; idx--)
            {
                XamlNodeType currentNodeType = _sortingInfoArray[idx].XamlNodeType;
                int nodeDepth = _sortingInfoArray[idx].Depth;
                if (nodeDepth == searchDepth)
                {
                    if (currentNodeType == nodeType)
                    {
                        end = idx;
                        return true;
                    }
                    else if (nodeDepth < searchDepth)
                    {
                        end = idx;
                        return false;  // we have searched past the start of the current Object.
                    }
                }
            }
            end = 0;
            return false;
        }
    }
}
