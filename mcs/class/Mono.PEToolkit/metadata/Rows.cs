// Auto-generated file - DO NOT EDIT!
// Please edit md-schema.xml or rows.xsl if you want to make changes.

using System;
using System.IO;

namespace Mono.PEToolkit.Metadata {



	/// <summary>
	///  Represents row in Module table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.27
	/// </remarks>
	public class ModuleRow : Row {

		private MDTable table;

		
		public ushort Generation;
		public int Name;
		public int Mvid;
		public int EncId;
		public int EncBaseId;

		public ModuleRow()
		{
		}

		public ModuleRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in Module table has 5 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 5;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return sizeof (ushort) + 4 + 4 + 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Generation = LEBitConverter.ToUInt16(buff, offs);
			offs += sizeof (ushort);
			this.Name = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.Mvid = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.EncId = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.EncBaseId = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Generation        : {0}" + Environment.NewLine + 
				"Name              : {1}" + Environment.NewLine + 
				"Mvid              : {2}" + Environment.NewLine + 
				"EncId             : {3}" + Environment.NewLine + 
				"EncBaseId         : {4}" + Environment.NewLine,
				this.Generation,
				(Table == null) ? Name.ToString() : "\"" + ((Table.Heap.Stream.Root.Streams["#Strings"] as MDStream).Heap as StringsHeap) [Name] + "\" (#Strings[0x" + Name.ToString("X") + "])",
				"#GUID[" + Mvid.ToString() + "]",
				"#GUID[" + EncId.ToString() + "]",
				"#GUID[" + EncBaseId.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in TypeRef table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.35
	/// </remarks>
	public class TypeRefRow : Row {

		private MDTable table;

		
		public MDToken ResolutionScope;
		public int Name;
		public int Namespace;

		public TypeRefRow()
		{
		}

		public TypeRefRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in TypeRef table has 3 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 3;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return 4 + 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.ResolutionScope = TabsDecoder.DecodeToken(CodedTokenId.ResolutionScope, LEBitConverter.ToInt32(buff, offs));
			offs += 4;
			this.Name = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.Namespace = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"ResolutionScope   : {0}" + Environment.NewLine + 
				"Name              : {1}" + Environment.NewLine + 
				"Namespace         : {2}" + Environment.NewLine,
				this.ResolutionScope,
				(Table == null) ? Name.ToString() : "\"" + ((Table.Heap.Stream.Root.Streams["#Strings"] as MDStream).Heap as StringsHeap) [Name] + "\" (#Strings[0x" + Name.ToString("X") + "])",
				(Table == null) ? Namespace.ToString() : "\"" + ((Table.Heap.Stream.Root.Streams["#Strings"] as MDStream).Heap as StringsHeap) [Namespace] + "\" (#Strings[0x" + Namespace.ToString("X") + "])"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in TypeDef table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.34
	/// </remarks>
	public class TypeDefRow : Row {

		private MDTable table;

		
		public System.Reflection.TypeAttributes Flags;
		public int Name;
		public int Namespace;
		public MDToken Extends;
		public int FieldList;
		public int MethodList;

		public TypeDefRow()
		{
		}

		public TypeDefRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in TypeDef table has 6 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 6;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return sizeof (uint) + 4 + 4 + 4 + 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Flags = (System.Reflection.TypeAttributes) LEBitConverter.ToUInt32(buff, offs);
			offs += sizeof (uint);
			this.Name = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.Namespace = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.Extends = TabsDecoder.DecodeToken(CodedTokenId.TypeDefOrRef, LEBitConverter.ToInt32(buff, offs));
			offs += 4;
			this.FieldList = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.MethodList = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Flags             : {0}" + Environment.NewLine + 
				"Name              : {1}" + Environment.NewLine + 
				"Namespace         : {2}" + Environment.NewLine + 
				"Extends           : {3}" + Environment.NewLine + 
				"FieldList         : {4}" + Environment.NewLine + 
				"MethodList        : {5}" + Environment.NewLine,
				(int)this.Flags,
				(Table == null) ? Name.ToString() : "\"" + ((Table.Heap.Stream.Root.Streams["#Strings"] as MDStream).Heap as StringsHeap) [Name] + "\" (#Strings[0x" + Name.ToString("X") + "])",
				(Table == null) ? Namespace.ToString() : "\"" + ((Table.Heap.Stream.Root.Streams["#Strings"] as MDStream).Heap as StringsHeap) [Namespace] + "\" (#Strings[0x" + Namespace.ToString("X") + "])",
				this.Extends,
				"Field[" + FieldList.ToString() + "]",
				"Method[" + MethodList.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in FieldPtr table.
	/// </summary>
	/// <remarks>
	///  
	/// </remarks>
	public class FieldPtrRow : Row {

		private MDTable table;

		
		public int Field;

		public FieldPtrRow()
		{
		}

		public FieldPtrRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in FieldPtr table has 1 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 1;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Field = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Field             : {0}" + Environment.NewLine,
				"Field[" + Field.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in Field table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.15
	/// </remarks>
	public class FieldRow : Row {

		private MDTable table;

		
		public System.Reflection.FieldAttributes Flags;
		public int Name;
		public int Signature;

		public FieldRow()
		{
		}

