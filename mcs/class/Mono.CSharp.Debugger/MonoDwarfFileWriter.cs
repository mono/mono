//
// System.Diagnostics.SymbolStore/MonoDwarfWriter.cs
//
// Author:
//   Martin Baulig (martin@gnome.org)
//
// This is the default implementation of the System.Diagnostics.SymbolStore.ISymbolWriter
// interface.
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Diagnostics.SymbolStore;
using System.Collections;
using System.Text;
using System.IO;
	
namespace Mono.CSharp.Debugger
{
	public class DwarfFileWriter
	{
		protected const string producer_id = "Mono C# Compiler 0.01 03-18-2002";

		protected ArrayList compile_units = new ArrayList ();
		protected ArrayList line_number_engines = new ArrayList ();
		protected StreamWriter writer = null;
		protected IAssemblerWriter aw = null;
		protected string symbol_file = null;

		public bool timestamps = false;
		public bool use_gnu_extensions = false;

		// Write a generic file which contains no machine dependant stuff but
		// only function and type declarations.
		protected readonly bool DoGeneric = false;

		public readonly TypeHandle void_type;
		public readonly TypeHandle int_type;
		public readonly TypeHandle char_type;
		public readonly TypeHandle array_type;
		public readonly TypeHandle array_bounds_type;
		public readonly TypeHandle string_type;
		public readonly DieCompileUnit DieGlobalCompileUnit;

		//
		// DwarfFileWriter public interface
		//
		public DwarfFileWriter (string symbol_file, string[] args)
		{
			foreach (string arg in args) {
				if (arg.StartsWith ("output="))
					symbol_file = arg.Substring (7);
				else if (arg == "timestamp")
					timestamps = true;
				else if (arg == "gnu_extensions")
					use_gnu_extensions = true;
				else if (arg == "generic")
					DoGeneric = true;
				else
					Console.WriteLine ("Symbol writer warning: Unknown argument: " + arg);
			}

			this.symbol_file = symbol_file;
			this.writer = new StreamWriter (symbol_file, false, Encoding.ASCII);
			this.aw = new AssemblerWriterI386 (this.writer);
			this.last_time = DateTime.Now;

			CompileUnit compile_unit = new CompileUnit (this, symbol_file);
			DieGlobalCompileUnit = new DieCompileUnit (compile_unit);

			void_type = RegisterType (typeof (void));
			RegisterType (typeof (bool));
			char_type = RegisterType (typeof (char));
			RegisterType (typeof (SByte));
			RegisterType (typeof (Byte));
			RegisterType (typeof (Int16));
			RegisterType (typeof (UInt16));
			int_type = RegisterType (typeof (Int32));
			RegisterType (typeof (UInt32));
			RegisterType (typeof (Int64));
			RegisterType (typeof (UInt64));
			RegisterType (typeof (Single));
			RegisterType (typeof (Double));
			if (!use_gnu_extensions) {
				array_type = RegisterType (typeof (MonoArray));
				array_bounds_type = RegisterType (typeof (MonoArrayBounds));
				string_type = RegisterType (typeof (MonoString));
			}
		}

		DateTime last_time;
		void ShowTime (string msg)
		{
			DateTime now = DateTime.Now;
			TimeSpan span = now - last_time;
			last_time = now;

			Console.WriteLine (
				"MonoDwarfFileWriter [{0:00}:{1:000}] {2}",
				(int) span.TotalSeconds, span.Milliseconds, msg);
		}

		public void CreateTypes ()
		{
			types_closed = true;
		}

		// Writes the final dwarf file.
		public void Close ()
		{
			CreateTypes ();

			if (timestamps)
				ShowTime ("Emitting compile units");

			foreach (CompileUnit compile_unit in compile_units)
				compile_unit.Emit ();

			if (timestamps)
				ShowTime ("Done");

			foreach (LineNumberEngine line_number_engine in line_number_engines)
				line_number_engine.Emit ();

			if (timestamps)
				ShowTime ("Done emitting " + LineNumberEngine.count + " line numbers");

			WriteAbbrevDeclarations ();
			if (timestamps)
				ShowTime ("Done writing abbrev declarations");

			WriteRelocEntries ();
			if (timestamps)
				ShowTime ("Done writing " + reloc_entries.Count + " reloc entries");

			writer.Close ();
		}

		// Adds a new compile unit to this dwarf file
		public void AddCompileUnit (CompileUnit compile_unit)
		{
			compile_units.Add (compile_unit);
		}

		// Adds a new line number engine to this dwarf file
		public void AddLineNumberEngine (LineNumberEngine line_number_engine)
		{
			line_number_engines.Add (line_number_engine);
		}

		public IAssemblerWriter AssemblerWriter {
			get {
				return aw;
			}
		}

		// This string is written into the generated dwarf file to identify the
		// producer and version number.
		public string ProducerID {
			get {
				return producer_id;
			}
		}

		private Hashtable type_hash = new Hashtable ();
		private bool types_closed = false;

		public TypeHandle RegisterType (Type type)
		{
			if (types_closed)
				throw new InvalidOperationException ();

			if (type_hash.Contains (type))
				return (TypeHandle) type_hash [type];

			TypeHandle handle = new TypeHandle (type);
			type_hash.Add (type, handle);

			if (!use_gnu_extensions) {
				if (type.IsArray && (type != typeof (Array)))
					handle.CreateArrayType (RegisterType (type.GetElementType ()));
				else if (type.Equals (typeof (MonoString)))
					handle.CreateStringType (char_type);
			} else {
				if (type.IsArray & (type != typeof (Array)))
					handle.ArrayElementType = RegisterType (type.GetElementType ());
			}

			handle.CreateType (DieGlobalCompileUnit);

			return handle;
		}

		//
		// This is used to reference types.
		//

		public class TypeHandle : ITypeHandle
		{
			protected static ArrayList external_types = new ArrayList ();

			protected readonly Type type;
			protected DieType type_die;
			protected int token;
			protected Die pointer_die;
			protected TypeHandle array_bounds_type;
			protected TypeHandle array_vector_type;
			protected TypeHandle array_element_type;

			public TypeHandle (Type type)
			{
				this.type = type;
			}

			public void CreateArrayType (TypeHandle element_type)
			{
				this.array_element_type = element_type;
				this.array_bounds_type = new TypeHandle_ArrayBounds (type.GetArrayRank ());
				this.array_vector_type = new TypeHandle_ArrayVector (array_element_type);
			}

			public void CreateStringType (TypeHandle element_type)
			{
				this.array_element_type = element_type;
				this.array_vector_type = new TypeHandle_ArrayVector (element_type);
			}

			public string Name {
				get {
					return type.FullName;
				}
			}

			public Type Type {
				get {
					return type;
				}
			}

			public int Token {
				get {
					if ((token == 0) && !type.Equals (typeof (object)))
						throw new InvalidOperationException ();

					return token;
				}
			}

			public DieType TypeDie {
				get {
					if (type_die == null)
						throw new InvalidOperationException ();
					else
						return type_die;
				}
			}

			public ITypeHandle ArrayBoundsType {
				get {
					if (array_bounds_type == null)
						throw new InvalidOperationException ();
					else
						return array_bounds_type;
				}
			}

			public ITypeHandle ArrayElementType {
				get {
					if (array_element_type == null)
						throw new InvalidOperationException ();
					else
						return array_element_type;
				}
				set {
					array_element_type = (TypeHandle) value;
				}
			}

			public ITypeHandle ArrayVectorType {
				get {
					if (array_vector_type == null)
						throw new InvalidOperationException ("FUCK: " + type);
					else
						return array_vector_type;
				}
			}

			public Die PointerDie {
				get {
					return pointer_die;
				}
			}

			public static Type[] ExternalTypes {
				get {
					Type[] retval = new Type [external_types.Count];
					external_types.CopyTo (retval, 0);
					return retval;
				}
			}

			public virtual void CreateType (DieCompileUnit parent_die)
			{
				if (type_die != null)
					return;

				if (array_bounds_type != null)
					array_bounds_type.CreateType (parent_die);
				if (array_element_type != null)
					array_element_type.CreateType (parent_die);
				if (array_vector_type != null)
					array_vector_type.CreateType (parent_die);

				ITypeHandle void_type = parent_die.Writer.void_type;

				if ((type.IsPrimitive && !type.IsByRef) || (type == typeof (void))) {
					type_die = new DieBaseType (parent_die, this);
					return;
				} else if (type.Equals (typeof (string))) {
					if (parent_die.Writer.use_gnu_extensions)
						type_die = new DieMonoStringType (parent_die);
					else
						type_die = new DieStringType (parent_die, this);
					pointer_die = new DieInternalPointer (parent_die, type_die);
					type_die.CreateType ();
					return;
				} else if (type.IsArray && (type != typeof (Array))) {
					if (parent_die.Writer.use_gnu_extensions)
						type_die = new DieMonoArrayType (parent_die, this);
					else
						type_die = new DieArrayType (parent_die, this);
					pointer_die = new DieInternalPointer (parent_die, type_die);
					type_die.CreateType ();
					return;
				} else if (type.Equals (typeof (MonoArrayBounds))) {
					type_die = new DieArrayBoundsType (parent_die, this);
					pointer_die = new DieInternalPointer (parent_die, type_die);
					type_die.CreateType ();
					return;
				} else if (type.IsPointer || type.IsByRef) {
					type_die = new DiePointerType (parent_die, type);
					type_die.CreateType ();
					return;
				}

				if (type is TypeBuilder)
					token = ((TypeBuilder) type).TypeToken.Token;
				else if (!type.Equals (typeof (object)))
					token = external_types.Add (type) + 1;

				if (type.IsPointer || type.IsByRef)
					type_die = new DiePointerType (parent_die, type);
				else if (type.IsEnum)
					type_die = new DieEnumType (parent_die, this);
				else if (type.IsValueType) {
					type_die = new DieStructureType (parent_die, this);
					new DieInternalTypeDef (parent_die, type_die, type.FullName);
				} else if (type.IsClass) {
					type_die = new DieClassType (parent_die, this);
					pointer_die = new DieInternalPointer (parent_die, type_die);
					new DieInternalTypeDef (parent_die, type_die, type.FullName);
				} else
					type_die = new DieTypeDef (parent_die, void_type, type.FullName);

				type_die.CreateType ();
			}
		}

		public class TypeHandle_ArrayBounds : TypeHandle
		{
			private readonly int rank;

			public TypeHandle_ArrayBounds (int rank)
				: base (typeof (MonoArrayBounds))
			{
				this.rank = rank;
			}

			public override void CreateType (DieCompileUnit parent_die)
			{
				ITypeHandle int_type = parent_die.Writer.int_type;
				ITypeHandle array_bounds_type = parent_die.Writer.array_bounds_type;

				type_die = new DieInternalArray (parent_die, array_bounds_type);
				new DieSubRangeType (type_die, int_type, 0, rank);

				type_die.CreateType ();
			}
		}

