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
	public abstract class DirectoryServer : IDisposable
	{
		public string Name {
			get {
				throw new NotImplementedException ();
			}
		}

		public ReadOnlyStringCollection Partitions {
			get {
				throw new NotImplementedException ();
			}
		}

		public abstract string IPAddress {
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
			get;
		}

		public abstract string SiteName {
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
			get;
		}

		public abstract SyncUpdateCallback SyncFromAllServersCallback {
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
			get;
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
			set;
		}

		public abstract ReplicationConnectionCollection InboundConnections {
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
			get;
		}

		public abstract ReplicationConnectionCollection OutboundConnections {
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
			get;
		}

		internal DirectoryContext Context {
			get {
				throw new NotImplementedException ();
			}
		}

		public void Dispose ()
		{

		}

		protected virtual void Dispose (bool disposing)
		{

		}

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		public void MoveToAnotherSite (string siteName)
		{
			throw new NotImplementedException ();
		}

		public DirectoryEntry GetDirectoryEntry ()
		{
			throw new NotImplementedException ();
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public abstract void CheckReplicationConsistency ();

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public abstract ReplicationCursorCollection GetReplicationCursors (string partition);

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public abstract ReplicationOperationInformation GetReplicationOperationInformation ();

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public abstract ReplicationNeighborCollection GetReplicationNeighbors (string partition);

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public abstract ReplicationNeighborCollection GetAllReplicationNeighbors ();

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public abstract ReplicationFailureCollection GetReplicationConnectionFailures ();

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public abstract ActiveDirectoryReplicationMetadata GetReplicationMetadata (string objectPath);

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public abstract void SyncReplicaFromServer (string partition, string sourceServer);

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public abstract void TriggerSyncReplicaFromNeighbors (string partition);

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public abstract void SyncReplicaFromAllServers (string partition, SyncFromAllServersOptions options);
	}

}
