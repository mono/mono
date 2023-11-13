#!/usr/bin/env bash

set -xe

function cleanup()
{
	if [ -n "$ARCHIVE_TEMP_DIR" ]; then
		rm -rf "$ARCHIVE_TEMP_DIR"
	fi
}

trap cleanup 0

## Parameters:
#    $1: path to source archive
#    $2: destination directory

#
# Special case archives - archives which have no single root directory when unpacked and thus
#                         the workaround below doesn't work for them.
# Entry format: grep regex matching the END of path in $1 (no trailing $ necessary)
#
SPECIAL_CASE_ARCHIVES="
    cmake-.*\.zip
"

HAVE_SPECIAL_CASE=no
for sac in $SPECIAL_CASE_ARCHIVES; do
	if echo $1 | grep "${sac}$" > /dev/null 2>&1; then
		HAVE_SPECIAL_CASE=yes
		break
	fi
done

if [ "$HAVE_SPECIAL_CASE" == "no" ]; then
	# This weird syntax is necessary because some zip archives do not contain a separate
	# entry for the root directory but merely a collection of its subdirectories (for instance
	# platform-tools). The very first entry in the archive is retrieved, then its path is read and
	# finally the part up to the first '/' of the path is retrieved as the root directory of
	# the archive. With platform-tools the last part is necessary because otherwise the root directory
	# would end up being reported as `platform-tools/adb` as this is the first entry in the archive and
	# there's no `platform-tools/` entry
	ZIP_ROOT_DIR=$(unzip -qql "$1" | head -n1 | tr -s ' ' | cut -d' ' -f5- | tr '/' ' ' | cut -d' ' -f1)
fi

# We need a temporary directory because some archives (emulator) have their root directory named the
# same as a file/directory inside it (emulator has emulator/emulator executable for instance) and
# moving such a file/directory to .. wouldn't work
ARCHIVE_TEMP_DIR=$(mktemp -d -t unzip_android_archive_XXXXXXXX)

unzip "$1" -d "$ARCHIVE_TEMP_DIR"
mkdir -p "$2"

if [ -z "$ZIP_ROOT_DIR" ]; then
	mv -f "$ARCHIVE_TEMP_DIR"/* "$2"
else
	find "$ARCHIVE_TEMP_DIR/$ZIP_ROOT_DIR/" -maxdepth 1 -not \( -name "$ZIP_ROOT_DIR" -and -type d \) -and -not -name . -and -not -name .. -exec mv -f '{}' "$2" ';'
fi
