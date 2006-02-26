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
// Novell.Directory.Ldap.MessageAgent.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap.Utilclass;

namespace Novell.Directory.Ldap
{
	
	/* package */
	class MessageAgent
	{
		private void  InitBlock()
		{
			messages = new MessageVector(5, 5);
		}
		/// <summary> empty and return all messages owned by this agent
		/// 
		/// 
		/// </summary>
		virtual internal System.Object[] MessageArray
		{
			/* package */
			
			get
			{
				return messages.ObjectArray;
			}
			
		}
		/// <summary> Get a list of message ids controlled by this agent
		/// 
		/// </summary>
		/// <returns> an array of integers representing the message ids
		/// </returns>
		virtual internal int[] MessageIDs
		{
			/* package */
			
			get
			{
				int size = messages.Count;
				int[] ids = new int[size];
				Message info;
				
				for (int i = 0; i < size; i++)
				{
					info = (Message) messages[i];
					ids[i] = info.MessageID;
				}
				return ids;
			}
			
		}
		/// <summary> Get the maessage agent number for debugging
		/// 
		/// </summary>
		/// <returns> the agent number
		/// </returns>
		virtual internal System.String AgentName
		{
			/*packge*/
			
			get
			{
				return name;
			}
			
		}
		/// <summary> Get a count of all messages queued</summary>
		virtual internal int Count
		{
			/* package */
			
			get
			{
				int count = 0;
				for (int i = 0; i < messages.Count; i++)
				{
					Message m = (Message) messages[i];
					count += m.Count;
				}
				return count;
			}
			
		}
		private MessageVector messages;
		private int indexLastRead = 0;
		private static System.Object nameLock; // protect agentNum
		private static int agentNum = 0; // Debug, agent number
		private System.String name; // String name for debug
		
		/* package */
		internal MessageAgent()
		{
			InitBlock();
			// Get a unique agent id for debug
		}
		
		/// <summary> merges two message agents
		/// 
		/// </summary>
		/// <param name="fromAgent">the agent to be merged into this one
		/// </param>
		/* package */
		internal void  merge(MessageAgent fromAgent)
		{
			System.Object[] msgs = fromAgent.MessageArray;
			for (int i = 0; i < msgs.Length; i++)
			{
				messages.Add(msgs[i]);
				((Message) (msgs[i])).Agent = this;
			}
			lock (messages.SyncRoot)
			{
				if (msgs.Length > 1)
				{
					System.Threading.Monitor.PulseAll(messages.SyncRoot); // wake all threads waiting for messages
				}
				else if (msgs.Length == 1)
				{
					System.Threading.Monitor.Pulse(messages.SyncRoot); // only wake one thread
				}
			}
			return ;
		}
		
		
		/// <summary> Wakes up any threads waiting for messages in the message agent
		/// 
		/// </summary>
		/* package */
		internal void  sleepersAwake(bool all)
		{
			lock (messages.SyncRoot)
			{
				if (all)
					System.Threading.Monitor.PulseAll(messages.SyncRoot);
				else
					System.Threading.Monitor.Pulse(messages.SyncRoot);
			}
			return ;
		}
		
		/// <summary> Returns true if any responses are queued for any of the agent's messages
		/// 
		/// return false if no responses are queued, otherwise true
		/// </summary>
		/* package */
		internal bool isResponseReceived()
		{
			int size = messages.Count;
			int next = indexLastRead + 1;
			Message info;
			for (int i = 0; i < size; i++)
			{
				if (next == size)
				{
					next = 0;
				}
				info = (Message) messages[next];
				if (info.hasReplies())
				{
					return true;
				}
			}
			return false;
		}
		
		/// <summary> Returns true if any responses are queued for the specified msgId
		/// 
		/// return false if no responses are queued, otherwise true
		/// </summary>
		/* package */
		internal bool isResponseReceived(int msgId)
		{
			try
			{
				Message info = messages.findMessageById(msgId);
				return info.hasReplies();
			}
			catch (System.FieldAccessException ex)
			{
				return false;
			}
		}
		
		/// <summary> Abandon the request associated with MsgId
		/// 
		/// </summary>
		/// <param name="msgId">the message id to abandon
		/// 
		/// </param>
		/// <param name="cons">constraints associated with this request
		/// </param>
		/* package */
		internal void  Abandon(int msgId, LdapConstraints cons)
		//, boolean informUser)
		{
			Message info = null;
			try
			{
				// Send abandon request and remove from connection list
				info = messages.findMessageById(msgId);
				SupportClass.VectorRemoveElement(messages, info); // This message is now dead
				info.Abandon(cons, null);
				
				return ;
			}
			catch (System.FieldAccessException ex)
			{
			}
			return ;
		}
		