		public class TypeHandle_ArrayVector : TypeHandle
		{
			TypeHandle element_type;

			public TypeHandle_ArrayVector (TypeHandle element_type)
				: base (element_type.Type)
			{
				this.element_type = element_type;
			}

			public override void CreateType (DieCompileUnit parent_die)
			{
				ITypeHandle int_type = parent_die.Writer.int_type;

				type_die = new DieInternalArray (parent_die, element_type);
				new DieSubRangeType (type_die, int_type, 0, -1);

				type_die.CreateType ();
			}
		}

		//
		// A compile unit refers to a single C# source file.
		//
		public class CompileUnit
		{
			protected DwarfFileWriter dw;
			protected IAssemblerWriter aw;
			protected string source_file;
			protected ArrayList dies = new ArrayList ();

			public readonly int ReferenceIndex;

			public CompileUnit (DwarfFileWriter dw, string source_file, Die[] dies)
			{
				this.dw = dw;
				this.aw = dw.AssemblerWriter;
				this.source_file = source_file;
				if (dies != null)
					this.dies.AddRange (dies);

				this.ReferenceIndex = this.aw.GetNextLabelIndex ();

				dw.AddCompileUnit (this);
			}

			//
			// Construct a new compile unit for source file @source_file.
			//
			// This constructor automatically adds the newly created compile
			// unit to the DwarfFileWriter's list of compile units.
			//
			public CompileUnit (DwarfFileWriter dw, string source_file)
				: this (dw, source_file, null)
			{ }

			public string SourceFile {
				get {
					return source_file;
				}
			}

			public string ProducerID {
				get {
					return dw.ProducerID;
				}
			}

			public DwarfFileWriter DwarfFileWriter
			{
				get {
					return dw;
				}
			}

			// Add a new debugging information entry to this compile unit.
			public void AddDie (Die die)
			{
				dies.Add (die);
			}

			// Write the whole compile unit to the dwarf file.
			public void Emit ()
			{
				object start_index, end_index;

				dw.WriteSectionStart (Section.DEBUG_INFO);

				aw.WriteLabel (ReferenceIndex);

				start_index = aw.WriteLabel ();

				end_index = aw.StartSubsectionWithSize ();
				aw.WriteUInt16 (2);
				aw.WriteAbsoluteOffset ("debug_abbrev_b");
				if (dw.DoGeneric)
					aw.WriteUInt8 (4);
				else {
					dw.AddRelocEntry (RelocEntryType.TARGET_ADDRESS_SIZE);
					aw.WriteUInt8 (4);
				}

				if (dies != null)
					foreach (Die die in dies)
						die.Emit ();

				aw.EndSubsection (end_index);

				aw.WriteSectionEnd ();
			}
		}

		public class LineNumberEngine
		{
			public readonly int ReferenceIndex;

			public readonly int LineBase = 1;
			public readonly int LineRange = 8;

			protected DwarfFileWriter dw;
			protected IAssemblerWriter aw;

			public readonly int[] StandardOpcodeSizes = {
				0, 0, 1, 1, 1, 1, 0, 0, 0, 0
			};

			public readonly int OpcodeBase;

			public static int count = 0;

			private Hashtable _sources = new Hashtable ();
			private Hashtable _directories = new Hashtable ();
			private Hashtable _methods = new Hashtable ();

			private int next_source_id;
			private int next_directory_id;

			private int next_method_id;

			private enum DW_LNS {
				LNS_extended_op		= 0,
				LNS_copy		= 1,
				LNS_advance_pc		= 2,
				LNS_advance_line	= 3,
				LNS_set_file		= 4,
				LNS_set_column		= 5,
				LNS_negate_stmt		= 6,
				LNS_set_basic_block	= 7,
				LNS_const_add_pc	= 8,
				LNS_fixed_advance_pc	= 9
			};

			private enum DW_LNE {
				LNE_end_sequence	= 1,
				LNE_set_address		= 2,
				LNE_define_file		= 3
			};

			public ISourceFile[] Sources {
				get {
					ISourceFile[] retval = new ISourceFile [_sources.Count];

					foreach (ISourceFile source in _sources.Keys)
						retval [(int) _sources [source] - 1] = source;

					return retval;
				}
			}

			public string[] Directories {
				get {
					string[] retval = new string [_directories.Count];

					foreach (string directory in _directories.Keys)
						retval [(int) _directories [directory] - 1] = directory;

					return retval;
				}
			}

			public ISourceMethod[] Methods {
				get {
					ISourceMethod[] retval = new ISourceMethod [_methods.Count];

					foreach (ISourceMethod method in _methods.Keys) {
						retval [(int) _methods [method] - 1] = method;
					}

					return retval;
				}
			}
			
			public LineNumberEngine (DwarfFileWriter writer)
			{
				this.dw = writer;
				this.aw = writer.AssemblerWriter;
				this.ReferenceIndex = aw.GetNextLabelIndex ();

				dw.AddLineNumberEngine (this);
			}

			public int LookupSource (ISourceFile source)
			{
				if (_sources.ContainsKey (source))
					return (int) _sources [source];

				int index = ++next_source_id;
				_sources.Add (source, index);
				return index;
			}

			public int LookupDirectory (string directory)
			{
				if (_directories.ContainsKey (directory))
					return (int) _directories [directory];

				int index = ++next_directory_id;
				_directories.Add (directory, index);
				return index;
			}

			public void AddMethod (ISourceMethod method)
			{
				LookupSource (method.SourceFile);

				int index = ++next_method_id;
				_methods.Add (method, index);
			}

			private void SetFile (ISourceFile source)
			{
				aw.WriteInt8 ((int) DW_LNS.LNS_set_file);
				aw.WriteULeb128 (LookupSource (source));
			}

			private int st_line = 1;

			private void SetLine (int line)
			{
				aw.WriteInt8 ((int) DW_LNS.LNS_advance_line);
				aw.WriteSLeb128 (line - st_line);
				st_line = line;
			}

			private void SetAddress (int token, int address)
			{
				aw.WriteUInt8 (0);
				object end_index = aw.StartSubsectionWithShortSize ();
				aw.WriteUInt8 ((int) DW_LNE.LNE_set_address);
				dw.AddRelocEntry (RelocEntryType.IL_OFFSET, token, address);
				aw.WriteAddress (0);
				aw.EndSubsection (end_index);
			}

			private void SetStartAddress (int token)
			{
				aw.WriteUInt8 (0);
				object end_index = aw.StartSubsectionWithShortSize ();
				aw.WriteUInt8 ((int) DW_LNE.LNE_set_address);
				dw.AddRelocEntry (RelocEntryType.METHOD_START_ADDRESS, token);
				aw.WriteAddress (0);
				aw.EndSubsection (end_index);
			}

			private void SetEndAddress (int token)
			{
				aw.WriteUInt8 (0);
				object end_index = aw.StartSubsectionWithShortSize ();
				aw.WriteUInt8 ((int) DW_LNE.LNE_set_address);
				dw.AddRelocEntry (RelocEntryType.METHOD_END_ADDRESS, token);
				aw.WriteAddress (0);
				aw.EndSubsection (end_index);
			}

			private void SetBasicBlock ()
			{
				aw.WriteUInt8 ((int) DW_LNS.LNS_set_basic_block);
			}

			private void EndSequence ()
			{
				aw.WriteUInt8 (0);
				aw.WriteUInt8 (1);
				aw.WriteUInt8 ((int) DW_LNE.LNE_end_sequence);

				st_line = 1;
			}

			private void Commit ()
			{
				aw.WriteUInt8 ((int) DW_LNS.LNS_copy);
			}

			private void WriteOneLine (int token, int line, int offset)
			{
				aw.WriteInt8 ((int) DW_LNS.LNS_advance_line);
				aw.WriteSLeb128 (line - st_line);
				aw.WriteUInt8 (0);
				object end_index = aw.StartSubsectionWithShortSize ();
				aw.WriteUInt8 ((int) DW_LNE.LNE_set_address);
				dw.AddRelocEntry (RelocEntryType.IL_OFFSET, token, offset);
				aw.WriteAddress (0);
				aw.EndSubsection (end_index);

				aw.Write2Bytes ((int) DW_LNS.LNS_set_basic_block,
						(int) DW_LNS.LNS_copy);

				st_line = line;
			}

			public void Emit ()
			{
				dw.WriteSectionStart (Section.DEBUG_LINE);
				aw.WriteLabel (ReferenceIndex);
				object end_index = aw.StartSubsectionWithSize ();

				aw.WriteUInt16 (2);
				object start_index = aw.StartSubsectionWithSize ();
				aw.WriteUInt8 (1);
				aw.WriteUInt8 (1);
				aw.WriteInt8 (LineBase);
				aw.WriteUInt8 (LineRange);
				aw.WriteUInt8 (StandardOpcodeSizes.Length);
				for (int i = 1; i < StandardOpcodeSizes.Length; i++)
					aw.WriteUInt8 (StandardOpcodeSizes [i]);

				foreach (string directory in Directories)
					aw.WriteString (directory);
				aw.WriteUInt8 (0);					

				foreach (ISourceFile source in Sources) {
					aw.WriteString (source.FileName);
					aw.WriteULeb128 (0);
					aw.WriteULeb128 (0);
					aw.WriteULeb128 (0);
				}

				aw.WriteUInt8 (0);

				aw.EndSubsection (start_index);

				foreach (ISourceMethod method in Methods) {
					if (method.Start == null || method.Start.Row == 0)
						continue;

					SetFile (method.SourceFile);
					SetLine (method.Start.Row);
					SetStartAddress (method.Token);
					SetBasicBlock ();
					Commit ();

					foreach (ISourceLine line in method.Lines) {
						count++;
						WriteOneLine (method.Token, line.Row, line.Offset);
						// SetLine (line.Row);
						// SetAddress (method.Token, line.Offset);
						// SetBasicBlock ();
						// Commit ();
					}

					SetLine (method.End.Row);
					SetEndAddress (method.Token);
					SetBasicBlock ();
					Commit ();

					EndSequence ();
				}

				aw.EndSubsection (end_index);
				aw.WriteSectionEnd ();
			}
		}

		// DWARF tag from the DWARF 2 specification.
		public enum DW_TAG {
			TAG_array_type		= 0x01,
			TAG_class_type		= 0x02,
			TAG_enumeration_type	= 0x04,
			TAG_formal_parameter	= 0x05,
			TAG_lexical_block	= 0x0b,
			TAG_member		= 0x0d,
			TAG_pointer_type	= 0x0f,
			TAG_compile_unit	= 0x11,
			TAG_string_type		= 0x12,
			TAG_structure_type	= 0x13,
			TAG_typedef		= 0x16,
			TAG_inheritance		= 0x1c,
			TAG_subrange_type	= 0x21,
			TAG_base_type		= 0x24,
			TAG_enumerator		= 0x28,
			TAG_subprogram		= 0x2e,
			TAG_variable		= 0x34
		}

