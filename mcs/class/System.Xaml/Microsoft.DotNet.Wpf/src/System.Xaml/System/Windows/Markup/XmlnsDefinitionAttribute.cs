// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Microsoft Windows Client Platform
//
//
//  Contents: Attribute that keep mapping between Xml namespace and  
//            the known types in assembly.   

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Windows.Markup
{
    /// <summary>
    ///
    /// XmlnsDefinitionAttribute keeps a mapping between Xml namespace and CLR namespace in an Assembly.
    /// The Xml namespace can be used in a Xaml Markup file.
    /// 
    ///  
    /// To find the appropriate types for element and attribute in xaml file, xaml processors MUST 
    /// search each referenced assembly for XmlnsDefinitionAttribute. If the xmlns for element tag 
    /// or attribute matches with the XmlNamespace in this XmlnsDefinitionAttibute, the Xaml processor
    /// then takes use of the ClrNamespace and AssemblyName stored in this Attibute instance to check 
    /// if the element or attribute matches any type inside this namespace in the Assembly.
    /// 
    /// For a WinFX assembly, it can set this attibute like below:
    /// 
    /// [assembly:XmlnsDefinition("http://schemas.fabrikam.com/mynamespace", "fabrikam.myproduct.mycategory1")]
    /// [assembly:XmlnsDefinition("http://schemas.fabrikam.com/mynamespace", "fabrikam.myproduct.mycategory2")]
    /// 
    /// [assembly:XmlnsDefinition("xmlnamsspace", "clrnamespace", AssemblyName="myassembly or full assemblyname")]
    /// 
    /// If fabrikam.myproduct.mycategory namespace in this assembly contains a UIElement such as "MyButton", the 
    /// xaml file could use it like below:
    /// 
    ///   Page xmlns:myns="http://schemas.fabrikam.com/mynamespace" .... 
    ///      myns:MyButton ...../myns:MyButton
    ///   /Page
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public sealed class XmlnsDefinitionAttribute: Attribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="xmlNamespace">
        /// XmlNamespace used by Markup file
        /// </param>
        /// <param name="clrNamespace">
        /// Clr namespace which contains known types that are used by Markup File.
        /// </param>
        public XmlnsDefinitionAttribute(string xmlNamespace, string clrNamespace)
        {
            // Validate Input Arguments
            if (xmlNamespace == null)
            {
                throw new ArgumentNullException("xmlNamespace");
            }

            if (clrNamespace == null)
            {
                throw new ArgumentNullException("clrNamespace");
            }

            _xmlNamespace = xmlNamespace;
            _clrNamespace = clrNamespace;
        }

        #region public properties

        /// <summary>
        /// XmlNamespace which can be used in Markup file.
        /// such as XmlNamespace is set to 
        /// "http://schemas.fabrikam.com/mynamespace".
        /// 
        /// The markup file can have definition like
        /// xmlns:myns="http://schemas.fabrikam.com/mynamespace" 
        /// 
        /// </summary>
        public string XmlNamespace 
        {
            get { return _xmlNamespace; }
        }

        /// <summary>
        /// ClrNamespace which map to XmlNamespace.
        /// This ClrNamespace should contain some types which are used 
        /// by Xaml markup file.
        /// </summary>
        public string ClrNamespace 
        {
            get { return _clrNamespace; }
        }

        /// <summary>
        /// The name of Assembly that contains some types inside CLRNamespace.
        /// If the assemblyName is not set, the code should take the assembly 
        /// for which the instance of this attribute is created. 
        /// </summary>
        public string AssemblyName
        {
            get {  return _assemblyName; }
            set { _assemblyName = value; }
        }

        #endregion public properties

  
        #region Private Fields

        private string _xmlNamespace;
        private string _clrNamespace;
        private string _assemblyName;

        #endregion Private Fields

   }
}

