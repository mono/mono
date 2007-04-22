using System;
using System.Net;

namespace System.Net.Sockets
{
	/// <summary>
	/// Summary description for GHSocketFactory.
	/// </summary>
	public class GHSocketFactory
	{
		internal static GHSocket Socket_internal(AddressFamily family,
												 SocketType type,
												 ProtocolType proto,
												 out int error)
		{
			if ( family == AddressFamily.InterNetwork &&
				//(family == AddressFamily.InterNetwork || family == AddressFamily.InterNetworkV6) &&
				(type == SocketType.Stream || type == SocketType.Unknown) &&
				(proto == ProtocolType.Tcp || proto == ProtocolType.Unknown || proto == ProtocolType.Unspecified) )
			{
				error = 0;
				return new GHStreamSocket();
			}

			error = 10044; //WSAESOCKTNOSUPPORT (Socket type not supported)
			return null;
		}

		internal static void Select_internal (ref Socket [] sockets, int microSeconds, out int error)
		{
			error = 0;

			java.nio.channels.Selector selector = java.nio.channels.Selector.open();

			int mode = 0;
			int count = sockets.Length;
			for (int i = 0; i < count; i++) 
			{
				if (sockets [i] == null) 
				{ // separator
					mode++;
					continue;
				}

				GHSocket sock = sockets [i].GHHandle;
				if (sock == null)
				{
					throw new ArgumentNullException ("GHSocket handle is null");
				}

				sock.RegisterSelector(selector, mode, sockets [i], out error);
				if (error != 0)
				{
					error = 0;
					sockets = null;
					CloseSelector(selector);
					return;
				}
			}
			
			sockets = null;

			long timeOutMillis = 1;
			if (microSeconds < 0)
			{
				timeOutMillis = 0;
			} 
			else if (microSeconds > 999)
			{
				timeOutMillis = (long)(microSeconds / 1000);
			}

			int readyCount = 0;
			try
			{
				readyCount = selector.select(timeOutMillis);
			}
			catch (Exception e)
			{
				error = 10022; //WSAEINVAL (Invalid argument)
#if DEBUG
				Console.WriteLine("Caught exception during Select_internal selector.select - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
			}

			if (readyCount > 0)
			{
				try
				{
					sockets = new Socket[readyCount+2];
					Socket[] writeList = new Socket[readyCount];
					Socket[] errorList = new Socket[readyCount];

					int readListCount = 0;
					int writeListCount = 0;
					int errorListCount = 0;

					java.util.Set readyKeys = selector.selectedKeys();
					java.util.Iterator it = readyKeys.iterator();

					while (it.hasNext()) 
					{
						java.nio.channels.SelectionKey key = (java.nio.channels.SelectionKey)it.next();
                
						if (key.isAcceptable() || key.isReadable()) 
						{
							sockets[readListCount] = (Socket)key.attachment();
							readListCount++;
						}
						if (key.isWritable()) 
						{
							writeList[writeListCount] = (Socket)key.attachment();
							writeListCount++;
						}
						if (key.isConnectable()) 
						{
							Socket source = (Socket)key.attachment();
							if (source.GHHandle.CheckConnectionFinished())
							{
								writeList[writeListCount] = source;
								writeListCount++;
							}
							else
							{
								errorList[errorListCount] = source;
								errorListCount++;
							}
						}
					}

					sockets[readListCount] = null;
					readListCount++;
					for (int i = 0; i < writeListCount; i++, readListCount++)
					{
						sockets[readListCount] = writeList[i];
					}
					sockets[readListCount] = null;
					readListCount++;
					for (int i = 0; i < errorListCount; i++, readListCount++)
					{
						sockets[readListCount] = errorList[i];
					}
				}
				catch (Exception e)
				{
					error = 10022; //WSAEINVAL (Invalid argument)
#if DEBUG
					Console.WriteLine("Caught exception during Select_internal iterate selected keys - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
				}
			}

			CloseSelector(selector);
		}

		internal static void CloseSelector (java.nio.channels.Selector selector)
		{
			java.util.Set keys = selector.keys();
			java.util.Iterator it = keys.iterator();

			try
			{
				selector.close();
			}
			catch (Exception e)
			{
#if DEBUG
				Console.WriteLine("Caught exception during CloseSelector selector.close - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
			}

			while (it.hasNext()) 
			{
				java.nio.channels.SelectionKey key = (java.nio.channels.SelectionKey)it.next();
				Socket source = (Socket)key.attachment();
				key.cancel ();
				try
				{
					if (source.Blocking)
					{
						/*
							A channel must be placed into non-blocking mode before being registered 
							with a selector, and may not be returned to blocking mode until it has been 
							deregistered. So, I need set the channel back to the blocking mode, if it was
							in blocking mode before select operation
						*/
						source.Blocking = true;
					}
				}
				catch (Exception be)
				{
#if DEBUG
					Console.WriteLine("Caught exception during CloseSelector source.Blocking - {0}: {1}\n{2}", be.GetType(), be.Message, be.StackTrace);
#endif
				}
			}

			try
			{
				selector.close();
			}
			catch (Exception e)
			{
#if DEBUG
				Console.WriteLine("Caught exception during CloseSelector selector.close - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
			}
		}
	}
}
