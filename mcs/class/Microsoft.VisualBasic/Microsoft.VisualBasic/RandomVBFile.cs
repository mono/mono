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
using System.Text;
using System.IO;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;


/**
 * @author along
 */
public class RandomVBFile : BaseVBFile
{
	protected BinaryWriter _binaryWrite;
	protected BinaryReader _binaryReader;
	protected Encoding _encoding;
	private int _width;
    
	public RandomVBFile(String fileName, OpenMode mode, OpenAccess access,int recordLength)
		: base(fileName, mode, 
		       (access == OpenAccess.Default) ? OpenAccess.ReadWrite : (OpenAccess) access, recordLength)
	{
		
		if (access != OpenAccess.Read)    
			_binaryWrite = new BinaryWriter(_fileStream);
		if (access != OpenAccess.Write) 
			_binaryReader = new BinaryReader(_fileStream);
		_encoding = new UTF8Encoding(false, true);
	}
    
	protected bool getBoolean(long recordNumber,bool isSetRecord)
	{
		if (isSetRecord)
			setRecord(recordNumber);
		bool tmp = _binaryReader.ReadBoolean();
		return tmp;
	}
	/* (non-Javadoc)
	 * @see Microsoft.VisualBasic.VBFile#getChar(long)
	 */
	protected char getChar(long recordNumber,bool isSetRecord)
	{
		if (isSetRecord)
			setRecord(recordNumber);
		char tmp = _binaryReader.ReadChar();
		return tmp;        
	}
	/* (non-Javadoc)
	 * @see Microsoft.VisualBasic.VBFile#getByte(long)
	 */
	protected byte getByte(long recordNumber,bool isSetRecord)
	{
		if (isSetRecord)
			setRecord(recordNumber);
		int tmp = _binaryReader.ReadByte();
		return (byte)tmp; 
	}
	/* (non-Javadoc)
	 * @see Microsoft.VisualBasic.VBFile#getShort(long)
	 */
	protected short getShort(long recordNumber,bool isSetRecord)
	{
		if (isSetRecord)
			setRecord(recordNumber);
		short tmp = _binaryReader.ReadInt16();
		return tmp;
	}
	/* (non-Javadoc)
	 * @see Microsoft.VisualBasic.VBFile#getInt(long)
	 */
	protected int getInt(long recordNumber,bool isSetRecord)
	{
		if (isSetRecord)
			setRecord(recordNumber);
		int tmp = _binaryReader.ReadInt32();
		return tmp;
	}
	/* (non-Javadoc)
	 * @see Microsoft.VisualBasic.VBFile#getLong(long)
	 */
	protected long getLong(long recordNumber,bool isSetRecord)
	{
		if (isSetRecord)
			setRecord(recordNumber);
		long tmp = _binaryReader.ReadInt64();
		return tmp;
	}
	/* (non-Javadoc)
	 * @see Microsoft.VisualBasic.VBFile#getFloat(long)
	 */
	protected float getFloat(long recordNumber,bool isSetRecord)
	{
		if (isSetRecord)
			setRecord(recordNumber);
		float tmp = _binaryReader.ReadSingle();
		return tmp;
	}
	/* (non-Javadoc)
	 * @see Microsoft.VisualBasic.VBFile#getDouble(long)
	 */
	protected double getDouble(long recordNumber,bool isSetRecord)
	{
		if (isSetRecord)
			setRecord(recordNumber);
		double tmp = _binaryReader.ReadDouble();
		return tmp;
	}

	protected Decimal getDecimal(long recordNumber,bool isSetRecord)
	{
		if (isSetRecord)
			setRecord(recordNumber);
		Decimal tmp = _binaryReader.ReadDecimal();
		return tmp;
	}

	protected DateTime getDateTime(long recordNumber,bool isSetRecord)
	{
		if (isSetRecord)
			setRecord(recordNumber);

		double tmp = _binaryReader.ReadDouble();       
		return DateTime.FromOADate(tmp);
	}
   
	public override void get(out bool value,long recordNumber)
	{
		checkReadPermision();
		value = getBoolean(recordNumber,true)? true : false;        
	} 

	public override void get(out byte value,long recordNumber)
	{
		checkReadPermision();
		value = getByte(recordNumber,true);
	}

	public override void get(out short value,long recordNumber)
	{
		checkReadPermision();
		value = getShort(recordNumber,true);
	}

