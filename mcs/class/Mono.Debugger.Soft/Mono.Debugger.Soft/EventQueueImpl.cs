#if FALSE
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Mono.Debugger;
using Mono.Debugger.Requests;
using Mono.Debugger.Events;

namespace Mono.Debugger.Soft
{
	class EventQueueImpl : MirrorImpl, EventQueue
	{
		bool disconnected;
		Dictionary<int, byte[]> reply_packets;
		Thread receiver_thread;
		Queue queue;
		object queue_monitor;
		object reply_packets_monitor;

		public EventQueueImpl (VirtualMachineImpl vm) : base (vm) {
			reply_packets = new Dictionary<int, byte[]> ();
			reply_packets_monitor = new Object ();

			queue = new Queue ();
			queue_monitor = new Object ();
			receiver_thread = new Thread (new ThreadStart (receiver_thread_main));
			receiver_thread.Start ();
		}

		public EventSet Remove () {
			if (disconnected)
				// FIXME: VMDisconnectedException
				throw new IOException ();

			lock (queue_monitor) {
				if (queue.Count == 0)
					Monitor.Wait (queue_monitor);
				return (EventSet)queue.Dequeue ();
			}
		}

		public EventSet Remove (int timeout) {
			throw new NotImplementedException ();
		}

		Event DecodeEventInfo (WireProtocol.EventInfo info) {
			EventRequest req = FindRequest (info.requestId);
			if (info.eventKind == WireProtocol.EVENT_VM_START) {
				WireProtocol.VMStartEventInfo einfo = (WireProtocol.VMStartEventInfo)info;
				return new VMStartEventImpl (vm, req, new ThreadReferenceImpl (vm, einfo.thread), new AppDomainMirrorImpl (vm, einfo.domain));
			} else if (info.eventKind == WireProtocol.EVENT_VM_DEATH) {
				return new VMDeathEventImpl (vm, req);
			} else if (info.eventKind == WireProtocol.EVENT_THREAD_START) {
				WireProtocol.ThreadStartEventInfo einfo = (WireProtocol.ThreadStartEventInfo)info;
				return new ThreadStartEventImpl (vm, req, new ThreadReferenceImpl (vm, einfo.thread));
			} else if (info.eventKind == WireProtocol.EVENT_THREAD_DEATH) {
				WireProtocol.ThreadDeathEventInfo einfo = (WireProtocol.ThreadDeathEventInfo)info;
				return new ThreadDeathEventImpl (vm, req, new ThreadReferenceImpl (vm, einfo.thread));
			} else {
				throw new NotImplementedException ();
			}
		}

		EventRequest FindRequest (int requestId) {
			if (requestId == 0)
				return null;
			else
				return ((EventRequestManagerImpl)vm.EventRequestManager).FindRequest (requestId);
		}

		// Wait for the reply for a command packet
		public byte[] WaitForReply (int packetId) {
			while (true) {
				lock (reply_packets_monitor) {
					if (reply_packets.ContainsKey (packetId)) {
						byte[] reply = reply_packets [packetId];
						reply_packets.Remove (packetId);
						return reply;
					} else {
						Monitor.Wait (reply_packets_monitor);
					}
				}
			}
		}

		void add_event_set (EventSet set) {
			lock (queue_monitor) {
				queue.Enqueue (set);
				Monitor.Pulse (queue_monitor);
			}
		}

		void receiver_thread_main () {

			Connection conn = vm.Connection;

			while (true) {
				byte[] packet = conn.ReadPacket ();

				if (packet.Length == 0) {
					disconnected = true;
				
					VMDisconnectEventImpl ev = new VMDisconnectEventImpl (vm, null);
					add_event_set (new EventSetImpl (vm, new Event [] { ev }, SuspendPolicy.SuspendNone));
					break;
				}

				if (WireProtocol.IsReplyPacket (packet)) {
					/* Reply packet */
					int id = WireProtocol.GetPacketId (packet);
					lock (reply_packets_monitor) {
						reply_packets [id] = packet;
						Monitor.PulseAll (reply_packets_monitor);
					}
				} else {
					WireProtocol.Packet decoded = WireProtocol.DecodePacket (packet);
					if (decoded is WireProtocol.Event.CompositePacket) {
						WireProtocol.Event.CompositePacket p = (WireProtocol.Event.CompositePacket)decoded;
						Event[] events = new Event [p.events.Length];
						for (int i = 0; i < p.events.Length; ++i) {
							events [i] = DecodeEventInfo (p.events [i]);
						}

						add_event_set (new EventSetImpl (vm, events, p.suspendPolicy));
					}
				}
			}
		}
    }
}
#endif