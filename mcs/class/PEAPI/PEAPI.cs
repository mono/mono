using System;
using System.IO;
using System.Collections;
using System.Text;

namespace PEAPI {

	/**************************************************************************/  
	/// <summary>
	/// Image for a PEFile
	/// File Structure
	///     DOS Header (128 bytes) 
	///     PE Signature ("PE\0\0") 
	///     PEFileHeader (20 bytes)
	///     PEOptionalHeader (224 bytes) 
	///     SectionHeaders (40 bytes * NumSections)
	///
	///     Sections .text (always present - contains metadata)
	///              .sdata (contains any initialised data in the file - may not be present)
	///                     (for ilams /debug this contains the Debug table)
	///              .reloc (always present - in pure CIL only has one fixup)
	///               others???  c# produces .rsrc section containing a Resource Table
	///
	/// .text layout
	///     IAT (single entry 8 bytes for pure CIL)
	///     CLIHeader (72 bytes)
	///     CIL instructions for all methods (variable size)
	///     MetaData 
	///       Root (20 bytes + UTF-8 Version String + quad align padding)
	///       StreamHeaders (8 bytes + null terminated name string + quad align padding)
	///       Streams 
	///         #~        (always present - holds metadata tables)
	///         #Strings  (always present - holds identifier strings)
	///         #US       (Userstring heap)
	///         #Blob     (signature blobs)
	///         #GUID     (guids for assemblies or Modules)
	///    ImportTable (40 bytes)
	///    ImportLookupTable(8 bytes) (same as IAT for standard CIL files)
	///    Hint/Name Tables with entry "_CorExeMain" for .exe file and "_CorDllMain" for .dll (14 bytes)
	///    ASCII string "mscoree.dll" referenced in ImportTable (+ padding = 16 bytes)
	///    Entry Point  (0xFF25 followed by 4 bytes 0x400000 + RVA of .text)
	///
	///  #~ stream structure
	///    Header (24 bytes)
	///    Rows   (4 bytes * numTables)
	///    Tables
	/// </summary>
	internal class FileImage : BinaryWriter {

		internal readonly static uint[] iByteMask = {0x000000FF, 0x0000FF00, 0x00FF0000, 0xFF000000};
		internal readonly static ulong[] lByteMask = {0x00000000000000FF, 0x000000000000FF00,
			0x0000000000FF0000, 0x00000000FF000000,
			0x000000FF00000000, 0x0000FF0000000000,
			0x00FF000000000000, 0xFF00000000000000 };
		internal readonly static uint nibble0Mask = 0x0000000F;
		internal readonly static uint nibble1Mask = 0x000000F0;

		private static readonly byte[] DOSHeader = { 0x4d,0x5a,0x90,0x00,0x03,0x00,0x00,0x00,
			0x04,0x00,0x00,0x00,0xff,0xff,0x00,0x00,
			0xb8,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
			0x40,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
			0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
			0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
			0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
			0x00,0x00,0x00,0x00,0x80,0x00,0x00,0x00,
			0x0e,0x1f,0xba,0x0e,0x00,0xb4,0x09,0xcd,
			0x21,0xb8,0x01,0x4c,0xcd,0x21,0x54,0x68,
			0x69,0x73,0x20,0x70,0x72,0x6f,0x67,0x72,
			0x61,0x6d,0x20,0x63,0x61,0x6e,0x6e,0x6f,
			0x74,0x20,0x62,0x65,0x20,0x72,0x75,0x6e,
			0x20,0x69,0x6e,0x20,0x44,0x4f,0x53,0x20,
			0x6d,0x6f,0x64,0x65,0x2e,0x0d,0x0d,0x0a,
			0x24,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
			0x50,0x45,0x00,0x00};
		private static byte[] PEHeader = { 0x4c, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0xE0, 0x00, 0x0E, 0x01, // PE Header Standard Fields
			0x0B, 0x01, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
		};

		private static readonly uint minFileAlign = 0x200;
		private static readonly uint maxFileAlign = 0x1000;
		private static readonly uint fileHeaderSize = 0x178;
		private static readonly uint sectionHeaderSize = 40;
		private static readonly uint SectionAlignment = 0x2000;
		private static readonly uint ImageBase = 0x400000;
		private static readonly uint ImportTableSize = 40;
		private static readonly uint IATSize = 8;
		private static readonly uint CLIHeaderSize = 72;
		private uint runtimeFlags = 0x01;  // COMIMAGE_FLAGS_ILONLY
		// 32BITREQUIRED 0x02, STRONGNAMESIGNED 0x08, TRACKDEBUGDATA 0x10000
		private static readonly uint StrongNameSignatureSize = 128;
		private bool reserveStrongNameSignatureSpace = false;

		private static readonly uint relocFlags = 0x42000040;
		private static readonly ushort exeCharacteristics = 0x010E;
		private static readonly ushort dllCharacteristics = 0x210E;
		// section names are all 8 bytes
		private static readonly string textName = ".text\0\0\0";
		private static readonly string sdataName = ".sdata\0\0";
		private static readonly string relocName = ".reloc\0\0";
		private static readonly string rsrcName = ".rsrc\0\0\0";
		private static readonly string exeHintNameTable = "\0\0_CorExeMain\0";
		private static readonly string dllHintNameTable = "\0\0_CorDllMain\0";
		private static readonly string runtimeEngineName = "mscoree.dll\0\0";

