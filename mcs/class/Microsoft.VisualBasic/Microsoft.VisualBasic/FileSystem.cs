/*
 * Copyright (c) 2002-2003 Mainsoft Corporation.
 * Copyright (C) 2004 Novell, Inc (http://www.novell.com)
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */
 

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Globalization;
using Microsoft.VisualBasic;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic.CompilerServices;

/**
 * CURRENT LIMITATIONS
 * @limit TAB(int) - not supported.
 * @limit not all file attributes are supported. supported are: Hidden, Directory, ReadOnly
 * @limit ChDir(..) - not supported.
 * @limit ChDrive - not supported.
 * @limit CurDir(Char) - ignores the parameter.
 */

namespace Microsoft.VisualBasic
{
	[StandardModuleAttribute]
	sealed public class FileSystem
	{
		private FileSystem () {}

		private static Hashtable _fileNameIdMap = new Hashtable();
		private static Hashtable _openFilesMap = new Hashtable();
		private static Hashtable _randomRecordLength = new Hashtable();

		static String _pattern;
		static int _fileAttrs;
		private static int _fileIndex;
		private static FileInfo[] _files;
		private static bool _isEndOfFiles = true;


		public static void Reset()
		{
			ICollection s = _openFilesMap.Keys;

			int [] iArr = new int[s.Count];
			s.CopyTo(iArr, 0);
			close(iArr);
		}

		public static void FileClose(int[] fileNumbers)
		{
			int[] iArr = null;
			if (fileNumbers.Length == 0)
			{
				ICollection keySet = FileSystem._openFilesMap.Keys;
				iArr = new int[keySet.Count];
				keySet.CopyTo(iArr, 0);
			}
			else
			{
				iArr = new int[fileNumbers.Length];
				for (int i = 0; i < fileNumbers.Length; i++)
					iArr[i] = fileNumbers[i];
			}
			close(iArr);
		}

		private static void close(int [] keys)
		{
			Object obj;

			for (int i = 0; i < keys.Length; i++)
			{
				if (keys[i] < 0 || keys[i] > 255)
				{
					ExceptionUtils.VbMakeException(
								       VBErrors.IllegalFuncCall);
					throw new ArgumentOutOfRangeException();
				}
				obj = _openFilesMap[keys[i]];

				if (obj == null)
				{
					String message = VBUtils.GetResourceString(52);
					throw (IOException)VBUtils.VBException(new IOException(message), 52);
				}
				String fileName = null;
				if (obj is VBFile)
				{
					((VBFile)obj).closeFile();
					fileName = ((VBFile)obj).getFullPath();
				}


				_openFilesMap.Remove(keys[i]);
				_fileNameIdMap.Remove(fileName);
			}
		}

		public static void FileOpen(
					    int fileNumber,
					    String fileName,
					    OpenMode mode,
					    [Optional, __DefaultArgumentValue((OpenAccess)(-1))] OpenAccess access, 
					    [Optional, __DefaultArgumentValue((OpenShare)(-1))] OpenShare share, 
					    [Optional, __DefaultArgumentValue(-1)] int recordLength)

		{
			if (!isFileNumberFree(fileNumber))
				throw ExceptionUtils.VbMakeException(VBErrors.FileAlreadyOpen);
			if (fileNumber < 0 || fileNumber > 255)
				throw ExceptionUtils.VbMakeException(VBErrors.BadFileNameOrNumber);
			if (recordLength != -1 && recordLength <= 0)
				throw ExceptionUtils.VbMakeException(VBErrors.IllegalFuncCall);
			if (share != OpenShare.Shared && _fileNameIdMap.ContainsKey(Path.GetFullPath(string.Intern(fileName))))
				throw ExceptionUtils.VbMakeException(VBErrors.FileAlreadyOpen);
			if (mode == OpenMode.Input)
			{
				VBFile vbFile = new InputVBFile(fileName, (FileAccess) access,(recordLength == -1)? 4096:recordLength);
				_openFilesMap.Add(fileNumber, vbFile);
			}
			else if (mode == OpenMode.Output || mode == OpenMode.Append)
			{
				VBFile vbFile = new OutPutVBFile(fileName,mode,access,recordLength);
				_openFilesMap.Add(fileNumber, vbFile);
			}
			else if (mode == OpenMode.Random)
			{
				VBFile vbFile = new RandomVBFile(fileName,mode,access,recordLength);
				_openFilesMap.Add(fileNumber, vbFile);
			}
			else if (mode == OpenMode.Binary)
			{
				VBFile vbFile = new BinaryVBFile(fileName,mode,access,recordLength);
				_openFilesMap.Add(fileNumber, vbFile);
			}
			_fileNameIdMap.Add(Path.GetFullPath(string.Intern(fileName)),fileNumber);
		}

