/*
 * ucm2cp.c - Convert IBM ".ucm" files or hexadecimal mapping ".TXT" files
 * into code page handling classes.
 *
 * Copyright (c) 2002  Southern Storm Software, Pty Ltd
 * Copyright (c) 2006  Bruno Haible
 * Copyright (c) 2013  Mikko Korkalo
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

Usage: ucm2cp [options] file

	--region name			I18N region name
	--page num				Code page number
	--wpage num				Windows code page number (optional)
	--name str				Human-readable encoding name
	--webname str			Web name of the encoding
	--headername str		Header name of the encoding (optional)
	--bodyname str			Body name of the encoding (optional)
	--no-browser-display	Set browser display value to false (optional)
	--no-browser-save		Set browser save value to false (optional)
	--no-mailnews-display	Set mail/news display value to false (optional)
	--no-mailnews-save		Set mail/news save value to false (optional)

*/

#include <stdio.h>
#include <string.h>
#include <stdlib.h>

/*
 * Option values.
 */
static char *region = 0;
static int codePage = 0;
static int windowsCodePage = 0;
static char *name = 0;
static char *webName = 0;
static char *headerName = 0;
static char *bodyName = 0;
static int isBrowserDisplay = 1;
static int isBrowserSave = 1;
static int isMailNewsDisplay = 1;
static int isMailNewsSave = 1;
static const char *filename = 0;

/*
 * Forward declarations.
 */
static void usage(char *progname);
static void loadCharMaps(FILE *file);
static void printHeader(void);
static void printFooter(void);
static void printByteToChar(void);
static void printCharToByte(void);

int main(int argc, char *argv[])
{
	char *progname = argv[0];
	FILE *file;
	int len;

	/* Process the command-line options */
	while(argc > 1 && argv[1][0] == '-')
	{
		if(!strcmp(argv[1], "--page") && argc > 2)
		{
			codePage = atoi(argv[2]);
			++argv;
			--argc;
		}
		else if(!strcmp(argv[1], "--wpage") && argc > 2)
		{
			windowsCodePage = atoi(argv[2]);
			++argv;
			--argc;
		}
		else if(!strcmp(argv[1], "--region") && argc > 2)
		{
			region = argv[2];
			++argv;
			--argc;
		}
		else if(!strcmp(argv[1], "--name") && argc > 2)
		{
			name = argv[2];
			++argv;
			--argc;
		}
		else if(!strcmp(argv[1], "--webname") && argc > 2)
		{
			webName = argv[2];
			++argv;
			--argc;
		}
		else if(!strcmp(argv[1], "--headername") && argc > 2)
		{
			headerName = argv[2];
			++argv;
			--argc;
		}
		else if(!strcmp(argv[1], "--bodyname") && argc > 2)
		{
			bodyName = argv[2];
			++argv;
			--argc;
		}
		else if(!strcmp(argv[1], "--no-browser-display"))
		{
			isBrowserDisplay = 0;
		}
		else if(!strcmp(argv[1], "--no-browser-save"))
		{
			isBrowserSave = 0;
		}
		else if(!strcmp(argv[1], "--no-mailnews-display"))
		{
			isMailNewsDisplay = 0;
		}
		else if(!strcmp(argv[1], "--no-mailnews-save"))
		{
			isMailNewsSave = 0;
		}
		++argv;
		--argc;
	}

	/* Make sure that we have sufficient options */
	if(!region || !codePage || !name || !webName || argc != 2)
	{
		usage(progname);
		return 1;
	}

	/* Set defaults for unspecified options */
	if(!headerName)
	{
		headerName = webName;
	}
	if(!bodyName)
	{
		bodyName = webName;
	}
	if(!windowsCodePage)
	{
		windowsCodePage = codePage;
	}

	/* Open the UCM or TXT file */
	file = fopen(argv[1], "r");
	if(!file)
	{
		perror(argv[1]);
		return 1;
	}
	filename = argv[1];
	len = strlen(filename);
	while(len > 0 && filename[len - 1] != '/' && filename[len - 1] != '\\')
	{
		--len;
	}
	filename += len;

	/* Load the character maps from the input file */
	loadCharMaps(file);

	/* Print the output header */
	printHeader();

	/* Print the byte->char conversion table */
	printByteToChar();

	/* Output the char->byte conversion methods */
	printCharToByte();

	/* Print the output footer */
	printFooter();

	/* Clean up and exit */
	fclose(file);
	return 0;
}

