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
using System.Reflection;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

public class OutPutVBFile : BaseVBFile
{
	private int _currentColumn;
	private int _width = 4096;
	protected StreamWriter _streamWrite;
    
	public override bool canWrite()
	{
		return true;
	}
    
	public OutPutVBFile(String fileName, OpenMode mode, OpenAccess access,int recordLength)
		: base(fileName, mode, (access == OpenAccess.Default) ? OpenAccess.Write : access, recordLength)
	{
	
		if ((access != OpenAccess.Write) && (access != OpenAccess.Default)&&
		    !(mode == OpenMode.Append && access == OpenAccess.ReadWrite))
			throw new ArgumentException(Utils.GetResourceString("FileSystem_IllegalOutputAccess"));
		this._streamWrite = new StreamWriter (_fileStream);
	}
    
	public override void closeFile()
	{
		if (_streamWrite != null)
		{
			_streamWrite.Close();
			_streamWrite = null;
		}
		base.closeFile();
	}    

	public override void writeLine(Object[] arr)
	{
		write (arr,true);
	}
    
	public override void write(Object[] arr)
	{
		write (arr,false);
	}
    
	private void printSpc(SpcInfo spc)
	{
		StringBuilder sb = new StringBuilder();
		int count = spc.Count;
		if (_width != 0)
			count = count % _width;
		for (int i = 0; i < count; i++)
			sb.Append((char)32);
		_streamWrite.Write(sb.ToString());
		_currentColumn += count;
	}

	private void printTab(TabInfo tab)
	{
		int i;
		int j;
		int column = tab.Column;
		if (column == -1)
		{
			i = _currentColumn;
			i += 14 - i % 14;
			_streamWrite.Write(Strings.StrDup(i - _currentColumn, ' '));
			_currentColumn = i;
			return;
		}
		if (column < 1)
			column = 1;
		column--;
		i = _currentColumn;
		j = _width;
		if (j != 0 && column >= j)
			column %= j;
		if (column < i)
		{
			_streamWrite.WriteLine();
			_currentColumn = 0;
		}
		if (column > i)
		{
			_streamWrite.Write(Strings.StrDup(column - i, ' '));
			_currentColumn = i;
		}
	}
    
	private void write(Object[] output, bool eol)
	{
		UTF8Encoding UTF8 = new UTF8Encoding();

		String outStr = null;
		if (output != null && output.Length != 0 )
		{
			for (int i = 0; i < output.Length; i++)
			{
				Object tmp = output[i];
				if (tmp != null)
				{
					if (tmp is SpcInfo)
						printSpc((SpcInfo) tmp);
					else if (tmp is TabInfo)
						printTab((TabInfo) tmp);
					else
					{
						Type type = tmp.GetType();
						switch (Type.GetTypeCode(type))
						{
						case TypeCode.Boolean :
							{
								outStr = "#" + tmp.ToString() + "#";
								break;
							}
						case TypeCode.Byte :
							{
								outStr = tmp.ToString();
								break;
							}
						case TypeCode.Char :
							{
								outStr = "\"" + tmp.ToString() + "\"";
								break;
							}
						case TypeCode.DateTime :
							{
								DateTime tmpTime =(DateTime) tmp;
								if (tmpTime.Hour ==0 && 
								    tmpTime.Minute == 0 &&
								    tmpTime.Second == 0 &&
								    tmpTime.Millisecond == 0)
									outStr =
										"#"
										+ tmpTime.ToString(
												   "yyyy'-'MM'-'dd")
										+ "#";
								else
									outStr =
										"#"
										+ tmpTime.ToString(
												   "yyyy'-'MM'-'dd hh':'mm':'ss")
										+ "#";
								break;
							}
						case TypeCode.DBNull :
							{
								outStr = "#NULL#";
								break;
							}
						case TypeCode.Decimal :
							{
								outStr = tmp.ToString();
								break;
							}
						case TypeCode.Int16 :
							{
								outStr = tmp.ToString();
								break;
							}
						case TypeCode.Int32 :
							{
								outStr = tmp.ToString();
								break;
							}
						case TypeCode.Int64 :
							{
								outStr = tmp.ToString();
								break;
							}
						case TypeCode.Single :
							{
								outStr = tmp.ToString();
								break;
							}
						case TypeCode.Double :
							{
								outStr = tmp.ToString();
								break;
							}
						case TypeCode.String :
							{
								outStr = "\"" +
									(String) tmp + "\"";
								break;
							}
						default :
							throw new ArgumentException(
										    Utils.GetResourceString(
													    "Argument_UnsupportedIOType1",
													    Utils.VBFriendlyName(type)));
						}                        
						_streamWrite.Write(outStr);
						try
						{
							_currentColumn += UTF8.GetBytes(outStr).Length;
						}
						catch (Exception e)
						{
							Console.WriteLine(e.StackTrace);
						}
						if ((i == output.Length - 1) && (eol == true))
						{
							_streamWrite.WriteLine();
							_currentColumn = 0;
						}
						else
						{
							_streamWrite.Write(',');
							_currentColumn += 1;
						}
					}
				}
			}
		}
		else if (eol == true)
		{
			_streamWrite.WriteLine();
			_currentColumn = 0;
		}
		_streamWrite.Flush();
	}
    
