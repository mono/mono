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
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted = true)]
	public class ReplicationConnection : IDisposable
	{
		public string Name {
			get {
				throw new NotImplementedException ();
			}
		}

		public string SourceServer {
			get {
				throw new NotImplementedException ();
			}
		}

		public string DestinationServer {
			get {
				throw new NotImplementedException ();
			}
		}

		public bool Enabled {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public ActiveDirectoryTransportType TransportType {
			get {
				throw new NotImplementedException ();
			}
		}

		public bool GeneratedByKcc {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public bool ReciprocalReplicationEnabled {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public NotificationStatus ChangeNotificationStatus {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public bool DataCompressionEnabled {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public bool ReplicationScheduleOwnedByUser {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public ReplicationSpan ReplicationSpan {
			get {
				throw new NotImplementedException ();
			}
		}

		public ActiveDirectorySchedule ReplicationSchedule {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public static ReplicationConnection FindByName (DirectoryContext context, string name)
		{
			throw new NotImplementedException ();
		}

		public ReplicationConnection (DirectoryContext context, string name, DirectoryServer sourceServer) : this(context, name, sourceServer, null, ActiveDirectoryTransportType.Rpc)
		{
			throw new NotImplementedException ();
		}

		public ReplicationConnection (DirectoryContext context, string name, DirectoryServer sourceServer, ActiveDirectorySchedule schedule) : this(context, name, sourceServer, schedule, ActiveDirectoryTransportType.Rpc)
		{
			throw new NotImplementedException ();
		}

		public ReplicationConnection (DirectoryContext context, string name, DirectoryServer sourceServer, ActiveDirectoryTransportType transport) : this(context, name, sourceServer, null, transport)
		{
			throw new NotImplementedException ();
		}

		public ReplicationConnection (DirectoryContext context, string name, DirectoryServer sourceServer, ActiveDirectorySchedule schedule, ActiveDirectoryTransportType transport)
		{
			throw new NotImplementedException ();
		}

		public void Dispose ()
		{
			throw new NotImplementedException ();
		}

		protected virtual void Dispose (bool disposing)
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

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		public DirectoryEntry GetDirectoryEntry ()
		{
			throw new NotImplementedException ();
		}

	}
}
