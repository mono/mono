// System.Net.Sockets.Socket.cs
//
// Authors:
//    Phillip Pearson (pp@myelin.co.nz)
//    Dick Porter <dick@ximian.com>
//
// Copyright (C) 2001, 2002 Phillip Pearson and Ximian, Inc.
//    http://www.myelin.co.nz
//

using System;
using System.Net;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Net.Sockets 
{
	public class Socket : IDisposable 
	{
		private sealed class SocketAsyncResult: IAsyncResult 
		{
			private object state;
			private WaitHandle waithandle;
			private bool completed_sync, completed;
			private Worker worker;

			public SocketAsyncResult(object state) {
				this.state=state;
				waithandle=new ManualResetEvent(false);
				completed_sync=completed=false;
			}

			public object AsyncState {
				get {
					return(state);
				}
			}

			public WaitHandle AsyncWaitHandle {
				get {
					return(waithandle);
				}
				set {
					waithandle=value;
				}
			}

			public bool CompletedSynchronously {
				get {
					return(completed_sync);
				}
			}

			public bool IsCompleted {
				get {
					return(completed);
				}
				set {
					completed=value;
				}
			}
			
			public Worker Worker {
				get {
					return(worker);
				}
				set {
					worker=value;
				}
			}
		}

		private sealed class Worker 
		{
			private AsyncCallback callback;
			private SocketAsyncResult result;
			private Socket socket;

			// Parameters
			private EndPoint endpoint;	// Connect,ReceiveFrom,SendTo
			private byte[] buffer;		// Receive,ReceiveFrom,Send,SendTo
			private int offset;		// Receive,ReceiveFrom,Send,SendTo
			private int size;		// Receive,ReceiveFrom,Send,SendTo
			private SocketFlags sockflags;	// Receive,ReceiveFrom,Send,SendTo

			// Return values
			private Socket acc_socket;
			private int total;
			

			// For Accept
			public Worker(Socket req_sock,
				      AsyncCallback req_callback,
				      SocketAsyncResult req_result)
				: this(req_sock, null, 0, 0, SocketFlags.None,
				       null, req_callback, req_result) {}

			// For Connect
			public Worker(Socket req_sock, EndPoint req_endpoint,
				      AsyncCallback req_callback,
				      SocketAsyncResult req_result)
				: this(req_sock, null, 0, 0, SocketFlags.None,
				       req_endpoint, req_callback,
				       req_result) {}

			// For Receive and Send
			public Worker(Socket req_sock, byte[] req_buffer,
				      int req_offset, int req_size,
				      SocketFlags req_sockflags,
				      AsyncCallback req_callback,
				      SocketAsyncResult req_result)
				: this(req_sock, req_buffer, req_offset,
				       req_size, req_sockflags, null,
				       req_callback, req_result) {}

			// For ReceiveFrom and SendTo
			public Worker(Socket req_sock, byte[] req_buffer,
				      int req_offset, int req_size,
				      SocketFlags req_sockflags,
				      EndPoint req_endpoint,
				      AsyncCallback req_callback,
				      SocketAsyncResult req_result) {
				socket=req_sock;
				buffer=req_buffer;
				offset=req_offset;
				size=req_size;
				sockflags=req_sockflags;
				endpoint=req_endpoint;
				callback=req_callback;
				result=req_result;
			}

			private void End() {
				callback(result);
				((ManualResetEvent)result.AsyncWaitHandle).Set();
				result.IsCompleted=true;
			}
			
			public void Accept() {
				lock(result) {
					acc_socket=socket.Accept();
					End();
				}
			}

			public void Connect() {
				lock(result) {
					socket.Connect(endpoint);
					End();
				}
			}

			public void Receive() {
				lock(result) {
					total=socket.Receive(buffer, offset,
							     size, sockflags);
					End();
				}
			}

			public void ReceiveFrom() {
				lock(result) {
					total=socket.ReceiveFrom(buffer,
								 offset, size,
								 sockflags,
								 ref endpoint);
					End();
				}
			}

			public void Send() {
				lock(result) {
					total=socket.Send(buffer, offset, size,
							  sockflags);
					End();
				}
			}

			public void SendTo() {
				lock(result) {
					total=socket.SendTo(buffer, offset,
							    size, sockflags,
							    endpoint);
					End();
				}
			}

			public EndPoint EndPoint {
				get {
					return(endpoint);
				}
			}

			public Socket Socket {
				get {
					return(acc_socket);
				}
			}

			public int Total {
				get {
					return(total);
				}
			}
		}
			
		private IntPtr socket;
		private AddressFamily address_family;
		private SocketType socket_type;
		private ProtocolType protocol_type;
		
		[MonoTODO]
		public static void Select(IList read_list, IList write_list,
					  IList err_list, int time_us) {
			if(read_list==null &&
			   write_list==null &&
			   err_list==null) {
				throw new ArgumentNullException();
			}
			
			throw new NotImplementedException();
		}

		// private constructor used by Accept, which already
		// has a socket handle to use
		private Socket(AddressFamily family, SocketType type,
			       ProtocolType proto, IntPtr sock) {
			address_family=family;
			socket_type=type;
			protocol_type=proto;
			
			socket=sock;
		}
		
		// Creates a new system socket, returning the handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern IntPtr Socket_internal(AddressFamily family,
						      SocketType type,
						      ProtocolType proto);
		
		public Socket(AddressFamily family, SocketType type,
			      ProtocolType proto) {
			address_family=family;
			socket_type=type;
			protocol_type=proto;
			
			socket=Socket_internal(family, type, proto);
		}

		public AddressFamily AddressFamily {
			get {
				return(address_family);
			}
		}

		// Returns the amount of data waiting to be read on socket
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int Available_internal(IntPtr socket);
		
		public int Available {
			get {
				return(Available_internal(socket));
			}
		}

		[MonoTODO]
		public bool Blocking {
			get {
				throw new NotImplementedException();
				//return false;
			}
			set { }
		}

		[MonoTODO]
		public bool Connected {
			get {
				throw new NotImplementedException();
				//return false;
			}
		}

		public IntPtr Handle {
			get {
				return(socket);
			}
		}

		// Returns the local endpoint details in addr and port
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static SocketAddress LocalEndPoint_internal(IntPtr socket);

		[MonoTODO("Support non-IP endpoints")]
		public EndPoint LocalEndPoint {
			get {
				SocketAddress sa;
				
				sa=LocalEndPoint_internal(socket);

				if(sa.Family==AddressFamily.InterNetwork) {
					// Stupidly, EndPoint.Create() is an
					// instance method
					return new IPEndPoint(0, 0).Create(sa);
				} else {
					throw new NotImplementedException();
				}
			}
		}

		public ProtocolType ProtocolType {
			get {
				return(protocol_type);
			}
		}

		// Returns the remote endpoint details in addr and port
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static SocketAddress RemoteEndPoint_internal(IntPtr socket);

		[MonoTODO("Support non-IP endpoints")]
		public EndPoint RemoteEndPoint {
			get {
				SocketAddress sa;
				
				sa=RemoteEndPoint_internal(socket);

				if(sa.Family==AddressFamily.InterNetwork) {
					// Stupidly, EndPoint.Create() is an
					// instance method
					return new IPEndPoint(0, 0).Create(sa);
				} else {
					throw new NotImplementedException();
				}
			}
		}

		public SocketType SocketType {
			get {
				return(socket_type);
			}
		}

		// Creates a new system socket, returning the handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static IntPtr Accept_internal(IntPtr sock);
		
		public Socket Accept() {
			IntPtr sock=Accept_internal(socket);
			
			return(new Socket(this.AddressFamily, this.SocketType,
					  this.ProtocolType, sock));
		}

		public IAsyncResult BeginAccept(AsyncCallback callback,
						object state) {
			SocketAsyncResult req=new SocketAsyncResult(state);
			Worker worker=new Worker(this, callback, req);
			Thread child=new Thread(new ThreadStart(worker.Accept));
			child.Start();
			return(req);
		}

		public IAsyncResult BeginConnect(EndPoint end_point,
						 AsyncCallback callback,
						 object state) {
			SocketAsyncResult req=new SocketAsyncResult(state);
			Worker worker=new Worker(this, end_point, callback,
						 req);
			Thread child=new Thread(new ThreadStart(worker.Connect));
			child.Start();
			return(req);
		}

		public IAsyncResult BeginReceive(byte[] buffer, int offset,
						 int size,
						 SocketFlags socket_flags,
						 AsyncCallback callback,
						 object state) {
			SocketAsyncResult req=new SocketAsyncResult(state);
			Worker worker=new Worker(this, buffer, offset, size,
						 socket_flags, callback, req);
			Thread child=new Thread(new ThreadStart(worker.Receive));
			child.Start();
			return(req);
		}

		public IAsyncResult BeginReceiveFrom(byte[] buffer, int offset,
						     int size,
						     SocketFlags socket_flags,
						     ref EndPoint remote_end,
						     AsyncCallback callback,
						     object state) {
			SocketAsyncResult req=new SocketAsyncResult(state);
			Worker worker=new Worker(this, buffer, offset, size,
						 socket_flags, remote_end,
						 callback, req);
			Thread child=new Thread(new ThreadStart(worker.ReceiveFrom));
			child.Start();
			return(req);
		}

		public IAsyncResult BeginSend(byte[] buffer, int offset,
					      int size,
					      SocketFlags socket_flags,
					      AsyncCallback callback,
					      object state) {
			SocketAsyncResult req=new SocketAsyncResult(state);
			Worker worker=new Worker(this, buffer, offset, size,
						 socket_flags, callback, req);
			Thread child=new Thread(new ThreadStart(worker.Send));
			child.Start();
			return(req);
		}

		public IAsyncResult BeginSendTo(byte[] buffer, int offset,
						int size,
						SocketFlags socket_flags,
						EndPoint remote_end,
						AsyncCallback callback,
						object state) {
			SocketAsyncResult req=new SocketAsyncResult(state);
			Worker worker=new Worker(this, buffer, offset, size,
						 socket_flags, remote_end,
						 callback, req);
			Thread child=new Thread(new ThreadStart(worker.SendTo));
			child.Start();
			return(req);
		}

		// Creates a new system socket, returning the handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Bind_internal(IntPtr sock,
							 SocketAddress sa);

		public void Bind(EndPoint local_end) {
			if(local_end==null) {
				throw new ArgumentNullException();
			}
			
			Bind_internal(socket, local_end.Serialize());
		}

		// Closes the socket
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Close_internal(IntPtr socket);
		
		public void Close() {
			Close_internal(socket);
		}

		// Connects to the remote address
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Connect_internal(IntPtr sock,
							    SocketAddress sa);

		public void Connect(EndPoint remote_end) {
			if(remote_end==null) {
				throw new ArgumentNullException();
			}
			
			Connect_internal(socket, remote_end.Serialize());
		}
		
		public Socket EndAccept(IAsyncResult result) {
			SocketAsyncResult req=(SocketAsyncResult)result;

			result.AsyncWaitHandle.WaitOne();
			return(req.Worker.Socket);
		}

		public void EndConnect(IAsyncResult result) {
			SocketAsyncResult req=(SocketAsyncResult)result;

			result.AsyncWaitHandle.WaitOne();
		}

		public int EndReceive(IAsyncResult result) {
			SocketAsyncResult req=(SocketAsyncResult)result;

			result.AsyncWaitHandle.WaitOne();
			return(req.Worker.Total);
		}

		public int EndReceiveFrom(IAsyncResult result,
					  ref EndPoint end_point) {
			SocketAsyncResult req=(SocketAsyncResult)result;

			result.AsyncWaitHandle.WaitOne();
			end_point=req.Worker.EndPoint;
			return(req.Worker.Total);
		}

		public int EndSend(IAsyncResult result) {
			SocketAsyncResult req=(SocketAsyncResult)result;

			result.AsyncWaitHandle.WaitOne();
			return(req.Worker.Total);
		}

		public int EndSendTo(IAsyncResult result) {
			SocketAsyncResult req=(SocketAsyncResult)result;

			result.AsyncWaitHandle.WaitOne();
			return(req.Worker.Total);
		}

		[MonoTODO]
		public object GetSocketOption(SocketOptionLevel level,
					      SocketOptionName name) {
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void GetSocketOption(SocketOptionLevel level,
					    SocketOptionName name,
					    byte[] opt_value) {
			throw new NotImplementedException();
		}

		[MonoTODO]
		public byte[] GetSocketOption(SocketOptionLevel level,
					      SocketOptionName name,
					      int length) {
			throw new NotImplementedException();
		}

		[MonoTODO]
		public int IOControl(int ioctl_code, byte[] in_value,
				     byte[] out_value) {
			throw new NotImplementedException();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Listen_internal(IntPtr sock,
							   int backlog);

		public void Listen(int backlog) {
			Listen_internal(socket, backlog);
		}

		[MonoTODO]
		public bool Poll(int time_us, SelectMode mode) {
			throw new NotImplementedException();
		}
		
		public int Receive(byte[] buf) {
			return(Receive(buf, 0, buf.Length, SocketFlags.None));
		}

		public int Receive(byte[] buf, SocketFlags flags) {
			return(Receive(buf, 0, buf.Length, flags));
		}

		public int Receive(byte[] buf, int size, SocketFlags flags) {
			return(Receive(buf, 0, size, flags));
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int Receive_internal(IntPtr sock,
							   byte[] buffer,
							   int offset,
							   int count,
							   SocketFlags flags);

		public int Receive(byte[] buf, int offset, int size,
				   SocketFlags flags) {
			if(buf==null) {
				throw new ArgumentNullException();
			}
			if(offset+size>buf.Length) {
				throw new ArgumentException();
			}
			
			return(Receive_internal(socket, buf, offset, size,
						flags));
		}
		
		public int ReceiveFrom(byte[] buf, ref EndPoint remote_end) {
			return(ReceiveFrom(buf, 0, buf.Length,
					   SocketFlags.None, ref remote_end));
		}

		public int ReceiveFrom(byte[] buf, SocketFlags flags,
				       ref EndPoint remote_end) {
			return(ReceiveFrom(buf, 0, buf.Length, flags,
					   ref remote_end));
		}

		public int ReceiveFrom(byte[] buf, int size, SocketFlags flags,
				       ref EndPoint remote_end) {
			return(ReceiveFrom(buf, 0, size, flags,
					   ref remote_end));
		}


		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int RecvFrom_internal(IntPtr sock,
							    byte[] buffer,
							    int offset,
							    int count,
							    SocketFlags flags,
							    ref SocketAddress sockaddr);

		public int ReceiveFrom(byte[] buf, int offset, int size,
				       SocketFlags flags,
				       ref EndPoint remote_end) {
			if(buf==null || remote_end==null) {
				throw new ArgumentNullException();
			}
			if(offset+size>buf.Length) {
				throw new ArgumentException();
			}

			SocketAddress sockaddr=remote_end.Serialize();
			int count;

			count=RecvFrom_internal(socket, buf, offset, size,
						flags, ref sockaddr);
			
			// Stupidly, EndPoint.Create() is an
			// instance method
			remote_end=remote_end.Create(sockaddr);

			return(count);
		}

		public int Send(byte[] buf) {
			return(Send(buf, 0, buf.Length, SocketFlags.None));
		}

		public int Send(byte[] buf, SocketFlags flags) {
			return(Send(buf, 0, buf.Length, flags));
		}

		public int Send(byte[] buf, int size, SocketFlags flags) {
			return(Send(buf, 0, size, flags));
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int Send_internal(IntPtr sock,
							byte[] buf, int offset,
							int count,
							SocketFlags flags);

		public int Send(byte[] buf, int offset, int size,
				SocketFlags flags) {
			if(buf==null) {
				throw new ArgumentNullException();
			}
			if(offset+size>buf.Length) {
				throw new ArgumentException();
			}
			
			return(Send_internal(socket, buf, offset, size,
					     flags));
		}

		public int SendTo(byte[] buffer, EndPoint remote_end) {
			return(SendTo(buffer, 0, buffer.Length,
				      SocketFlags.None, remote_end));
		}

		public int SendTo(byte[] buffer, SocketFlags flags,
				  EndPoint remote_end) {
			return(SendTo(buffer, 0, buffer.Length, flags,
				      remote_end));
		}

		public int SendTo(byte[] buffer, int size, SocketFlags flags,
				  EndPoint remote_end) {
			return(SendTo(buffer, size, buffer.Length, flags,
				      remote_end));
		}


		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int SendTo_internal(IntPtr sock,
							  byte[] buffer,
							  int offset,
							  int count,
							  SocketFlags flags,
							  SocketAddress sa);

		public int SendTo(byte[] buffer, int offset, int size,
				  SocketFlags flags, EndPoint remote_end) {
			if(buffer==null || remote_end==null) {
				throw new ArgumentNullException();
			}
			if(offset+size>buffer.Length) {
				throw new ArgumentException();
			}

			SocketAddress sockaddr=remote_end.Serialize();

			return(SendTo_internal(socket, buffer, offset, size,
					       flags, sockaddr));
		}

		[MonoTODO]
		public void SetSocketOption(SocketOptionLevel level,
					    SocketOptionName name,
					    byte[] opt_value) {
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void SetSocketOption(SocketOptionLevel level,
					    SocketOptionName name,
					    int opt_value) {
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void SetSocketOption(SocketOptionLevel level,
					    SocketOptionName name,
					    object opt_value) {
			if(opt_value==null) {
				throw new ArgumentNullException();
			}
			
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void Shutdown(SocketShutdown how) {
			throw new NotImplementedException();
		}

		private bool disposed = false;
		
		protected virtual void Dispose(bool explicitDisposing) {
			// Check to see if Dispose has already been called
			if(!this.disposed) {
				// If this is a call to Dispose,
				// dispose all managed resources.
				if(explicitDisposing) {
					// Free up stuff here
				}

				// Release unmanaged resources
				this.disposed=true;
				this.Close();
			}
		}

		public void Dispose() {
			Dispose(true);
			// Take yourself off the Finalization queue
			GC.SuppressFinalize(this);
		}

		~Socket () {
			Dispose(false);
		}
	}
}
