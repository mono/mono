// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System;

namespace HtmlAgilityPack
{
    /// <summary>
    /// Flags that describe the behavior of an Element node.
    /// </summary>
    [Flags]
    public enum HtmlElementFlag
    {
        /// <summary>
        /// The node is a CDATA node.
        /// </summary>
        CData = 1,

        /// <summary>
        /// The node is empty. META or IMG are example of such nodes.
        /// </summary>
        Empty = 2,

        /// <summary>
        /// The node will automatically be closed during parsing.
        /// </summary>
        Closed = 4,

        /// <summary>
        /// The node can overlap.
        /// </summary>
        CanOverlap = 8
    }
}