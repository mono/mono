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
using System.Diagnostics.SymbolStore;
using System.Collections;
using System.IO;
	
namespace Mono.CSharp.Debugger
{

	public class DwarfFileWriter
	{
		protected const string producer_id = "Mono C# Compiler 0.01 03-18-2002";

		protected CompileUnit[] compile_units = new CompileUnit [0];
		protected StreamWriter writer = null;
		protected string symbol_file = null;

		// Write a generic file which contains no machine dependant stuff but
		// only function and type declarations.
		protected readonly bool DoGeneric = false;

		//
		// DwarfFileWriter public interface
		//
		public DwarfFileWriter (string symbol_file)
		{
			this.symbol_file = symbol_file;
			this.writer = new StreamWriter (symbol_file);
		}

		// Writes the final dwarf file.
		public void Close ()
		{
			foreach (CompileUnit compile_unit in compile_units)
				compile_unit.Emit ();

			WriteAbbrevDeclarations ();
			WriteRelocEntries ();

			writer.Close ();
		}

		// Adds a new compile unit to this dwarf file
		public void AddCompileUnit (CompileUnit compile_unit)
		{
			int i = compile_units.Length;
			CompileUnit[] new_c = new CompileUnit [i + 1];
			Array.Copy (compile_units, new_c, i);
			new_c [i] = compile_unit;
			compile_units = new_c;
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
		public Die CreateType (DieCompileUnit die_compile_unit, Type type)
		{
			if (type.IsPrimitive)
				return new DieBaseType (die_compile_unit, type);
			else if (type.IsPointer)
				return new DiePointerType (die_compile_unit, type.GetElementType ());

			throw new NotSupportedException ("Type " + type + " is not yet supported.");
		}

		//
		// A compile unit refers to a single C# source file.
		//
		public class CompileUnit
		{
			protected DwarfFileWriter dw;
			protected string source_file;
			protected Die[] dies;

			private static int next_ref_index = 0;

			public readonly int ReferenceIndex;
			public readonly string ReferenceLabel;

			public CompileUnit (DwarfFileWriter dw, string source_file, Die[] dies)
			{
				this.dw = dw;
				this.source_file = source_file;
				this.dies = dies;

				this.ReferenceIndex = ++next_ref_index;
				this.ReferenceLabel = ".L_COMPILE_UNIT_" + this.ReferenceIndex;

				dw.AddCompileUnit (this);
			}

			//
			// Construct a new compile unit for source file @source_file.
			//
			// This constructor automatically adds the newly created compile
			// unit to the DwarfFileWriter's list of compile units.
			//
			public CompileUnit (DwarfFileWriter dw, string source_file)
				: this (dw, source_file, new Die [0])
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
				int i = dies.Length;
				Die[] new_d = new Die [i + 1];
				Array.Copy (dies, new_d, i);
				new_d [i] = die;
				dies = new_d;
			}

			// Write the whole compile unit to the dwarf file.
			public void Emit ()
			{
				int start_index, end_index;

				dw.WriteSectionStart (Section.DEBUG_INFO);

				dw.WriteLabel (ReferenceLabel);

				start_index = dw.WriteAnonLabel ();

				end_index = dw.WriteSectionSize ();
				dw.WriteUInt16 (2);
				dw.WriteOffset ("debug_abbrev_b");
				if (dw.DoGeneric)
					dw.WriteUInt8 (4);
				else {
					dw.AddRelocEntry (RelocEntryType.TARGET_ADDRESS_SIZE);
					dw.WriteUInt8 (4);
				}

				if (dies != null)
					foreach (Die die in dies)
						die.Emit ();

				dw.WriteAnonLabel (end_index);

				dw.WriteSectionEnd ();
			}
		}

		// DWARF tag from the DWARF 2 specification.
		public enum DW_TAG {
			TAG_pointer_type	= 0x0f,
			TAG_compile_unit	= 0x11,
			TAG_base_type		= 0x24,
			TAG_subprogram		= 0x2e
		}

		// DWARF attribute from the DWARF 2 specification.
		public enum DW_AT {
			AT_name			= 0x03,
			AT_byte_size		= 0x0b,
			AT_low_pc		= 0x11,
			AT_high_pc		= 0x12,
			AT_language		= 0x13,
			AT_producer		= 0x25,
			AT_declaration		= 0x3c,
			AT_encoding		= 0x3e,
			AT_external		= 0x3f,
			AT_type			= 0x49
		}

