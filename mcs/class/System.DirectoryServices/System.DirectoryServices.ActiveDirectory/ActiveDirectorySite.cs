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
	public class ActiveDirectorySite : IDisposable
	{
		public string Name {
			get {
				throw new NotImplementedException ();
			}
		}

		public DomainCollection Domains {
			get {
				throw new NotImplementedException ();
			}
		}

		public ActiveDirectorySubnetCollection Subnets {
			get {
				throw new NotImplementedException ();
			}
		}

		public ReadOnlyDirectoryServerCollection Servers {
			get {
				throw new NotImplementedException ();
			}
		}

		public ReadOnlySiteCollection AdjacentSites {
			get {
				throw new NotImplementedException ();
			}
		}

		public ReadOnlySiteLinkCollection SiteLinks {
			get {
				throw new NotImplementedException ();
			}
		}

		public DirectoryServer InterSiteTopologyGenerator {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public ActiveDirectorySiteOptions Options {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public string Location {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public ReadOnlyDirectoryServerCollection BridgeheadServers {
			get {
				throw new NotImplementedException ();
			}
		}

		public DirectoryServerCollection PreferredSmtpBridgeheadServers {
			get {
				throw new NotImplementedException ();
			}
		}

		public DirectoryServerCollection PreferredRpcBridgeheadServers {
			get {
				throw new NotImplementedException ();
			}
		}

		public ActiveDirectorySchedule IntraSiteReplicationSchedule {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public static ActiveDirectorySite FindByName (DirectoryContext context, string siteName)
		{
			throw new NotImplementedException ();
		}

		public ActiveDirectorySite (DirectoryContext context, string siteName)
		{
			throw new NotImplementedException ();
		}

		public static ActiveDirectorySite GetComputerSite ()
		{
			throw new NotImplementedException ();
		}

		public void Save ()
		{
			throw new NotImplementedException ();
		}

		public void Delete ()
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		public DirectoryEntry GetDirectoryEntry ()
		{
			throw new NotImplementedException ();
		}

		public void Dispose ()
		{

		}

		protected virtual void Dispose (bool disposing)
		{

		}
	}
}