		private Section text, sdata;
		static readonly Section rsrc = null;
		ArrayList data;
		BinaryWriter reloc = new BinaryWriter(new MemoryStream());
		uint dateStamp = 0;
		DateTime origin = new DateTime(1970,1,1);
		uint numSections = 2; // always have .text  and .reloc sections
		internal SubSystem subSys = SubSystem.Windows_CUI;  // default is Windows Console mode
		internal long stackReserve = 0x100000; // default is 1Mb 
		internal uint fileAlign = minFileAlign;
		uint entryPointOffset, entryPointPadding, imageSize, headerSize, headerPadding, entryPointToken = 0;
		uint relocOffset, relocRVA, relocSize, relocPadding, relocTide, hintNameTableOffset;
		uint metaDataOffset, runtimeEngineOffset, initDataSize = 0, importTablePadding;
		uint resourcesSize, resourcesOffset;
		uint strongNameSigOffset;
		uint importTableOffset, importLookupTableOffset, totalImportTableSize;
		MetaData metaData;
		char[] runtimeEngine = runtimeEngineName.ToCharArray(), hintNameTable;
		bool doDLL, largeStrings, largeGUID, largeUS, largeBlob;
		ushort characteristics;

		internal FileImage(bool makeDLL, string fileName) : base(new FileStream(fileName,FileMode.Create)) 
		{
			InitFileImage(makeDLL);
			TimeSpan tmp = System.IO.File.GetCreationTime(fileName).Subtract(origin);
			dateStamp = Convert.ToUInt32(tmp.TotalSeconds);
		}

		internal FileImage(bool makeDLL, Stream str) : base(str) 
		{
			InitFileImage(makeDLL);
			TimeSpan tmp = DateTime.Now.Subtract(origin);
			dateStamp = Convert.ToUInt32(tmp.TotalSeconds);
		}

		private void InitFileImage(bool makeDLL) 
		{
			doDLL = makeDLL;
			if (doDLL) {
				hintNameTable = dllHintNameTable.ToCharArray();
				characteristics = dllCharacteristics;
			} else {
				hintNameTable = exeHintNameTable.ToCharArray();
				characteristics = exeCharacteristics;
			}
			text = new Section(textName,0x60000020);     // IMAGE_SCN_CNT  CODE, EXECUTE, READ
			//                      rsrc = new Section(rsrcName,0x40000040);     // IMAGE_SCN_CNT  INITIALIZED_DATA, READ
			metaData = new MetaData(this);
		}

		internal MetaData GetMetaData() 
		{
			return metaData;
		}

		private uint GetNextSectStart(uint rva, uint tide) 
		{
			uint c = tide / SectionAlignment;
			if ((tide % SectionAlignment) != 0)
				c++;
			return rva + (c * SectionAlignment);
		}

		private void BuildTextSection() 
		{
			// .text layout
			//    IAT (single entry 8 bytes for pure CIL)
			//    CLIHeader (72 bytes)
			//    CIL instructions for all methods (variable size)
			//    MetaData 
			//    ImportTable (40 bytes)
			//    ImportLookupTable(8 bytes) (same as IAT for standard CIL files)
			//    Hint/Name Tables with entry "_CorExeMain" for .exe file and "_CorDllMain" for .dll (14 bytes)
			//    ASCII string "mscoree.dll" referenced in ImportTable (+ padding = 16 bytes)
			//    Entry Point  (0xFF25 followed by 4 bytes 0x400000 + RVA of .text)
			metaData.BuildMetaData(IATSize + CLIHeaderSize);
			metaDataOffset = IATSize + CLIHeaderSize;
			// Console.WriteLine("Code starts at " + metaDataOffset);
			metaDataOffset += metaData.CodeSize();
			// resourcesStart =
			resourcesOffset = metaDataOffset + metaData.Size ();
			resourcesSize = metaData.GetResourcesSize ();
			if (reserveStrongNameSignatureSpace) {
				strongNameSigOffset = resourcesOffset + resourcesSize;
				// fixUps = RVA for vtable
				importTableOffset = strongNameSigOffset + StrongNameSignatureSize;
			} else {
				strongNameSigOffset = 0;
				// fixUps = RVA for vtable
				importTableOffset = resourcesOffset + resourcesSize;
			}
			importTablePadding = NumToAlign(importTableOffset,16);
			importTableOffset += importTablePadding;
			importLookupTableOffset = importTableOffset + ImportTableSize;
			hintNameTableOffset = importLookupTableOffset + IATSize;
			runtimeEngineOffset = hintNameTableOffset + (uint)hintNameTable.Length;
			entryPointOffset = runtimeEngineOffset + (uint)runtimeEngine.Length;
			totalImportTableSize = entryPointOffset - importTableOffset;
			// Console.WriteLine("total import table size = " + totalImportTableSize);
			// Console.WriteLine("entrypoint offset = " + entryPointOffset);
			entryPointPadding = NumToAlign(entryPointOffset,4) + 2;
			entryPointOffset += entryPointPadding;
			text.AddReloc(entryPointOffset+2);
			text.IncTide(entryPointOffset + 6);
			//if (text.Tide() < fileAlign) fileAlign = minFileAlign;
			text.SetSize(NumToAlign(text.Tide(),fileAlign));
			// Console.WriteLine("text size = " + text.Size() + " text tide = " + text.Tide() + " text padding = " + text.Padding());
			// Console.WriteLine("metaDataOffset = " + Hex.Int(metaDataOffset));
			// Console.WriteLine("importTableOffset = " + Hex.Int(importTableOffset));
			// Console.WriteLine("importLookupTableOffset = " + Hex.Int(importLookupTableOffset));
			// Console.WriteLine("hintNameTableOffset = " + Hex.Int(hintNameTableOffset));
			// Console.WriteLine("runtimeEngineOffset = " + Hex.Int(runtimeEngineOffset));
			// Console.WriteLine("entryPointOffset = " + Hex.Int(entryPointOffset));
			// Console.WriteLine("entryPointPadding = " + Hex.Int(entryPointPadding));

		}

		internal void BuildRelocSection() 
		{
			text.DoRelocs(reloc);
			if (sdata != null) sdata.DoRelocs(reloc);
			if (rsrc != null) rsrc.DoRelocs(reloc);
			relocTide = (uint)reloc.Seek(0,SeekOrigin.Current);
			relocPadding = NumToAlign(relocTide,fileAlign);
			relocSize = relocTide + relocPadding;
			imageSize = relocRVA + SectionAlignment;
			initDataSize += relocSize;
		}