static void usage(char *progname)
{
	fprintf(stderr, "Usage: %s [options] file\n\n", progname);
	fprintf(stderr, "    --region name         I18N region name\n");
	fprintf(stderr, "    --page num            Code page number\n");
	fprintf(stderr, "    --wpage num           Windows code page number (optional)\n");
	fprintf(stderr, "    --name str            Human-readable encoding name\n");
	fprintf(stderr, "    --webname str         Web name of the encoding\n");
	fprintf(stderr, "    --headername str      Header name of the encoding (optional)\n");
	fprintf(stderr, "    --bodyname str        Body name of the encoding (optional)\n");
	fprintf(stderr, "    --no-browser-display  Set browser display value to false (optional)\n");
	fprintf(stderr, "    --no-browser-save     Set browser save value to false (optional)\n");
	fprintf(stderr, "    --no-mailnews-display Set mail/news display value to false (optional)\n");
	fprintf(stderr, "    --no-mailnews-save    Set mail/news save value to false (optional)\n");
}

/*
 * Map bytes to characters.  The level value is used to determine
 * which char mapping is the most likely if there is more than one.
 */
static unsigned byteToChar[256];
static int      byteToCharLevel[256];

/*
 * Map characters to bytes.
 */
static int charToByte[65536];

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
 * Load the character mapping information from a UCM or TXT file.
 */
static void loadCharMaps(FILE *file)
{
	enum { unknown, ucm, txt } syntax;
	unsigned long posn;
	unsigned long byteValue;
	int level;
	char buffer[BUFSIZ];
	const char *buf;

	/* Initialize the mapping tables */
	for(posn = 0; posn < 256; ++posn)
	{
		byteToChar[posn] = (unsigned)'?';
		byteToCharLevel[posn] = 100;
	}
	for(posn = 0; posn < 65536; ++posn)
	{
		charToByte[posn] = -1;
	}

	syntax = unknown;

	/* Read the contents of the file */
	while(fgets(buffer, BUFSIZ, file))
	{
		/* Syntax recognition */
		if (syntax == unknown)
		{
			if (memcmp(buffer, "CHARMAP", 7) == 0)
				syntax = ucm;
			else if (memcmp(buffer, "0x", 2) == 0)
				syntax = txt;
		}

		if (syntax == ucm)
		{
			/* Lines of interest begin with "<U" */
			if(buffer[0] != '<' || buffer[1] != 'U')
			{
				continue;
			}

			/* Parse the fields on the line */
			buf = buffer + 2;
			buf += parseHex(buf, &posn);
			if(posn >= 65536)
			{
				continue;
			}
			while(*buf != '\0' && *buf != '\\')
			{
				++buf;
			}
			if(*buf != '\\' || buf[1] != 'x')
			{
				continue;
			}
			buf += 2;
			buf += parseHex(buf, &byteValue);
			if(byteValue >= 256)
			{
				continue;
			}
			while(*buf != '\0' && *buf != '|')
			{
				++buf;
			}
			if(*buf != '|')
			{
				continue;
			}
			level = (int)(buf[1] - '0');
		}
		else
		if (syntax == txt)
		{
			unsigned int x;
			int cnt;

			/* Lines of interest begin with "0x" */
			if(buffer[0] != '0' || buffer[1] != 'x')
				continue;

			/* Parse the fields on the line */
			if(sscanf(buffer, "0x%x%n", &x, &cnt) <= 0)
				exit(1);
			if(!(x < 0x100))
				exit(1);
			byteValue = x;
			while (buffer[cnt] == ' ' || buffer[cnt] == '\t')
				cnt++;
			if(sscanf(buffer+cnt, "0x%x", &x) != 1)
				continue;
			if(!(x < 0x10000))
				exit(1);
			posn = x;
			level = 0;
		}
		else
			continue;

		/* Update the byte->char mapping table */
		if(level < byteToCharLevel[byteValue])
		{
			byteToCharLevel[byteValue] = level;
			byteToChar[byteValue] = (unsigned)posn;
		}

		/* Update the char->byte mapping table */
		charToByte[posn] = (int)byteValue;
	}
}

