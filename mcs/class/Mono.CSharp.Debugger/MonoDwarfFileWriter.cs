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

		public bool timestamps = true;
		public bool use_gnu_extensions = false;

		// Write a generic file which contains no machine dependant stuff but
		// only function and type declarations.
		protected readonly bool DoGeneric = false;

		//
		// DwarfFileWriter public interface
		//
		public DwarfFileWriter (string symbol_file)
		{
			this.symbol_file = symbol_file;
			this.writer = new StreamWriter (symbol_file, false, Encoding.ASCII);
			this.aw = new AssemblerWriterI386 (this.writer);
			this.last_time = DateTime.Now;
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

		// Writes the final dwarf file.
		public void Close ()
		{
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

		//
		// Create a debugging information entry for the given type.
		//
		public DieType CreateType (DieCompileUnit die_compile_unit, Type type)
		{
			if (type.IsPointer)
				return new DiePointerType (die_compile_unit, type.GetElementType ());
			else if (type.IsEnum)
				return new DieEnumType (die_compile_unit, type);
			else if (type.Equals (typeof (string)))
				return new DieStringType (die_compile_unit, type);
			else if (type.IsArray)
				return new DieArrayType (die_compile_unit, type.GetElementType (),
							 type.GetArrayRank ());
			else if (type.IsValueType)
				return new DieStructureType (die_compile_unit, type);
			else if (type.IsClass)
				return new DieClassType (die_compile_unit, type);

			return new DiePointerType (die_compile_unit, typeof (void));

			// throw new NotSupportedException ("Type " + type + " is not yet supported.");
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
			AT_data_member_location	= 0x38,
			AT_declaration		= 0x3c,
			AT_encoding		= 0x3e,
			AT_external		= 0x3f,
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
			offset_vector		= 0x01
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

		protected class MethodParameter : IMethodParameter
		{
			public MethodParameter (ISourceMethod method, ParameterInfo param)
			{
				this._method = method;
				this._param = param;
			}

			private readonly ISourceMethod _method;
			private readonly ParameterInfo _param;

			// interface IMethodParameter

			public string Name {
				get {
					return _param.Name;
				}
			}

			public int Token {
				get {
					return _method.Token;
				}
			}

			public int Index {
				get {
					return _method.MethodInfo.IsStatic ? _param.Position - 1 :
						_param.Position;
				}
			}

			public Type Type {
				get {
					return _param.ParameterType;
				}
			}

			public byte[] Signature {
				get {
					return null;
				}
			}

			public ISourceLine Line {
				get {
					return null;
				}
			}
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
		}

		// DW_TAG_compile_unit
		public class DieCompileUnit : Die
		{
			private static int my_abbrev_id;

			protected Hashtable types = new Hashtable ();
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

				// GDB doesn't support DW_TAG_base_types yet, so we need to
				// include the types in each compile unit.
				RegisterType (typeof (bool));
				RegisterType (typeof (char));
				RegisterType (typeof (SByte));
				RegisterType (typeof (Byte));
				RegisterType (typeof (Int16));
				RegisterType (typeof (UInt16));
				RegisterType (typeof (Int32));
				RegisterType (typeof (UInt32));
				RegisterType (typeof (Int64));
				RegisterType (typeof (UInt64));
				RegisterType (typeof (Single));
				RegisterType (typeof (Double));

				LineNumberEngine = new LineNumberEngine (dw);
			}

			// Registers a new type
			public Die RegisterType (Type type)
			{
				if (types.Contains (type))
					return (Die) types [type];

				if (type.IsPrimitive) {
					Die base_type = new DieBaseType (this, type);

					types.Add (type, base_type);

					return base_type;
				}

				DieType die = dw.CreateType (this, type);

				types.Add (type, die);

				die.RegisterDependencyTypes ();

				return die;
			}

			public Die RegisterPointerType (Type type)
			{
				if (pointer_types.Contains (type))
					return (Die) pointer_types [type];

				Die type_die = RegisterType (type);

				Die pointer_die = new DieInternalPointer (this, type_die);

				pointer_types.Add (type, pointer_die);

				return pointer_die;
			}

			public void WriteRelativeDieReference (Die target_die)
			{
				if (!this.Equals (target_die.GetCompileUnit ()))
					throw new ArgumentException ("Target die must be in the same "
								     + "compile unit");

				aw.WriteRelativeOffset (CompileUnit.ReferenceIndex,
							target_die.ReferenceIndex);
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


				AbbrevDeclaration decl_1 = new AbbrevDeclaration (
					DW_TAG.TAG_subprogram, true, entries_1);
				AbbrevDeclaration decl_2 = new AbbrevDeclaration (
					DW_TAG.TAG_subprogram, true, entries_2);
				AbbrevDeclaration decl_3 = new AbbrevDeclaration (
					DW_TAG.TAG_subprogram, true, entries_3);
				AbbrevDeclaration decl_4 = new AbbrevDeclaration (
					DW_TAG.TAG_subprogram, true, entries_4);

				my_abbrev_id_1 = RegisterAbbrevDeclaration (decl_1);
				my_abbrev_id_2 = RegisterAbbrevDeclaration (decl_2);
				my_abbrev_id_3 = RegisterAbbrevDeclaration (decl_3);
				my_abbrev_id_4 = RegisterAbbrevDeclaration (decl_4);
			}

			private static int get_abbrev_id (DieCompileUnit parent_die, ISourceMethod method)
			{
				if (parent_die.DoGeneric)
					if (method.MethodInfo.ReturnType == typeof (void))
						return my_abbrev_id_3;
					else
						return my_abbrev_id_4;
				else
					if (method.MethodInfo.ReturnType == typeof (void))
						return my_abbrev_id_1;
					else
						return my_abbrev_id_2;
			}

			protected ISourceMethod method;
			protected Die retval_die;

			//
			// Create a new DW_TAG_subprogram debugging information entry
			// for method @name (which has a void return value) and add it
			// to the @parent_die
			//
			public DieSubProgram (DieCompileUnit parent_die, ISourceMethod method)
				: base (parent_die, get_abbrev_id (parent_die, method))
			{
				this.method = method;

				if (method.MethodInfo.ReturnType != typeof (void))
					retval_die = DieCompileUnit.RegisterType (
						method.MethodInfo.ReturnType);

				if (!method.MethodInfo.IsStatic)
					new DieMethodVariable (this, method);

				ParameterInfo[] parameters = method.MethodInfo.GetParameters ();
				foreach (ParameterInfo param in parameters) {
					MethodParameter mp = new MethodParameter (method, param);

					new DieMethodVariable (this, mp);
				}

				DieCompileUnit.LineNumberEngine.AddMethod (method);
			}

			public override void DoEmit ()
			{
				aw.WriteString (method.MethodInfo.Name);
				aw.WriteUInt8 (true);
				if (dw.DoGeneric)
					aw.WriteUInt8 (true);
				else {
					dw.AddRelocEntry (RelocEntryType.METHOD_START_ADDRESS, method.Token);
					aw.WriteAddress (0);
					dw.AddRelocEntry (RelocEntryType.METHOD_END_ADDRESS, method.Token);
					aw.WriteAddress (0);
				}
				if (method.MethodInfo.ReturnType != typeof (void))
					DieCompileUnit.WriteRelativeDieReference (retval_die);
			}
		}

		// DW_TAG_base_type
		public class DieBaseType : Die
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

			//
			// Create a new DW_TAG_base_type debugging information entry
			//
			public DieBaseType (DieCompileUnit parent_die, Type type)
				: base (parent_die, my_abbrev_id)
			{
				this.type = type;
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
				string name = type.Name;

				aw.WriteString (name);
				switch (name) {
				case "Void":
					aw.WriteUInt8 ((int) DW_ATE.ATE_address);
					aw.WriteUInt8 (0);
					break;
				case "Boolean":
					aw.WriteUInt8 ((int) DW_ATE.ATE_boolean);
					aw.WriteUInt8 (1);
					break;
				case "Char":
					aw.WriteUInt8 ((int) DW_ATE.ATE_unsigned_char);
					aw.WriteUInt8 (2);
					break;
				case "SByte":
					aw.WriteUInt8 ((int) DW_ATE.ATE_signed);
					aw.WriteUInt8 (1);
					break;
				case "Byte":
					aw.WriteUInt8 ((int) DW_ATE.ATE_unsigned);
					aw.WriteUInt8 (1);
					break;
				case "Int16":
					aw.WriteUInt8 ((int) DW_ATE.ATE_signed);
					aw.WriteUInt8 (2);
					break;
				case "UInt16":
					aw.WriteUInt8 ((int) DW_ATE.ATE_unsigned);
					aw.WriteUInt8 (2);
					break;
				case "Int32":
					aw.WriteUInt8 ((int) DW_ATE.ATE_signed);
					aw.WriteUInt8 (4);
					break;
				case "UInt32":
					aw.WriteUInt8 ((int) DW_ATE.ATE_unsigned);
					aw.WriteUInt8 (4);
					break;
				case "Int64":
					aw.WriteUInt8 ((int) DW_ATE.ATE_signed);
					aw.WriteUInt8 (8);
					break;
				case "UInt64":
					aw.WriteUInt8 ((int) DW_ATE.ATE_unsigned);
					aw.WriteUInt8 (8);
					break;
				case "Single":
					aw.WriteUInt8 ((int) DW_ATE.ATE_float);
					aw.WriteUInt8 (4);
					break;
				case "Double":
					aw.WriteUInt8 ((int) DW_ATE.ATE_float);
					aw.WriteUInt8 (8);
					break;
				default:
					throw new ArgumentException ("Not a base type: " + type);
				}
			}
		}

		public abstract class DieType : Die
		{
			public DieType (DieCompileUnit parent_die, Type type, int abbrev_id)
				: base (parent_die, abbrev_id)
			{
				this.type = type;
			}

			protected Type type;

			//
			// This is called after the type has been added to the type hash.
			//
			// You need to register your dependency types here and not in the
			// constructor to avoid a recursion loop if a type references itself.
			//
			public virtual void RegisterDependencyTypes ()
			{
				// do nothing
			}
		}

		public class DieTypeDef : Die
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
			protected Die type_die;

			public DieTypeDef (Die parent_die, Die type_die, string name)
				: base (parent_die, my_abbrev_id)
			{
				this.name = name;
				this.type_die = type_die;
			}

			public override void DoEmit ()
			{
				aw.WriteString (name);
				DieCompileUnit.WriteRelativeDieReference (type_die);
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

			protected Die type_die;

			public DiePointerType (DieCompileUnit parent_die, Type type)
				: base (parent_die, type, my_abbrev_id)
			{ }

			public override void RegisterDependencyTypes ()
			{
				type_die = DieCompileUnit.RegisterType (type);
			}

			public override void DoEmit ()
			{
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

			public DieInternalPointer (Die parent_die, Die type_die)
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

			public DieEnumType (DieCompileUnit parent_die, Type type)
				: base (parent_die, type, my_abbrev_id)
			{
				Array values = Enum.GetValues (type);
				string[] names = Enum.GetNames (type);

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
				aw.WriteString (type.Name);
				dw.AddRelocEntry_TypeSize (type);
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

			protected string name;
			protected FieldInfo[] fields;
			protected Die[] field_type_dies;
			protected Die[] field_dies;

			protected const BindingFlags FieldBindingFlags =
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
				BindingFlags.Instance;

			public override void RegisterDependencyTypes ()
			{
				fields = type.GetFields (FieldBindingFlags);
				field_type_dies = new Die [fields.Length];
				field_dies = new Die [fields.Length];

				for (int i = 0; i < fields.Length; i++) {
					Type field_type = fields [i].FieldType;
					field_type_dies [i] = DieCompileUnit.RegisterType (field_type);

					DW_ACCESS access;
					if (fields [i].IsPublic)
						access = DW_ACCESS.ACCESS_public;
					else if (fields [i].IsPrivate)
						access = DW_ACCESS.ACCESS_private;
					else
						access = DW_ACCESS.ACCESS_protected;

					field_dies [i] = new DieMember (this, type, fields [i].Name,
									i, field_type_dies [i],
									access);
				}
			}

			public DieStructureType (DieCompileUnit parent_die, Type type)
				: this (parent_die, type, type.Name, my_abbrev_id)
			{ }

			protected DieStructureType (DieCompileUnit parent_die, Type type, int abbrev_id)
				: this (parent_die, type, type.Name, abbrev_id)
			{ }

			protected DieStructureType (DieCompileUnit parent_die, Type type, string name)
				: this (parent_die, type, name, my_abbrev_id)
			{ }

			protected DieStructureType (DieCompileUnit parent_die, Type type,
						    string name, int abbrev_id)
				: base (parent_die, type, abbrev_id)
			{
				this.name = name;
			}

			public override void DoEmit ()
			{
				aw.WriteString (name);
				dw.AddRelocEntry_TypeSize (type);
				aw.WriteUInt8 (0);
			}
		}

		public class DieArrayType : DieStructureType
		{
			public override void RegisterDependencyTypes ()
			{
				Die array_die = new DieInternalArray (DieCompileUnit, typeof (MonoArrayBounds));
				new DieSubRangeType (array_die, typeof (int), 0, rank);

				Die bounds_die = new DieInternalPointer (DieCompileUnit, array_die);
				new DieMember (this, typeof (MonoArray), "Bounds",
					       (int) MRI_array.offset_bounds, bounds_die);

				Die length_die = DieCompileUnit.RegisterType (typeof (int));
				new DieMember (this, typeof (MonoArray), "MaxLength",
					       (int) MRI_array.offset_max_length, length_die);

				Die vector_die = new DieInternalArray (DieCompileUnit, element_type);
				new DieSubRangeType (vector_die, typeof (int), 0, -1);

				new DieMember (this, typeof (MonoArray), "Vector",
					       (int) MRI_array.offset_vector, vector_die);
			}

			protected Type element_type;
			protected int rank;

			protected static string MakeArrayName (Type type, int rank)
			{
				string name = type.Name;

				for (int i = 0; i < rank; i++)
					name += "[]";

				return name;
			}

			public DieArrayType (DieCompileUnit parent_die, Type type, int rank)
				: base (parent_die, typeof (MonoArray), MakeArrayName (type, rank))
			{
				this.element_type = type;
				this.rank = rank;
			}
		}

		public class DieStringType : DieStructureType
		{
			public override void RegisterDependencyTypes ()
			{
				Die length_die = DieCompileUnit.RegisterType (typeof (int));
				new DieMember (this, typeof (MonoString), "Length",
					       (int) MRI_string.offset_length, length_die);

				if (dw.use_gnu_extensions) {
					Die string_die = new DieInternalString (DieCompileUnit);

					new DieMember (this, typeof (MonoString), "String",
						       (int) MRI_string.offset_vector, string_die);
				} else {
					Die vector_die = new DieArrayType (DieCompileUnit, typeof (char), 1);
					new DieSubRangeType (vector_die, typeof (int), 0, -1);

					Die ptr_die = new DieInternalPointer (DieCompileUnit, vector_die);

					new DieMember (this, typeof (MonoString), "Vector",
						       (int) MRI_string.offset_vector, ptr_die);
				}
			}

			public DieStringType (DieCompileUnit parent_die, Type type)
				: base (parent_die, typeof (MonoString))
			{ }
		}

		public class DieInternalString : Die
		{
			private static int my_abbrev_id;

			static DieInternalString ()
			{
				AbbrevEntry[] entries = {
					new AbbrevEntry (DW_AT.AT_string_length, DW_FORM.FORM_data4),
					new AbbrevEntry (DW_AT.AT_byte_size, DW_FORM.FORM_data4),
					new AbbrevEntry (DW_AT.AT_data_location, DW_FORM.FORM_data4)
				};

				AbbrevDeclaration decl = new AbbrevDeclaration (
					DW_TAG.TAG_string_type, false, entries);

				my_abbrev_id = RegisterAbbrevDeclaration (decl);
			}

			public DieInternalString (Die parent_die)
				: base (parent_die, my_abbrev_id)
			{ }

			public override void DoEmit ()
			{
				dw.AddRelocEntry_TypeFieldOffset (typeof (MonoArray),
								  (int) MRI_array.offset_max_length);
				aw.WriteInt32 (0);
				dw.AddRelocEntry_TypeFieldSize (typeof (MonoArray),
								(int) MRI_array.offset_max_length);
				aw.WriteInt32 (0);
				dw.AddRelocEntry_TypeFieldOffset (typeof (MonoArray),
								  (int) MRI_array.offset_vector);
				aw.WriteInt32 (0);
			}
		}

		protected class DieInternalArray : Die
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

			public DieInternalArray (Die parent_die, Type type)
				: base (parent_die, my_abbrev_id)
			{
				this.type = type;

				this.type_die = DieCompileUnit.RegisterType (type);
			}

			protected Type type;
			protected Die type_die;

			public override void DoEmit ()
			{
				aw.WriteString (type.Name);
				DieCompileUnit.WriteRelativeDieReference (type_die);
				aw.WriteUInt8 (4);
			}
		}

		public class DieSubRangeType : Die
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
			protected Die type_die;

			public DieSubRangeType (Die parent_die, Type type, int lower, int upper)
				: base (parent_die, my_abbrev_id)
			{
				this.lower = lower;
				this.upper = upper;

				type_die = DieCompileUnit.RegisterType (type);
			}

			public override void DoEmit ()
			{
				DieCompileUnit.WriteRelativeDieReference (type_die);
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
					new AbbrevEntry (DW_AT.AT_byte_size, DW_FORM.FORM_data1)
				};

				AbbrevDeclaration decl = new AbbrevDeclaration (
					DW_TAG.TAG_class_type, true, entries);

				my_abbrev_id = RegisterAbbrevDeclaration (decl);
			}

			public DieClassType (DieCompileUnit parent_die, Type type)
				: base (parent_die, type, my_abbrev_id)
			{ }

			public override void RegisterDependencyTypes ()
			{
				if ((type.BaseType != null) && !type.BaseType.Equals (typeof (object)) &&
				    !type.BaseType.Equals (typeof (System.Array))) {
					Die parent_die = DieCompileUnit.RegisterType (type.BaseType);

					new DieInheritance (this, parent_die);
				}
			}

			public override void DoEmit ()
			{
				aw.WriteString (type.Name);
				dw.AddRelocEntry_TypeSize (type);
				aw.WriteUInt8 (0);
			}
		}

		// DW_TAG_inheritance
		public class DieInheritance : Die
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

			Die type_die;

			public DieInheritance (Die parent_die, Die type_die)
				: base (parent_die, my_abbrev_id)
			{
				this.type_die = type_die;
			}

			public override void DoEmit ()
			{
				DieCompileUnit.WriteRelativeDieReference (type_die);

				object end_index = aw.StartSubsectionWithSize ();
				aw.WriteUInt8 ((int) DW_OP.OP_const1u);
				aw.WriteUInt8 (0);
				aw.EndSubsection (end_index);
			}
		}

		// DW_TAG_member
		public class DieMember : Die
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

			protected Type parent_type;
			protected string name;
			protected int index;
			protected Die type_die;
			protected DW_ACCESS access;

			public DieMember (Die parent_die, Type parent_type, string name, int index,
					  Die type_die, DW_ACCESS access)
				: base (parent_die, my_abbrev_id)
			{
				this.name = name;
				this.index = index;
				this.parent_type = parent_type;
				this.type_die = type_die;
				this.access = access;
			}

			public DieMember (Die parent_die, Type parent_type, string name, int index,
					  Die type_die)
				: this (parent_die, parent_type, name, index, type_die,
					DW_ACCESS.ACCESS_public)
			{ }

			public override void DoEmit ()
			{
				aw.WriteString (name);
				DieCompileUnit.WriteRelativeDieReference (type_die);
				aw.WriteUInt8 ((int) access);

				object end_index = aw.StartSubsectionWithSize ();
				aw.WriteUInt8 ((int) DW_OP.OP_const4u);
				dw.AddRelocEntry_TypeFieldOffset (parent_type, index);
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

		public abstract class DieVariable : Die
		{
			private static int my_abbrev_id_this;
			private static int my_abbrev_id_local;
			private static int my_abbrev_id_param;

			public enum VariableType {
				VARIABLE_THIS,
				VARIABLE_PARAMETER,
				VARIABLE_LOCAL
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

				AbbrevDeclaration decl_local = new AbbrevDeclaration (
					DW_TAG.TAG_variable, false, entries_1);
				AbbrevDeclaration decl_param = new AbbrevDeclaration (
					DW_TAG.TAG_formal_parameter, false, entries_2);
				AbbrevDeclaration decl_this = new AbbrevDeclaration (
					DW_TAG.TAG_formal_parameter, false, entries_3);


				my_abbrev_id_local = RegisterAbbrevDeclaration (decl_local);
				my_abbrev_id_param = RegisterAbbrevDeclaration (decl_param);
				my_abbrev_id_this = RegisterAbbrevDeclaration (decl_this);
			}

			protected string name;
			protected Die type_die;
			protected VariableType vtype;

			public DieVariable (Die parent_die, string name, Type type, VariableType vtype)
				: base (parent_die, get_abbrev_id (vtype))
			{
				this.name = name;
				if (type.IsValueType)
					this.type_die = DieCompileUnit.RegisterType (type);
				else
					this.type_die = DieCompileUnit.RegisterPointerType (type);
				this.vtype = vtype;
			}

			public override void DoEmit ()
			{
				aw.WriteString (name);
				DieCompileUnit.WriteRelativeDieReference (type_die);
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
				}
			}

			protected abstract void DoEmitLocation ();
			protected abstract void DoEmitScope ();
		}

		public class DieMethodVariable : DieVariable
		{
			public DieMethodVariable (Die parent_die, ILocalVariable local)
				: base (parent_die, local.Name, local.Type, VariableType.VARIABLE_LOCAL)
			{
				this.var = local;
			}

			public DieMethodVariable (Die parent_die, IMethodParameter param)
				: base (parent_die, param.Name, param.Type, VariableType.VARIABLE_PARAMETER)
			{
				this.var = param;
			}

			public DieMethodVariable (Die parent_die, ISourceMethod method)
				: base (parent_die, "this", method.MethodInfo.ReflectedType,
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
							  var.Token, var.Index);
					break;
				case VariableType.VARIABLE_PARAMETER:
					dw.AddRelocEntry (RelocEntryType.METHOD_PARAMETER,
							  var.Token, var.Index);
					break;
				case VariableType.VARIABLE_THIS:
					dw.AddRelocEntry (RelocEntryType.METHOD_PARAMETER,
							  method.Token, 0);
					break;
				}
				// This looks a bit strange, but OP_fbreg takes a sleb128
				// agument and we can't fields of variable size.
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
						  var.Token, var.Index);
				aw.WriteAddress (0);
				dw.AddRelocEntry (RelocEntryType.VARIABLE_END_SCOPE,
						  var.Token, var.Index);
				aw.WriteAddress (0);
			}
		}

		protected const int reloc_table_version = 9;

		protected enum Section {
			DEBUG_INFO		= 0x01,
			DEBUG_ABBREV		= 0x02,
			DEBUG_LINE		= 0x03,
			MONO_RELOC_TABLE	= 0x04
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
			TYPE_FIELD_FIELDSIZE		= 0x13
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

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static int get_type_token (Type type);

		protected void AddRelocEntry (RelocEntryType entry_type, Type type)
		{
			AddRelocEntry (entry_type, type, 0);
		}

		protected void AddRelocEntry (RelocEntryType entry_type, Type type, int original)
		{
			AddRelocEntry (entry_type, get_type_token (type), original);
		}

		protected void AddRelocEntry_TypeSize (Type type)
		{
			if (type.Equals (typeof (MonoString)))
				AddRelocEntry (RelocEntryType.MONO_STRING_SIZEOF);
			else if (type.Equals (typeof (MonoArray)))
				AddRelocEntry (RelocEntryType.MONO_ARRAY_SIZEOF);
			else if (type.Equals (typeof (MonoArrayBounds)))
				AddRelocEntry (RelocEntryType.MONO_ARRAY_BOUNDS_SIZEOF);
			else
				AddRelocEntry (RelocEntryType.TYPE_SIZEOF, type);
		}

		protected void AddRelocEntry_TypeFieldOffset (Type type, int index)
		{
			if (type.Equals (typeof (MonoString)))
				AddRelocEntry (RelocEntryType.MONO_STRING_OFFSET, index);
			else if (type.Equals (typeof (MonoArray)))
				AddRelocEntry (RelocEntryType.MONO_ARRAY_OFFSET, index);
			else if (type.Equals (typeof (MonoArrayBounds)))
				AddRelocEntry (RelocEntryType.MONO_ARRAY_BOUNDS_OFFSET, index);
			else
				AddRelocEntry (RelocEntryType.TYPE_FIELD_OFFSET, type, index);
		}

		protected void AddRelocEntry_TypeFieldSize (Type type, int index)
		{
			if (type.Equals (typeof (MonoString)))
				AddRelocEntry (RelocEntryType.MONO_STRING_FIELDSIZE, index);
			else if (type.Equals (typeof (MonoArray)))
				AddRelocEntry (RelocEntryType.MONO_ARRAY_FIELDSIZE, index);
			else
				AddRelocEntry (RelocEntryType.TYPE_FIELD_FIELDSIZE, type, index);
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
	}

	internal struct MonoString
	{ }

	internal struct MonoArrayBounds
	{ }

	internal struct MonoArray
	{ }
}
