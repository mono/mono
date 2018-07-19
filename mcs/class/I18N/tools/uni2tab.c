/*
 * uni2tab.c - Convert Unicode data files into CJK conversion tables.
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

/*
 *
 * Usage: uni2tab
 *
 * Required files from ftp.unicode.org: Unihan.txt, CP932.TXT
 *
 * Unihan.txt and CP932.TXT can be found at:
 * ftp://www.unicode.org/Public/5.0.0/ucd/Unihan.txt
 * ftp://ftp.unicode.org/Public/MAPPINGS/VENDORS/MICSFT/WINDOWS/CP932.TXT
 */

#include <stdio.h>
#include <string.h>
#include <stdlib.h>

/*
 * Forward declarations.
 */
static void convertLine(char *buf);
static void convertSJISLine(char *buf);
static int createTables(void);

int main(int argc, char *argv[])
{
	FILE *file;
	char buffer[BUFSIZ];
	int error;

	/* Load the relevant contents from the Unihan.txt file */
	if((file = fopen("Unihan.txt", "r")) == NULL)
	{
		perror("Unihan.txt");
		return 1;
	}
	while(fgets(buffer, sizeof(buffer), file))
	{
		if(buffer[0] == 'U' && buffer[1] == '+')
		{
			convertLine(buffer + 2);
		}
	}
	fclose(file);

	/* Load the relevant contents from the CP932.TXT file,
	   to get mappings for non-CJK characters */
	if((file = fopen("CP932.TXT", "r")) == NULL)
	{
		perror("CP932.TXT");
		return 1;
	}
	while(fgets(buffer, sizeof(buffer), file))
	{
		if(buffer[0] == '0' && buffer[1] == 'x')
		{
			convertSJISLine(buffer + 2);
		}
	}
	fclose(file);

	/* Create the output tables */
	error = createTables();

	/* Clean up and exit */
	return error;
}

/*
 * Parse a hexadecimal value.  Returns the length
 * of the value that was parsed.
 */
static int parseHex(const char *buf, unsigned long *value)
{
	int len = 0;
	char ch;
	*value = 0;
	while((ch = buf[len]) != '\0')
	{
		if(ch >= '0' && ch <= '9')
		{
			*value = *value * 16 + (unsigned long)(ch - '0');
		}
		else if(ch >= 'A' && ch <= 'F')
		{
			*value = *value * 16 + (unsigned long)(ch - 'A' + 10);
		}
		else if(ch >= 'a' && ch <= 'f')
		{
			*value = *value * 16 + (unsigned long)(ch - 'a' + 10);
		}
		else
		{
			break;
		}
		++len;
	}
	return len;
}

/*
 * Parse "ku" and "ten" values from a buffer.
 */
static void parseKuTen(const char *buf, int *ku, int *ten)
{
	int value = 0;
	while(*buf >= '0' && *buf <= '9')
	{
		value = value * 10 + (*buf++ - '0');
	}
	*ku = value / 100;
	*ten = value % 100;
}

/*
 * Tables.
 */
static unsigned short jisx0208ToUnicode[94*94];
static unsigned short jisx0212ToUnicode[94*94];
static unsigned short unicodeToJis[65536];
static unsigned short greekToJis[0x451 - 0x0391 + 1];
static unsigned short extraToJis[0xFFEF - 0xFF01 + 1];
static unsigned long  lowJis  = 0xFFFF;
static unsigned long  highJis = 0x0000;

/*
 * Process a JIS X 0208 sequence by ku and ten values.
 */
static void processJis0208(unsigned long code, int ku, int ten)
{
	int offset = (ku - 1) * 94 + (ten - 1);
	jisx0208ToUnicode[offset] = (unsigned short)code;
	unicodeToJis[code] = (unsigned short)(offset + 0x0100);
	if(code < lowJis)
	{
		lowJis = code;
	}
	if(code > highJis)
	{
		highJis = code;
	}
}

/*
 * Process a JIS X 0212 sequence by ku and ten values.
 */
static void processJis0212(unsigned long code, int ku, int ten)
{
	int offset = (ku - 1) * 94 + (ten - 1);
	jisx0212ToUnicode[offset] = (unsigned short)code;
	unicodeToJis[code] = (unsigned short)(offset + 0x8000);
	if(code < lowJis)
	{
		lowJis = code;
	}
	if(code > highJis)
	{
		highJis = code;
	}
}

/*
 * Convert an input line into table entries.
 */
static void convertLine(char *buf)
{
	unsigned long code;
	const char *key;
	int ku, ten;

	/* Parse the hex name of the Unicode character */
	buf += parseHex(buf, &code);
	if(code >= 0x10000)
	{
		/* Cannot handle surrogate-based CJK characters yet */
		return;
	}

	/* Skip to the key name */
	while(*buf != '\0' && *buf != 'k')
	{
		++buf;
	}
	if(*buf == '\0')
	{
		return;
	}

	/* Extract the key name from the buffer */
	key = buf;
	while(*buf != '\0' && *buf != ' ' && *buf != '\t')
	{
		++buf;
	}
	if(*buf == '\0')
	{
		return;
	}
	*buf++ = '\0';

	/* Skip to the value field */
	while(*buf != '\0' && (*buf == ' ' || *buf == '\t' ||
						   *buf == '\r' || *buf == '\n'))
	{
		++buf;
	}
	if(*buf == '\0')
	{
		return;
	}

	/* Determine what to do based on the key */
	if(!strcmp(key, "kJis0"))
	{
		parseKuTen(buf, &ku, &ten);
		processJis0208(code, ku, ten);
	}
	else if(!strcmp(key, "kJis1"))
	{
		parseKuTen(buf, &ku, &ten);
		processJis0212(code, ku, ten);
	}
}