	public override void print(Object[] output)
	{
		String outStr="";
		UTF8Encoding UTF8 = new UTF8Encoding();

		if (output != null)
		{
			for (int i = 0; i < output.Length; i++)
			{
				Object tmp = output[i];
				if (tmp is SpcInfo)
					printSpc((SpcInfo)tmp);
				else if (tmp is TabInfo)
					printTab((TabInfo)tmp);
				else
				{
					Type type = tmp.GetType();
					switch (Type.GetTypeCode(type))
					{
					case TypeCode.Boolean :
						{
							outStr = tmp.ToString();
							break;
						}
					case TypeCode.Byte :
						{
							outStr = AddSpaces(tmp.ToString());
							break;
						}
					case TypeCode.Char :
						{
							outStr = tmp.ToString();
							break;
						}
					case TypeCode.DateTime :
						{
							outStr = StringType.FromDate((DateTime)tmp) + " ";
							break;
						}
					case TypeCode.DBNull :
						{
							outStr = "Null";
							break;
						}
					case TypeCode.Decimal :
						{
							outStr = AddSpaces(tmp.ToString());
							break;
						}
					case TypeCode.Int16 :
						{
							outStr = AddSpaces(tmp.ToString());
							break;
						}
					case TypeCode.Int32 :
						{
							outStr = AddSpaces(tmp.ToString());
							break;
						}
					case TypeCode.Int64 :
						{
							outStr = AddSpaces(tmp.ToString());
							break;
						}
					case TypeCode.Single :
						{
							outStr = AddSpaces(tmp.ToString());
							break;
						}
					case TypeCode.Double :
						{
							outStr = AddSpaces(tmp.ToString());
							break;
						}
					case TypeCode.String :
						{
							outStr = tmp.ToString();
							break;
						}
					default :
						throw new ArgumentException(
									    Utils.GetResourceString(
												    "Argument_UnsupportedIOType1",
												    Utils.VBFriendlyName(type)));
					}
					if (_currentColumn + outStr.Length > _width)
					{
						if (outStr.Length == 1)
							_streamWrite.WriteLine();
						else
						{
							int numOfChars =
								_currentColumn + outStr.Length - _width;
							_streamWrite.WriteLine(outStr.Substring(0,outStr.Length - numOfChars));
							outStr = outStr.Substring(outStr.Length - numOfChars);
						}
						_currentColumn = 0;
					}
					if (i != output.Length-1 && outStr.Length < 14)
						outStr+= Strings.Space(14-outStr.Length); 
					_streamWrite.Write(outStr);                    
					try
					{
						_currentColumn += UTF8.GetBytes(outStr).Length;
					}
					catch (Exception e)
					{
						Console.WriteLine(e.StackTrace);
					}
				}
			}
		}
	}

	private static String AddSpaces(String s)
	{
		String str2 = "-";

		if (s[0] == str2[0])
			return s + " ";

		return " " + s + " ";
	}

	/**
	 * Writes display-formatted data to a sequential file.
	 * @param fileNumber Any valid file number.
	 */
	public void printLine()
	{
		_streamWrite.WriteLine();
		_currentColumn = 0;
	}

	/**
	 * Writes display-formatted data to a sequential file.
	 * @param fileNumber Any valid file number.
	 * @param output One or more comma-delimited expressions to write to a file.
	 */
	public override void printLine( Object[] output)
	{
		print(output);
		if (this._currentColumn != 0 && output != null && output.Length != 0)
			printLine();
		else if (output == null || output.Length == 0)
			printLine();
		else if (output.Length == 1 && output[0] is String && ((String)output[0]).Length ==0 )
			printLine();      
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
		_streamWrite.BaseStream.Position = nPos;       
	}
    
	public override bool isEndOfFile()
	{
		return (this._fileStream.Length == this._fileStream.Position);
	}
    
	public override void width(int fileNumber, int RecordWidth)
	{
		if (RecordWidth < 0 || RecordWidth > 255)
			throw (ArgumentException) ExceptionUtils.VbMakeException( VBErrors.IllegalFuncCall);
		_width = RecordWidth;
	}
}
