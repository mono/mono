/*
 * JISConvert.cs - Implementation of the "System.Text.JISConvert" class.
 *
 * Copyright (c) 2002  Southern Storm Software, Pty Ltd
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR
 * OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
 * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */

namespace I18N.CJK
{

using System;

// This class assists other encoding classes in converting back
// and forth between JIS character sets and Unicode.  It uses
// several large tables to do this, some of which are stored in
// the resource section of the assembly for efficient access.

internal unsafe sealed class JISConvert
{
	// Table identifiers.
	private const int JISX0208_To_Unicode = 1;
	private const int JISX0212_To_Unicode = 2;
	private const int CJK_To_JIS          = 3;
	private const int Greek_To_JIS        = 4;
	private const int Extra_To_JIS        = 5;

	// Public access to the conversion tables.
#if __PNET__
	public byte *jisx0208ToUnicode;
	public byte *jisx0212ToUnicode;
	public byte *cjkToJis;
	public byte *greekToJis;
	public byte *extraToJis;
#else
	public byte[] jisx0208ToUnicode;
	public byte[] jisx0212ToUnicode;
	public byte[] cjkToJis;
	public byte[] greekToJis;
	public byte[] extraToJis;
#endif

	// Constructor.
	private JISConvert()
			{
				// Load the conversion tables.
				CodeTable table = new CodeTable("jis.table");
				jisx0208ToUnicode = table.GetSection(JISX0208_To_Unicode);
				jisx0212ToUnicode = table.GetSection(JISX0212_To_Unicode);
				cjkToJis = table.GetSection(CJK_To_JIS);
				greekToJis = table.GetSection(Greek_To_JIS);
				extraToJis = table.GetSection(Extra_To_JIS);
				table.Dispose();
			}

	// The one and only JIS conversion object in the system.
	private static JISConvert convert;
	static readonly object lockobj = new object ();
	// Get the primary JIS conversion object.
	public static JISConvert Convert
			{
				get
				{
					lock(lockobj)
					{
						if(convert != null)
						{
							return convert;
						}
						else
						{
							convert = new JISConvert();
							return convert;
						}
					}
				}
			}

}; // class JISConvert

}; // namespace I18N.CJK
