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
// Novell.Directory.Ldap.LdapResponseQueue.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap
{
	
	/// <summary>  A mechanism for processing asynchronous messages received from a server.
	/// It represents the message queue associated with a particular asynchronous
	/// Ldap operation or operations.
	/// </summary>
	public class LdapResponseQueue:LdapMessageQueue
	{
		/// <summary> Constructs a response queue using the specified message agent
		/// 
		/// </summary>
		/// <param name="agent">The message agent to associate with this queue
		/// </param>
		/* package */
		internal LdapResponseQueue(MessageAgent agent):base("LdapResponseQueue", agent)
		{
			return ;
		}
		
		/// <summary> Merges two message queues.  It appends the current and
		/// future contents from another queue to this one.
		/// 
		/// After the operation, queue2.getMessageIDs()
		/// returns an empty array, and its outstanding responses
		/// have been removed and appended to this queue.
		/// 
		/// </summary>
		/// <param name="queue2">   The queue that is merged from.  Following
		/// the merge, this queue object will no
		/// longer receive any data, and calls made
		/// to its methods will fail with a RuntimeException.
		/// The queue can be reactivated by using it in an 
		/// Ldap request, after which it will receive responses
		/// for that request..
		/// </param>
		public virtual void  merge(LdapMessageQueue queue2)
		{
			LdapResponseQueue q = (LdapResponseQueue) queue2;
			agent.merge(q.MessageAgent);
			
			return ;
		}
	}
}