		// DWARF attribute from the DWARF 2 specification.
		public enum DW_AT {
			AT_location		= 0x02,
			AT_name			= 0x03,
			AT_byte_size		= 0x0b,
			AT_stmt_list		= 0x10,
			AT_low_pc		= 0x11,
			AT_high_pc		= 0x12,
			AT_language		= 0x13,
			AT_string_length	= 0x19,
			AT_const_value		= 0x1c,
			AT_lower_bound		= 0x22,
			AT_producer		= 0x25,
			AT_start_scope		= 0x2c,
			AT_upper_bound		= 0x2f,
			AT_accessibility	= 0x32,
			AT_artificial		= 0x34,
			AT_count		= 0x37,
			AT_data_member_location	= 0x38,
			AT_declaration		= 0x3c,
			AT_encoding		= 0x3e,
			AT_external		= 0x3f,
			AT_specification	= 0x47,
			AT_type			= 0x49,
			AT_data_location	= 0x50,
			AT_end_scope		= 0x2121
		}

		// DWARF form from the DWARF 2 specification.
		public enum DW_FORM {
			FORM_addr		= 0x01,
			FORM_block4		= 0x04,
			FORM_data4		= 0x06,
			FORM_string		= 0x08,
			FORM_data1		= 0x0b,
			FORM_flag		= 0x0c,
			FORM_sdata		= 0x0d,
			FORM_udata		= 0x0f,
			FORM_ref4		= 0x13
		}

		public enum DW_LANG {
			LANG_C_plus_plus	= 0x04,
			LANG_C_sharp		= 0x9001
		}

		public enum DW_OP {
			OP_addr			= 0x03,
			OP_deref		= 0x06,
			OP_const1u		= 0x08,
			OP_const1s		= 0x09,
			OP_const2u		= 0x0a,
			OP_const2s		= 0x0b,
			OP_const4u		= 0x0c,
			OP_const4s		= 0x0d,
			OP_const8u		= 0x0e,
			OP_const8s		= 0x0f,
			OP_constu		= 0x10,
			OP_consts		= 0x11,
			OP_plus			= 0x22,
			OP_plus_uconst		= 0x23,
			OP_fbreg		= 0x91,
		}

		public enum DW_ACCESS {
			ACCESS_public		= 1,
			ACCESS_protected	= 2,
			ACCESS_private		= 3
		};

		protected enum MRI_string {
			offset_length		= 0x00,
			offset_chars		= 0x01
		}

		protected enum MRI_array {
			offset_bounds		= 0x00,
			offset_max_length	= 0x01,
			offset_vector		= 0x02
		}

		protected enum MRI_array_bounds {
			offset_lower		= 0x00,
			offset_length		= 0x01
		}

		// Abstract base class for a "debugging information entry" (die).
		public abstract class Die
		{
			protected DwarfFileWriter dw;
			protected IAssemblerWriter aw;
			protected ArrayList child_dies = new ArrayList ();
			public readonly Die Parent;

			protected readonly int abbrev_id;
			protected readonly AbbrevDeclaration abbrev_decl;

			public readonly int ReferenceIndex;

			//
			// Create a new die If @parent is not null, add the newly
			// created die to the parent's list of child dies.
			//
			// @abbrev_id is the abbreviation id for this die class.
			// Derived classes should call the DwarfFileWriter's static
			// RegisterAbbrevDeclaration function in their static constructor
			// to get an abbrev id. Once you registered an abbrev entry, it'll
			// be automatically written to the debug_abbrev section.
			//
			public Die (DwarfFileWriter dw, Die parent, int abbrev_id)
			{
				this.dw = dw;
				this.aw = dw.AssemblerWriter;
				this.Parent = parent;
				this.abbrev_id = abbrev_id;
				this.abbrev_decl = GetAbbrevDeclaration (abbrev_id);
				this.ReferenceIndex = this.aw.GetNextLabelIndex ();

				if (parent != null)
					parent.AddChildDie (this);
			}

			public Die (DwarfFileWriter dw, int abbrev_id)
				: this (dw, null, abbrev_id)
			{ }

			public Die (Die parent, int abbrev_id)
				: this (parent.dw, parent, abbrev_id)
			{ }

			protected void AddChildDie (Die die)
			{
				child_dies.Add (die);
			}

			public override bool Equals (object o)
			{
				if (!(o is Die))
					return false;

				return ((Die) o).ReferenceIndex == ReferenceIndex;
			}

			public override int GetHashCode ()
			{
				return ReferenceIndex;
			}

			//
			// Write this die and all its children to the dwarf file.
			//
			public virtual void Emit ()
			{
				aw.WriteLabel (ReferenceIndex);

				aw.WriteULeb128 (abbrev_id);
				DoEmit ();

				if (abbrev_decl.HasChildren) {
					foreach (Die child in child_dies)
						child.Emit ();

					aw.WriteUInt8 (0);
				}
			}

			//
			// Derived classes must implement this function to actually
			// write themselves to the dwarf file.
			//
			// Note that the abbrev id has already been written in Emit() -
			// if you don't like this, you must override Emit() as well.
			//
			public abstract void DoEmit ();

			//
			// Gets the compile unit of this die.
			//
			public virtual DieCompileUnit GetCompileUnit ()
			{
				Die die = this;

				while (die.Parent != null)
					die = die.Parent;

				if (die is DieCompileUnit)
					return (DieCompileUnit) die;
				else
					return null;
			}

			public DieCompileUnit DieCompileUnit {
				get {
					return GetCompileUnit ();
				}
			}

			public DwarfFileWriter Writer {
				get {
					return dw;
				}
			}
		}

		// DW_TAG_compile_unit
		public class DieCompileUnit : Die
		{
			private static int my_abbrev_id;

			protected Hashtable types = new Hashtable ();
			protected Hashtable methods = new Hashtable ();
			protected Hashtable pointer_types = new Hashtable ();

			static DieCompileUnit ()
			{
				AbbrevEntry[] entries = {
					new AbbrevEntry (DW_AT.AT_name, DW_FORM.FORM_string),
					new AbbrevEntry (DW_AT.AT_producer, DW_FORM.FORM_string),
					new AbbrevEntry (DW_AT.AT_language, DW_FORM.FORM_udata),
					new AbbrevEntry (DW_AT.AT_stmt_list, DW_FORM.FORM_ref4)
				};
				AbbrevDeclaration decl = new AbbrevDeclaration (
					DW_TAG.TAG_compile_unit, true, entries);

				my_abbrev_id = RegisterAbbrevDeclaration (decl);
			}

			public readonly CompileUnit CompileUnit;
			public readonly bool DoGeneric;
			public readonly LineNumberEngine LineNumberEngine;

			//
			// Create a new DW_TAG_compile_unit debugging information entry
			// and add it to the @compile_unit.
			//
			public DieCompileUnit (CompileUnit compile_unit)
				: base (compile_unit.DwarfFileWriter, my_abbrev_id)
			{
				this.CompileUnit = compile_unit;
				this.DoGeneric = dw.DoGeneric;
				compile_unit.AddDie (this);

				LineNumberEngine = new LineNumberEngine (dw);
			}

			public void WriteRelativeDieReference (Die target_die)
			{
				if (!this.Equals (target_die.GetCompileUnit ()))
					throw new ArgumentException ("Target die must be in the same "
								     + "compile unit");

				aw.WriteRelativeOffset (CompileUnit.ReferenceIndex,
							target_die.ReferenceIndex);
			}

			public void WriteTypeReference (ITypeHandle ihandle)
			{
				WriteTypeReference (ihandle, true);
			}

			public void WriteTypeReference (ITypeHandle ihandle, bool use_pointer_die)
			{
				if (!(ihandle is TypeHandle))
					throw new NotSupportedException ();

				TypeHandle handle = (TypeHandle) ihandle;

				if (use_pointer_die && (handle.PointerDie != null))
					WriteRelativeDieReference (handle.PointerDie);
				else
					WriteRelativeDieReference (handle.TypeDie);
			}

			public override void DoEmit ()
			{
				aw.WriteString (CompileUnit.SourceFile);
				aw.WriteString (CompileUnit.ProducerID);
				if (dw.use_gnu_extensions)
					aw.WriteULeb128 ((int) DW_LANG.LANG_C_sharp);
				else
					aw.WriteULeb128 ((int) DW_LANG.LANG_C_plus_plus);
				aw.WriteAbsoluteOffset (LineNumberEngine.ReferenceIndex);
			}
		}

		// DW_TAG_subprogram
		public class DieSubProgram : Die
		{
			private static int my_abbrev_id_1;
			private static int my_abbrev_id_2;
			private static int my_abbrev_id_3;
			private static int my_abbrev_id_4;
			private static int my_abbrev_id_5;

			static DieSubProgram ()
			{
				// Method without return value
				AbbrevEntry[] entries_1 = {
					new AbbrevEntry (DW_AT.AT_name, DW_FORM.FORM_string),
					new AbbrevEntry (DW_AT.AT_external, DW_FORM.FORM_flag),
					new AbbrevEntry (DW_AT.AT_low_pc, DW_FORM.FORM_addr),
					new AbbrevEntry (DW_AT.AT_high_pc, DW_FORM.FORM_addr)
				};
				// Method with return value
				AbbrevEntry[] entries_2 = {
					new AbbrevEntry (DW_AT.AT_name, DW_FORM.FORM_string),
					new AbbrevEntry (DW_AT.AT_external, DW_FORM.FORM_flag),
					new AbbrevEntry (DW_AT.AT_low_pc, DW_FORM.FORM_addr),
					new AbbrevEntry (DW_AT.AT_high_pc, DW_FORM.FORM_addr),
					new AbbrevEntry (DW_AT.AT_type, DW_FORM.FORM_ref4)
				};
				// Method declaration without return value
				AbbrevEntry[] entries_3 = {
					new AbbrevEntry (DW_AT.AT_name, DW_FORM.FORM_string),
					new AbbrevEntry (DW_AT.AT_external, DW_FORM.FORM_flag),
					new AbbrevEntry (DW_AT.AT_declaration, DW_FORM.FORM_flag)
				};
				// Method declaration with return value
				AbbrevEntry[] entries_4 = {
					new AbbrevEntry (DW_AT.AT_name, DW_FORM.FORM_string),
					new AbbrevEntry (DW_AT.AT_external, DW_FORM.FORM_flag),
					new AbbrevEntry (DW_AT.AT_declaration, DW_FORM.FORM_flag),
					new AbbrevEntry (DW_AT.AT_type, DW_FORM.FORM_ref4)
				};
				// Method definition
				AbbrevEntry[] entries_5 = {
					new AbbrevEntry (DW_AT.AT_name, DW_FORM.FORM_string),
					new AbbrevEntry (DW_AT.AT_external, DW_FORM.FORM_flag),
					new AbbrevEntry (DW_AT.AT_specification, DW_FORM.FORM_ref4)
				};


				AbbrevDeclaration decl_1 = new AbbrevDeclaration (
					DW_TAG.TAG_subprogram, true, entries_1);
				AbbrevDeclaration decl_2 = new AbbrevDeclaration (
					DW_TAG.TAG_subprogram, true, entries_2);
				AbbrevDeclaration decl_3 = new AbbrevDeclaration (
					DW_TAG.TAG_subprogram, true, entries_3);
				AbbrevDeclaration decl_4 = new AbbrevDeclaration (
					DW_TAG.TAG_subprogram, true, entries_4);
				AbbrevDeclaration decl_5 = new AbbrevDeclaration (
					DW_TAG.TAG_subprogram, true, entries_5);

				my_abbrev_id_1 = RegisterAbbrevDeclaration (decl_1);
				my_abbrev_id_2 = RegisterAbbrevDeclaration (decl_2);
				my_abbrev_id_3 = RegisterAbbrevDeclaration (decl_3);
				my_abbrev_id_4 = RegisterAbbrevDeclaration (decl_4);
				my_abbrev_id_5 = RegisterAbbrevDeclaration (decl_5);
			}

