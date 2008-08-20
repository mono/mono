//
// MsmqBindingBase.cs
//
// Author: Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace System.ServiceModel
{
	public abstract class MsmqBindingBase : Binding, IBindingRuntimePreferences
	{
		Uri custom_dead_letter_queue;
		DeadLetterQueue dead_letter_queue = DeadLetterQueue.System;
		bool durable = true, exactly_once = true, use_msmq_trace, use_source_journal;
		int max_retry_cycles = 2, receive_retry_count = 5;
		long max_recv_msg_size = 0x10000;
		ReceiveErrorHandling receive_error_handling;
		TimeSpan retry_cycle_delay = TimeSpan.FromMinutes (30), ttl = TimeSpan.FromDays (1);

		public Uri CustomDeadLetterQueue {
			get { return custom_dead_letter_queue; }
			set { custom_dead_letter_queue = value; }
		}

		public DeadLetterQueue DeadLetterQueue {
			get { return dead_letter_queue; }
			set { dead_letter_queue = value; }
		}

		public bool Durable {
			get { return durable; }
			set { durable = value; }
		}

		public bool ExactlyOnce {
			get { return exactly_once; }
			set { exactly_once = value; }
		}

		public long MaxReceivedMessageSize {
			get { return max_recv_msg_size; }
			set { max_recv_msg_size = value; }
		}

		public int MaxRetryCycles {
			get { return max_retry_cycles; }
			set { max_retry_cycles = value; }
		}

		public ReceiveErrorHandling ReceiveErrorHandling {
			get { return receive_error_handling; }
			set { receive_error_handling = value; }
		}

		public int ReceiveRetryCount {
			get { return receive_retry_count; }
			set { receive_retry_count = value; }
		}

		public TimeSpan RetryCycleDelay {
			get { return retry_cycle_delay; }
			set { retry_cycle_delay = value; }
		}

		public TimeSpan TimeToLive {
			get { return ttl; }
			set { ttl = value; }
		}

		public override string Scheme {
			get {
				foreach (BindingElement be in CreateBindingElements ())
					if (be is TransportBindingElement)
						return ((TransportBindingElement) be).Scheme;
				throw new Exception ("INTERNAL ERROR: no TransportBindingElement was created.");
			}
		}

		public bool UseMsmqTracing {
			get { return use_msmq_trace; }
			set { use_msmq_trace = value; }
		}

		public bool UseSourceJournal {
			get { return use_source_journal; }
			set { use_source_journal = value; }
		}

		[MonoTODO]
		bool IBindingRuntimePreferences.ReceiveSynchronously {
			get { throw new NotImplementedException (); }
		}
	}
}
