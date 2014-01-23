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
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true)]
	public class ApplicationPartition : ActiveDirectoryPartition
	{
		public DirectoryServerCollection DirectoryServers {
			get {
				throw new NotImplementedException ();
			}
		}

		public string SecurityReferenceDomain {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public ApplicationPartition (DirectoryContext context, string distinguishedName)
		{
			throw new NotImplementedException ();
		}

		public ApplicationPartition (DirectoryContext context, string distinguishedName, string objectClass)
		{
			throw new NotImplementedException ();
		}

		protected override void Dispose (bool disposing)
		{

		}

		public static ApplicationPartition GetApplicationPartition (DirectoryContext context)
		{
			throw new NotImplementedException ();
		}

		public static ApplicationPartition FindByName (DirectoryContext context, string distinguishedName)
		{
			throw new NotImplementedException ();
		}

		public DirectoryServer FindDirectoryServer ()
		{
			throw new NotImplementedException ();
		}

		public DirectoryServer FindDirectoryServer (string siteName)
		{
			throw new NotImplementedException ();
		}

		public DirectoryServer FindDirectoryServer (bool forceRediscovery)
		{
			throw new NotImplementedException ();
		}

		public DirectoryServer FindDirectoryServer (string siteName, bool forceRediscovery)
		{
			throw new NotImplementedException ();
		}

		public ReadOnlyDirectoryServerCollection FindAllDirectoryServers ()
		{
			throw new NotImplementedException ();
		}

		public ReadOnlyDirectoryServerCollection FindAllDirectoryServers (string siteName)
		{
			throw new NotImplementedException ();
		}

		public ReadOnlyDirectoryServerCollection FindAllDiscoverableDirectoryServers ()
		{
			throw new NotImplementedException ();
		}

		public ReadOnlyDirectoryServerCollection FindAllDiscoverableDirectoryServers (string siteName)
		{
			throw new NotImplementedException ();
		}

		public void Delete ()
		{
			throw new NotImplementedException ();
		}

		public void Save ()
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public override DirectoryEntry GetDirectoryEntry ()
		{
			throw new NotImplementedException ();
		}
	}
}