		public static void FilePut(int fileNumber,
					   bool value,
					   [Optional, __DefaultArgumentValue(-1)] long  recordNumber)
		{
			checkRecordNumber(recordNumber,false);
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.put(value,recordNumber);
		}


		public static void FilePut(int fileNumber, 
					   byte value, 
					   [Optional, __DefaultArgumentValue((long)-1)] long  recordNumber)
		{
			checkRecordNumber(recordNumber,false);
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.put(value,recordNumber);
		}

		public static void FilePut(int fileNumber, 
					   short value, 
					   [Optional, __DefaultArgumentValue((long)-1)] long  recordNumber)
		{
			checkRecordNumber(recordNumber,false);
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.put(value,recordNumber);
		}


		public static void FilePut(int fileNumber, 
					   char value, 
					   [Optional, __DefaultArgumentValue((long)-1)] long  recordNumber)
		{
			checkRecordNumber(recordNumber,false);
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.put(value,recordNumber);
		}

		public static void FilePut(int fileNumber, 
					   int value, 
					   [Optional, __DefaultArgumentValue((long)-1)] long  recordNumber)
		{
			checkRecordNumber(recordNumber,false);
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.put(value,recordNumber);
		}

		public static void FilePut(int fileNumber, 
					   long value, 
					   [Optional, __DefaultArgumentValue((long)-1)] long  recordNumber)
		{
			checkRecordNumber(recordNumber,false);
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.put(value,recordNumber);
		}


		public static void FilePut(int fileNumber, 
					   float value, 
					   [Optional, __DefaultArgumentValue((long)-1)] long  recordNumber)
		{
			checkRecordNumber(recordNumber,false);
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.put(value,recordNumber);
		}

		public static void FilePut(int fileNumber, 
					   double value, 
					   [Optional, __DefaultArgumentValue((long)-1)] long  recordNumber)
		{
			checkRecordNumber(recordNumber,false);
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.put(value,recordNumber);
		}

		public static void FilePut(int fileNumber,
					   String value,
					   [Optional, __DefaultArgumentValue((long)-1)] long recordNumber,
					   [Optional, __DefaultArgumentValue(false)] bool stringIsFixedLength)
		{
			checkRecordNumber(recordNumber,true);
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.put(value,recordNumber,stringIsFixedLength);
		}

		public static void FilePut(int fileNumber,
					   DateTime value,
					   [Optional, __DefaultArgumentValue((long)-1)] long  recordNumber)
		{
			checkRecordNumber(recordNumber,false);
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.put(value,recordNumber);
		}

		public static void FileGet(int fileNumber,
					   ref bool value,
					   [Optional, __DefaultArgumentValue((long)-1)] long recordNumber) 

		{
			checkRecordNumber(recordNumber,false);
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.get(out value,recordNumber);
		}

		private static void checkRecordNumber(long recordNumber,bool throwArgExc)
		{
			if ((recordNumber < 1) && (recordNumber != -1))
			{
				if (!throwArgExc)
					throw (IOException) ExceptionUtils.VbMakeException(
											   new ArgumentException(
														 Utils.GetResourceString(
																	 "Argument_InvalidValue1",
																	 "RecordNumber")),
											   VBErrors.BadRecordNum);
				else
				{
					ExceptionUtils.VbMakeException(VBErrors.BadRecordNum);
					throw new ArgumentException(
								    Utils.GetResourceString(
											    "Argument_InvalidValue1",
											    "RecordNumber"));
				}

			}
		}

		private static VBFile getVBFile(int fileNumber)
		{
			if ((fileNumber < 1) || (fileNumber > 255))
				throw (IOException) ExceptionUtils.VbMakeException(
										   VBErrors.BadFileNameOrNumber);
			Object obj = _openFilesMap[fileNumber];
			if (obj == null)
				throw (IOException) ExceptionUtils.VbMakeException(
										   VBErrors.BadFileNameOrNumber);
			return (VBFile)obj;
		}

		private static VBFile getVBFile(String pathName)
		{
			object o = _fileNameIdMap[pathName];
			int fileNumber = o == null ? 0 : (int) o ;
			if (fileNumber == 0)
				throw (IOException) ExceptionUtils.VbMakeException(
										   VBErrors.BadFileNameOrNumber);
			Object obj = _openFilesMap[fileNumber];
			if (obj == null)
				throw (IOException) ExceptionUtils.VbMakeException(
										   VBErrors.BadFileNameOrNumber);
			return (VBFile)obj;
		}

