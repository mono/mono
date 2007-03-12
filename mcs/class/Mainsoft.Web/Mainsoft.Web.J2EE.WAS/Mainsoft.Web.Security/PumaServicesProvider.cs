//
// Mainsoft.Web.Security.PumaServicesProvider
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


using java.util;

using javax.portlet;
using javax.servlet;
using javax.servlet.http;
using javax.naming;


using com.ibm.portal.um;
using com.ibm.portal.um.portletservice;

using com.ibm.portal.portlet.service;

using vmw.portlet;

namespace Mainsoft.Web.Security
{
    /// <summary>
    /// The class implements interface IPumaServiceProvider, by providing an access to the
    /// PumaLocator, PumaController and PumaProfile interfaces through PumaHome API. 
    /// Note: PumaHome - will be different for servlet and portlet context, the class hides 
    /// the different PumaHome approaches under internal class - PumaHomeWrapper.
    /// 
    /// </summary>
    internal class PumaServicesProvider : IPumaServicesProvider
    {
        private readonly static string VMW_SESSION_ATTRIB_HOME_NAME_WRAPPER = "VMW_PUMA_HOME_WRAPPER_";
        
        private PumaHomeWrapper _pumaHome;

        private java.util.List _nameAttributeList;

        #region Initialization 
        
        internal PumaServicesProvider()
        {
            PortletRequest pr = vmw.portlet.PortletUtils.getPortletRequest();
            if (pr != null)
            {
                _pumaHome = GetPumaHomeWrapper(pr);
            }
            else
            {
                HttpServletRequest httpReq = vmw.j2ee.J2EEUtils.getHttpServletRequest();
                if (httpReq == null)
                    throw new ApplicationException("Cannot obtain servlet/portlet request");
                _pumaHome = GetPumaHomeWrapper(httpReq);
            }

            _nameAttributeList = new java.util.ArrayList(1);
            _nameAttributeList.add("uid");
        }

        /* Needs HttpServletRequest because ServletRequest doesn't contain getSession method*/
        private PumaHomeWrapper GetPumaHomeWrapper(HttpServletRequest request)
        {
            HttpSession session = request.getSession(true);
            PumaHomeWrapper phw = (PumaHomeWrapper)
                    session.getAttribute(VMW_SESSION_ATTRIB_HOME_NAME_WRAPPER);
            if (phw == null)
            {
                try
                {
                    Context ctx = new InitialContext();
                    Name jndiName = new CompositeName(com.ibm.portal.um.PumaHome__Finals.JNDI_NAME);
                    com.ibm.portal.um.PumaHome ph = (com.ibm.portal.um.PumaHome)ctx.lookup(jndiName);
                    phw = new PumaHomeWrapper(ph);
                    session.setAttribute(VMW_SESSION_ATTRIB_HOME_NAME_WRAPPER, phw);
                }
                catch (Exception e)
                {
                    throw new ApplicationException("Cannot initialized servlet PUMA Home from JNDI", e);
                }
            }
            return phw;
        }

        private PumaHomeWrapper GetPumaHomeWrapper(PortletRequest pr)
        {

            PortletSession session = pr.getPortletSession(true);
            PumaHomeWrapper phw = (PumaHomeWrapper) session.getAttribute(VMW_SESSION_ATTRIB_HOME_NAME_WRAPPER);
            if (phw == null)
            {
                try
                {
                    Context cntx = new InitialContext();
                    PortletServiceHome psh = (PortletServiceHome)
                        cntx.lookup("portletservice/com.ibm.portal.um.portletservice.PumaHome");
                    if (psh == null)
                        throw new ApplicationException("Cannot find PortletServiceHome in JNDI");
                    com.ibm.portal.um.portletservice.PumaHome ph = (com.ibm.portal.um.portletservice.PumaHome)psh.getPortletService(
                        vmw.common.TypeUtils.ToClass(typeof(com.ibm.portal.um.portletservice.PumaHome)));
                    phw = new PumaHomeWrapper(ph);
                    session.setAttribute(VMW_SESSION_ATTRIB_HOME_NAME_WRAPPER, phw);
                }
                catch (Exception e)
                {
                    throw new ApplicationException("Cannot initialize portlet PUMA HOME", e);
                }
            }
            return phw;
        }
        #endregion

        #region IPumaServicesProvider Members

        public PumaController PumaController
        {
            get { return _pumaHome.PumaController; }
        }

        public com.ibm.portal.um.PumaLocator PumaLocator
        {
            get { return _pumaHome.PumaLocator; }
        }

        public com.ibm.portal.um.PumaProfile PumaProfile
        {
            get { return _pumaHome.PumaProfile; }
        }

        public com.ibm.portal.um.User CurrentUser
        {
            get { return _pumaHome.PumaProfile.getCurrentUser(); }
        }

        public string CurrentUserName
        {
            get
            {
                com.ibm.portal.um.User user = CurrentUser;
                string username = null;
                
                if (user != null)
                {
                    java.util.Map m = PumaProfile.getAttributes(user, _nameAttributeList);
                    username = (string)m.get("uid");
                }

                return username;
            }
        }

        public void AddAttribute(com.ibm.portal.um.Principal p, string attributeName, string attributeValue)
        {
            HashMap map = new HashMap();
            map.put(attributeName, attributeValue);
            _pumaHome.PumaController.addAttributes(p, map);
        }

        public void AddAllAttributes(com.ibm.portal.um.Principal p, IDictionary attributes)
        {
            _pumaHome.PumaController.addAttributes(p, ConvertToJavaMap(attributes));
        }

