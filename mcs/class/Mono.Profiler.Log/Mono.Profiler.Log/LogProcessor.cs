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

		public LogStream Stream { get; }

		public LogEventVisitor ImmediateVisitor { get; }

		public LogEventVisitor SortedVisitor { get; }

		public LogStreamHeader StreamHeader { get; private set; }

		LogReader _reader;

		LogBufferHeader _bufferHeader;

		ulong _time;

		bool _used;

		public LogProcessor (LogStream stream, LogEventVisitor immediateVisitor, LogEventVisitor sortedVisitor)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			Stream = stream;
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
			_reader = new LogReader (Stream, true);

			StreamHeader = new LogStreamHeader (_reader);

			var events = new List<LogEvent> (Environment.ProcessorCount * 1000);

			while (!Stream.EndOfStream) {
				token.ThrowIfCancellationRequested ();

				_bufferHeader = new LogBufferHeader (StreamHeader, _reader);

				// Read the entire buffer into a MemoryStream ahead of time to
				// reduce the amount of I/O system calls we do. This should be
				// fine since the profiler tries to keep buffers small and
				// flushes them every second at minimum. This also has the
				// advantage that we can use the Position and Length properties
				// even if the stream we read the buffer from is actually
				// non-seekable.
				var stream = new MemoryStream (_reader.ReadBytes (_bufferHeader.Length), false);

				using (var reader = new LogReader (stream, false)) {
					var oldReader = _reader;

					_reader = reader;

					while (stream.Position < stream.Length) {
						token.ThrowIfCancellationRequested ();

						var ev = ReadEvent ();

						ProcessEvent (ImmediateVisitor, ev);
						events.Add (ev);

						if (ev is SynchronizationPointEvent)
							ProcessEvents (events, token);
					}

					_reader = oldReader;
				}
			}

			ProcessEvents (events, token);
		}

		LogEvent ReadEvent ()
		{
			var type = _reader.ReadByte ();
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
						ClassPointer = StreamHeader.FormatVersion < 15 ? ReadPointer () : 0,
						VTablePointer = StreamHeader.FormatVersion >= 15 ? ReadPointer () : 0,
						ObjectPointer = ReadObject (),
						ObjectSize = (long) _reader.ReadULeb128 (),
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
						Type = (LogGCEvent) _reader.ReadByte (),
						Generation = _reader.ReadByte (),
					};
					break;
				case LogEventType.GCResize:
					ev = new GCResizeEvent {
						NewSize = (long) _reader.ReadULeb128 (),
					};
					break;
				case LogEventType.GCMove: {
					var list = new long [(int) _reader.ReadULeb128 ()];

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
						Type = (LogGCHandleType) _reader.ReadULeb128 (),
						Handle = (long) _reader.ReadULeb128 (),
						ObjectPointer = ReadObject (),
						Backtrace = ReadBacktrace (extType == LogEventType.GCHandleCreationBacktrace),
					};
					break;
				case LogEventType.GCHandleDeletionNoBacktrace:
				case LogEventType.GCHandleDeletionBacktrace:
					ev = new GCHandleDeletionEvent {
						Type = (LogGCHandleType) _reader.ReadULeb128 (),
						Handle = (long) _reader.ReadULeb128 (),
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

				var metadataType = (LogMetadataType) _reader.ReadByte ();

				switch (metadataType) {
				case LogMetadataType.Class:
					if (load) {
						ev = new ClassLoadEvent {
							ClassPointer = ReadPointer (),
							ImagePointer = ReadPointer (),
							Name = _reader.ReadCString (),
						};
					} else
						throw new LogException ("Invalid class metadata event.");
					break;
				case LogMetadataType.Image:
					if (load) {
						var ile = new ImageLoadEvent {
							ImagePointer = ReadPointer (),
							Name = _reader.ReadCString (),
						};

						if (StreamHeader.FormatVersion >= 16) {
							var guid = _reader.ReadCString ();

							ile.ModuleVersionId = guid == string.Empty ? Guid.Empty : Guid.Parse (guid);
						}

						ev = ile;
					} else if (unload) {
						ev = new ImageUnloadEvent {
							ImagePointer = ReadPointer (),
							Name = _reader.ReadCString (),
						};
					} else
						throw new LogException ("Invalid image metadata event.");
					break;
				case LogMetadataType.Assembly:
					if (load) {
						ev = new AssemblyLoadEvent {
							AssemblyPointer = ReadPointer (),
							ImagePointer = StreamHeader.FormatVersion >= 14 ? ReadPointer () : 0,
							Name = _reader.ReadCString (),
						};
					} else if (unload) {
						ev = new AssemblyUnloadEvent {
							AssemblyPointer = ReadPointer (),
							ImagePointer = StreamHeader.FormatVersion >= 14 ? ReadPointer () : 0,
							Name = _reader.ReadCString (),
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
							Name = _reader.ReadCString (),
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
							Name = _reader.ReadCString (),
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
				case LogMetadataType.VTable:
					if (load) {
						ev = new VTableLoadEvent {
							VTablePointer = ReadPointer (),
							AppDomainId = ReadPointer (),
							ClassPointer = ReadPointer (),
						};
					} else
						throw new LogException ("Invalid VTable metadata event.");
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
						CodeSize = (long) _reader.ReadULeb128 (),
						Name = _reader.ReadCString (),
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
						Type = (LogExceptionClause) _reader.ReadByte (),
						Index = (long) _reader.ReadULeb128 (),
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
						        (LogMonitorEvent) _reader.ReadByte () :
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
						ClassPointer = StreamHeader.FormatVersion < 15 ? ReadPointer () : 0,
						VTablePointer = StreamHeader.FormatVersion >= 15 ? ReadPointer () : 0,
						ObjectSize = (long) _reader.ReadULeb128 (),
						Generation = StreamHeader.FormatVersion >= 16 ? _reader.ReadByte () : 0,
					};

					var list = new HeapObjectEvent.HeapObjectReference [(int) _reader.ReadULeb128 ()];

					for (var i = 0; i < list.Length; i++) {
						list [i] = new HeapObjectEvent.HeapObjectReference {
							Offset = (long) _reader.ReadULeb128 (),
							ObjectPointer = ReadObject (),
						};
					}

					hoe.References = list;
					ev = hoe;

					break;
				}

				case LogEventType.HeapRoots: {
					var hre = new HeapRootsEvent ();
					var list = new HeapRootsEvent.HeapRoot [(int) _reader.ReadULeb128 ()];

					if (StreamHeader.FormatVersion < 15)
						hre.MaxGenerationCollectionCount = (long) _reader.ReadULeb128 ();

					for (var i = 0; i < list.Length; i++) {
						list [i] = new HeapRootsEvent.HeapRoot {
							SlotPointer = StreamHeader.FormatVersion >= 15 ? ReadPointer () : 0,
							ObjectPointer = ReadObject (),
							Attributes = StreamHeader.FormatVersion < 15 ?
							             (StreamHeader.FormatVersion == 13 ?
							              (LogHeapRootAttributes) _reader.ReadByte () :
							              (LogHeapRootAttributes) _reader.ReadULeb128 ()) :
							             0,
							ExtraInfo = StreamHeader.FormatVersion < 15 ? (long) _reader.ReadULeb128 () : 0,
						};
					}

					hre.Roots = list;
					ev = hre;

					break;
				}
				case LogEventType.HeapRootRegister:
					ev = new HeapRootRegisterEvent {
						RootPointer = ReadPointer (),
						RootSize = (long) _reader.ReadULeb128 (),
						Source = (LogHeapRootSource) _reader.ReadByte (),
						Key = ReadPointer (),
						Name = _reader.ReadCString (),
					};
					break;
				case LogEventType.HeapRootUnregister:
					ev = new HeapRootUnregisterEvent {
						RootPointer = ReadPointer (),
					};
					break;
				default:
					throw new LogException ($"Invalid extended event type ({extType}).");
				}
				break;
			case LogEventType.Sample:
				switch (extType) {
				case LogEventType.SampleHit:
					if (StreamHeader.FormatVersion < 14) {
						// Read SampleType (always set to .Cycles) for versions < 14
						_reader.ReadByte ();
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
						CodeSize = (long) _reader.ReadULeb128 (),
						Name = _reader.ReadCString (),
					};
					break;
				case LogEventType.SampleUnmanagedBinary:
					ev = new UnmanagedBinaryEvent {
						SegmentPointer = StreamHeader.FormatVersion >= 14 ? ReadPointer () : _reader.ReadSLeb128 (),
						SegmentOffset = (long) _reader.ReadULeb128 (),
						SegmentSize = (long) _reader.ReadULeb128 (),
						FileName = _reader.ReadCString (),
					};
					break;
				case LogEventType.SampleCounterDescriptions: {
					var cde = new CounterDescriptionsEvent ();
					var list = new CounterDescriptionsEvent.CounterDescription [(int) _reader.ReadULeb128 ()];

					for (var i = 0; i < list.Length; i++) {
						var section = (LogCounterSection) _reader.ReadULeb128 ();

						list [i] = new CounterDescriptionsEvent.CounterDescription {
							Section = section,
							SectionName = section == LogCounterSection.User ? _reader.ReadCString () : null,
							CounterName = _reader.ReadCString (),
							Type = StreamHeader.FormatVersion < 15 ? (LogCounterType) _reader.ReadByte () : (LogCounterType) _reader.ReadULeb128 (),
							Unit = StreamHeader.FormatVersion < 15 ? (LogCounterUnit) _reader.ReadByte () : (LogCounterUnit) _reader.ReadULeb128 (),
							Variance = StreamHeader.FormatVersion < 15 ? (LogCounterVariance) _reader.ReadByte () : (LogCounterVariance) _reader.ReadULeb128 (),
							Index = (long) _reader.ReadULeb128 (),
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
						var index = (long) _reader.ReadULeb128 ();

						if (index == 0)
							break;

						var counterType = StreamHeader.FormatVersion < 15 ? (LogCounterType) _reader.ReadByte () : (LogCounterType) _reader.ReadULeb128 ();

						object value = null;

						switch (counterType) {
						case LogCounterType.String:
							value = _reader.ReadByte () == 1 ? _reader.ReadCString () : null;
							break;
						case LogCounterType.Int32:
						case LogCounterType.Word:
						case LogCounterType.Int64:
						case LogCounterType.Interval:
							value = _reader.ReadSLeb128 ();
							break;
						case LogCounterType.UInt32:
						case LogCounterType.UInt64:
							value = _reader.ReadULeb128 ();
							break;
						case LogCounterType.Double:
							value = _reader.ReadDouble ();
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
					var helperType = (LogJitHelper) _reader.ReadByte ();

					if (StreamHeader.FormatVersion < 14)
						helperType--;

					ev = new JitHelperEvent {
						Type = helperType,
						BufferPointer = ReadPointer (),
						BufferSize = (long) _reader.ReadULeb128 (),
						Name = helperType == LogJitHelper.SpecificTrampoline ? _reader.ReadCString () : null,
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
						Type = (LogSynchronizationPoint) _reader.ReadByte (),
					};
					break;
				case LogEventType.MetaAotId:
					ev = new AotIdEvent {
						AotId = Guid.Parse (_reader.ReadCString ()),
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
			var ptr = _reader.ReadSLeb128 () + _bufferHeader.PointerBase;

			return StreamHeader.PointerSize == sizeof (long) ? ptr : ptr & 0xffffffffL;
		}

		long ReadObject ()
		{
			return _reader.ReadSLeb128 () + _bufferHeader.ObjectBase << 3;
		}

		long ReadMethod ()
		{
			return _bufferHeader.CurrentMethod += _reader.ReadSLeb128 ();
		}

		ulong ReadTime ()
		{
			return _bufferHeader.CurrentTime += _reader.ReadULeb128 ();
		}

		IReadOnlyList<long> ReadBacktrace (bool actuallyRead, bool managed = true)
		{
			if (!actuallyRead)
				return Array.Empty<long> ();

			var list = new long [(int) _reader.ReadULeb128 ()];

			for (var i = 0; i < list.Length; i++)
				list [i] = managed ? ReadMethod () : ReadPointer ();

			return list;
		}
	}
}