		public static void FileGet(
					   int fileNumber,
					   ref byte value,
					   [Optional, __DefaultArgumentValue((long)-1)] long recordNumber) 

		{
			checkRecordNumber(recordNumber,false);
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.get(out value,recordNumber);
		}

		public static void FileGet(
					   int fileNumber,
					   ref short value,
					   [Optional, __DefaultArgumentValue((long)-1)] long recordNumber) 

		{
			checkRecordNumber(recordNumber,false);
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.get(out value,recordNumber);
		}

		public static void FileGet(
					   int fileNumber,
					   ref char value,
					   [Optional, __DefaultArgumentValue((long)-1)] long recordNumber) 


		{
			checkRecordNumber(recordNumber,false);
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.get(out value,recordNumber);
		}


		public static void FileGet(int fileNumber, 
					   ref int value,
   					   [Optional, __DefaultArgumentValue((long)-1)] long recordNumber) 
		{
			checkRecordNumber(recordNumber,false);
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.get(out value,recordNumber);
		}

		public static void FileGet(int fileNumber, 
					   ref long value, 
					   [Optional, __DefaultArgumentValue((long)-1)] long recordNumber) 
		{
			checkRecordNumber(recordNumber,false);
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.get(out value,recordNumber);
		}

		public static void FileGet(int fileNumber,
					   ref float value,
					   [Optional, __DefaultArgumentValue((long)-1)] long recordNumber) 
		{
			checkRecordNumber(recordNumber,false);
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.get(out value,recordNumber);
		}

		public static void FileGet(int fileNumber,
					   ref double value,
					   [Optional, __DefaultArgumentValue((long)-1)] long recordNumber) 
					   
		{
			checkRecordNumber(recordNumber,false);
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.get(out value,recordNumber);
		}

		public static void FileGet(int fileNumber,
					   ref Decimal value,
					   [Optional, __DefaultArgumentValue((long)-1)] long recordNumber) 
					   
		{
			checkRecordNumber(recordNumber,false);
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.get(out value,recordNumber);
		}

		public static void FileGet(int fileNumber,
					   ref string value,
					   [Optional, __DefaultArgumentValue((long)-1)] long recordNumber,
					   [Optional, __DefaultArgumentValue(false)] bool bIgnored)
		{
			checkRecordNumber(recordNumber,true);
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.get(ref value,recordNumber,bIgnored);
		}

		public static void FileGet(int fileNumber,
					   ref Object value,
					   [Optional, __DefaultArgumentValue((long)-1)] long recordNumber) 
		{
			checkRecordNumber(recordNumber,false);
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.get(ref value,recordNumber);
		}

		public static long Seek(int fileNumber)
		{
			VBFile vbFile = getVBFile(fileNumber);
			return vbFile.seek();
		}

		public static void Seek(int fileNumber, long position)
		{
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.seek(position);

		}

		public static long Loc(int fileNumber)
		{
			VBFile vbFile = getVBFile(fileNumber);
			return vbFile.getPosition();
		}

		public static long LOF(int fileNumber)
		{
			VBFile vbFile = getVBFile(fileNumber);
			return vbFile.getLength();
		}

		public static void Input(int fileNumber, ref Object value)
		{
			VBFile vbFile = getVBFile(fileNumber);
			if (value != null)
			{
				if (value is bool) {
					bool tmp;
					vbFile.Input(out tmp);
					value = tmp;
				}
				else if (value is byte) {
					byte tmp;
					vbFile.Input(out tmp);
					value = tmp;
				}
				else if (value is char) {
					char tmp;
					vbFile.Input(out tmp);
					value = tmp;
				}
				else if (value is double) {
					double tmp;
					vbFile.Input(out tmp);
					value = tmp;
				}
				else if (value is short) {
					short tmp;
					vbFile.Input(out tmp);
					value = tmp;
				}
				else if (value is int) {
					int tmp;
					vbFile.Input(out tmp);
					value = tmp;
				}
				else if (value is long) {
					long tmp;
					vbFile.Input(out tmp);
					value = tmp;
				}
				else if (value is float) {
					float tmp;
					vbFile.Input(out tmp);
					value = tmp;
				}
				else if (value is Decimal) {
					Decimal tmp;
					vbFile.Input(out tmp);
					value = tmp;
				}
				else if (value is string)
					vbFile.Input((string)value);
				// Come back to it later
				// 			else if (type.get_IsByRef())
				// 				vbFile.Input((ObjectRefWrapper)value[i],false);

			}
		}