/*
 * Convert a line from the "CP932.TXT" file.
 */
static void convertSJISLine(char *buf)
{
	unsigned long sjis;
	unsigned long code;
	int ch1, ch2;
	int offset;

	/* Read the Shift-JIS code point */
	buf += parseHex(buf, &sjis);
	if(sjis < 0x8000)
	{
		return;
	}
	while(*buf != '\0' && (*buf == ' ' || *buf == '\t' ||
						   *buf == '\r' || *buf == '\n'))
	{
		++buf;
	}
	if(*buf != '0' || buf[1] != 'x')
	{
		return;
	}
	buf += 2;

	/* Read the Unicode code point */
	buf += parseHex(buf, &code);

	/* Convert the Shift-JIS code point into a JIS kuten value */
	ch1 = (int)(sjis >> 8);
	ch2 = (int)(sjis & 0xFF);
	if(ch1 >= 0x81 && ch1 <= 0x9F)
	{
		offset = (ch1 - 0x81) * 0xBC;
	}
	else if(ch1 >= 0xE0 && ch1 <= 0xEF)
	{
		offset = (ch1 - 0xE0 + (0xA0 - 0x81)) * 0xBC;
	}
	else
	{
		/* Invalid first byte */
		return;
	}
	if(ch2 >= 0x40 && ch2 <= 0x7E)
	{
		offset += (ch2 - 0x40);
	}
	else if(ch2 >= 0x80 && ch2 <= 0xFC)
	{
		offset += (ch2 - 0x80 + 0x3F);
	}
	else
	{
		/* Invalid second byte */
		return;
	}

	/* Process the kuten value */
	if(code >= 0x0391 && code <= 0x0451)
	{
		/* Greek subset */
		greekToJis[code - 0x0391] = (unsigned short)(offset + 0x0100);
		/* This is required to decode Extra subset to Unicode!! */
		jisx0208ToUnicode[offset] = (unsigned short)code;
	}
	else if(code >= 0xFF01 && code <= 0xFFEF)
	{
		/* Extra subset */
		extraToJis[code - 0xFF01] = (unsigned short)(offset + 0x0100);
		/* This is required to decode Extra subset to Unicode!! */
		jisx0208ToUnicode[offset] = (unsigned short)code;
	}
	else if(code >= 0x0100 && code < 0x4E00)
	{
		/* Non-CJK characters within JIS */
		processJis0208(code, (offset / 94) + 1, (offset % 94) + 1);
	}
	else if(code >= 0x00A7 && code <= 0x00F7)
	{
		/* Non-CJK characters within JIS for which unicodeToJis should not be
		 * edited. In addition to this, do not track lowJis and highJis. */
		jisx0208ToUnicode[offset] = (unsigned short)(code & 0xFF);
		jisx0208ToUnicode[offset + 1] = (((unsigned short)(code & 0x00FF)) >> 8);
	}
}

/*
 * Write a section header.
 */
static void writeSection(FILE *file, unsigned long num, unsigned long size)
{
	putc((int)(num & 0xFF), file);
	putc((int)((num >> 8) & 0xFF), file);
	putc((int)((num >> 16) & 0xFF), file);
	putc((int)((num >> 24) & 0xFF), file);
	putc((int)(size & 0xFF), file);
	putc((int)((size >> 8) & 0xFF), file);
	putc((int)((size >> 16) & 0xFF), file);
	putc((int)((size >> 24) & 0xFF), file);
}

/*
 * Write an array of 16-bit data values.
 */
static void writeData(FILE *file, unsigned short *data, unsigned long size)
{
	while(size > 0)
	{
		putc((int)(*data & 0xFF), file);
		putc((int)((*data >> 8) & 0xFF), file);
		++data;
		--size;
	}
}

/*
 * Section numbers for the JIS table.
 */
#define	JISX0208_To_Unicode 1
#define	JISX0212_To_Unicode 2
#define	CJK_To_JIS          3
#define	Greek_To_JIS        4
#define	Extra_To_JIS        5

/*
 * Write the JIS table file.
 */
static void writeJis(FILE *file)
{
	unsigned long size;

	/* Write the JIS X 0208 to Unicode conversion table */
	writeSection(file, JISX0208_To_Unicode, 94 * 94 * 2);
	writeData(file, jisx0208ToUnicode, 94 * 94);

	/* Write the JIS X 0212 to Unicode conversion table */
	writeSection(file, JISX0212_To_Unicode, 94 * 94 * 2);
	writeData(file, jisx0212ToUnicode, 94 * 94);

	/* Write the Unicode to JIS conversion table */
	size = highJis - lowJis + 1;
	writeSection(file, CJK_To_JIS, size * 2);
	writeData(file, unicodeToJis + lowJis, size);
	printf("JIS: U+%04lX to U+%04lX\n", lowJis, highJis);

	/* Write the Greek to JIS conversion table */
	writeSection(file, Greek_To_JIS, sizeof(greekToJis));
	writeData(file, greekToJis, sizeof(greekToJis) / 2);

	/* Write the Extra to JIS conversion table */
	writeSection(file, Extra_To_JIS, sizeof(extraToJis));
	writeData(file, extraToJis, sizeof(extraToJis) / 2);
}

/*
 * Create all of the tables that we need based on the Unihan.txt file.
 */
static int createTables(void)
{
	FILE *file;

	/* Create the JIS conversion table */
	if((file = fopen("jis.table", "wb")) == NULL)
	{
		if((file = fopen("jis.table", "wb")) == NULL)
		{
			perror("jis.table");
			return 1;
		}
	}
	writeJis(file);
	fclose(file);

	/* Done */
	return 0;
}