		private void CalcOffsets() 
		{
			if (sdata != null)
				numSections++;
			if (rsrc != null)
				numSections++;
			headerSize = fileHeaderSize + (numSections * sectionHeaderSize);
			headerPadding = NumToAlign(headerSize,fileAlign);
			headerSize += headerPadding;
			uint offset = headerSize;
			uint rva = SectionAlignment;
			text.SetOffset(offset);
			text.SetRVA(rva);
			offset += text.Size();
			rva  = GetNextSectStart(rva,text.Tide());
			// Console.WriteLine("headerSize = " + headerSize);
			// Console.WriteLine("headerPadding = " + headerPadding);
			// Console.WriteLine("textOffset = " + Hex.Int(text.Offset()));
			if (sdata != null) { 
				sdata.SetSize(NumToAlign(sdata.Tide(),fileAlign));
				sdata.SetOffset(offset);
				sdata.SetRVA(rva);
				offset += sdata.Size();
				rva = GetNextSectStart(rva,sdata.Tide());
				initDataSize += sdata.Size();
			}
			if (rsrc != null) { 
				rsrc.SetSize(NumToAlign(rsrc.Tide(),fileAlign));
				rsrc.SetOffset(offset);
				rsrc.SetRVA(rva);
				offset += rsrc.Size();
				rva = GetNextSectStart(rva,rsrc.Tide());
				initDataSize += rsrc.Size();
			}
			relocOffset = offset;
			relocRVA = rva;
		}

		internal void MakeFile() 
		{
			if (doDLL) hintNameTable = dllHintNameTable.ToCharArray();
			else hintNameTable = exeHintNameTable.ToCharArray();
			BuildTextSection();
			CalcOffsets();
			BuildRelocSection();
			// now write it out
			WriteHeader();
			WriteSections();
			Flush();
			Close();
		}

		private void WriteHeader() 
		{
			Write(DOSHeader);
			// Console.WriteLine("Writing PEHeader at offset " + Seek(0,SeekOrigin.Current));
			WritePEHeader();
			// Console.WriteLine("Writing text section header at offset " + Hex.Long(Seek(0,SeekOrigin.Current)));
			text.WriteHeader(this,relocRVA);
			if (sdata != null) sdata.WriteHeader(this,relocRVA);
			if (rsrc != null) rsrc.WriteHeader(this,relocRVA);
			// Console.WriteLine("Writing reloc section header at offset " + Seek(0,SeekOrigin.Current));
			WriteRelocSectionHeader();
			// Console.WriteLine("Writing padding at offset " + Seek(0,SeekOrigin.Current));
			WriteZeros(headerPadding);
		}

		private void WriteSections() 
		{
			// Console.WriteLine("Writing text section at offset " + Seek(0,SeekOrigin.Current));
			WriteTextSection();
			if (sdata != null) WriteSDataSection();
			if (rsrc != null) WriteRsrcSection();
			WriteRelocSection();
		}

		private void WriteIAT() 
		{
			Write(text.RVA() + hintNameTableOffset);
			Write(0);
		}

		private void WriteImportTables() 
		{
			// Import Table
			WriteZeros(importTablePadding);
			// Console.WriteLine("Writing import tables at offset " + Hex.Long(Seek(0,SeekOrigin.Current)));
			Write(importLookupTableOffset + text.RVA());
			WriteZeros(8); 
			Write(runtimeEngineOffset + text.RVA());
			Write(text.RVA());    // IAT is at the beginning of the text section
			WriteZeros(20);
			// Import Lookup Table
			WriteIAT();                // lookup table and IAT are the same
			// Hint/Name Table
			// Console.WriteLine("Writing hintname table at " + Hex.Long(Seek(0,SeekOrigin.Current)));
			Write(hintNameTable);
			Write(runtimeEngineName.ToCharArray());
		}

		private void WriteTextSection() 
		{
			WriteIAT();
			WriteCLIHeader();
			// Console.WriteLine("Writing code at " + Hex.Long(Seek(0,SeekOrigin.Current)));
			metaData.WriteByteCodes(this);
			// Console.WriteLine("Finished writing code at " + Hex.Long(Seek(0,SeekOrigin.Current)));
			largeStrings = metaData.LargeStringsIndex();
			largeGUID = metaData.LargeGUIDIndex();
			largeUS = metaData.LargeUSIndex();
			largeBlob = metaData.LargeBlobIndex();
			metaData.WriteMetaData(this);
			metaData.WriteResources (this);
			if (reserveStrongNameSignatureSpace) {
				WriteZeros(StrongNameSignatureSize);
			}
			WriteImportTables();
			WriteZeros(entryPointPadding);
			Write((ushort)0x25FF);
			Write(ImageBase + text.RVA());
			WriteZeros(text.Padding());
		}

		private void WriteCLIHeader() 
		{
			Write(CLIHeaderSize);       // Cb
			Write((short)2);            // Major runtime version
			Write((short)0);            // Minor runtime version
			Write(text.RVA() + metaDataOffset);
			Write(metaData.Size());
			Write(runtimeFlags);
			Write(entryPointToken);
			if (resourcesSize > 0) {
				Write (text.RVA () + resourcesOffset);
				Write (resourcesSize);
			} else {
				WriteZeros (8);
			}
			// Strong Name Signature (RVA, size)
			if (reserveStrongNameSignatureSpace) {
				Write(text.RVA() + strongNameSigOffset); 
				Write(StrongNameSignatureSize);
			} else {
				WriteZeros(8);
			}
			WriteZeros(8);                     // CodeManagerTable
			WriteZeros(8);                     // VTableFixups NYI
			WriteZeros(16);                    // ExportAddressTableJumps, ManagedNativeHeader
		}

