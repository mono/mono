/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Mono.PEToolkit {

	/// <summary>
	/// IMAGE_FILE_HEADER
	/// </summary>
	[StructLayoutAttribute(LayoutKind.Sequential)]
	public struct COFFHeader {

		internal MachineId machine;
		internal short sections;
		internal uint tdStampRaw;
		internal uint symTabPtr;
		internal uint numSymbols;
		internal short optHeaderSize;
		internal Characteristics characteristics;



		/// <summary>
		///  Machine identifier.
		/// </summary>
		public MachineId Machine {
			get {
				return machine;
			}
			set {
				machine = value;
			}
		}


		/// <summary>
		/// </summary>
		public short NumberOfSections {
			get {
				return sections;
			}
			set {
				sections = value;
			}
		}


		/// <summary>
		/// </summary>
		public uint TimeDateStamp {
			get {
				return tdStampRaw;
			}
			set {
				tdStampRaw = value;
			}
		}

		/// <summary>
		/// </summary>
		public DateTime TimeStamp {
			get {
				return (new DateTime(1970, 1, 1) +
				       TimeSpan.FromSeconds(tdStampRaw)).ToLocalTime();
			}
		}


		/// <summary>
		/// </summary>
		public uint PointerToSymbolTable {
			get {
				return symTabPtr;
			}
			set {
				symTabPtr = value;
			}
		}


		/// <summary>
		/// </summary>
		public uint NumberOfSymbols {
			get {
				return numSymbols;
			}
			set {
				numSymbols = value;
			}
		}


		/// <summary>
		/// </summary>
		public short SizeOfOptionalHeader {
			get {
				return optHeaderSize;
			}
			set {
				optHeaderSize = value;
			}
		}


		/// <summary>
		/// </summary>
		public Characteristics Characteristics {
			get {
				return characteristics;
			}
			set {
				characteristics = value;
			}
		}


		/// <summary>
		/// </summary>
		unsafe public void Read(BinaryReader reader) {
			fixed (void* pThis = &this) {
				PEUtils.ReadStruct(reader, pThis, sizeof (COFFHeader), typeof (COFFHeader));
			}
		}


		/// <summary>
		/// </summary>
		/// <param name="writer"></param>
		public void Dump(TextWriter writer)
		{
			writer.WriteLine(
				"Machine ID      : {0}" + Environment.NewLine +
				"Sections        : {1}" + Environment.NewLine +
				"timestamp       : {2}" + Environment.NewLine +
				"Characteristics : {3}" + Environment.NewLine,
				machine, sections,
				TimeStamp + " (" + tdStampRaw.ToString("X") + ")",
				characteristics + " (0x" + characteristics.ToString("X") + ")"
			);
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}



	}
}