		public static String InputString(int fileNumber, int count)
		{
			if (count < 0)
				throw (ArgumentException)ExceptionUtils.VbMakeException(VBErrors.IllegalFuncCall);
			VBFile vbFile = getVBFile(fileNumber);
			if (vbFile.getLength()- vbFile.getPosition() < count)
				throw (EndOfStreamException)ExceptionUtils.VbMakeException(VBErrors.EndOfFile);
			return vbFile.InputString(count);
		}

		public static String LineInput(int fileNumber)
		{
			VBFile vbFile = getVBFile(fileNumber);

			if (EOF(fileNumber))
				throw new EndOfStreamException("Input past end of file.");

			return vbFile.readLine();
		}

		public static void Print(int fileNumber, Object[] output)
		{
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.print(output);
		}

// Seems not to exist in MS's 1.1 implementation as told by class status pages
//		public static void PrintLine(int fileNumber)
//		{
//			VBFile vbFile = getVBFile(fileNumber);
//			vbFile.printLine(null);
//		}

		public static void PrintLine(int fileNumber, Object[] output)
		{
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.printLine(output);
		}

		public static void Write(int fileNumber, Object[] output)
		{
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.write(output);
		}

		public static void WriteLine(int fileNumber, Object[] output)
		{
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.writeLine(output);
		}

		public static void Rename(String oldPath, String newPath)
		{
			FileInfo file = new FileInfo(newPath);
			if (file.Exists)
				throw (IOException) ExceptionUtils.VbMakeException(
										   VBErrors.FileAlreadyExists);
			try
			{
				Directory.Move(oldPath, newPath);
			}catch (DirectoryNotFoundException e){
				throw (ArgumentException)ExceptionUtils.VbMakeException(VBErrors.IllegalFuncCall);
			}
		}

		public static void FileCopy(String source, String destination)
		{
			DirectoryInfo dir;

			if ((source == null) || (source.Length == 0))
			{
				ExceptionUtils.VbMakeException(VBErrors.BadFileNameOrNumber);
				throw new ArgumentException(
							    Utils.GetResourceString("Argument_PathNullOrEmpty"));
			}

			if ((destination == null) || (destination.Length == 0))
			{
				ExceptionUtils.VbMakeException(VBErrors.BadFileNameOrNumber);
				throw new ArgumentException(
							    Utils.GetResourceString("Argument_PathNullOrEmpty"));
			}

			FileInfo f = new FileInfo(source);
			if (!f.Exists)
				throw (FileNotFoundException) ExceptionUtils.VbMakeException(
											     VBErrors.FileNotFound);
			if (_fileNameIdMap[source] != null)
				throw (IOException) ExceptionUtils.VbMakeException(
										   VBErrors.FileAlreadyOpen);
			int lastIndex = destination.LastIndexOf('/');

			if(lastIndex == -1)
				dir = new DirectoryInfo(".");
			else
				dir = new DirectoryInfo(destination.Substring(0,lastIndex));

			if (!dir.Exists)
			{
				ExceptionUtils.VbMakeException(VBErrors.FileAlreadyOpen);
				throw new DirectoryNotFoundException();
			}

			// the file name length is 0
			if (destination.Length == lastIndex +1)
			{
				throw (IOException)ExceptionUtils.VbMakeException(VBErrors.FileAlreadyOpen);

			}

			f = new FileInfo(destination);
			if (f.Exists)
				throw (IOException) ExceptionUtils.VbMakeException(
										   VBErrors.FileAlreadyExists);
			File.Copy(source, destination);
		}

		public static void MkDir(String path)
		{
			if ((path == null) || (path.Length == 0))
			{
				ExceptionUtils.VbMakeException(VBErrors.BadFileNameOrNumber);
				throw new ArgumentException(
							    Utils.GetResourceString("Argument_PathNullOrEmpty"));
			}
			if (Directory.Exists(path))
				throw (IOException) ExceptionUtils.VbMakeException(
										   VBErrors.PathFileAccess);

			Directory.CreateDirectory(path);
		}

		public static void Kill(String pathName)
		{
			if (_fileNameIdMap[pathName] != null)
				throw (IOException) ExceptionUtils.VbMakeException(
										   VBErrors.FileAlreadyOpen);
			if (!File.Exists(pathName))
				throw (IOException) ExceptionUtils.VbMakeException(
										   VBErrors.FileNotFound);
			File.Delete(pathName);
		}

