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
using System.Net;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true)]
	public class DomainController : DirectoryServer
	{
		public Forest Forest {
			get {
				throw new NotImplementedException ();
			}
		}

		public DateTime CurrentTime {
			get {
				throw new NotImplementedException ();
			}
		}

		public long HighestCommittedUsn {
			get {
				throw new NotImplementedException ();
			}
		}

		public string OSVersion {
			get {
				throw new NotImplementedException ();
			}
		}

		public ActiveDirectoryRoleCollection Roles {
			get {
				throw new NotImplementedException ();
			}
		}

		public Domain Domain {
			get {
				throw new NotImplementedException ();
			}
		}

		public override string IPAddress {
			[DnsPermission(SecurityAction.Assert, Unrestricted = true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
			get {
				throw new NotImplementedException ();
			}
		}

		public override string SiteName {
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
			get {
				throw new NotImplementedException ();
			}
		}

		public override SyncUpdateCallback SyncFromAllServersCallback {
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
			get {
				throw new NotImplementedException ();
			}
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
			set {
				throw new NotImplementedException ();
			}
		}

		public override ReplicationConnectionCollection InboundConnections {
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
			get {
				throw new NotImplementedException ();
			}
		}

		public override ReplicationConnectionCollection OutboundConnections {
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
			get {
				throw new NotImplementedException ();
			}
		}

		protected DomainController ()
		{
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose ();

		}

		public static DomainController GetDomainController (DirectoryContext context)
		{
			throw new NotImplementedException ();
		}

		public static DomainController FindOne (DirectoryContext context)
		{
			throw new NotImplementedException ();
		}

		public static DomainController FindOne (DirectoryContext context, string siteName)
		{
			throw new NotImplementedException ();
		}

		public static DomainController FindOne (DirectoryContext context, LocatorOptions flag)
		{
			throw new NotImplementedException ();
		}

		public static DomainController FindOne (DirectoryContext context, string siteName, LocatorOptions flag)
		{
			throw new NotImplementedException ();
		}

		public static DomainControllerCollection FindAll (DirectoryContext context)
		{
			throw new NotImplementedException ();
		}

		public static DomainControllerCollection FindAll (DirectoryContext context, string siteName)
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public virtual GlobalCatalog EnableGlobalCatalog ()
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public virtual bool IsGlobalCatalog ()
		{
			throw new NotImplementedException ();
		}

		public void TransferRoleOwnership (ActiveDirectoryRole role)
		{
			throw new NotImplementedException ();
		}

		public void SeizeRoleOwnership (ActiveDirectoryRole role)
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public virtual DirectorySearcher GetDirectorySearcher ()
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public override void CheckReplicationConsistency ()
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public override ReplicationCursorCollection GetReplicationCursors (string partition)
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public override ReplicationOperationInformation GetReplicationOperationInformation ()
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public override ReplicationNeighborCollection GetReplicationNeighbors (string partition)
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public override ReplicationNeighborCollection GetAllReplicationNeighbors ()
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public override ReplicationFailureCollection GetReplicationConnectionFailures ()
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public override ActiveDirectoryReplicationMetadata GetReplicationMetadata (string objectPath)
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public override void SyncReplicaFromServer (string partition, string sourceServer)
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public override void TriggerSyncReplicaFromNeighbors (string partition)
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public override void SyncReplicaFromAllServers (string partition, SyncFromAllServersOptions options)
		{
			throw new NotImplementedException ();
		}

	}
}
