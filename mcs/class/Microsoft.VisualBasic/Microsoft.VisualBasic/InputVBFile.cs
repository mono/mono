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


public class InputVBFile : BaseVBFile
{
	private StreamReader _streamReader;

	public InputVBFile (string fileName, FileAccess access, int recordLength) 
		: base(fileName, OpenMode.Input, (access == (FileAccess)OpenAccess.Default) ? OpenAccess.Read : (OpenAccess) access, recordLength)
	{
		if ((access != (FileAccess)OpenAccess.Read) && (access != (FileAccess)OpenAccess.Default))
			throw new ArgumentException(Utils.GetResourceString("FileSystem_IllegalOutputAccess"));

		this._streamReader = new StreamReader(_fileStream);
	}
    
	public override void closeFile()
	{
		if (_streamReader != null)
		{
			_streamReader.Close();
			_streamReader = null;
		}
		(this as BaseVBFile).closeFile();
	}
    
	private string readString()
	{
		StringBuilder sb = new StringBuilder("");
		int cInt = _streamReader.Read();
		char c = (char)cInt;
        
		bool inStr = false;

		while (cInt != -1 && !(c == ',' && !inStr)  && c != '\r')
		{
			if (c == '\"' && inStr)
			{
				sb.Append(c);
				if ((char)_streamReader.Peek() == ',')
					_streamReader.Read();
				break;                                                    
			}
			else if (c == '\"')
				inStr = true;            
			sb.Append(c);
			cInt = _streamReader.Read();
			c = (char)cInt;
		}
		if (c == '\r')
			_streamReader.Read();            
		if (sb.Length > 1 && sb[0]=='\"' && sb[sb.Length-1] =='\"')
		{
			sb.Remove(0, 1);
			sb.Remove(sb.Length-1, 1);
		}

		return sb.ToString(); 
	}
    
	private string readNumber()
	{

		StringBuilder sb = new StringBuilder();
		int cInt = _streamReader.Read();
		char c = (char)cInt;

		while (cInt != -1 && c != ',' && c != '\r' && c != 32) //spacebar
		{
			sb.Append(c);
			cInt = _streamReader.Read();
			c = (char)cInt;
		}

		return sb.ToString();

	}
    
	public override void Input(out bool Value)
	{
		string str = readString().Trim();

		if (str[0] == '#' && str.Length != 1)
			str = str.Substring(1, str.Length - 1);
		Object obj = str;

		// TODO
		// Value = BooleanType.FromObject(obj) ? 1 : 0);

		Value = BooleanType.FromObject(obj);
	}

	public override void Input(out byte Value)
	{
		string str = readNumber().Trim();
		Value = byte.Parse(str);
	}

	public override void Input(out short Value)
	{
		string str = readNumber().Trim();
		Value = short.Parse(str);
	}

	public override void Input(out  int Value)
	{
		string str = readNumber().Trim();
		Value = int.Parse(str);
	}

	public override void Input(out  long Value)
	{
		string str = readNumber().Trim();
		Value = long.Parse(str);
	}

	public override void Input(out  char Value)
	{
		string str = readString().Trim();
		Value = char.Parse(str);
	}

	public override void Input(out  float Value)
	{
		string str = readNumber().Trim();
		Value = float.Parse(str);
	}

	public override void Input(out  double Value)
	{
		string str = readNumber().Trim();
		Value = double.Parse(str);    
	}

	public override void Input(out  Decimal Value)
	{
		string str = readNumber().Trim();
		Value = Decimal.Parse(str);        
	}

	public override string Input(string val)
	{
		return readString().Trim();
	}
    
	public override string InputString(int count)
	{
		int i = 0;
		StringBuilder sb = new StringBuilder("");
		int current = 0;
		while (i < count)
		{
			current = this._streamReader.Peek();
			_fileStream.Position = _fileStream.Position+1;
			if (current == -1 || current == 26)
				throw (EndOfStreamException) ExceptionUtils.VbMakeException(VBErrors.EndOfFile);
			sb.Append((char)current);
			i++;            
		}
		return sb.ToString();
	}

	public override void Input(out DateTime Value)
	{
		string str = readString().Trim();
		if (str[0] == '#' && str.Length != 1)
			str = str.Substring(1, str.Length - 1);
		Value = DateTime.Parse(str);
	}
    
	public override string readLine()
	{
		return this._streamReader.ReadLine();
	}
 
	public override bool canRead()
	{
		return true;  
	}
    
	public override long seek()
	{
		return getPosition();
	}
    
	public override void seek(long position)
	{
		long nPos = position - 1;
		if (_fileStream.Length <= nPos)
			_fileStream.SetLength(nPos);
		_fileStream.Position = nPos;
	}
    
	public override bool isEndOfFile()
	{
		return (this._streamReader.Peek() == -1);
	}

}