			private static int get_abbrev_id (bool DoGeneric, ISourceMethod method)
			{
				if (DoGeneric)
					if (method.ReturnType == typeof (void))
						return my_abbrev_id_3;
					else
						return my_abbrev_id_4;
				else
					if (method.ReturnType == typeof (void))
						return my_abbrev_id_1;
					else
						return my_abbrev_id_2;
			}

			protected string name;
			protected Die specification_die;
			protected ISourceMethod method;
			protected ITypeHandle retval_type;

			//
			// Create a new DW_TAG_subprogram debugging information entry
			// for method @name (which has a void return value) and add it
			// to the @parent_die
			//
			public DieSubProgram (DieCompileUnit parent_die, Die specification_die,
					      ISourceMethod method)
				: this (parent_die, method.FullName, method, my_abbrev_id_5)
			{
				this.specification_die = specification_die;
			}

			public DieSubProgram (Die parent_die, ISourceMethod method)
				: this (parent_die, method.MethodBase.Name, method,
					get_abbrev_id (false, method))
			{
				if (method.ReturnType != typeof (void))
					retval_type = dw.RegisterType (method.ReturnType);

				DieCompileUnit.LineNumberEngine.AddMethod (method);
			}

			private DieSubProgram (Die parent_die, string name, ISourceMethod method,
					       int abbrev_id)
				: base (parent_die, abbrev_id)
			{
				this.name = name;
				this.method = method;

				if (!method.MethodBase.IsStatic)
					new DieMethodVariable (this, method);

				foreach (ParameterInfo param in method.Parameters) {
					MethodParameter mp = new MethodParameter (dw, method, param);

					new DieMethodVariable (this, mp);
				}
			}

			public override void DoEmit ()
			{
				aw.WriteString (name);
				aw.WriteUInt8 (true);
				if (specification_die != null) {
					DieCompileUnit.WriteRelativeDieReference (specification_die);
					return;
				}

				if (dw.DoGeneric)
					aw.WriteUInt8 (true);
				else {
					dw.AddRelocEntry (RelocEntryType.METHOD_START_ADDRESS, method.Token);
					aw.WriteAddress (0);
					dw.AddRelocEntry (RelocEntryType.METHOD_END_ADDRESS, method.Token);
					aw.WriteAddress (0);
				}
				if (method.ReturnType != typeof (void))
					DieCompileUnit.WriteTypeReference (retval_type);
			}
		}

		// DW_TAG_base_type
		public class DieBaseType : DieType
		{
			private static int my_abbrev_id;

			static DieBaseType ()
			{
				AbbrevEntry[] entries = {
					new AbbrevEntry (DW_AT.AT_name, DW_FORM.FORM_string),
					new AbbrevEntry (DW_AT.AT_encoding, DW_FORM.FORM_data1),
					new AbbrevEntry (DW_AT.AT_byte_size, DW_FORM.FORM_data1)
				};

				AbbrevDeclaration decl = new AbbrevDeclaration (
					DW_TAG.TAG_base_type, false, entries);

				my_abbrev_id = RegisterAbbrevDeclaration (decl);
			}

			protected Type type;
			protected string name;

			//
			// Create a new DW_TAG_base_type debugging information entry
			//
			public DieBaseType (DieCompileUnit parent_die, ITypeHandle type)
				: this (parent_die, type, type.Name)
			{ }

			public DieBaseType (DieCompileUnit parent_die, ITypeHandle type, string name)
				: base (parent_die, type, my_abbrev_id)
			{
				this.type = type.Type;
				this.name = name;
			}

			protected enum DW_ATE {
				ATE_void		= 0x00,
				ATE_address		= 0x01,
				ATE_boolean		= 0x02,
				ATE_complex_float	= 0x03,
				ATE_float		= 0x04,
				ATE_signed		= 0x05,
				ATE_signed_char		= 0x06,
				ATE_unsigned		= 0x07,
				ATE_unsigned_char	= 0x08
			}

			public override void DoEmit ()
			{
				aw.WriteString (name);
				if (type == typeof (void)) {
					aw.WriteUInt8 ((int) DW_ATE.ATE_address);
					aw.WriteUInt8 (0);
				} else if (type == typeof (bool)) {
					aw.WriteUInt8 ((int) DW_ATE.ATE_boolean);
					aw.WriteUInt8 (1);
				} else if (type == typeof (char)) {
					aw.WriteUInt8 ((int) DW_ATE.ATE_unsigned_char);
					aw.WriteUInt8 (2);
				} else if (type == typeof (sbyte)) {
					aw.WriteUInt8 ((int) DW_ATE.ATE_signed);
					aw.WriteUInt8 (1);
				} else if (type == typeof (byte)) {
					aw.WriteUInt8 ((int) DW_ATE.ATE_unsigned);
					aw.WriteUInt8 (1);
				} else if (type == typeof (short)) {
					aw.WriteUInt8 ((int) DW_ATE.ATE_signed);
					aw.WriteUInt8 (2);
				} else if (type == typeof (ushort)) {
					aw.WriteUInt8 ((int) DW_ATE.ATE_unsigned);
					aw.WriteUInt8 (2);
				} else if (type == typeof (int)) {
					aw.WriteUInt8 ((int) DW_ATE.ATE_signed);
					aw.WriteUInt8 (4);
				} else if (type == typeof (uint)) {
					aw.WriteUInt8 ((int) DW_ATE.ATE_unsigned);
					aw.WriteUInt8 (4);
				} else if (type == typeof (long)) {
					aw.WriteUInt8 ((int) DW_ATE.ATE_signed);
					aw.WriteUInt8 (8);
				} else if (type == typeof (ulong)) {
					aw.WriteUInt8 ((int) DW_ATE.ATE_unsigned);
					aw.WriteUInt8 (8);
				} else if (type == typeof (float)) {
					aw.WriteUInt8 ((int) DW_ATE.ATE_float);
					aw.WriteUInt8 (4);
				} else if (type == typeof (double)) {
					aw.WriteUInt8 ((int) DW_ATE.ATE_float);
					aw.WriteUInt8 (8);
				} else
					throw new ArgumentException ("Not a base type: " + type);
			}
		}

		//
		// Abstract base class for types.
		//

		public abstract class DieType : Die
		{
			private readonly ITypeHandle type_handle;

			public DieType (Die parent_die, Type type, int abbrev_id)
				: this (parent_die, parent_die.Writer.RegisterType (type), abbrev_id)
			{ }

			public DieType (Die parent_die, ITypeHandle type_handle, int abbrev_id)
				: base (parent_die, abbrev_id)
			{
				this.type_handle = type_handle;
			}

			public virtual void CreateType ()
			{ }

			public ITypeHandle TypeHandle {
				get {
					return type_handle;
				}
			}
		}


		// DW_TAG_pointer_type
		public class DiePointerType : DieType
		{
			private static int my_abbrev_id;

			static DiePointerType ()
			{
				AbbrevEntry[] entries = {
					new AbbrevEntry (DW_AT.AT_type, DW_FORM.FORM_ref4)
				};

				AbbrevDeclaration decl = new AbbrevDeclaration (
					DW_TAG.TAG_pointer_type, false, entries);

				my_abbrev_id = RegisterAbbrevDeclaration (decl);
			}

			public DiePointerType (DieCompileUnit parent_die, Type type)
				: base (parent_die, parent_die.Writer.RegisterType (type.GetElementType ()),
					my_abbrev_id)
			{ }

			public override void DoEmit ()
			{
				DieCompileUnit.WriteTypeReference (TypeHandle);
			}
		}

		public class DieTypeDef : DieType
		{
			private static int my_abbrev_id;

			static DieTypeDef ()
			{
				AbbrevEntry[] entries = {
					new AbbrevEntry (DW_AT.AT_name, DW_FORM.FORM_string),
					new AbbrevEntry (DW_AT.AT_type, DW_FORM.FORM_ref4)
				};

				AbbrevDeclaration decl = new AbbrevDeclaration (
					DW_TAG.TAG_typedef, false, entries);

				my_abbrev_id = RegisterAbbrevDeclaration (decl);
			}

			protected string name;

			public DieTypeDef (Die parent_die, ITypeHandle type, string name)
				: base (parent_die, type, my_abbrev_id)
			{
				this.name = name;
			}

			public override void DoEmit ()
			{
				aw.WriteString (name);
				DieCompileUnit.WriteTypeReference (TypeHandle);
			}
		}

		public class DieInternalTypeDef : Die
		{
			private static int my_abbrev_id;

			static DieInternalTypeDef ()
			{
				AbbrevEntry[] entries = {
					new AbbrevEntry (DW_AT.AT_name, DW_FORM.FORM_string),
					new AbbrevEntry (DW_AT.AT_type, DW_FORM.FORM_ref4)
				};

				AbbrevDeclaration decl = new AbbrevDeclaration (
					DW_TAG.TAG_typedef, false, entries);

				my_abbrev_id = RegisterAbbrevDeclaration (decl);
			}

			protected Die type_die;
			protected string name;

			public DieInternalTypeDef (DieCompileUnit parent_die, Die type_die, string name)
				: base (parent_die, my_abbrev_id)
			{
				this.type_die = type_die;
				this.name = name;
			}

			public override void DoEmit ()
			{
				aw.WriteString (name);
				DieCompileUnit.WriteRelativeDieReference (type_die);
			}
		}

		public class DieInternalPointer : Die
		{
			private static int my_abbrev_id;

			static DieInternalPointer ()
			{
				AbbrevEntry[] entries = {
					new AbbrevEntry (DW_AT.AT_type, DW_FORM.FORM_ref4)
				};

				AbbrevDeclaration decl = new AbbrevDeclaration (
					DW_TAG.TAG_pointer_type, false, entries);

				my_abbrev_id = RegisterAbbrevDeclaration (decl);
			}

			protected Die type_die;

