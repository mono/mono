/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Mono.PEToolkit {

	public class COFFHeader {

		private MachineId machine;
		private short sections;
		private uint tdStampRaw;
		private uint symTabPtr;
		private uint numSymbols;
		private short optHeaderSize;
		private Characteristics characteristics;

		public MachineId Machine {
			get { return machine; }
			set { machine = value; }
		}

		public short NumberOfSections {
			get { return sections; }
			set { sections = value;	}
		}

		public uint TimeDateStamp {
			get { return tdStampRaw; }
			set { tdStampRaw = value; }
		}

		public DateTime TimeStamp {
			get {
				return (new DateTime(1970, 1, 1) +
				       TimeSpan.FromSeconds(tdStampRaw)).ToLocalTime();
			}
		}

		public uint PointerToSymbolTable {
			get { return symTabPtr;	}
			set { symTabPtr = value; }
		}

		public uint NumberOfSymbols {
			get { return numSymbols; }
			set { numSymbols = value; }
		}

		public short SizeOfOptionalHeader {
			get { return optHeaderSize; }
			set { optHeaderSize = value; }
		}

		public Characteristics Characteristics {
			get { return characteristics; }
			set { characteristics = value; }
		}

		public void Read (BinaryReader reader) 
		{
			machine = (MachineId) reader.ReadUInt16 ();
			sections = reader.ReadInt16 ();
			tdStampRaw = reader.ReadUInt32 ();
			symTabPtr = reader.ReadUInt32 ();
			numSymbols = reader.ReadUInt32 ();
			optHeaderSize = reader.ReadInt16 ();
			characteristics = (Characteristics) reader.ReadUInt16 ();	
		}

		public void Write (BinaryWriter writer) 
		{
			writer.Write ((ushort)machine);
			writer.Write (sections);
			writer.Write (tdStampRaw);
			writer.Write (symTabPtr);
			writer.Write (numSymbols);
			writer.Write (optHeaderSize);
			writer.Write ((ushort)characteristics);	
		}

		public void Dump(TextWriter writer)
		{
			writer.WriteLine(
				"Machine ID      : {0}" + Environment.NewLine + 
				"Sections        : {1}" + Environment.NewLine +
				"Characteristics : {2}" + Environment.NewLine +
				"timestamp       : {3} ({4})" + Environment.NewLine
				,machine, sections, (ushort)characteristics,
				TimeStamp, tdStampRaw.ToString("X")
			);
		}		

		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}
}

