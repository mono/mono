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

namespace System.DirectoryServices.ActiveDirectory
{
	public class ReplicationNeighbor
	{
		[Flags]
		public enum ReplicationNeighborOptions : long
		{
			Writeable = 16L,
			SyncOnStartup = 32L,
			ScheduledSync = 64L,
			UseInterSiteTransport = 128L,
			TwoWaySync = 512L,
			ReturnObjectParent = 2048L,
			FullSyncInProgress = 65536L,
			FullSyncNextPacket = 131072L,
			NeverSynced = 2097152L,
			Preempted = 16777216L,
			IgnoreChangeNotifications = 67108864L,
			DisableScheduledSync = 134217728L,
			CompressChanges = 268435456L,
			NoChangeNotifications = 536870912L,
			PartialAttributeSet = 1073741824L
		}

		public string PartitionName {
			get {
				throw new NotImplementedException ();
			}
		}

		public string SourceServer {
			get {
				throw new NotImplementedException ();
			}
		}

		public ActiveDirectoryTransportType TransportType {
			get {
				throw new NotImplementedException ();
			}
		}

		public ReplicationNeighbor.ReplicationNeighborOptions ReplicationNeighborOption {
			get {
				throw new NotImplementedException ();
			}
		}

		public Guid SourceInvocationId {
			get {
				throw new NotImplementedException ();
			}
		}
		
		public long UsnLastObjectChangeSynced {
			get {
				throw new NotImplementedException ();
			}
		}
		
		public long UsnAttributeFilter {
			get {
				throw new NotImplementedException ();
			}
		}
		
		public DateTime LastSuccessfulSync {
			get {
				throw new NotImplementedException ();
			}
		}
		
		public DateTime LastAttemptedSync {
			get {
				throw new NotImplementedException ();
			}
		}
		
		public int LastSyncResult {
			get {
				throw new NotImplementedException ();
			}
		}
		
		public string LastSyncMessage {
			get {
				throw new NotImplementedException ();
			}
		}

		public int ConsecutiveFailureCount {
			get {
				throw new NotImplementedException ();
			}
		}
		
	}
}