		private void WriteSDataSection() 
		{
			long size = sdata.Size ();
			long start = BaseStream.Position;
			for (int i=0; i < data.Count; i++) {
				((DataConstant)data[i]).Write(this);
			}
			while (BaseStream.Position < (start + size))
				Write ((byte) 0);
		}

		private void WriteRsrcSection() 
		{
		}

		private void WriteRelocSection() 
		{
			// Console.WriteLine("Writing reloc section at " + Seek(0,SeekOrigin.Current) + " = " + relocOffset);
			MemoryStream str = (MemoryStream)reloc.BaseStream;
			Write(str.ToArray());
			WriteZeros(NumToAlign((uint)str.Position,fileAlign));
		}

		internal void SetEntryPoint(uint entryPoint) 
		{
			entryPointToken = entryPoint;
		}

		internal void AddInitData(DataConstant cVal) 
		{
			if (sdata == null) {                    
				sdata = new Section(sdataName,0xC0000040);   // IMAGE_SCN_CNT  INITIALIZED_DATA, READ, WRITE
				data = new ArrayList(); 
			}
			data.Add(cVal);
			cVal.DataOffset = sdata.Tide();
			sdata.IncTide(cVal.GetSize());
		}

		internal void WriteZeros(uint numZeros) 
		{
			for (int i=0; i < numZeros; i++) {
				Write((byte)0);
			}
		}

		internal void WritePEHeader() 
		{
			Write((ushort)0x014C);  // Machine - always 0x14C for Managed PE Files (allow others??)
			Write((ushort)numSections);
			Write(dateStamp);
			WriteZeros(8); // Pointer to Symbol Table and Number of Symbols (always zero for ECMA CLI files)
			Write((ushort)0x00E0);  // Size of Optional Header
			Write(characteristics);
			// PE Optional Header
			Write((ushort)0x010B);   // Magic
			Write((byte)0x6);        // LMajor pure-IL = 6   C++ = 7
			Write((byte)0x0);        // LMinor
			Write(text.Size());
			Write(initDataSize);
			Write(0);                // Check other sections here!!
			Write(text.RVA() + entryPointOffset);
			Write(text.RVA());
			uint dataBase = 0;
			if (sdata != null) dataBase = sdata.RVA();
			else if (rsrc != null) dataBase = rsrc.RVA();
			else dataBase = relocRVA;
			Write(dataBase);
			Write(ImageBase);
			Write(SectionAlignment);
			Write(fileAlign);
			Write((ushort)0x04);     // OS Major
			WriteZeros(6);                  // OS Minor, User Major, User Minor
			Write((ushort)0x04);     // SubSys Major
			WriteZeros(6);           // SybSys Minor, Reserved
			Write(imageSize);
			Write(headerSize);
			Write((int)0);           // File Checksum
			Write((ushort)subSys);
			Write((short)0);         // DLL Flags
			Write((uint)stackReserve);   // Stack Reserve Size
			Write((uint)0x1000);     // Stack Commit Size
			Write((uint)0x100000);   // Heap Reserve Size
			Write((uint)0x1000);     // Heap Commit Size
			Write(0);                // Loader Flags
			Write(0x10);             // Number of Data Directories
			WriteZeros(8);                  // Export Table
			Write(importTableOffset + text.RVA());
			Write(totalImportTableSize);
			WriteZeros(24);            // Resource, Exception and Certificate Tables
			Write(relocRVA);
			Write(relocTide);
			WriteZeros(48);            // Debug, Copyright, Global Ptr, TLS, Load Config and Bound Import Tables
			Write(text.RVA());         // IATRVA - IAT is at start of .text Section
			Write(IATSize);
			WriteZeros(8);             // Delay Import Descriptor
			Write(text.RVA()+IATSize); // CLIHeader immediately follows IAT
			Write(CLIHeaderSize);    
			WriteZeros(8);             // Reserved
		}

		internal void WriteRelocSectionHeader() 
		{
			Write(relocName.ToCharArray());
			Write(relocTide);
			Write(relocRVA);
			Write(relocSize);
			Write(relocOffset);
			WriteZeros(12);
			Write(relocFlags);
		}

		private void Align (MemoryStream str, int val) 
		{
			if ((str.Position % val) != 0) {
				for (int i=val - (int)(str.Position % val); i > 0; i--) {
					str.WriteByte(0);
				}
			}
		}

		private uint Align(uint val, uint alignVal) 
		{
			if ((val % alignVal) != 0) {
				val += alignVal - (val % alignVal);
			}
			return val;
		}

		private uint NumToAlign(uint val, uint alignVal) 
		{
			if ((val % alignVal) == 0) return 0;
			return alignVal - (val % alignVal);
		}

		internal void StringsIndex(uint ix) 
		{
			if (largeStrings) Write(ix);
			else Write((ushort)ix);
		}

		internal void GUIDIndex(uint ix) 
		{
			if (largeGUID) Write(ix);
			else Write((ushort)ix);
		}

		internal void USIndex(uint ix) 
		{
			if (largeUS) Write(ix);
			else Write((ushort)ix);
		}

		internal void BlobIndex(uint ix) 
		{
			if (largeBlob) Write(ix);
			else Write((ushort)ix);
		}

		internal void WriteIndex(MDTable tabIx,uint ix) 
		{
			if (metaData.LargeIx(tabIx)) Write(ix);
			else Write((ushort)ix);
		}

		internal void WriteCodedIndex(CIx code, MetaDataElement elem) 
		{
			metaData.WriteCodedIndex(code,elem,this);
		}

		internal void WriteCodeRVA(uint offs) 
		{
			Write(text.RVA() + offs);
		}

		internal void WriteDataRVA(uint offs) 
		{
			Write(sdata.RVA() + offs);
		}