		/// <summary> Abandon all requests on this MessageAgent</summary>
		/* package */
		internal void  AbandonAll()
		{
			int size = messages.Count;
			Message info;
			
			for (int i = 0; i < size; i++)
			{
				info = (Message) messages[i];
				// Message complete and no more replies, remove from id list
				SupportClass.VectorRemoveElement(messages, info);
				info.Abandon(null, null);
			}
			return ;
		}
		
		/// <summary> Indicates whether a specific operation is complete
		/// 
		/// </summary>
		/// <returns> true if a specific operation is complete
		/// </returns>
		/* package */
		internal bool isComplete(int msgid)
		{
			try
			{
				Message info = messages.findMessageById(msgid);
				if (!info.Complete)
				{
					return false;
				}
			}
			catch (System.FieldAccessException ex)
			{
				; // return true, if no message, it must be complete
			}
			return true;
		}
		
		/// <summary> Returns the Message object for a given messageID
		/// 
		/// </summary>
		/// <param name="msgid">the message ID.
		/// </param>
		/* package */
		internal Message getMessage(int msgid)
		{
			return messages.findMessageById(msgid);
		}
		
		/// <summary> Send a request to the server.  A Message class is created
		/// for the specified request which causes the message to be sent.
		/// The request is added to the list of messages being managed by
		/// this agent.
		/// 
		/// </summary>
		/// <param name="conn">the connection that identifies the server.
		/// 
		/// </param>
		/// <param name="msg">the LdapMessage to send
		/// 
		/// </param>
		/// <param name="timeOut">the interval to wait for the message to complete or
		/// <code>null</code> if infinite.
		/// </param>
		/// <param name="queue">the LdapMessageQueue associated with this request.
		/// </param>
		/* package */
		internal void  sendMessage(Connection conn, LdapMessage msg, int timeOut, LdapMessageQueue queue, BindProperties bindProps)
		{
			// creating a messageInfo causes the message to be sent
			// and a timer to be started if needed.
			Message message = new Message(msg, timeOut, conn, this, queue, bindProps);
			messages.Add(message);
			message.sendMessage(); // Now send message to server
			return ;
		}
		
		/// <summary> Returns a response queued, or waits if none queued
		/// 
		/// </summary>
		/* package */
//		internal System.Object getLdapMessage(System.Int32 msgId)
		internal System.Object getLdapMessage(System.Int32 msgId)
		{
			return (getLdapMessage(new Integer32(msgId)));
		}

		internal System.Object getLdapMessage(Integer32 msgId)
		{
			System.Object rfcMsg;
			// If no messages for this agent, just return null
			if (messages.Count == 0)
			{
				return null;
			}
			if ( msgId != null)
			{
				// Request messages for a specific ID
				try
				{
					// Get message for this ID
//					Message info = messages.findMessageById(msgId);
					Message info = messages.findMessageById(msgId.intValue);
					rfcMsg = info.waitForReply(); // blocks for a response
					if (!info.acceptsReplies() && !info.hasReplies())
					{
						// Message complete and no more replies, remove from id list
						SupportClass.VectorRemoveElement(messages, info);
						info.Abandon(null, null); // Get rid of resources
					}
					else
					{
					}
					return rfcMsg;
				}
				catch (System.FieldAccessException ex)
				{
					// no such message id
					return null;
				}
			}
			else
			{
				// A msgId was NOT specified, any message will do
				lock (messages.SyncRoot)
				{
					while (true)
					{
						int next = indexLastRead + 1;
						Message info;
						for (int i = 0; i < messages.Count; i++)
						{
							if (next >= messages.Count)
							{
								next = 0;
							}
							info = (Message) messages[next];
							indexLastRead = next++;
							rfcMsg = info.Reply;
							// Check this request is complete
							if (!info.acceptsReplies() && !info.hasReplies())
							{
								// Message complete & no more replies, remove from id list
								SupportClass.VectorRemoveElement(messages, info); // remove from list
								info.Abandon(null, null); // Get rid of resources
								// Start loop at next message that is now moved
								// to the current position in the Vector.
								i -= 1;
							}
							if (rfcMsg != null)
							{
								// We got a reply
								return rfcMsg;
							}
							else
							{
								// We found no reply here
							}
						} // end for loop */
						// Messages can be removed in this loop, we we must
						// check if any messages left for this agent
						if (messages.Count == 0)
						{
							return null;
						}
						
						// No data, wait for something to come in.
						try
						{
							System.Threading.Monitor.Wait(messages.SyncRoot);
						}
						catch (System.Threading.ThreadInterruptedException ex)
						{
						}
					} /* end while */
				} /* end synchronized */
			}
		}
		
		/// <summary> Debug code to print messages in message vector</summary>
		private void  debugDisplayMessages()
		{
			return ;
		}
		static MessageAgent()
		{
			nameLock = new System.Object();
		}
	}
}
