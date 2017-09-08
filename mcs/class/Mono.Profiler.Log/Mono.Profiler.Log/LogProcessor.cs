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
			LogEvent ev = null;

			switch (basicType) {
			case LogEventType.Allocation:
				switch (extType) {
				case LogEventType.AllocationBacktrace:
				case LogEventType.AllocationNoBacktrace:
					ev = new AllocationEvent {
						ClassPointer = ReadPointer (),
						ObjectPointer = ReadObject (),
						ObjectSize = (long) Reader.ReadULeb128 (),
						Backtrace = ReadBacktrace (extType == LogEventType.AllocationBacktrace),
					};
					break;
				default:
					throw new LogException ($"Invalid extended event type ({extType}).");
				}
				break;
			case LogEventType.GC:
				switch (extType) {
				case LogEventType.GCEvent:
					ev = new GCEvent {
						Type = (LogGCEvent) Reader.ReadByte (),
						Generation = Reader.ReadByte (),
					};
					break;
				case LogEventType.GCResize:
					ev = new GCResizeEvent {
						NewSize = (long) Reader.ReadULeb128 (),
					};
					break;
				case LogEventType.GCMove: {
					var list = new long [(int) Reader.ReadULeb128 ()];

					for (var i = 0; i < list.Length; i++)
						list [i] = ReadObject ();

					ev = new GCMoveEvent {
						OldObjectPointers = list.Where ((_, i) => i % 2 == 0).ToArray (),
						NewObjectPointers = list.Where ((_, i) => i % 2 != 0).ToArray (),
					};
					break;
				}
				case LogEventType.GCHandleCreationNoBacktrace:
				case LogEventType.GCHandleCreationBacktrace:
					ev = new GCHandleCreationEvent {
						Type = (LogGCHandleType) Reader.ReadULeb128 (),
						Handle = (long) Reader.ReadULeb128 (),
						ObjectPointer = ReadObject (),
						Backtrace = ReadBacktrace (extType == LogEventType.GCHandleCreationBacktrace),
					};
					break;
				case LogEventType.GCHandleDeletionNoBacktrace:
				case LogEventType.GCHandleDeletionBacktrace:
					ev = new GCHandleDeletionEvent {
						Type = (LogGCHandleType) Reader.ReadULeb128 (),
						Handle = (long) Reader.ReadULeb128 (),
						Backtrace = ReadBacktrace (extType == LogEventType.GCHandleDeletionBacktrace),
					};
					break;
				case LogEventType.GCFinalizeBegin:
					ev = new GCFinalizeBeginEvent ();
					break;
				case LogEventType.GCFinalizeEnd:
					ev = new GCFinalizeEndEvent ();
					break;
				case LogEventType.GCFinalizeObjectBegin:
					ev = new GCFinalizeObjectBeginEvent {
						ObjectPointer = ReadObject (),
					};
					break;
				case LogEventType.GCFinalizeObjectEnd:
					ev = new GCFinalizeObjectEndEvent {
						ObjectPointer = ReadObject (),
					};
					break;
				default:
					throw new LogException ($"Invalid extended event type ({extType}).");
				}
				break;
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
						ev = new ClassLoadEvent {
							ClassPointer = ReadPointer (),
							ImagePointer = ReadPointer (),
							Name = Reader.ReadCString (),
						};
					} else
						throw new LogException ("Invalid class metadata event.");
					break;
				case LogMetadataType.Image:
					if (load) {
						ev = new ImageLoadEvent {
							ImagePointer = ReadPointer (),
							Name = Reader.ReadCString (),
						};
					} else if (unload) {
						ev = new ImageUnloadEvent {
							ImagePointer = ReadPointer (),
							Name = Reader.ReadCString (),
						};
					} else
						throw new LogException ("Invalid image metadata event.");
					break;
				case LogMetadataType.Assembly:
					if (load) {
						ev = new AssemblyLoadEvent {
							AssemblyPointer = ReadPointer (),
							ImagePointer = StreamHeader.FormatVersion >= 14 ? ReadPointer () : 0,
							Name = Reader.ReadCString (),
						};
					} else if (unload) {
						ev = new AssemblyUnloadEvent {
							AssemblyPointer = ReadPointer (),
							ImagePointer = StreamHeader.FormatVersion >= 14 ? ReadPointer () : 0,
							Name = Reader.ReadCString (),
						};
					} else
						throw new LogException ("Invalid assembly metadata event.");
					break;
				case LogMetadataType.AppDomain:
					if (load) {
						ev = new AppDomainLoadEvent {
							AppDomainId = ReadPointer (),
						};
					} else if (unload) {
						ev = new AppDomainUnloadEvent {
							AppDomainId = ReadPointer (),
						};
					} else {
						ev = new AppDomainNameEvent {
							AppDomainId = ReadPointer (),
							Name = Reader.ReadCString (),
						};
					}
					break;
				case LogMetadataType.Thread:
					if (load) {
						ev = new ThreadStartEvent {
							ThreadId = ReadPointer (),
						};
					} else if (unload) {
						ev = new ThreadEndEvent {
							ThreadId = ReadPointer (),
						};
					} else {
						ev = new ThreadNameEvent {
							ThreadId = ReadPointer (),
							Name = Reader.ReadCString (),
						};
					}
					break;
				case LogMetadataType.Context:
					if (load) {
						ev = new ContextLoadEvent {
							ContextId = ReadPointer (),
							AppDomainId = ReadPointer (),
						};
					} else if (unload) {
						ev = new ContextUnloadEvent {
							ContextId = ReadPointer (),
							AppDomainId = ReadPointer (),
						};
					} else
						throw new LogException ("Invalid context metadata event.");
					break;
				default:
					throw new LogException ($"Invalid metadata type ({metadataType}).");
				}
				break;
			}
			case LogEventType.Method:
				switch (extType) {
				case LogEventType.MethodLeave:
					ev = new LeaveEvent {
						MethodPointer = ReadMethod (),
					};
					break;
				case LogEventType.MethodEnter:
					ev = new EnterEvent {
						MethodPointer = ReadMethod (),
					};
					break;
				case LogEventType.MethodLeaveExceptional:
					ev = new ExceptionalLeaveEvent {
						MethodPointer = ReadMethod (),
					};
					break;
				case LogEventType.MethodJit:
					ev = new JitEvent {
						MethodPointer = ReadMethod (),
						CodePointer = ReadPointer (),
						CodeSize = (long) Reader.ReadULeb128 (),
						Name = Reader.ReadCString (),
					};
					break;
				default:
					throw new LogException ($"Invalid extended event type ({extType}).");
				}
				break;
			case LogEventType.Exception:
				switch (extType) {
				case LogEventType.ExceptionThrowNoBacktrace:
				case LogEventType.ExceptionThrowBacktrace:
					ev = new ThrowEvent {
						ObjectPointer = ReadObject (),
						Backtrace = ReadBacktrace (extType == LogEventType.ExceptionThrowBacktrace),
					};
					break;
				case LogEventType.ExceptionClause:
					ev = new ExceptionClauseEvent {
						Type = (LogExceptionClause) Reader.ReadByte (),
						Index = (long) Reader.ReadULeb128 (),
						MethodPointer = ReadMethod (),
						ObjectPointer = StreamHeader.FormatVersion >= 14 ? ReadObject () : 0,
					};
					break;
				default:
					throw new LogException ($"Invalid extended event type ({extType}).");
				}
				break;
			case LogEventType.Monitor:
				if (StreamHeader.FormatVersion < 14) {
					if (extType.HasFlag (LogEventType.MonitorBacktrace)) {
						extType = LogEventType.MonitorBacktrace;
					} else {
						extType = LogEventType.MonitorNoBacktrace;
					}
				}
				switch (extType) {
				case LogEventType.MonitorNoBacktrace:
				case LogEventType.MonitorBacktrace:
					ev = new MonitorEvent {
						Event = StreamHeader.FormatVersion >= 14 ?
						                    (LogMonitorEvent) Reader.ReadByte () :
						                    (LogMonitorEvent) ((((byte) type & 0xf0) >> 4) & 0x3),
						ObjectPointer = ReadObject (),
						Backtrace = ReadBacktrace (extType == LogEventType.MonitorBacktrace),
					};
					break;
				default:
					throw new LogException ($"Invalid extended event type ({extType}).");
				}
				break;
			case LogEventType.Heap:
				switch (extType) {
				case LogEventType.HeapBegin:
					ev = new HeapBeginEvent ();
					break;
				case LogEventType.HeapEnd:
					ev = new HeapEndEvent ();
					break;
				case LogEventType.HeapObject: {
					HeapObjectEvent hoe = new HeapObjectEvent {
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

					hoe.References = list;
					ev = hoe;

					break;
				}

				case LogEventType.HeapRoots: {
					// TODO: This entire event makes no sense.
					var hre = new HeapRootsEvent ();
					var list = new HeapRootsEvent.HeapRoot [(int) Reader.ReadULeb128 ()];

					hre.MaxGenerationCollectionCount = (long) Reader.ReadULeb128 ();

					for (var i = 0; i < list.Length; i++) {
						list [i] = new HeapRootsEvent.HeapRoot {
							ObjectPointer = ReadObject (),
							Attributes = StreamHeader.FormatVersion == 13 ? (LogHeapRootAttributes) Reader.ReadByte () : (LogHeapRootAttributes) Reader.ReadULeb128 (),
							ExtraInfo = (long) Reader.ReadULeb128 (),
						};
					}

					hre.Roots = list;
					ev = hre;

					break;
				}
				default:
					throw new LogException ($"Invalid extended event type ({extType}).");
				}
				break;
			case LogEventType.Sample:
				switch (extType) {
				case LogEventType.SampleHit:
					if (StreamHeader.FormatVersion < 14) {
						// Read SampleType (always set to .Cycles) for versions < 14
						Reader.ReadByte ();
					}
					ev = new SampleHitEvent {
						ThreadId = ReadPointer (),
						UnmanagedBacktrace = ReadBacktrace (true, false),
						ManagedBacktrace = ReadBacktrace (true).Reverse ().ToArray (),
					};
					break;
				case LogEventType.SampleUnmanagedSymbol:
					ev = new UnmanagedSymbolEvent {
						CodePointer = ReadPointer (),
						CodeSize = (long) Reader.ReadULeb128 (),
						Name = Reader.ReadCString (),
					};
					break;
				case LogEventType.SampleUnmanagedBinary:
					ev = new UnmanagedBinaryEvent {
						SegmentPointer = StreamHeader.FormatVersion >= 14 ? ReadPointer () : Reader.ReadSLeb128 (),
						SegmentOffset = (long) Reader.ReadULeb128 (),
						SegmentSize = (long) Reader.ReadULeb128 (),
						FileName = Reader.ReadCString (),
					};
					break;
				case LogEventType.SampleCounterDescriptions: {
					var cde = new CounterDescriptionsEvent ();
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

					cde.Descriptions = list;
					ev = cde;

					break;
				}
				case LogEventType.SampleCounters: {
					var cse = new CounterSamplesEvent ();
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

					cse.Samples = list;
					ev = cse;

					break;
				}
				default:
					throw new LogException ($"Invalid extended event type ({extType}).");
				}
				break;
			case LogEventType.Runtime:
				switch (extType) {
				case LogEventType.RuntimeJitHelper: {
					var helperType = (LogJitHelper) Reader.ReadByte ();

					ev = new JitHelperEvent {
						Type = helperType,
						BufferPointer = ReadPointer (),
						BufferSize = (long) Reader.ReadULeb128 (),
						Name = helperType == LogJitHelper.SpecificTrampoline ? Reader.ReadCString () : string.Empty,
					};
					break;
				}
				default:
					throw new LogException ($"Invalid extended event type ({extType}).");
				}
				break;
			case LogEventType.Meta:
				switch (extType) {
				case LogEventType.MetaSynchronizationPoint:
					ev = new SynchronizationPointEvent {
						Type = (LogSynchronizationPoint) Reader.ReadByte (),
					};
					break;
				default:
					throw new LogException ($"Invalid extended event type ({extType}).");
				}
				break;
			default:
				throw new LogException ($"Invalid basic event type ({basicType}).");
			}

			ev.Timestamp = _time;
			ev.Buffer = _bufferHeader;

			return ev;
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