	public override void get(out char value,long recordNumber)
	{
		checkReadPermision();
		value = getChar(recordNumber,true);
	}

	public override void get(out int value, long recordNumber)
	{
		checkReadPermision();
		value = getInt(recordNumber,true);
	}

	public override void get(out long value, long recordNumber)
	{
		checkReadPermision();
		value = getLong(recordNumber,true);
	}

	public override void get(out float value,long recordNumber)
	{
		checkReadPermision();
		value = getFloat(recordNumber,true);
	}

	public override void get(out double value,long recordNumber)
	{
		checkReadPermision();
		value = getDouble(recordNumber,true);
	}
    
	public override void get(out DateTime value,long recordNumber)
	{
		checkReadPermision();
		value = getDateTime(recordNumber,true);
	}
    
	public override void get(out Decimal value,long recordNumber)
	{
		checkReadPermision();
		Decimal tmp = getDecimal(recordNumber,true);       
		value = tmp;
	}

	public override void get(ref string str, long recordNumber, bool bIgnored)
	{
		checkReadPermision();
		int strLen = 0;
		if (str == null || str.Length == 0)
			strLen = _binaryReader.ReadInt16();
		else
			strLen = str.Length;   
		str = new String(_binaryReader.ReadChars(strLen));
	} 

	public override void get(ref object value,long recordNumber)
	{
		checkReadPermision();
		if (value.GetType().IsArray) {
			// need to figure out how to convert from Object& to Array &
			// get(value,recordNumber,false,false); 
			throw new NotImplementedException();
		}
		else
		{        
			setRecord(recordNumber);           
			value = _binaryReader.ReadString();
		}        
	}
    
	public override void get(ref Array value, long recordNum, bool arrIsDynamic, bool strIsFixedLen)
	{
		checkReadPermision();
		Object arr = value;

		Type type = arr.GetType().GetElementType();
		int  rank = (arr as Array).Rank;
		if (rank == 0 || rank > 2)
			throw new ArgumentException(Utils.GetResourceString("Argument_UnsupportedArrayDimensions"));

		if (getPosition() >= getLength())
		{
			return;
		}
		base.setRecord(recordNum);
		int strLen = 0;
		Object obj;
		if (strIsFixedLen && (type == typeof(string)))
		{
			if (rank == 1)
				obj = (arr as Array).GetValue(0);
			else             
				obj = (arr as Array).GetValue(0, 0);
			if (obj != null)
				strLen = ((String) obj).Length;
			if (strLen == 0)
				throw new ArgumentException(
							    Utils.GetResourceString(
										    "Argument_InvalidFixedLengthString"));            
		}      
		int len1 = (arr as Array).GetLength(0);
		int len2 = (rank == 2) ? (arr as Array).GetLength(1):0;
		String val = "";

		if (rank == 1)
		{
			for (int i = 0 ; i < len1 ; i++)
			{
				if (strIsFixedLen)
					val = new String(_binaryReader.ReadChars(strLen));                
				else
				{                    
					strLen = _binaryReader.ReadInt16();
					val = new String(_binaryReader.ReadChars(strLen));
				}
				(arr as Array).SetValue(val,i);                                
			}
		}
		else
		{
			for (int i = 0 ; i < len1 ; i++)
			{
				for (int j = 0 ; j < len2 ; j++)
				{                
					val = this._binaryReader.ReadString();                
					if (strIsFixedLen && val.Length != strLen)
						throw new ArgumentException(
									    Utils.GetResourceString(
												    "Argument_InvalidFixedLengthString"));
					(arr as Array).SetValue(val,i,j);
				}                                 
			}
		}       
	}     

    
	public override void put(bool val, long recordNumber)
	{
		checkWritePermision();
		putBoolean(val,recordNumber,true);        
	}
	/* (non-Javadoc)
	 * @see Microsoft.VisualBasic.VBFile#putChar(char, long)
	 */
	public override void put(byte value, long recordNumber)
	{
		checkWritePermision();
		putByte(value,recordNumber,true);   
	}

	public override void put(short value, long recordNumber)
	{
		checkWritePermision();
		putShort(value,recordNumber,true); 
	}
    

	public override void put( char value, long recordNumber)
	{
		checkWritePermision();
		putChar(value,recordNumber,true); 
	}
    