		// DWARF form from the DWARF 2 specification.
		public enum DW_FORM {
			FORM_addr		= 0x01,
			FORM_string		= 0x08,
			FORM_data1		= 0x0b,
			FORM_flag		= 0x0c,
			FORM_ref4		= 0x13

		}

		public enum DW_LANG {
			LANG_C_plus_plus	= 0x04,
			LANG_C_sharp		= LANG_C_plus_plus
		}

		// Abstract base class for a "debugging information entry" (die).
		public abstract class Die
		{
			protected DwarfFileWriter dw;
			protected Die[] child_dies;
			public readonly Die Parent;

			private static int next_ref_index = 0;

			protected readonly int abbrev_id;
			protected readonly AbbrevDeclaration abbrev_decl;

			public readonly int ReferenceIndex;
			public readonly string ReferenceLabel;

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
				this.Parent = parent;
				this.abbrev_id = abbrev_id;
				this.abbrev_decl = GetAbbrevDeclaration (abbrev_id);
				this.ReferenceIndex = ++next_ref_index;
				this.ReferenceLabel = ".L_DIE_" + this.ReferenceIndex;

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
				if (child_dies != null) {
					int i = child_dies.Length;
					Die[] new_d =
						new Die [i + 1];
					Array.Copy (child_dies, new_d, i);
					new_d [i] = die;
					child_dies = new_d;
				} else {
					child_dies = new Die [1];
					child_dies [0] = die;
				}
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
				dw.WriteLabel (ReferenceLabel);

				dw.WriteULeb128 (abbrev_id);
				DoEmit ();

				if (abbrev_decl.HasChildren) {
					if (child_dies != null)
						foreach (Die child in child_dies)
							child.Emit ();

					dw.WriteUInt8 (0);
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

			static DieCompileUnit ()
			{
				AbbrevEntry[] entries = {
					new AbbrevEntry (DW_AT.AT_name, DW_FORM.FORM_string),
					new AbbrevEntry (DW_AT.AT_producer, DW_FORM.FORM_string),
					new AbbrevEntry (DW_AT.AT_language, DW_FORM.FORM_data1)
				};
				AbbrevDeclaration decl = new AbbrevDeclaration (
					DW_TAG.TAG_compile_unit, true, entries);

				my_abbrev_id = RegisterAbbrevDeclaration (decl);
			}

			public readonly CompileUnit compile_unit;
			public readonly bool DoGeneric;

			//
			// Create a new DW_TAG_compile_unit debugging information entry
			// and add it to the @compile_unit.
			//
			public DieCompileUnit (CompileUnit compile_unit)
				: base (compile_unit.DwarfFileWriter, my_abbrev_id)
			{
				this.compile_unit = compile_unit;
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
			}

			// Registers a new type
			public Die RegisterType (Type type)
			{
				if (types.Contains (type))
					return (Die) types [type];

				Die die = dw.CreateType (this, type);

				types.Add (type, die);
				return die;
			}

			public void WriteRelativeDieReference (Die target_die)
			{
				if (!this.Equals (target_die.GetCompileUnit ()))
					throw new ArgumentException ("Target die must be in the same "
								     + "compile unit");

				dw.WriteRelativeReference (compile_unit.ReferenceLabel,
							   target_die.ReferenceLabel);
			}

			public override void DoEmit ()
			{
				dw.WriteString (compile_unit.SourceFile);
				dw.WriteString (compile_unit.ProducerID);
				dw.WriteUInt8 ((int) DW_LANG.LANG_C_sharp);
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

			private static int get_abbrev_id (DieCompileUnit parent_die,
							  ISourceMethod method)
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
			}

			public override void DoEmit ()
			{
				dw.WriteString (method.MethodInfo.Name);
				dw.WriteFlag (true);
				if (dw.DoGeneric)
					dw.WriteFlag (true);
				else {
					dw.AddRelocEntry (RelocEntryType.IL_OFFSET);
					dw.WriteAddress (0);
					dw.AddRelocEntry (RelocEntryType.IL_OFFSET);
					dw.WriteAddress (0);
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

				dw.WriteString (name);
				switch (name) {
				case "Void":
					dw.WriteUInt8 ((int) DW_ATE.ATE_address);
					dw.WriteUInt8 (0);
					break;
				case "Boolean":
					dw.WriteUInt8 ((int) DW_ATE.ATE_boolean);
					dw.WriteUInt8 (1);
					break;
				case "Char":
					dw.WriteUInt8 ((int) DW_ATE.ATE_unsigned_char);
					dw.WriteUInt8 (2);
					break;
				case "SByte":
					dw.WriteUInt8 ((int) DW_ATE.ATE_signed);
					dw.WriteUInt8 (1);
					break;
				case "Byte":
					dw.WriteUInt8 ((int) DW_ATE.ATE_unsigned);
					dw.WriteUInt8 (1);
					break;
				case "Int16":
					dw.WriteUInt8 ((int) DW_ATE.ATE_signed);
					dw.WriteUInt8 (2);
					break;
				case "UInt16":
					dw.WriteUInt8 ((int) DW_ATE.ATE_unsigned);
					dw.WriteUInt8 (2);
					break;
				case "Int32":
					dw.WriteUInt8 ((int) DW_ATE.ATE_signed);
					dw.WriteUInt8 (4);
					break;
				case "UInt32":
					dw.WriteUInt8 ((int) DW_ATE.ATE_unsigned);
					dw.WriteUInt8 (4);
					break;
				case "Int64":
					dw.WriteUInt8 ((int) DW_ATE.ATE_signed);
					dw.WriteUInt8 (8);
					break;
				case "UInt64":
					dw.WriteUInt8 ((int) DW_ATE.ATE_unsigned);
					dw.WriteUInt8 (8);
					break;
				case "Single":
					dw.WriteUInt8 ((int) DW_ATE.ATE_float);
					dw.WriteUInt8 (4);
					break;
				case "Double":
					dw.WriteUInt8 ((int) DW_ATE.ATE_float);
					dw.WriteUInt8 (8);
					break;
				default:
					throw new ArgumentException ("Not a base type: " + type);
				}
			}
		}

		// DW_TAG_pointer_type
		public class DiePointerType : Die
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

			protected Type type;
			protected Die type_die;

			//
			// Create a new type DIE describing a pointer to @type.
			//
			public DiePointerType (DieCompileUnit parent_die, Type type)
				: base (parent_die, my_abbrev_id)
			{
				this.type = type;
				type_die = DieCompileUnit.RegisterType (type);
			}

			public override void DoEmit ()
			{
				DieCompileUnit.WriteRelativeDieReference (type_die);
			}
		}

