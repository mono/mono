// Auto-generated file - DO NOT EDIT!
// Please edit md-schema.xml or tabs-base.xsl if you want to make changes.

using System;

namespace Mono.PEToolkit.Metadata {


	/// <summary>
	/// </summary>
	/// <remarks>
	/// </remarks>
	public abstract class TablesHeapBase : MDHeap {

		internal TablesHeapBase(MDStream stream) : base(stream)
		{
		}

		/// <summary>
		/// Gets or sets bitvector of valid tables (64-bit).
		/// </summary>
		public abstract long Valid {get; set;}

		/// <summary>
		/// Gets or sets bitvector of sorted tables (64-bit).
		/// </summary>
		public abstract long Sorted {get; set;}


		//
		// Accessors to decode Valid bitvector.
		//


		/// <summary>
		/// True if heap has Module table.
		/// </summary>
		public bool HasModule {
			get {
				return (Valid & (1L << 0x00)) != 0;
			}
			set {
				long mask = (1L << 0x00);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has TypeRef table.
		/// </summary>
		public bool HasTypeRef {
			get {
				return (Valid & (1L << 0x01)) != 0;
			}
			set {
				long mask = (1L << 0x01);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has TypeDef table.
		/// </summary>
		public bool HasTypeDef {
			get {
				return (Valid & (1L << 0x02)) != 0;
			}
			set {
				long mask = (1L << 0x02);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has FieldPtr table.
		/// </summary>
		public bool HasFieldPtr {
			get {
				return (Valid & (1L << 0x03)) != 0;
			}
			set {
				long mask = (1L << 0x03);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has Field table.
		/// </summary>
		public bool HasField {
			get {
				return (Valid & (1L << 0x04)) != 0;
			}
			set {
				long mask = (1L << 0x04);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has MethodPtr table.
		/// </summary>
		public bool HasMethodPtr {
			get {
				return (Valid & (1L << 0x05)) != 0;
			}
			set {
				long mask = (1L << 0x05);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has Method table.
		/// </summary>
		public bool HasMethod {
			get {
				return (Valid & (1L << 0x06)) != 0;
			}
			set {
				long mask = (1L << 0x06);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has ParamPtr table.
		/// </summary>
		public bool HasParamPtr {
			get {
				return (Valid & (1L << 0x07)) != 0;
			}
			set {
				long mask = (1L << 0x07);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has Param table.
		/// </summary>
		public bool HasParam {
			get {
				return (Valid & (1L << 0x08)) != 0;
			}
			set {
				long mask = (1L << 0x08);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has InterfaceImpl table.
		/// </summary>
		public bool HasInterfaceImpl {
			get {
				return (Valid & (1L << 0x09)) != 0;
			}
			set {
				long mask = (1L << 0x09);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has MemberRef table.
		/// </summary>
		public bool HasMemberRef {
			get {
				return (Valid & (1L << 0x0a)) != 0;
			}
			set {
				long mask = (1L << 0x0a);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has Constant table.
		/// </summary>
		public bool HasConstant {
			get {
				return (Valid & (1L << 0x0b)) != 0;
			}
			set {
				long mask = (1L << 0x0b);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has CustomAttribute table.
		/// </summary>
		public bool HasCustomAttribute {
			get {
				return (Valid & (1L << 0x0c)) != 0;
			}
			set {
				long mask = (1L << 0x0c);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has FieldMarshal table.
		/// </summary>
		public bool HasFieldMarshal {
			get {
				return (Valid & (1L << 0x0d)) != 0;
			}
			set {
				long mask = (1L << 0x0d);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has DeclSecurity table.
		/// </summary>
		public bool HasDeclSecurity {
			get {
				return (Valid & (1L << 0x0e)) != 0;
			}
			set {
				long mask = (1L << 0x0e);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has ClassLayout table.
		/// </summary>
		public bool HasClassLayout {
			get {
				return (Valid & (1L << 0x0f)) != 0;
			}
			set {
				long mask = (1L << 0x0f);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has FieldLayout table.
		/// </summary>
		public bool HasFieldLayout {
			get {
				return (Valid & (1L << 0x10)) != 0;
			}
			set {
				long mask = (1L << 0x10);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has StandAloneSig table.
		/// </summary>
		public bool HasStandAloneSig {
			get {
				return (Valid & (1L << 0x11)) != 0;
			}
			set {
				long mask = (1L << 0x11);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has EventMap table.
		/// </summary>
		public bool HasEventMap {
			get {
				return (Valid & (1L << 0x12)) != 0;
			}
			set {
				long mask = (1L << 0x12);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has EventPtr table.
		/// </summary>
		public bool HasEventPtr {
			get {
				return (Valid & (1L << 0x13)) != 0;
			}
			set {
				long mask = (1L << 0x13);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has Event table.
		/// </summary>
		public bool HasEvent {
			get {
				return (Valid & (1L << 0x14)) != 0;
			}
			set {
				long mask = (1L << 0x14);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has PropertyMap table.
		/// </summary>
		public bool HasPropertyMap {
			get {
				return (Valid & (1L << 0x15)) != 0;
			}
			set {
				long mask = (1L << 0x15);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has PropertyPtr table.
		/// </summary>
		public bool HasPropertyPtr {
			get {
				return (Valid & (1L << 0x16)) != 0;
			}
			set {
				long mask = (1L << 0x16);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has Property table.
		/// </summary>
		public bool HasProperty {
			get {
				return (Valid & (1L << 0x17)) != 0;
			}
			set {
				long mask = (1L << 0x17);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has MethodSemantics table.
		/// </summary>
		public bool HasMethodSemantics {
			get {
				return (Valid & (1L << 0x18)) != 0;
			}
			set {
				long mask = (1L << 0x18);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has MethodImpl table.
		/// </summary>
		public bool HasMethodImpl {
			get {
				return (Valid & (1L << 0x19)) != 0;
			}
			set {
				long mask = (1L << 0x19);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has ModuleRef table.
		/// </summary>
		public bool HasModuleRef {
			get {
				return (Valid & (1L << 0x1a)) != 0;
			}
			set {
				long mask = (1L << 0x1a);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has TypeSpec table.
		/// </summary>
		public bool HasTypeSpec {
			get {
				return (Valid & (1L << 0x1b)) != 0;
			}
			set {
				long mask = (1L << 0x1b);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has ImplMap table.
		/// </summary>
		public bool HasImplMap {
			get {
				return (Valid & (1L << 0x1c)) != 0;
			}
			set {
				long mask = (1L << 0x1c);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has FieldRVA table.
		/// </summary>
		public bool HasFieldRVA {
			get {
				return (Valid & (1L << 0x1d)) != 0;
			}
			set {
				long mask = (1L << 0x1d);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has ENCLog table.
		/// </summary>
		public bool HasENCLog {
			get {
				return (Valid & (1L << 0x1e)) != 0;
			}
			set {
				long mask = (1L << 0x1e);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has ENCMap table.
		/// </summary>
		public bool HasENCMap {
			get {
				return (Valid & (1L << 0x1f)) != 0;
			}
			set {
				long mask = (1L << 0x1f);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has Assembly table.
		/// </summary>
		public bool HasAssembly {
			get {
				return (Valid & (1L << 0x20)) != 0;
			}
			set {
				long mask = (1L << 0x20);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has AssemblyProcessor table.
		/// </summary>
		public bool HasAssemblyProcessor {
			get {
				return (Valid & (1L << 0x21)) != 0;
			}
			set {
				long mask = (1L << 0x21);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has AssemblyOS table.
		/// </summary>
		public bool HasAssemblyOS {
			get {
				return (Valid & (1L << 0x22)) != 0;
			}
			set {
				long mask = (1L << 0x22);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has AssemblyRef table.
		/// </summary>
		public bool HasAssemblyRef {
			get {
				return (Valid & (1L << 0x23)) != 0;
			}
			set {
				long mask = (1L << 0x23);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has AssemblyRefProcessor table.
		/// </summary>
		public bool HasAssemblyRefProcessor {
			get {
				return (Valid & (1L << 0x24)) != 0;
			}
			set {
				long mask = (1L << 0x24);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has AssemblyRefOS table.
		/// </summary>
		public bool HasAssemblyRefOS {
			get {
				return (Valid & (1L << 0x25)) != 0;
			}
			set {
				long mask = (1L << 0x25);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has File table.
		/// </summary>
		public bool HasFile {
			get {
				return (Valid & (1L << 0x26)) != 0;
			}
			set {
				long mask = (1L << 0x26);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has ExportedType table.
		/// </summary>
		public bool HasExportedType {
			get {
				return (Valid & (1L << 0x27)) != 0;
			}
			set {
				long mask = (1L << 0x27);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has ManifestResource table.
		/// </summary>
		public bool HasManifestResource {
			get {
				return (Valid & (1L << 0x28)) != 0;
			}
			set {
				long mask = (1L << 0x28);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has NestedClass table.
		/// </summary>
		public bool HasNestedClass {
			get {
				return (Valid & (1L << 0x29)) != 0;
			}
			set {
				long mask = (1L << 0x29);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has TypeTyPar table.
		/// </summary>
		public bool HasTypeTyPar {
			get {
				return (Valid & (1L << 0x2a)) != 0;
			}
			set {
				long mask = (1L << 0x2a);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}

		/// <summary>
		/// True if heap has MethodTyPar table.
		/// </summary>
		public bool HasMethodTyPar {
			get {
				return (Valid & (1L << 0x2b)) != 0;
			}
			set {
				long mask = (1L << 0x2b);
				if (value) {
					Valid |= mask;
				} else {
					Valid &= ~mask;
				}
			}
		}


	}

}

