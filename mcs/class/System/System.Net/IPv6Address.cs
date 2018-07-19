//
// System.Net.IPv6AddressFormatter.cs
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
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

using System.Globalization;
using System.Text;

namespace System.Net {

	struct IPv6AddressFormatter
	{
		ushort [] address;
		long scopeId;

		public IPv6AddressFormatter (ushort[] addr, long scopeId)
		{
			this.address = addr;
			this.scopeId = scopeId;
		}

		static ushort SwapUShort (ushort number)
		{
			return (ushort) ( ((number >> 8) & 0xFF) + ((number << 8) & 0xFF00) );
		}

		// Convert the address into a format expected by the IPAddress (long) ctor
		// This needs to be unsigned to satisfy the '> 1' test in IsIPv4Compatible()
		uint AsIPv4Int ()
		{
			return (uint)(SwapUShort (address [7]) << 16) + SwapUShort (address [6]);
		}			

		bool IsIPv4Compatible ()
		{
			for (int i = 0; i < 6; i++) 
				if (address [i] != 0)
					return false;
			/* MS .net only seems to format the last 4
			 * bytes as an IPv4 address if address[6] is
			 * non-zero
			 */
			if (address[6] == 0)
				return false;
			return (AsIPv4Int () > 1);
		}
		
		bool IsIPv4Mapped ()
		{
			for (int i = 0; i < 5; i++) 
				if (address [i] != 0)
					return false;
			/* MS .net only seems to format the last 4
			 * bytes as an IPv4 address if address[6] is
			 * non-zero
			 */
			if (address[6] == 0)
				return false;
			
			return address [5] == 0xffff;
		}
		
		public override string ToString ()
		{
			StringBuilder s = new StringBuilder ();


			if(IsIPv4Compatible() || IsIPv4Mapped())
			{
				s.Append("::");

				if(IsIPv4Mapped())
					s.Append("ffff:");

				s.Append(new IPAddress( AsIPv4Int ()).ToString ());

				return s.ToString ();
			}
			
			int bestChStart = -1; // Best chain start
			int bestChLen = 0; // Best chain length
			int currChLen = 0; // Current chain length

			// Looks for the longest zero chain
			for (int i=0; i<8; i++)
			{
				if (address[i] != 0)
				{
					if ((currChLen > bestChLen) 
						&& (currChLen > 1))
					{
						bestChLen = currChLen;
						bestChStart = i - currChLen;
					}
					currChLen = 0;
				}
				else
					currChLen++;
			}
			if ((currChLen > bestChLen) 
				&& (currChLen > 1))
			{
				bestChLen = currChLen;
				bestChStart = 8 - currChLen;
			}

			// makes the string
			if (bestChStart == 0)
				s.Append(":");
			for (int i=0; i<8; i++)
			{
				if (i == bestChStart)
				{
					s.Append (":");
					i += (bestChLen - 1);
					continue;
				}
				s.AppendFormat("{0:x}", address [i]);
				if (i < 7) s.Append (':');
			}
			
			if (scopeId != 0)
				s.Append ('%').Append (scopeId);
			return s.ToString ();
		}
	}
}
