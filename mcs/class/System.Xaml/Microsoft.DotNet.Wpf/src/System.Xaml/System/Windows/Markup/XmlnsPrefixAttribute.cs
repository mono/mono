// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Microsoft Windows Client Platform
//
//
//  Contents: Namespace default prefix recommendation support 

//  Created:   04/28/2005 Microsoft
//

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Windows.Markup
{
    /// <summary>
    ///
    /// This attribute allows an assembly to recommend a prefix to be used when writing elements and
    /// attributes in a xaml file. 
    /// 
    /// For a WinFX assembly, it can set the attributes as follows:
    ///
    /// <code>
    /// [assembly:XmlnsDefinition("http://schemas.fabrikam.com/mynamespace", "fabrikam.myproduct.mycategory1")]
    /// [assembly:XmlnsDefinition("http://schemas.fabrikam.com/mynamespace", "fabrikam.myproduct.mycategory2")]
    /// [assembly:XmlnsPrefix("http://schemas.fabrikam.com/mynamespace", "myns")]
    /// </code>
    /// 
    /// If fabrikam.myproduct.mycategory namespace in this assembly contains a UIElement such as "MyButton", the 
    /// xaml file could use it like below:
    ///   <code>
    ///   &lt;Page xmlns:myns="http://schemas.fabrikam.com/mynamespace" .... &gt;
    ///      &lt;myns:MyButton&gt; ..... &lt;/myns:MyButton&gt;
    ///   &lt;/Page&gt;
    ///   </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public sealed class XmlnsPrefixAttribute : Attribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="xmlNamespace">XML namespce</param>
        /// <param name="prefix">recommended prefix</param>
        public XmlnsPrefixAttribute(string xmlNamespace, string prefix)
        {
            // Validate Input Arguments
            if (xmlNamespace == null)
            {
                throw new ArgumentNullException("xmlNamespace");
            }

            if (prefix == null)
            {
                throw new ArgumentNullException("prefix");
            }

            _xmlNamespace = xmlNamespace;
            _prefix= prefix;
        }

        #region public properties

        /// <summary>
        /// XML Namespace
        /// </summary>
        public string XmlNamespace 
        {
            get { return _xmlNamespace; }
        }

        /// <summary>
        /// New Xml Namespace
        /// </summary>
        public string Prefix
        {
            get { return _prefix; }
        }

        #endregion public properties

  
        #region Private Fields

        private string _xmlNamespace;
        private string _prefix;

        #endregion Private Fields

   }
}