			public DieInternalPointer (DieCompileUnit parent_die, Die type_die)
				: base (parent_die, my_abbrev_id)
			{
				this.type_die = type_die;
			}

			public override void DoEmit ()
			{
				DieCompileUnit.WriteRelativeDieReference (type_die);
			}
		}

		// DW_TAG_enumeration_type
		public class DieEnumType : DieType
		{
			private static int my_abbrev_id;

			static DieEnumType ()
			{
				AbbrevEntry[] entries = {
					new AbbrevEntry (DW_AT.AT_name, DW_FORM.FORM_string),
					new AbbrevEntry (DW_AT.AT_byte_size, DW_FORM.FORM_data1)
				};

				AbbrevDeclaration decl = new AbbrevDeclaration (
					DW_TAG.TAG_enumeration_type, true, entries);

				my_abbrev_id = RegisterAbbrevDeclaration (decl);
			}

			public DieEnumType (DieCompileUnit parent_die, ITypeHandle type)
				: base (parent_die, type, my_abbrev_id)
			{
				Array values = Enum.GetValues (type.Type);
				string[] names = Enum.GetNames (type.Type);

				foreach (object value in values) {
					int intval;
					string name = null;

					if (value is int)
						intval = (int) value;
					else
						intval = System.Convert.ToInt32 (value);

					for (int i = 0; i < values.Length; ++i)
						if (value.Equals (values.GetValue (i))) {
							name = names [i];
							break;
						}

					if (name == null)
						throw new ArgumentException ();

					new DieEnumerator (this, name, intval);
				}
			}

			public override void DoEmit ()
			{
				aw.WriteString (TypeHandle.Name);
				dw.AddRelocEntry_TypeSize (TypeHandle);
				aw.WriteUInt8 (0);
			}
		}

		// DW_TAG_enumerator
		public class DieEnumerator : Die
		{
			private static int my_abbrev_id;

			static DieEnumerator ()
			{
				AbbrevEntry[] entries = {
					new AbbrevEntry (DW_AT.AT_name, DW_FORM.FORM_string),
					new AbbrevEntry (DW_AT.AT_const_value, DW_FORM.FORM_data4)
				};

				AbbrevDeclaration decl = new AbbrevDeclaration (
					DW_TAG.TAG_enumerator, false, entries);

				my_abbrev_id = RegisterAbbrevDeclaration (decl);
			}

			protected string name;
			protected int value;

			public DieEnumerator (DieEnumType parent_die, string name, int value)
				: base (parent_die, my_abbrev_id)
			{
				this.name = name;
				this.value = value;
			}

			public override void DoEmit ()
			{
				aw.WriteString (name);
				aw.WriteInt32 (value);
			}
		}

		// DW_TAG_structure_type
		public class DieStructureType : DieType
		{
			private static int my_abbrev_id;

			static DieStructureType ()
			{
				AbbrevEntry[] entries = {
					new AbbrevEntry (DW_AT.AT_name, DW_FORM.FORM_string),
					new AbbrevEntry (DW_AT.AT_byte_size, DW_FORM.FORM_data1)
				};

				AbbrevDeclaration decl = new AbbrevDeclaration (
					DW_TAG.TAG_structure_type, true, entries);

				my_abbrev_id = RegisterAbbrevDeclaration (decl);
			}

			protected DieCompileUnit parent_die;
			protected readonly Type type;
			protected string name;
			protected FieldInfo[] fields;
			protected ITypeHandle[] field_types;
			protected Die[] field_dies;
			protected MethodInfo[] methods;
			protected Die[] method_dies;

			protected const BindingFlags FieldBindingFlags =
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
				BindingFlags.Static | BindingFlags.DeclaredOnly;

			public override void CreateType ()
			{
				fields = this.type.GetFields (FieldBindingFlags);
				field_types = new ITypeHandle [fields.Length];
				field_dies = new Die [fields.Length];

				int index = 0;
				for (int i = 0; i < fields.Length; i++) {
					Type field_type = fields [i].FieldType;
					field_types [i] = dw.RegisterType (field_type);
					DW_ACCESS access;

					if (fields [i].IsPublic)
						access = DW_ACCESS.ACCESS_public;
					else if (fields [i].IsPrivate)
						access = DW_ACCESS.ACCESS_private;
					else
						access = DW_ACCESS.ACCESS_protected;

					if (fields [i].IsStatic) {
						field_dies [i] = new DieStaticVariable (this, fields [i].Name,
											i, field_types [i]);
						string name = String.Concat (type.FullName, ".",
									     fields [i].Name);
						new DieStaticVariableDefinition (parent_die, fields [i].Name,
										 field_dies [i]);
					} else
						field_dies [i] = new DieMember (this, fields [i].Name,
										index++, field_types [i],
										access);
				}

				base.CreateType ();
			}

			public DieStructureType (DieCompileUnit parent_die, ITypeHandle type)
				: this (parent_die, type, type.Name, my_abbrev_id)
			{ }

			protected DieStructureType (DieCompileUnit parent_die, ITypeHandle type,
						    int abbrev_id)
				: this (parent_die, type, type.Name, abbrev_id)
			{ }

			protected DieStructureType (DieCompileUnit parent_die, ITypeHandle type,
						    string name)
				: this (parent_die, type, name, my_abbrev_id)
			{ }

			protected DieStructureType (DieCompileUnit parent_die, ITypeHandle type,
						    string name, int abbrev_id)
				: base (parent_die, type, abbrev_id)
			{
				this.parent_die = parent_die;
				this.type = type.Type;
				this.name = name;
			}

			public override void DoEmit ()
			{
				aw.WriteString (name);
				dw.AddRelocEntry_TypeSize (TypeHandle);
				aw.WriteUInt8 (0);
			}
		}

		public class DieArrayType : DieStructureType
		{
			protected new readonly ITypeHandle type;
			protected ITypeHandle element_type;
			protected int rank;

			public DieArrayType (DieCompileUnit parent_die, ITypeHandle type)
				: this (parent_die, type, type.Type.GetArrayRank ())
			{ }

			private DieArrayType (DieCompileUnit parent_die, ITypeHandle type, int rank)
				: base (parent_die, parent_die.Writer.array_type, type.Name)
			{
				this.type = type;
				this.rank = rank;
			}

			public override void CreateType ()
			{
				element_type = ((TypeHandle) type).ArrayElementType;

				new DieMember (this, "Bounds",
					       (int) MRI_array.offset_bounds,
					       ((TypeHandle) type).ArrayBoundsType);

				new DieMember (this, "MaxLength",
					       (int) MRI_array.offset_max_length,
					       dw.int_type);

				new DieMember (this, "Vector",
					       (int) MRI_array.offset_vector,
					       ((TypeHandle) type).ArrayVectorType);

				base.CreateType ();
			}
		}

		public class DieArrayBoundsType : DieStructureType
		{
			public DieArrayBoundsType (DieCompileUnit parent_die, ITypeHandle type)
				: base (parent_die, type, type.Name)
			{ }

			public override void CreateType ()
			{
				new DieMember (this, "Lower",
					       (int) MRI_array_bounds.offset_lower,
					       dw.int_type);

				new DieMember (this, "Length",
					       (int) MRI_array_bounds.offset_length,
					       dw.int_type);

				base.CreateType ();
			}
		}

		public class DieStringType : DieStructureType
		{
			protected new readonly ITypeHandle type;

			public DieStringType (DieCompileUnit parent_die, ITypeHandle type)
				: base (parent_die, parent_die.Writer.string_type, "MonoString")
			{
				this.type = type;
			}

			public override void CreateType ()
			{
				new DieMember (this, "Length",
					       (int) MRI_string.offset_length,
					       dw.int_type);

				new DieMember (this, "Chars",
					       (int) MRI_string.offset_chars,
					       dw.string_type.ArrayVectorType);

				base.CreateType ();
			}
		}

		public class DieMonoStringType : DieType
		{
			private static int my_abbrev_id;

			static DieMonoStringType ()
			{
				AbbrevEntry[] entries = {
					new AbbrevEntry (DW_AT.AT_string_length, DW_FORM.FORM_block4),
					new AbbrevEntry (DW_AT.AT_byte_size, DW_FORM.FORM_data4),
					new AbbrevEntry (DW_AT.AT_data_location, DW_FORM.FORM_block4)
				};

				AbbrevDeclaration decl = new AbbrevDeclaration (
					DW_TAG.TAG_string_type, false, entries);

				my_abbrev_id = RegisterAbbrevDeclaration (decl);
			}

			public DieMonoStringType (Die parent_die)
				: base (parent_die, parent_die.Writer.void_type, my_abbrev_id)
			{ }

			public override void DoEmit ()
			{
				object end_index;

				end_index = aw.StartSubsectionWithSize ();
				dw.AddRelocEntry (RelocEntryType.MONO_STRING_STRING_LENGTH);
				aw.WriteUInt32 (0);
				aw.WriteUInt32 (0);
				aw.EndSubsection (end_index);

				dw.AddRelocEntry (RelocEntryType.MONO_STRING_BYTE_SIZE);
				aw.WriteUInt32 (0);

				end_index = aw.StartSubsectionWithSize ();
				dw.AddRelocEntry (RelocEntryType.MONO_STRING_DATA_LOCATION);
				aw.WriteUInt32 (0);
				aw.WriteUInt32 (0);
				aw.EndSubsection (end_index);
			}
		}

		protected class DieInternalArray : DieType
		{
			private static int my_abbrev_id;

			static DieInternalArray ()
			{
				AbbrevEntry[] entries = {
					new AbbrevEntry (DW_AT.AT_name, DW_FORM.FORM_string),
					new AbbrevEntry (DW_AT.AT_type, DW_FORM.FORM_ref4),
					new AbbrevEntry (DW_AT.AT_byte_size, DW_FORM.FORM_data1)
				};

				AbbrevDeclaration decl = new AbbrevDeclaration (
					DW_TAG.TAG_array_type, true, entries);

				my_abbrev_id = RegisterAbbrevDeclaration (decl);
			}

			public DieInternalArray (Die parent_die, ITypeHandle type)
				: base (parent_die, type, my_abbrev_id)
			{ }

			public override void DoEmit ()
			{
				aw.WriteString (TypeHandle.Name);
				DieCompileUnit.WriteTypeReference (TypeHandle);
				aw.WriteUInt8 (4);
			}
		}

		public class DieMonoArrayType : DieType
		{
			private static int my_abbrev_id;

			protected readonly ITypeHandle type;
			protected ITypeHandle element_type;
			protected int rank;

			static DieMonoArrayType ()
			{
				AbbrevEntry[] entries = {
					new AbbrevEntry (DW_AT.AT_type, DW_FORM.FORM_ref4),
					new AbbrevEntry (DW_AT.AT_data_location, DW_FORM.FORM_block4)
				};

				AbbrevDeclaration decl = new AbbrevDeclaration (
					DW_TAG.TAG_array_type, true, entries);

				my_abbrev_id = RegisterAbbrevDeclaration (decl);
			}

