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
using System.Reflection;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;



public abstract class BaseVBFile : VBFile
{
	protected FileStream _fileStream;
	private String _fileName;
	private FileInfo _fileInfo;
	protected FileMode _mode;
	protected FileAccess _access;
	//   protected long _position;
	protected bool _isEOF;
	protected int _recordLen;
	protected long _recordStart;
	protected bool _append;
    
	public BaseVBFile(string fileName,OpenMode mode, OpenAccess access,int recordLength)
	{
		if(access != OpenAccess.Read && access != OpenAccess.Write && access != OpenAccess.ReadWrite)
			throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", "Access"));

		if(mode != OpenMode.Append && mode != OpenMode.Output && mode != OpenMode.Binary && mode != OpenMode.Random
		   && mode != OpenMode.Input)
			throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", "Mode"));

		_fileName = fileName;
		_fileInfo = new FileInfo(fileName);
		_access = (FileAccess) access;
		_mode = (FileMode) mode;
		_recordLen = recordLength;
		FileMode fileMode = FileMode.OpenOrCreate;
		if(mode == OpenMode.Input)
			fileMode = FileMode.Open; 

		this._fileStream = new FileStream(_fileName, fileMode, _access);
	}
    
	public virtual void closeFile()
	{
		if(_fileStream != null) {

			_fileStream.Close();
			_fileStream = null;
		}
	}  
    
	public long getLength()
	{
		return this._fileStream.Length;  
	}

	public bool endOfFile()
	{
		return _isEOF;
	}


	public bool isExist()
	{
		return false;
	}
    