		internal void Write3Bytes(uint val) 
		{
			byte b3 = (byte)((val & FileImage.iByteMask[2]) >> 16);
			byte b2 = (byte)((val & FileImage.iByteMask[1]) >> 8);;
			byte b1 = (byte)(val & FileImage.iByteMask[0]);
			Write(b1);
			Write(b2);
			Write(b3);
		}

		internal bool ReserveStrongNameSignatureSpace {
			get { return reserveStrongNameSignatureSpace; }
			set { reserveStrongNameSignatureSpace = value; }
		}

	}

	/**************************************************************************/  
	/// <summary>
	/// Base class for the PEFile (starting point)
	/// </summary>
	public class PEFile {

		private static readonly string mscorlibName = "mscorlib";
		private Module thisMod;
		private ClassDef moduleClass;
		private ArrayList resources = new ArrayList ();
		private Assembly thisAssembly;
		private static bool isMSCorlib;
		private int corFlags = 1;
		FileImage fileImage;
		MetaData metaData;

		/// <summary>
		/// Create a new PEFile.  Each PEFile is a module.
		/// </summary>
		/// <param name="name">module name, also used for the file name</param>
		/// <param name="isDLL">create a .dll or .exe file</param>
		/// <param name="hasAssembly">this file is an assembly and 
		/// will contain the assembly manifest.  The assembly name is the 
		/// same as the module name</param>
		public PEFile(string name, bool isDLL, bool hasAssembly)
			: this (name, null, isDLL, hasAssembly, null, null) 
		{
			// Console.WriteLine(Hex.Byte(0x12));
			// Console.WriteLine(Hex.Short(0x1234));
			// Console.WriteLine(Hex.Int(0x12345678));
		}

		/// <summary>
		/// Create a new PEFile.  Each PEFile is a module.
		/// </summary>
		/// <param name="name">module name, also used for the file name</param>
		/// <param name="isDLL">create a .dll or .exe file</param>
		/// <param name="hasAssembly">this file is an assembly and 
		/// will contain the assembly manifest.  The assembly name is the 
		/// same as the module name</param>
		/// <param name="outputDir">write the PEFile to this directory.  If this
		/// string is null then the output will be to the current directory</param>
		public PEFile(string name, bool isDLL, bool hasAssembly, string outputDir)
			: this (name, null, isDLL, hasAssembly, outputDir, null) 
		{
			// Console.WriteLine(Hex.Byte(0x12));
			// Console.WriteLine(Hex.Short(0x1234));
			// Console.WriteLine(Hex.Int(0x12345678));
		}

		/// <summary>
		/// Create a new PEFile
		/// </summary>
		/// <param name="name">module name</param>
		/// <param name="isDLL">create a .dll or .exe</param>
		/// <param name="hasAssembly">this PEfile is an assembly and
		/// will contain the assemly manifest.  The assembly name is the
		/// same as the module name</param>
		/// <param name="outStream">write the PEFile to this stream instead
		/// of to a new file</param>
		public PEFile(string name, bool isDLL, bool hasAssembly, Stream outStream)
			: this (name, null, isDLL, hasAssembly, null, outStream) 
		{
		}

		public PEFile(string name, string module_name, bool isDLL, bool hasAssembly, Stream outStream)
			: this (name, module_name, isDLL, hasAssembly, null, outStream) 
		{
		}  

		public PEFile(string name, string module_name, bool isDLL, bool hasAssembly, string outputDir, Stream outStream) 
		{
			SetName (name);
			string fname = module_name == null ? MakeFileName (outputDir, name, isDLL) : module_name;
			if (outStream == null)
				fileImage = new FileImage (isDLL, fname);
			else  
				fileImage = new FileImage (isDLL, outStream);

			InitPEFile (name, fname, hasAssembly);
		}

		private void SetName (string name)
		{
			if (name == "mscorlib")
				isMSCorlib = true;
		}

		private void InitPEFile(string name, string fName, bool hasAssembly) 
		{
			metaData = fileImage.GetMetaData();
			thisMod = new Module(fName,metaData);
			if (hasAssembly) {
				thisAssembly = new Assembly(name,metaData);
				metaData.AddToTable(MDTable.Assembly,thisAssembly);      
			}
			moduleClass = AddClass(TypeAttr.Private,"","<Module>");
			moduleClass.SpecialNoSuper();
			metaData.AddToTable(MDTable.Module,thisMod);
		}

		internal static bool IsMSCorlib {
			get { return isMSCorlib; }
		}

		public ClassDef ModuleClass {
			get { return moduleClass; }
		}

		/// <summary>
		/// Set the subsystem (.subsystem) (Default is Windows Console mode)
		/// </summary>
		/// <param name="subS">subsystem value</param>
		public void SetSubSystem(SubSystem subS) 
		{
			fileImage.subSys = subS;
		}

		/// <summary>
		/// Set the flags (.corflags)
		/// </summary>
		/// <param name="flags">the flags value</param>
		public void SetCorFlags(int flags) 
		{
			corFlags = flags;
		}

		public void SetStackReserve (long stackReserve)
		{
			fileImage.stackReserve = stackReserve;
		}

		private string MakeFileName(string dirName, string name, bool isDLL) 
		{
			string result = "";
			if ((dirName != null) && (dirName.CompareTo("") != 0)) {
				result = dirName;
				if (!dirName.EndsWith("\\")) result += "\\";
			}
			result += name;

			// if (isDLL) result += ".dll";  else result += ".exe";

			return result;
		}

		/// <summary>
		/// Add an external assembly to this PEFile (.assembly extern)
		/// </summary>
		/// <param name="assemName">the external assembly name</param>
		/// <returns>a descriptor for this external assembly</returns>
		public AssemblyRef AddExternAssembly(string assemName) 
		{
			if (assemName.CompareTo(mscorlibName) == 0) return metaData.mscorlib;
			AssemblyRef anAssem = new AssemblyRef(metaData,assemName);
			metaData.AddToTable(MDTable.AssemblyRef,anAssem);
			// Console.WriteLine("Adding assembly " + assemName);
			return anAssem;
		}

