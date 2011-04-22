//
// System.Web.SessionStateServerHandler
//
// Authors:
//   Marek Habersack (grendello@gmail.com)
//
// (C) 2006 Marek Habersack
// (C) 2007-2010 Novell, Inc (http://novell.com/)
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
#if NET_2_0
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Web;
using System.Web.Configuration;
using System.Runtime.Remoting;
using System.Diagnostics;

namespace System.Web.SessionState 
{
	internal class SessionStateServerHandler : SessionStateStoreProviderBase
	{
		const Int32 lockAcquireTimeout = 30000;
		
		SessionStateSection config;
		RemoteStateServer stateServer;

		public override SessionStateStoreData CreateNewStoreData (HttpContext context, int timeout)
		{
			return new SessionStateStoreData (new SessionStateItemCollection (),
							  HttpApplicationFactory.ApplicationState.SessionObjects,
							  timeout);
		}
		
		public override void CreateUninitializedItem (HttpContext context, string id, int timeout)
		{
			EnsureGoodId (id, true);
			stateServer.CreateUninitializedItem (id, timeout);
		}
		
		public override void Dispose ()
		{
		}
		
		public override void EndRequest (HttpContext context)
		{
		}

		SessionStateStoreData GetItemInternal (HttpContext context,
						       string id,
						       out bool locked,
						       out TimeSpan lockAge,
						       out object lockId,
						       out SessionStateActions actions,
						       bool exclusive)
		{
			locked = false;
			lockAge = TimeSpan.MinValue;
			lockId = Int32.MinValue;
			actions = SessionStateActions.None;

			if (id == null)
				return null;
			
			StateServerItem item = stateServer.GetItem (id,
								    out locked,
								    out lockAge,
								    out lockId,
								    out actions,
								    exclusive);
			
			if (item == null)
				return null;
			
			if (actions == SessionStateActions.InitializeItem)
				return CreateNewStoreData (context, item.Timeout);
			
			SessionStateItemCollection items = null;
			HttpStaticObjectsCollection sobjs = null;
			MemoryStream stream = null;
			BinaryReader reader = null;
			Stream input = null;
#if NET_4_0
			GZipStream gzip = null;
#endif
			try {
				if (item.CollectionData != null && item.CollectionData.Length > 0) {
					stream = new MemoryStream (item.CollectionData);
#if NET_4_0					
					if (config.CompressionEnabled)
						input = gzip = new GZipStream (stream, CompressionMode.Decompress, true);
					else
#endif
						input = stream;
					reader = new BinaryReader (input);
					items = SessionStateItemCollection.Deserialize (reader);
#if NET_4_0
					if (gzip != null)
						gzip.Close ();
#endif
					reader.Close ();
				} else
					items = new SessionStateItemCollection ();
				if (item.StaticObjectsData != null && item.StaticObjectsData.Length > 0)
					sobjs = HttpStaticObjectsCollection.FromByteArray (item.StaticObjectsData);
				else
					sobjs = new HttpStaticObjectsCollection ();
			} catch (Exception ex) {
				throw new HttpException ("Failed to retrieve session state.", ex);
			} finally {
				if (stream != null)
					stream.Dispose ();
#if NET_4_0
				if (reader != null)
					reader.Dispose ();
				if (gzip != null)
					gzip.Dispose ();
#else
				if (reader != null)
					((IDisposable)reader).Dispose ();
#endif
			}
				
			return new SessionStateStoreData (items,
							  sobjs,
							  item.Timeout);
		}
		
		public override SessionStateStoreData GetItem (HttpContext context,
							       string id,
							       out bool locked,
							       out TimeSpan lockAge,
							       out object lockId,
							       out SessionStateActions actions)
		{
			EnsureGoodId (id, false);
			return GetItemInternal (context, id, out locked, out lockAge, out lockId, out actions, false);
		}
		
		public override SessionStateStoreData GetItemExclusive (HttpContext context,
									string id,
									out bool locked,
									out TimeSpan lockAge,
									out object lockId,
									out SessionStateActions actions)
		{
			EnsureGoodId (id, false);
			return GetItemInternal (context, id, out locked, out lockAge, out lockId, out actions, true);
		}

