// Auto-generated file - DO NOT EDIT!
// Please edit md-schema.xml or tabs.xsl if you want to make changes.

using System;

namespace Mono.PEToolkit.Metadata {


	public class ModuleTable : MDTableBase {

		public ModuleTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new ModuleRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += ModuleRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "Module";
			}
		}

		public override TableId Id {
			get {
				return TableId.Module;
			}
		}
	}

	public class TypeRefTable : MDTableBase {

		public TypeRefTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new TypeRefRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += TypeRefRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "TypeRef";
			}
		}

		public override TableId Id {
			get {
				return TableId.TypeRef;
			}
		}
	}

	public class TypeDefTable : MDTableBase {

		public TypeDefTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new TypeDefRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += TypeDefRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "TypeDef";
			}
		}

		public override TableId Id {
			get {
				return TableId.TypeDef;
			}
		}
	}

	public class FieldPtrTable : MDTableBase {

		public FieldPtrTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new FieldPtrRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += FieldPtrRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "FieldPtr";
			}
		}

		public override TableId Id {
			get {
				return TableId.FieldPtr;
			}
		}
	}

	public class FieldTable : MDTableBase {

		public FieldTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new FieldRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += FieldRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "Field";
			}
		}

		public override TableId Id {
			get {
				return TableId.Field;
			}
		}
	}

	public class MethodPtrTable : MDTableBase {

		public MethodPtrTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new MethodPtrRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += MethodPtrRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "MethodPtr";
			}
		}

		public override TableId Id {
			get {
				return TableId.MethodPtr;
			}
		}
	}

	public class MethodTable : MDTableBase {

		public MethodTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new MethodRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += MethodRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "Method";
			}
		}

		public override TableId Id {
			get {
				return TableId.Method;
			}
		}
	}

	public class ParamPtrTable : MDTableBase {

		public ParamPtrTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new ParamPtrRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += ParamPtrRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "ParamPtr";
			}
		}

		public override TableId Id {
			get {
				return TableId.ParamPtr;
			}
		}
	}

	public class ParamTable : MDTableBase {

		public ParamTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new ParamRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += ParamRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "Param";
			}
		}

		public override TableId Id {
			get {
				return TableId.Param;
			}
		}
	}

	public class InterfaceImplTable : MDTableBase {

		public InterfaceImplTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new InterfaceImplRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += InterfaceImplRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "InterfaceImpl";
			}
		}

		public override TableId Id {
			get {
				return TableId.InterfaceImpl;
			}
		}
	}

	public class MemberRefTable : MDTableBase {

		public MemberRefTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new MemberRefRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += MemberRefRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "MemberRef";
			}
		}

		public override TableId Id {
			get {
				return TableId.MemberRef;
			}
		}
	}

	public class ConstantTable : MDTableBase {

		public ConstantTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new ConstantRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += ConstantRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "Constant";
			}
		}

		public override TableId Id {
			get {
				return TableId.Constant;
			}
		}
	}

	public class CustomAttributeTable : MDTableBase {

		public CustomAttributeTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new CustomAttributeRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += CustomAttributeRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "CustomAttribute";
			}
		}

		public override TableId Id {
			get {
				return TableId.CustomAttribute;
			}
		}
	}

	public class FieldMarshalTable : MDTableBase {

		public FieldMarshalTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new FieldMarshalRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += FieldMarshalRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "FieldMarshal";
			}
		}

		public override TableId Id {
			get {
				return TableId.FieldMarshal;
			}
		}
	}

	public class DeclSecurityTable : MDTableBase {

		public DeclSecurityTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new DeclSecurityRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += DeclSecurityRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "DeclSecurity";
			}
		}

		public override TableId Id {
			get {
				return TableId.DeclSecurity;
			}
		}
	}

	public class ClassLayoutTable : MDTableBase {

		public ClassLayoutTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new ClassLayoutRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += ClassLayoutRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "ClassLayout";
			}
		}

		public override TableId Id {
			get {
				return TableId.ClassLayout;
			}
		}
	}

	public class FieldLayoutTable : MDTableBase {

		public FieldLayoutTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new FieldLayoutRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += FieldLayoutRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "FieldLayout";
			}
		}

		public override TableId Id {
			get {
				return TableId.FieldLayout;
			}
		}
	}

	public class StandAloneSigTable : MDTableBase {

		public StandAloneSigTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new StandAloneSigRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += StandAloneSigRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "StandAloneSig";
			}
		}

		public override TableId Id {
			get {
				return TableId.StandAloneSig;
			}
		}
	}

	public class EventMapTable : MDTableBase {

		public EventMapTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new EventMapRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += EventMapRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "EventMap";
			}
		}

		public override TableId Id {
			get {
				return TableId.EventMap;
			}
		}
	}

	public class EventPtrTable : MDTableBase {

		public EventPtrTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new EventPtrRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += EventPtrRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "EventPtr";
			}
		}

		public override TableId Id {
			get {
				return TableId.EventPtr;
			}
		}
	}

	public class EventTable : MDTableBase {

		public EventTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new EventRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += EventRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "Event";
			}
		}

		public override TableId Id {
			get {
				return TableId.Event;
			}
		}
	}

	public class PropertyMapTable : MDTableBase {

		public PropertyMapTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new PropertyMapRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += PropertyMapRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "PropertyMap";
			}
		}

		public override TableId Id {
			get {
				return TableId.PropertyMap;
			}
		}
	}

	public class PropertyPtrTable : MDTableBase {

		public PropertyPtrTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new PropertyPtrRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += PropertyPtrRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "PropertyPtr";
			}
		}

		public override TableId Id {
			get {
				return TableId.PropertyPtr;
			}
		}
	}

	public class PropertyTable : MDTableBase {

		public PropertyTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new PropertyRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += PropertyRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "Property";
			}
		}

		public override TableId Id {
			get {
				return TableId.Property;
			}
		}
	}

	public class MethodSemanticsTable : MDTableBase {

		public MethodSemanticsTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new MethodSemanticsRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += MethodSemanticsRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "MethodSemantics";
			}
		}

		public override TableId Id {
			get {
				return TableId.MethodSemantics;
			}
		}
	}

	public class MethodImplTable : MDTableBase {

		public MethodImplTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new MethodImplRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += MethodImplRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "MethodImpl";
			}
		}

		public override TableId Id {
			get {
				return TableId.MethodImpl;
			}
		}
	}

	public class ModuleRefTable : MDTableBase {

		public ModuleRefTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new ModuleRefRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += ModuleRefRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "ModuleRef";
			}
		}

		public override TableId Id {
			get {
				return TableId.ModuleRef;
			}
		}
	}

	public class TypeSpecTable : MDTableBase {

		public TypeSpecTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new TypeSpecRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += TypeSpecRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "TypeSpec";
			}
		}

		public override TableId Id {
			get {
				return TableId.TypeSpec;
			}
		}
	}

	public class ImplMapTable : MDTableBase {

		public ImplMapTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new ImplMapRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += ImplMapRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "ImplMap";
			}
		}

		public override TableId Id {
			get {
				return TableId.ImplMap;
			}
		}
	}

	public class FieldRVATable : MDTableBase {

		public FieldRVATable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new FieldRVARow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += FieldRVARow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "FieldRVA";
			}
		}

		public override TableId Id {
			get {
				return TableId.FieldRVA;
			}
		}
	}

	public class ENCLogTable : MDTableBase {

		public ENCLogTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new ENCLogRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += ENCLogRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "ENCLog";
			}
		}

		public override TableId Id {
			get {
				return TableId.ENCLog;
			}
		}
	}

	public class ENCMapTable : MDTableBase {

		public ENCMapTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new ENCMapRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += ENCMapRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "ENCMap";
			}
		}

		public override TableId Id {
			get {
				return TableId.ENCMap;
			}
		}
	}

	public class AssemblyTable : MDTableBase {

		public AssemblyTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new AssemblyRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += AssemblyRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "Assembly";
			}
		}

		public override TableId Id {
			get {
				return TableId.Assembly;
			}
		}
	}

	public class AssemblyProcessorTable : MDTableBase {

		public AssemblyProcessorTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new AssemblyProcessorRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += AssemblyProcessorRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "AssemblyProcessor";
			}
		}

		public override TableId Id {
			get {
				return TableId.AssemblyProcessor;
			}
		}
	}

	public class AssemblyOSTable : MDTableBase {

		public AssemblyOSTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new AssemblyOSRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += AssemblyOSRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "AssemblyOS";
			}
		}

		public override TableId Id {
			get {
				return TableId.AssemblyOS;
			}
		}
	}

	public class AssemblyRefTable : MDTableBase {

		public AssemblyRefTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new AssemblyRefRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += AssemblyRefRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "AssemblyRef";
			}
		}

		public override TableId Id {
			get {
				return TableId.AssemblyRef;
			}
		}
	}

	public class AssemblyRefProcessorTable : MDTableBase {

		public AssemblyRefProcessorTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new AssemblyRefProcessorRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += AssemblyRefProcessorRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "AssemblyRefProcessor";
			}
		}

		public override TableId Id {
			get {
				return TableId.AssemblyRefProcessor;
			}
		}
	}

	public class AssemblyRefOSTable : MDTableBase {

		public AssemblyRefOSTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new AssemblyRefOSRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += AssemblyRefOSRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "AssemblyRefOS";
			}
		}

		public override TableId Id {
			get {
				return TableId.AssemblyRefOS;
			}
		}
	}

	public class FileTable : MDTableBase {

		public FileTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new FileRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += FileRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "File";
			}
		}

		public override TableId Id {
			get {
				return TableId.File;
			}
		}
	}

	public class ExportedTypeTable : MDTableBase {

		public ExportedTypeTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new ExportedTypeRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += ExportedTypeRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "ExportedType";
			}
		}

		public override TableId Id {
			get {
				return TableId.ExportedType;
			}
		}
	}

	public class ManifestResourceTable : MDTableBase {

		public ManifestResourceTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new ManifestResourceRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += ManifestResourceRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "ManifestResource";
			}
		}

		public override TableId Id {
			get {
				return TableId.ManifestResource;
			}
		}
	}

	public class NestedClassTable : MDTableBase {

		public NestedClassTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new NestedClassRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += NestedClassRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "NestedClass";
			}
		}

		public override TableId Id {
			get {
				return TableId.NestedClass;
			}
		}
	}

	public class TypeTyParTable : MDTableBase {

		public TypeTyParTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new TypeTyParRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += TypeTyParRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "TypeTyPar";
			}
		}

		public override TableId Id {
			get {
				return TableId.TypeTyPar;
			}
		}
	}

	public class MethodTyParTable : MDTableBase {

		public MethodTyParTable(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new MethodTyParRow(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += MethodTyParRow.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "MethodTyPar";
			}
		}

		public override TableId Id {
			get {
				return TableId.MethodTyPar;
			}
		}
	}


}