	public override void put(int value, long recordNumber)
	{
		checkWritePermision();
		putInt(value,recordNumber,true); 
	}

	public override void put(long value, long recordNumber)
	{
		checkWritePermision();
		putLong(value,recordNumber,true); 
	}

	public override void put(float value, long recordNumber)
	{
		checkWritePermision();
		putSingle(value,recordNumber,true); 
	}

	public override void put(double value, long recordNumber)
	{
		checkWritePermision();
		putDouble(value,recordNumber,true); 
	}
    
	public override void put(Decimal value, long recordNumber)
	{
		checkWritePermision();
		putDecimal(value,recordNumber,true); 
	} 

	public override void put(String value,long recordNumber,bool stringIsFixedLength)
	{
		checkWritePermision();
		if (value.IndexOf('\"') == -1)
			putString(value,recordNumber,stringIsFixedLength,true);
		else
		{
			for (int i = 0 ; i < value.Length;i++)
				_binaryWrite.Write(value[i]); 
                    
		}
	}
    
	public override void put(Object arr,long recordNum,
				 bool arrIsDynamic,bool strIsFixedLen)
	{
		checkWritePermision();
		if (arr == null)
		{
			throw new ArgumentException(
						    Utils.GetResourceString("Argument_ArrayNotInitialized"));
		}
		Type type = arr.GetType().GetElementType();
		int  rank = (arr as Array).Rank;
		if (rank == 0 || rank > 2)
			throw new ArgumentException(
						    Utils.GetResourceString("Argument_UnsupportedArrayDimensions"));
		base.setRecord(recordNum);
		int strLen = 0;
		Object obj;             
		if (strIsFixedLen && (type == typeof(string)))
		{
			if (rank == 1)
				obj = (arr as Array).GetValue(0);
			else             
				obj = (arr as Array).GetValue(0, 0);
			if (obj != null)
				strLen = ((String) obj).Length;
			if (strLen == 0)
				throw new ArgumentException(
							    Utils.GetResourceString(
										    "Argument_InvalidFixedLengthString"));            
		}      
		int len1 = (arr as Array).GetLength(0);
		int len2 = (rank == 2) ? (arr as Array).GetLength(1) : 0;
		if (rank == 1)
		{
			for (int i = 0 ; i < len1 ; i++)
			{                
				putObject((arr as Array).GetValue(i), recordNum , type,strIsFixedLen);                                 
			}
		}
		else
		{
			for (int i = 0 ; i < len1 ; i++)
			{
				for (int j = 0 ; j < len2 ; j++)
				{                
					putObject((arr as Array).GetValue(i), recordNum , type,strIsFixedLen);                                 
				}                                 
			}
		}           
	}
    
	private void putObject(Object obj ,long recordNumber ,Type type,bool strIsFixedLen)
	{
		if (obj is string)
			putString((string)obj,recordNumber,strIsFixedLen,false);
		else if (obj is bool)
			putBoolean((bool)obj, recordNumber,false);
		else if (obj is char)
			putChar((char)obj, recordNumber,false);
		else if (obj is byte)
			putByte((byte)obj, recordNumber,false);
		else if (obj is short)
			putShort((short)obj,recordNumber,false);
		else if (obj is int)
			putInt((int)obj, recordNumber,false);
		else if (obj is long)
			putLong((long)obj, recordNumber,false);
		else if (obj is double)
			putDouble((double)obj,recordNumber,false);
		else if (obj is float)
			putSingle((float)obj, recordNumber,false);
		else if (obj is DateTime)
			putDatetime((DateTime)obj,recordNumber,false);        
          
	}

	public override void put(DateTime value,long recordNumber)
	{
		checkWritePermision();
		putDatetime(value,recordNumber,true);          
	}
    
	protected void putBoolean(bool val, long recordNumber,bool isSetRecord)
	{
		if (isSetRecord)
			setRecord(recordNumber);
		checkLength(2);
		if (val)
			_binaryWrite.Write(val);
		else
			_binaryWrite.Write(val);
		//  setPosition(getPosition()+2);
	}
	/* (non-Javadoc)
	 * @see Microsoft.VisualBasic.VBFile#putChar(char, long)
	 */
	protected void putChar(char val, long recordNumber,bool isSetRecord)
	{
		if (isSetRecord)
			setRecord(recordNumber);
		this.checkLength(2);
		_binaryWrite.Write(val);        
		//  setPosition(getPosition()+2);
	}
    
