#! /bin/sh
#
# mklist.sh - Make the I18N handler list file "I18N-handlers.def".
#
# Copyright (C) 2002  Southern Storm Software, Pty Ltd.
#
# This program is free software; you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation; either version 2 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program; if not, write to the Free Software
# Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

# Process the command-line options.
ILFIND="$1"
OUTFILE="$2"
if test "x$ILFIND" = "x" ; then
	echo "$0: missing ilfind argument" 1>&2
	exit 1
fi
if test "x$OUTFILE" = "x" ; then
	echo "$0: missing output file argument" 1>&2
	exit 1
fi
shift
shift

# Search all region assemblies for interesting classes and
# write them to the specified output file.
exec "${ILFIND}" --public-only --sub-string I18N $* | \
	grep 'class ' | \
	sed -e '1,$s/^.*: class //g' - | \
	sed -e '1,$s/^class //g' - >"${OUTFILE}"
