/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;

using Mono.PEToolkit.Metadata;

namespace Mono.PEToolkit {

	public class Image : IDisposable {

		internal DOSHeader dosHdr;
		internal COFFHeader coffHdr;
		internal PEHeader peHdr;

		internal CorHeader corHdr;

		internal Hashtable sections;
		// File position right after PEHeader (NT Optional Header).
		protected long sectionsPos;

		private MetaDataRoot mdRoot;

		private string name;
		private bool open;
		internal BinaryReader reader;

		public Image(string name)
		{
			this.name = name;
			open = false;
			reader = null;

			mdRoot = null;

			dosHdr = new DOSHeader();
			coffHdr = new COFFHeader();
			peHdr = new PEHeader();
			corHdr = new CorHeader();

			sections = new Hashtable();
			sectionsPos = -1;
		}

		~Image()
		{
			Close();
		}


		public Hashtable Sections {
			get {
				return sections;
			}
		}

		public void Open()
		{
			lock (this) if (!open) {
				FileInfo pe = new FileInfo(name);
				if (!pe.Exists) {
					throw new Exception("Invalid file path.");
				}

				reader = new BinaryReader(new BufferedStream(pe.OpenRead()));
				if (!reader.BaseStream.CanSeek) {
					throw new Exception("Can't seek.");
				}

				open = true;
			}
		}

		public void Close()
		{
			lock (this) if (open) {
				reader.Close();
				open = false;
			}
		}

		// IDisposable
		public void Dispose()
		{
			Close();
		}


		public bool IsCLI {
			get {
				return peHdr.IsCLIImage;
			}
		}

		public MetaDataRoot MetadataRoot {
			get {
				return mdRoot;
			}
		}

		/// <summary>
		/// </summary>
		public void ReadHeaders()
		{
			if (!open) {
				throw new Exception("You must open image before trying to read it.");
			}

			dosHdr.Read(reader);
			reader.BaseStream.Position = dosHdr.Lfanew;
			ExeSignature peSig = (ExeSignature) reader.ReadUInt16();
			if (peSig != ExeSignature.NT) {
				throw new Exception ("Invalid image format: cannot find PE signature.");
			}
			peSig = (ExeSignature) reader.ReadUInt16();
			if (peSig != ExeSignature.NT2) {
				throw new Exception ("Invalid image format: cannot find PE signature.");
			}

			coffHdr.Read(reader);
			peHdr.Read(reader);
		
			sectionsPos = reader.BaseStream.Position;
			ReadSections();
			
			if (this.IsCLI) {
				
				reader.BaseStream.Position = RVAToVA(peHdr.CLIHdrDir.virtAddr);
				corHdr.Read (reader);
				
				mdRoot = new MetaDataRoot(this);
				reader.BaseStream.Position = RVAToVA(corHdr.MetaData.virtAddr);
				mdRoot.Read(reader);
				
			}
			
		}

		public void WriteHeaders (BinaryWriter writer)
		{
			dosHdr.Write (writer);
			writer.BaseStream.Position = dosHdr.Lfanew;
			writer.Write ((ushort)ExeSignature.NT);
			writer.Write ((ushort)ExeSignature.NT2);
			
			coffHdr.Write (writer);
			peHdr.Write (writer);
		
			/*
			int pos = reader.BaseStream.Position;
			ReadSections();
			
			if (this.IsCLI) {
				
				reader.BaseStream.Position = RVAToVA(peHdr.CLIHdrDir.virtAddr);
				corHdr.Read (reader);
				
				mdRoot = new MetaDataRoot(this);
				reader.BaseStream.Position = RVAToVA(corHdr.MetaData.virtAddr);
				mdRoot.Read(reader);
				
			}
			*/
			
		}

		/// <summary>
		/// </summary>
		protected void ReadSections()
		{
			if (sectionsPos < 0) {
				throw new Exception("Read headers first.");
			}
			reader.BaseStream.Position = sectionsPos;

			int n = coffHdr.NumberOfSections;
			for (int i = n; --i >=0;) {
				Section sect = new Section();
				sect.Read(reader);
				sections [sect.Name] = sect;
			}
		}


		/// <summary>
		/// </summary>
		/// <param name="writer"></param>
		public void Dump(TextWriter writer)
		{
			writer.WriteLine (
				"COFF Header:" + Environment.NewLine +
				coffHdr.ToString() + Environment.NewLine +
				"PE Header:" + Environment.NewLine +
				peHdr.ToString() + Environment.NewLine +
				"Core Header:" + Environment.NewLine +
				corHdr.ToString()
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


		/// <summary>
		///  Returns name of the section for the given RVA.
		/// </summary>
		/// <param name="rva"></param>
		/// <returns></returns>
		public string RVAToSectionName(RVA rva)
		{
			string res = null;
			foreach (Section s in Sections.Values) {
				RVA sva = s.VirtualAddress;
				if (rva >= sva && rva < sva + s.SizeOfRawData) {
					res = s.Name;
					break;
				}
			}
			return res;
		}

		public long RVAToVA(RVA rva)
		{
			string sectName = RVAToSectionName(rva);
			long res = 0;
			if (sectName != null) {
				Section s = (Section) Sections [sectName];
				res = rva + (s.PointerToRawData - s.VirtualAddress);
			}
			return res;
		}

		public MetaDataRoot MetaDataRoot {
			get {
				return mdRoot;
			}
		}

		public void DumpStreamHeader(TextWriter writer, string name)
		{
			if (mdRoot == null || name == null || name == String.Empty || writer == null) return;
			writer.Write(name + " header: ");
			MDStream s = MetaDataRoot.Streams[name] as MDStream;
			if (s != null) {
				writer.WriteLine();
				writer.WriteLine(s);
			} else {
				writer.WriteLine("not present.");
				writer.WriteLine();
			}
		}

		public void DumpStreamHeader(string name)
		{
			DumpStreamHeader(Console.Out, name);
		}

	}

}