		public override void Initialize (string name, NameValueCollection config)
		{
			this.config = (SessionStateSection) WebConfigurationManager.GetSection ("system.web/sessionState");
			if (String.IsNullOrEmpty (name))
				name = "Session Server handler";
			RemotingConfiguration.Configure (null);
			string cons = null, proto = null, server = null, port = null;
                        GetConData (out proto, out server, out port);
                        cons = String.Format ("{0}://{1}:{2}/StateServer", proto, server, port);
                        stateServer = Activator.GetObject (typeof (RemoteStateServer), cons) as RemoteStateServer;

			base.Initialize (name, config);
		}
		
		public override void InitializeRequest (HttpContext context)
		{
			// nothing to do here
		}
		
		public override void ReleaseItemExclusive (HttpContext context,
							   string id,
							   object lockId)
		{
			EnsureGoodId (id, true);
			stateServer.ReleaseItemExclusive (id, lockId);
		}
		
		public override void RemoveItem (HttpContext context,
						 string id,
						 object lockId,
						 SessionStateStoreData item)
		{
			EnsureGoodId (id, true);
			stateServer.Remove (id, lockId);
		}
		
		public override void ResetItemTimeout (HttpContext context, string id)
		{
			EnsureGoodId (id, true);
			stateServer.ResetItemTimeout (id);
		}

		public override void SetAndReleaseItemExclusive (HttpContext context,
								 string id,
								 SessionStateStoreData item,
								 object lockId,
								 bool newItem)
		{
			if (item == null)
				return;
			
			EnsureGoodId (id, true);
			byte[] collection_data = null;
			byte[] sobjs_data = null;
			MemoryStream stream = null;
			BinaryWriter writer = null;
			Stream output = null;
#if NET_4_0
			GZipStream gzip = null;
#endif
			
			try {
				SessionStateItemCollection items = item.Items as SessionStateItemCollection : null;
				if (items != null && items.Count > 0) {
					stream = new MemoryStream ();
#if NET_4_0
					if (config.CompressionEnabled)
						output = gzip = new GZipStream (stream, CompressionMode.Compress, true);
					else
#endif
						output = stream;
					writer = new BinaryWriter (output);
					items.Serialize (writer);
#if NET_4_0
					if (gzip != null)
						gzip.Close ();
#endif
					writer.Close ();
					collection_data = stream.ToArray ();
				}
				HttpStaticObjectsCollection sobjs = item.StaticObjects : null;
				if (sobjs != null && sobjs.Count > 0)
					sobjs_data = sobjs.ToByteArray ();
			} catch (Exception ex) {
				throw new HttpException ("Failed to store session data.", ex);
			} finally {

#if NET_4_0
				if (writer != null)
					writer.Dispose ();
				if (gzip != null)
					gzip.Dispose ();
#else
				if (writer != null)
					((IDisposable)writer).Dispose ();
#endif
				if (stream != null)
					stream.Dispose ();
			}
			
			stateServer.SetAndReleaseItemExclusive (id, collection_data, sobjs_data, lockId, item.Timeout, newItem);
		}
		
		public override bool SetItemExpireCallback (SessionStateItemExpireCallback expireCallback)
		{
			return false;
		}

		void EnsureGoodId (string id, bool throwOnNull)
		{
			if (id == null)
				if (throwOnNull)
					throw new HttpException ("Session ID is invalid");
				else
					return;
			
			if (id.Length > SessionIDManager.SessionIDMaxLength)
				throw new HttpException ("Session ID too long");
		}

		void GetConData (out string proto, out string server, out string port)
                {
			string cons = config.StateConnectionString;
                        int ei = cons.IndexOf ('=');
                        int ci = cons.IndexOf (':');

                        if (ei < 0 || ci < 0)
                                throw new HttpException ("Invalid StateConnectionString");
                        
                        proto = cons.Substring (0, ei);
                        server = cons.Substring (ei+1, ci - ei - 1);
                        port = cons.Substring (ci+1, cons.Length - ci - 1);
			
                        if (proto == "tcpip")
                                proto = "tcp";
                }
	}
}
#endif