			public DieMonoArrayType (Die parent_die, ITypeHandle type)
				: this (parent_die, type, type.Type.GetArrayRank ())
			{ }

			private DieMonoArrayType (Die parent_die, ITypeHandle type, int rank)
				: base (parent_die, type, my_abbrev_id)
			{
				this.type = type;
				this.rank = rank;
			}

			public override void CreateType ()
			{
				element_type = ((TypeHandle) type).ArrayElementType;

				new DieInternalSubRangeType (this);

				base.CreateType ();
			}

			public override void DoEmit ()
			{
				bool use_ptr_die;
				if (element_type.Type.Equals (typeof (string)))
					use_ptr_die = true;
				else
					use_ptr_die = false;

				DieCompileUnit.WriteTypeReference (element_type, use_ptr_die);

				object end_index = aw.StartSubsectionWithSize ();
				dw.AddRelocEntry (RelocEntryType.MONO_ARRAY_DATA_LOCATION);
				aw.WriteUInt32 (0);
				aw.WriteUInt32 (0);
				aw.EndSubsection (end_index);
			}
		}

		public class DieInternalSubRangeType : DieType
		{
			private static int my_abbrev_id_1;
			private static int my_abbrev_id_2;

			static DieInternalSubRangeType ()
			{
				AbbrevEntry[] entries_1 = {
					new AbbrevEntry (DW_AT.AT_type, DW_FORM.FORM_ref4),
					new AbbrevEntry (DW_AT.AT_byte_size, DW_FORM.FORM_data4),
					new AbbrevEntry (DW_AT.AT_count, DW_FORM.FORM_block4)
				};
				AbbrevEntry[] entries_2 = {
					new AbbrevEntry (DW_AT.AT_type, DW_FORM.FORM_ref4),
					new AbbrevEntry (DW_AT.AT_lower_bound, DW_FORM.FORM_block4),
					new AbbrevEntry (DW_AT.AT_upper_bound, DW_FORM.FORM_block4)
				};

				AbbrevDeclaration decl_1 = new AbbrevDeclaration (
					DW_TAG.TAG_subrange_type, false, entries_1);
				AbbrevDeclaration decl_2 = new AbbrevDeclaration (
					DW_TAG.TAG_subrange_type, false, entries_2);

				my_abbrev_id_1 = RegisterAbbrevDeclaration (decl_1);
				my_abbrev_id_2 = RegisterAbbrevDeclaration (decl_2);
			}

			protected int dimension;

			public DieInternalSubRangeType (Die parent_die)
				: base (parent_die, parent_die.Writer.int_type, my_abbrev_id_1)
			{
				this.dimension = -1;
			}

			public DieInternalSubRangeType (Die parent_die, int dimension)
				: base (parent_die, parent_die.Writer.int_type, my_abbrev_id_2)
			{
				this.dimension = dimension;
			}

			public override void DoEmit ()
			{
				DieCompileUnit.WriteTypeReference (TypeHandle);

				if (dimension >= 0)
					throw new NotImplementedException ();
				else {
					dw.AddRelocEntry (RelocEntryType.MONO_ARRAY_LENGTH_BYTE_SIZE);
					aw.WriteUInt32 (0);

					object end_index = aw.StartSubsectionWithSize ();
					dw.AddRelocEntry (RelocEntryType.MONO_ARRAY_MAX_LENGTH);
					aw.WriteUInt32 (0);
					aw.WriteUInt32 (0);
					aw.EndSubsection (end_index);
				}
			}
		}

		public class DieSubRangeType : DieType
		{
			private static int my_abbrev_id;

			static DieSubRangeType ()
			{
				AbbrevEntry[] entries = {
					new AbbrevEntry (DW_AT.AT_type, DW_FORM.FORM_ref4),
					new AbbrevEntry (DW_AT.AT_lower_bound, DW_FORM.FORM_data4),
					new AbbrevEntry (DW_AT.AT_upper_bound, DW_FORM.FORM_data4)
				};

				AbbrevDeclaration decl = new AbbrevDeclaration (
					DW_TAG.TAG_subrange_type, false, entries);

				my_abbrev_id = RegisterAbbrevDeclaration (decl);
			}

			protected int lower;
			protected int upper;

			public DieSubRangeType (Die parent_die, ITypeHandle type, int lower, int upper)
				: base (parent_die, type, my_abbrev_id)
			{
				this.lower = lower;
				this.upper = upper;
			}

			public override void DoEmit ()
			{
				DieCompileUnit.WriteTypeReference (TypeHandle);
				aw.WriteInt32 (lower);
				aw.WriteInt32 (upper);
			}
		}

		// DW_TAG_class_type
		public class DieClassType : DieStructureType
		{
			private static int my_abbrev_id;

			static DieClassType ()
			{
				AbbrevEntry[] entries = {
					new AbbrevEntry (DW_AT.AT_name, DW_FORM.FORM_string),
					new AbbrevEntry (DW_AT.AT_byte_size, DW_FORM.FORM_data1),
					new AbbrevEntry (DW_AT.AT_external, DW_FORM.FORM_flag)
				};

				AbbrevDeclaration decl = new AbbrevDeclaration (
					DW_TAG.TAG_class_type, true, entries);

				my_abbrev_id = RegisterAbbrevDeclaration (decl);
			}

			ITypeHandle base_type;

			public DieClassType (DieCompileUnit parent_die, ITypeHandle type_handle)
				: base (parent_die, type_handle, my_abbrev_id)
			{
			}

			public override void CreateType ()
			{
				Type baset = TypeHandle.Type.BaseType;

				if ((baset != null) && !baset.Equals (typeof (object)) && !baset.Equals (typeof (System.Array))) {
					base_type = dw.RegisterType (baset);

					new DieInheritance (this, base_type);
				}

				base.CreateType ();
			}

			public override void DoEmit ()
			{
				aw.WriteString (TypeHandle.Name);
				dw.AddRelocEntry_TypeSize (TypeHandle);
				aw.WriteUInt8 (0);
				aw.WriteUInt8 (true);
			}
		}

		// DW_TAG_inheritance
		public class DieInheritance : DieType
		{
			private static int my_abbrev_id;

			static DieInheritance ()
			{
				AbbrevEntry[] entries = {
					new AbbrevEntry (DW_AT.AT_type, DW_FORM.FORM_ref4),
					new AbbrevEntry (DW_AT.AT_data_member_location, DW_FORM.FORM_block4)
				};

				AbbrevDeclaration decl = new AbbrevDeclaration (
					DW_TAG.TAG_inheritance, false, entries);

				my_abbrev_id = RegisterAbbrevDeclaration (decl);
			}

			public DieInheritance (Die parent_die, ITypeHandle type)
				: base (parent_die, type, my_abbrev_id)
			{ }

			public override void DoEmit ()
			{
				DieCompileUnit.WriteTypeReference (TypeHandle, false);

				object end_index = aw.StartSubsectionWithSize ();
				aw.WriteUInt8 ((int) DW_OP.OP_const1u);
				aw.WriteUInt8 (0);
				aw.EndSubsection (end_index);
			}
		}

		// DW_TAG_member
		public class DieMember : DieType
		{
			private static int my_abbrev_id;

			static DieMember ()
			{
				AbbrevEntry[] entries = {
					new AbbrevEntry (DW_AT.AT_name, DW_FORM.FORM_string),
					new AbbrevEntry (DW_AT.AT_type, DW_FORM.FORM_ref4),
					new AbbrevEntry (DW_AT.AT_accessibility, DW_FORM.FORM_data1),
					new AbbrevEntry (DW_AT.AT_data_member_location, DW_FORM.FORM_block4)
				};

				AbbrevDeclaration decl = new AbbrevDeclaration (
					DW_TAG.TAG_member, false, entries);

				my_abbrev_id = RegisterAbbrevDeclaration (decl);
			}

			protected string name;
			protected int index;
			protected DW_ACCESS access;
			protected DieType parent_die;

			public DieMember (DieType parent_die, string name, int index,
					  ITypeHandle type, DW_ACCESS access)
				: base (parent_die, type, my_abbrev_id)
			{
				this.name = name;
				this.index = index;
				this.access = access;
				this.parent_die = parent_die;
			}

			public DieMember (DieType parent_die, string name, int index, ITypeHandle type)
				: this (parent_die, name, index, type,
					DW_ACCESS.ACCESS_public)
			{ }

			public override void DoEmit ()
			{
				aw.WriteString (name);
				DieCompileUnit.WriteTypeReference (TypeHandle);
				aw.WriteUInt8 ((int) access);

				object end_index = aw.StartSubsectionWithSize ();
				aw.WriteUInt8 ((int) DW_OP.OP_const4u);
				dw.AddRelocEntry_TypeFieldOffset (parent_die.TypeHandle, index);
				aw.WriteInt32 (0);
				aw.EndSubsection (end_index);
			}
		}

		// DW_TAG_lexical_block
		public class DieLexicalBlock : Die
		{
			private static int my_abbrev_id;

			static DieLexicalBlock ()
			{
				AbbrevEntry[] entries = {
					new AbbrevEntry (DW_AT.AT_low_pc, DW_FORM.FORM_addr),
					new AbbrevEntry (DW_AT.AT_high_pc, DW_FORM.FORM_addr)
				};

				AbbrevDeclaration decl = new AbbrevDeclaration (
					DW_TAG.TAG_lexical_block, true, entries);

				my_abbrev_id = RegisterAbbrevDeclaration (decl);
			}

			protected ISourceBlock block;

			public DieLexicalBlock (Die parent_die, ISourceBlock block)
				: base (parent_die, my_abbrev_id)
			{
				this.block = block;
			}

			public override void DoEmit ()
			{
				int token = block.SourceMethod.Token;
				int start_offset = block.Start.Offset;
				int end_offset = block.End.Offset;

				dw.AddRelocEntry (RelocEntryType.IL_OFFSET, token, start_offset);
				aw.WriteAddress (0);
				dw.AddRelocEntry (RelocEntryType.IL_OFFSET, token, end_offset);
				aw.WriteAddress (0);
			}
		}

		public abstract class DieVariable : DieType
		{
			private static int my_abbrev_id_this;
			private static int my_abbrev_id_local;
			private static int my_abbrev_id_param;
			private static int my_abbrev_id_static;

			public enum VariableType {
				VARIABLE_THIS,
				VARIABLE_PARAMETER,
				VARIABLE_LOCAL,
				VARIABLE_STATIC
			};

			static int get_abbrev_id (VariableType vtype)
			{
				switch (vtype) {
				case VariableType.VARIABLE_THIS:
					return my_abbrev_id_this;
				case VariableType.VARIABLE_PARAMETER:
					return my_abbrev_id_param;
				case VariableType.VARIABLE_LOCAL:
					return my_abbrev_id_local;
				case VariableType.VARIABLE_STATIC:
					return my_abbrev_id_static;
				default:
					throw new ArgumentException ();
				}
			}