#define	COPYRIGHT_MSG \
" *\n" \
" * Copyright (c) 2002  Southern Storm Software, Pty Ltd\n" \
" *\n" \
" * Permission is hereby granted, free of charge, to any person obtaining\n" \
" * a copy of this software and associated documentation files (the \"Software\"),\n" \
" * to deal in the Software without restriction, including without limitation\n" \
" * the rights to use, copy, modify, merge, publish, distribute, sublicense,\n" \
" * and/or sell copies of the Software, and to permit persons to whom the\n" \
" * Software is furnished to do so, subject to the following conditions:\n" \
" *\n" \
" * The above copyright notice and this permission notice shall be included\n" \
" * in all copies or substantial portions of the Software.\n" \
" *\n" \
" * THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS\n" \
" * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,\n" \
" * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL\n" \
" * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR\n" \
" * OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,\n" \
" * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR\n" \
" * OTHER DEALINGS IN THE SOFTWARE.\n" \
" */\n\n"

/*
 * Print the header for the current code page definition.
 */
static void printHeader(void)
{
	printf("/*\n * CP%d.cs - %s code page.\n", codePage, name);
	fputs(COPYRIGHT_MSG, stdout);
	printf("// Generated from \"%s\".\n\n", filename);
	printf("// WARNING: Modifying this file directly might be a bad idea.\n");
	printf("// You should edit the code generator tools/ucm2cp.c instead for your changes\n");
	printf("// to appear in all relevant classes.\n");
	printf("namespace I18N.%s\n{\n\n", region);
	printf("using System;\n");
	printf("using System.Text;\n");
	printf("using I18N.Common;\n\n");
	printf("[Serializable]\n");
	printf("public class CP%d : ByteEncoding\n{\n", codePage);
	printf("\tpublic CP%d()\n", codePage);
	printf("\t\t: base(%d, ToChars, \"%s\",\n", codePage, name);
	printf("\t\t       \"%s\", \"%s\", \"%s\",\n",
	       bodyName, headerName, webName);
	printf("\t\t       %s, %s, %s, %s, %d)\n",
		   (isBrowserDisplay ? "true" : "false"),
		   (isBrowserSave ? "true" : "false"),
		   (isMailNewsDisplay ? "true" : "false"),
		   (isMailNewsSave ? "true" : "false"),
		   windowsCodePage);
	printf("\t{}\n\n");
}

/*
 * Print an encoding name, adjusted to look like a type name.
 */
static void printEncodingName(const char *name)
{
	while(*name != '\0')
	{
		if(*name >= 'A' && *name <= 'Z')
		{
			putc(*name - 'A' + 'a', stdout);
		}
		else if(*name == '-')
		{
			putc('_', stdout);
		}
		else
		{
			putc(*name, stdout);
		}
		++name;
	}
}

/*
 * Print the footer for the current code page definition.
 */
static void printFooter(void)
{
	printf("}; // class CP%d\n\n", codePage);
	printf("[Serializable]\n");
	printf("public class ENC");
	printEncodingName(webName);
	printf(" : CP%d\n{\n", codePage);
	printf("\tpublic ENC");
	printEncodingName(webName);
	printf("() : base() {}\n\n");
	printf("}; // class ENC");
	printEncodingName(webName);
	printf("\n\n}; // namespace I18N.%s\n", region);
}

/*
 * Print the byte->char conversion table.
 */
static void printByteToChar(void)
{
	int posn;
	printf("\tprivate static readonly char[] ToChars = {");
	for(posn = 0; posn < 256; ++posn)
	{
		if((posn % 6) == 0)
		{
			printf("\n\t\t");
		}
		printf("'\\u%04X', ", byteToChar[posn]);
	}
	printf("\n\t};\n\n");
}

/*
 * Print a "switch" statement that converts "ch" from
 * a character value into a byte value.
 */