		public FieldRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in Field table has 3 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 3;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return sizeof (ushort) + 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Flags = (System.Reflection.FieldAttributes) LEBitConverter.ToUInt16(buff, offs);
			offs += sizeof (ushort);
			this.Name = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.Signature = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Flags             : {0}" + Environment.NewLine + 
				"Name              : {1}" + Environment.NewLine + 
				"Signature         : {2}" + Environment.NewLine,
				(int)this.Flags,
				(Table == null) ? Name.ToString() : "\"" + ((Table.Heap.Stream.Root.Streams["#Strings"] as MDStream).Heap as StringsHeap) [Name] + "\" (#Strings[0x" + Name.ToString("X") + "])",
				"#Blob[" + Signature.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in MethodPtr table.
	/// </summary>
	/// <remarks>
	///  
	/// </remarks>
	public class MethodPtrRow : Row {

		private MDTable table;

		
		public int Method;

		public MethodPtrRow()
		{
		}

		public MethodPtrRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in MethodPtr table has 1 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 1;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Method = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Method            : {0}" + Environment.NewLine,
				"Method[" + Method.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in Method table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.24
	/// </remarks>
	public class MethodRow : Row {

		private MDTable table;

		
		public RVA RVA;
		public System.Reflection.MethodImplAttributes ImplFlags;
		public System.Reflection.MethodAttributes Flags;
		public int Name;
		public int Signature;
		public int ParamList;

		public MethodRow()
		{
		}

		public MethodRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in Method table has 6 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 6;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return RVA.Size + sizeof (ushort) + sizeof (ushort) + 4 + 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.RVA = LEBitConverter.ToUInt32(buff, offs);
			offs += RVA.Size;
			this.ImplFlags = (System.Reflection.MethodImplAttributes) LEBitConverter.ToUInt16(buff, offs);
			offs += sizeof (ushort);
			this.Flags = (System.Reflection.MethodAttributes) LEBitConverter.ToUInt16(buff, offs);
			offs += sizeof (ushort);
			this.Name = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.Signature = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.ParamList = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"RVA               : {0}" + Environment.NewLine + 
				"ImplFlags         : {1}" + Environment.NewLine + 
				"Flags             : {2}" + Environment.NewLine + 
				"Name              : {3}" + Environment.NewLine + 
				"Signature         : {4}" + Environment.NewLine + 
				"ParamList         : {5}" + Environment.NewLine,
				this.RVA,
				this.ImplFlags,
				(int)this.Flags,
				(Table == null) ? Name.ToString() : "\"" + ((Table.Heap.Stream.Root.Streams["#Strings"] as MDStream).Heap as StringsHeap) [Name] + "\" (#Strings[0x" + Name.ToString("X") + "])",
				"#Blob[" + Signature.ToString() + "]",
				"Param[" + ParamList.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in ParamPtr table.
	/// </summary>
	/// <remarks>
	///  
	/// </remarks>
	public class ParamPtrRow : Row {

		private MDTable table;

		
		public int Param;

		public ParamPtrRow()
		{
		}

		public ParamPtrRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in ParamPtr table has 1 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 1;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Param = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Param             : {0}" + Environment.NewLine,
				"Param[" + Param.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in Param table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.30
	/// </remarks>
	public class ParamRow : Row {

		private MDTable table;

		
		public System.Reflection.ParameterAttributes Flags;
		public ushort Sequence;
		public int Name;

		public ParamRow()
		{
		}

		public ParamRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in Param table has 3 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 3;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return sizeof (ushort) + sizeof (ushort) + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Flags = (System.Reflection.ParameterAttributes) LEBitConverter.ToUInt16(buff, offs);
			offs += sizeof (ushort);
			this.Sequence = LEBitConverter.ToUInt16(buff, offs);
			offs += sizeof (ushort);
			this.Name = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Flags             : {0}" + Environment.NewLine + 
				"Sequence          : {1}" + Environment.NewLine + 
				"Name              : {2}" + Environment.NewLine,
				(int)this.Flags,
				this.Sequence,
				(Table == null) ? Name.ToString() : "\"" + ((Table.Heap.Stream.Root.Streams["#Strings"] as MDStream).Heap as StringsHeap) [Name] + "\" (#Strings[0x" + Name.ToString("X") + "])"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in InterfaceImpl table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.21
	/// </remarks>
	public class InterfaceImplRow : Row {

		private MDTable table;

		
		public int Class;
		public MDToken Interface;

		public InterfaceImplRow()
		{
		}

		public InterfaceImplRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in InterfaceImpl table has 2 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 2;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Class = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.Interface = TabsDecoder.DecodeToken(CodedTokenId.TypeDefOrRef, LEBitConverter.ToInt32(buff, offs));
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Class             : {0}" + Environment.NewLine + 
				"Interface         : {1}" + Environment.NewLine,
				"TypeDef[" + Class.ToString() + "]",
				this.Interface
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in MemberRef table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.23
	/// </remarks>
	public class MemberRefRow : Row {

		private MDTable table;

		
		public MDToken Class;
		public int Name;
		public int Signature;

		public MemberRefRow()
		{
		}

