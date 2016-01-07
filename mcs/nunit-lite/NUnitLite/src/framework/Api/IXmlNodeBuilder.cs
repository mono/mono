using System;

namespace NUnit.Framework.Api
{
    /// <summary>
    /// An object implementing IXmlNodeBuilder is able to build 
    /// an XmlResult representation of itself and any children.
    /// Note that the interface refers to the implementation
    /// of XmlNode in the NUnit.Framework.Api namespace.
    /// </summary>
    public interface IXmlNodeBuilder
    {
        /// <summary>
        /// Returns an XmlNode representating the current object.
        /// </summary>
        /// <param name="recursive">If true, children are included where applicable</param>
        /// <returns>An XmlNode representing the result</returns>
        XmlNode ToXml(bool recursive);

        /// <summary>
        /// Returns an XmlNode representing the current object after 
        /// adding it as a child of the supplied parent node.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="recursive">If true, children are included, where applicable</param>
        /// <returns></returns>
        XmlNode AddToXml(XmlNode parentNode, bool recursive);
    }
}
