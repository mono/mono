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

using Microsoft.VisualBasic;

using System;
using System.IO;


interface VBFile
{
	void closeFile(); 
   
	bool  endOfFile();
    
	bool  isExist();
    
	void get(out bool value,long recordNumber); 

	void get(out byte value,long recordNumber);

	void get(out short value,long recordNumber);

	void get(out char value,long recordNumber);

	void get(out int value, long recordNumber);

	void get(out long value, long recordNumber);

	void get(out float value,long recordNumber);

	void get(out double value,long recordNumber);
    
	void get(out Decimal value,long recordNumber);
    
	void get(out DateTime value,long recordNumber);
    
	void get(ref Array Value,long RecordNumber,bool  ArrayIsDynamic,
		 bool  StringIsFixedLength); 

	void get(ref string value,long recordNumber,bool  bIgnored); 

	void get(ref object value,long recordNumber);    
  
	string getFullPath();
   
	FileMode getMode();
    
	long getPosition();
    
	void setPosition(long pos);
    
	long getLength();
    
	Stream getFileStream();  
   
	void writeLine(Object[] arr);
    
	void write(Object[] arr);
    
	void printLine(Object[] arr);
    
	void print(Object[] arr);    
   
	void put(bool  value,long recordNumber);

	void put(byte value, long recordNumber);

	void put(short value, long recordNumber);

	void put( char value, long recordNumber);

	void put(int value, long recordNumber);

	void put(long value, long recordNumber);

	void put(float value, long recordNumber);

	void put(double value, long recordNumber);
    
	void put(Decimal value, long recordNumber);

	void put(string value,long recordNumber,bool  stringIsFixedLength);
    
	void put(Object Value,long RecordNumber,
		 bool  ArrayIsDynamic,bool  StringIsFixedLength); 

	void put(DateTime value,long recordNumber);
    
    
	bool  canWrite();
    
	bool  canRead();
    
	void Input(out bool Value);

	void Input(out byte Value);

	void Input(out short Value);

	void Input(out int Value);

	void Input(out long Value);

	void Input(out char Value);

	void Input(out float Value);

	void Input(out double Value);

	void Input(out Decimal Value);

	void Input(ref object Value, bool  isString);

	void Input(out string Value);
    
	string InputString(int count);

	string Input(string val);

	void Input(out DateTime Value);
    
	FileAttributes getAttributes();
    
	void setAttributes(FileAttributes fileAttr);
    
	long seek();
    
	void seek(long position);
    
	bool  isEndOfFile();
    
	string readLine();
    
	void width(int fileNumber, int RecordWidth);
     
       
}
