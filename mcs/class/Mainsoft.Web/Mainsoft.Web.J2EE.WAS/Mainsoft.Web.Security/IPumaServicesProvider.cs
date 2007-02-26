//
// Mainsoft.Web.Security.IPumaServicesProvider
//
// Authors:
//	Ilya Kharmatsky (ilyak@mainsoft.com)
//
// (C) 2007 Mainsoft
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using com.ibm.portal.um;
using com.ibm.portal.um.portletservice;



namespace Mainsoft.Web.Security
{
    /// <summary>
    /// The interface provides the proxy instance to obtain the PUMA
    /// services. Since, there are two different ways to obtain PUMA
    /// Home instance (from portlet context and servlet context) we need provide
    /// some abstraction level, which will hold internally the "knowledge" - how to get
    /// services - such as PumaController, PumaLocator etc.
    /// </summary>
    public interface IPumaServicesProvider
    {
        /// <summary>
        /// Returns the com.ibm.portal.um.PumaController instance, usually used
        /// to read and/or write attributes of com.ibm.portal.um.User,
        /// com.ibm.portal.um.Group objects.
        /// </summary>
        PumaController PumaController { get;}

        /// <summary>
        /// Returns the com.ibm.portal.um.PumaLocator instance, 
        /// which provides functions to lookup 
        /// com.ibm.portal.um.User,com.ibm.portal.um.Group objects.
        /// </summary>
        PumaLocator PumaLocator { get; }

        /// <summary>
        /// Returns the com.ibm.portal.um.PumaProfile instance, which
        /// provides functions to read attribute of 
        /// com.ibm.portal.um.User,com.ibm.portal.um.Group objects.
        /// </summary>
        PumaProfile PumaProfile { get; }
        
        /// <summary>
        /// Returns the instance of Puma's User - current user, which 
        /// authenticated in portal for current request.
        /// </summary>
        User CurrentUser { get; }


        /// <summary>
        /// Adds attribute for given principal (User or Group)
        /// </summary>
        /// <param name="p">the instance of PUMA's User or Group</param>
        /// <param name="attributeName">the name of an attribute</param>
        /// <param name="attributeValue">the value to be assigned</param>
        void AddAttribute(Principal p, string attributeName, string attributeValue);

        /// <summary>
        /// Adds all given attributes (IDictionary instance, which includes pairs - 
        /// attributeName -> attributeValue) to the given instance of Principal.
        /// </summary>
        /// <param name="p">User or Group</param>
        /// <param name="attributes">attributes to be added to given principal</param>
        void AddAllAttributes(Principal p, IDictionary attributes);

        /// <summary>
        /// Returns all attributes defined to given principal (User or Group)
        /// </summary>
        /// <param name="p"></param>
        /// <returns>guess what</returns>
        IDictionary GetAllAttributes(Principal p);
        
        /// <summary>
        /// Returns map attributeName->values defined for given Principal and 
        /// given attributeName (note: usually the IDictionary.Count will be less or equal
        /// than 1)
        /// </summary>
        /// <param name="p"></param>
        /// <param name="attibute"></param>
        /// <returns></returns>
        IDictionary GetAttributes(Principal p, string attibute);

        /// <summary>
        /// Same as above, but for given 2 parameters
        /// </summary>
        /// <param name="p"></param>
        /// <param name="attibute1"></param>
        /// <param name="attribute2"></param>
        /// <returns></returns>
        IDictionary GetAttributes(Principal p, string attibute1, string attribute2);
        
        /// <summary>
        /// Same as above, but for 'a lot' of parameters (256 max?)
        /// </summary>
        /// <param name="p"></param>
        /// <param name="attibs"></param>
        /// <returns></returns>
        IDictionary GetAttributes(Principal p, params string[] attibs);

        /// <summary>
        /// Should return string representation of configuration attribute - it could be for instance:
        /// a) Init parameter of portlet
        /// b) Environment variable 
        /// c) Init parameter of servlet / servlet config
        /// d) JNDI entry
        /// 
        /// For more details please see concrete implementation of this interface.
        /// </summary>
        /// <param name="attribName">the name of attribute</param>
        /// <returns>null in case no configuration attribute defined with given name, otherwise
        /// string representation of value</returns>
        string GetConfigAttribute(string attribName);
        
    }
}

#endif