		public MemberRefRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in MemberRef table has 3 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 3;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return 4 + 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Class = TabsDecoder.DecodeToken(CodedTokenId.MemberRefParent, LEBitConverter.ToInt32(buff, offs));
			offs += 4;
			this.Name = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.Signature = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Class             : {0}" + Environment.NewLine + 
				"Name              : {1}" + Environment.NewLine + 
				"Signature         : {2}" + Environment.NewLine,
				this.Class,
				(Table == null) ? Name.ToString() : "\"" + ((Table.Heap.Stream.Root.Streams["#Strings"] as MDStream).Heap as StringsHeap) [Name] + "\" (#Strings[0x" + Name.ToString("X") + "])",
				"#Blob[" + Signature.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in Constant table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.9
	/// </remarks>
	public class ConstantRow : Row {

		private MDTable table;

		
		public ElementType Type;
		public MDToken Parent;
		public int Value;

		public ConstantRow()
		{
		}

		public ConstantRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in Constant table has 3 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 3;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return sizeof (short) + 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Type = (ElementType) LEBitConverter.ToInt16(buff, offs);
			offs += sizeof (short);
			this.Parent = TabsDecoder.DecodeToken(CodedTokenId.HasConstant, LEBitConverter.ToInt32(buff, offs));
			offs += 4;
			this.Value = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Type              : {0}" + Environment.NewLine + 
				"Parent            : {1}" + Environment.NewLine + 
				"Value             : {2}" + Environment.NewLine,
				this.Type,
				this.Parent,
				"#Blob[" + Value.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in CustomAttribute table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.10
	/// </remarks>
	public class CustomAttributeRow : Row {

		private MDTable table;

		
		public MDToken Parent;
		public MDToken Type;
		public int Value;

		public CustomAttributeRow()
		{
		}

		public CustomAttributeRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in CustomAttribute table has 3 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 3;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return 4 + 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Parent = TabsDecoder.DecodeToken(CodedTokenId.HasCustomAttribute, LEBitConverter.ToInt32(buff, offs));
			offs += 4;
			this.Type = TabsDecoder.DecodeToken(CodedTokenId.CustomAttributeType, LEBitConverter.ToInt32(buff, offs));
			offs += 4;
			this.Value = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Parent            : {0}" + Environment.NewLine + 
				"Type              : {1}" + Environment.NewLine + 
				"Value             : {2}" + Environment.NewLine,
				this.Parent,
				this.Type,
				"#Blob[" + Value.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in FieldMarshal table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.17
	/// </remarks>
	public class FieldMarshalRow : Row {

		private MDTable table;

		
		public MDToken Parent;
		public int NativeType;

		public FieldMarshalRow()
		{
		}

		public FieldMarshalRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in FieldMarshal table has 2 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 2;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Parent = TabsDecoder.DecodeToken(CodedTokenId.HasFieldMarshal, LEBitConverter.ToInt32(buff, offs));
			offs += 4;
			this.NativeType = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Parent            : {0}" + Environment.NewLine + 
				"NativeType        : {1}" + Environment.NewLine,
				this.Parent,
				"#Blob[" + NativeType.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in DeclSecurity table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.11
	/// </remarks>
	public class DeclSecurityRow : Row {

		private MDTable table;

		
		public short Action;
		public MDToken Parent;
		public int PermissionSet;

		public DeclSecurityRow()
		{
		}

		public DeclSecurityRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in DeclSecurity table has 3 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 3;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return sizeof (short) + 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Action = LEBitConverter.ToInt16(buff, offs);
			offs += sizeof (short);
			this.Parent = TabsDecoder.DecodeToken(CodedTokenId.HasDeclSecurity, LEBitConverter.ToInt32(buff, offs));
			offs += 4;
			this.PermissionSet = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Action            : {0}" + Environment.NewLine + 
				"Parent            : {1}" + Environment.NewLine + 
				"PermissionSet     : {2}" + Environment.NewLine,
				this.Action,
				this.Parent,
				"#Blob[" + PermissionSet.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in ClassLayout table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.8
	/// </remarks>
	public class ClassLayoutRow : Row {

		private MDTable table;

		
		public short PackingSize;
		public int ClassSize;
		public int Parent;

		public ClassLayoutRow()
		{
		}

		public ClassLayoutRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in ClassLayout table has 3 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 3;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return sizeof (short) + sizeof (int) + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.PackingSize = LEBitConverter.ToInt16(buff, offs);
			offs += sizeof (short);
			this.ClassSize = LEBitConverter.ToInt32(buff, offs);
			offs += sizeof (int);
			this.Parent = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"PackingSize       : {0}" + Environment.NewLine + 
				"ClassSize         : {1}" + Environment.NewLine + 
				"Parent            : {2}" + Environment.NewLine,
				this.PackingSize,
				this.ClassSize,
				"TypeDef[" + Parent.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in FieldLayout table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.16
	/// </remarks>
	public class FieldLayoutRow : Row {

		private MDTable table;

		
		public int Offset;
		public int Field;

		public FieldLayoutRow()
		{
		}

		public FieldLayoutRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in FieldLayout table has 2 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 2;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return sizeof (int) + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Offset = LEBitConverter.ToInt32(buff, offs);
			offs += sizeof (int);
			this.Field = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Offset            : {0}" + Environment.NewLine + 
				"Field             : {1}" + Environment.NewLine,
				this.Offset,
				"Field[" + Field.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in StandAloneSig table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.33
	/// </remarks>
	public class StandAloneSigRow : Row {

