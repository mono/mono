/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
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
//
// Novell.Directory.Ldap.Rfc2251.RfcMessageID.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Rfc2251
{
	
	/// <summary> Represents an Ldap Message ID.
	/// 
	/// <pre>
	/// MessageID ::= INTEGER (0 .. maxInt)
	/// 
	/// maxInt INTEGER ::= 2147483647 -- (2^^31 - 1) --
	/// 
	/// Note: The creation of a MessageID should be hidden within the creation of
	/// an RfcLdapMessage. The MessageID needs to be in sequence, and has an
	/// upper and lower limit. There is never a case when a user should be
	/// able to specify the MessageID for an RfcLdapMessage. The MessageID()
	/// class should be package protected. (So the MessageID value isn't
	/// arbitrarily run up.)
	/// </pre>
	/// </summary>
	class RfcMessageID:Asn1Integer
	{
		/// <summary> Increments the message number atomically
		/// 
		/// </summary>
		/// <returns> the new message number
		/// </returns>
		private static int MessageID
		{
			get
			{
				lock (lock_Renamed)
				{
					return (messageID < System.Int32.MaxValue)?++messageID:(messageID = 1);
				}
			}
			
		}
		
		private static int messageID = 0;
		private static System.Object lock_Renamed;
		
		/// <summary> Creates a MessageID with an auto incremented Asn1Integer value.
		/// 
		/// Bounds: (0 .. 2,147,483,647) (2^^31 - 1 or Integer.MAX_VALUE)
		/// 
		/// MessageID zero is never used in this implementation.  Always
		/// start the messages with one.
		/// </summary>
		protected internal RfcMessageID():base(MessageID)
		{
		}
		
		/// <summary> Creates a MessageID with a specified int value.</summary>
		protected internal RfcMessageID(int i):base(i)
		{
		}
		static RfcMessageID()
		{
			lock_Renamed = new System.Object();
		}
	}
}
