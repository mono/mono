//
// Mainsoft.Web.Security.WPAuthenticationModule
//
// Authors:
//	Ilya Kharmatsky (ilyak@mainsoft.com)
//
// (C) 2007 Mainsoft Co. (http://www.mainsoft.com)
//

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

using System;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Util;
using System.Web.Security;


using javax.portlet;

using vmw.portlet;

namespace Mainsoft.Web.Security
{
    public sealed class WPAuthenticationModule : IHttpModule
    {
        public void Dispose()
        {
        }

        public void Init(HttpApplication app)
        {
            app.AuthenticateRequest += new EventHandler(OnAuthenticateRequest);
        }

        void OnAuthenticateRequest(object sender, EventArgs args)
        {
            HttpApplication app = (HttpApplication)sender;
            PortletRequest req = vmw.portlet.PortletUtils.getPortletRequest();
            if (req != null)
                app.Context.User = new WPPrincipal(req);
        }
    }

    internal class WPPrincipal : IPrincipal
    {
        private IIdentity _identity;
        private string _username;

        public WPPrincipal(PortletRequest req)
        {
            string authType = req.getAuthType();
            if (authType == null)
                authType = "";
            IPumaServicesProvider provider = PumaServicesProviderFactory.CreateProvider();
            _username = provider.CurrentUserName;
            _identity = new GenericIdentity(_username, authType);
        }

        public bool IsInRole(string role)
        {
            if (role == null)
                return false;

            if (_username == null)
                return false;

            return false;

//            try
//            {
//                return Roles.IsUserInRole(_username, role);
//            }
//            catch (Exception e)
//            {
//#if DEBUG
//                Console.WriteLine(e);
//#endif
//                return false;
//            }

        }

        public IIdentity Identity { get { return _identity; } }
    }
}