		private MDTable table;

		
		public int Signature;

		public StandAloneSigRow()
		{
		}

		public StandAloneSigRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in StandAloneSig table has 1 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 1;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Signature = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Signature         : {0}" + Environment.NewLine,
				"#Blob[" + Signature.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in EventMap table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.12
	/// </remarks>
	public class EventMapRow : Row {

		private MDTable table;

		
		public int Parent;
		public int EventList;

		public EventMapRow()
		{
		}

		public EventMapRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in EventMap table has 2 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 2;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Parent = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.EventList = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Parent            : {0}" + Environment.NewLine + 
				"EventList         : {1}" + Environment.NewLine,
				"TypeDef[" + Parent.ToString() + "]",
				"Event[" + EventList.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in EventPtr table.
	/// </summary>
	/// <remarks>
	///  
	/// </remarks>
	public class EventPtrRow : Row {

		private MDTable table;

		
		public int Event;

		public EventPtrRow()
		{
		}

		public EventPtrRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in EventPtr table has 1 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 1;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Event = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Event             : {0}" + Environment.NewLine,
				"Event[" + Event.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in Event table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.13
	/// </remarks>
	public class EventRow : Row {

		private MDTable table;

		
		public System.Reflection.EventAttributes EventFlags;
		public int Name;
		public MDToken EventType;

		public EventRow()
		{
		}

		public EventRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in Event table has 3 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 3;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return sizeof (short) + 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.EventFlags = (System.Reflection.EventAttributes) LEBitConverter.ToInt16(buff, offs);
			offs += sizeof (short);
			this.Name = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.EventType = TabsDecoder.DecodeToken(CodedTokenId.TypeDefOrRef, LEBitConverter.ToInt32(buff, offs));
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"EventFlags        : {0}" + Environment.NewLine + 
				"Name              : {1}" + Environment.NewLine + 
				"EventType         : {2}" + Environment.NewLine,
				this.EventFlags,
				(Table == null) ? Name.ToString() : "\"" + ((Table.Heap.Stream.Root.Streams["#Strings"] as MDStream).Heap as StringsHeap) [Name] + "\" (#Strings[0x" + Name.ToString("X") + "])",
				this.EventType
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in PropertyMap table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.32
	/// </remarks>
	public class PropertyMapRow : Row {

		private MDTable table;

		
		public int Parent;
		public int PropertyList;

		public PropertyMapRow()
		{
		}

		public PropertyMapRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in PropertyMap table has 2 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 2;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Parent = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.PropertyList = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Parent            : {0}" + Environment.NewLine + 
				"PropertyList      : {1}" + Environment.NewLine,
				"TypeDef[" + Parent.ToString() + "]",
				"Property[" + PropertyList.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in PropertyPtr table.
	/// </summary>
	/// <remarks>
	///  
	/// </remarks>
	public class PropertyPtrRow : Row {

		private MDTable table;

		
		public int Property;

		public PropertyPtrRow()
		{
		}

		public PropertyPtrRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in PropertyPtr table has 1 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 1;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Property = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Property          : {0}" + Environment.NewLine,
				"Property[" + Property.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in Property table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.30
	/// </remarks>
	public class PropertyRow : Row {

		private MDTable table;

		
		public System.Reflection.PropertyAttributes Flags;
		public int Name;
		public int Type;

		public PropertyRow()
		{
		}

		public PropertyRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in Property table has 3 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 3;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return sizeof (ushort) + 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Flags = (System.Reflection.PropertyAttributes) LEBitConverter.ToUInt16(buff, offs);
			offs += sizeof (ushort);
			this.Name = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.Type = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Flags             : {0}" + Environment.NewLine + 
				"Name              : {1}" + Environment.NewLine + 
				"Type              : {2}" + Environment.NewLine,
				(int)this.Flags,
				(Table == null) ? Name.ToString() : "\"" + ((Table.Heap.Stream.Root.Streams["#Strings"] as MDStream).Heap as StringsHeap) [Name] + "\" (#Strings[0x" + Name.ToString("X") + "])",
				"#Blob[" + Type.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in MethodSemantics table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.26
	/// </remarks>
	public class MethodSemanticsRow : Row {

		private MDTable table;

		
		public MethodSemanticsAttributes Semantics;
		public int Method;
		public MDToken Association;

		public MethodSemanticsRow()
		{
		}

		public MethodSemanticsRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in MethodSemantics table has 3 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 3;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return sizeof (ushort) + 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Semantics = (MethodSemanticsAttributes) LEBitConverter.ToUInt16(buff, offs);
			offs += sizeof (ushort);
			this.Method = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.Association = TabsDecoder.DecodeToken(CodedTokenId.HasSemantics, LEBitConverter.ToInt32(buff, offs));
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Semantics         : {0}" + Environment.NewLine + 
				"Method            : {1}" + Environment.NewLine + 
				"Association       : {2}" + Environment.NewLine,
				(int)this.Semantics,
				"Method[" + Method.ToString() + "]",
				this.Association
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in MethodImpl table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.25
	/// </remarks>
	public class MethodImplRow : Row {

		private MDTable table;

		
		public int Class;
		public MDToken MethodBody;
		public MDToken MethodDeclaration;

		public MethodImplRow()
		{
		}

