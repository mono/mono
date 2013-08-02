/******************************************************************************
* The MIT License
*
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to  permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
using System;
using System.Collections;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true)]
	public class GlobalCatalog : DomainController
	{
		public static GlobalCatalog GetGlobalCatalog (DirectoryContext context)
		{
			throw new NotImplementedException ();
		}

		public new static GlobalCatalog FindOne (DirectoryContext context)
		{
			throw new NotImplementedException ();
		}

		public new static GlobalCatalog FindOne (DirectoryContext context, string siteName)
		{
			throw new NotImplementedException ();
		}

		public new static GlobalCatalog FindOne (DirectoryContext context, LocatorOptions flag)
		{
			throw new NotImplementedException ();
		}

		public new static GlobalCatalog FindOne (DirectoryContext context, string siteName, LocatorOptions flag)
		{
			throw new NotImplementedException ();
		}

		public new static GlobalCatalogCollection FindAll (DirectoryContext context)
		{
			throw new NotImplementedException ();
		}

		public new static GlobalCatalogCollection FindAll (DirectoryContext context, string siteName)
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public override GlobalCatalog EnableGlobalCatalog ()
		{
			throw new NotImplementedException ();
		}

		public DomainController DisableGlobalCatalog ()
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public override bool IsGlobalCatalog ()
		{
			throw new NotImplementedException ();
		}

		public ReadOnlyActiveDirectorySchemaPropertyCollection FindAllProperties ()
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public override DirectorySearcher GetDirectorySearcher ()
		{
			throw new NotImplementedException ();
		}

	}
}