		protected const int reloc_table_version = 1;

		protected enum Section {
			DEBUG_INFO,
			DEBUG_ABBREV,
			DEBUG_LINES,
			MONO_RELOC_TABLE
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
			TARGET_ADDRESS_SIZE	= 0x01,
			// Map an IL offset to a machine address
			IL_OFFSET		= 0x02
		}

		protected struct RelocEntry {
			public RelocEntry (RelocEntryType type, Section section, int index)
			{
				_type = type;
				_section = section;
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

			private RelocEntryType _type;
			private Section _section;
			private int _index;
		}

		private int next_anon_label_idx = 0;
		private Section current_section;
		private RelocEntry[] reloc_entries = new RelocEntry [0];

		private static AbbrevDeclaration[] abbrev_declarations = new AbbrevDeclaration [0];

		protected string GetSectionName (Section section)
		{
			switch (section) {
			case Section.DEBUG_INFO:
				return "debug_info";
			case Section.DEBUG_ABBREV:
				return "debug_abbrev";
			case Section.DEBUG_LINES:
				return "debug_lines";
			case Section.MONO_RELOC_TABLE:
				return "mono_reloc_table";
			default:
				throw new ArgumentException ();
			}
		}

		protected int WriteAnonLabel ()
		{
			int index = ++next_anon_label_idx;

			WriteAnonLabel (index);

			return index;
		}

		protected void AddRelocEntry (RelocEntry entry)
		{
			int i = reloc_entries.Length;
			RelocEntry[] new_r = new RelocEntry [i + 1];
			Array.Copy (reloc_entries, new_r, i);
			new_r [i] = entry;
			reloc_entries = new_r;
		}

		protected void AddRelocEntry (RelocEntryType type, Section section, int index)
		{
			AddRelocEntry (new RelocEntry (type, section, index));
		}

		protected void AddRelocEntry (RelocEntryType type)
		{
			AddRelocEntry (type, current_section, WriteAnonLabel ());
		}