		public static void RmDir(String pathName)
		{
			if (pathName == null || pathName.Length == 0 )
			{
				ExceptionUtils.VbMakeException(VBErrors.BadFileNameOrNumber);
				throw new ArgumentException(pathName);
			}
			DirectoryInfo dir = new DirectoryInfo(pathName);
			if (!dir.Exists)
			{
				ExceptionUtils.VbMakeException(VBErrors.PathNotFound);
				throw new DirectoryNotFoundException();
			}
			if (dir.GetFiles().Length != 0)
				throw (IOException) ExceptionUtils.VbMakeException(
										   VBErrors.PathFileAccess);
			Directory.Delete(pathName);
		}

		public static FileAttribute  GetAttr(String pathName)
		{
			if (pathName == null || pathName.Length == 0 ||
			    pathName.IndexOf('*') != -1 || pathName.IndexOf('?') != -1)
			{
				throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileNameOrNumber);
			}
			// 		File f = new File(pathName);
			// 		if (!f.exists())
			// 			throw (FileNotFoundException)
			// 				ExceptionUtils.VbMakeException(VBErrors.FileNotFound);
			return (FileAttribute) File.GetAttributes(pathName);
		}

		public static void SetAttr(String pathName, FileAttribute fileAttr)
		{
			if (pathName == null || pathName.Length == 0 ||
			    pathName.IndexOf('*') != -1 || pathName.IndexOf('?') != -1)
			{
				throw (ArgumentException)ExceptionUtils.VbMakeException(VBErrors.IllegalFuncCall);
			}
			FileInfo f = new FileInfo(pathName);
			if (!f.Directory.Exists)
			{
				ExceptionUtils.VbMakeException(VBErrors.PathNotFound);
				throw new DirectoryNotFoundException();
			}
			if (!f.Exists)
			{
				throw (FileNotFoundException)
					ExceptionUtils.VbMakeException(VBErrors.FileNotFound);
			}

			try
			{
				File.SetAttributes(pathName, (FileAttributes)fileAttr);
			}
			catch (ArgumentException e)
			{
				throw (ArgumentException) ExceptionUtils.VbMakeException(
											 VBErrors.IllegalFuncCall);
			}
			catch (DirectoryNotFoundException ex)
			{
				ExceptionUtils.VbMakeException(VBErrors.PathNotFound);
				throw ex;
			}
			catch (FileNotFoundException ex)
			{
				throw (FileNotFoundException)
					ExceptionUtils.VbMakeException(VBErrors.FileNotFound);
			}

		}


		public static /*synchronized*/ String Dir(String pathName)
		{
			return Dir(pathName, 0);
		}

		public static /*synchronized*/ String Dir(String pathName, 
							  [Optional, __DefaultArgumentValue((FileAttribute)0)] 
							  FileAttribute fileAttribute)
		{
			_fileIndex = 0;
			_files = null;
			_pattern = null;

			_fileAttrs = (int)fileAttribute;

			if (pathName == null || pathName.Equals(""))
			{
				return "";
			}

			if (FileAttribute.Volume == fileAttribute)
				return "";


			int lastBabkSlashInx = pathName.LastIndexOf('\\');
			int lastSlashInx = pathName.LastIndexOf('/');
			int maxIndex = (lastSlashInx>lastBabkSlashInx)?lastSlashInx:lastBabkSlashInx; 
			String dir = pathName.Substring(0, maxIndex + 1);
			String fileName = pathName.Substring(1 + maxIndex);
			if (fileName == null || fileName.Length == 0)
				fileName = "*";

			//        int astricsInx = fileName.indexOf('*');
			//        int questionInx = fileName.indexOf('?');

			//        String pattern;
			DirectoryInfo directory = new DirectoryInfo(dir);
			//      java.io.File directory = new java.io.File(dir);
			//		java.io.File file;
			if (!directory.Exists)
			{
				// path not found - return empty string
				return "";
			}

			//        if (astricsInx == -1 && questionInx == -1)
			//        {
			//            pattern = fileName;
			//        }
			//        else
			//        {
			//            pattern = Strings.Replace(fileName, ".", "\\.", 1, -1, CompareMethod.Binary);
			//            pattern = Strings.Replace(pattern, "*", ".*", 1, -1, CompareMethod.Binary);
			//            pattern = Strings.Replace(pattern, "?", ".?", 1, -1, CompareMethod.Binary);
			//        }

			_pattern = fileName;
			//        _pattern = Pattern.compile(pattern, Pattern.CASE_INSENSITIVE);

			_files = directory.GetFiles(_pattern);
			String answer;
			if (_files == null || _files.Length == 0)
			{
				DirectoryInfo[] dirs = directory.GetDirectories(_pattern);
				if (dirs == null || dirs.Length == 0)
				{
					return "";
				}
				answer = dirs[0].Name;            
			}
			else
			{   
				answer = _files[0].Name;
			}   

			_fileIndex++;
			_isEndOfFiles = false;


			return answer;

		}

