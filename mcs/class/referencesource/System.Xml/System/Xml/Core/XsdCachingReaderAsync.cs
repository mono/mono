
using System.IO;
using System.Text;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Diagnostics;
using System.Globalization;
using System.Collections;
using System.Security.Policy;

using System.Threading.Tasks;

namespace System.Xml {

    internal partial class XsdCachingReader : XmlReader, IXmlLineInfo {

        // Gets the text value of the current node.
        public override Task<string> GetValueAsync() {
            if (returnOriginalStringValues) {
                return Task.FromResult(cachedNode.OriginalStringValue);
            }
            else {
                return Task.FromResult(cachedNode.RawValue);
            }
        }
    
        // Reads the next node from the stream/TextReader.
        public override async Task< bool > ReadAsync() {
            switch (cacheState) {
                case CachingReaderState.Init:
                    cacheState = CachingReaderState.Record;
                    goto case CachingReaderState.Record;

                case CachingReaderState.Record: 
                    ValidatingReaderNodeData recordedNode = null;
                    if (await coreReader.ReadAsync().ConfigureAwait(false)) {
                        switch(coreReader.NodeType) {
                            case XmlNodeType.Element:
                                //Dont record element within the content of a union type since the main reader will break on this and the underlying coreReader will be positioned on this node
                                cacheState = CachingReaderState.ReaderClosed;
                                return false;

                            case XmlNodeType.EndElement:
                                recordedNode = AddContent(coreReader.NodeType);
                                recordedNode.SetItemData(coreReader.LocalName, coreReader.Prefix, coreReader.NamespaceURI, coreReader.Depth);  //Only created for element node type
                                recordedNode.SetLineInfo(lineInfo);
                                break;

                            case XmlNodeType.Comment:
                            case XmlNodeType.ProcessingInstruction:
                            case XmlNodeType.Text:
                            case XmlNodeType.CDATA:
                            case XmlNodeType.Whitespace:
                            case XmlNodeType.SignificantWhitespace:
                                recordedNode = AddContent(coreReader.NodeType);
                                recordedNode.SetItemData(await coreReader.GetValueAsync().ConfigureAwait(false));
                                recordedNode.SetLineInfo(lineInfo);
                                recordedNode.Depth = coreReader.Depth;
                                break;

                            default:
                                break;       
                        }
                        cachedNode = recordedNode;
                        return true;    
                    }
                    else {
                        cacheState = CachingReaderState.ReaderClosed;
                        return false;
                    }    

                case CachingReaderState.Replay:
                    if (currentContentIndex >= contentIndex) { //When positioned on the last cached node, switch back as the underlying coreReader is still positioned on this node
                        cacheState = CachingReaderState.ReaderClosed;
                        cacheHandler(this);
                        if (coreReader.NodeType != XmlNodeType.Element || readAhead) { //Only when coreReader not positioned on Element node, read ahead, otherwise it is on the next element node already, since this was not cached
                            return await coreReader.ReadAsync().ConfigureAwait(false);
                        }
                        return true;                        
                    }
                    cachedNode = contentEvents[currentContentIndex];
                    if (currentContentIndex > 0) {
                        ClearAttributesInfo();
                    }
                    currentContentIndex++;
                    return true;

                default:
                    return false;
            }
        }

        // Skips to the end tag of the current element.
        public override async Task SkipAsync() {
            //Skip on caching reader should move to the end of the subtree, past all cached events
            switch (cachedNode.NodeType) {
                case XmlNodeType.Element:
                    if (coreReader.NodeType != XmlNodeType.EndElement && !readAhead) { //will be true for IsDefault cases where we peek only one node ahead
                        int startDepth = coreReader.Depth - 1;
                        while (await coreReader.ReadAsync().ConfigureAwait(false) && coreReader.Depth > startDepth) 
                        ;
                    }
                    await coreReader.ReadAsync().ConfigureAwait(false);
                    cacheState = CachingReaderState.ReaderClosed;
                    cacheHandler(this);
                    break;
    
                case XmlNodeType.Attribute:
                    MoveToElement();
                    goto case XmlNodeType.Element;

                default:
                    Debug.Assert(cacheState == CachingReaderState.Replay);
                    await ReadAsync().ConfigureAwait(false);
                    break;
            }
        }

//Private methods
        internal Task SetToReplayModeAsync() {
            cacheState = CachingReaderState.Replay;
            currentContentIndex = 0;
            currentAttrIndex = -1;
            return ReadAsync(); //Position on first node recorded to begin replaying
        }

    }
}
