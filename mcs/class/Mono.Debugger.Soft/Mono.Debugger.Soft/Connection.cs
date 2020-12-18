using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Mono.Debugger.Soft
{
	public class VersionInfo {
		public string VMVersion {
			get; set;
		}

		public int MajorVersion {
			get; set;
		}

		public int MinorVersion {
			get; set;
		}

		/*
		 * Check that this version is at least major:minor
		 */
		public bool AtLeast (int major, int minor) {
			if ((MajorVersion > major) || ((MajorVersion == major && MinorVersion >= minor)))
				return true;
			else
				return false;
		}
	}

	struct SourceInfo {
		public string source_file;
		public byte[] hash;
	}

	class DebugInfo {
		public int max_il_offset;
		public int[] il_offsets;
		public int[] line_numbers;
		public int[] column_numbers;
		public int[] end_line_numbers;
		public int[] end_column_numbers;
		public SourceInfo[] source_files;
	}

	struct FrameInfo {
		public long id;
		public long method;
		public int il_offset;
		public StackFrameFlags flags;
	}

	class TypeInfo {
		public string ns, name, full_name;
		public long assembly, module, base_type, element_type;
		public int token, rank, attributes;
		public bool is_byref, is_pointer, is_primitive, is_valuetype, is_enum;
		public bool is_gtd, is_generic_type;
		public long[] nested;
		public long gtd;
		public long[] type_args;
	}

	struct IfaceMapInfo {
		public long iface_id;
		public long[] iface_methods;
		public long[] target_methods;
	}

	class MethodInfo {
		public int attributes, iattributes, token;
		public bool is_gmd, is_generic_method;
		public long gmd;
		public long[] type_args;
	}

	class MethodBodyInfo {
		public byte[] il;
		public ExceptionClauseInfo[] clauses;
	}

	struct ExceptionClauseInfo {
		public ExceptionClauseFlags flags;
		public int try_offset;
		public int try_length;
		public int handler_offset;
		public int handler_length;
		public int filter_offset;
		public long catch_type_id;
	}

	[Flags]
	enum ExceptionClauseFlags {
		None = 0x0,
		Filter = 0x1,
		Finally = 0x2,
		Fault = 0x4,
	}

	struct ParamInfo {
		public int call_conv;
		public int param_count;
		public int generic_param_count;
		public long ret_type;
		public long[] param_types;
		public string[] param_names;
	}

	struct LocalsInfo {
		public long[] types;
		public string[] names;
		public int[] live_range_start;
		public int[] live_range_end;
		public int[] scopes_start;
		public int[] scopes_end;
	}

	struct PropInfo {
		public long id;
		public string name;
		public long get_method, set_method;
		public int attrs;
	}

	class CattrNamedArgInfo {
		public bool is_property;
		public long id;
		public ValueImpl value;
	}

	class CattrInfo {
		public long ctor_id;
		public ValueImpl[] ctor_args;
		public CattrNamedArgInfo[] named_args;
	}

	class ThreadInfo {
		public bool is_thread_pool;
	}

	struct ObjectRefInfo {
		public long type_id;
		public long domain_id;
	}

	enum ValueTypeId {
		VALUE_TYPE_ID_NULL = 0xf0,
		VALUE_TYPE_ID_TYPE = 0xf1,
		VALUE_TYPE_ID_PARENT_VTYPE = 0xf2,
		VALUE_TYPE_ID_FIXED_ARRAY = 0xf3
	}

	[Flags]
	enum InvokeFlags {
		NONE = 0,
		DISABLE_BREAKPOINTS = 1,
		SINGLE_THREADED = 2,
		OUT_THIS = 4,
		OUT_ARGS = 8,
		VIRTUAL = 16,
	}

	enum ElementType {
		End		 = 0x00,
		Void		= 0x01,
		Boolean	 = 0x02,
		Char		= 0x03,
		I1		  = 0x04,
		U1		  = 0x05,
		I2		  = 0x06,
		U2		  = 0x07,
		I4		  = 0x08,
		U4		  = 0x09,
		I8		  = 0x0a,
		U8		  = 0x0b,
		R4		  = 0x0c,
		R8		  = 0x0d,
		String	  = 0x0e,
		Ptr		 = 0x0f,
		ByRef	   = 0x10,
		ValueType   = 0x11,
		Class	   = 0x12,
		Var        = 0x13,
		Array	   = 0x14,
		GenericInst = 0x15,
		TypedByRef  = 0x16,
		I		   = 0x18,
		U		   = 0x19,
		FnPtr	   = 0x1b,
		Object	  = 0x1c,
		SzArray	 = 0x1d,
		MVar       = 0x1e,
		CModReqD	= 0x1f,
		CModOpt	 = 0x20,
		Internal	= 0x21,
		Modifier	= 0x40,
		Sentinel	= 0x41,
		Pinned	  = 0x45,

		Type		= 0x50,
		Boxed	   = 0x51,
		Enum		= 0x55
	}

	class ValueImpl {
		public ElementType Type; /* or one of the VALUE_TYPE_ID constants */
		public long Objid;
		public object Value;
		public long Klass; // For ElementType.ValueType
		public ValueImpl[] Fields; // for ElementType.ValueType
		public bool IsEnum; // For ElementType.ValueType
		public long Id; /* For VALUE_TYPE_ID_TYPE */
		public int Index; /* For VALUE_TYPE_PARENT_VTYPE */
		public int FixedSize;
	}

	class ModuleInfo {
		public string Name, ScopeName, FQName, Guid, SourceLink;
		public long Assembly;
	}		

	class FieldMirrorInfo {
		public string Name;
		public long Parent, TypeId;
		public int Attrs;
	}

	enum TokenType {
		STRING = 0,
		TYPE = 1,
		FIELD = 2,
		METHOD = 3,
		UNKNOWN = 4
	}

	[Flags]
	enum StackFrameFlags {
		NONE = 0,
		DEBUGGER_INVOKE = 1,
		NATIVE_TRANSITION = 2
	}

	class ResolvedToken {
		public TokenType Type;
		public string Str;
		public long Id;
	}

	class Modifier {
	}

	class CountModifier : Modifier {
		public int Count {
			get; set;
		}
	}

	class LocationModifier : Modifier {
		public long Method {
			get; set;
		}

		public long Location {
			get; set;
		}
	}

	class StepModifier : Modifier {
		public long Thread {
			get; set;
		}

		public int Depth {
			get; set;
		}

		public int Size {
			get; set;
		}

		public int Filter {
			get; set;
		}
	}

	class ThreadModifier : Modifier {
		public long Thread {
			get; set;
		}
	}

	class ExceptionModifier : Modifier {
		public long Type {
			get; set;
		}
		public bool Caught {
			get; set;
		}
		public bool Uncaught {
			get; set;
		}
		public bool Subclasses {
			get; set;
		}
		public bool NotFilteredFeature {
			get; set;
		}
		public bool EverythingElse {
			get; set;
		}
	}

	class AssemblyModifier : Modifier {
		public long[] Assemblies {
			get; set;
		}
	}

	class SourceFileModifier : Modifier {
		public string[] SourceFiles {
			get; set;
		}
	}

	class TypeNameModifier : Modifier {
		public string[] TypeNames {
			get; set;
		}
	}

	class EventInfo {
		public EventType EventType {
			get; set;
		}

		public int ReqId {
			get; set;
		}

		public SuspendPolicy SuspendPolicy {
			get; set;
		}

		public long ThreadId {
			get; set;
		}

		public long Id {
			get; set;
		}

		public long Location {
			get; set;
		}

		public int Level {
			get; set;
		}

		public string Category {
			get; set;
		}

		public string Message {
			get; set;
		}

		public int ExitCode {
			get; set;
		}

		public string Dump {
			get; set;
		}

		public ulong Hash {
			get; set;
		}

		public EventInfo (EventType type, int req_id) {
			EventType = type;
			ReqId = req_id;
		}
	}

	public enum ErrorCode {
		NONE = 0,
		INVALID_OBJECT = 20,
		INVALID_FIELDID = 25,
		INVALID_FRAMEID = 30,
		NOT_IMPLEMENTED = 100,
		NOT_SUSPENDED = 101,
		INVALID_ARGUMENT = 102,
		ERR_UNLOADED = 103,
		ERR_NO_INVOCATION = 104,
		ABSENT_INFORMATION = 105,
		NO_SEQ_POINT_AT_IL_OFFSET = 106,
		INVOKE_ABORTED = 107
	}

	public class ErrorHandlerEventArgs : EventArgs {

		public ErrorCode ErrorCode {
			get; set;
		}

		public string ErrorMessage {
			get; set;
		}
	}

	/*
	 * Represents the connection to the debuggee
	 */
	public abstract class Connection
	{
		/*
		 * The protocol and the packet format is based on JDWP, the differences 
		 * are in the set of supported events, and the commands.
		 */
		internal const string HANDSHAKE_STRING = "DWP-Handshake";

		internal const int HEADER_LENGTH = 11;

		static readonly bool EnableConnectionLogging = !String.IsNullOrEmpty (Environment.GetEnvironmentVariable ("MONO_SDB_LOG"));
		static int ConnectionId;
		readonly StreamWriter LoggingStream;

		/*
		 * Th version of the wire-protocol implemented by the library. The library
		 * and the debuggee can communicate if they implement the same major version.
		 * If they implement a different minor version, they can communicate, but some
		 * features might not be available. This allows older clients to communicate
		 * with newer runtimes, and vice versa.
		 */
		internal const int MAJOR_VERSION = 2;
		internal const int MINOR_VERSION = 58;

		enum WPSuspendPolicy {
			NONE = 0,
			EVENT_THREAD = 1,
			ALL = 2
		}

		enum CommandSet {
			VM = 1,
			OBJECT_REF = 9,
			STRING_REF = 10,
			THREAD = 11,
			ARRAY_REF = 13,
			EVENT_REQUEST = 15,
			STACK_FRAME = 16,
			APPDOMAIN = 20,
			ASSEMBLY = 21,
			METHOD = 22,
			TYPE = 23,
			MODULE = 24,
			FIELD = 25,
			EVENT = 64,
			POINTER = 65
		}

		enum EventKind {
			VM_START = 0,
			VM_DEATH = 1,
			THREAD_START = 2,
			THREAD_DEATH = 3,
			APPDOMAIN_CREATE = 4, // Not in JDI
			APPDOMAIN_UNLOAD = 5, // Not in JDI
			METHOD_ENTRY = 6,
			METHOD_EXIT = 7,
			ASSEMBLY_LOAD = 8,
			ASSEMBLY_UNLOAD = 9,
			BREAKPOINT = 10,
			STEP = 11,
			TYPE_LOAD = 12,
			EXCEPTION = 13,
			KEEPALIVE = 14,
			USER_BREAK = 15,
			USER_LOG = 16,
			CRASH = 17
		}

		enum ModifierKind {
			COUNT = 1,
			THREAD_ONLY = 3,
			LOCATION_ONLY = 7,
			EXCEPTION_ONLY = 8,
			STEP = 10,
			ASSEMBLY_ONLY = 11,
			SOURCE_FILE_ONLY = 12,
			TYPE_NAME_ONLY = 13
		}

		enum CmdVM {
			VERSION = 1,
			ALL_THREADS = 2,
			SUSPEND = 3,
			RESUME = 4,
			EXIT = 5,
			DISPOSE = 6,
			INVOKE_METHOD = 7,
			SET_PROTOCOL_VERSION = 8,
			ABORT_INVOKE = 9,
			SET_KEEPALIVE = 10,
			GET_TYPES_FOR_SOURCE_FILE = 11,
			GET_TYPES = 12,
			INVOKE_METHODS = 13,
			START_BUFFERING = 14,
			STOP_BUFFERING = 15
		}

		enum CmdEvent {
			COMPOSITE = 100
		}

		enum CmdThread {
			GET_FRAME_INFO = 1,
			GET_NAME = 2,
			GET_STATE = 3,
			GET_INFO = 4,
			/* FIXME: Merge into GET_INFO when the major protocol version is increased */
			GET_ID = 5,
			/* Ditto */
			GET_TID = 6,
			SET_IP = 7,
			GET_ELAPSED_TIME = 8
		}

		enum CmdEventRequest {
			SET = 1,
			CLEAR = 2,
			CLEAR_ALL_BREAKPOINTS = 3
		}

		enum CmdAppDomain {
			GET_ROOT_DOMAIN = 1,
			GET_FRIENDLY_NAME = 2,
			GET_ASSEMBLIES = 3,
			GET_ENTRY_ASSEMBLY = 4,
			CREATE_STRING = 5,
			GET_CORLIB = 6,
			CREATE_BOXED_VALUE = 7,
			CREATE_BYTE_ARRAY = 8,
		}

		enum CmdAssembly {
			GET_LOCATION = 1,
			GET_ENTRY_POINT = 2,
			GET_MANIFEST_MODULE = 3,
			GET_OBJECT = 4,
			GET_TYPE = 5,
			GET_NAME = 6,
			GET_DOMAIN = 7,
			GET_METADATA_BLOB = 8,
			GET_IS_DYNAMIC = 9,
			GET_PDB_BLOB = 10,
			GET_TYPE_FROM_TOKEN = 11,
			GET_METHOD_FROM_TOKEN = 12,
			HAS_DEBUG_INFO = 13,
			GET_CATTRS = 14,
		}

		enum CmdModule {
			GET_INFO = 1,
		}

		enum CmdMethod {
			GET_NAME = 1,
			GET_DECLARING_TYPE = 2,
			GET_DEBUG_INFO = 3,
			GET_PARAM_INFO = 4,
			GET_LOCALS_INFO = 5,
			GET_INFO = 6,
			GET_BODY = 7,
			RESOLVE_TOKEN = 8,
			GET_CATTRS = 9,
			MAKE_GENERIC_METHOD = 10
		}

		enum CmdType {
			GET_INFO = 1,
			GET_METHODS = 2,
			GET_FIELDS = 3,
			GET_VALUES = 4,
			GET_OBJECT = 5,
			GET_SOURCE_FILES = 6,
			SET_VALUES = 7,
			IS_ASSIGNABLE_FROM = 8,
			GET_PROPERTIES = 9,
			GET_CATTRS = 10,
			GET_FIELD_CATTRS = 11,
			GET_PROPERTY_CATTRS = 12,
			/* FIXME: Merge into GET_SOURCE_FILES when the major protocol version is increased */
			GET_SOURCE_FILES_2 = 13,
			/* FIXME: Merge into GET_VALUES when the major protocol version is increased */
			GET_VALUES_2 = 14,
			CMD_TYPE_GET_METHODS_BY_NAME_FLAGS = 15,
			GET_INTERFACES = 16,
			GET_INTERFACE_MAP = 17,
			IS_INITIALIZED = 18,
			CREATE_INSTANCE = 19,
			GET_VALUE_SIZE = 20
		}

		enum CmdField {
			GET_INFO = 1
		}

		[Flags]
		enum BindingFlagsExtensions {
			BINDING_FLAGS_IGNORE_CASE = 0x70000000,
		}

		enum MemberListTypeExtensions {
			CaseSensitive = 1,
			CaseInsensitive = 2
		}

		enum CmdStackFrame {
			GET_VALUES = 1,
			GET_THIS = 2,
			SET_VALUES = 3,
			GET_DOMAIN = 4,
			SET_THIS = 5,
		}

		enum CmdArrayRef {
			GET_LENGTH = 1,
			GET_VALUES = 2,
			SET_VALUES = 3
		}

		enum CmdStringRef {
			GET_VALUE = 1,
			GET_LENGTH = 2,
			GET_CHARS = 3
		}

		enum CmdPointer {
			GET_VALUE = 1
		}

		enum CmdObjectRef {
			GET_TYPE = 1,
			GET_VALUES = 2,
			IS_COLLECTED = 3,
			GET_ADDRESS = 4,
			GET_DOMAIN = 5,
			SET_VALUES = 6,
			GET_INFO = 7,
		}

		class Header {
			public int id;
			public int command_set;
			public int command;
			public int flags;
		}			

		internal static int GetPacketLength (byte[] header) {
			int offset = 0;
			return decode_int (header, ref offset);
		}

		internal static bool IsReplyPacket (byte[] packet) {
			int offset = 8;
			return decode_byte (packet, ref offset) == 0x80;
		}

		internal static int GetPacketId (byte[] packet) {
			int offset = 4;
			return decode_int (packet, ref offset);
		}

		static int decode_byte (byte[] packet, ref int offset) {
			return packet [offset++];
		}

		static byte[] decode_bytes (byte[] packet, ref int offset, int length)
		{
			if (length + offset > packet.Length)
				throw new ArgumentOutOfRangeException ();

			var bytes = new byte[length];
			Array.Copy (packet, offset, bytes, 0, length);
			offset += length;
			return bytes;
		}

		static int decode_short (byte[] packet, ref int offset) {
			int res = ((int)packet [offset] << 8) | (int)packet [offset + 1];
			offset += 2;
			return res;
		}

		static int decode_int (byte[] packet, ref int offset) {
			int res = ((int)packet [offset] << 24) | ((int)packet [offset + 1] << 16) | ((int)packet [offset + 2] << 8) | (int)packet [offset + 3];
			offset += 4;
			return res;
		}

		static long decode_id (byte[] packet, ref int offset) {
			return decode_int (packet, ref offset);
		}

		static long decode_long (byte[] packet, ref int offset) {
			uint high = (uint)decode_int (packet, ref offset);
			uint low = (uint)decode_int (packet, ref offset);

			return (long)(((ulong)high << 32) | (ulong)low);
		}

		internal static SuspendPolicy decode_suspend_policy (int suspend_policy) {
			switch ((WPSuspendPolicy)suspend_policy) {
			case WPSuspendPolicy.NONE:
				return SuspendPolicy.None;
			case WPSuspendPolicy.EVENT_THREAD:
				return SuspendPolicy.EventThread;
			case WPSuspendPolicy.ALL:
				return SuspendPolicy.All;
			default:
				throw new NotImplementedException ();
			}
		}

		static Header decode_command_header (byte[] packet) {
			int offset = 0;
			Header res = new Header ();

			decode_int (packet, ref offset);
			res.id = decode_int (packet, ref offset);
			res.flags = decode_byte (packet, ref offset);
			res.command_set = decode_byte (packet, ref offset);
			res.command = decode_byte (packet, ref offset);

			return res;
		}

		static void encode_byte (byte[] buf, int b, ref int offset) {
			buf [offset] = (byte)b;
			offset ++;
		}

		static void encode_int (byte[] buf, int i, ref int offset) {
			buf [offset] = (byte)((i >> 24) & 0xff);
			buf [offset + 1] = (byte)((i >> 16) & 0xff);
			buf [offset + 2] = (byte)((i >> 8) & 0xff);
			buf [offset + 3] = (byte)((i >> 0) & 0xff);
			offset += 4;
		}

		static void encode_id (byte[] buf, long id, ref int offset) {
			encode_int (buf, (int)id, ref offset);
		}

		static void encode_long (byte[] buf, long l, ref int offset) {
			encode_int (buf, (int)((l >> 32) & 0xffffffff), ref offset);
			encode_int (buf, (int)(l & 0xffffffff), ref offset);
		}

		internal static byte[] EncodePacket (int id, int commandSet, int command, byte[] data, int dataLen) {
			byte[] buf = new byte [dataLen + 11];
			int offset = 0;
			
			encode_int (buf, buf.Length, ref offset);
			encode_int (buf, id, ref offset);
			encode_byte (buf, 0, ref offset);
			encode_byte (buf, commandSet, ref offset);
			encode_byte (buf, command, ref offset);

			for (int i = 0; i < dataLen; ++i)
				buf [offset + i] = data [i];

			return buf;
		}

		class PacketReader {
			Connection connection;
			byte[] packet;
			int offset;

			public PacketReader (Connection connection, byte[] packet) {
				this.connection = connection;
				this.packet = packet;

				// For event packets
				Header header = decode_command_header (packet);
				CommandSet = (CommandSet)header.command_set;
				Command = header.command;

				// For reply packets
				offset = 0;
				var len = ReadInt (); // length
				ReadInt (); // id
				ReadByte (); // flags
				ErrorCode = ReadShort ();
				if (ErrorCode == (int)Mono.Debugger.Soft.ErrorCode.INVALID_ARGUMENT && connection.Version.AtLeast (2, 56) && len > offset)
					ErrorMsg = ReadString ();
			}

			public CommandSet CommandSet {
				get; set;
			}

			public string ErrorMsg {
				get; internal set;
			}

			public int Command {
				get; set;
			}

			public int ErrorCode {
				get; set;
			}

			public int Offset {
				get {
					return offset;
				}
			}

			public int ReadByte () {
				return decode_byte (packet, ref offset);
			}

			public int ReadShort () {
				return decode_short (packet, ref offset);
			}

			public int ReadInt () {
				return decode_int (packet, ref offset);
			}

			public long ReadId () {
				return decode_id (packet, ref offset);
			}

			public long ReadLong () {
				return decode_long (packet, ref offset);
			}

			public float ReadFloat () {
				float f = DataConverter.FloatFromBE (packet, offset);
				offset += 4;
				return f;
			}

			public double ReadDouble () {
				double d = DataConverter.DoubleFromBE (packet, offset);
				offset += 8;
				return d;
			}

			public string ReadString () {
				int len = decode_int (packet, ref offset);
				string res = new String (Encoding.UTF8.GetChars (packet, offset, len));
				offset += len;
				return res;
			}

			public string ReadUTF16String () {
				int len = decode_int (packet, ref offset);
				string res = new String (Encoding.Unicode.GetChars (packet, offset, len));
				offset += len;
				return res;
			}

			public ValueImpl ReadValue () {
				ElementType etype = (ElementType)ReadByte ();

				switch (etype) {
				case ElementType.Void:
					return new ValueImpl { Type = etype };
				case ElementType.I1:
					return new ValueImpl { Type = etype, Value = (sbyte)ReadInt () };
				case ElementType.U1:
					return new ValueImpl { Type = etype, Value = (byte)ReadInt () };
				case ElementType.Boolean:
					return new ValueImpl { Type = etype, Value = ReadInt () != 0 };
				case ElementType.I2:
					return new ValueImpl { Type = etype, Value = (short)ReadInt () };
				case ElementType.U2:
					return new ValueImpl { Type = etype, Value = (ushort)ReadInt () };
				case ElementType.Char:
					return new ValueImpl { Type = etype, Value = (char)ReadInt () };
				case ElementType.I4:
					return new ValueImpl { Type = etype, Value = ReadInt () };
				case ElementType.U4:
					return new ValueImpl { Type = etype, Value = (uint)ReadInt () };
				case ElementType.I8:
					return new ValueImpl { Type = etype, Value = ReadLong () };
				case ElementType.U8:
					return new ValueImpl { Type = etype, Value = (ulong)ReadLong () };
				case ElementType.R4:
					return new ValueImpl { Type = etype, Value = ReadFloat () };
				case ElementType.R8:
					return new ValueImpl { Type = etype, Value = ReadDouble () };
				case ElementType.I:
				case ElementType.U:
					// FIXME: The client and the debuggee might have different word sizes
					return new ValueImpl { Type = etype, Value = ReadLong () };
				case ElementType.Ptr:
				case ElementType.FnPtr:
					long value = ReadLong ();
					if (connection.Version.AtLeast (2, 46)) {
						long pointerClass = ReadId ();
						return new ValueImpl { Type = etype, Klass = pointerClass, Value = value };
					} else {
						return new ValueImpl { Type = etype, Value = value };
					}
				case ElementType.String:
				case ElementType.SzArray:
				case ElementType.Class:
				case ElementType.Array:
				case ElementType.Object:
					long objid = ReadId ();
					return new ValueImpl () { Type = etype, Objid = objid };
				case ElementType.ValueType:
					bool is_enum = ReadByte () == 1;
					long klass = ReadId ();
					long nfields = ReadInt ();
					ValueImpl[] fields = new ValueImpl [nfields];
					for (int i = 0; i < nfields; ++i)
						fields [i] = ReadValue ();
					return new ValueImpl () { Type = etype, Klass = klass, Fields = fields, IsEnum = is_enum };
				case (ElementType)ValueTypeId.VALUE_TYPE_ID_NULL:
					return new ValueImpl { Type = etype };
				case (ElementType)ValueTypeId.VALUE_TYPE_ID_TYPE:
					return new ValueImpl () { Type = etype, Id = ReadId () };
				case (ElementType)ValueTypeId.VALUE_TYPE_ID_PARENT_VTYPE:
					return new ValueImpl () { Type = etype, Index = ReadInt () };
				case (ElementType)ValueTypeId.VALUE_TYPE_ID_FIXED_ARRAY:
					return ReadValueFixedSize ();
				default:
					throw new NotImplementedException ("Unable to handle type " + etype);
				}
			}

			ValueImpl ReadValueFixedSize () {
				var lenFixedSize = 1;
				ElementType etype = (ElementType)ReadByte ();
				lenFixedSize = ReadInt ();
				switch (etype) {
					case ElementType.I1: {
						var val = new sbyte[lenFixedSize];
						for (int i = 0; i < lenFixedSize; i++)
							val[i] = (sbyte)ReadInt ();
						return new ValueImpl { Type = etype, Value = val };
					}
					case ElementType.U1: {
						var val = new byte[lenFixedSize];
						for (int i = 0; i < lenFixedSize; i++)
							val[i] = (byte)ReadInt ();
						return new ValueImpl { Type = etype, Value = val };
					}
					case ElementType.Boolean: {
						var val = new bool[lenFixedSize];
						for (int i = 0; i < lenFixedSize; i++)
							val[i] = (ReadInt () != 0);
						return new ValueImpl { Type = etype, Value = val };
					}
					case ElementType.I2: {
						var val = new short[lenFixedSize];
						for (int i = 0; i < lenFixedSize; i++)
							val[i] = (short)ReadInt ();
						return new ValueImpl { Type = etype, Value = val };
					}
					case ElementType.U2: {
						var val = new ushort[lenFixedSize];
						for (int i = 0; i < lenFixedSize; i++)
							val[i] = (ushort)ReadInt ();
						return new ValueImpl { Type = etype, Value = val };
					}
					case ElementType.Char: {
						var val = new char[lenFixedSize];
						for (int i = 0; i < lenFixedSize; i++)
							val[i] = (char)ReadInt ();
						return new ValueImpl { Type = etype, Value = val };
					}
					case ElementType.I4: {
						var val = new int[lenFixedSize];
						for (int i = 0; i < lenFixedSize; i++)
							val[i] = ReadInt ();
						return new ValueImpl { Type = etype, Value = val };
					}
					case ElementType.U4: {
						var val = new uint[lenFixedSize];
						for (int i = 0; i < lenFixedSize; i++)
							val[i] = (uint)ReadInt ();
						return new ValueImpl { Type = etype, Value = val };
					}
					case ElementType.I8: {
						var val = new long[lenFixedSize];
						for (int i = 0; i < lenFixedSize; i++)
							val[i] = ReadLong ();
						return new ValueImpl { Type = etype, Value = val };
					}
					case ElementType.U8: {
						var val = new ulong[lenFixedSize];
						for (int i = 0; i < lenFixedSize; i++)
							val[i] = (ulong) ReadLong ();
						return new ValueImpl { Type = etype, Value = val };
					}
					case ElementType.R4: {
						var val = new float[lenFixedSize];
						for (int i = 0; i < lenFixedSize; i++)
							val[i] = ReadFloat ();
						return new ValueImpl { Type = etype, Value = val };
					}
					case ElementType.R8: {
						var val = new double[lenFixedSize];
						for (int i = 0; i < lenFixedSize; i++)
							val[i] = ReadDouble ();
						return new ValueImpl { Type = etype, Value = val };
					}
				}
				throw new NotImplementedException ("Unable to handle type " + etype);
			}

			public long[] ReadIds (int n) {
				long[] res = new long [n];
				for (int i = 0; i < n; ++i)
					res [i] = ReadId ();
				return res;
			}
			
			public byte[] ReadByteArray () {
				var length = ReadInt ();
				return decode_bytes (packet, ref offset, length);
			}

			public bool ReadBool () {
				return ReadByte () != 0;
			}
		}

		class PacketWriter {

			byte[] data;
			int offset;

			public PacketWriter () {
				data = new byte [1024];
				offset = 0;
			}

			void MakeRoom (int size) {
				if (offset + size >= data.Length) {
					int new_len = data.Length * 2;
					while (new_len < offset + size) {
						new_len *= 2;
					}
					byte[] new_data = new byte [new_len];
					Array.Copy (data, new_data, data.Length);
					data = new_data;
				}
			}

			public PacketWriter WriteByte (byte val) {
				MakeRoom (1);
				encode_byte (data, val, ref offset);
				return this;
			}

			public PacketWriter WriteInt (int val) {
				MakeRoom (4);
				encode_int (data, val, ref offset);
				return this;
			}

			public PacketWriter WriteId (long id) {
				MakeRoom (8);
				encode_id (data, id, ref offset);
				return this;
			}

			public PacketWriter WriteLong (long val) {
				MakeRoom (8);
				encode_long (data, val, ref offset);
				return this;
			}

			public PacketWriter WriteFloat (float f) {
				MakeRoom (8);
				byte[] b = DataConverter.GetBytesBE (f);
				for (int i = 0; i < 4; ++i)
					data [offset + i] = b [i];
				offset += 4;
				return this;
			}

			public PacketWriter WriteDouble (double d) {
				MakeRoom (8);
				byte[] b = DataConverter.GetBytesBE (d);
				for (int i = 0; i < 8; ++i)
					data [offset + i] = b [i];
				offset += 8;
				return this;
			}

			public PacketWriter WriteInts (int[] ids) {
				for (int i = 0; i < ids.Length; ++i)
					WriteInt (ids [i]);
				return this;
			}

			public PacketWriter WriteIds (long[] ids) {
				for (int i = 0; i < ids.Length; ++i)
					WriteId (ids [i]);
				return this;
			}

			public PacketWriter WriteString (string s) {
				if (s == null)
					return WriteInt (-1);

				byte[] b = Encoding.UTF8.GetBytes (s);
				MakeRoom (4);
				encode_int (data, b.Length, ref offset);
				MakeRoom (b.Length);
				Buffer.BlockCopy (b, 0, data, offset, b.Length);
				offset += b.Length;
				return this;
			}
			public PacketWriter WriteBytes (byte[] b) {
				if (b == null)
					return WriteInt (-1);
				MakeRoom (4);
				encode_int (data, b.Length, ref offset);
				MakeRoom (b.Length);
				Buffer.BlockCopy (b, 0, data, offset, b.Length);
				offset += b.Length;
				return this;
			}

			public PacketWriter WriteBool (bool val) {
				WriteByte (val ? (byte)1 : (byte)0);
				return this;
			}

			public PacketWriter WriteValue (ValueImpl v) {
				ElementType t;

				if (v.Value != null)
					t = TypeCodeToElementType (Type.GetTypeCode (v.Value.GetType ()), v.Value.GetType ());
				else
					t = v.Type;
				if (v.FixedSize > 1 && t != ElementType.ValueType) {
					WriteFixedSizeValue (v);
					return this;
				}
				WriteByte ((byte)t);
				switch (t) {
				case ElementType.Boolean:
					WriteInt ((bool)v.Value ? 1 : 0);
					break;
				case ElementType.Char:
					WriteInt ((int)(char)v.Value);
					break;
				case ElementType.I1:
					WriteInt ((int)(sbyte)v.Value);
					break;
				case ElementType.U1:
					WriteInt ((int)(byte)v.Value);
					break;
				case ElementType.I2:
					WriteInt ((int)(short)v.Value);
					break;
				case ElementType.U2:
					WriteInt ((int)(ushort)v.Value);
					break;
				case ElementType.I4:
					WriteInt ((int)(int)v.Value);
					break;
				case ElementType.U4:
					WriteInt ((int)(uint)v.Value);
					break;
				case ElementType.I8:
					WriteLong ((long)(long)v.Value);
					break;
				case ElementType.U8:
					WriteLong ((long)(ulong)v.Value);
					break;
				case ElementType.R4:
					WriteFloat ((float)v.Value);
					break;
				case ElementType.R8:
					WriteDouble ((double)v.Value);
					break;
				case ElementType.String:
				case ElementType.SzArray:
				case ElementType.Class:
				case ElementType.Array:
				case ElementType.Object:
					WriteId (v.Objid);
					break;
				case ElementType.ValueType:
					// FIXME: 
					if (v.IsEnum)
						throw new NotImplementedException ();
					WriteByte (0);
					WriteId (v.Klass);
					WriteInt (v.Fields.Length);
					for (int i = 0; i < v.Fields.Length; ++i)
						WriteValue (v.Fields [i]);
					break;
				case (ElementType)ValueTypeId.VALUE_TYPE_ID_NULL:
					break;
				default:
					throw new NotImplementedException ();
				}

				return this;
			}

			PacketWriter WriteFixedSizeValue (ValueImpl v) {
				ElementType t;

				if (v.Value != null)
					t = TypeCodeToElementType (Type.GetTypeCode (v.Value.GetType ()), v.Value.GetType ());
				else
					t = v.Type;
				WriteByte ((byte) ValueTypeId.VALUE_TYPE_ID_FIXED_ARRAY);
				WriteByte ((byte)t);
				WriteInt (v.FixedSize);
				for (int j = 0 ; j < v.FixedSize; j++) {
					switch (t) {
						case ElementType.Boolean:
							WriteInt (((bool[])v.Value)[j]? 1 : 0);
							break;
						case ElementType.Char:
							WriteInt ((int)((char[])v.Value)[j]);
							break;
						case ElementType.I1:
							WriteInt ((int)((sbyte[])v.Value)[j]);
							break;
						case ElementType.U1:
							WriteInt ((int)((byte[])v.Value)[j]);
							break;
						case ElementType.I2:
							WriteInt ((int)((short[])v.Value)[j]);
							break;
						case ElementType.U2:
							WriteInt ((int)((ushort[])v.Value)[j]);
							break;
						case ElementType.I4:
							WriteInt ((int)((int[])v.Value)[j]);
							break;
						case ElementType.U4:
							WriteInt ((int)((uint[])v.Value)[j]);
							break;
						case ElementType.I8:
							WriteLong ((long)((long[])v.Value)[j]);
							break;
						case ElementType.U8:
							WriteLong ((long)((ulong[])v.Value)[j]);
							break;
						case ElementType.R4:
							WriteFloat (((float[])v.Value)[j]);
							break;
						case ElementType.R8:
							WriteDouble (((double[])v.Value)[j]);
							break;
						default:
							throw new NotImplementedException ();
					}
				}
				return this;
			}

			public PacketWriter WriteValues (ValueImpl[] values) {
				for (int i = 0; i < values.Length; ++i)
					WriteValue (values [i]);
				return this;
			}

			public byte[] Data {
				get {
					return data;
				}
			}

			public int Offset {
				get {
					return offset;
				}
			}
		}

		delegate void ReplyCallback (int packet_id, byte[] packet);

		bool closed;
		Thread receiver_thread;
		Dictionary<int, byte[]> reply_packets;
		Dictionary<int, ReplyCallback> reply_cbs;
		Dictionary<int, int> reply_cb_counts;
		object reply_packets_monitor;

		internal event EventHandler<ErrorHandlerEventArgs> ErrorHandler;

		protected Connection () {
			closed = false;
			reply_packets = new Dictionary<int, byte[]> ();
			reply_cbs = new Dictionary<int, ReplyCallback> ();
			reply_cb_counts = new Dictionary<int, int> ();
			reply_packets_monitor = new Object ();
			if (EnableConnectionLogging) {
				var path = Environment.GetEnvironmentVariable ("MONO_SDB_LOG");
				if (path.Contains ("{0}")) {
					//C:\SomeDir\sdbLog{0}.txt -> C:\SomeDir\sdbLog1.txt
					LoggingStream = new StreamWriter (string.Format (path, ConnectionId++), false);
				} else if (Path.HasExtension (path)) {
					//C:\SomeDir\sdbLog.txt -> C:\SomeDir\sdbLog1.txt
					LoggingStream = new StreamWriter (Path.GetDirectoryName (path) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension (path) + ConnectionId++ + "." + Path.GetExtension (path), false);
				} else {
					//C:\SomeDir\sdbLog -> C:\SomeDir\sdbLog1
					LoggingStream = new StreamWriter (path + ConnectionId++, false);
				}
			}
		}

		protected abstract int TransportReceive (byte[] buf, int buf_offset, int len);
		protected abstract int TransportSend (byte[] buf, int buf_offset, int len);
		protected abstract void TransportSetTimeouts (int send_timeout, int receive_timeout);
		protected abstract void TransportClose ();
		// Shutdown breaks all communication, resuming blocking waits
		protected abstract void TransportShutdown ();

		internal VersionInfo Version;
		
		int Receive (byte[] buf, int buf_offset, int len) {
			int offset = 0;

			while (offset < len) {
				int n = TransportReceive (buf, buf_offset + offset, len - offset);

				if (n == 0)
					return offset;
				offset += n;
			}

			return offset;
		}
		
		// Do the wire protocol handshake
		internal void Connect () {
			byte[] buf = new byte [HANDSHAKE_STRING.Length];
			char[] cbuf = new char [buf.Length];

			// FIXME: Add a timeout
			int n = Receive (buf, 0, buf.Length);
			if (n == 0)
				throw new IOException ("DWP Handshake failed.");
			for (int i = 0; i < buf.Length; ++i)
				cbuf [i] = (char)buf [i];

			if (new String (cbuf) != HANDSHAKE_STRING)
				throw new IOException ("DWP Handshake failed.");
			
			TransportSend (buf, 0, buf.Length);

			receiver_thread = new Thread (new ThreadStart (receiver_thread_main));
			receiver_thread.Name = "SDB Receiver";
			receiver_thread.IsBackground = true;
			receiver_thread.Start ();

			Version = VM_GetVersion ();

			//
			// Tell the debuggee our protocol version, so newer debuggees can work
			// with older clients
			//

			//
			// Older debuggees might not support this request
			EventHandler<ErrorHandlerEventArgs> OrigErrorHandler = ErrorHandler;
			ErrorHandler = null;
			ErrorHandler += delegate (object sender, ErrorHandlerEventArgs args) {
				throw new NotSupportedException ();
			};
			try {
				VM_SetProtocolVersion (MAJOR_VERSION, MINOR_VERSION);
			} catch (NotSupportedException) {
			}
			ErrorHandler = OrigErrorHandler;
		}

		internal byte[] ReadPacket () {
			// FIXME: Throw ClosedConnectionException () if the connection is closed
			// FIXME: Throw ClosedConnectionException () if another thread closes the connection
			// FIXME: Locking
			byte[] header = new byte [HEADER_LENGTH];

			int len = Receive (header, 0, header.Length);
			if (len == 0)
				return new byte [0];
			if (len != HEADER_LENGTH) {
				// FIXME:
				throw new IOException ("Packet of length " + len + " is read.");
			}

			int packetLength = GetPacketLength (header);
			if (packetLength < 11)
				throw new IOException ("Invalid packet length.");

			if (packetLength == 11) {
				return header;
			} else {
				byte[] buf = new byte [packetLength];
				for (int i = 0; i < header.Length; ++i)
					buf [i] = header [i];
				len = Receive (buf, header.Length, packetLength - header.Length);
				if (len != packetLength - header.Length)
					throw new IOException ();
				return buf;
			}
		}

		internal void WritePacket (byte[] packet) {
			// FIXME: Throw ClosedConnectionException () if the connection is closed
			// FIXME: Throw ClosedConnectionException () if another thread closes the connection
			// FIXME: Locking
			TransportSend (packet, 0, packet.Length);
		}

		internal void WritePackets (List<byte[]> packets) {
			// FIXME: Throw ClosedConnectionException () if the connection is closed
			// FIXME: Throw ClosedConnectionException () if another thread closes the connection
			// FIXME: Locking
			int len = 0;
			for (int i = 0; i < packets.Count; ++i)
				len += packets [i].Length;
			byte[] data = new byte [len];
			int pos = 0;
			for (int i = 0; i < packets.Count; ++i) {
				Buffer.BlockCopy (packets [i], 0, data, pos, packets [i].Length);
				pos += packets [i].Length;
			}
			TransportSend (data, 0, data.Length);
		}

		internal void Close () {
			closed = true;
			TransportShutdown ();
		}

		internal bool IsClosed {
			get {
				return closed;
			}
		}

		bool disconnected;
		VMCrashException crashed;

		internal ManualResetEvent DisconnectedEvent = new ManualResetEvent (false);

		void receiver_thread_main () {
			while (!closed) {
				try {
					bool res = ReceivePacket ();
					if (!res) {
						break;
					}
				} catch (ThreadAbortException) {
					break;
				} catch (VMCrashException ex) {
					crashed = ex;
					break;
				} catch (Exception ex) {
					if (!closed) {
						Console.WriteLine (ex);
					}
					break;
				}
			}

			lock (reply_packets_monitor) {
				disconnected = true;
				DisconnectedEvent.Set ();
				Monitor.PulseAll (reply_packets_monitor);
				TransportClose ();
			}
			EventHandler.VMDisconnect (0, 0, null);
		}

		void disconnected_check () {
			if (!disconnected)
				return;
			else if (crashed != null)
				throw crashed;
			else
				throw new VMDisconnectedException ();
		}

		bool ReceivePacket () {
				byte[] packet = ReadPacket ();

				if (packet.Length == 0) {
					return false;
				}

				if (IsReplyPacket (packet)) {
					int id = GetPacketId (packet);
					ReplyCallback cb = null;
					lock (reply_packets_monitor) {
						reply_cbs.TryGetValue (id, out cb);
						if (cb == null) {
							reply_packets [id] = packet;
							Monitor.PulseAll (reply_packets_monitor);
						} else {
							int c = reply_cb_counts [id];
							c --;
							if (c == 0) {
								reply_cbs.Remove (id);
								reply_cb_counts.Remove (id);
							}
						}
					}

					if (cb != null)
						cb.Invoke (id, packet);
				} else {
					PacketReader r = new PacketReader (this, packet);

					if (r.CommandSet == CommandSet.EVENT && r.Command == (int)CmdEvent.COMPOSITE) {
						int spolicy = r.ReadByte ();
						int nevents = r.ReadInt ();

						SuspendPolicy suspend_policy = decode_suspend_policy (spolicy);

						EventInfo[] events = new EventInfo [nevents];

						for (int i = 0; i < nevents; ++i) {
							EventKind kind = (EventKind)r.ReadByte ();
							int req_id = r.ReadInt ();

							EventType etype = (EventType)kind;

							long thread_id = r.ReadId ();
							if (kind == EventKind.VM_START) {
								events [i] = new EventInfo (etype, req_id) { ThreadId = thread_id };
								//EventHandler.VMStart (req_id, thread_id, null);
							} else if (kind == EventKind.VM_DEATH) {
								int exit_code = 0;
								if (Version.AtLeast (2, 27))
									exit_code = r.ReadInt ();
								//EventHandler.VMDeath (req_id, 0, null);
								events [i] = new EventInfo (etype, req_id) { ExitCode = exit_code };
							} else if (kind == EventKind.CRASH) {
								ulong hash = (ulong) r.ReadLong ();
								string dump = r.ReadString ();

								events [i] = new EventInfo (etype, req_id) { Dump = dump, Hash = hash};
							} else if (kind == EventKind.THREAD_START) {
								events [i] = new EventInfo (etype, req_id) { ThreadId = thread_id, Id = thread_id };
								//EventHandler.ThreadStart (req_id, thread_id, thread_id);
							} else if (kind == EventKind.THREAD_DEATH) {
								events [i] = new EventInfo (etype, req_id) { ThreadId = thread_id, Id = thread_id };
								//EventHandler.ThreadDeath (req_id, thread_id, thread_id);
							} else if (kind == EventKind.ASSEMBLY_LOAD) {
								long id = r.ReadId ();
								events [i] = new EventInfo (etype, req_id) { ThreadId = thread_id, Id = id };
								//EventHandler.AssemblyLoad (req_id, thread_id, id);
							} else if (kind == EventKind.ASSEMBLY_UNLOAD) {
								long id = r.ReadId ();
								events [i] = new EventInfo (etype, req_id) { ThreadId = thread_id, Id = id };
								//EventHandler.AssemblyUnload (req_id, thread_id, id);
							} else if (kind == EventKind.TYPE_LOAD) {
								long id = r.ReadId ();
								events [i] = new EventInfo (etype, req_id) { ThreadId = thread_id, Id = id };
								//EventHandler.TypeLoad (req_id, thread_id, id);
							} else if (kind == EventKind.METHOD_ENTRY) {
								long id = r.ReadId ();
								events [i] = new EventInfo (etype, req_id) { ThreadId = thread_id, Id = id };
								//EventHandler.MethodEntry (req_id, thread_id, id);
							} else if (kind == EventKind.METHOD_EXIT) {
								long id = r.ReadId ();
								events [i] = new EventInfo (etype, req_id) { ThreadId = thread_id, Id = id };
								//EventHandler.MethodExit (req_id, thread_id, id);
							} else if (kind == EventKind.BREAKPOINT) {
								long id = r.ReadId ();
								long loc = r.ReadLong ();
								events [i] = new EventInfo (etype, req_id) { ThreadId = thread_id, Id = id, Location = loc };
								//EventHandler.Breakpoint (req_id, thread_id, id, loc);
							} else if (kind == EventKind.STEP) {
								long id = r.ReadId ();
								long loc = r.ReadLong ();
								events [i] = new EventInfo (etype, req_id) { ThreadId = thread_id, Id = id, Location = loc };
								//EventHandler.Step (req_id, thread_id, id, loc);
							} else if (kind == EventKind.EXCEPTION) {
								long id = r.ReadId ();
								long loc = 0; // FIXME
								events [i] = new EventInfo (etype, req_id) { ThreadId = thread_id, Id = id, Location = loc };
								//EventHandler.Exception (req_id, thread_id, id, loc);
							} else if (kind == EventKind.APPDOMAIN_CREATE) {
								long id = r.ReadId ();
								events [i] = new EventInfo (etype, req_id) { ThreadId = thread_id, Id = id };
								//EventHandler.AppDomainCreate (req_id, thread_id, id);
							} else if (kind == EventKind.APPDOMAIN_UNLOAD) {
								long id = r.ReadId ();
								events [i] = new EventInfo (etype, req_id) { ThreadId = thread_id, Id = id };
								//EventHandler.AppDomainUnload (req_id, thread_id, id);
							} else if (kind == EventKind.USER_BREAK) {
								long id = 0;
								long loc = 0;
								events [i] = new EventInfo (etype, req_id) { ThreadId = thread_id, Id = id, Location = loc };
								//EventHandler.Exception (req_id, thread_id, id, loc);
							} else if (kind == EventKind.USER_LOG) {
								int level = r.ReadInt ();
								string category = r.ReadString ();
								string message = r.ReadString ();
								events [i] = new EventInfo (etype, req_id) { ThreadId = thread_id, Level = level, Category = category, Message = message };
								//EventHandler.Exception (req_id, thread_id, id, loc);
							} else if (kind == EventKind.KEEPALIVE) {
								events [i] = new EventInfo (etype, req_id) { };
							} else {
								throw new NotImplementedException ("Unknown event kind: " + kind);
							}
						}

						EventHandler.Events (suspend_policy, events);
					}
				}

				return true;
		}

		internal IEventHandler EventHandler {
			get; set;
		}

		static String CommandString (CommandSet command_set, int command)
		{
			string cmd;
			switch (command_set) {
			case CommandSet.VM:
				cmd = ((CmdVM)command).ToString ();
				break;
			case CommandSet.OBJECT_REF:
				cmd = ((CmdObjectRef)command).ToString ();
				break;
			case CommandSet.STRING_REF:
				cmd = ((CmdStringRef)command).ToString ();
				break;
			case CommandSet.THREAD:
				cmd = ((CmdThread)command).ToString ();
				break;
			case CommandSet.ARRAY_REF:
				cmd = ((CmdArrayRef)command).ToString ();
				break;
			case CommandSet.EVENT_REQUEST:
				cmd = ((CmdEventRequest)command).ToString ();
				break;
			case CommandSet.STACK_FRAME:
				cmd = ((CmdStackFrame)command).ToString ();
				break;
			case CommandSet.APPDOMAIN:
				cmd = ((CmdAppDomain)command).ToString ();
				break;
			case CommandSet.ASSEMBLY:
				cmd = ((CmdAssembly)command).ToString ();
				break;
			case CommandSet.METHOD:
				cmd = ((CmdMethod)command).ToString ();
				break;
			case CommandSet.TYPE:
				cmd = ((CmdType)command).ToString ();
				break;
			case CommandSet.MODULE:
				cmd = ((CmdModule)command).ToString ();
				break;
			case CommandSet.FIELD:
				cmd = ((CmdField)command).ToString ();
				break;
			case CommandSet.EVENT:
				cmd = ((CmdEvent)command).ToString ();
				break;
			default:
				cmd = command.ToString ();
				break;
			}
			return string.Format ("[{0} {1}]", command_set, cmd);
		}

		long total_protocol_ticks;

		void LogPacket (int packet_id, byte[] encoded_packet, byte[] reply_packet, CommandSet command_set, int command, Stopwatch watch) {
			watch.Stop ();
			total_protocol_ticks += watch.ElapsedTicks;
			var ts = TimeSpan.FromTicks (total_protocol_ticks);
			string msg = string.Format ("Packet: {0} sent: {1} received: {2} ms: {3} total ms: {4} {5}",
			   packet_id, encoded_packet.Length, reply_packet.Length, watch.ElapsedMilliseconds,
			   (ts.Seconds * 1000) + ts.Milliseconds,
			   CommandString (command_set, command));

			LoggingStream.WriteLine (msg);
			LoggingStream.Flush ();
		}

		bool buffer_packets;
		List<byte[]> buffered_packets = new List<byte[]> ();

		//
		// Start buffering request/response packets on both the client and the debuggee side.
		// Packets sent between StartBuffering ()/StopBuffering () must be async, i.e. sent
		// using Send () and not SendReceive ().
		//
		public void StartBuffering () {
			buffer_packets = true;
			if (Version.AtLeast (2, 34))
				VM_StartBuffering ();
		}

		public void StopBuffering () {
			if (Version.AtLeast (2, 34))
				VM_StopBuffering ();
			buffer_packets = false;

			WritePackets (buffered_packets);
			if (EnableConnectionLogging) {
				LoggingStream.WriteLine (String.Format ("Sent {0} packets.", buffered_packets.Count));
				LoggingStream.Flush ();
			}
			buffered_packets.Clear ();
		}

		/* Send a request and call cb when a result is received */
		int Send (CommandSet command_set, int command, PacketWriter packet, Action<PacketReader> cb, int count) {
			int id = IdGenerator;

			Stopwatch watch = null;
			if (EnableConnectionLogging)
				watch = Stopwatch.StartNew ();

			byte[] encoded_packet;
			if (packet == null)
				encoded_packet = EncodePacket (id, (int)command_set, command, null, 0);
			else
				encoded_packet = EncodePacket (id, (int)command_set, command, packet.Data, packet.Offset);

			if (cb != null) {
				lock (reply_packets_monitor) {
					reply_cbs [id] = delegate (int packet_id, byte[] p) {
						if (EnableConnectionLogging)
							LogPacket (packet_id, encoded_packet, p, command_set, command, watch);
						/* Run the callback on a tp thread to avoid blocking the receive thread */
						PacketReader r = new PacketReader (this, p);
						cb.BeginInvoke (r, null, null);
					};
					reply_cb_counts [id] = count;
				}
			}

			if (buffer_packets)
				buffered_packets.Add (encoded_packet);
			else
				WritePacket (encoded_packet);

			return id;
		}

		// Send a request without waiting for an answer
		void Send (CommandSet command_set, int command) {
			Send (command_set, command, null, null, 0);
		}

		PacketReader SendReceive (CommandSet command_set, int command, PacketWriter packet) {
			int id = IdGenerator;
			Stopwatch watch = null;

			disconnected_check ();

			if (EnableConnectionLogging)
				watch = Stopwatch.StartNew ();

			byte[] encoded_packet;

			if (packet == null)
				encoded_packet = EncodePacket (id, (int)command_set, command, null, 0);
			else
				encoded_packet = EncodePacket (id, (int)command_set, command, packet.Data, packet.Offset);

			WritePacket (encoded_packet);

			int packetId = id;

			/* Wait for the reply packet */
			while (true) {
				lock (reply_packets_monitor) {
					byte[] reply;
					if (reply_packets.TryGetValue (packetId, out reply)) {
						reply_packets.Remove (packetId);
						PacketReader r = new PacketReader (this, reply);

						if (EnableConnectionLogging)
							LogPacket (packetId, encoded_packet, reply, command_set, command, watch);
						if (r.ErrorCode != 0) {
							if (ErrorHandler != null)
								ErrorHandler (this, new ErrorHandlerEventArgs () { ErrorCode = (ErrorCode)r.ErrorCode, ErrorMessage = r.ErrorMsg});
							throw new NotImplementedException ("No error handler set.");
						} else {
							return r;
						}
					} else {
						disconnected_check ();
						Monitor.Wait (reply_packets_monitor);
					}
				}
			}
		}

		PacketReader SendReceive (CommandSet command_set, int command) {
			return SendReceive (command_set, command, null);
		}

		int packet_id_generator;

		int IdGenerator {
			get {
				return Interlocked.Increment (ref packet_id_generator);
			}
		}

		CattrInfo[] ReadCattrs (PacketReader r) {
			CattrInfo[] res = new CattrInfo [r.ReadInt ()];
			for (int i = 0; i < res.Length; ++i) {
				CattrInfo info = new CattrInfo ();
				info.ctor_id = r.ReadId ();
				info.ctor_args = new ValueImpl [r.ReadInt ()];
				for (int j = 0; j < info.ctor_args.Length; ++j) {
					info.ctor_args [j] = r.ReadValue ();
				}
				info.named_args = new CattrNamedArgInfo [r.ReadInt ()];
				for (int j = 0; j < info.named_args.Length; ++j) {
					CattrNamedArgInfo arg = new CattrNamedArgInfo ();
					int arg_type = r.ReadByte ();
					arg.is_property = arg_type == 0x54;

					// 2.12 is the only version we can guarantee the server will send a field id
					// It was added in https://github.com/mono/mono/commit/db0b932cd6c3c93976479ae3f6b5b2a885f681de
					// In between 2.11 and 2.12
					if (arg.is_property)
						arg.id = r.ReadId ();
					else if (Version.AtLeast (2, 12))
						arg.id = r.ReadId ();

					arg.value = r.ReadValue ();
					info.named_args [j] = arg;
				}
				res [i] = info;
			}
			return res;
		}

		static ElementType TypeCodeToElementType (TypeCode c, Type t) {
			switch (c) {
			case TypeCode.Boolean:
				return ElementType.Boolean;
			case TypeCode.Char:
				return ElementType.Char;
			case TypeCode.SByte:
				return ElementType.I1;
			case TypeCode.Byte:
				return ElementType.U1;
			case TypeCode.Int16:
				return ElementType.I2;
			case TypeCode.UInt16:
				return ElementType.U2;
			case TypeCode.Int32:
				return ElementType.I4;
			case TypeCode.UInt32:
				return ElementType.U4;
			case TypeCode.Int64:
				return ElementType.I8;
			case TypeCode.UInt64:
				return ElementType.U8;
			case TypeCode.Single:
				return ElementType.R4;
			case TypeCode.Double:
				return ElementType.R8;
			case TypeCode.Object:
				return TypeCodeToElementType(Type.GetTypeCode (t.GetElementType()), t.GetElementType());
			default:
				throw new NotImplementedException ();
			}
		}

		/*
		 * Implementation of debugger commands
		 */

		internal VersionInfo VM_GetVersion () {
			var res = SendReceive (CommandSet.VM, (int)CmdVM.VERSION, null);
			VersionInfo info = new VersionInfo ();
			info.VMVersion = res.ReadString ();
			info.MajorVersion = res.ReadInt ();
			info.MinorVersion = res.ReadInt ();
			return info;
		}

		internal void VM_SetProtocolVersion (int major, int minor) {
			SendReceive (CommandSet.VM, (int)CmdVM.SET_PROTOCOL_VERSION, new PacketWriter ().WriteInt (major).WriteInt (minor));
		}

		internal void VM_GetThreads (Action<long[]> resultCallaback) {
			Send (CommandSet.VM, (int)CmdVM.ALL_THREADS, null, (res) => {
				int len = res.ReadInt ();
				long[] arr = new long [len];
				for (int i = 0; i < len; ++i)
					arr [i] = res.ReadId ();
				resultCallaback(arr);
			}, 1);
		}

		internal void VM_Suspend () {
			SendReceive (CommandSet.VM, (int)CmdVM.SUSPEND);
		}

		internal void VM_Resume () {
			SendReceive (CommandSet.VM, (int)CmdVM.RESUME);
		}

		internal void VM_Exit (int exitCode) {
			SendReceive (CommandSet.VM, (int)CmdVM.EXIT, new PacketWriter ().WriteInt (exitCode));
		}

		internal void VM_Dispose () {
			SendReceive (CommandSet.VM, (int)CmdVM.DISPOSE);
		}

		internal ValueImpl VM_InvokeMethod (long thread, long method, ValueImpl this_arg, ValueImpl[] arguments, InvokeFlags flags, out ValueImpl exc) {
			exc = null;
			PacketReader r = SendReceive (CommandSet.VM, (int)CmdVM.INVOKE_METHOD, new PacketWriter ().WriteId (thread).WriteInt ((int)flags).WriteId (method).WriteValue (this_arg).WriteInt (arguments.Length).WriteValues (arguments));
			if (r.ReadByte () == 0) {
				exc = r.ReadValue ();
				return null;
			} else {
				return r.ReadValue ();
			}
		}

		internal delegate void InvokeMethodCallback (ValueImpl v, ValueImpl exc, ValueImpl out_this, ValueImpl[] out_args, ErrorCode error, object state);

		void read_invoke_res (PacketReader r, out ValueImpl v, out ValueImpl exc, out ValueImpl out_this, out ValueImpl[] out_args) {
			int resflags = r.ReadByte ();
			v = null;
			exc = null;
			out_this = null;
			out_args = null;
			if (resflags == 0) {
				exc = r.ReadValue ();
			} else {
				v = r.ReadValue ();
				if ((resflags & 2) != 0)
					out_this = r.ReadValue ();
				if ((resflags & 4) != 0) {
					int nargs = r.ReadInt ();
					out_args = new ValueImpl [nargs];
					for (int i = 0; i < nargs; ++i)
						out_args [i] = r.ReadValue ();
				}
			}
		}

		internal int VM_BeginInvokeMethod (long thread, long method, ValueImpl this_arg, ValueImpl[] arguments, InvokeFlags flags, InvokeMethodCallback callback, object state) {
			return Send (CommandSet.VM, (int)CmdVM.INVOKE_METHOD, new PacketWriter ().WriteId (thread).WriteInt ((int)flags).WriteId (method).WriteValue (this_arg).WriteInt (arguments.Length).WriteValues (arguments), delegate (PacketReader r) {
					ValueImpl v, exc, out_this = null;
					ValueImpl[] out_args = null;

					if (r.ErrorCode != 0) {
						callback (null, null, null, null, (ErrorCode)r.ErrorCode, state);
					} else {
						read_invoke_res (r, out v, out exc, out out_this, out out_args);
						callback (v, exc, out_this, out_args, 0, state);
					}
				}, 1);
		}

		internal int VM_BeginInvokeMethods (long thread, long[] methods, ValueImpl this_arg, List<ValueImpl[]> arguments, InvokeFlags flags, InvokeMethodCallback callback, object state) {
			// FIXME: Merge this with INVOKE_METHOD
			var w = new PacketWriter ();
			w.WriteId (thread);
			w.WriteInt ((int)flags);
			w.WriteInt (methods.Length);
			for (int i = 0; i < methods.Length; ++i) {
				w.WriteId (methods [i]);
				w.WriteValue (this_arg);
				w.WriteInt (arguments [i].Length);
				w.WriteValues (arguments [i]);
			}
			return Send (CommandSet.VM, (int)CmdVM.INVOKE_METHODS, w, delegate (PacketReader r) {
					ValueImpl v, exc, out_this = null;
					ValueImpl[] out_args = null;

					if (r.ErrorCode != 0) {
						callback (null, null, null, null, (ErrorCode)r.ErrorCode, state);
					} else {
						read_invoke_res (r, out v, out exc, out out_this, out out_args);
						callback (v, exc, out_this, out_args, 0, state);
					}
				}, methods.Length);
		}

		internal void VM_AbortInvoke (long thread, int id)
		{
			SendReceive (CommandSet.VM, (int)CmdVM.ABORT_INVOKE, new PacketWriter ().WriteId (thread).WriteInt (id));
		}

		internal void SetSocketTimeouts (int send_timeout, int receive_timeout, int keepalive_interval)
		{
			TransportSetTimeouts (send_timeout, receive_timeout);
			SendReceive (CommandSet.VM, (int)CmdVM.SET_KEEPALIVE, new PacketWriter ().WriteId (keepalive_interval));
		}

		internal long[] VM_GetTypesForSourceFile (string fname, bool ignoreCase) {
			var res = SendReceive (CommandSet.VM, (int)CmdVM.GET_TYPES_FOR_SOURCE_FILE, new PacketWriter ().WriteString (fname).WriteBool (ignoreCase));
			int count = res.ReadInt ();
			long[] types = new long [count];
			for (int i = 0; i < count; ++i)
				types [i] = res.ReadId ();
			return types;
		}

		internal long[] VM_GetTypes (string name, bool ignoreCase) {
			var res = SendReceive (CommandSet.VM, (int)CmdVM.GET_TYPES, new PacketWriter ().WriteString (name).WriteBool (ignoreCase));
			int count = res.ReadInt ();
			long[] types = new long [count];
			for (int i = 0; i < count; ++i)
				types [i] = res.ReadId ();
			return types;
		}

		internal void VM_StartBuffering () {
			Send (CommandSet.VM, (int)CmdVM.START_BUFFERING);
		}

		internal void VM_StopBuffering () {
			Send (CommandSet.VM, (int)CmdVM.STOP_BUFFERING);
		}

		/*
		 * DOMAIN
		 */

		internal long RootDomain {
			get {
				return SendReceive (CommandSet.APPDOMAIN, (int)CmdAppDomain.GET_ROOT_DOMAIN, null).ReadId ();
			}
		}

		internal string Domain_GetName (long id) {
			return SendReceive (CommandSet.APPDOMAIN, (int)CmdAppDomain.GET_FRIENDLY_NAME, new PacketWriter ().WriteId (id)).ReadString ();
		}

		internal long[] Domain_GetAssemblies (long id) {
			var res = SendReceive (CommandSet.APPDOMAIN, (int)CmdAppDomain.GET_ASSEMBLIES, new PacketWriter ().WriteId (id));
			int count = res.ReadInt ();
			long[] assemblies = new long [count];
			for (int i = 0; i < count; ++i)
				assemblies [i] = res.ReadId ();
			return assemblies;
		}

		internal long Domain_GetEntryAssembly (long id) {
			return SendReceive (CommandSet.APPDOMAIN, (int)CmdAppDomain.GET_ENTRY_ASSEMBLY, new PacketWriter ().WriteId (id)).ReadId ();
		}

		internal long Domain_GetCorlib (long id) {
			return SendReceive (CommandSet.APPDOMAIN, (int)CmdAppDomain.GET_CORLIB, new PacketWriter ().WriteId (id)).ReadId ();
		}

		internal long Domain_CreateString (long id, string s) {
			return SendReceive (CommandSet.APPDOMAIN, (int)CmdAppDomain.CREATE_STRING, new PacketWriter ().WriteId (id).WriteString (s)).ReadId ();
		}

		internal long Domain_CreateByteArray (long id, byte [] bytes) {
			var w = new PacketWriter ().WriteId (id);
			w.WriteBytes (bytes);
			return SendReceive (CommandSet.APPDOMAIN, (int)CmdAppDomain.CREATE_BYTE_ARRAY, w).ReadId ();
		}

		internal long Domain_CreateBoxedValue (long id, long type_id, ValueImpl v) {
			return SendReceive (CommandSet.APPDOMAIN, (int)CmdAppDomain.CREATE_BOXED_VALUE, new PacketWriter ().WriteId (id).WriteId (type_id).WriteValue (v)).ReadId ();
		}

		/*
		 * METHOD
		 */

		internal string Method_GetName (long id) {
			return SendReceive (CommandSet.METHOD, (int)CmdMethod.GET_NAME, new PacketWriter ().WriteId (id)).ReadString ();
		}

		internal long Method_GetDeclaringType (long id) {
			return SendReceive (CommandSet.METHOD, (int)CmdMethod.GET_DECLARING_TYPE, new PacketWriter ().WriteId (id)).ReadId ();
		}

		internal DebugInfo Method_GetDebugInfo (long id) {
			var res = SendReceive (CommandSet.METHOD, (int)CmdMethod.GET_DEBUG_INFO, new PacketWriter ().WriteId (id));

			DebugInfo info = new DebugInfo ();
			info.max_il_offset = res.ReadInt ();

			SourceInfo[] sources = null;
			if (Version.AtLeast (2, 13)) {
				int n = res.ReadInt ();
				sources = new SourceInfo [n];
				for (int i = 0; i < n; ++i) {
					sources [i].source_file = res.ReadString ();
					if (Version.AtLeast (2, 14)) {
						sources [i].hash = new byte [16];
						for (int j = 0; j < 16; ++j)
							sources [i].hash [j] = (byte)res.ReadByte ();
					}
				}
			} else {
				sources = new SourceInfo [1];
				sources [0].source_file = res.ReadString ();
			}

			int n_il_offsets = res.ReadInt ();
			info.il_offsets = new int [n_il_offsets];
			info.line_numbers = new int [n_il_offsets];
			info.source_files = new SourceInfo [n_il_offsets];
			info.column_numbers = new int [n_il_offsets];
			info.end_line_numbers = new int [n_il_offsets];
			info.end_column_numbers = new int [n_il_offsets];
			for (int i = 0; i < n_il_offsets; ++i) {
				info.il_offsets [i] = res.ReadInt ();
				info.line_numbers [i] = res.ReadInt ();
				if (Version.AtLeast (2, 12)) {
					int idx = res.ReadInt ();
					info.source_files [i] = idx >= 0 ? sources [idx] : default (SourceInfo);
				} else {
					info.source_files [i] = sources [0];
				}
				if (Version.AtLeast (2, 19))
					info.column_numbers [i] = res.ReadInt ();
				else
					info.column_numbers [i] = 0;
				if (Version.AtLeast (2, 32)) {
					info.end_line_numbers [i] = res.ReadInt ();
					info.end_column_numbers [i] = res.ReadInt ();
				} else {
					info.end_column_numbers [i] = -1;
					info.end_column_numbers [i] = -1;
				}
			}

			return info;
		}

		internal ParamInfo Method_GetParamInfo (long id) {
			var res = SendReceive (CommandSet.METHOD, (int)CmdMethod.GET_PARAM_INFO, new PacketWriter ().WriteId (id));

			ParamInfo info = new ParamInfo ();
			info.call_conv = res.ReadInt ();
			info.param_count = res.ReadInt ();
			info.generic_param_count = res.ReadInt ();
			info.ret_type = res.ReadId ();
			info.param_types = new long [info.param_count];
			for (int i = 0; i < info.param_count; ++i)
				info.param_types [i] = res.ReadId ();
			info.param_names = new string [info.param_count];			
			for (int i = 0; i < info.param_count; ++i)
				info.param_names [i] = res.ReadString ();

			return info;
		}

		internal LocalsInfo Method_GetLocalsInfo (long id) {
			var res = SendReceive (CommandSet.METHOD, (int)CmdMethod.GET_LOCALS_INFO, new PacketWriter ().WriteId (id));

			LocalsInfo info = new LocalsInfo ();

			if (Version.AtLeast (2, 43)) {
				int nscopes = res.ReadInt ();
				info.scopes_start = new int [nscopes];
				info.scopes_end = new int [nscopes];
				int last_start = 0;
				for (int i = 0; i < nscopes; ++i) {
					info.scopes_start [i] = last_start + res.ReadInt ();
					info.scopes_end [i] = info.scopes_start [i] + res.ReadInt ();
					last_start = info.scopes_start [i];
				}
			}

			int nlocals = res.ReadInt ();
			info.types = new long [nlocals];
			for (int i = 0; i < nlocals; ++i)
				info.types [i] = res.ReadId ();
			info.names = new string [nlocals];
			for (int i = 0; i < nlocals; ++i)
				info.names [i] = res.ReadString ();
			info.live_range_start = new int [nlocals];
			info.live_range_end = new int [nlocals];
			for (int i = 0; i < nlocals; ++i) {
				info.live_range_start [i] = res.ReadInt ();
				info.live_range_end [i] = res.ReadInt ();
			}

			return info;
		}

		internal MethodInfo Method_GetInfo (long id) {
			var res = SendReceive (CommandSet.METHOD, (int)CmdMethod.GET_INFO, new PacketWriter ().WriteId (id));

			MethodInfo info = new MethodInfo ();
			info.attributes = res.ReadInt ();
			info.iattributes = res.ReadInt ();
			info.token = res.ReadInt ();
			if (Version.AtLeast (2, 12)) {
				int attrs = res.ReadByte ();
				if ((attrs & (1 << 0)) != 0)
					info.is_gmd = true;
				if ((attrs & (1 << 1)) != 0)
					info.is_generic_method = true;
				info.gmd = res.ReadId ();
				if (Version.AtLeast (2, 15)) {
					if (info.is_generic_method) {
						int n = res.ReadInt ();
						info.type_args = res.ReadIds (n);
					}
				}
			}
			return info;
		}

		internal MethodBodyInfo Method_GetBody (long id) {
			var res = SendReceive (CommandSet.METHOD, (int)CmdMethod.GET_BODY, new PacketWriter ().WriteId (id));

			MethodBodyInfo info = new MethodBodyInfo ();
			info.il = new byte [res.ReadInt ()];
			for (int i = 0; i < info.il.Length; ++i)
				info.il [i] = (byte)res.ReadByte ();

			if (Version.AtLeast (2, 18)) {
				info.clauses = new ExceptionClauseInfo [res.ReadInt ()];

				for (int i = 0; i < info.clauses.Length; ++i) {
					var clause = new ExceptionClauseInfo {
						flags = (ExceptionClauseFlags) res.ReadInt (),
						try_offset = res.ReadInt (),
						try_length = res.ReadInt (),
						handler_offset = res.ReadInt (),
						handler_length = res.ReadInt (),
					};

					if (clause.flags == ExceptionClauseFlags.None)
						clause.catch_type_id = res.ReadId ();
					else if (clause.flags == ExceptionClauseFlags.Filter)
						clause.filter_offset = res.ReadInt ();

					info.clauses [i] = clause;
				}
			} else
				info.clauses = new ExceptionClauseInfo [0];

			return info;
		}

		internal ResolvedToken Method_ResolveToken (long id, int token) {
			var res = SendReceive (CommandSet.METHOD, (int)CmdMethod.RESOLVE_TOKEN, new PacketWriter ().WriteId (id).WriteInt (token));

			TokenType type = (TokenType)res.ReadByte ();
			switch (type) {
			case TokenType.STRING:
				return new ResolvedToken () { Type = type, Str = res.ReadString () };
			case TokenType.TYPE:
			case TokenType.METHOD:
			case TokenType.FIELD:
				return new ResolvedToken () { Type = type, Id = res.ReadId () };
			case TokenType.UNKNOWN:
				return new ResolvedToken () { Type = type };
			default:
				throw new NotImplementedException ();
			}
		}

		internal CattrInfo[] Method_GetCustomAttributes (long id, long attr_type_id, bool inherit) {
			PacketReader r = SendReceive (CommandSet.METHOD, (int)CmdMethod.GET_CATTRS, new PacketWriter ().WriteId (id).WriteId (attr_type_id));
			return ReadCattrs (r);
		}

		internal long Method_MakeGenericMethod (long id, long[] args) {
			PacketReader r = SendReceive (CommandSet.METHOD, (int)CmdMethod.MAKE_GENERIC_METHOD, new PacketWriter ().WriteId (id).WriteInt (args.Length).WriteIds (args));
			return r.ReadId ();
		}

		/*
		 * THREAD
		 */

		internal string Thread_GetName (long id) {
			return SendReceive (CommandSet.THREAD, (int)CmdThread.GET_NAME, new PacketWriter ().WriteId (id)).ReadString ();
		}

		internal long Thread_GetElapsedTime (long id) {
			return SendReceive (CommandSet.THREAD, (int)CmdThread.GET_ELAPSED_TIME, new PacketWriter ().WriteId (id)).ReadLong ();
		}

		internal void Thread_GetFrameInfo (long id, int start_frame, int length, Action<FrameInfo[]> resultCallaback) {
			Send (CommandSet.THREAD, (int)CmdThread.GET_FRAME_INFO, new PacketWriter ().WriteId (id).WriteInt (start_frame).WriteInt (length), (res) => {
				int count = res.ReadInt ();
				var frames = new FrameInfo[count];
				for (int i = 0; i < count; ++i) {
					var f = new FrameInfo ();
					f.id = res.ReadInt ();
					f.method = res.ReadId ();
					f.il_offset = res.ReadInt ();
					f.flags = (StackFrameFlags)res.ReadByte ();
					frames [i] = f;
				}
				resultCallaback (frames);
			}, 1);
		}

		internal int Thread_GetState (long id) {
			return SendReceive (CommandSet.THREAD, (int)CmdThread.GET_STATE, new PacketWriter ().WriteId (id)).ReadInt ();
		}

		internal ThreadInfo Thread_GetInfo (long id) {
			PacketReader r = SendReceive (CommandSet.THREAD, (int)CmdThread.GET_INFO, new PacketWriter ().WriteId (id));

			ThreadInfo res = new ThreadInfo () { is_thread_pool = r.ReadByte () > 0 ? true : false };

			return res;
		}

		internal long Thread_GetId (long id) {
			return SendReceive (CommandSet.THREAD, (int)CmdThread.GET_ID, new PacketWriter ().WriteId (id)).ReadLong ();
		}

		internal long Thread_GetTID (long id) {
			return SendReceive (CommandSet.THREAD, (int)CmdThread.GET_TID, new PacketWriter ().WriteId (id)).ReadLong ();
		}

		internal void Thread_SetIP (long id, long method_id, long il_offset) {
			SendReceive (CommandSet.THREAD, (int)CmdThread.SET_IP, new PacketWriter ().WriteId (id).WriteId (method_id).WriteLong (il_offset));
		}

		/*
		 * MODULE
		 */

		internal ModuleInfo Module_GetInfo (long id) {
			PacketReader r = SendReceive (CommandSet.MODULE, (int)CmdModule.GET_INFO, new PacketWriter ().WriteId (id));
			ModuleInfo info = new ModuleInfo { Name = r.ReadString (), ScopeName = r.ReadString (), FQName = r.ReadString (), Guid = r.ReadString (), Assembly = r.ReadId () };
			if (Version.AtLeast (2, 48))
				info.SourceLink = r.ReadString ();
			return info;
		}

		/*
		 * ASSEMBLY
		 */

		internal string Assembly_GetLocation (long id) {
			return SendReceive (CommandSet.ASSEMBLY, (int)CmdAssembly.GET_LOCATION, new PacketWriter ().WriteId (id)).ReadString ();
		}

		internal long Assembly_GetEntryPoint (long id) {
			return SendReceive (CommandSet.ASSEMBLY, (int)CmdAssembly.GET_ENTRY_POINT, new PacketWriter ().WriteId (id)).ReadId ();
		}

		internal long Assembly_GetManifestModule (long id) {
			return SendReceive (CommandSet.ASSEMBLY, (int)CmdAssembly.GET_MANIFEST_MODULE, new PacketWriter ().WriteId (id)).ReadId ();
		}

		internal long Assembly_GetObject (long id) {
			return SendReceive (CommandSet.ASSEMBLY, (int)CmdAssembly.GET_OBJECT, new PacketWriter ().WriteId (id)).ReadId ();
		}

		internal long Assembly_GetType (long id, string name, bool ignoreCase) {
			return SendReceive (CommandSet.ASSEMBLY, (int)CmdAssembly.GET_TYPE, new PacketWriter ().WriteId (id).WriteString (name).WriteBool (ignoreCase)).ReadId ();
		}

		internal string Assembly_GetName (long id) {
			return SendReceive (CommandSet.ASSEMBLY, (int)CmdAssembly.GET_NAME, new PacketWriter ().WriteId (id)).ReadString ();
		}

		internal long Assembly_GetIdDomain (long id) {
			return SendReceive (CommandSet.ASSEMBLY, (int)CmdAssembly.GET_DOMAIN, new PacketWriter ().WriteId (id)).ReadId ();
		}
		
		internal byte[] Assembly_GetMetadataBlob (long id) {
			return SendReceive (CommandSet.ASSEMBLY, (int)CmdAssembly.GET_METADATA_BLOB, new PacketWriter ().WriteId (id)).ReadByteArray ();
		}

		internal bool Assembly_IsDynamic (long id) {
			return SendReceive (CommandSet.ASSEMBLY, (int) CmdAssembly.GET_IS_DYNAMIC, new PacketWriter ().WriteId (id)).ReadBool ();
		}
		
		internal byte[] Assembly_GetPdbBlob (long id) {
			return SendReceive (CommandSet.ASSEMBLY, (int)CmdAssembly.GET_PDB_BLOB, new PacketWriter ().WriteId (id)).ReadByteArray ();
		}

		internal long Assembly_GetType (long id, uint token) {
			return SendReceive (CommandSet.ASSEMBLY, (int)CmdAssembly.GET_TYPE_FROM_TOKEN, new PacketWriter ().WriteId (id).WriteInt ((int)token)).ReadId ();
		}

		internal long Assembly_GetMethod (long id, uint token) {
			return SendReceive (CommandSet.ASSEMBLY, (int)CmdAssembly.GET_METHOD_FROM_TOKEN, new PacketWriter ().WriteId (id).WriteInt ((int)token)).ReadId ();
		}

		internal bool Assembly_HasDebugInfo (long id) {
			return SendReceive (CommandSet.ASSEMBLY, (int)CmdAssembly.HAS_DEBUG_INFO, new PacketWriter ().WriteId (id)).ReadBool ();
		}

		internal CattrInfo[] Assembly_GetCustomAttributes (long id, long attr_type_id) {
			PacketReader r = SendReceive (CommandSet.ASSEMBLY, (int)CmdAssembly.GET_CATTRS, new PacketWriter ().WriteId (id).WriteId (attr_type_id));
			return ReadCattrs (r);
		}

		/*
		 * TYPE
		 */

		internal TypeInfo Type_GetInfo (long id) {
			PacketReader r = SendReceive (CommandSet.TYPE, (int)CmdType.GET_INFO, new PacketWriter ().WriteId (id));
			TypeInfo res = new TypeInfo ();

			res.ns = r.ReadString ();
			res.name = r.ReadString ();
			res.full_name = r.ReadString ();
			res.assembly = r.ReadId ();
			res.module = r.ReadId ();
			res.base_type = r.ReadId ();
			res.element_type = r.ReadId ();
			res.token = r.ReadInt ();
			res.rank = r.ReadByte ();
			res.attributes = r.ReadInt ();
			int b = r.ReadByte ();
			res.is_byref = (b & 1) != 0;
			res.is_pointer = (b & 2) != 0;
			res.is_primitive = (b & 4) != 0;
			res.is_valuetype = (b & 8) != 0;
			res.is_enum = (b & 16) != 0;
			res.is_gtd = (b & 32) != 0;
			res.is_generic_type = (b & 64) != 0;

			int nested_len = r.ReadInt ();
			res.nested = new long [nested_len];
			for (int i = 0; i < nested_len; ++i)
				res.nested [i] = r.ReadId ();

			if (Version.AtLeast (2, 12))
				res.gtd = r.ReadId ();
			if (Version.AtLeast (2, 15) && res.is_generic_type) {
				int n = r.ReadInt ();
				res.type_args = r.ReadIds (n);
			}

			return res;
		}

		internal long[] Type_GetMethods (long id) {
			PacketReader r = SendReceive (CommandSet.TYPE, (int)CmdType.GET_METHODS, new PacketWriter ().WriteId (id));

			int n = r.ReadInt ();
			long[] res = new long [n];
			for (int i = 0; i < n; ++i)
				res [i] = r.ReadId ();
			return res;
		}

		internal long[] Type_GetFields (long id, out string[] names, out long[] types, out int[] attrs) {
			PacketReader r = SendReceive (CommandSet.TYPE, (int)CmdType.GET_FIELDS, new PacketWriter ().WriteId (id));

			int n = r.ReadInt ();
			long[] res = new long [n];
			names = new string [n];
			types = new long [n];
			attrs = new int [n];
			for (int i = 0; i < n; ++i) {
				res [i] = r.ReadId ();
				names [i] = r.ReadString ();
				types [i] = r.ReadId ();
				attrs [i] = r.ReadInt ();
			}
			return res;
		}

		internal PropInfo[] Type_GetProperties (long id) {
			PacketReader r = SendReceive (CommandSet.TYPE, (int)CmdType.GET_PROPERTIES, new PacketWriter ().WriteId (id));

			int n = r.ReadInt ();
			PropInfo[] res = new PropInfo [n];
			for (int i = 0; i < n; ++i) {
				res [i] = new PropInfo ();
				res [i].id = r.ReadId ();
				res [i].name = r.ReadString ();
				res [i].get_method = r.ReadId ();
				res [i].set_method = r.ReadId ();
				res [i].attrs = r.ReadInt ();
			}

			return res;
		}

		internal long Type_GetObject (long id) {
			return SendReceive (CommandSet.TYPE, (int)CmdType.GET_OBJECT, new PacketWriter ().WriteId (id)).ReadId ();
		}

		internal ValueImpl[] Type_GetValues (long id, long[] fields, long thread_id) {
			int len = fields.Length;
			PacketReader r;
			if (thread_id != 0 && Version.AtLeast(2, 3))
				r = SendReceive (CommandSet.TYPE, (int)CmdType.GET_VALUES_2, new PacketWriter ().WriteId (id).WriteId (thread_id).WriteInt (len).WriteIds (fields));
			else
				r = SendReceive (CommandSet.TYPE, (int)CmdType.GET_VALUES, new PacketWriter ().WriteId (id).WriteInt (len).WriteIds (fields));

			ValueImpl[] res = new ValueImpl [len];
			for (int i = 0; i < len; ++i)
				res [i] = r.ReadValue ();
			return res;
		}			

		internal void Type_SetValues (long id, long[] fields, ValueImpl[] values) {
			SendReceive (CommandSet.TYPE, (int)CmdType.SET_VALUES, new PacketWriter ().WriteId (id).WriteInt (fields.Length).WriteIds (fields).WriteValues (values));
		}

		internal string[] Type_GetSourceFiles (long id, bool return_full_paths) {
			var r = SendReceive (CommandSet.TYPE, return_full_paths ? (int)CmdType.GET_SOURCE_FILES_2 : (int)CmdType.GET_SOURCE_FILES, new PacketWriter ().WriteId (id));
			int len = r.ReadInt ();
			string[] res = new string [len];
			for (int i = 0; i < len; ++i)
				res [i] = r.ReadString ();
			return res;
		}

		internal bool Type_IsAssignableFrom (long id, long c_id) {
			return SendReceive (CommandSet.TYPE, (int)CmdType.IS_ASSIGNABLE_FROM, new PacketWriter ().WriteId (id).WriteId (c_id)).ReadByte () > 0;
		}

		internal CattrInfo[] Type_GetCustomAttributes (long id, long attr_type_id, bool inherit) {
			PacketReader r = SendReceive (CommandSet.TYPE, (int)CmdType.GET_CATTRS, new PacketWriter ().WriteId (id).WriteId (attr_type_id));
			return ReadCattrs (r);
		}

		internal CattrInfo[] Type_GetFieldCustomAttributes (long id, long field_id, long attr_type_id, bool inherit) {
			PacketReader r = SendReceive (CommandSet.TYPE, (int)CmdType.GET_FIELD_CATTRS, new PacketWriter ().WriteId (id).WriteId (field_id).WriteId (attr_type_id));
			return ReadCattrs (r);
		}

		internal CattrInfo[] Type_GetPropertyCustomAttributes (long id, long field_id, long attr_type_id, bool inherit) {
			PacketReader r = SendReceive (CommandSet.TYPE, (int)CmdType.GET_PROPERTY_CATTRS, new PacketWriter ().WriteId (id).WriteId (field_id).WriteId (attr_type_id));
			return ReadCattrs (r);
		}

		public long[] Type_GetMethodsByNameFlags (long id, string name, int flags, bool ignoreCase) {
			flags |= ignoreCase ? (int)BindingFlagsExtensions.BINDING_FLAGS_IGNORE_CASE : 0;
			int listType = ignoreCase ? (int)MemberListTypeExtensions.CaseInsensitive : (int)MemberListTypeExtensions.CaseSensitive;
			var w = new PacketWriter ().WriteId (id).WriteString (name).WriteInt (flags);
			if (Version.AtLeast (2, 48))
				w.WriteInt (listType);
			PacketReader r = SendReceive (CommandSet.TYPE, (int)CmdType.CMD_TYPE_GET_METHODS_BY_NAME_FLAGS, w);
			int len = r.ReadInt ();
			long[] res = new long [len];
			for (int i = 0; i < len; ++i)
				res [i] = r.ReadId ();
			return res;
		}

		internal long[] Type_GetInterfaces (long id) {
			PacketReader r = SendReceive (CommandSet.TYPE, (int)CmdType.GET_INTERFACES, new PacketWriter ().WriteId (id));
			int len = r.ReadInt ();
			return r.ReadIds (len);
		}

		internal IfaceMapInfo[] Type_GetInterfaceMap (long id, long[] ids) {
			PacketReader r = SendReceive (CommandSet.TYPE, (int)CmdType.GET_INTERFACE_MAP, new PacketWriter ().WriteId (id).WriteInt (ids.Length).WriteIds (ids));
			var res = new IfaceMapInfo [ids.Length];
			for (int i = 0; i < ids.Length; ++i) {
				int n = r.ReadInt ();

				res [i].iface_id = ids [i];
				res [i].iface_methods = r.ReadIds (n);
				res [i].target_methods = r.ReadIds (n);
			}

			return res;
		}

		internal bool Type_IsInitialized (long id) {
			PacketReader r = SendReceive (CommandSet.TYPE, (int)CmdType.IS_INITIALIZED, new PacketWriter ().WriteId (id));
			return r.ReadInt () == 1;
		}

		internal long Type_CreateInstance (long id) {
			PacketReader r = SendReceive (CommandSet.TYPE, (int)CmdType.CREATE_INSTANCE, new PacketWriter ().WriteId (id));
			return r.ReadId ();
		}

		internal int Type_GetValueSize (long id) {
			PacketReader r = SendReceive (CommandSet.TYPE, (int)CmdType.GET_VALUE_SIZE, new PacketWriter ().WriteId (id));
			return r.ReadInt ();
		}

		/*
		 * FIELD
		 */

		internal FieldMirrorInfo Field_GetInfo (long id) {
			PacketReader r = SendReceive (CommandSet.FIELD, (int)CmdField.GET_INFO, new PacketWriter ().WriteId (id));
			FieldMirrorInfo info = new FieldMirrorInfo { Name = r.ReadString (), Parent = r.ReadId (), TypeId = r.ReadId (), Attrs = r.ReadInt () };
			return info;
		}

		/*
		 * EVENTS
		 */

		internal int EnableEvent (EventType etype, SuspendPolicy suspend_policy, List<Modifier> mods) {
			var w = new PacketWriter ().WriteByte ((byte)etype).WriteByte ((byte)suspend_policy);
			if (mods != null) {
				if (mods.Count > 255)
					throw new NotImplementedException ();
				w.WriteByte ((byte)mods.Count);
				foreach (Modifier mod in mods) {
					if (mod is CountModifier) {
						w.WriteByte ((byte)ModifierKind.COUNT);
						w.WriteInt ((mod as CountModifier).Count);
					} else if (mod is LocationModifier) {
						w.WriteByte ((byte)ModifierKind.LOCATION_ONLY);
						w.WriteId ((mod as LocationModifier).Method);
						w.WriteLong ((mod as LocationModifier).Location);
					} else if (mod is StepModifier) {
						w.WriteByte ((byte)ModifierKind.STEP);
						w.WriteId ((mod as StepModifier).Thread);
						w.WriteInt ((mod as StepModifier).Size);
						w.WriteInt ((mod as StepModifier).Depth);
						if (Version.AtLeast (2, 16))
							w.WriteInt ((mod as StepModifier).Filter);
					} else if (mod is ThreadModifier) {
						w.WriteByte ((byte)ModifierKind.THREAD_ONLY);
						w.WriteId ((mod as ThreadModifier).Thread);
					} else if (mod is ExceptionModifier) {
						var em = mod as ExceptionModifier;
						w.WriteByte ((byte)ModifierKind.EXCEPTION_ONLY);
						w.WriteId (em.Type);
						if (Version.MajorVersion > 2 || Version.MinorVersion > 0) {
							/* This is only supported in protocol version 2.1 */
							w.WriteBool (em.Caught);
							w.WriteBool (em.Uncaught);
						} else if (!em.Caught || !em.Uncaught) {
							throw new NotSupportedException ("This request is not supported by the protocol version implemented by the debuggee.");
						}
						if (Version.MajorVersion > 2 || Version.MinorVersion > 24) {
							w.WriteBool (em.Subclasses);
						} else if (!em.Subclasses) {
							throw new NotSupportedException ("This request is not supported by the protocol version implemented by the debuggee.");
						}
						if (Version.MajorVersion > 2 || Version.MinorVersion >= 54) {
							w.WriteBool (em.NotFilteredFeature);
							w.WriteBool (em.EverythingElse);
						}
					} else if (mod is AssemblyModifier) {
						w.WriteByte ((byte)ModifierKind.ASSEMBLY_ONLY);
						var amod = (mod as AssemblyModifier);
						w.WriteInt (amod.Assemblies.Length);
						foreach (var id in amod.Assemblies)
							w.WriteId (id);
					} else if (mod is SourceFileModifier) {
						w.WriteByte ((byte)ModifierKind.SOURCE_FILE_ONLY);
						var smod = (mod as SourceFileModifier);
						w.WriteInt (smod.SourceFiles.Length);
						foreach (var s in smod.SourceFiles)
							w.WriteString (s);
					} else if (mod is TypeNameModifier) {
						w.WriteByte ((byte)ModifierKind.TYPE_NAME_ONLY);
						var tmod = (mod as TypeNameModifier);
						w.WriteInt (tmod.TypeNames.Length);
						foreach (var s in tmod.TypeNames)
							w.WriteString (s);
					} else {
						throw new NotImplementedException ();
					}
				}
			} else {
				w.WriteByte (0);
			}
			return SendReceive (CommandSet.EVENT_REQUEST, (int)CmdEventRequest.SET, w).ReadInt ();
		}

		internal void ClearEventRequest (EventType etype, int req_id) {
			SendReceive (CommandSet.EVENT_REQUEST, (int)CmdEventRequest.CLEAR, new PacketWriter ().WriteByte ((byte)etype).WriteInt (req_id));
		}			

		internal void ClearAllBreakpoints () {
			SendReceive (CommandSet.EVENT_REQUEST, (int)CmdEventRequest.CLEAR_ALL_BREAKPOINTS, new PacketWriter ());
		}
			
		/*
		 * STACK FRAME
		 */
		internal ValueImpl StackFrame_GetThis (long thread_id, long id) {
			PacketReader r = SendReceive (CommandSet.STACK_FRAME, (int)CmdStackFrame.GET_THIS, new PacketWriter ().WriteId (thread_id).WriteId (id));
			return r.ReadValue ();
		}

		internal ValueImpl[] StackFrame_GetValues (long thread_id, long id, int[] pos) {
			/* pos < 0 -> argument at pos (-pos) - 1 */
			/* pos >= 0 -> local at pos */
			int len = pos.Length;
			PacketReader r = SendReceive (CommandSet.STACK_FRAME, (int)CmdStackFrame.GET_VALUES, new PacketWriter ().WriteId (thread_id).WriteId (id).WriteInt (len).WriteInts (pos));

			ValueImpl[] res = new ValueImpl [len];
			for (int i = 0; i < len; ++i)
				res [i] = r.ReadValue ();
			return res;
		}

		internal void StackFrame_SetValues (long thread_id, long id, int[] pos, ValueImpl[] values) {
			/* pos < 0 -> argument at pos (-pos) - 1 */
			/* pos >= 0 -> local at pos */
			int len = pos.Length;
			SendReceive (CommandSet.STACK_FRAME, (int)CmdStackFrame.SET_VALUES, new PacketWriter ().WriteId (thread_id).WriteId (id).WriteInt (len).WriteInts (pos).WriteValues (values));
		}

		internal long StackFrame_GetDomain (long thread_id, long id) {
			return SendReceive (CommandSet.STACK_FRAME, (int)CmdStackFrame.GET_DOMAIN, new PacketWriter ().WriteId (thread_id).WriteId (id)).ReadId ();
		}

		internal void StackFrame_SetThis (long thread_id, long id, ValueImpl value) {
			SendReceive (CommandSet.STACK_FRAME, (int)CmdStackFrame.SET_THIS, new PacketWriter ().WriteId (thread_id).WriteId (id).WriteValue (value));
		}

		/*
		 * ARRAYS
		 */
		internal int[] Array_GetLength (long id, out int rank, out int[] lower_bounds) {
			var r = SendReceive (CommandSet.ARRAY_REF, (int)CmdArrayRef.GET_LENGTH, new PacketWriter ().WriteId (id));
			rank = r.ReadInt ();
			int[] res = new int [rank];
			lower_bounds = new int [rank];
			for (int i = 0; i < rank; ++i) {
				res [i] = r.ReadInt ();
				lower_bounds [i] = r.ReadInt ();
			}
			return res;
		}

		internal ValueImpl[] Array_GetValues (long id, int index, int len) {
			var r = SendReceive (CommandSet.ARRAY_REF, (int)CmdArrayRef.GET_VALUES, new PacketWriter ().WriteId (id).WriteInt (index).WriteInt (len));
			ValueImpl[] res = new ValueImpl [len];
			for (int i = 0; i < len; ++i)
				res [i] = r.ReadValue ();
			return res;
		}

		internal void Array_SetValues (long id, int index, ValueImpl[] values) {
			SendReceive (CommandSet.ARRAY_REF, (int)CmdArrayRef.SET_VALUES, new PacketWriter ().WriteId (id).WriteInt (index).WriteInt (values.Length).WriteValues (values));
		}

		// This is a special case when setting values of an array that
		// consists of a large number of bytes. This saves much time and
		// cost than we create ValueImpl object for each byte.
		internal void ByteArray_SetValues (long id, byte [] bytes)
		{
			int index = 0;
			var typ = (byte)ElementType.U1;
			var w = new PacketWriter ().WriteId (id).WriteInt (index).WriteInt (bytes.Length);
			for (int i = 0; i < bytes.Length; i++) {
				w.WriteByte (typ);
				w.WriteInt (bytes [i]);
			}
			SendReceive (CommandSet.ARRAY_REF, (int)CmdArrayRef.SET_VALUES, w);
		}

		/*
		 * STRINGS
		 */
		internal string String_GetValue (long id) {
			var r = SendReceive (CommandSet.STRING_REF, (int)CmdStringRef.GET_VALUE, new PacketWriter ().WriteId (id));

			bool is_utf16 = false;
			if (Version.AtLeast (2, 41))
				is_utf16 = r.ReadByte () == 1;

			if (is_utf16)
				return r.ReadUTF16String ();
			else
				return r.ReadString ();
		}			

		internal int String_GetLength (long id) {
			return (int)SendReceive (CommandSet.STRING_REF, (int)CmdStringRef.GET_LENGTH, new PacketWriter ().WriteId (id)).ReadLong ();
		}			

		internal char[] String_GetChars (long id, int index, int length) {
			var r = SendReceive (CommandSet.STRING_REF, (int)CmdStringRef.GET_CHARS, new PacketWriter ().WriteId (id).WriteLong (index).WriteLong (length));
			var res = new char [length];
			for (int i = 0; i < length; ++i)
				res [i] = (char)r.ReadShort ();
			return res;
		}

		/*
		 * POINTERS
		 */

		internal ValueImpl Pointer_GetValue (long address, TypeMirror type)
		{
			return SendReceive (CommandSet.POINTER, (int)CmdPointer.GET_VALUE, new PacketWriter ().WriteLong (address).WriteId (type.Id)).ReadValue ();
		}

		/*
		 * OBJECTS
		 */
		internal long Object_GetType (long id) {
			return SendReceive (CommandSet.OBJECT_REF, (int)CmdObjectRef.GET_TYPE, new PacketWriter ().WriteId (id)).ReadId ();
		}			

		internal long Object_GetDomain (long id) {
			return SendReceive (CommandSet.OBJECT_REF, (int)CmdObjectRef.GET_DOMAIN, new PacketWriter ().WriteId (id)).ReadId ();
		}			

		internal ValueImpl[] Object_GetValues (long id, long[] fields) {
			int len = fields.Length;
			PacketReader r = SendReceive (CommandSet.OBJECT_REF, (int)CmdObjectRef.GET_VALUES, new PacketWriter ().WriteId (id).WriteInt (len).WriteIds (fields));

			ValueImpl[] res = new ValueImpl [len];
			for (int i = 0; i < len; ++i)
				res [i] = r.ReadValue ();
			return res;
		}

		internal void Object_SetValues (long id, long[] fields, ValueImpl[] values) {
			SendReceive (CommandSet.OBJECT_REF, (int)CmdObjectRef.SET_VALUES, new PacketWriter ().WriteId (id).WriteInt (fields.Length).WriteIds (fields).WriteValues (values));
		}

		internal bool Object_IsCollected (long id) {
			return SendReceive (CommandSet.OBJECT_REF, (int)CmdObjectRef.IS_COLLECTED, new PacketWriter ().WriteId (id)).ReadInt () == 1;
		}			

		internal long Object_GetAddress (long id) {
			return SendReceive (CommandSet.OBJECT_REF, (int)CmdObjectRef.GET_ADDRESS, new PacketWriter ().WriteId (id)).ReadLong ();
		}			

		internal ObjectRefInfo Object_GetInfo (long id) {
			ObjectRefInfo res = new ObjectRefInfo ();
			PacketReader r = SendReceive (CommandSet.OBJECT_REF, (int)CmdObjectRef.GET_INFO, new PacketWriter ().WriteId (id));

			res.type_id = r.ReadId ();
			res.domain_id = r.ReadId ();
			return res;
		}

		public void ForceDisconnect ()
		{
			closed = true;
			disconnected = true;
			DisconnectedEvent.Set ();
			TransportClose ();
		}
	}
	
	class TcpConnection : Connection
	{
		Socket socket;
		
		internal TcpConnection (Socket socket)
		{
			this.socket = socket;
			//socket.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.NoDelay, 1);
		}
		
		internal EndPoint EndPoint {
			get {
				return socket.RemoteEndPoint;
			}
		}
		
		protected override int TransportSend (byte[] buf, int buf_offset, int len)
		{
			return socket.Send (buf, buf_offset, len, SocketFlags.None);
		}
		
		protected override int TransportReceive (byte[] buf, int buf_offset, int len)
		{
			return socket.Receive (buf, buf_offset, len, SocketFlags.None);
		}
		
		protected override void TransportSetTimeouts (int send_timeout, int receive_timeout)
		{
			socket.SendTimeout = send_timeout;
			socket.ReceiveTimeout = receive_timeout;
		}
		
		protected override void TransportClose ()
		{
			socket.Close ();
		}

		protected override void TransportShutdown ()
		{
			socket.Shutdown (SocketShutdown.Both);
		}
	}

	/* This is the interface exposed by the debugger towards the debugger agent */
	interface IEventHandler
	{
		void Events (SuspendPolicy suspend_policy, EventInfo[] events);

		void VMDisconnect (int req_id, long thread_id, string vm_uri);
	}
}
