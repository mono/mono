//==========================================================================
//  File:       HttpHelper.cs
//
//  Summary:    Implements Helper internal class for transmission over HTTP.
//
//  Classes:    internal  HttpHelper
//              
//	By :
//		Ahmad    Tantawy	popsito82@hotmail.com
//		Ahmad	 Kadry		kadrianoz@hotmail.com
//		Hussein  Mehanna	hussein_mehanna@hotmail.com
//
//==========================================================================


using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace System.Runtime.Remoting.Channels.Http
{
	internal class HttpHelper
	{
		public static string Parse(string URL , out string ObjectURI)
		{

			int Pos;
			ObjectURI = null;
			string ChannelURI = null;
			

			if(StartsWithHttp(URL))
			{
				Pos = URL.IndexOf("/",7);
				if(Pos >= 0) 
				{
					ObjectURI = URL.Substring(Pos);
					ChannelURI = URL.Substring(0, Pos);
				}
				else ChannelURI = URL;
			}
			return ChannelURI;
			
		}

		public static bool StartsWithHttp(string URL)
		{
			if(URL.StartsWith("http://"))
				return true;
			else
				return false;
		}

		public static Stream CopyStream(Stream inStream)
		{
			
			Stream outStream = new MemoryStream();
			
			int  temp;
			
			try
			{
				while(true)
				{
					temp = inStream.ReadByte();
					if(temp==-1)
						break;
					outStream.WriteByte((byte)temp);
				}
				outStream.Flush();
				outStream.Seek(0,SeekOrigin.Begin);
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
			
			return outStream;
		}

		public static bool CopyStream(Stream inStream, Stream outStream)
		{
			int  temp;

			try
			{
				while(true)
				{
					temp = inStream.ReadByte();
										
					if(temp==-1)
						break;
					outStream.WriteByte((byte)temp);
				}
				
				outStream.Flush();
				
				if(outStream.CanSeek)
					outStream.Seek(0,SeekOrigin.Begin);
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
				return false;
			}
			return true;
			
		}

		public static String GetHostName()
		{
				string hostName = Dns.GetHostName();

				if (hostName == null)
				{
					throw new ArgumentNullException("hostName");
				}

			return hostName;
		} // GetHostName

		public static String GetMachineName()
		{
			
			String machineName = GetHostName();
			if (machineName != null)
				machineName= Dns.GetHostByName(machineName).HostName;
				
			if (machineName== null)
			{
				throw new ArgumentNullException("machine");
			}
			            
			return machineName;      
		} 
	}
}