		/// <summary>
		/// Add an external module to this PEFile (.module extern)
		/// </summary>
		/// <param name="name">the external module name</param>
		/// <returns>a descriptor for this external module</returns>
		public ModuleRef AddExternModule(string name) 
		{
			ModuleRef modRef = new ModuleRef(metaData,name);
			metaData.AddToTable(MDTable.ModuleRef,modRef);
			return modRef;
		}

		public ClassRef AddExternClass(string ns, string name, TypeAttr attrs, MetaDataElement declRef)
		{
			return new ExternClassRef (attrs, ns, name, declRef, metaData);
		}
		
		/// <summary>
		/// Add a "global" method to this module
		/// </summary>
		/// <param name="name">method name</param>
		/// <param name="retType">return type</param>
		/// <param name="pars">method parameters</param>
		/// <returns>a descriptor for this new "global" method</returns>
		public MethodDef AddMethod (string name, Param ret_param, Param [] pars) 
		{
			return moduleClass.AddMethod (name, ret_param, pars);
		}
		
		public MethodDef AddMethod(string name, Type retType, Param[] pars) 
		{
			return AddMethod (name, new Param (ParamAttr.Default, "", retType), pars);
		}

		/// <summary>
		/// Add a "global" method to this module
		/// </summary>
		/// <param name="mAtts">method attributes</param>
		/// <param name="iAtts">method implementation attributes</param>
		/// <param name="name">method name</param>
		/// <param name="retType">return type</param>
		/// <param name="pars">method parameters</param>
		/// <returns>a descriptor for this new "global" method</returns>
		public MethodDef AddMethod (MethAttr mAtts, ImplAttr iAtts, string name, Param ret_param, Param [] pars) 
		{
			return moduleClass.AddMethod (mAtts, iAtts, name, ret_param, pars);
		}

		public MethodDef AddMethod(MethAttr mAtts, ImplAttr iAtts, string name, Type retType, Param[] pars) 
		{
			return AddMethod (mAtts, iAtts, name, new Param (ParamAttr.Default, "", retType), pars);
		}

		public MethodRef AddMethodToTypeSpec (Type item, string name, Type retType, Type[] pars) 
		{
			return AddMethodToTypeSpec (item, name, retType, pars, 0);
		}
		
		public MethodRef AddMethodToTypeSpec (Type item, string name, Type retType, Type[] pars, int gen_param_count) 
		{
			MethodRef meth = new MethodRef (item.GetTypeSpec (metaData), name, retType, pars, false, null, gen_param_count);
			metaData.AddToTable (MDTable.MemberRef,meth);
			return meth;
		}

		public MethodRef AddVarArgMethodToTypeSpec (Type item, string name, Type retType,
				Type[] pars, Type[] optPars) {
			MethodRef meth = new MethodRef(item.GetTypeSpec (metaData), name,retType,pars,true,optPars, 0);
			metaData.AddToTable(MDTable.MemberRef,meth);
			return meth;
		}

		public FieldRef AddFieldToTypeSpec (Type item, string name, Type fType) 
		{
			FieldRef field = new FieldRef (item.GetTypeSpec (metaData), name,fType);
			metaData.AddToTable (MDTable.MemberRef,field);
			return field;
		}

		public Method AddMethodSpec (Method m, GenericMethodSig g_sig)
		{
			MethodSpec ms = new MethodSpec (m, g_sig);
			metaData.AddToTable (MDTable.MethodSpec, ms);
			return ms;
		}

		/// <summary>
		/// Add a "global" field to this module
		/// </summary>
		/// <param name="name">field name</param>
		/// <param name="fType">field type</param>
		/// <returns>a descriptor for this new "global" field</returns>
		public FieldDef AddField(string name, Type fType) 
		{
			return moduleClass.AddField(name,fType);
		}

		/// <summary>
		/// Add a "global" field to this module
		/// </summary>
		/// <param name="attrSet">attributes of this field</param>
		/// <param name="name">field name</param>
		/// <param name="fType">field type</param>
		/// <returns>a descriptor for this new "global" field</returns>
		public FieldDef AddField(FieldAttr attrSet, string name, Type fType) 
		{
			return moduleClass.AddField(attrSet,name,fType);
		}

		/// <summary>
		/// Add a class to this module
		/// </summary>
		/// <param name="attrSet">attributes of this class</param>
		/// <param name="nsName">name space name</param>
		/// <param name="name">class name</param>
		/// <returns>a descriptor for this new class</returns>
		public ClassDef AddClass(TypeAttr attrSet, string nsName, string name) 
		{
			return AddClass (attrSet, nsName, name, null);
		}

		/// <summary>
		/// Add a class which extends System.ValueType to this module
		/// </summary>
		/// <param name="attrSet">attributes of this class</param>
		/// <param name="nsName">name space name</param>
		/// <param name="name">class name</param>
		/// <returns>a descriptor for this new class</returns>
		public ClassDef AddValueClass(TypeAttr attrSet, string nsName, string name, ValueClass vClass) 
		{
			ClassDef aClass = new ClassDef(attrSet,nsName,name,metaData);
			if (!ClassDef.IsValueType (nsName, name) && !ClassDef.IsEnum (nsName, name)) {
				aClass.MakeValueClass(vClass);
			} else {
				if (ClassDef.IsEnum (nsName, name))
					aClass.SetSuper (metaData.mscorlib.ValueType ());
				else
					aClass.SetSuper (metaData.mscorlib.GetSpecialSystemClass (PrimitiveType.Object));

				metaData.mscorlib.SetSpecialSystemClass (nsName, name, aClass);
			}
			aClass.SetTypeIndex (PrimitiveType.ValueType.GetTypeIndex ());
			metaData.AddToTable(MDTable.TypeDef,aClass);
			return aClass;
		}