		//
		// Mono relocation table. See the README.relocation-table file in this
		// directory for a detailed description of the file format.
		//
		protected void WriteRelocEntries ()
		{
			WriteSectionStart (Section.MONO_RELOC_TABLE);
			WriteUInt16 (reloc_table_version);
			WriteUInt8 (0);
			int end_index = WriteSectionSize ();

			foreach (RelocEntry entry in reloc_entries) {
				WriteUInt8 ((int) entry.RelocType);
				int tmp_index = WriteSectionSize ();

				WriteUInt8 ((int) entry.Section);
				WriteUInt16 (entry.Index);

				WriteAnonLabel (tmp_index);
			}

			WriteAnonLabel (end_index);
			WriteSectionEnd ();
		}

		//
		// Registers a new abbreviation declaration.
		//
		// This function should be called by a static constructor in one of
		// Die's subclasses.
		//
		protected static int RegisterAbbrevDeclaration (AbbrevDeclaration decl)
		{
			if (abbrev_declarations != null) {
				int i = abbrev_declarations.Length;
				AbbrevDeclaration[] new_a = new AbbrevDeclaration [i + 1];
				Array.Copy (abbrev_declarations, new_a, i);
				new_a [i] = decl;
				abbrev_declarations = new_a;
				return i + 1;
			} else {
				abbrev_declarations = new AbbrevDeclaration [1];
				abbrev_declarations [0] = decl;
				return 1;
			}
		}

		protected static AbbrevDeclaration GetAbbrevDeclaration (int index)
		{
			return abbrev_declarations [index - 1];
		}

		protected void WriteAbbrevDeclarations ()
		{
			WriteSectionStart (Section.DEBUG_ABBREV);
			WriteLabel ("debug_abbrev_b");

			for (int index = 0; index < abbrev_declarations.Length; index++) {
				AbbrevDeclaration decl = abbrev_declarations [index];

				WriteULeb128 (index + 1);
				WriteULeb128 ((int) decl.Tag);
				WriteFlag (decl.HasChildren);

				foreach (AbbrevEntry entry in decl.Entries)
					WritePair ((int) entry.Attribute, (int) entry.Form);

				WritePair (0, 0);
			}

			WriteSectionEnd ();
		}

		protected void WriteAnonLabel (int index)
		{
			writer.WriteLine (".L_" + index + ":");
		}

		protected void WriteLabel (string label)
		{
			writer.WriteLine (label + ":");
		}

		protected int WriteSectionSize ()
		{
			int start_index = ++next_anon_label_idx;
			int end_index = ++next_anon_label_idx;

			writer.WriteLine ("\t.long\t\t.L_" + end_index + " - .L_" + start_index);
			WriteAnonLabel (start_index);

			return end_index;
		}

		protected void WriteRelativeReference (string start_label, string end_label)
		{
			writer.WriteLine ("\t.long\t\t" + end_label + " - " + start_label);
		}

		protected void WriteSectionStart (Section section)
		{
			writer.WriteLine ("\t.section\t." + GetSectionName (section));
			current_section = section;
		}

		protected void WriteSectionEnd ()
		{
			writer.WriteLine ("\t.previous\n");
		}

		protected void WriteFlag (bool value)
		{
			writer.WriteLine ("\t.byte\t\t" + (value ? 1 : 0));
		}

		protected void WritePair (int key, int value)
		{
			writer.WriteLine ("\t.byte\t\t" + key + ", " + value);
		}

		protected void WriteUInt8 (int value)
		{
			writer.WriteLine ("\t.byte\t\t" + value);
		}

		protected void WriteUInt16 (int value)
		{
			writer.WriteLine ("\t.2byte\t\t" + value);
		}

		protected void WriteSLeb128 (int value)
		{
			writer.WriteLine ("\t.sleb128\t" + value);
		}

		protected void WriteULeb128 (int value)
		{
			writer.WriteLine ("\t.uleb128\t" + value);
		}

		protected void WriteOffset (string section)
		{
			writer.WriteLine ("\t.long\t\t" + section);
		}

		protected void WriteAddress (int value)
		{
			writer.WriteLine ("\t.long\t\t" + value);
		}

		protected void WriteString (string value)
		{
			writer.WriteLine ("\t.string\t\t\"" + value + "\"");
		}
	}
}