		public MethodImplRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in MethodImpl table has 3 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 3;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return 4 + 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Class = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.MethodBody = TabsDecoder.DecodeToken(CodedTokenId.MethodDefOrRef, LEBitConverter.ToInt32(buff, offs));
			offs += 4;
			this.MethodDeclaration = TabsDecoder.DecodeToken(CodedTokenId.MethodDefOrRef, LEBitConverter.ToInt32(buff, offs));
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Class             : {0}" + Environment.NewLine + 
				"MethodBody        : {1}" + Environment.NewLine + 
				"MethodDeclaration : {2}" + Environment.NewLine,
				"TypeDef[" + Class.ToString() + "]",
				this.MethodBody,
				this.MethodDeclaration
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in ModuleRef table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.28
	/// </remarks>
	public class ModuleRefRow : Row {

		private MDTable table;

		
		public int Name;

		public ModuleRefRow()
		{
		}

		public ModuleRefRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in ModuleRef table has 1 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 1;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Name = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Name              : {0}" + Environment.NewLine,
				(Table == null) ? Name.ToString() : "\"" + ((Table.Heap.Stream.Root.Streams["#Strings"] as MDStream).Heap as StringsHeap) [Name] + "\" (#Strings[0x" + Name.ToString("X") + "])"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in TypeSpec table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.36
	/// </remarks>
	public class TypeSpecRow : Row {

		private MDTable table;

		
		public int Signature;

		public TypeSpecRow()
		{
		}

		public TypeSpecRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in TypeSpec table has 1 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 1;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Signature = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Signature         : {0}" + Environment.NewLine,
				"#Blob[" + Signature.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in ImplMap table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.20
	/// </remarks>
	public class ImplMapRow : Row {

		private MDTable table;

		
		public PInvokeAttributes MappingFlags;
		public MDToken MemberForwarded;
		public int ImportName;
		public int ImportScope;

		public ImplMapRow()
		{
		}

		public ImplMapRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in ImplMap table has 4 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 4;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return sizeof (ushort) + 4 + 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.MappingFlags = (PInvokeAttributes) LEBitConverter.ToUInt16(buff, offs);
			offs += sizeof (ushort);
			this.MemberForwarded = TabsDecoder.DecodeToken(CodedTokenId.MemberForwarded, LEBitConverter.ToInt32(buff, offs));
			offs += 4;
			this.ImportName = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.ImportScope = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"MappingFlags      : {0}" + Environment.NewLine + 
				"MemberForwarded   : {1}" + Environment.NewLine + 
				"ImportName        : {2}" + Environment.NewLine + 
				"ImportScope       : {3}" + Environment.NewLine,
				this.MappingFlags,
				this.MemberForwarded,
				(Table == null) ? ImportName.ToString() : "\"" + ((Table.Heap.Stream.Root.Streams["#Strings"] as MDStream).Heap as StringsHeap) [ImportName] + "\" (#Strings[0x" + ImportName.ToString("X") + "])",
				"ModuleRef[" + ImportScope.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in FieldRVA table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.18
	/// </remarks>
	public class FieldRVARow : Row {

		private MDTable table;

		
		public RVA RVA;
		public int Field;

		public FieldRVARow()
		{
		}

		public FieldRVARow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in FieldRVA table has 2 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 2;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return RVA.Size + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.RVA = LEBitConverter.ToUInt32(buff, offs);
			offs += RVA.Size;
			this.Field = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"RVA               : {0}" + Environment.NewLine + 
				"Field             : {1}" + Environment.NewLine,
				this.RVA,
				"Field[" + Field.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in ENCLog table.
	/// </summary>
	/// <remarks>
	///  
	/// </remarks>
	public class ENCLogRow : Row {

		private MDTable table;

		
		public uint Token;
		public uint FuncCode;

		public ENCLogRow()
		{
		}

		public ENCLogRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in ENCLog table has 2 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 2;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return sizeof (uint) + sizeof (uint);
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Token = LEBitConverter.ToUInt32(buff, offs);
			offs += sizeof (uint);
			this.FuncCode = LEBitConverter.ToUInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Token             : {0}" + Environment.NewLine + 
				"FuncCode          : {1}" + Environment.NewLine,
				this.Token,
				this.FuncCode
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in ENCMap table.
	/// </summary>
	/// <remarks>
	///  
	/// </remarks>
	public class ENCMapRow : Row {

		private MDTable table;

		
		public uint Token;

		public ENCMapRow()
		{
		}

		public ENCMapRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in ENCMap table has 1 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 1;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return sizeof (uint);
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Token = LEBitConverter.ToUInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Token             : {0}" + Environment.NewLine,
				this.Token
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in Assembly table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.2
	/// </remarks>
	public class AssemblyRow : Row {

		private MDTable table;

		
		public System.Configuration.Assemblies.AssemblyHashAlgorithm HashAlgId;
		public short MajorVersion;
		public short MinorVersion;
		public short BuildNumber;
		public short RevisionNumber;
		public AssemblyFlags Flags;
		public int PublicKey;
		public int Name;
		public int Culture;

		public AssemblyRow()
		{
		}

