//
// FileSystem.cs
//
// Author:
//   
// Daniel Campos ( danielcampos@netcourrier.com )
// 
//

using System;
using System.IO;
namespace Microsoft.VisualBasic 
{
        [Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
        sealed public class FileSystem {
                private static System.IO.FileStream[] FHandle=new FileStream[255];
		private static Microsoft.VisualBasic.OpenMode[] FMode= new Microsoft.VisualBasic.OpenMode[255];
		private static string InitialPath=Environment.CurrentDirectory; 
                // Declarations
                // Constructors
                // Properties
                // Methods
                [MonoTODO("Needs testing")]
                public static void ChDir (System.String Path) 
                {
                	if ( (Path=="") || (Path==null))
                		throw new System.ArgumentException("Path is empty"); 
                	try
                	{
                		Environment.CurrentDirectory=Path;
                	}
                	catch ( Exception e){ throw new System.IO.FileNotFoundException ("Invalid drive is specified, or drive is unavailable");}
                	
                }
                
                [MonoTODO]
                public static void ChDrive (System.Char Drive) { throw new NotImplementedException (); }
                [MonoTODO]
                public static void ChDrive (System.String Drive) { throw new NotImplementedException (); }
                
                [MonoTODO("Needs testing")]
                public static System.String CurDir () 
                { 
                	return Environment.CurrentDirectory;
                }
                
                [MonoTODO("Needs Testing")]
                public static System.String CurDir (System.Char Drive)
		{
			bool MyOK=false;
			string MyDrive=(Drive.ToString()).ToLower();
			string[] buf=System.IO.Directory.GetLogicalDrives ();
			for (int lookfor=0;lookfor<buf.Length;lookfor++)
				if ( buf[lookfor].Substring(0,1).ToLower() == MyDrive ) {MyOK=true; break; }
			if (!MyOK)
				throw new System.ArgumentException("Invalid drive is specified.");
			if ( Environment.CurrentDirectory.Substring(0,1).ToLower() == MyDrive )
				return Environment.CurrentDirectory  ;
			else
			{
				if ( InitialPath.Substring(0,1).ToLower() == MyDrive ) 
					return InitialPath;
				else
					return (MyDrive.ToUpper()  + Path.VolumeSeparatorChar + Path.DirectorySeparatorChar  );
			}
		}
                [MonoTODO]
                public static System.String Dir () { throw new NotImplementedException (); }
                [MonoTODO]
                public static System.String Dir (System.String Pathname, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] Microsoft.VisualBasic.FileAttribute Attributes) { throw new NotImplementedException (); }
                
                [MonoTODO("Needs testing")]
                public static void MkDir (System.String Path) 
                { 
            		// if a file called like 'path' does exist
            		// no exception is generated using .net
            		// A little extrange?
            		if (Path==null || Path=="")
            		{
            			throw new System.ArgumentException(); 
            		}
            		else
            		{
            			if (System.IO.Directory.Exists (Path))
            				throw new System.IO.IOException("Directory already exists");
            			else
            				System.IO.Directory.CreateDirectory(Path);
            		}
            	}
            	
            	[MonoTODO("Needs testing")]
                public static void RmDir (System.String Path) 
                { 
                	System.IO.Directory.Delete(Path); 
                }
                
                [MonoTODO("Needs testing")]
                public static void FileCopy (System.String Source, System.String Destination) 
                { 
                	// using VB, filecopy always overwrites Destination
                	System.IO.File.Copy(Source,Destination,true); 
                }
                
                [MonoTODO("Needs testing")]
                public static System.DateTime FileDateTime (System.String PathName) 
                {
                	// A better exception handling is needed : exceptions
                	// are not the same as 'GetLastWriteTime'
                 	return System.IO.File.GetLastWriteTime (PathName);
                }
                
                [MonoTODO("Needs Testing")]
                public static System.Int64 FileLen(System.String PathName) 
		{
			FileInfo MyFile=new FileInfo(PathName);
			if ( !MyFile.Exists )
				throw new System.ArgumentException(PathName + " does not exists");
			return (System.Int64)MyFile.Length;  
		}
                [MonoTODO]
                public static Microsoft.VisualBasic.FileAttribute GetAttr (System.String PathName) { throw new NotImplementedException (); }
                
                [MonoTODO("Needs testing")]
                public static void Kill (System.String PathName) 
                {
                	if (!System.IO.File.Exists(PathName))
                		throw new System.IO.FileNotFoundException();
                	else
                		System.IO.File.Delete(PathName);
                }
                
                [MonoTODO]
                public static void SetAttr (System.String PathName, Microsoft.VisualBasic.FileAttribute Attributes) { throw new NotImplementedException (); }
               
                [MonoTODO("Needs testing")]
                public static void FileOpen (System.Int32 FileNumber, System.String FileName, Microsoft.VisualBasic.OpenMode Mode, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] Microsoft.VisualBasic.OpenAccess Access, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] Microsoft.VisualBasic.OpenShare Share, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int32 RecordLength)
                {  
                	// at this moment you can open a file
                	// only for Append, Input or Output
                	System.IO.FileMode MyMode;
                	System.IO.FileAccess MyAccess; 
                	System.IO.FileShare MyShare;
                	//
                	// exceptions
                	//
                	if ( RecordLength < -1 )
                		throw new System.ArgumentException("Record Length is negative (nad not equal to -1)");
                	if ( RecordLength > 32767)
                		throw new System.ArgumentException ("Invalid Record Length");
                	if (FileNumber <0 || FileNumber > 255)
                   		throw new System.IO.IOException(FileNumber.ToString() + " is invalid (<-1 or >255)",5); 
                	if ( (Mode == OpenMode.Output) && ( Access != OpenAccess.Default ) && ( Access != OpenAccess.Write ) )
						throw new System.ArgumentException("To use Output Mode, you have to use OpenAccess.Write or OpenAccess.Default");
  					if ( (Mode == OpenMode.Input) && ( Access != OpenAccess.Default ) && ( Access != OpenAccess.Read ) )
						throw new System.ArgumentException("To use Input Mode, you have to use OpenAccess.Read or OpenAccess.Default");
                	//
                	// implementation
                	//
                	FileNumber--;
            		if (FHandle[FileNumber] != null)
            			throw new System.IO.IOException (FileNumber.ToString() + " is in use",5); 	
            		
        			switch (Mode)
        			{
        				case Microsoft.VisualBasic.OpenMode.Append :
        					MyMode=System.IO.FileMode.Append  ;  
        					break;
        				case Microsoft.VisualBasic.OpenMode.Binary :
        					throw new NotImplementedException ();
        					
        				case Microsoft.VisualBasic.OpenMode.Input  :
        					MyMode=System.IO.FileMode.Open  ;
        					break;
        				case Microsoft.VisualBasic.OpenMode.Output  :
        					MyMode=System.IO.FileMode.OpenOrCreate  ;
        					break;
        				case Microsoft.VisualBasic.OpenMode.Random  :
        					 throw new NotImplementedException ();	
        				default:
        					throw new System.ArgumentException("Invalid Share"); 
        			}
        			switch (Access)
        			{
        				case Microsoft.VisualBasic.OpenAccess.ReadWrite :  
        				case Microsoft.VisualBasic.OpenAccess.Default :
        					MyAccess=System.IO.FileAccess.ReadWrite;
        					break;
        				case Microsoft.VisualBasic.OpenAccess.Read :
        					MyAccess=System.IO.FileAccess.Read;
        					break;
        				case Microsoft.VisualBasic.OpenAccess.Write :
        					MyAccess=System.IO.FileAccess.Write;
        					break;
        				default:
        					throw new System.ArgumentException("Invalid Access");
        				
        			}
        			switch(Share)
        			{
        				case Microsoft.VisualBasic.OpenShare.Default :
        				case Microsoft.VisualBasic.OpenShare.Shared :
        					MyShare=System.IO.FileShare.ReadWrite ;  
        					break;
        				case Microsoft.VisualBasic.OpenShare.LockRead  :
        					MyShare=System.IO.FileShare.Write; 
        					break;
        				case Microsoft.VisualBasic.OpenShare.LockReadWrite :
        					MyShare=System.IO.FileShare.None ;
        					break;
        				case Microsoft.VisualBasic.OpenShare.LockWrite :
        					MyShare=System.IO.FileShare.Read;
        					break;
        				default:
        					throw new System.ArgumentException("Invalid Share");
        			}
        			FHandle[FileNumber]=new System.IO.FileStream (FileName,MyMode,MyAccess,MyShare);
            			FMode[FileNumber]=Mode;
            		
                		
                	
                }
                
                [MonoTODO("Needs testing")]
                public static void FileClose (params System.Int32[] FileNumbers) 
                { 
                	int bucle=0;
                	if (FileNumbers.Length  == 0)
                	{
                		for(bucle=0;bucle<255;bucle++)
                		{
                			if (FHandle[bucle]!=null)
                				try
                				{
                					FHandle[bucle].Close();	
                				}
                				catch (Exception e) {e.GetType (); FHandle[bucle]=null ;}
                		}
                	}
                	else
                	{
                		for(bucle=0;bucle<FileNumbers.Length;bucle++)
                		{
                			if ( FHandle [ FileNumbers[bucle] - 1 ] != null )
                			{
                				if (FileNumbers[bucle]>0 && FileNumbers[bucle]<256)
                				{
                					try
                					{
                						FHandle[ FileNumbers[bucle] - 1].Close();
                						FHandle[ FileNumbers[bucle] - 1]=null;
                					}
                					catch (Exception e){e.GetType (); FHandle[ FileNumbers[bucle] - 1]= null ;}
                				}
                				else
                					throw new System.IO.IOException (FileNumbers[bucle].ToString() + " Does not exist",52);
                			}
                			else
                				throw new System.IO.IOException (FileNumbers[bucle].ToString() + " Does not exist",52); 
                		}
                	}
                }
                
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
                 [MonoTODO("Needs Testing")]
                public static void Write (System.Int32 FileNumber, params System.Object[] Output) 
		{
			string MyBuf=null;
			byte[] Separator=new byte[1];
			byte[] bufout=new Byte[1];
			Separator[0]=(byte)',';
			if (FileNumber<1 || FileNumber>255)
				throw new System.IO.IOException (FileNumber.ToString() + " does not exists",52);
			if (FHandle[FileNumber - 1]==null)
				throw new System.IO.IOException(FileNumber + " does not exists",52);
			if ( FMode[ FileNumber - 1] != OpenMode.Output  )
				throw new System.IO.IOException ("FileMode is invalid");
			if (Output.Length == 0)
			{
				FHandle[FileNumber - 1].Write(Separator,0,1);
			}
			else
			{
				for (int MyArgs=0;MyArgs<Output.Length;MyArgs++)
				{
					MyBuf=WriteAuxiliar(Output[MyArgs]);
					for (int PutsData=0;PutsData<MyBuf.Length;PutsData++)
					{
						bufout[0]=(byte)MyBuf[PutsData];
						FHandle[FileNumber-1].Write(bufout,0,1);
					}
					FHandle[FileNumber - 1].Write(Separator,0,1);
				}
			}
		}
                [MonoTODO("Needs Testing")]
                public static void WriteLine (System.Int32 FileNumber, params System.Object[] Output) 
		{
			byte[] Separator=new byte[1];
			byte[] bufout=new Byte[1];
			byte[] NewLine=new Byte[2];
			string MyBuf="";
			Separator[0]=(byte)',';
			NewLine[0]=(byte)'\x0D';
			NewLine[1]=(byte)'\x0A';
			if (FileNumber<1 || FileNumber>255)
				throw new System.IO.IOException (FileNumber.ToString() + " does not exists",52);
			if (FHandle[FileNumber - 1]==null)
				throw new System.IO.IOException(FileNumber + " does not exists",52);
			if ( FMode[ FileNumber - 1] != OpenMode.Output  )
				throw new System.IO.IOException ("FileMode is invalid");
			if (Output.Length == 0)
			{
				FHandle[FileNumber - 1].Write(NewLine,0,2);
			}
			else
			{
				for (int MyArgs=0;MyArgs<Output.Length;MyArgs++)
				{
					MyBuf=WriteAuxiliar(Output[MyArgs]);
					for (int PutsData=0;PutsData<MyBuf.Length;PutsData++)
					{
						bufout[0]=(byte)MyBuf[PutsData];
						FHandle[FileNumber-1].Write(bufout,0,1);
					}
					if (MyArgs < (Output.Length -1))
					{
						FHandle[FileNumber - 1].Write(Separator,0,1);
					}
					else
					{
						FHandle[FileNumber - 1].Write(NewLine,0,2);
					}
				}
			}
		}
		private static string WriteAuxiliar(Object Argument)
		{
			string retval="";
			if (Argument==null)
				retval="#NULL#";
			else
			{
				switch (Argument.GetType().ToString())
				{
				case "System.Boolean": 
					if ( (bool)Argument == true)
						retval="#TRUE#";
					else
						retval="#FALSE#";
					break;
				case "System.String":
					retval="\"" + (string)Argument + "\"";
					break;
				case "System.Int64":
				case "System.UInt64":
				case "System.Int32":
				case "System.UInt32":
				case "System.Int16":
				case "System.UInt16":
				case "System.Sbyte":
				case "System.Byte":
					retval = Argument.ToString();
					break;
				case "System.Single":
				case "System.Double":
				case "System.Decimal":
					string buf= (Argument.ToString()) ;
					if ( buf.IndexOf(System.Globalization.NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator)>=0)
						retval=buf.Substring(0, buf.IndexOf(System.Globalization.NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator)) 
						+ "." + buf.Substring(1+buf.IndexOf(System.Globalization.NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator));
	 				break;
				case "System.Exception":
					retval=((Exception)Argument).ToString();
					break;
				case "System.Char":
					retval=((char)Argument).ToString();
					break;
				case "System.DateTime":
					retval=((System.DateTime )Argument).ToString("u");
					retval=retval.Substring(0,retval.Length -1);
					if (retval.Substring(11,8)=="00:00:00")
						retval=retval.Substring(0,10);
					retval= "#" + retval + "#";
					break;
				default :
					throw new NotImplementedException ();
					
				}
			}
			return retval;
		}
                [MonoTODO("Needs Testing")]
                public static System.String InputString (System.Int32 FileNumber, System.Int32 CharCount) 
                {
                	byte[] Buf;
                	string retval="";
                	//
                	// exceptions
                	if ( FileNumber<1 || FileNumber>255)
                		throw new System.IO.IOException (FileNumber.ToString() + " does not exists",52);
                	if ( FHandle[FileNumber - 1] == null)
                		throw new System.IO.IOException (FileNumber.ToString() + " does not exists",52);
                	if ( (FMode[FileNumber - 1] != OpenMode.Input) && (FMode[FileNumber - 1] != OpenMode.Binary ))
                		throw new System.IO.IOException ("FileMode is invalid");
                	if (CharCount <0 || CharCount>2e14 )
                		throw new System.ArgumentException(); 
                	if ( (CharCount + FHandle[FileNumber - 1].Position) > FHandle[FileNumber - 1].Length)
                		throw new System.IO.EndOfStreamException();	
                	//
                	// implementation
                	Buf=new byte[CharCount];
                	FHandle[FileNumber - 1].Read(Buf,0,Buf.Length);
                	for (int myloop=0;myloop<Buf.Length;myloop++)
                		retval+=((char)Buf[myloop]).ToString();
                	return retval;
                }
                [MonoTODO("Needs testing")]
                public static System.String LineInput (System.Int32 FileNumber) 
                { 
                	string retval="";
 					int buf='\x00';  
                	bool found=false;
                	if ( FileNumber<1 || FileNumber>255)
                		throw new System.IO.IOException (FileNumber.ToString() + " does not exists",52);
                	if ( FHandle[FileNumber - 1] == null)
                		throw new System.IO.IOException (FileNumber.ToString() + " does not exists",52);
                	          	
                	if ( EOF(FileNumber) )
                		throw new System.IO.EndOfStreamException();
                	
                	while (!found)
                	{
                		 
                		buf=FHandle[FileNumber - 1].ReadByte();
                		if ( (buf == -1) || (buf == '\x0A' ) )
                			found=true;
                		else
                			retval+= ((char)buf).ToString();
                	}
                	if ( retval.Length > 0 )
                		if ( (buf == '\x0A') && (retval[retval.Length -1 ] == '\x0D') )
                			retval=retval.Substring(0,retval.Length -1) ;
                	return retval;
                	    
                }
                
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
                
                [MonoTODO("Needs testing")]                
                public static System.Int32 FreeFile () 
                { 
                	int bucle=0;
                	bool found=false;
                	for (bucle=0;bucle<255;bucle++)
                		if (FHandle[bucle]==null)
                		{
                			found=true;
                			break;
                		}
                	if (!found)
                		throw new System.IO.IOException ("More than 255 files are in use",67);
                	else
                		return bucle+1;
                }
                [MonoTODO]
                public static void Seek (System.Int32 FileNumber, System.Int64 Position) { throw new NotImplementedException (); }
                [MonoTODO]
                public static System.Int64 Seek (System.Int32 FileNumber) { throw new NotImplementedException (); }
                
                [MonoTODO("Needs testing")]
                public static System.Boolean EOF ( System.Int32 FileNumber) 
                { 
                	if (FileNumber<1 || FileNumber>255)
                		throw new System.IO.IOException (FileNumber.ToString() + " does not exists",52);
                	if ( FHandle[FileNumber - 1] == null)
                		throw new System.IO.IOException (FileNumber.ToString() + " does not exists",52);
                	if ( FHandle[FileNumber - 1].Length == FHandle[FileNumber - 1].Position)
                		return true;
                	else
                		return false;
                		
                }
                
                [MonoTODO]
                public static System.Int64 Loc (System.Int32 FileNumber) { throw new NotImplementedException (); }
                [MonoTODO]
                public static System.Int64 LOF (System.Int32 FileNumber) 
                {
                	if (FileNumber<1 || FileNumber>255)
                		throw new System.IO.IOException (FileNumber.ToString() + " does not exists",52);
                	if ( FHandle[FileNumber - 1] == null)
                		throw new System.IO.IOException (FileNumber.ToString() + " does not exists",52);	
                	return (System.Int64)FHandle[FileNumber - 1].Length;
                }
                [MonoTODO]
                public static Microsoft.VisualBasic.TabInfo TAB () { throw new NotImplementedException (); }
                [MonoTODO]
                public static Microsoft.VisualBasic.TabInfo TAB (System.Int16 Column) { throw new NotImplementedException (); }
                [MonoTODO]
                public static Microsoft.VisualBasic.SpcInfo SPC (System.Int16 Count) { throw new NotImplementedException (); }
                [MonoTODO("Needs Testing")]
                public static Microsoft.VisualBasic.OpenMode FileAttr (System.Int32 FileNumber) 
                { 
                	if (FileNumber<1 || FileNumber>255)
                		throw new System.IO.IOException (FileNumber.ToString() + " does not exists",52);
                	if ( FHandle[FileNumber - 1] == null)
                		throw new System.IO.IOException (FileNumber.ToString() + " does not exists",52);
               		return FMode[FileNumber - 1];
                }
                [MonoTODO]
                public static void Reset () { throw new NotImplementedException (); }
                [MonoTODO("Needs Testing")]
                public static void Rename (System.String OldPath, System.String NewPath) 
		{
			if ( !File.Exists (OldPath) && !Directory.Exists( OldPath))
				throw new System.ArgumentException ( OldPath + " does not exist");
			if ( File.Exists ( NewPath) || Directory.Exists ( NewPath))
				throw new System.IO.IOException ( NewPath + " already exists");
			if ( File.Exists (OldPath))
				File.Move (OldPath, NewPath);
			else
				Directory.Move (OldPath, NewPath); 
		}
                // Events
                
        };
}
