//
// FileSystem.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//

using System;

namespace Microsoft.VisualBasic 
{
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	sealed public class FileSystem {
		// Declarations
		// Constructors
		// Properties
		// Methods
		[MonoTODO]
		public static void ChDir (System.String Path) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void ChDrive (System.Char Drive) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void ChDrive (System.String Drive) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.String CurDir () { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.String CurDir (System.Char Drive) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.String Dir () { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.String Dir (System.String Pathname, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] Microsoft.VisualBasic.FileAttribute Attributes) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void MkDir (System.String Path) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void RmDir (System.String Path) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FileCopy (System.String Source, System.String Destination) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.DateTime FileDateTime (System.String PathName) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Int64 FileLen (System.String PathName) { throw new NotImplementedException (); }
		[MonoTODO]
		public static Microsoft.VisualBasic.FileAttribute GetAttr (System.String PathName) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void Kill (System.String PathName) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void SetAttr (System.String PathName, Microsoft.VisualBasic.FileAttribute Attributes) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FileOpen (System.Int32 FileNumber, System.String FileName, Microsoft.VisualBasic.OpenMode Mode, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] Microsoft.VisualBasic.OpenAccess Access, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] Microsoft.VisualBasic.OpenShare Share, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int32 RecordLength) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FileClose (params System.Int32[] FileNumbers) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FileGetObject (System.Int32 FileNumber, ref System.Object Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FileGet (System.Int32 FileNumber, ref System.ValueType Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FileGet (System.Int32 FileNumber, ref System.Array Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(false)] ref System.Boolean ArrayIsDynamic, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(false)] ref System.Boolean StringIsFixedLength) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FileGet (System.Int32 FileNumber, ref System.Boolean Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FileGet (System.Int32 FileNumber, ref System.Byte Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FileGet (System.Int32 FileNumber, ref System.Int16 Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FileGet (System.Int32 FileNumber, ref System.Int32 Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FileGet (System.Int32 FileNumber, ref System.Int64 Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FileGet (System.Int32 FileNumber, ref System.Char Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FileGet (System.Int32 FileNumber, ref System.Single Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FileGet (System.Int32 FileNumber, ref System.Double Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FileGet (System.Int32 FileNumber, ref System.Decimal Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FileGet (System.Int32 FileNumber, ref System.String Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(false)] ref System.Boolean StringIsFixedLength) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FileGet (System.Int32 FileNumber, ref System.DateTime Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FilePutObject (System.Int32 FileNumber, System.Object Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		[System.ObsoleteAttribute("Use FilePutObject to write Object types, or coerce FileNumber and RecordNumber to Integer for writing non-Object types", false)] 
		public static void FilePut (System.Object FileNumber, System.Object Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Object RecordNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FilePut (System.Int32 FileNumber, System.ValueType Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FilePut (System.Int32 FileNumber, System.Array Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(false)] System.Boolean ArrayIsDynamic, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(false)] System.Boolean StringIsFixedLength) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FilePut (System.Int32 FileNumber, System.Boolean Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FilePut (System.Int32 FileNumber, System.Byte Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FilePut (System.Int32 FileNumber, System.Int16 Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FilePut (System.Int32 FileNumber, System.Int32 Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FilePut (System.Int32 FileNumber, System.Int64 Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FilePut (System.Int32 FileNumber, System.Char Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FilePut (System.Int32 FileNumber, System.Single Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FilePut (System.Int32 FileNumber, System.Double Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FilePut (System.Int32 FileNumber, System.Decimal Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FilePut (System.Int32 FileNumber, System.String Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(false)] System.Boolean StringIsFixedLength) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FilePut (System.Int32 FileNumber, System.DateTime Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void Print (System.Int32 FileNumber, params System.Object[] Output) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void PrintLine (System.Int32 FileNumber, params System.Object[] Output) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void Input (System.Int32 FileNumber, ref System.Object Value) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void Input (System.Int32 FileNumber, ref System.Boolean Value) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void Input (System.Int32 FileNumber, ref System.Byte Value) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void Input (System.Int32 FileNumber, ref System.Int16 Value) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void Input (System.Int32 FileNumber, ref System.Int32 Value) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void Input (System.Int32 FileNumber, ref System.Int64 Value) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void Input (System.Int32 FileNumber, ref System.Char Value) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void Input (System.Int32 FileNumber, ref System.Single Value) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void Input (System.Int32 FileNumber, ref System.Double Value) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void Input (System.Int32 FileNumber, ref System.Decimal Value) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void Input (System.Int32 FileNumber, ref System.String Value) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void Input (System.Int32 FileNumber, ref System.DateTime Value) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void Write (System.Int32 FileNumber, params System.Object[] Output) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void WriteLine (System.Int32 FileNumber, params System.Object[] Output) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.String InputString (System.Int32 FileNumber, System.Int32 CharCount) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.String LineInput (System.Int32 FileNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void Lock (System.Int32 FileNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void Lock (System.Int32 FileNumber, System.Int64 Record) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void Lock (System.Int32 FileNumber, System.Int64 FromRecord, System.Int64 ToRecord) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void Unlock (System.Int32 FileNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void Unlock (System.Int32 FileNumber, System.Int64 Record) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void Unlock (System.Int32 FileNumber, System.Int64 FromRecord, System.Int64 ToRecord) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void FileWidth (System.Int32 FileNumber, System.Int32 RecordWidth) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Int32 FreeFile () { throw new NotImplementedException (); }
		[MonoTODO]
		public static void Seek (System.Int32 FileNumber, System.Int64 Position) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Int64 Seek (System.Int32 FileNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Boolean EOF (System.Int32 FileNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Int64 Loc (System.Int32 FileNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Int64 LOF (System.Int32 FileNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static Microsoft.VisualBasic.TabInfo TAB () { throw new NotImplementedException (); }
		[MonoTODO]
		public static Microsoft.VisualBasic.TabInfo TAB (System.Int16 Column) { throw new NotImplementedException (); }
		[MonoTODO]
		public static Microsoft.VisualBasic.SpcInfo SPC (System.Int16 Count) { throw new NotImplementedException (); }
		[MonoTODO]
		public static Microsoft.VisualBasic.OpenMode FileAttr (System.Int32 FileNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void Reset () { throw new NotImplementedException (); }
		[MonoTODO]
		public static void Rename (System.String OldPath, System.String NewPath) { throw new NotImplementedException (); }
		// Events
	};
}