		public AssemblyRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in Assembly table has 9 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 9;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return sizeof (int) + sizeof (short) + sizeof (short) + sizeof (short) + sizeof (short) + sizeof (uint) + 4 + 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.HashAlgId = (System.Configuration.Assemblies.AssemblyHashAlgorithm) LEBitConverter.ToInt32(buff, offs);
			offs += sizeof (int);
			this.MajorVersion = LEBitConverter.ToInt16(buff, offs);
			offs += sizeof (short);
			this.MinorVersion = LEBitConverter.ToInt16(buff, offs);
			offs += sizeof (short);
			this.BuildNumber = LEBitConverter.ToInt16(buff, offs);
			offs += sizeof (short);
			this.RevisionNumber = LEBitConverter.ToInt16(buff, offs);
			offs += sizeof (short);
			this.Flags = (AssemblyFlags) LEBitConverter.ToUInt32(buff, offs);
			offs += sizeof (uint);
			this.PublicKey = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.Name = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.Culture = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"HashAlgId         : {0}" + Environment.NewLine + 
				"MajorVersion      : {1}" + Environment.NewLine + 
				"MinorVersion      : {2}" + Environment.NewLine + 
				"BuildNumber       : {3}" + Environment.NewLine + 
				"RevisionNumber    : {4}" + Environment.NewLine + 
				"Flags             : {5}" + Environment.NewLine + 
				"PublicKey         : {6}" + Environment.NewLine + 
				"Name              : {7}" + Environment.NewLine + 
				"Culture           : {8}" + Environment.NewLine,
				this.HashAlgId,
				this.MajorVersion,
				this.MinorVersion,
				this.BuildNumber,
				this.RevisionNumber,
				(int)this.Flags,
				"#Blob[" + PublicKey.ToString() + "]",
				(Table == null) ? Name.ToString() : "\"" + ((Table.Heap.Stream.Root.Streams["#Strings"] as MDStream).Heap as StringsHeap) [Name] + "\" (#Strings[0x" + Name.ToString("X") + "])",
				(Table == null) ? Culture.ToString() : "\"" + ((Table.Heap.Stream.Root.Streams["#Strings"] as MDStream).Heap as StringsHeap) [Culture] + "\" (#Strings[0x" + Culture.ToString("X") + "])"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in AssemblyProcessor table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.4
	/// </remarks>
	public class AssemblyProcessorRow : Row {

		private MDTable table;

		
		public int Processor;

		public AssemblyProcessorRow()
		{
		}

		public AssemblyProcessorRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in AssemblyProcessor table has 1 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 1;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return sizeof (int);
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Processor = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Processor         : {0}" + Environment.NewLine,
				this.Processor
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in AssemblyOS table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.3
	/// </remarks>
	public class AssemblyOSRow : Row {

		private MDTable table;

		
		public int OSPlatformID;
		public int OSMajorVersion;
		public int OSMinorVersion;

		public AssemblyOSRow()
		{
		}

		public AssemblyOSRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in AssemblyOS table has 3 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 3;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return sizeof (int) + sizeof (int) + sizeof (int);
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.OSPlatformID = LEBitConverter.ToInt32(buff, offs);
			offs += sizeof (int);
			this.OSMajorVersion = LEBitConverter.ToInt32(buff, offs);
			offs += sizeof (int);
			this.OSMinorVersion = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"OSPlatformID      : {0}" + Environment.NewLine + 
				"OSMajorVersion    : {1}" + Environment.NewLine + 
				"OSMinorVersion    : {2}" + Environment.NewLine,
				this.OSPlatformID,
				this.OSMajorVersion,
				this.OSMinorVersion
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in AssemblyRef table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.5
	/// </remarks>
	public class AssemblyRefRow : Row {

		private MDTable table;

		
		public short MajorVersion;
		public short MinorVersion;
		public short BuildNumber;
		public short RevisionNumber;
		public AssemblyFlags Flags;
		public int PublicKeyOrToken;
		public int Name;
		public int Culture;
		public int HashValue;

		public AssemblyRefRow()
		{
		}

		public AssemblyRefRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in AssemblyRef table has 9 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 9;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return sizeof (short) + sizeof (short) + sizeof (short) + sizeof (short) + sizeof (uint) + 4 + 4 + 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.MajorVersion = LEBitConverter.ToInt16(buff, offs);
			offs += sizeof (short);
			this.MinorVersion = LEBitConverter.ToInt16(buff, offs);
			offs += sizeof (short);
			this.BuildNumber = LEBitConverter.ToInt16(buff, offs);
			offs += sizeof (short);
			this.RevisionNumber = LEBitConverter.ToInt16(buff, offs);
			offs += sizeof (short);
			this.Flags = (AssemblyFlags) LEBitConverter.ToUInt32(buff, offs);
			offs += sizeof (uint);
			this.PublicKeyOrToken = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.Name = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.Culture = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.HashValue = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"MajorVersion      : {0}" + Environment.NewLine + 
				"MinorVersion      : {1}" + Environment.NewLine + 
				"BuildNumber       : {2}" + Environment.NewLine + 
				"RevisionNumber    : {3}" + Environment.NewLine + 
				"Flags             : {4}" + Environment.NewLine + 
				"PublicKeyOrToken  : {5}" + Environment.NewLine + 
				"Name              : {6}" + Environment.NewLine + 
				"Culture           : {7}" + Environment.NewLine + 
				"HashValue         : {8}" + Environment.NewLine,
				this.MajorVersion,
				this.MinorVersion,
				this.BuildNumber,
				this.RevisionNumber,
				(int)this.Flags,
				"#Blob[" + PublicKeyOrToken.ToString() + "]",
				(Table == null) ? Name.ToString() : "\"" + ((Table.Heap.Stream.Root.Streams["#Strings"] as MDStream).Heap as StringsHeap) [Name] + "\" (#Strings[0x" + Name.ToString("X") + "])",
				(Table == null) ? Culture.ToString() : "\"" + ((Table.Heap.Stream.Root.Streams["#Strings"] as MDStream).Heap as StringsHeap) [Culture] + "\" (#Strings[0x" + Culture.ToString("X") + "])",
				"#Blob[" + HashValue.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in AssemblyRefProcessor table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.7
	/// </remarks>
	public class AssemblyRefProcessorRow : Row {