			static DieVariable ()
			{
				AbbrevEntry[] entries_1 = {
					new AbbrevEntry (DW_AT.AT_name, DW_FORM.FORM_string),
					new AbbrevEntry (DW_AT.AT_type, DW_FORM.FORM_ref4),
					new AbbrevEntry (DW_AT.AT_external, DW_FORM.FORM_flag),
					new AbbrevEntry (DW_AT.AT_location, DW_FORM.FORM_block4),
					new AbbrevEntry (DW_AT.AT_start_scope, DW_FORM.FORM_addr),
					new AbbrevEntry (DW_AT.AT_end_scope, DW_FORM.FORM_addr)
				};
				AbbrevEntry[] entries_2 = {
					new AbbrevEntry (DW_AT.AT_name, DW_FORM.FORM_string),
					new AbbrevEntry (DW_AT.AT_type, DW_FORM.FORM_ref4),
					new AbbrevEntry (DW_AT.AT_external, DW_FORM.FORM_flag),
					new AbbrevEntry (DW_AT.AT_location, DW_FORM.FORM_block4),
				};
				AbbrevEntry[] entries_3 = {
					new AbbrevEntry (DW_AT.AT_name, DW_FORM.FORM_string),
					new AbbrevEntry (DW_AT.AT_type, DW_FORM.FORM_ref4),
					new AbbrevEntry (DW_AT.AT_artificial, DW_FORM.FORM_flag),
					new AbbrevEntry (DW_AT.AT_location, DW_FORM.FORM_block4),
				};
				AbbrevEntry[] entries_4 = {
					new AbbrevEntry (DW_AT.AT_name, DW_FORM.FORM_string),
					new AbbrevEntry (DW_AT.AT_type, DW_FORM.FORM_ref4),
					new AbbrevEntry (DW_AT.AT_declaration, DW_FORM.FORM_flag),
					new AbbrevEntry (DW_AT.AT_location, DW_FORM.FORM_block4)
				};

				AbbrevDeclaration decl_local = new AbbrevDeclaration (
					DW_TAG.TAG_variable, false, entries_1);
				AbbrevDeclaration decl_param = new AbbrevDeclaration (
					DW_TAG.TAG_formal_parameter, false, entries_2);
				AbbrevDeclaration decl_this = new AbbrevDeclaration (
					DW_TAG.TAG_formal_parameter, false, entries_3);
				AbbrevDeclaration decl_static = new AbbrevDeclaration (
					DW_TAG.TAG_variable, false, entries_4);

				my_abbrev_id_local = RegisterAbbrevDeclaration (decl_local);
				my_abbrev_id_param = RegisterAbbrevDeclaration (decl_param);
				my_abbrev_id_this = RegisterAbbrevDeclaration (decl_this);
				my_abbrev_id_static = RegisterAbbrevDeclaration (decl_static);
			}

			protected string name;
			protected ITypeHandle type_handle;
			protected VariableType vtype;

			public DieVariable (Die parent_die, string name, ITypeHandle handle, VariableType vtype)
				: base (parent_die, handle, get_abbrev_id (vtype))
			{
				this.name = name;
				this.type_handle = handle;
				this.vtype = vtype;
			}

			public override void DoEmit ()
			{
				aw.WriteString (name);
				DieCompileUnit.WriteTypeReference (type_handle);
				switch (vtype) {
				case VariableType.VARIABLE_LOCAL:
					aw.WriteUInt8 (false);
					DoEmitLocation ();
					DoEmitScope ();
					break;
				case VariableType.VARIABLE_PARAMETER:
					aw.WriteUInt8 (false);
					DoEmitLocation ();
					break;
				case VariableType.VARIABLE_THIS:
					aw.WriteUInt8 (true);
					DoEmitLocation ();
					break;
				case VariableType.VARIABLE_STATIC:
					aw.WriteUInt8 (true);
					DoEmitLocation ();
					break;
				}
			}

			protected abstract void DoEmitLocation ();
			protected abstract void DoEmitScope ();
		}

		public class DieMethodVariable : DieVariable
		{
			public DieMethodVariable (Die parent_die, ILocalVariable local)
				: base (parent_die, local.Name, local.TypeHandle,
					VariableType.VARIABLE_LOCAL)
			{
				this.var = local;
			}

			public DieMethodVariable (Die parent_die, IMethodParameter param)
				: base (parent_die, param.Name, param.TypeHandle,
					VariableType.VARIABLE_PARAMETER)
			{
				this.var = param;
			}

			public DieMethodVariable (Die parent_die, ISourceMethod method)
				: base (parent_die, "this",
					parent_die.Writer.RegisterType (method.MethodBase.ReflectedType),
					VariableType.VARIABLE_THIS)
			{
				this.method = method;
			}

			protected IVariable var;
			protected ISourceMethod method;

			protected override void DoEmitLocation ()
			{
				object end_index = aw.StartSubsectionWithSize ();
				// These relocation entries expect a location description
				// of exactly 8 bytes.
				switch (vtype) {
				case VariableType.VARIABLE_LOCAL:
					dw.AddRelocEntry (RelocEntryType.LOCAL_VARIABLE,
							  var.Method.Token, var.Index);
					break;
				case VariableType.VARIABLE_PARAMETER:
					dw.AddRelocEntry (RelocEntryType.METHOD_PARAMETER,
							  var.Method.Token, var.Index);
					break;
				case VariableType.VARIABLE_THIS:
					dw.AddRelocEntry (RelocEntryType.METHOD_PARAMETER,
							  method.Token, 0);
					break;
				}
				// This looks a bit strange, but OP_fbreg takes a sleb128
				// agument and we can't use fields of variable size.
				aw.WriteUInt8 ((int) DW_OP.OP_fbreg);
				aw.WriteSLeb128 (0);
				aw.WriteUInt8 ((int) DW_OP.OP_const4s);
				aw.WriteInt32 (0);
				aw.WriteUInt8 ((int) DW_OP.OP_plus);
				aw.EndSubsection (end_index);
			}

			protected override void DoEmitScope ()
			{
				dw.AddRelocEntry (RelocEntryType.VARIABLE_START_SCOPE,
						  var.Method.Token, var.Index);
				aw.WriteAddress (0);
				dw.AddRelocEntry (RelocEntryType.VARIABLE_END_SCOPE,
						  var.Method.Token, var.Index);
				aw.WriteAddress (0);
			}
		}

		public class DieStaticVariable : DieVariable
		{
			public DieStaticVariable (DieType parent_die, string name, int index,
						  ITypeHandle type)
				: base (parent_die, name, type, VariableType.VARIABLE_STATIC)
			{
				this.parent_die = parent_die;
				this.index = index;
				this.type = type;
			}

			DieType parent_die;
			ITypeHandle type;
			int index;

			protected override void DoEmitLocation ()
			{
				object end_index = aw.StartSubsectionWithSize ();
				aw.WriteUInt8 ((int) DW_OP.OP_addr);
				dw.AddRelocEntry_TypeStaticFieldOffset (parent_die.TypeHandle, index);
				aw.WriteAddress (0);
				aw.EndSubsection (end_index);
			}

			protected override void DoEmitScope ()
			{ }
		}

		public class DieStaticVariableDefinition : Die
		{
			private static int my_abbrev_id;

			static DieStaticVariableDefinition ()
			{
				AbbrevEntry[] entries = {
					new AbbrevEntry (DW_AT.AT_name, DW_FORM.FORM_string),
					new AbbrevEntry (DW_AT.AT_specification, DW_FORM.FORM_ref4),
					new AbbrevEntry (DW_AT.AT_external, DW_FORM.FORM_flag)
				};

				AbbrevDeclaration decl = new AbbrevDeclaration (
					DW_TAG.TAG_variable, false, entries);

				my_abbrev_id = RegisterAbbrevDeclaration (decl);
			}

			protected Die specification_die;
			protected string name;

			public DieStaticVariableDefinition (DieCompileUnit parent_die, string name,
							    Die specification_die)
				: base (parent_die, my_abbrev_id)
			{
				this.name = name;
				this.specification_die = specification_die;
			}

			public override void DoEmit ()
			{
				aw.WriteString (name);
				DieCompileUnit.WriteRelativeDieReference (specification_die);
				aw.WriteUInt8 (true);
			}
		}

		protected const int reloc_table_version = 15;

		protected enum Section {
			DEBUG_INFO		= 0x01,
			DEBUG_ABBREV		= 0x02,
			DEBUG_LINE		= 0x03,
			MONO_RELOC_TABLE	= 0x04,
			MONO_LINE_NUMBERS	= 0x05,
			MONO_SYMBOL_TABLE	= 0x06
		}

		public struct AbbrevEntry {
			public AbbrevEntry (DW_AT attribute, DW_FORM form)
			{
				this._attribute = attribute;
				this._form = form;
			}

			private DW_AT _attribute;
			private DW_FORM _form;

			public DW_AT Attribute {
				get {
					return _attribute;
				}
			}

			public DW_FORM Form {
				get {
					return _form;
				}
			}
		}

		public struct AbbrevDeclaration {
			public AbbrevDeclaration (DW_TAG tag, bool has_children, AbbrevEntry[] entries)
			{
				this._tag = tag;
				this._has_children = has_children;
				this._entries = entries;
			}

			private DW_TAG _tag;
			private bool _has_children;
			private AbbrevEntry[] _entries;

			public DW_TAG Tag {
				get {
					return _tag;
				}
			}

			public bool HasChildren {
				get {
					return _has_children;
				}
			}

			public AbbrevEntry[] Entries {
				get {
					return _entries;
				}
			}
		}

	    
		protected enum RelocEntryType {
			NONE,
			// Size of an address on the target machine
			TARGET_ADDRESS_SIZE		= 0x01,
			// Map an IL offset to a machine address
			IL_OFFSET			= 0x02,
			// Start address of machine code for this method
			METHOD_START_ADDRESS		= 0x03,
			// End address of machine code for this method
			METHOD_END_ADDRESS		= 0x04,
			// Stack offset of local variable
			LOCAL_VARIABLE			= 0x05,
			// Stack offset of method parameter
			METHOD_PARAMETER		= 0x06,
			// Sizeof (type)
			TYPE_SIZEOF			= 0x07,
			TYPE_FIELD_OFFSET		= 0x08,
			MONO_STRING_SIZEOF		= 0x09,
			MONO_STRING_OFFSET		= 0x0a,
			MONO_ARRAY_SIZEOF		= 0x0b,
			MONO_ARRAY_OFFSET		= 0x0c,
			MONO_ARRAY_BOUNDS_SIZEOF	= 0x0d,
			MONO_ARRAY_BOUNDS_OFFSET	= 0x0e,
			VARIABLE_START_SCOPE		= 0x0f,
			VARIABLE_END_SCOPE		= 0x10,
			MONO_STRING_FIELDSIZE		= 0x11,
			MONO_ARRAY_FIELDSIZE		= 0x12,
			TYPE_FIELD_FIELDSIZE		= 0x13,
			MONO_STRING_STRING_LENGTH	= 0x14,
			MONO_STRING_BYTE_SIZE		= 0x15,
			MONO_STRING_DATA_LOCATION	= 0x16,
			TYPE_STATIC_FIELD_OFFSET	= 0x17,
			MONO_ARRAY_DATA_LOCATION	= 0x18,
			MONO_ARRAY_MAX_LENGTH		= 0x19,
			MONO_ARRAY_LENGTH_BYTE_SIZE	= 0x1a
		}