		/// <summary>
		/// Add a class to this module
		/// </summary>
		/// <param name="attrSet">attributes of this class</param>
		/// <param name="nsName">name space name</param>
		/// <param name="name">class name</param>
		/// <param name="superType">super type of this class (extends)</param>
		/// <returns>a descriptor for this new class</returns>
		public ClassDef AddClass(TypeAttr attrSet, string nsName, string name, Class superType) 
		{
			ClassDef aClass = new ClassDef(attrSet,nsName,name,metaData);
			if (superType != null)
				aClass.SetSuper(superType);
			if (PEFile.IsMSCorlib)
				metaData.mscorlib.SetSpecialSystemClass (nsName, name, aClass);
			metaData.AddToTable(MDTable.TypeDef,aClass);
			return aClass;
		}

		public void  AddGenericClass (GenericTypeInst gti)
		{
			metaData.AddToTable (MDTable.TypeSpec, gti);
		}

		public void AddGenericParam (GenParam param)
		{
			metaData.AddToTable (MDTable.TypeSpec, param);
		}

		public FileRef AddFile(string fName, byte[] hashBytes, bool hasMetaData, bool entryPoint) 
		{
			FileRef file = new FileRef(fName,hashBytes,hasMetaData,entryPoint,metaData);
			metaData.AddToTable(MDTable.File,file);
			return file;
		}

		public PrimitiveTypeRef AddPrimitiveType (PrimitiveType type)
		{
			return new PrimitiveTypeRef (type, metaData);
		}

		/// <summary>
		/// Add a manifest resource to this PEFile NOT YET IMPLEMENTED
		/// </summary>
		/// <param name="mr"></param>
		public void AddManifestResource(ManifestResource mr) 
		{
			metaData.AddToTable(MDTable.ManifestResource,mr);
			resources.Add (mr);
			//mr.FixName(metaData);
		}

		public void AddCustomAttribute (Method meth, byte [] data, MetaDataElement element)
		{
			metaData.AddCustomAttribute (new CustomAttribute (element, meth, data));
			element.HasCustomAttr = true;
		}

		public void AddCustomAttribute (Method meth, Constant constant, MetaDataElement element)
		{
			metaData.AddCustomAttribute (new CustomAttribute (element, meth, constant));
			element.HasCustomAttr = true;
		}

		public void AddDeclSecurity (SecurityAction sec_action, byte [] data, MetaDataElement element)
		{
			metaData.AddDeclSecurity (new DeclSecurity (element, (ushort) sec_action, data));
		}

		public void AddDeclSecurity (SecurityAction sec_action, PEAPI.PermissionSet ps, MetaDataElement element)
		{
			metaData.AddDeclSecurity (new DeclSecurity_20 (element, (ushort) sec_action, ps));
		}

		/// <summary>
		/// Add a managed resource from another assembly.
		/// </summary>
		/// <param name="resName">The name of the resource</param>
		/// <param name="assem">The assembly where the resource is</param>
		/// <param name="isPublic">Access for the resource</param>
		public void AddExternalManagedResource (string resName, AssemblyRef assem, uint flags) 
		{
			resources.Add (new ManifestResource (resName, flags, assem));
		}

		/// <summary>
		/// Add a managed resource from another assembly.
		/// </summary>
		/// <param name="mr"></param>
		/// <param name="isPublic"></param>
		public void AddExternalManagedResource (ManifestResource mr) 
		{
			resources.Add (new ManifestResource (mr));
		}
		/// <summary>
		/// Find a resource
		/// </summary>
		/// <param name="name">The name of the resource</param>
		/// <returns>The resource with the name "name" or null </returns>
		public ManifestResource GetResource (string name) 
		{
			for (int i = 0; i < resources.Count; i ++) {
				if (((ManifestResource) resources [i]).Name == name)
					return (ManifestResource) resources [i];
			}
			return null;
		}

		public ManifestResource [] GetResources() 
		{
			return (ManifestResource []) resources.ToArray (typeof (ManifestResource));
		}

		/// <summary>
		/// Write out the PEFile (the "bake" function)
		/// </summary>
		public void WritePEFile() { /* the "bake" function */
			if (thisAssembly != null)
				fileImage.ReserveStrongNameSignatureSpace = thisAssembly.HasPublicKey;
			fileImage.MakeFile();
		}

		/// <summary>
		/// Get the descriptor of this module
		/// </summary>
		/// <returns>the descriptor for this module</returns>
		public Module GetThisModule() 
		{
			return thisMod;
		}

		/// <summary>
		/// Get the descriptor for this assembly.  The PEFile must have been
		/// created with hasAssembly = true
		/// </summary>
		/// <returns>the descriptor for this assembly</returns>
		public Assembly GetThisAssembly() 
		{
			return thisAssembly;
		}

	}

	/**************************************************************************/  
	/// <summary>
	/// Descriptor for a Section in a PEFile  eg .text, .sdata
	/// </summary>
	internal class Section {
		private static readonly uint relocPageSize = 4096;  // 4K pages for fixups

		char[] name; 
		uint offset = 0, tide = 0, size = 0, rva = 0, relocTide = 0;
		//uint relocOff = 0;
		uint flags = 0, padding = 0;
		uint[] relocs; 

		internal Section(string sName, uint sFlags) 
		{
			name = sName.ToCharArray();
			flags = sFlags;
		}

		internal uint Tide() { return tide; }

		internal void IncTide(uint incVal) { tide += incVal; }

		internal uint Padding() { return padding; }

		internal uint Size() { return size; }

		internal void SetSize(uint pad) 
		{
			padding = pad;
			size = tide + padding;
		}

		internal uint RVA() { return rva; }

