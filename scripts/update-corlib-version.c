// Read configure.ac, look for \nMONO_CORLIB_VERSION=...\n
// replace ... with a new uuid and write it back out.

#include <stdarg.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#ifdef _WIN32
#include <windows.h>
#else
#include <uuid/uuid.h>
#endif

typedef struct Uuid_t {
	// There is some structure to the underlying bytes but we don't care.
	unsigned char bytes [16];
} Uuid_t;

void
UuidGen (Uuid_t* uuid)
{
#ifdef _WIN32
	UUID uid = { 0 };
	UuidCreate (&uid);
#else
	uuid_t uid = { 0 };
	uuid_generate (uid);
#endif
	memcpy (uuid, &uid, 16);
}

const static char hex [] = "0123456789ABCDEF";

void
ByteToHex (int byte, char str[2])
{
	str [1] = hex [byte & 0xF];
	str [0] = hex [(byte >> 4) & 0xF];
}

void
UuidToStr (const Uuid_t* uuid, char str[16])
{
	int i;
	for (i = 0; i < 16; ++i)
		ByteToHex (uuid->bytes [i], &str [i * 2]);
}

int
Die (const char *format, ...)
{
	va_list args;
	va_start (args, format);
	vfprintf (stderr, format, args);
	exit (1);
	return 0;
}

int
#ifdef _MSC_VER
__cdecl
#endif
main (int argc, char** argv)
{
	size_t available = { 0 };
	char* contents = { 0 };
	char* corlib = { 0 };
	char* corlib_newline = { 0 };
	FILE* file = { 0 };
	size_t next_read = { 0 };
	char* path = { 0 };
	size_t size_buffer = { 0 };
	size_t size_read = { 0 };
	Uuid_t uuid = { 0 };
	char uuidstr[33] = { 0 };
	const static char* marker = "\nMONO_CORLIB_VERSION="; // must start with newline

	UuidGen (&uuid);
	UuidToStr (&uuid, uuidstr);

	(path = argv [1]) || Die ("missing path on command line\n");
	(file = fopen (path, "r")) || Die ("unable to open(r) file %s\n", path);

	// read entire file into memory, growing buffer as needed

	while (1)
	{
		contents = realloc (contents, size_buffer = size_buffer * 2 + 4096);
		available = size_buffer - size_read;
		next_read = fread (contents + size_read, 1, available, file);
		if (next_read < available) {
			size_read += next_read;
			contents [size_read] = 0;
			break;
		}
		size_read += next_read;
	}

	// Split the contents at marker and the following newline (marker starts with newlines).

	(corlib = strstr (contents, marker)) || Die ("%s not found in %s\n", marker + 1, path);
	(corlib_newline = strchr (corlib + 1, '\n')) || Die ("%s terminal newline not found in %s\n", marker + 1, path);

	// Truncate at marker.
	// Output pre-marker, marker, new uuid, post-marker-newline.

	*corlib = 0;
	fclose (file) && Die ("fclose %s failed\n", path);
	(file = fopen (path, "w")) || Die ("unable to open(w) file %s\n", path);
	fprintf (file, "%s%s%s%s", contents, marker, uuidstr, corlib_newline);
	fclose (file) && Die ("fclose %s failed\n", path);
	return 0;
}