	/* (non-Javadoc)
	 * @see Microsoft.VisualBasic.VBFile#putShort(short, long)
	 */
	protected void putShort(short val, long recordNumber,bool isSetRecord)
	{
		if (isSetRecord)
			setRecord(recordNumber);
		this.checkLength(2);
		_binaryWrite.Write(val);        
		//  setPosition(getPosition()+2);
	}
    
	protected void putByte(byte val, long recordNumber,bool isSetRecord)
	{
		if (isSetRecord)
			setRecord(recordNumber);
		this.checkLength(2);
		_binaryWrite.Write(val);        
		//   setPosition(getPosition()+2);
	}
	/* (non-Javadoc)
	 * @see Microsoft.VisualBasic.VBFile#putInt(int, long)
	 */
	protected void putInt(int val, long recordNumber,bool isSetRecord)
	{
		if (isSetRecord)
			setRecord(recordNumber);
		this.checkLength(4);
		_binaryWrite.Write(val);        
		//  setPosition(getPosition()+4);
	}
	/* (non-Javadoc)
	 * @see Microsoft.VisualBasic.VBFile#putLong(long, long)
	 */
	protected void putLong(long val, long recordNumber,bool isSetRecord)
	{
		if (isSetRecord)
			setRecord(recordNumber);
		this.checkLength(8);
		_binaryWrite.Write(val);        
		//  setPosition(getPosition()+8);
	}
	/* (non-Javadoc)
	 * @see Microsoft.VisualBasic.VBFile#putFloat(float, long)
	 */
	protected void putSingle(float val, long recordNumber,bool isSetRecord)
	{
		if (isSetRecord)
			setRecord(recordNumber);
		this.checkLength(4);
		_binaryWrite.Write(val);        
		// setPosition(getPosition()+4);
	}
	/* (non-Javadoc)
	 * @see Microsoft.VisualBasic.VBFile#putDouble(double, long)
	 */
	protected void putDouble(double val, long recordNumber,bool isSetRecord)
	{
		if (isSetRecord)
			setRecord(recordNumber);
		this.checkLength(8);
		_binaryWrite.Write(val);        
		//  setPosition(getPosition()+8);
	}
    
	protected void putDecimal(Decimal val, long recordNumber,bool isSetRecord)
	{
		if (isSetRecord)
			setRecord(recordNumber);
		this.checkLength(8);
		_binaryWrite.Write(val);        
		//  setPosition(getPosition()+8);
	}
    
	protected void putDatetime(DateTime val, long recordNumber,bool isSetRecord)
	{
		if (isSetRecord)
			setRecord(recordNumber);
		this.checkLength(8);
		_binaryWrite.Write(val.ToOADate());        
		//  setPosition(getPosition()+8);
	}  
    
	public void putString(
			      String value,
			      long recordNumber,
			      bool stringIsFixedLength,bool isSetRecord)
	{
		if (value == null)
			value = "";
		int byteCount = _encoding.GetByteCount(value);
		if (isSetRecord)
			setRecord(recordNumber);
		if (stringIsFixedLength)
			checkLength(byteCount);
		else
		{
			checkLength(byteCount+2);
			_binaryWrite.Write((short)byteCount);
		}
		if (byteCount != 0)
			for (int i = 0 ; i < byteCount ; i++)
				_binaryWrite.Write(value[i]);
		//  setPosition(getPosition()+((stringIsFixedLength) ? byteCount :byteCount+2));
	}
    
	public override long seek()
	{
		if (_recordLen == 0)
			throw ExceptionUtils.VbMakeException(VBErrors.InternalError);
		return ((getPosition() + _recordLen - 1) / _recordLen);
	}
    
	public override void seek(long position)
	{
		_fileStream.Position = position;
	}
    
	public override bool isEndOfFile()
	{
		return (this._fileStream.Length == this._fileStream.Position);
	}
    
	public override void width(int fileNumber, int RecordWidth)
	{
		if (RecordWidth < 0 || RecordWidth > 255)
			throw (ArgumentException) ExceptionUtils.VbMakeException(
										 VBErrors.IllegalFuncCall);
		_width = RecordWidth;
	}    
   
}