		public static /*synchronized*/ String Dir()
		{
			String name;
			if (_files == null || _isEndOfFiles)
				throw new /*Illegal*/ArgumentException("no path has been initiated");

			if (_fileIndex < _files.Length)
			{
				name = _files[_fileIndex].Name;
				_fileIndex++;
			}
			else
			{
				_isEndOfFiles = true;
				name = "";
			}

			return name;
		}

		public static DateTime FileDateTime(String pathName)
		{
			if (pathName == null || pathName.Length == 0 ||
			    pathName.IndexOf('*') != -1 || pathName.IndexOf('?') != -1)
			{
				ExceptionUtils.VbMakeException(VBErrors.BadFileNameOrNumber);
				throw new ArgumentException(
							    VBUtils.GetResourceString(
										      "Argument_InvalidValue1",
										      "PathName"));
			}
			FileInfo f = new FileInfo(pathName);
			if (!f.Exists)
			{
				DirectoryInfo d = new DirectoryInfo(pathName);
				if (!d.Exists)	
					throw (FileNotFoundException)
						ExceptionUtils.VbMakeException(VBErrors.FileNotFound);
				return d.LastWriteTime;
			}
			return f.LastWriteTime;
		}

		public static long FileLen(String pathName)
		{
			FileInfo f = new FileInfo(pathName);
			if (!f.Exists)
				throw new FileNotFoundException(
								"file not exists: " + pathName);

			return f.Length;
		}


		internal static String SPC(int count)
		{
			StringBuilder sb = new StringBuilder(count);
			for (int i = 0; i < count; i++)
				sb.Append(' ');

			return sb.ToString();
		}

		public static int FreeFile()
		{
			ICollection s = _openFilesMap.Keys;

			if (s.Count == 0)
				return 1;

			int [] keyArr = new int[s.Count];

			s.CopyTo(keyArr, 0);

			Array.Sort(keyArr);
			int i = 0;
			for (; i < keyArr.Length - 1; i++)
			{
				if ((keyArr[i]+ 1) < keyArr[i + 1])
					break;
			}

			int retVal = keyArr[i]+ 1;

			if (retVal > 255)
			{
				String message = VBUtils.GetResourceString(67);
				throw (IOException)VBUtils.VBException(
								       new IOException(message),
								       67);
			}

			return retVal;

		}

		public static bool EOF(int fileNumber)
		{
			bool  retVal = false;
			VBFile vbFile = getVBFile(fileNumber);
			return vbFile.isEndOfFile();
		}

		// check if a specific number is free
		private static bool isFileNumberFree(int fileNumber)
		{
			return !_openFilesMap.ContainsKey(fileNumber);
		}

		[MonoTODO("If path is another drive, it should change the default folder for that drive, but not switch to it.")]
		public static void ChDir (string Path) 
		{
			if ((Path=="") || (Path==null))
				throw new ArgumentException (Utils.GetResourceString ("Argument_PathNullOrEmpty")); 
			try {
				Environment.CurrentDirectory = Path;
			}
			catch { 
				throw new FileNotFoundException (Utils.GetResourceString ("FileSystem_PathNotFound1", Path));
			}
		}


		public static void ChDrive(char Drive)
		{
			Drive = char.ToUpper(Drive, CultureInfo.InvariantCulture);
			if ((Drive < 65) || (Drive > 90))
			{
				throw new ArgumentException(
							    Utils.GetResourceString("Argument_InvalidValue1", "Drive"));
			}
			Directory.SetCurrentDirectory(String.Concat(StringType.FromChar(Drive), StringType.FromChar(Path.VolumeSeparatorChar)));
		}

		public static void ChDrive(String Drive)
		{
			if (Drive != null && Drive.Length != 0)
				FileSystem.ChDrive(Drive[0]);
		}

		public static String CurDir()
		{
			return Environment.CurrentDirectory;
		}

		public static String CurDir(char Drive)
		{
			return Directory.GetCurrentDirectory();
		}

		public static void FileGetObject(int fileNumber,
						 ref object value,
						 [Optional, __DefaultArgumentValue((long)-1)] long recordNumber) 


