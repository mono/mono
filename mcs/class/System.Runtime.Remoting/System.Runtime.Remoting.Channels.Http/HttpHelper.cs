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

		public static void CopyStream (Stream inStream, Stream outStream)
		{
			byte[] buffer = new byte [1024];

			int nr;
			while ((nr = inStream.Read (buffer, 0, buffer.Length)) > 0)
				outStream.Write (buffer, 0, nr);

			outStream.Flush ();

			if (outStream.CanSeek)
				outStream.Seek (0,SeekOrigin.Begin);
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
