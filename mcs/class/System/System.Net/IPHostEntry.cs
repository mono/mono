// System.Net.IPHostEntry.cs
//
// Author: Mads Pultz (mpultz@diku.dk)
//
// (C) Mads Pultz, 2001

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

namespace System.Net {
	
	public class IPHostEntry {
		private IPAddress[] addressList;
		private String[] aliases;
		private String hostName;
		
		public IPHostEntry() {
		}
		
		public IPAddress[] AddressList {
			get { return addressList; }
			set { addressList = value; }
		}
		
		public string[] Aliases	{
			get { return aliases; }
			set { aliases = value; }
		}
		
		public string HostName {
			get { return hostName; }
			set { hostName = value; }
		}
		
/* According to the .NET Framework SDK Documentation (beta 2) the following
   methods from Object are not overrided. I implemented them before realizing
   this but I leave the implementation here if needed in the future.
   
		public override string ToString() {
			string res = hostName;
			if (addressList != null && addressList.Length > 0)
				res += " [" + addressList[0] + "]";
			return res;
		}
		
		public override bool Equals(object obj) {
			if (obj is IPHostEntry) {
				IPHostEntry h = (IPHostEntry)obj;
				return hostName.Equals(h.HostName) && aliases.Equals(h.Aliases) &&
					addressList.Equals(h.AddressList);
			}
			else
			  return false;
		}
		
		public override int GetHashCode() {
			return hostName.GetHashCode();
		}
		
		protected new object MemberwiseClone() {
			IPHostEntry res = new IPHostEntry();
			res.AddressList = new IPAddress[addressList.Length];
			Array.Copy(addressList, res.AddressList, addressList.Length);
			res.Aliases = new String[aliases.Length];
			Array.Copy(aliases, res.Aliases, aliases.Length);
			res.HostName = hostName;
			return res;
		}
*/
	}
}