		private MDTable table;

		
		public int Processor;
		public int AssemblyRef;

		public AssemblyRefProcessorRow()
		{
		}

		public AssemblyRefProcessorRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in AssemblyRefProcessor table has 2 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 2;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return sizeof (int) + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Processor = LEBitConverter.ToInt32(buff, offs);
			offs += sizeof (int);
			this.AssemblyRef = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Processor         : {0}" + Environment.NewLine + 
				"AssemblyRef       : {1}" + Environment.NewLine,
				this.Processor,
				"AssemblyRef[" + AssemblyRef.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in AssemblyRefOS table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.6
	/// </remarks>
	public class AssemblyRefOSRow : Row {

		private MDTable table;

		
		public int OSPlatformID;
		public int OSMajorVersion;
		public int OSMinorVersion;
		public int AssemblyRef;

		public AssemblyRefOSRow()
		{
		}

		public AssemblyRefOSRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in AssemblyRefOS table has 4 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 4;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return sizeof (int) + sizeof (int) + sizeof (int) + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.OSPlatformID = LEBitConverter.ToInt32(buff, offs);
			offs += sizeof (int);
			this.OSMajorVersion = LEBitConverter.ToInt32(buff, offs);
			offs += sizeof (int);
			this.OSMinorVersion = LEBitConverter.ToInt32(buff, offs);
			offs += sizeof (int);
			this.AssemblyRef = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"OSPlatformID      : {0}" + Environment.NewLine + 
				"OSMajorVersion    : {1}" + Environment.NewLine + 
				"OSMinorVersion    : {2}" + Environment.NewLine + 
				"AssemblyRef       : {3}" + Environment.NewLine,
				this.OSPlatformID,
				this.OSMajorVersion,
				this.OSMinorVersion,
				"AssemblyRef[" + AssemblyRef.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in File table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.19
	/// </remarks>
	public class FileRow : Row {

		private MDTable table;

		
		public System.IO.FileAttributes Flags;
		public int Name;
		public int HashValue;

		public FileRow()
		{
		}

		public FileRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in File table has 3 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 3;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return sizeof (uint) + 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Flags = (System.IO.FileAttributes) LEBitConverter.ToUInt32(buff, offs);
			offs += sizeof (uint);
			this.Name = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.HashValue = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Flags             : {0}" + Environment.NewLine + 
				"Name              : {1}" + Environment.NewLine + 
				"HashValue         : {2}" + Environment.NewLine,
				(int)this.Flags,
				(Table == null) ? Name.ToString() : "\"" + ((Table.Heap.Stream.Root.Streams["#Strings"] as MDStream).Heap as StringsHeap) [Name] + "\" (#Strings[0x" + Name.ToString("X") + "])",
				"#Blob[" + HashValue.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in ExportedType table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.14
	/// </remarks>
	public class ExportedTypeRow : Row {

		private MDTable table;

		
		public System.Reflection.TypeAttributes Flags;
		public int TypeDefId;
		public int TypeName;
		public int TypeNamespace;
		public MDToken Implementation;

		public ExportedTypeRow()
		{
		}

		public ExportedTypeRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in ExportedType table has 5 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 5;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return sizeof (uint) + 4 + 4 + 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Flags = (System.Reflection.TypeAttributes) LEBitConverter.ToUInt32(buff, offs);
			offs += sizeof (uint);
			this.TypeDefId = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.TypeName = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.TypeNamespace = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.Implementation = TabsDecoder.DecodeToken(CodedTokenId.Implementation, LEBitConverter.ToInt32(buff, offs));
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Flags             : {0}" + Environment.NewLine + 
				"TypeDefId         : {1}" + Environment.NewLine + 
				"TypeName          : {2}" + Environment.NewLine + 
				"TypeNamespace     : {3}" + Environment.NewLine + 
				"Implementation    : {4}" + Environment.NewLine,
				(int)this.Flags,
				"TypeDef[" + TypeDefId.ToString() + "]",
				(Table == null) ? TypeName.ToString() : "\"" + ((Table.Heap.Stream.Root.Streams["#Strings"] as MDStream).Heap as StringsHeap) [TypeName] + "\" (#Strings[0x" + TypeName.ToString("X") + "])",
				(Table == null) ? TypeNamespace.ToString() : "\"" + ((Table.Heap.Stream.Root.Streams["#Strings"] as MDStream).Heap as StringsHeap) [TypeNamespace] + "\" (#Strings[0x" + TypeNamespace.ToString("X") + "])",
				this.Implementation
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in ManifestResource table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.22
	/// </remarks>
	public class ManifestResourceRow : Row {

