#!/usr/bin/env bash

set -e

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

# This weird syntax is necessary because some zip archives do not contain a separate
# entry for the root directory but merely a collection of its subdirectories (for instance
# platform-tools). The very first entry in the archive is retrieved, then its path is read and
# finally the part up to the first '/' of the path is retrieved as the root directory of
# the archive. With platform-tools the last part is necessary because otherwise the root directory
# would end up being reported as `platform-tools/adb` as this is the first entry in the archive and
# there's no `platform-tools/` entry
ZIP_ROOT_DIR=$(unzip -qql "$1" | head -n1 | tr -s ' ' | cut -d' ' -f5- | tr '/' ' ' | cut -d' ' -f1)

# We need a temporary directory because some archives (emulator) have their root directory named the
# same as a file/directory inside it (emulator has emulator/emulator executable for instance) and
# moving such a file/directory to .. wouldn't work
ARCHIVE_TEMP_DIR=$(mktemp -d)

unzip "$1" -d "$ARCHIVE_TEMP_DIR"
mkdir -p "$2"
find "$ARCHIVE_TEMP_DIR/$ZIP_ROOT_DIR/" -maxdepth 1 -not \( -name "$ZIP_ROOT_DIR" -and -type d \) -and -not -name . -and -not -name .. -exec mv -f '{}' "$2" ';'