		{
			checkRecordNumber(recordNumber,true);
			VBFile vbFile = getVBFile(fileNumber);

			Type type = value.GetType();

			if (type == null || value is string) {
				string tmp = null;
				vbFile.get(ref tmp, recordNumber, false);
				value = tmp;
			}
			else if ( value is bool) {
				bool tmp;
				vbFile.get(out tmp,recordNumber);
				value = tmp;
			}
			else if ( value is char) {
				char tmp;
				vbFile.get(out tmp,recordNumber);
				value = tmp;
			}
			else if ( value is byte) {
				byte tmp;
				vbFile.get(out tmp,recordNumber);
				value = tmp;
			}
			else if ( value is short) {
				short tmp;
				vbFile.get(out tmp,recordNumber);
				value = tmp;
			}
			else if ( value is int) {
				int tmp;
				vbFile.get(out tmp,recordNumber);
				value = tmp;
			}
			else if ( value is long) {
				long tmp;
				vbFile.get(out tmp,recordNumber);
				value = tmp;
			}
			else if ( value is float) {
				float tmp;
				vbFile.get(out tmp,recordNumber);
				value = tmp;
			}
			else if ( value is double) {
				double tmp;
				vbFile.get(out tmp,recordNumber);
				value = tmp;
			}
			else if ( value is Decimal) {
				Decimal tmp;
				vbFile.get(out tmp,recordNumber);
				value = tmp;
			}
			else if ( value is DateTime) {
				DateTime tmp;
				vbFile.get(out tmp,recordNumber);
				value = tmp;
			}
			else if (type.IsArray) {
				// need to figure out how to convert from Object& to Array&
				// vbFile.get(out value, recordNumber,true,false);
				// value = tmp;
				throw new NotImplementedException();
			}
			else
				throw new NotSupportedException();
		}

		public static void FileGet(int fileNumber,
					   ref DateTime value,
					   [Optional, __DefaultArgumentValue((long)-1)] long recordNumber)

		{
			checkRecordNumber(recordNumber,true);
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.get(out value,recordNumber);
		}

		[MonoTODO]
		public static void FileGet(int fileNumber, 
						ref ValueType value, 
						long recordNumber) 
		{
			throw new NotImplementedException();
		}

		public static void FileGet(int fileNumber,
					   ref Array value,
					   [Optional, __DefaultArgumentValue((long)-1)] long recordNumber, 
					   [Optional, __DefaultArgumentValue(false)] bool arrayIsDynamic, 
					   [Optional, __DefaultArgumentValue(false)] bool stringIsFixedLength) 


		{
			checkRecordNumber(recordNumber,true);
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.get(ref value,recordNumber,arrayIsDynamic,stringIsFixedLength);
		}

		public static void FilePutObject(int fileNumber,
						 Object value,
						 [Optional, __DefaultArgumentValue((long)-1)] long recordNumber)

		{
			checkRecordNumber(recordNumber,true);
			VBFile vbFile = getVBFile(fileNumber);
			Type type = value.GetType();
			if(value is string || value == null)
				vbFile.put((String)value,recordNumber,false);
			else if( value is bool) {
				vbFile.put((bool)value, recordNumber);
			}
			else if( value is char) {
				vbFile.put((char)value,recordNumber);
			}
			else if( value is byte) {
				vbFile.put((byte)value, recordNumber);
			}
			else if( value is short) {
				vbFile.put((short)value, recordNumber);
			}
			else if( value is int) {
				vbFile.put((int)value, recordNumber);
			}
			else if( value is long) {
				vbFile.put((long)value, recordNumber);
			}
			else if( value is float) {
				vbFile.put((float)value, recordNumber);
			}
			else if( value is double) {
				vbFile.put((double)value, recordNumber);
			}
			else if( value is Decimal) {
				vbFile.put((Decimal)value,recordNumber);
			}
			else if( value is DateTime) {
				vbFile.put((DateTime)value, recordNumber);
			}
			else if(type.IsArray) {
				vbFile.put(value,recordNumber,true,false);
			}
			else {
				throw new NotSupportedException();
			}

		}

		private const string obsoleteMsg1 = "Use FilePutObject to write Object types, or";
		private const string obsoleteMsg2 = "coerce FileNumber and RecordNumber to Integer for writing non-Object types";
		private const string obsoleteMsg = obsoleteMsg1 + obsoleteMsg2; 