static void printConvertSwitch(int forString)
{
	unsigned long directLimit;
	unsigned long posn;
	unsigned long posn2;
	unsigned long rangeSize;
	int haveDirect;
	int haveFullWidth;

	/* Find the limit of direct byte mappings */
	directLimit = 0;
	while(directLimit < 256 && charToByte[directLimit] == (int)directLimit)
	{
		++directLimit;
	}

	/* Determine if we have the full-width Latin1 mappings, which
	   we can optimise in the default case of the switch */
	haveFullWidth = 1;
	for(posn = 0xFF01; posn <= 0xFF5E; ++posn)
	{
		if((charToByte[posn] - 0x21) != (int)(posn - 0xFF01))
		{
			haveFullWidth = 0;
		}
	}

	/* Print the switch header.  The "if" is an optimisation
	   to ignore the common case of direct ASCII mappings */
	printf("\t\t\tif(ch >= %lu) switch(ch)\n", directLimit);
	printf("\t\t\t{\n");

	/* Handle all direct byte mappings above the direct limit */
	haveDirect = 0;
	for(posn = directLimit; posn < 256; ++posn)
	{
		if(charToByte[posn] == (int)posn)
		{
			haveDirect = 1;
			printf("\t\t\t\tcase 0x%04lX:\n", posn);
		}
	}
	if(haveDirect)
	{
		printf("\t\t\t\t\tbreak;\n");
	}

	/* Handle the indirect mappings */
	for(posn = 0; posn < 65536; ++posn)
	{
		if(haveFullWidth && posn >= 0xFF01 && posn <= 0xFF5E)
		{
			/* Handle full-width Latin1 conversions later */
			continue;
		}
		if(charToByte[posn] != (int)posn &&
		   charToByte[posn] != -1)
		{
			/* See if we have a run of 4 or more characters that
			   can be mapped algorithmically to some other range */
			rangeSize = 1;
			for(posn2 = posn + 1; posn2 < 65536; ++posn2)
			{
				if(charToByte[posn2] == (int)posn2 ||
				   charToByte[posn2] == -1)
				{
					break;
				}
				if((charToByte[posn2] - charToByte[posn]) !=
				   (int)(posn2 - posn))
				{
					break;
				}
				++rangeSize;
			}
			if(rangeSize >= 4)
			{
				/* Output a range mapping for the characters */
				for(posn2 = posn; posn2 < (posn + rangeSize); ++posn2)
				{
					printf("\t\t\t\tcase 0x%04lX:\n", posn2);
				}
				posn += rangeSize - 1;
				if(((long)posn) >= (long)(charToByte[posn]))
				{
					printf("\t\t\t\t\tch -= 0x%04lX;\n",
						   (long)(posn - charToByte[posn]));
				}
				else
				{
					printf("\t\t\t\t\tch += 0x%04lX;\n",
						   (long)(charToByte[posn] - posn));
				}
				printf("\t\t\t\t\tbreak;\n");
			}
			else
			{
				/* Use a simple non-algorithmic mapping */
				printf("\t\t\t\tcase 0x%04lX: ch = 0x%02X; break;\n",
					   posn, (unsigned)(charToByte[posn]));
			}
		}
	}

	/* Print the switch footer */
	if(!haveFullWidth)
	{
		if(forString)
			printf("\t\t\t\tdefault: ch = 0x3F; break;\n");
		else {
			printf("\t\t\t\tdefault:\n");
			printf("\t\t\t\t\tHandleFallback (ref buffer, chars, ref charIndex, ref charCount, bytes, ref byteIndex, ref byteCount);\n");
			printf("\t\t\t\t\tcharIndex++;\n");
			printf("\t\t\t\t\tcharCount--;\n");
			printf("\t\t\t\t\tcontinue;\n");
		}
	}
	else
	{
		printf("\t\t\t\tdefault:\n");
		printf("\t\t\t\t{\n");
		printf("\t\t\t\t\tif(ch >= 0xFF01 && ch <= 0xFF5E)\n");
		printf("\t\t\t\t\t{\n");
		printf("\t\t\t\t\t\tch -= 0xFEE0;\n");
		printf("\t\t\t\t\t}\n");
		printf("\t\t\t\t\telse\n");
		printf("\t\t\t\t\t{\n");
		if(forString) /* this is basically meaningless, just to make diff for unused code minimum */
			printf("\t\t\t\t\t\tch = 0x3F;\n");
		else {
			printf("\t\t\t\t\t\tHandleFallback (ref buffer, chars, ref charIndex, ref charCount, bytes, ref byteIndex, ref byteCount);\n");
			printf("\t\t\t\t\t\tcharIndex++;\n");
			printf("\t\t\t\t\t\tcharCount--;\n");
			printf("\t\t\t\t\t\tcontinue;\n");
		}
		printf("\t\t\t\t\t}\n");
		printf("\t\t\t\t}\n");
		printf("\t\t\t\tbreak;\n");
	}
	printf("\t\t\t}\n");
}

