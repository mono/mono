/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;
using System.IO;
using System.Runtime.InteropServices;

using Mono.PEToolkit.Metadata;

namespace Mono.PEToolkit {

	[Flags]
	public enum CorFlags : uint {
		/// <summary>
		/// COMIMAGE_FLAGS_ILONLY
		/// </summary>
		ILONLY = 0x00000001,
		
		/// <summary>
		/// COMIMAGE_FLAGS_32BITREQUIRED
		/// </summary>
		REQUIRED_32BIT = 0x00000002,
		
		/// <summary>
		/// COMIMAGE_FLAGS_IL_LIBRARY
		/// </summary>
		IL_LIBRARY = 0x00000004,
		
		/// <summary>
		/// COMIMAGE_FLAGS_TRACKDEBUGDATA
		/// </summary>
		TRACKDEBUGDATA = 0x00010000,
	}


	/// <summary>
	/// CLR 2.0 header structure.
	/// IMAGE_COR20_HEADER
	/// </summary>
	[StructLayoutAttribute(LayoutKind.Sequential)]
	public  struct CorHeader {
		// Header versioning
		internal uint cb;
		internal short runtimeMaj;
		internal short runtimeMin;

		// Symbol table and startup information.
		internal DataDir meta;
		internal CorFlags flags;
		internal uint entryTok;

		// Binding information.
		internal DataDir rsrc;
		internal DataDir strongSig;

		// Regular fixup and binding information.
		internal DataDir codeManTab;
		internal DataDir vtab;
		internal DataDir jumps;

		// Managed Native Code.
		internal DataDir eeInfo;
		internal DataDir helper;
		internal DataDir dynInfo;
		internal DataDir delayInfo;
		internal DataDir modImg;
		internal DataDir extFixups;
		internal DataDir ridMap;
		internal DataDir dbgMap;

		// obsolete?
		internal DataDir ipMap;


		//
		// Accessors
		//

		public uint Size {
			get {
				return cb;
			}
			set {
				cb = value;
			}
		}

		public short MajorRuntimeVersion {
			get {
				return runtimeMaj;
			}
			set {
				runtimeMaj = value;
			}
		}

		public short MinorRuntimeVersion {
			get {
				return runtimeMin;
			}
			set {
				runtimeMin = value;
			}
		}

		public string RuntimeVersion {
			get {
				return String.Format("{0}.{1}", runtimeMaj, runtimeMin);
			}
		}

		public DataDir MetaData {
			get {
				return meta;
			}
			set {
				meta = value;
			}
		}
		
		public CorFlags Flags {
			get {
				return flags;
			}
			set {
				flags = value;
			}
		}
		
		public MDToken EntryPointToken {
			get {
				return entryTok;
			}
			set {
				entryTok = value;
			}
		}

		public DataDir Resources {
			get {
				return rsrc;
			}
			set {
				rsrc = value;
			}
		}
		
		public DataDir StrongNameSignature {
			get {
				return strongSig;
			}
			set {
				strongSig = value;
			}
		}

		public DataDir CodeManagerTable {
			get {
				return codeManTab;
			}
			set {
				codeManTab = value;
			}
		}
		
		public DataDir VTableFixups {
			get {
				return vtab;
			}
			set {
				vtab = value;
			}
		}
		
		public DataDir ExportAddressTableJumps {
			get {
				return jumps;
			}
			set {
				jumps = value;
			}
		}


		public DataDir EEInfoTable {
			get {
				return eeInfo;
			}
			set {
				eeInfo = value;
			}
		}

		public DataDir HelperTable {
			get {
				return helper;
			}
			set {
				helper = value;
			}
		}
		
		public DataDir DynamicInfo {
			get {
				return dynInfo;
			}
			set {
				dynInfo = value;
			}
		}
		
		public DataDir DelayLoadInfo {
			get {
				return delayInfo;
			}
			set {
				delayInfo = value;
			}
		}
		
		public DataDir ModuleImage {
			get {
				return modImg;
			}
			set {
				modImg = value;
			}
		}
		
		public DataDir ExternalFixups {
			get {
				return extFixups;
			}
			set {
				extFixups = value;
			}
		}
		
		public DataDir RidMap {
			get {
				return ridMap;
			}
			set {
				ridMap = value;
			}
		}
		
		public DataDir DebugMap {
			get {
				return dbgMap;
			}
			set {
				dbgMap = value;
			}
		}


		public DataDir IPMap {
			get {
				return ipMap;
			}
			set {
				ipMap = value;
			}
		}


		unsafe public void Read(BinaryReader reader)
		{
			// TODO: clear structure before reading to initialize/reset
			// unused fields. initblk would be great.

			// Read exactly the number of bytes as specified in the header.
			// This number is duplicated in PEHeader::CLIHdrDir.
			cb = reader.ReadUInt32();
			if (cb > sizeof(uint)) {
				fixed (void* ptr = &this.runtimeMaj, pThis = &this) {
					PEUtils.ReadStruct(reader, ptr, (int)cb - sizeof (uint));
					if (!System.BitConverter.IsLittleEndian) {
						// fix entries on big-endian machine
						// preserving cb
						uint oldcb = cb;
						PEUtils.ChangeStructEndianess(pThis, typeof (CorHeader));
						cb = oldcb;
					}
				}
			}
		}


		public void Dump(TextWriter writer)
		{
			writer.WriteLine(String.Format (
				"Header Size                : {0}"  + Environment.NewLine +
				"Runtime Version            : {1}"  + Environment.NewLine +
				"MetaData Root              : {2}"  + Environment.NewLine +
				"Flags                      : {3}"  + Environment.NewLine +
				"Entry Point Token          : {4}"  + Environment.NewLine +
				"Resources                  : {5}"  + Environment.NewLine +
				"Strong Name Signature      : {6}"  + Environment.NewLine +
				"Code Manager Table         : {7}"  + Environment.NewLine +
				"VTable Fixups              : {8}"  + Environment.NewLine +
				"Export Address Table Jumps : {9}"  + Environment.NewLine +
				"EE Info Table              : {10}"  + Environment.NewLine +
				"Helper Table               : {11}"  + Environment.NewLine +
				"Dynamic Info               : {12}"  + Environment.NewLine +
				"Delay Load Info            : {13}"  + Environment.NewLine +
				"Module Image               : {14}"  + Environment.NewLine +
				"External Fixups            : {15}"  + Environment.NewLine +
				"Rid Map                    : {16}"  + Environment.NewLine +
				"Debug Map                  : {17}"  + Environment.NewLine +
				"IP Map                     : {18}"  + Environment.NewLine,
				cb + String.Format(" (0x{0})", cb.ToString("X")),
				RuntimeVersion,
				meta, flags, EntryPointToken,
				rsrc, strongSig,
				codeManTab, vtab, jumps,
				eeInfo, helper, dynInfo, delayInfo, modImg, extFixups,
				ridMap, dbgMap, ipMap
			));
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}


	}
}