	public virtual void get(out bool value,long recordNumber)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	} 

	public virtual void get(out byte value,long recordNumber)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}

	public virtual void get(out short value,long recordNumber)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}

	public virtual void get(out char value,long recordNumber)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}

	public virtual void get(out int value, long recordNumber)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}

	public virtual void get(out long value, long recordNumber)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}

	public virtual void get(out float value,long recordNumber)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}

	public virtual void get(out double value,long recordNumber)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}
    
	public virtual void get(out DateTime value,long recordNumber)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}
    
	public virtual void get(out Decimal value,long recordNumber)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}
    
	public virtual void get(ref Array Value,long RecordNumber,bool ArrayIsDynamic,
			bool StringIsFixedLength)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}  

	public virtual void get(ref string value,long recordNumber,bool bIgnored)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	} 

	public virtual void get(ref object value,long recordNumber)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}   

	public String getFullPath()
	{
		return _fileInfo.FullName;
	}

	public FileMode getMode()
	{
		return _mode;
	}

	public long getPosition()
	{
		return _fileStream.Position;
	}
    
	public void setPosition(long pos)
	{
		_fileStream.Position = pos;
	}

	public Stream getFileStream()
	{
		return this._fileStream;
	}

	public virtual void writeLine(Object[] arr)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}

	public virtual void write(Object[] arr)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}
    
	public FileAttributes getAttributes()
	{
		return _fileInfo.Attributes;
	}
    
	public void setAttributes(FileAttributes fileAttr)
	{
		_fileInfo.Attributes = fileAttr;   
	}
    
	public virtual void printLine(Object[] arr)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}

	public virtual void print(Object[] arr)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}
    
	protected void checkLength(int len)
	{
		if(_recordLen == -1)
			return;
		if(len > _recordLen || (getPosition() + len) > (this._recordStart + this._recordLen))
			throw (IOException) ExceptionUtils.VbMakeException(VBErrors.BadRecordLen);
	}
	public virtual void put(bool value,long recordNumber)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	} 

	public virtual void put(byte value, long recordNumber)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	} 

	public virtual void put(short value, long recordNumber)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	} 

	public virtual void put( char value, long recordNumber)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	} 

	public virtual void put(int value, long recordNumber)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	} 

	public virtual void put(long value, long recordNumber)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}
    
	public virtual void put(Decimal value, long recordNumber)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	} 

	public virtual void put(float value, long recordNumber)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	} 

	public virtual void put(double value, long recordNumber)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	} 

	public virtual void put(String value,long recordNumber,bool stringIsFixedLength)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}
    
	public virtual void put(Object Value,long RecordNumber, bool ArrayIsDynamic,bool StringIsFixedLength)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}                             
 
	public virtual void put(DateTime value,long recordNumber)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}        
    
	public virtual void Input(out bool Value)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}

	public virtual void Input(out byte Value)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}

	public virtual void Input(out short Value)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}

	public virtual void Input(out int Value)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}

	public virtual void Input(out long Value)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}

	public virtual void Input(out char Value)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}

	public virtual void Input(out float Value)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}

	public virtual void Input(out double Value)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}

	public virtual void Input(out Decimal Value)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}
    
	public virtual string readLine()
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}

	public virtual void Input(out string Value)
	{
		Value = null;
		string answer = Input(Value);
		Value = answer;
		return;
	}

	public virtual void Input(ref object Value, bool isString)
	{
		if(Value == null && !isString ) {
			string answer = Input((string) Value);
			Object obj = (answer.Equals("#NULL#"))? (object)DBNull.Value : answer;
			Value = obj;
			return;
		}
		if(Value == null || Value is String) {
			String answer = Input((String) Value);
			Value = answer;
			return;
		}
		if(Value is DateTime) {
			DateTime tmp;
			Input(out tmp);
			Value = tmp;
			return;
		}

		if(Value is bool) {
			bool tmp;
			Input(out tmp);
			Value = tmp;
			return;
		}

		if(Value is byte) {
			byte res;
			Input(out res);
			Value = res;
			return;
		}
		if(Value is short) {
			short res;
			Input(out res);
			Value = res;
			return;
		}
		if(Value is char) {
			char res;
			Input(out res);
			Value = res;
			return;
		}
		if(Value is int) {
			int res;
			Input(out res);
			Value = res;
			return;
		}
		if(Value is long) {
			long res;
			Input(out res);
			Value = res;
			return;
		}
		if(Value is float) {
			float res;
			Input(out res);
			Value = res;
			return;
		}

		if(Value is double) {
			double res;
			Input(out res);
			Value = res;
			return;
		}
		if(Value is Decimal) {
			Decimal res;
			Input(out res);
			Value = res;
			return;
		}

		if(Value is DBNull) {
			String res = "";
			res = Input(res);
			if(res.StartsWith("#ERROR ")&& res.EndsWith("#")) {
				Value = IntegerType.FromString(res.Substring(7,res.Length-1));
				return;
			}
			Value = res;
			return;
		}
	}
    
	public virtual string InputString(int count)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}

	public virtual string Input(string val)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}
    
	public virtual void width(int fileNumber, int RecordWidth)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}

	public virtual void Input(out DateTime Value)
	{
		throw (IOException)ExceptionUtils.VbMakeException(VBErrors.BadFileMode);
	}
    
	protected void checkWritePermision()
	{
		if((_access != (FileAccess)OpenAccess.Write) && (_access != (FileAccess)OpenAccess.ReadWrite)) {
			string errStr = Utils.GetResourceString("FileOpenedNoWrite");
			throw (IOException) ExceptionUtils.VbMakeExceptionEx(VBErrors.PathFileAccess, errStr);
		}
	}
    
	protected void checkReadPermision()
	{
		if((_access != (FileAccess)OpenAccess.Read) && (_access != (FileAccess)OpenAccess.ReadWrite)) {
			string errStr = Utils.GetResourceString("FileOpenedNoWrite");
			throw (IOException) ExceptionUtils.VbMakeExceptionEx(VBErrors.PathFileAccess, errStr);
		}
	}
    
	public virtual bool canWrite()
	{
		return false;
	}
    
	public virtual bool canRead()
	{
		return false;  
	}
    
	public void setRecord(long recordNumber)
	{
		long tmp = 0;
		if(_recordLen == 0 || recordNumber == 0)
			return;
		if(_recordLen == -1 && recordNumber == -1)
			return;
		else if(_recordLen == -1)
			tmp = (recordNumber - 1);        
		else if(recordNumber == -1) {
			tmp = getPosition();
			if(tmp == 0) {
				_recordStart = 0;
				return;
			}
			else if((tmp % ((long) _recordLen)) == 0) {
				_recordStart = tmp;
				return;
			}
			tmp = (((long) _recordLen) * ((tmp / ((long) _recordLen)) + 1));
		}
		else if(recordNumber != 0) {
			if(_recordLen == -1)
				tmp = recordNumber;
			else 
				tmp = ((recordNumber - 1) * ((long) _recordLen));
		}
		this._fileStream.Position = tmp;
		_recordStart = tmp;
	} 
    
	public virtual bool isEndOfFile()
	{
		return (this._fileStream.Length == this._fileStream.Position);
	}   

	public virtual long seek()
	{
		throw new Exception("Temporary exception to avoid compiler cribbing");
	}
    
	public virtual void seek(long position)
	{

		throw new Exception("Temporary exception to avoid compiler cribbing");
	}

}