        public IDictionary GetAllAttributes(com.ibm.portal.um.Principal p)
        {
            PumaProfile profile = _pumaHome.PumaProfile;
            java.util.List definedAttribs = profile.getDefinedUserAttributeNames();
            java.util.Map map = profile.getAttributes(p, definedAttribs);
            return ConvertToIDictionary(map);
        }

        public IDictionary GetAttributes(com.ibm.portal.um.Principal p, string attribute)
        {
            return GetAttributes(p, new string[] { attribute });
        }

        public IDictionary GetAttributes(com.ibm.portal.um.Principal p, string attibute1, string attribute2)
        {
            return GetAttributes(p, new string[] { attibute1, attribute2 });
        }

        public IDictionary GetAttributes(com.ibm.portal.um.Principal p, params string[] attribs)
        {
            PumaProfile profile = _pumaHome.PumaProfile;
            java.util.ArrayList list = new java.util.ArrayList(1);
            foreach (string s in attribs)
                list.add(s);

            java.util.Map map = profile.getAttributes(p, list);
            return ConvertToIDictionary(map);
        }

        public string GetConfigAttribute(string attribName)
        {
            return _pumaHome.GetConfigAttribute(attribName);
        }

        #endregion

        #region Convert Java <-> .Net collection helper methods
        internal static IDictionary ConvertToIDictionary(java.util.Map map)
        {
            System.Collections.Hashtable ht = new System.Collections.Hashtable();
            if (map.size() > 0)
            {
                for (java.util.Iterator iter = map.keySet().iterator(); iter.hasNext(); )
                {
                    object key = iter.next();
                    ht[key] = map.get(key);
                }
            }
            return (IDictionary) ht;
        }
        internal static java.util.Map ConvertToJavaMap(IDictionary dictionary)
        {
            
            HashMap res = new HashMap();
            if (dictionary.Count > 0)
            {
                foreach (object key in dictionary.Keys)
                    res.put(key, dictionary[key]);
            }
            return res;
        }
        #endregion

        #region PumaHomeWrapper class
        private class PumaHomeWrapper
        {
            private com.ibm.portal.um.PumaHome _servletPumaHome = null;
            private com.ibm.portal.um.portletservice.PumaHome _portletPumaHome = null;

            private bool _isPortletPumaHome;

            public PumaHomeWrapper(com.ibm.portal.um.PumaHome pumaHome)
            {
                if (pumaHome == null)
                    throw new ArgumentNullException("pumaHome");

                _servletPumaHome = pumaHome;
                _isPortletPumaHome = false;
            }

            public PumaHomeWrapper(com.ibm.portal.um.portletservice.PumaHome pumaHome)
            {
                if (pumaHome == null)
                    throw new ArgumentNullException("pumaHome");

                _portletPumaHome = pumaHome;
                _isPortletPumaHome = true;
            }

            public bool IsPortletPumaHome
            {
                get { return _isPortletPumaHome; }
            }

            public PumaController PumaController
            {
                get
                {
                    if (_isPortletPumaHome)
                    {
                        ActionRequest ar = CurrentPortletRequest as ActionRequest;
                        if (ar == null)
                            throw new ApplicationException("The page is not in process action phase");
                        return _portletPumaHome.getController(ar);
                    }
                    else
                    {
                        return _servletPumaHome.getController(CurrentServletRequest);
                    }
                }
            }

            public PumaLocator PumaLocator
            {
                get
                {
                    if (IsPortletPumaHome)
                        return _portletPumaHome.getLocator(CurrentPortletRequest);
                    else
                        return _servletPumaHome.getLocator(CurrentServletRequest);
                }
            }

            public PumaProfile PumaProfile
            {
                get
                {
                    if (IsPortletPumaHome)
                        return _portletPumaHome.getProfile(CurrentPortletRequest);
                    else
                        return _servletPumaHome.getProfile(CurrentServletRequest);
                }
            }

            public string GetConfigAttribute(string attribute)
            {
                //optimization - instead of retrieving the attribute from JNDI each time
                //lazy store it into session
                if (IsPortletPumaHome)
                {
                    PortletSession pSession = CurrentPortletRequest.getPortletSession();
                    if (pSession != null)
                    {
                        string attribValue = (string)pSession.getAttribute(attribute);
                        if (attribValue == null)
                        {
                            attribValue = (string)ReadValueFromJNDI(attribute);
                            pSession.setAttribute(attribute, attribValue);
                        }
                        return attribValue;
                    }
                }
                else
                {
                    HttpSession hSession = CurrentServletRequest.getSession();
                    if (hSession != null)
                    {
                        string attribValue = (string) hSession.getAttribute(attribute);
                        if (attribValue == null)
                        {
                            attribValue = (string)ReadValueFromJNDI(attribute);
                            hSession.setAttribute(attribute, attribValue);
                        }
                        return attribValue;
                    }
                }
                //case when session (portlet or http) is not available
                return (string)ReadValueFromJNDI(attribute);
            }

            private object ReadValueFromJNDI(string attribName)
            {
                Context env = (Context)new InitialContext().lookup("java:comp/env");
                return env.lookup(attribName);
            }

            private HttpServletRequest CurrentServletRequest
            {
                get
                {
                    HttpServletRequest request = vmw.j2ee.J2EEUtils.getHttpServletRequest();
                    if (request == null)
                        throw new ApplicationException("Cannot retieve servlet request");
                    return request;
                }
            }

            private PortletRequest CurrentPortletRequest
            {
                get
                {
                    PortletRequest request = vmw.portlet.PortletUtils.getPortletRequest();
                    if (request == null)
                        throw new ApplicationException("Cannot retrive portlet request");
                    return request;
                }
            }
        }
        #endregion
    }

    
}

#endif