#if NET_2_0 && (!NET_2_1 || MONOTOUCH)

namespace System.Net.Sockets
{
	using System;	
	using System.IO;
	using System.Text;
	
	static class SocketPolicyClient
	{
		const string policy_request = "<policy-file-request/>\0";
		
		static int session=0;
		static void Log(string msg)
		{
			Console.WriteLine("SocketPolicyClient"+session+": "+msg);
		}

		//Called trough reflection by CrossDomainPolicyParser.dll
		static Stream GetPolicyStreamForIP(string ip, int policyport, int timeout)
		{
			session++;
			Log("Incoming GetPolicyStreamForIP");
			IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ip), policyport);
			
			Socket sender = new Socket(AddressFamily.InterNetwork, 
			SocketType.Stream, ProtocolType.Tcp );
			
			byte[] bytes = new byte[5000];
			
			var ms = new MemoryStream();
			int bytesRec;
			try
			{
				Log("About to BeginConnect to "+remoteEP);
		                var async = sender.BeginConnect(remoteEP, null, null, false);
				Log("About to WaitOne");
				var start = DateTime.Now;
                		if (!async.AsyncWaitHandle.WaitOne(timeout))
                		{
					Log("WaitOne timed out. Duration: "+(DateTime.Now-start).TotalMilliseconds);
                    			sender.Close();
                    			throw new Exception("BeginConnect timed out");
                		}
                		sender.EndConnect(async);

	            		Log("Socket connected");
	
	            // Encode the data string into a byte array.
	            byte[] msg = Encoding.ASCII.GetBytes(policy_request);
	
				SocketError error;
				
	            // Send the data through the socket.
	            sender.Send_nochecks(msg, 0, msg.Length, SocketFlags.None, out error);
				if (SocketError.Success != error)
				{
					Log("Socket error: "+error);
					return ms;
				}
	
	            // Receive the response from the remote device.
	            bytesRec = sender.Receive_nochecks(bytes, 0, bytes.Length, SocketFlags.None, out error);
				if (SocketError.Success != error)
				{
					Log("Socket error: "+ error);
					return ms;
				}
				
	
	            // Release the socket.
	            try
				{
					sender.Shutdown(SocketShutdown.Both);
					sender.Close();
				}
				catch (SocketException)
				{
					//sometimes the connection closes before we can close it, that's fine.
				}
				ms = new MemoryStream(bytes,0,bytesRec);
			}
			catch (Exception e)
			{
				Log("Caught exception: "+e.Message);
				//something went wrong, we'll just return an empty stream
				return ms;
			}
			
			ms.Seek(0, SeekOrigin.Begin);
			
			return ms;
		}
	}
}
#endif
