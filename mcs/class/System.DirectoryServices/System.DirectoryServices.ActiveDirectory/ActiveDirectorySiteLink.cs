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
	public class ActiveDirectorySiteLink : IDisposable
	{
		public string Name {
			get {
				throw new NotImplementedException ();
			}
		}

		public ActiveDirectoryTransportType TransportType {
			get {
				throw new NotImplementedException ();
			}
		}

		public ActiveDirectorySiteCollection Sites {
			get {
				throw new NotImplementedException ();
			}
		}

		public int Cost {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public TimeSpan ReplicationInterval {
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

		public bool NotificationEnabled {
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

		public ActiveDirectorySchedule InterSiteReplicationSchedule {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public ActiveDirectorySiteLink (DirectoryContext context, string siteLinkName) : this(context, siteLinkName, ActiveDirectoryTransportType.Rpc, null)
		{
		}

		public ActiveDirectorySiteLink (DirectoryContext context, string siteLinkName, ActiveDirectoryTransportType transport) : this(context, siteLinkName, transport, null)
		{
		}

		public ActiveDirectorySiteLink (DirectoryContext context, string siteLinkName, ActiveDirectoryTransportType transport, ActiveDirectorySchedule schedule)
		{
		}

		public static ActiveDirectorySiteLink FindByName (DirectoryContext context, string siteLinkName)
		{
			throw new NotImplementedException ();
		}

		public static ActiveDirectorySiteLink FindByName (DirectoryContext context, string siteLinkName, ActiveDirectoryTransportType transport)
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