/*
 * Print the char->byte conversion methods.
 */
static void printCharToByte(void)
{
	printf("\t// Get the number of bytes needed to encode a character buffer.\n");
	printf("\tpublic unsafe override int GetByteCountImpl (char* chars, int count)\n");
	printf("\t{\n");
	printf("\t\tif (this.EncoderFallback != null)");
	printf("\t\t{\n");
	printf("\t\t\t//Calculate byte count by actually doing encoding and discarding the data.\n");
	printf("\t\t\treturn GetBytesImpl(chars, count, null, 0);\n");
	printf("\t\t}\n");
	printf("\t\telse\n");
	printf("\t\t{\n");
	printf("\t\t\treturn count;\n");
	printf("\t\t}\n");
	printf("\t}\n");
	printf("\n");
	printf("\t// Get the number of bytes needed to encode a character buffer.\n");
	printf("\tpublic override int GetByteCount (String s)\n");
	printf("\t{\n");
	printf("\t\tif (this.EncoderFallback != null)\n");
	printf("\t\t{\n");
	printf("\t\t\t//Calculate byte count by actually doing encoding and discarding the data.\n");
	printf("\t\t\tunsafe\n");
	printf("\t\t\t{\n");
	printf("\t\t\t\tfixed (char *s_ptr = s)\n");
	printf("\t\t\t\t{\n");
	printf("\t\t\t\t\treturn GetBytesImpl(s_ptr, s.Length, null, 0);\n");
	printf("\t\t\t\t}\n");
	printf("\t\t\t}\n");
	printf("\t\t}\n");
	printf("\t\telse\n");
	printf("\t\t{\n");
	printf("\t\t\t//byte count equals character count because no EncoderFallback set\n");
	printf("\t\t\treturn s.Length;\n");
	printf("\t\t}\n");
	printf("\t}\n");
	printf("\n");
	printf("\t//ToBytes is just an alias for GetBytesImpl, but doesn't return byte count\n");
	printf("\tprotected unsafe override void ToBytes(char* chars, int charCount,\n");
	printf("\t                                byte* bytes, int byteCount)\n");
	printf("\t{\n");
	printf("\t\t//Calling ToBytes with null destination buffer doesn't make any sense\n");
	printf("\t\tif (bytes == null)\n");
	printf("\t\t\tthrow new ArgumentNullException(\"bytes\");\n");
	printf("\t\tGetBytesImpl(chars, charCount, bytes, byteCount);\n");
	printf("\t}\n");
	printf("\n");

	/* Print the conversion method for character buffers */
	//printf("\tprotected unsafe override void ToBytes(char* chars, int charCount,\n");
	//printf("\t                                byte* bytes, int byteCount)\n");
	printf("\tpublic unsafe override int GetBytesImpl (char* chars, int charCount,\n");
	printf("\t                                         byte* bytes, int byteCount)\n");
	printf("\t{\n");
	printf("\t\tint ch;\n");
	printf("\t\tint charIndex = 0;\n");
	printf("\t\tint byteIndex = 0;\n");
	printf("\t\tEncoderFallbackBuffer buffer = null;\n");
	printf("\t\twhile (charCount > 0)\n");
	printf("\t\t{\n");
	printf("\t\t\tch = (int)(chars[charIndex]);\n");
	printConvertSwitch(0);
	printf("\t\t\t//Write encoded byte to buffer, if buffer is defined and fallback was not used\n");
	printf("\t\t\tif (bytes != null)\n");
	printf("\t\t\t\tbytes[byteIndex] = (byte)ch;\n");
	printf("\t\t\tbyteIndex++;\n");
	printf("\t\t\tbyteCount--;\n");
	printf("\t\t\tcharIndex++;\n");
	printf("\t\t\tcharCount--;\n");
	printf("\t\t}\n");
	printf("\t\treturn byteIndex;\n");
	printf("\t}\n");
}
