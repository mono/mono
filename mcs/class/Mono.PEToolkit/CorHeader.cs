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
	public class CorHeader {
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


		public void Read(BinaryReader reader)
		{
			// Header versioning
			cb = reader.ReadUInt32 ();
			
			runtimeMaj = reader.ReadInt16 ();
			runtimeMin = reader.ReadInt16 ();
	
			
			// Symbol table and startup information.
			meta = new DataDir (reader);
			flags = (CorFlags) reader.ReadUInt32 ();
			entryTok = reader.ReadUInt32 ();

			// Binding information.
			rsrc  = new DataDir (reader);
			strongSig = new DataDir (reader);
		
			// Regular fixup and binding information.
			codeManTab = new DataDir (reader);
			vtab = new DataDir (reader);
			jumps = new DataDir (reader);

			// Managed Native Code.
			eeInfo = new DataDir (reader);
			helper = new DataDir (reader);
			dynInfo = new DataDir (reader);
			delayInfo = new DataDir (reader);
			modImg = new DataDir (reader);
			extFixups = new DataDir (reader);
			ridMap = new DataDir (reader);
			dbgMap = new DataDir (reader);

			// obsolete?
			ipMap = new DataDir (reader);
		}

		public void Write (BinaryWriter writer)
		{
			// Header versioning
			writer.Write (cb);
			
			writer.Write (runtimeMaj);
			writer.Write (runtimeMin);
				
			// Symbol table and startup information.
			meta.Write (writer);
			writer.Write ((uint)flags);
			writer.Write (entryTok);

			// Binding information.
			rsrc.Write (writer);
			strongSig.Write (writer);
		
			// Regular fixup and binding information.
			codeManTab.Write (writer);
			vtab.Write (writer);
			jumps.Write (writer);

			// Managed Native Code.
			eeInfo.Write (writer);
			helper.Write (writer);
			dynInfo.Write (writer);
			delayInfo.Write (writer);
			modImg.Write (writer);
			extFixups.Write (writer);
			ridMap.Write (writer);
			dbgMap.Write (writer);

			// obsolete?
			ipMap.Write (writer);
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
				"IP Map                     : {18}"  + Environment.NewLine +
				"Runtime Major		    : {19}"  + Environment.NewLine +
				"Runtime Minor		    : {20}" + Environment.NewLine,
				cb,
				RuntimeVersion,
				meta, null, EntryPointToken,
				rsrc, strongSig,
				codeManTab, vtab, jumps,
				eeInfo, helper, dynInfo, delayInfo, modImg, extFixups,
				ridMap, dbgMap, ipMap, MajorRuntimeVersion, MinorRuntimeVersion
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