		private MDTable table;

		
		public int Offset;
		public ManifestResourceAttributes Flags;
		public int Name;
		public MDToken Implementation;

		public ManifestResourceRow()
		{
		}

		public ManifestResourceRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in ManifestResource table has 4 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 4;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return sizeof (int) + sizeof (uint) + 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Offset = LEBitConverter.ToInt32(buff, offs);
			offs += sizeof (int);
			this.Flags = (ManifestResourceAttributes) LEBitConverter.ToUInt32(buff, offs);
			offs += sizeof (uint);
			this.Name = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.Implementation = TabsDecoder.DecodeToken(CodedTokenId.Implementation, LEBitConverter.ToInt32(buff, offs));
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Offset            : {0}" + Environment.NewLine + 
				"Flags             : {1}" + Environment.NewLine + 
				"Name              : {2}" + Environment.NewLine + 
				"Implementation    : {3}" + Environment.NewLine,
				this.Offset,
				(int)this.Flags,
				(Table == null) ? Name.ToString() : "\"" + ((Table.Heap.Stream.Root.Streams["#Strings"] as MDStream).Heap as StringsHeap) [Name] + "\" (#Strings[0x" + Name.ToString("X") + "])",
				this.Implementation
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in NestedClass table.
	/// </summary>
	/// <remarks>
	///  See Partition II, Metadata; section 21.29
	/// </remarks>
	public class NestedClassRow : Row {

		private MDTable table;

		
		public int NestedClass;
		public int EnclosingClass;

		public NestedClassRow()
		{
		}

		public NestedClassRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in NestedClass table has 2 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 2;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.NestedClass = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.EnclosingClass = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"NestedClass       : {0}" + Environment.NewLine + 
				"EnclosingClass    : {1}" + Environment.NewLine,
				"TypeDef[" + NestedClass.ToString() + "]",
				"TypeDef[" + EnclosingClass.ToString() + "]"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in TypeTyPar table.
	/// </summary>
	/// <remarks>
	///  
	/// </remarks>
	public class TypeTyParRow : Row {

		private MDTable table;

		
		public ushort Number;
		public int Class;
		public MDToken Bound;
		public int Name;

		public TypeTyParRow()
		{
		}

		public TypeTyParRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in TypeTyPar table has 4 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 4;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return sizeof (ushort) + 4 + 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Number = LEBitConverter.ToUInt16(buff, offs);
			offs += sizeof (ushort);
			this.Class = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.Bound = TabsDecoder.DecodeToken(CodedTokenId.TypeDefOrRef, LEBitConverter.ToInt32(buff, offs));
			offs += 4;
			this.Name = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Number            : {0}" + Environment.NewLine + 
				"Class             : {1}" + Environment.NewLine + 
				"Bound             : {2}" + Environment.NewLine + 
				"Name              : {3}" + Environment.NewLine,
				this.Number,
				"TypeDef[" + Class.ToString() + "]",
				this.Bound,
				(Table == null) ? Name.ToString() : "\"" + ((Table.Heap.Stream.Root.Streams["#Strings"] as MDStream).Heap as StringsHeap) [Name] + "\" (#Strings[0x" + Name.ToString("X") + "])"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}



	/// <summary>
	///  Represents row in MethodTyPar table.
	/// </summary>
	/// <remarks>
	///  
	/// </remarks>
	public class MethodTyParRow : Row {

		private MDTable table;

		
		public ushort Number;
		public int Method;
		public MDToken Bound;
		public int Name;

		public MethodTyParRow()
		{
		}

		public MethodTyParRow(MDTable parent)
		{
			table = parent;
		}


		/// <summary>
		///  Row in MethodTyPar table has 4 columns.
		/// </summary>
		public virtual int NumberOfColumns {
			get {
				return 4;
			}
		}


		/// <summary>
		///  Logical size of this instance in bytes.
		/// </summary>
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// <summary>
		/// </summary>
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// <summary>
		///  Logical size of this type of row in bytes.
		/// </summary>
		unsafe public static int LogicalSize {
			get {
				return sizeof (ushort) + 4 + 4 + 4;
			}
		}


		/// <summary>
		///  Fills the row from the array of bytes.
		/// </summary>
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		
			this.Number = LEBitConverter.ToUInt16(buff, offs);
			offs += sizeof (ushort);
			this.Method = LEBitConverter.ToInt32(buff, offs);
			offs += 4;
			this.Bound = TabsDecoder.DecodeToken(CodedTokenId.TypeDefOrRef, LEBitConverter.ToInt32(buff, offs));
			offs += 4;
			this.Name = LEBitConverter.ToInt32(buff, offs);
			
		}

		

		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Number            : {0}" + Environment.NewLine + 
				"Method            : {1}" + Environment.NewLine + 
				"Bound             : {2}" + Environment.NewLine + 
				"Name              : {3}" + Environment.NewLine,
				this.Number,
				"Method[" + Method.ToString() + "]",
				this.Bound,
				(Table == null) ? Name.ToString() : "\"" + ((Table.Heap.Stream.Root.Streams["#Strings"] as MDStream).Heap as StringsHeap) [Name] + "\" (#Strings[0x" + Name.ToString("X") + "])"
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}




}