		protected class RelocEntry {
			public RelocEntry (RelocEntryType type, int token, int original,
					   Section section, int index)
			{
				_type = type;
				_section = section;
				_token = token;
				_original = original;
				_index = index;
			}

			public RelocEntryType RelocType {
				get {
					return _type;
				}
			}

			public Section Section {
				get {
					return _section;
				}
			}

			public int Index {
				get {
					return _index;
				}
			}

			public int Token {
				get {
					return _token;
				}
			}

			public int Original {
				get {
					return _original;
				}
			}

			private RelocEntryType _type;
			private Section _section;
			private int _token;
			private int _index;
			private int _original;
		}

		private Section current_section;
		private ArrayList reloc_entries = new ArrayList ();

		private static ArrayList abbrev_declarations = new ArrayList ();

		protected string GetSectionName (Section section)
		{
			switch (section) {
			case Section.DEBUG_INFO:
				return "debug_info";
			case Section.DEBUG_ABBREV:
				return "debug_abbrev";
			case Section.DEBUG_LINE:
				return "debug_line";
			case Section.MONO_RELOC_TABLE:
				return "mono_reloc_table";
			case Section.MONO_LINE_NUMBERS:
				return "mono_line_numbers";
			case Section.MONO_SYMBOL_TABLE:
				return "mono_symbol_table";
			default:
				throw new ArgumentException ();
			}
		}

		protected void AddRelocEntry (RelocEntry entry)
		{
			reloc_entries.Add (entry);
		}

		protected void AddRelocEntry (RelocEntryType type, int token, int original,
					      Section section, int index)
		{
			AddRelocEntry (new RelocEntry (type, token, original, section, index));
		}

		protected void AddRelocEntry (RelocEntryType type, int token, int original)
		{
			AddRelocEntry (type, token, original, current_section, aw.WriteLabel ());
		}

		protected void AddRelocEntry (RelocEntryType type, int token)
		{
			AddRelocEntry (type, token, 0);
		}

		protected void AddRelocEntry (RelocEntryType type)
		{
			AddRelocEntry (type, 0);
		}

		protected void AddRelocEntry (RelocEntryType entry_type, ITypeHandle type)
		{
			AddRelocEntry (entry_type, type, 0);
		}

		protected void AddRelocEntry (RelocEntryType entry_type, ITypeHandle type, int original)
		{
			AddRelocEntry (entry_type, type.Token, original);
		}

		protected void AddRelocEntry_TypeSize (ITypeHandle type)
		{
			if (type == string_type)
				AddRelocEntry (RelocEntryType.MONO_STRING_SIZEOF);
			else if (type == array_type)
				AddRelocEntry (RelocEntryType.MONO_ARRAY_SIZEOF);
			else if (type.Type.Equals (typeof (MonoArrayBounds)))
				AddRelocEntry (RelocEntryType.MONO_ARRAY_BOUNDS_SIZEOF);
			else
				AddRelocEntry (RelocEntryType.TYPE_SIZEOF, type);
		}

		protected void AddRelocEntry_TypeFieldOffset (ITypeHandle type, int index)
		{
			if (type == string_type)
				AddRelocEntry (RelocEntryType.MONO_STRING_OFFSET, index);
			else if (type == array_type)
				AddRelocEntry (RelocEntryType.MONO_ARRAY_OFFSET, index);
			else if (type.Type.Equals (typeof (MonoArrayBounds)))
				AddRelocEntry (RelocEntryType.MONO_ARRAY_BOUNDS_OFFSET, index);
			else
				AddRelocEntry (RelocEntryType.TYPE_FIELD_OFFSET, type, index);
		}

		protected void AddRelocEntry_TypeFieldSize (ITypeHandle type, int index)
		{
			if (type == string_type)
				AddRelocEntry (RelocEntryType.MONO_STRING_FIELDSIZE, index);
			else if (type == array_type)
				AddRelocEntry (RelocEntryType.MONO_ARRAY_FIELDSIZE, index);
			else
				AddRelocEntry (RelocEntryType.TYPE_FIELD_FIELDSIZE, type, index);
		}

		protected void AddRelocEntry_TypeStaticFieldOffset (ITypeHandle type, int index)
		{
			AddRelocEntry (RelocEntryType.TYPE_STATIC_FIELD_OFFSET, type, index);
		}

		//
		// Mono relocation table. See the README.relocation-table file in this
		// directory for a detailed description of the file format.
		//
		protected void WriteRelocEntries ()
		{
			WriteSectionStart (Section.MONO_RELOC_TABLE);
			aw.WriteUInt16 (reloc_table_version);
			aw.WriteUInt8 (0);
			object end_index = aw.StartSubsectionWithSize ();

			int count = 0;

			foreach (RelocEntry entry in reloc_entries) {
				count++;

				aw.WriteUInt8 ((int) entry.RelocType);
				object tmp_index = aw.StartSubsectionWithSize ();

				aw.WriteUInt8 ((int) entry.Section);
				aw.WriteAbsoluteOffset (entry.Index);

				switch (entry.RelocType) {
				case RelocEntryType.METHOD_START_ADDRESS:
				case RelocEntryType.METHOD_END_ADDRESS:
				case RelocEntryType.TYPE_SIZEOF:
					aw.WriteUInt32 (entry.Token);
					break;
				case RelocEntryType.IL_OFFSET:
				case RelocEntryType.LOCAL_VARIABLE:
				case RelocEntryType.METHOD_PARAMETER:
				case RelocEntryType.TYPE_FIELD_OFFSET:
				case RelocEntryType.TYPE_STATIC_FIELD_OFFSET:
				case RelocEntryType.VARIABLE_START_SCOPE:
				case RelocEntryType.VARIABLE_END_SCOPE:
				case RelocEntryType.TYPE_FIELD_FIELDSIZE:
					aw.WriteUInt32 (entry.Token);
					aw.WriteUInt32 (entry.Original);
					break;
				case RelocEntryType.MONO_STRING_SIZEOF:
				case RelocEntryType.MONO_ARRAY_SIZEOF:
				case RelocEntryType.MONO_ARRAY_BOUNDS_SIZEOF:
					break;
				case RelocEntryType.MONO_STRING_OFFSET:
				case RelocEntryType.MONO_ARRAY_OFFSET:
				case RelocEntryType.MONO_ARRAY_BOUNDS_OFFSET:
				case RelocEntryType.MONO_STRING_FIELDSIZE:
				case RelocEntryType.MONO_ARRAY_FIELDSIZE:
					aw.WriteUInt32 (entry.Token);
					break;
				}

				aw.EndSubsection (tmp_index);
			}

			aw.EndSubsection (end_index);
			aw.WriteSectionEnd ();
		}

		//
		// Registers a new abbreviation declaration.
		//
		// This function should be called by a static constructor in one of
		// Die's subclasses.
		//
		protected static int RegisterAbbrevDeclaration (AbbrevDeclaration decl)
		{
			return abbrev_declarations.Add (decl) + 1;
		}

		protected static AbbrevDeclaration GetAbbrevDeclaration (int index)
		{
			return (AbbrevDeclaration) abbrev_declarations [index - 1];
		}

		protected void WriteAbbrevDeclarations ()
		{
			aw.WriteSectionStart (GetSectionName (Section.DEBUG_ABBREV));
			aw.WriteLabel ("debug_abbrev_b");

			for (int index = 0; index < abbrev_declarations.Count; index++) {
				AbbrevDeclaration decl = (AbbrevDeclaration) abbrev_declarations [index];

				aw.WriteULeb128 (index + 1);
				aw.WriteULeb128 ((int) decl.Tag);
				aw.WriteUInt8 (decl.HasChildren);

				foreach (AbbrevEntry entry in decl.Entries) {
					aw.WriteULeb128 ((int) entry.Attribute);
					aw.WriteULeb128 ((int) entry.Form);
				}

				aw.WriteUInt8 (0);
				aw.WriteUInt8 (0);
			}

			aw.WriteSectionEnd ();
		}

		protected void WriteSectionStart (Section section)
		{
			aw.WriteSectionStart (GetSectionName (section));
			current_section = section;
		}

		public void WriteLineNumberTable (IMonoSymbolWriter symwriter)
		{
			WriteSectionStart (Section.MONO_LINE_NUMBERS);
			aw.WriteUInt16 (reloc_table_version);

			Hashtable sources = new Hashtable ();

			foreach (ISourceFile source in symwriter.Sources) {
				if (sources.ContainsKey (source))
					continue;

				sources.Add (source, aw.GetNextLabelIndex ());
			}

			object line_number_end_index = aw.StartSubsectionWithSize ();

			Hashtable method_labels = new Hashtable ();

			foreach (ISourceMethod method in symwriter.Methods) {
				if (method.Start == null || method.Start.Row == 0)
					continue;

				int label = aw.GetNextLabelIndex ();
				aw.WriteUInt32 (method.Token);
				aw.WriteAbsoluteOffset ((int) sources [method.SourceFile]);
				aw.WriteUInt32 (method.Start.Row);
				aw.WriteAbsoluteOffset (label);

				method_labels [method] = label;
			}

			aw.EndSubsection (line_number_end_index);

			foreach (ISourceMethod method in method_labels.Keys) {
				aw.WriteLabel ((int) method_labels [method]);

				foreach (ISourceLine line in method.Lines) {
					aw.WriteUInt32 (line.Row);
					aw.WriteUInt32 (line.Offset);
				}

				aw.WriteUInt32 (0);
				aw.WriteUInt32 (0);
			}

			foreach (ISourceFile source in sources.Keys) {
				aw.WriteLabel ((int) sources [source]);

				aw.WriteString (source.FileName);
			}

			aw.WriteSectionEnd ();
		}

		public void WriteSymbolTable (IMonoSymbolWriter symwriter)
		{
			WriteSectionStart (Section.MONO_SYMBOL_TABLE);
			aw.WriteUInt16 (reloc_table_version);

			object end_index = aw.StartSubsectionWithSize ();

			Type[] external_types = TypeHandle.ExternalTypes;

			aw.WriteUInt32 (external_types.Length);
			for (int i = 0; i < external_types.Length; i++) {
				aw.WriteString (external_types [i].Namespace);
				aw.WriteString (external_types [i].Name);
			}

			aw.EndSubsection (end_index);

			aw.WriteSectionEnd ();
		}
	}

	internal struct MonoString
	{ }

	internal struct MonoArrayBounds
	{ }

	internal struct MonoArray
	{ }
}
