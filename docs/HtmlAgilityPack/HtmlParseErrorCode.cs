// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
namespace HtmlAgilityPack
{
    /// <summary>
    /// Represents the type of parsing error.
    /// </summary>
    public enum HtmlParseErrorCode
    {
        /// <summary>
        /// A tag was not closed.
        /// </summary>
        TagNotClosed,

        /// <summary>
        /// A tag was not opened.
        /// </summary>
        TagNotOpened,

        /// <summary>
        /// There is a charset mismatch between stream and declared (META) encoding.
        /// </summary>
        CharsetMismatch,

        /// <summary>
        /// An end tag was not required.
        /// </summary>
        EndTagNotRequired,

        /// <summary>
        /// An end tag is invalid at this position.
        /// </summary>
        EndTagInvalidHere
    }
}