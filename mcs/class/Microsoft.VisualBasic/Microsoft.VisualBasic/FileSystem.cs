//
// FileSystem.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
namespace Microsoft.VisualBasic 
{
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Auto)] 
	sealed public class FileSystem {
		// Declarations
		// Constructors
		// Properties
		// Methods
		public static void ChDir (System.String Path) { }
		public static void ChDrive (System.Char Drive) { }
		public static void ChDrive (System.String Drive) { }
		public static System.String CurDir () { return "";}
		public static System.String CurDir (System.Char Drive) { return "";}
		public static System.String Dir () { return "";}
		public static System.String Dir (System.String Pathname, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] Microsoft.VisualBasic.FileAttribute Attributes) { return "";}
		public static void MkDir (System.String Path) { }
		public static void RmDir (System.String Path) { }
		public static void FileCopy (System.String Source, System.String Destination) { }
		public static System.DateTime FileDateTime (System.String PathName) { return System.DateTime.MinValue;}
		public static System.Int64 FileLen (System.String PathName) { return 0;}
		public static Microsoft.VisualBasic.FileAttribute GetAttr (System.String PathName) { return 0;}
		public static void Kill (System.String PathName) { }
		public static void SetAttr (System.String PathName, Microsoft.VisualBasic.FileAttribute Attributes) { }
		public static void FileOpen (System.Int32 FileNumber, System.String FileName, Microsoft.VisualBasic.OpenMode Mode, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] Microsoft.VisualBasic.OpenAccess Access, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] Microsoft.VisualBasic.OpenShare Share, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int32 RecordLength) { }
		public static void FileClose (params System.Int32[] FileNumbers) { }
		public static void FileGetObject (System.Int32 FileNumber, ref System.Object Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber) { }
		public static void FileGet (System.Int32 FileNumber, ref System.ValueType Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber) { }
		public static void FileGet (System.Int32 FileNumber, ref System.Array Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(false)] ref System.Boolean ArrayIsDynamic, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(false)] ref System.Boolean StringIsFixedLength) { }
		public static void FileGet (System.Int32 FileNumber, ref System.Boolean Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber) { }
		public static void FileGet (System.Int32 FileNumber, ref System.Byte Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber) { }
		public static void FileGet (System.Int32 FileNumber, ref System.Int16 Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber) { }
		public static void FileGet (System.Int32 FileNumber, ref System.Int32 Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber) { }
		public static void FileGet (System.Int32 FileNumber, ref System.Int64 Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber) { }
		public static void FileGet (System.Int32 FileNumber, ref System.Char Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber) { }
		public static void FileGet (System.Int32 FileNumber, ref System.Single Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber) { }
		public static void FileGet (System.Int32 FileNumber, ref System.Double Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber) { }
		public static void FileGet (System.Int32 FileNumber, ref System.Decimal Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber) { }
		public static void FileGet (System.Int32 FileNumber, ref System.String Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(false)] ref System.Boolean StringIsFixedLength) { }
		public static void FileGet (System.Int32 FileNumber, ref System.DateTime Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] ref System.Int64 RecordNumber) { }
		public static void FilePutObject (System.Int32 FileNumber, System.Object Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber) { }
		[System.ObsoleteAttribute("Use FilePutObject to write Object types, or coerce FileNumber and RecordNumber to Integer for writing non-Object types", false)] 
		public static void FilePut (System.Object FileNumber, System.Object Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Object RecordNumber) { }
		public static void FilePut (System.Int32 FileNumber, System.ValueType Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber) { }
		public static void FilePut (System.Int32 FileNumber, System.Array Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(false)] System.Boolean ArrayIsDynamic, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(false)] System.Boolean StringIsFixedLength) { }
		public static void FilePut (System.Int32 FileNumber, System.Boolean Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber) { }
		public static void FilePut (System.Int32 FileNumber, System.Byte Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber) { }
		public static void FilePut (System.Int32 FileNumber, System.Int16 Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber) { }
		public static void FilePut (System.Int32 FileNumber, System.Int32 Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber) { }
		public static void FilePut (System.Int32 FileNumber, System.Int64 Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber) { }
		public static void FilePut (System.Int32 FileNumber, System.Char Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber) { }
		public static void FilePut (System.Int32 FileNumber, System.Single Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber) { }
		public static void FilePut (System.Int32 FileNumber, System.Double Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber) { }
		public static void FilePut (System.Int32 FileNumber, System.Decimal Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber) { }
		public static void FilePut (System.Int32 FileNumber, System.String Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(false)] System.Boolean StringIsFixedLength) { }
		public static void FilePut (System.Int32 FileNumber, System.DateTime Value, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int64 RecordNumber) { }
		public static void Print (System.Int32 FileNumber, params System.Object[] Output) { }
		public static void PrintLine (System.Int32 FileNumber, params System.Object[] Output) { }
		public static void Input (System.Int32 FileNumber, ref System.Object Value) { }
		public static void Input (System.Int32 FileNumber, ref System.Boolean Value) { }
		public static void Input (System.Int32 FileNumber, ref System.Byte Value) { }
		public static void Input (System.Int32 FileNumber, ref System.Int16 Value) { }
		public static void Input (System.Int32 FileNumber, ref System.Int32 Value) { }
		public static void Input (System.Int32 FileNumber, ref System.Int64 Value) { }
		public static void Input (System.Int32 FileNumber, ref System.Char Value) { }
		public static void Input (System.Int32 FileNumber, ref System.Single Value) { }
		public static void Input (System.Int32 FileNumber, ref System.Double Value) { }
		public static void Input (System.Int32 FileNumber, ref System.Decimal Value) { }
		public static void Input (System.Int32 FileNumber, ref System.String Value) { }
		public static void Input (System.Int32 FileNumber, ref System.DateTime Value) { }
		public static void Write (System.Int32 FileNumber, params System.Object[] Output) { }
		public static void WriteLine (System.Int32 FileNumber, params System.Object[] Output) { }
		public static System.String InputString (System.Int32 FileNumber, System.Int32 CharCount) { return "";}
		public static System.String LineInput (System.Int32 FileNumber) { return "";}
		public static void Lock (System.Int32 FileNumber) { }
		public static void Lock (System.Int32 FileNumber, System.Int64 Record) { }
		public static void Lock (System.Int32 FileNumber, System.Int64 FromRecord, System.Int64 ToRecord) { }
		public static void Unlock (System.Int32 FileNumber) { }
		public static void Unlock (System.Int32 FileNumber, System.Int64 Record) { }
		public static void Unlock (System.Int32 FileNumber, System.Int64 FromRecord, System.Int64 ToRecord) { }
		public static void FileWidth (System.Int32 FileNumber, System.Int32 RecordWidth) { }
		public static System.Int32 FreeFile () { return 0;}
		public static void Seek (System.Int32 FileNumber, System.Int64 Position) { }
		public static System.Int64 Seek (System.Int32 FileNumber) { return 0;}
		public static System.Boolean EOF (System.Int32 FileNumber) { return false;}
		public static System.Int64 Loc (System.Int32 FileNumber) { return 0;}
		public static System.Int64 LOF (System.Int32 FileNumber) { return 0;}
		public static Microsoft.VisualBasic.TabInfo TAB () { return new Microsoft.VisualBasic.TabInfo();}
		public static Microsoft.VisualBasic.TabInfo TAB (System.Int16 Column) { return new Microsoft.VisualBasic.TabInfo();}
		public static Microsoft.VisualBasic.SpcInfo SPC (System.Int16 Count) { return new Microsoft.VisualBasic.SpcInfo();}
		public static Microsoft.VisualBasic.OpenMode FileAttr (System.Int32 FileNumber) { return 0;}
		public static void Reset () { }
		public static void Rename (System.String OldPath, System.String NewPath) { }
		// Events
	};
}