		[System.ObsoleteAttribute(obsoleteMsg, false)] 
		public static void FilePut(Object FileNumber,
					   Object Value,
					   [Optional, __DefaultArgumentValue(-1)] System.Object RecordNumber)
		{
			throw new ArgumentException(Utils.GetResourceString("UseFilePutObject"));
		}

		[MonoTODO]
		public static void FilePut(int FileNumber,
					   ValueType Value,
					   [Optional, __DefaultArgumentValue((long)-1)] System.Int64 RecordNumber)

		{
			throw new NotImplementedException();
		}

		public static void FilePut(int fileNumber,
					   Array value,
					   [Optional, __DefaultArgumentValue((long)-1)] long recordNumber,
					   [Optional, __DefaultArgumentValue(false)] bool arrayIsDynamic,
					   [Optional, __DefaultArgumentValue(false)] bool stringIsFixedLength)
		{
			checkRecordNumber(recordNumber,true);
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.put(value,recordNumber,arrayIsDynamic,stringIsFixedLength);
		}

		public static void FilePut(int fileNumber,
					   Decimal value,
					   [Optional, __DefaultArgumentValue((long)-1)] long  recordNumber)
		{
			checkRecordNumber(recordNumber,true);
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.put(value,recordNumber);
		}

		public static void Input(int fileNumber, ref bool Value)
		{

			VBFile vbFile = getVBFile(fileNumber);
			vbFile.Input(out Value);
		}

		public static void Input(int fileNumber, ref byte Value)
		{
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.Input(out Value);
		}

		public static void Input(int fileNumber, ref short Value)
		{
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.Input(out Value);
		}

		public static void Input(int fileNumber, ref int Value)
		{
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.Input(out Value);
		}

		public static void Input(int fileNumber, ref long Value)
		{
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.Input(out Value);
		}

		public static void Input(int fileNumber, ref char Value)
		{
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.Input(out Value);
		}

		public static void Input(int fileNumber, ref float Value)
		{
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.Input(out Value);
		}

		public static void Input(int fileNumber, ref double Value)
		{
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.Input(out Value);
		}

		public static void Input(int fileNumber, ref Decimal Value)
		{
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.Input(out Value);
		}

		public static void Input(int fileNumber, ref DateTime Value)
		{
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.Input(out Value);
		}

		public static void Input(int fileNumber, ref string Value)
		{
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.Input(out Value);
		}


		// 	public static void Input$V$FileSystem$ILSystem_Object$$$(int fileNumber, ObjectRefWrapper Value)
		// 	{
		// 		VBFile vbFile = getVBFile(fileNumber);
		// 		vbFile.Input(Value,false);
		// 	}

		[MonoTODO]
		public static void Lock(int fileNumber) 
		{
			throw new NotImplementedException("The method Lock in class FileSystem is not supported");
		}

		[MonoTODO]
		public static void Lock(int FileNumber, long Record) 
		{
			throw new NotImplementedException("The method Lock in class FileSystem is not supported");
		}

		[MonoTODO]
		public static void Lock(int FileNumber, long FromRecord, long ToRecord) 
		{
			throw new NotImplementedException("The method Lock in class FileSystem is not supported");
		}

		[MonoTODO]
		public static void Unlock(int FileNumber) 
		{
			throw new NotImplementedException("The method Unlock in class FileSystem is not supported");
		}

		[MonoTODO]
		public static void Unlock(int FileNumber, long Record) 
		{
			throw new NotImplementedException("The method Unlock in class FileSystem is not supported");
		}

		[MonoTODO]
		public static void Unlock(int FileNumber, long FromRecord, long ToRecord) 
		{
			throw new NotImplementedException("The method Unlock in class FileSystem is not supported");
		}

		public static void FileWidth(int fileNumber, int RecordWidth)
		{
			VBFile vbFile = getVBFile(fileNumber);
			vbFile.width(fileNumber,RecordWidth);
		}

		public static TabInfo TAB()
		{
			return new TabInfo((short) - 1);
		}

		public static TabInfo TAB(short Column)
		{
			return new TabInfo(Column);
		}

		public static SpcInfo SPC(short Count)
		{
			return new SpcInfo(Count);
		}

		public static OpenMode FileAttr(int fileNumber)
		{
			VBFile vbFile = getVBFile(fileNumber);
			return (OpenMode) vbFile.getMode();
		}
	}

	class VBStreamWriter : StreamWriter
	{
		int _currentColumn;
		int _width;

		public VBStreamWriter(string fileName):base(fileName)
		{
		}

		public VBStreamWriter(string fileName, bool append):base(fileName, append)
		{
		}
	}

	//TODO: FileFilters from Mainsoft code

}
