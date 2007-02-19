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

        
        
    }
}