		internal void SetRVA(uint rva) { this.rva = rva; }

		internal uint Offset() { return offset; }

		internal void SetOffset(uint offs) { offset = offs; }

		internal void DoBlock(BinaryWriter reloc, uint page, int start, int end) 
		{
			//Console.WriteLine("rva = " + rva + "  page = " + page);
			reloc.Write(rva + page);
			reloc.Write((uint)(((end-start+1)*2) + 8));
			for (int j=start; j < end; j++) {
				//Console.WriteLine("reloc offset = " + relocs[j]);
				reloc.Write((ushort)((0x3 << 12) | (relocs[j] - page)));
			}
			reloc.Write((ushort)0);
		}

		internal void DoRelocs(BinaryWriter reloc) 
		{
			if (relocTide > 0) {
				//relocOff = (uint)reloc.Seek(0,SeekOrigin.Current);
				uint block = (relocs[0]/relocPageSize + 1) * relocPageSize;
				int start = 0;
				for (int i=1; i < relocTide; i++) {
					if (relocs[i] >= block) {
						DoBlock(reloc,block-relocPageSize,start,i);
						start = i;
						block = (relocs[i]/relocPageSize + 1) * relocPageSize;
					}
				}
				DoBlock(reloc,block-relocPageSize,start,(int)relocTide);
			}
		}

		internal void AddReloc(uint offs) 
		{
			int pos = 0;
			if (relocs == null) {
				relocs = new uint[5];
			} else {
				if (relocTide >= relocs.Length) {
					uint[] tmp = relocs;
					relocs = new uint[tmp.Length + 5];
					for (int i=0; i < relocTide; i++) {
						relocs[i] = tmp[i];
					}
				}
				while ((pos < relocTide) && (relocs[pos] < offs)) pos++;
				for (int i=pos; i < relocTide; i++) {
					relocs[i+1] = relocs[i];
				}
			}
			relocs[pos] = offs;
			relocTide++;    
		}

		internal void WriteHeader(BinaryWriter output, uint relocRVA) 
		{
			output.Write(name);
			output.Write(tide);
			output.Write(rva);
			output.Write(size);
			output.Write(offset);
			output.Write(0);
			//output.Write(relocRVA + relocOff);
			output.Write(0);
			output.Write(0);
			//output.Write((ushort)relocTide);
			//output.Write((ushort)0);
			output.Write(flags);
		}

	}

	public class Hex {
		readonly static char[] hexDigit = {'0','1','2','3','4','5','6','7',
			'8','9','A','B','C','D','E','F'};
		readonly static uint[] iByteMask = {0x000000FF, 0x0000FF00, 0x00FF0000, 0xFF000000};
		readonly static ulong[] lByteMask = {0x00000000000000FF, 0x000000000000FF00, 
			0x0000000000FF0000, 0x00000000FF000000,
			0x000000FF00000000, 0x0000FF0000000000,
			0x00FF000000000000, 0xFF00000000000000 };
		readonly static uint nibble0Mask = 0x0000000F;
		readonly static uint nibble1Mask = 0x000000F0;

		public static String Byte(int b) 
		{
			char[] str = new char[2];
			uint num = (uint)b;
			uint b1 = num & nibble0Mask;
			uint b2 = (num & nibble1Mask) >> 4;
			str[0] = hexDigit[b2];
			str[1] = hexDigit[b1];
			return new String(str);
		}

		public static String Short(int b) 
		{
			char[] str = new char[4];
			uint num1 = (uint)b & iByteMask[0];
			uint num2 = ((uint)b & iByteMask[1]) >> 8;
			uint b1 = num1 & nibble0Mask;
			uint b2 = (num1 & nibble1Mask) >> 4;
			uint b3 = num2 & nibble0Mask;
			uint b4 = (num2 & nibble1Mask) >> 4;
			str[0] = hexDigit[b4];
			str[1] = hexDigit[b3];
			str[2] = hexDigit[b2];
			str[3] = hexDigit[b1];
			return new String(str);
		}

		public static String Int(int val) 
		{
			char[] str = new char[8];
			uint num = (uint)val;
			int strIx = 7;
			for (int i=0; i < iByteMask.Length; i++) {
				uint b = num & iByteMask[i];
				b >>= (i*8);
				uint b1 = b & nibble0Mask;
				uint b2 = (b & nibble1Mask) >> 4;
				str[strIx--] = hexDigit[b1];
				str[strIx--] = hexDigit[b2];
			}
			return new String(str);
		}

		public static String Int(uint num) 
		{
			char[] str = new char[8];
			int strIx = 7;
			for (int i=0; i < iByteMask.Length; i++) {
				uint b = num & iByteMask[i];
				b >>= (i*8);
				uint b1 = b & nibble0Mask;
				uint b2 = (b & nibble1Mask) >> 4;
				str[strIx--] = hexDigit[b1];
				str[strIx--] = hexDigit[b2];
			}
			return new String(str);
		}

		public static String Long(long lnum) 
		{
			ulong num = (ulong)lnum;
			char[] str = new char[16];
			int strIx = 15;
			for (int i=0; i < lByteMask.Length; i++) {
				ulong b = num & lByteMask[i];
				b >>= (i*8);
				ulong b1 = b & nibble0Mask;
				ulong b2 = (b & nibble1Mask) >> 4;
				str[strIx--] = hexDigit[b1];
				str[strIx--] = hexDigit[b2];
			}
			return new String(str);
		}
	}

	/// <summary>
	/// Error for invalid PE file
	/// </summary>
	public class PEFileException : System.Exception {
		public PEFileException(string msg) : base(msg) { }
	}

	public class NotYetImplementedException : System.Exception  {
		public NotYetImplementedException(string msg) : base(msg + " Not Yet Implemented") { }
	}

	public class TypeSignatureException : System.Exception {
		public TypeSignatureException(string msg) : base(msg) { }
	}

}
