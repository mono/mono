// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Mono.Profiler.Log {

	public sealed class LogProcessor {

		public LogReader Reader { get; }

		public LogEventVisitor ImmediateVisitor { get; }

		public LogEventVisitor SortedVisitor { get; }

		public LogStreamHeader StreamHeader { get; private set; }

		LogBufferHeader _bufferHeader;

		ulong _time;

		bool _used;

		public LogProcessor (LogReader reader, LogEventVisitor immediateVisitor, LogEventVisitor sortedVisitor)
		{
			if (reader == null)
				throw new ArgumentNullException (nameof (reader));

			Reader = reader;
			ImmediateVisitor = immediateVisitor;
			SortedVisitor = sortedVisitor;
		}

		public void Process ()
		{
			Process (CancellationToken.None);
		}

		static void ProcessEvent (LogEventVisitor visitor, LogEvent ev)
		{
			if (visitor != null) {
				visitor.VisitBefore (ev);
				ev.Accept (visitor);
				visitor.VisitAfter (ev);
			}
		}

		void ProcessEvents (List<LogEvent> events, CancellationToken token)
		{
			foreach (var ev in events.OrderBy (x => x.Timestamp)) {
				token.ThrowIfCancellationRequested ();
				ProcessEvent (SortedVisitor, ev);
			}

			events.Clear ();
		}

		public void Process (CancellationToken token)
		{
			if (_used)
				throw new InvalidOperationException ("This log processor cannot be reused.");

			_used = true;
			StreamHeader = new LogStreamHeader (Reader);

			var events = new List<LogEvent> (Environment.ProcessorCount * 1000);

			while (!Reader.BaseStream.EndOfStream) {
				token.ThrowIfCancellationRequested ();

				_bufferHeader = new LogBufferHeader (StreamHeader, Reader);

				// Use the manual position tracking in LogReader so we're
				// compatible with non-seekable streams.
				var goal = Reader.Position + _bufferHeader.Length;

				while (Reader.Position < goal) {
					token.ThrowIfCancellationRequested ();

					var ev = ReadEvent ();

					ProcessEvent (ImmediateVisitor, ev);
					events.Add (ev);

					if (ev is SynchronizationPointEvent)
						ProcessEvents (events, token);
				}
			}

			ProcessEvents (events, token);
		}

		LogEvent ReadEvent ()
		{
			var type = Reader.ReadByte ();
			var basicType = (LogEventType) (type & 0xf);
			var extType = (LogEventType) (type & 0xf0);

			_time = ReadTime ();

			switch (basicType) {
			case LogEventType.Allocation:
				switch (extType) {
				case LogEventType.AllocationBacktrace:
				case LogEventType.AllocationNoBacktrace:
					return new AllocationEvent {
						ClassPointer = ReadPointer (),
						ObjectPointer = ReadObject (),
						ObjectSize = (long) Reader.ReadULeb128 (),
						Backtrace = ReadBacktrace (extType == LogEventType.AllocationBacktrace),
					};
				default:
					throw new LogException ($"Invalid extended event type ({extType}).");
				}
			case LogEventType.GC:
				switch (extType) {
				case LogEventType.GCEvent:
					return new GCEvent {
						Type = (LogGCEvent) Reader.ReadByte (),
						Generation = Reader.ReadByte (),
					};
				case LogEventType.GCResize:
					return new GCResizeEvent {
						NewSize = (long) Reader.ReadULeb128 (),
					};
				case LogEventType.GCMove: {
					var list = new long [(int) Reader.ReadULeb128 ()];

					for (var i = 0; i < list.Length; i++)
						list [i] = ReadObject ();

					return new GCMoveEvent {
						OldObjectPointers = list.Where ((_, i) => i % 2 == 0).ToArray (),
						NewObjectPointers = list.Where ((_, i) => i % 2 != 0).ToArray (),
					};
				}
				case LogEventType.GCHandleCreationNoBacktrace:
				case LogEventType.GCHandleCreationBacktrace:
					return new GCHandleCreationEvent {
						Type = (LogGCHandleType) Reader.ReadULeb128 (),
						Handle = (long) Reader.ReadULeb128 (),
						ObjectPointer = ReadObject (),
						Backtrace = ReadBacktrace (extType == LogEventType.GCHandleCreationBacktrace),
					};
				case LogEventType.GCHandleDeletionNoBacktrace:
				case LogEventType.GCHandleDeletionBacktrace:
					return new GCHandleDeletionEvent {
						Type = (LogGCHandleType) Reader.ReadULeb128 (),
						Handle = (long) Reader.ReadULeb128 (),
						Backtrace = ReadBacktrace (extType == LogEventType.GCHandleDeletionBacktrace),
					};
				case LogEventType.GCFinalizeBegin:
					return new GCFinalizeBeginEvent ();
				case LogEventType.GCFinalizeEnd:
					return new GCFinalizeEndEvent ();
				case LogEventType.GCFinalizeObjectBegin:
					return new GCFinalizeObjectBeginEvent {
						ObjectPointer = ReadObject (),
					};
				case LogEventType.GCFinalizeObjectEnd:
					return new GCFinalizeObjectEndEvent {
						ObjectPointer = ReadObject (),
					};
				default:
					throw new LogException ($"Invalid extended event type ({extType}).");
				}
			case LogEventType.Metadata: {
				var load = false;
				var unload = false;

				switch (extType) {
				case LogEventType.MetadataExtra:
					break;
				case LogEventType.MetadataEndLoad:
					load = true;
					break;
				case LogEventType.MetadataEndUnload:
					unload = true;
					break;
				default:
					throw new LogException ($"Invalid extended event type ({extType}).");
				}

				var metadataType = (LogMetadataType) Reader.ReadByte ();

				switch (metadataType) {
				case LogMetadataType.Class:
					if (load) {
						return new ClassLoadEvent {
							ClassPointer = ReadPointer (),
							ImagePointer = ReadPointer (),
							Name = Reader.ReadCString (),
						};
					} else
						throw new LogException ("Invalid class metadata event.");
				case LogMetadataType.Image:
					if (load) {
						return new ImageLoadEvent {
							ImagePointer = ReadPointer (),
							Name = Reader.ReadCString (),
						};
					} else if (unload) {
						return new ImageUnloadEvent {
							ImagePointer = ReadPointer (),
							Name = Reader.ReadCString (),
						};
					} else
						throw new LogException ("Invalid image metadata event.");
				case LogMetadataType.Assembly:
					if (load) {
						return new AssemblyLoadEvent {
							AssemblyPointer = ReadPointer (),
							ImagePointer = ReadPointer (),
							Name = Reader.ReadCString (),
						};
					} else if (unload) {
						return new AssemblyUnloadEvent {
							AssemblyPointer = ReadPointer (),
							ImagePointer = ReadPointer (),
							Name = Reader.ReadCString (),
						};
					} else
						throw new LogException ("Invalid assembly metadata event.");
				case LogMetadataType.AppDomain:
					if (load) {
						return new AppDomainLoadEvent {
							AppDomainId = ReadPointer (),
						};
					} else if (unload) {
						return new AppDomainUnloadEvent {
							AppDomainId = ReadPointer (),
						};
					} else {
						return new AppDomainNameEvent {
							AppDomainId = ReadPointer (),
							Name = Reader.ReadCString (),
						};
					}
				case LogMetadataType.Thread:
					if (load) {
						return new ThreadStartEvent {
							ThreadId = ReadPointer (),
						};
					} else if (unload) {
						return new ThreadEndEvent {
							ThreadId = ReadPointer (),
						};
					} else {
						return new ThreadNameEvent {
							ThreadId = ReadPointer (),
							Name = Reader.ReadCString (),
						};
					}
				case LogMetadataType.Context:
					if (load) {
						return new ContextLoadEvent {
							ContextId = ReadPointer (),
							AppDomainId = ReadPointer (),
						};
					} else if (unload) {
						return new ContextUnloadEvent {
							ContextId = ReadPointer (),
							AppDomainId = ReadPointer (),
						};
					} else
						throw new LogException ("Invalid context metadata event.");
				default:
					throw new LogException ($"Invalid metadata type ({metadataType}).");
				}
			}
			case LogEventType.Method:
				switch (extType) {
				case LogEventType.MethodLeave:
					return new LeaveEvent {
						MethodPointer = ReadMethod (),
					};
				case LogEventType.MethodEnter:
					return new EnterEvent {
						MethodPointer = ReadMethod (),
					};
				case LogEventType.MethodLeaveExceptional:
					return new ExceptionalLeaveEvent {
						MethodPointer = ReadMethod (),
					};
				case LogEventType.MethodJit:
					return new JitEvent {
						MethodPointer = ReadMethod (),
						CodePointer = ReadPointer (),
						CodeSize = (long) Reader.ReadULeb128 (),
						Name = Reader.ReadCString (),
					};
				default:
					throw new LogException ($"Invalid extended event type ({extType}).");
				}
			case LogEventType.Exception:
				switch (extType) {
				case LogEventType.ExceptionThrowNoBacktrace:
				case LogEventType.ExceptionThrowBacktrace:
					return new ThrowEvent {
						ObjectPointer = ReadObject (),
						Backtrace = ReadBacktrace (extType == LogEventType.ExceptionThrowBacktrace),
					};
				case LogEventType.ExceptionClause:
					return new ExceptionClauseEvent {
						Type = (LogExceptionClause) Reader.ReadByte (),
						Index = (long) Reader.ReadULeb128 (),
						MethodPointer = ReadMethod (),
						ObjectPointer = ReadObject (),
					};
				default:
					throw new LogException ($"Invalid extended event type ({extType}).");
				}
			case LogEventType.Monitor:
				switch (extType) {
				case LogEventType.MonitorNoBacktrace:
				case LogEventType.MonitorBacktrace:
					return new MonitorEvent {
						Event = (LogMonitorEvent) Reader.ReadByte (),
						ObjectPointer = ReadObject (),
						Backtrace = ReadBacktrace (extType == LogEventType.MonitorBacktrace),
					};
				default:
					throw new LogException ($"Invalid extended event type ({extType}).");
				}
			case LogEventType.Heap:
				switch (extType) {
				case LogEventType.HeapBegin:
					return new HeapBeginEvent ();
				case LogEventType.HeapEnd:
					return new HeapEndEvent ();
				case LogEventType.HeapObject: {
					var ev = new HeapObjectEvent {
						ObjectPointer = ReadObject (),
						ClassPointer = ReadPointer (),
						ObjectSize = (long) Reader.ReadULeb128 (),
					};

					var list = new HeapObjectEvent.HeapObjectReference [(int) Reader.ReadULeb128 ()];

					for (var i = 0; i < list.Length; i++) {
						list [i] = new HeapObjectEvent.HeapObjectReference {
							Offset = (long) Reader.ReadULeb128 (),
							ObjectPointer = ReadObject (),
						};
					}

					ev.References = list;

					return ev;
				}
				case LogEventType.HeapRoots: {
					// TODO: This entire event makes no sense.

					var ev = new HeapRootsEvent ();
					var list = new HeapRootsEvent.HeapRoot [(int) Reader.ReadULeb128 ()];

					ev.MaxGenerationCollectionCount = (long) Reader.ReadULeb128 ();

					for (var i = 0; i < list.Length; i++) {
						list [i] = new HeapRootsEvent.HeapRoot {
							ObjectPointer = ReadObject (),
							Attributes = (LogHeapRootAttributes) Reader.ReadByte (),
							ExtraInfo = (long) Reader.ReadULeb128 (),
						};
					}

					ev.Roots = list;

					return ev;
				}
				default:
					throw new LogException ($"Invalid extended event type ({extType}).");
				}
			case LogEventType.Sample:
				switch (extType) {
				case LogEventType.SampleHit:
					return new SampleHitEvent {
						ThreadId = ReadPointer (),
						UnmanagedBacktrace = ReadBacktrace (true, false),
						ManagedBacktrace = ReadBacktrace (true),
					};
				case LogEventType.SampleUnmanagedSymbol:
					return new UnmanagedSymbolEvent {
						CodePointer = ReadPointer (),
						CodeSize = (long) Reader.ReadULeb128 (),
						Name = Reader.ReadCString (),
					};
				case LogEventType.SampleUnmanagedBinary:
					return new UnmanagedBinaryEvent {
						SegmentPointer = ReadPointer (),
						SegmentOffset = (long) Reader.ReadULeb128 (),
						SegmentSize = (long) Reader.ReadULeb128 (),
						FileName = Reader.ReadCString (),
					};
				case LogEventType.SampleCounterDescriptions: {
					var ev = new CounterDescriptionsEvent ();
					var list = new CounterDescriptionsEvent.CounterDescription [(int) Reader.ReadULeb128 ()];

					for (var i = 0; i < list.Length; i++) {
						var section = (LogCounterSection) Reader.ReadULeb128 ();

						list [i] = new CounterDescriptionsEvent.CounterDescription {
							Section = section,
							SectionName = section == LogCounterSection.User ? Reader.ReadCString () : string.Empty,
							CounterName = Reader.ReadCString (),
							Type = (LogCounterType) Reader.ReadByte (),
							Unit = (LogCounterUnit) Reader.ReadByte (),
							Variance = (LogCounterVariance) Reader.ReadByte (),
							Index = (long) Reader.ReadULeb128 (),
						};
					}

					ev.Descriptions = list;

					return ev;
				}
				case LogEventType.SampleCounters: {
					var ev = new CounterSamplesEvent ();
					var list = new List<CounterSamplesEvent.CounterSample> ();

					while (true) {
						var index = (long) Reader.ReadULeb128 ();

						if (index == 0)
							break;

						var counterType = (LogCounterType) Reader.ReadByte ();

						object value = null;

						switch (counterType) {
						case LogCounterType.String:
							value = Reader.ReadByte () == 1 ? Reader.ReadCString () : null;
							break;
						case LogCounterType.Int32:
						case LogCounterType.Word:
						case LogCounterType.Int64:
						case LogCounterType.Interval:
							value = Reader.ReadSLeb128 ();
							break;
						case LogCounterType.UInt32:
						case LogCounterType.UInt64:
							value = Reader.ReadULeb128 ();
							break;
						case LogCounterType.Double:
							value = Reader.ReadDouble ();
							break;
						default:
							throw new LogException ($"Invalid counter type ({counterType}).");
						}

						list.Add (new CounterSamplesEvent.CounterSample {
							Index = index,
							Type = counterType,
							Value = value,
						});
					}

					ev.Samples = list;

					return ev;
				}
				default:
					throw new LogException ($"Invalid extended event type ({extType}).");
				}
			case LogEventType.Runtime:
				switch (extType) {
				case LogEventType.RuntimeJitHelper: {
					var helperType = (LogJitHelper) Reader.ReadByte ();

					return new JitHelperEvent {
						Type = helperType,
						BufferPointer = ReadPointer (),
						BufferSize = (long) Reader.ReadULeb128 (),
						Name = helperType == LogJitHelper.SpecificTrampoline ? Reader.ReadCString () : string.Empty,
					};
				}
				default:
					throw new LogException ($"Invalid extended event type ({extType}).");
				}
			case LogEventType.Meta:
				switch (extType) {
				case LogEventType.MetaSynchronizationPoint:
					return new SynchronizationPointEvent {
						Type = (LogSynchronizationPoint) Reader.ReadByte (),
					};
				default:
					throw new LogException ($"Invalid extended event type ({extType}).");
				}
			default:
				throw new LogException ($"Invalid basic event type ({basicType}).");
			}
		}

		long ReadPointer ()
		{
			var ptr = Reader.ReadSLeb128 () + _bufferHeader.PointerBase;

			return StreamHeader.PointerSize == sizeof (long) ? ptr : ptr & 0xffffffffL;
		}

		long ReadObject ()
		{
			return Reader.ReadSLeb128 () + _bufferHeader.ObjectBase << 3;
		}

		long ReadMethod ()
		{
			return _bufferHeader.CurrentMethod += Reader.ReadSLeb128 ();
		}

		ulong ReadTime ()
		{
			return _bufferHeader.CurrentTime += Reader.ReadULeb128 ();
		}

		IReadOnlyList<long> ReadBacktrace (bool actuallyRead, bool managed = true)
		{
			if (!actuallyRead)
				return Array.Empty<long> ();

			var list = new long [(int) Reader.ReadULeb128 ()];

			for (var i = 0; i < list.Length; i++)
				list [i] = managed ? ReadMethod () : ReadPointer ();

			return list;
		}
	}
}
