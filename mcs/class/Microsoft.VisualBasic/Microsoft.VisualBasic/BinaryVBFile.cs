/*
 * Copyright (c) 2002-2003 Mainsoft Corporation.
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

using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

public class BinaryVBFile : RandomVBFile
{
	public BinaryVBFile(String fileName, OpenMode mode, OpenAccess access, int recordLength) : 
		base(fileName,mode,access,recordLength)
	{
		
	}   
  
	public override bool canWrite()
	{
		return true;
	}
    
	public override bool canRead()
	{
		return true;  
	}
    
	private String readString()
	{
		StringBuilder sb = new StringBuilder("");
		int cInt = _binaryReader.Read();
		char c = (char) cInt;
		bool inStr = false;
		while (cInt != -1 && !(c == ',' && !inStr) && c != '\r') //spacebar
		{
			if (c == '\"' && inStr)
			{
				sb.Append(c);
				if ((char) _binaryReader.PeekChar() == ',')
					_binaryReader.Read();
				break;
			}
			else if (c == '\"')
				inStr = true;
			sb.Append(c);
			cInt = _binaryReader.Read();
			c = (char) cInt;
		}
		if (c == '\r')
			_binaryReader.Read();
		if (sb.Length > 1
		    && sb[0] == '\"'
		    && sb[sb.Length - 1] == '\"')
		{
			sb.Remove(0, 1);
			sb.Remove(sb.Length - 1, 1);
		}
		return sb.ToString();
	}
    
	private String readNumber()
	{
		StringBuilder sb = new StringBuilder();
		int cInt = _binaryReader.Read();
		char c = (char) cInt;
		while (cInt != -1 && c != ',' && c != '\r' && c != 32) //spacebar
		{
			sb.Append(c);
			cInt = _binaryReader.Read();
			c = (char) cInt;
		}
		return sb.ToString();
	}
    
	public override void Input(out bool Value)
	{
		String str = readString().Trim();

		if (str[0] == '#' && str.Length != 1)
			str = str.Substring(1, str.Length - 1);
		Object obj = str;
		// Temporary
		Value = BooleanType.FromObject(obj);
	}

	public override void Input(out byte Value)
	{
		char val = this._binaryReader.ReadChar();
		while (val == 10 || val == 32)
			val = this._binaryReader.ReadChar();
		String str = readString().Trim(); 
		Value = ByteType.FromString(val+str);
	}

	public override void Input(out short Value)
	{
		char val = this._binaryReader.ReadChar();
		while (val == 10 || val == 32)
			val = this._binaryReader.ReadChar();
		String str = readString().Trim(); 
		Value = ShortType.FromString(val+str); 
	}

	public override void Input(out int Value)
	{
		char val = this._binaryReader.ReadChar();
		while (val == 10 || val == 32)
			val = this._binaryReader.ReadChar();
		String str = readString().Trim(); 
		Value = IntegerType.FromString(val+str);
	}

	public override void Input(out long Value)
	{
		char val = this._binaryReader.ReadChar();
		while (val == 10 || val == 32)
			val = this._binaryReader.ReadChar();
		String str = readString().Trim(); 
		Value = LongType.FromString(val+str);
	}

	public override void Input(out char Value)
	{
		char val = this._binaryReader.ReadChar();
		while (val == 10 || val == 32)
			val = this._binaryReader.ReadChar();
		String str = readString().Trim(); 
		Value = CharType.FromString(val+str);
	}

	public override void Input(out float Value)
	{
		char val = this._binaryReader.ReadChar();
		while (val == 10 || val == 32)
			val = this._binaryReader.ReadChar();
		String str = readString().Trim(); 
		Value = SingleType.FromString(val+str);
	}

	public override void Input(out double Value)
	{
		char val = this._binaryReader.ReadChar();
		while (val == 10 || val == 32)
			val = this._binaryReader.ReadChar();
		String str = readString().Trim(); 
		Value = DoubleType.FromString(val+str);
	}

	public override void Input(out Decimal Value)
	{
		char val = (char)this._binaryReader.PeekChar();
		while (val == 10 || val == 32)
		{
			this._binaryReader.ReadChar();
			val = (char)this._binaryReader.PeekChar();
		}
		String str = readString().Trim(); 
		Value = DecimalType.FromString(str);        
	}

	public override string Input(string val)
	{
		return readString()/*.Trim()*/;
	}

	public DateTime Input( DateTime Value)
	{
		String str = readString().Trim();
		if (str[0] == '#' && str.Length != 1)
			str = str.Substring(1, str.Length - 1);
		return DateTime.Parse(str);
	}
    
	public  override string InputString(int count)
	{
		int i = 0;
		StringBuilder sb = new StringBuilder("");
		int current = 0;
		while (i < count)
		{
			current = this._binaryReader.Read();
			if (current == -1 || current == 26)
				throw (EndOfStreamException) ExceptionUtils.VbMakeException(VBErrors.EndOfFile);
			sb.Append((char)current);
			i++;            
		}
		return sb.ToString();
	}

    
	public override long seek()
	{
		return getPosition() + 1;
	}
    
	public override void seek(long position)
	{
		if (position <= 0)
			throw (IOException) ExceptionUtils.VbMakeException(
									   VBErrors.BadRecordNum);
		setPosition(position - 1);
	}
    
	public override string readLine()
	{
		return readString();
	}
}
