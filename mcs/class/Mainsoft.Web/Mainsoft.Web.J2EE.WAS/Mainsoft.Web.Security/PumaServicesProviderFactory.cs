//
// Mainsoft.Web.Security.PumaServicesProviderFactory
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
using System.Collections.Generic;
using System.Text;

namespace Mainsoft.Web.Security
{
    /// <summary>
    /// The class is a factory class for creation of IPumaServicesProvider(s).
    /// It is not really needed in current implementation (when we have only one implementation class-
    /// PumaServicesProvider), but since we want to add additional implementation - based on 
    /// com.ibm.portal.um.PumaAdminHome API and according to configuration to choose provider, we
    /// are providing the place where those operation could be done (yes it is here - see CreateProvider)
    /// </summary>
    public class PumaServicesProviderFactory
    {
        private PumaServicesProviderFactory()
        {
        }

        public static IPumaServicesProvider CreateProvider()
        {
            return new PumaServicesProvider();
        }
    }
}
#endif
