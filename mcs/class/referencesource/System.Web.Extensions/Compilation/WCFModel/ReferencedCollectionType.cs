#region Copyright (c) Microsoft Corporation
/// <copyright company='Microsoft Corporation'>
///    Copyright (c) Microsoft Corporation. All Rights Reserved.
///    Information Contained Herein is Proprietary and Confidential.
/// </copyright>
#endregion

using System;
using XmlSerialization = System.Xml.Serialization;

#if WEB_EXTENSIONS_CODE
namespace System.Web.Compilation.WCFModel
#else
namespace Microsoft.VSDesigner.WCFModel
#endif
{
    /// <summary>
    /// a collection type sharing record
    /// </summary>
    /// <remarks></remarks>
#if WEB_EXTENSIONS_CODE
    internal class ReferencedCollectionType
#else
    [CLSCompliant(true)]
    public class ReferencedCollectionType
#endif
    {
        private string m_TypeName;
        private CollectionCategory m_Category;

        /// <summary>
        /// assembly qualified type name
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlAttribute()]
        public string TypeName
        {
            get
            {
                return m_TypeName;
            }
            set
            {
                m_TypeName = value;
            }
        }

        /// <summary>
        /// Which combobox in the UI this collection type should go into
        ///   (the "Dictionary collection type" or "List collection type"
        ///   combobox) in the Configure Service Dialog.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlAttribute()]
        public CollectionCategory Category
        {
            get
            {
                return m_Category;
            }
            set
            {
                m_Category = value;
            }
        }


        /// <summary>
        /// Which combobox in the UI this collection type should go into
        ///   (the "Dictionary collection type" or "List collection type"
        ///   combobox) in the Configure Service Dialog.
        /// </summary>
        /// <remarks></remarks>
        public enum CollectionCategory
        {
            [XmlSerialization.XmlEnum(Name = "Unknown")]
            Unknown = 0,

            [XmlSerialization.XmlEnum(Name = "List")]
            List = 1,

            [XmlSerialization.XmlEnum(Name = "Dictionary")]
            Dictionary = 2,
        }

    }
}

