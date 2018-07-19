#!/bin/sh
#
# This script can be used to regenerate the CPnnnn.cs files
# from the ".ucm" files.  The list of unsupported code pages
# is at the end of this file.
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

# Make sure that we are in the correct directory to start.
if test -d ../Rare ; then
	cd ..
fi

#if test "x$1" = "x--include-rare" ; then
	RARE=""
#else
#	RARE="--rare"
#fi

# Compile the "ucm2cp" tool.
if gcc -o tools/ucm2cp tools/ucm2cp.c ; then
	:
else
	exit 1
fi
UCM2CP=tools/ucm2cp

${UCM2CP} --region Rare --page 37 --wpage 1252 \
	--name 'IBM EBCDIC (US-Canada)' \
	--webname IBM037 --bodyname IBM037 \
	--headername IBM037 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-37.ucm >Rare/CP37.cs

${UCM2CP} --region West --page 437 --wpage 1252 \
	--name 'OEM United States' \
	--webname IBM437 --bodyname IBM437 \
	--headername IBM437 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save West/windows-437-2000.ucm >West/CP437.cs

${UCM2CP} --region Rare --page 500 --wpage 1252 \
	--name 'IBM EBCDIC (International)' \
	--webname IBM500 --bodyname IBM500 \
	--headername IBM500 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-500.ucm >Rare/CP500.cs

${UCM2CP} --region Rare --page 708 --wpage 1256 \
	--name 'Arabic (ASMO 708)' \
	--webname asmo-708 --bodyname iso-8859-6 \
	--headername asmo-708 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-1089.ucm >Rare/CP708.cs

${UCM2CP} --region Rare --page 737 --wpage 1253 \
	--name 'Greek (DOS)' \
	--webname ibm737 --bodyname ibm737 \
	--headername ibm737 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-737.ucm >Rare/CP737.cs

${UCM2CP} --region Rare --page 775 --wpage 1257 \
	--name 'Baltic (DOS)' \
	--webname ibm775 --bodyname ibm775 \
	--headername ibm775 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-775.ucm >Rare/CP775.cs

${UCM2CP} --region West --page 850 --wpage 1252 \
	--name 'Western European (DOS)' \
	--webname ibm850 --bodyname ibm850 \
	--headername ibm850 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save West/ibm-850.ucm >West/CP850.cs

${UCM2CP} --region Rare --page 852 --wpage 1250 \
	--name 'Central European (DOS)' \
	--webname ibm852 --bodyname ibm852 \
	--headername ibm852 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-852.ucm >Rare/CP852.cs

${UCM2CP} --region Rare --page 855 --wpage 1251 \
	--name 'Cyrillic (DOS)' \
	--webname ibm855 --bodyname ibm855 \
	--headername ibm855 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-855.ucm >Rare/CP855.cs

${UCM2CP} --region Rare --page 857 --wpage 1254 \
	--name 'Turkish (DOS)' \
	--webname ibm857 --bodyname ibm857 \
	--headername ibm857 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-857.ucm >Rare/CP857.cs

${UCM2CP} --region Rare --page 858 --wpage 1252 \
	--name 'Western European (DOS with Euro)' \
	--webname IBM00858 --bodyname IBM00858 \
	--headername IBM00858 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-858.ucm >Rare/CP858.cs

${UCM2CP} --region West --page 860 --wpage 1252 \
	--name 'Portuguese (DOS)' \
	--webname ibm860 --bodyname ibm860 \
	--headername ibm860 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save West/ibm-860.ucm >West/CP860.cs

${UCM2CP} --region West --page 861 --wpage 1252 \
	--name 'Icelandic (DOS)' \
	--webname ibm861 --bodyname ibm861 \
	--headername ibm861 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save West/ibm-861.ucm >West/CP861.cs

${UCM2CP} --region Rare --page 862 --wpage 1255 \
	--name 'Hebrew (DOS)' \
	--webname ibm862 --bodyname ibm862 \
	--headername ibm861 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-862.ucm >Rare/CP862.cs

${UCM2CP} --region West --page 863 --wpage 1252 \
	--name 'French Canadian (DOS)' \
	--webname IBM863 --bodyname IBM863 \
	--headername IBM863 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save West/ibm-863.ucm >West/CP863.cs

${UCM2CP} --region Rare --page 864 --wpage 1256 \
	--name 'Arabic (DOS)' \
	--webname ibm864 --bodyname ibm864 \
	--headername ibm864 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-864.ucm >Rare/CP864.cs

${UCM2CP} --region West --page 865 --wpage 1252 \
	--name 'Nordic (DOS)' \
	--webname IBM865 --bodyname IBM863 \
	--headername IBM865 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save West/ibm-865.ucm >West/CP865.cs

${UCM2CP} --region Rare --page 866 --wpage 1251 \
	--name 'Russian (DOS)' \
	--webname ibm866 --bodyname ibm866 \
	--headername ibm866 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-866.ucm >Rare/CP866.cs

${UCM2CP} --region Rare --page 869 --wpage 1253 \
	--name 'Greek (DOS)' \
	--webname ibm869 --bodyname ibm869 \
	--headername ibm869 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-869.ucm >Rare/CP869.cs

${UCM2CP} --region Rare --page 870 --wpage 1250 \
	--name 'IBM EBCDIC (Latin 2)' \
	--webname ibm870 --bodyname ibm870 \
	--headername ibm870 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-870.ucm >Rare/CP870.cs

${UCM2CP} --region Other --page 874 --wpage 874 \
	--name 'Thai (Windows)' \
	--webname windows-874 --bodyname windows-874 \
	--headername windows-874 Other/ibm-874.ucm >Other/CP874.cs

${UCM2CP} --region Rare --page 875 --wpage 1253 \
	--name 'IBM EBCDIC (Greek)' \
	--webname ibm875 --bodyname ibm875 \
	--headername ibm875 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-875.ucm >Rare/CP875.cs

${UCM2CP} --region Rare --page 1026 --wpage 1254 \
	--name 'IBM EBCDIC (Turkish)' \
	--webname ibm1026 --bodyname ibm1026 \
	--headername ibm1026 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-1026.ucm >Rare/CP1026.cs

${UCM2CP} --region Rare --page 1047 --wpage 1252 \
	--name 'IBM EBCDIC (Open Systems Latin 1)' \
	--webname ibm1047 --bodyname ibm1047 \
	--headername ibm1047 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-1047.ucm >Rare/CP1047.cs

${UCM2CP} --region Rare --page 1140 --wpage 1252 \
	--name 'IBM EBCDIC (US-Canada with Euro)' \
	--webname IBM01140 --bodyname IBM01140 \
	--headername IBM01140 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-1140.ucm >Rare/CP1140.cs

${UCM2CP} --region Rare --page 1141 --wpage 1252 \
	--name 'IBM EBCDIC (Germany with Euro)' \
	--webname IBM01141 --bodyname IBM01141 \
	--headername IBM01141 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-1141.ucm >Rare/CP1141.cs

${UCM2CP} --region Rare --page 1142 --wpage 1252 \
	--name 'IBM EBCDIC (Denmark/Norway with Euro)' \
	--webname IBM01142 --bodyname IBM01142 \
	--headername IBM01142 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-1142.ucm >Rare/CP1142.cs

${UCM2CP} --region Rare --page 1143 --wpage 1252 \
	--name 'IBM EBCDIC (Finland/Sweden with Euro)' \
	--webname IBM01143 --bodyname IBM01143 \
	--headername IBM01143 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-1143.ucm >Rare/CP1143.cs

${UCM2CP} --region Rare --page 1144 --wpage 1252 \
	--name 'IBM EBCDIC (Italy with Euro)' \
	--webname ibm1144 --bodyname ibm1144 \
	--headername ibm1144 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-1144.ucm >Rare/CP1144.cs

${UCM2CP} --region Rare --page 1145 --wpage 1252 \
	--name 'IBM EBCDIC (Latin America/Spain with Euro)' \
	--webname ibm1145 --bodyname ibm1145 \
	--headername ibm1145 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-1145.ucm >Rare/CP1145.cs

${UCM2CP} --region Rare --page 1146 --wpage 1252 \
	--name 'IBM EBCDIC (United Kingdom with Euro)' \
	--webname ibm1146 --bodyname ibm1146 \
	--headername ibm1146 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-1146.ucm >Rare/CP1146.cs

${UCM2CP} --region Rare --page 1147 --wpage 1252 \
	--name 'IBM EBCDIC (France with Euro)' \
	--webname ibm1147 --bodyname ibm1147 \
	--headername ibm1147 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-1147.ucm >Rare/CP1147.cs

${UCM2CP} --region Rare --page 1148 --wpage 1252 \
	--name 'IBM EBCDIC (International with Euro)' \
	--webname ibm1148 --bodyname ibm1148 \
	--headername ibm1148 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-1148.ucm >Rare/CP1148.cs

${UCM2CP} --region Rare --page 1149 --wpage 1252 \
	--name 'IBM EBCDIC (Icelandic with Euro)' \
	--webname ibm1149 --bodyname ibm1149 \
	--headername ibm1149 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-1149.ucm >Rare/CP1149.cs

${UCM2CP} --region West --page 1250 --wpage 1250 \
	--name 'Central European (Windows)' \
	--webname windows-1250 --bodyname iso-8859-2 \
	--headername windows-1250 West/ibm-5346.ucm >West/CP1250.cs

${UCM2CP} --region Other --page 1251 --wpage 1251 \
	--name 'Cyrillic (Windows)' \
	--webname windows-1251 --bodyname koi8-r \
	--headername windows-1251 Other/ibm-5347.ucm >Other/CP1251.cs

${UCM2CP} --region West --page 1252 --wpage 1252 \
	--name 'Western European (Windows)' \
	--webname Windows-1252 --bodyname iso-8859-1 \
	--headername Windows-1252 West/ibm-5348.ucm >West/CP1252.cs

${UCM2CP} --region West --page 1253 --wpage 1253 \
	--name 'Greek (Windows)' \
	--webname windows-1253 --bodyname iso-8859-7 \
	--headername windows-1253 West/ibm-5349.ucm >West/CP1253.cs

${UCM2CP} --region MidEast --page 1254 --wpage 1254 \
	--name 'Turkish (Windows)' \
	--webname windows-1254 --bodyname iso-8859-9 \
	--headername windows-1254 MidEast/ibm-5350.ucm >MidEast/CP1254.cs

${UCM2CP} --region MidEast --page 1255 --wpage 1255 \
	--name 'Hebrew (Windows)' \
	--webname windows-1255 --bodyname windows-1255 \
	--headername windows-1255 MidEast/ibm-5351.ucm >MidEast/CP1255.cs

${UCM2CP} --region MidEast --page 1256 --wpage 1256 \
	--name 'Arabic (Windows)' \
	--webname windows-1256 --bodyname windows-1256 \
	--headername windows-1256 MidEast/ibm-5352.ucm >MidEast/CP1256.cs

${UCM2CP} --region Other --page 1257 --wpage 1257 \
	--name 'Baltic (Windows)' \
	--webname windows-1257 --bodyname iso-8859-4 \
	--headername windows-1257 Other/ibm-5353.ucm >Other/CP1257.cs

${UCM2CP} --region Other --page 1258 --wpage 1258 \
	--name 'Vietnamese (Windows)' \
	--webname windows-1258 --bodyname windows-1258 \
	--headername windows-1258 Other/ibm-5354.ucm >Other/CP1258.cs

${UCM2CP} --region West --page 10000 --wpage 1252 \
	--name 'Western European (Mac)' \
	--webname macintosh --bodyname macintosh \
	--headername macintosh --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save West/ibm-1275.ucm >West/CP10000.cs

${UCM2CP} --region West --page 10079 --wpage 1252 \
	--name 'Icelandic (Mac)' \
	--webname x-mac-icelandic --bodyname x-mac-icelandic \
	--headername x-mac-icelandic --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save West/windows-10079-2000.ucm >West/CP10079.cs

${UCM2CP} --region Rare --page 20273 --wpage 1252 \
	--name 'IBM EBCDIC (Germany)' \
	--webname IBM273 --bodyname IBM273 \
	--headername IBM273 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-273.ucm >Rare/CP20273.cs

${UCM2CP} --region Rare --page 20277 --wpage 1252 \
	--name 'IBM EBCDIC (Denmark/Norway)' \
	--webname IBM277 --bodyname IBM277 \
	--headername IBM277 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-277.ucm >Rare/CP20277.cs

${UCM2CP} --region Rare --page 20278 --wpage 1252 \
	--name 'IBM EBCDIC (Finland/Sweden)' \
	--webname IBM278 --bodyname IBM278 \
	--headername IBM278 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-278.ucm >Rare/CP20278.cs

${UCM2CP} --region Rare --page 20280 --wpage 1252 \
	--name 'IBM EBCDIC (Italy)' \
	--webname IBM280 --bodyname IBM280 \
	--headername IBM280 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-280.ucm >Rare/CP20280.cs

${UCM2CP} --region Rare --page 20284 --wpage 1252 \
	--name 'IBM EBCDIC (Latin America/Spain)' \
	--webname IBM284 --bodyname IBM284 \
	--headername IBM284 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-284.ucm >Rare/CP20284.cs

${UCM2CP} --region Rare --page 20285 --wpage 1252 \
	--name 'IBM EBCDIC (United Kingdom)' \
	--webname IBM285 --bodyname IBM285 \
	--headername IBM285 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-285.ucm >Rare/CP20285.cs

${UCM2CP} --region Rare --page 20290 --wpage 932 \
	--name 'IBM EBCDIC (Japanese Katakana Extended)' \
	--webname IBM290 --bodyname IBM290 \
	--headername IBM290 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-290.ucm >Rare/CP20290.cs

${UCM2CP} --region Rare --page 20297 --wpage 1252 \
	--name 'IBM EBCDIC (France)' \
	--webname IBM297 --bodyname IBM297 \
	--headername IBM297 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-297.ucm >Rare/CP20297.cs

${UCM2CP} --region Rare --page 20420 --wpage 1256 \
	--name 'IBM EBCDIC (Arabic)' \
	--webname IBM420 --bodyname IBM420 \
	--headername IBM420 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-420.ucm >Rare/CP20420.cs

${UCM2CP} --region Rare --page 20424 --wpage 1255 \
	--name 'IBM EBCDIC (Hebrew)' \
	--webname IBM424 --bodyname IBM424 \
	--headername IBM424 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-424.ucm >Rare/CP20424.cs

${UCM2CP} --region Other --page 20866 --wpage 1251 \
	--name 'Cyrillic (KOI8-R)' \
	--webname koi8-r --bodyname koi8-r \
	--headername koi8-r Other/ibm-878.ucm >Other/CP20866.cs

${UCM2CP} --region Rare --page 20871 --wpage 1252 \
	--name 'IBM EBCDIC (Icelandic)' \
	--webname IBM871 --bodyname IBM871 \
	--headername IBM871 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-871.ucm >Rare/CP20871.cs

${UCM2CP} --region Rare --page 21025 --wpage 1257 \
	--name 'IBM EBCDIC (Cyrillic - Serbian, Bulgarian)' \
	--webname IBM1025 --bodyname IBM1025 \
	--headername IBM1025 --no-browser-display \
	--no-browser-save --no-mailnews-display \
	--no-mailnews-save Rare/ibm-1025.ucm >Rare/CP21025.cs

${UCM2CP} --region Other --page 21866 --wpage 1251 \
	--name 'Ukrainian (KOI8-U)' \
	--webname koi8-u --bodyname koi8-u \
	--headername koi8-u Other/koi8-u.ucm >Other/CP21866.cs

${UCM2CP} --region West --page 28592 --wpage 1250 \
	--name 'Central European (ISO)' \
	--webname iso-8859-2 --bodyname iso-8859-2 \
	--headername iso-8859-2 West/ibm-912.ucm >West/CP28592.cs

${UCM2CP} --region West --page 28593 --wpage 28593 \
	--name 'Latin 3 (ISO)' \
	--webname iso-8859-3 --bodyname iso-8859-3 \
	--headername iso-8859-3 West/ibm-913.ucm >West/CP28593.cs

${UCM2CP} --region Other --page 28594 --wpage 1257 \
	--name 'Baltic (ISO)' \
	--webname iso-8859-4 --bodyname iso-8859-4 \
	--headername iso-8859-4 Other/ibm-914.ucm >Other/CP28594.cs

${UCM2CP} --region Other --page 28595 --wpage 1251 \
	--name 'Cyrillic (ISO)' \
	--webname iso-8859-5 --bodyname iso-8859-5 \
	--headername iso-8859-5 Other/ibm-915.ucm >Other/CP28595.cs

${UCM2CP} --region MidEast --page 28596 --wpage 1256 \
	--name 'Arabic (ISO)' \
	--webname iso-8859-6 --bodyname iso-8859-6 \
	--headername iso-8859-6 MidEast/windows-28596-2000.ucm >MidEast/CP28596.cs

${UCM2CP} --region West --page 28597 --wpage 1253 \
	--name 'Greek (ISO)' \
	--webname iso-8859-7 --bodyname iso-8859-7 \
	--headername iso-8859-7 West/ibm-4909.ucm >West/CP28597.cs

${UCM2CP} --region MidEast --page 28598 --wpage 1255 \
	--name 'Hebrew (ISO)' \
	--webname iso-8859-8 --bodyname iso-8859-8 \
	--headername iso-8859-8 MidEast/ibm-916.ucm >MidEast/CP28598.cs

${UCM2CP} --region MidEast --page 28599 --wpage 1254 \
	--name 'Latin 5 (ISO)' \
	--webname iso-8859-9 --bodyname iso-8859-9 \
	--headername iso-8859-9 MidEast/ibm-920.ucm >MidEast/CP28599.cs

${UCM2CP} --region West --page 28605 --wpage 1252 \
	--name 'Latin 9 (ISO)' \
	--webname iso-8859-15 --bodyname iso-8859-15 \
	--headername iso-8859-15 --no-browser-display \
	West/ibm-923.ucm >West/CP28605.cs

${UCM2CP} --region MidEast --page 38598 --wpage 1255 \
	--name 'Hebrew (ISO Alternative)' \
	--webname windows-38598 --bodyname iso-8859-8 \
	--headername windows-38598 MidEast/ibm-916.ucm >MidEast/CP38598.cs

exit 0

# Windows code pages that are handled internally by "Encoding":
#
#  1200  Unicode
#  1201  Unicode (Big-Endian)
#  20127 US-ASCII
#  28591 Western European (ISO)
#  65000 Unicode (UTF-7)
#  65001 Unicode (UTF-8)
#
# Other Windows code pages that aren't done yet:
#
#  709   Arabic - ASMO 449+, BCON V4
#  710   Arabic - Transparent Arabic
#  720   Arabic - Transparent ASMO
#  936   Chinese Simplified (GB2312)
#  949   Korean
#  950   Chinese Traditional (Big5)
#  1361  Korean (Johab)
#  10001 MAC - Japanese
#  10002 MAC - Traditional Chinese (Big5)
#  10003 MAC - Korean
#  10004 MAC - Arabic
#  10005 MAC - Hebrew
#  10006 MAC - Greek I
#  10007 MAC - Cyrillic
#  10008 MAC - Simplified Chinese (GB 2312)
#  10010 MAC - Romania
#  10017 MAC - Ukraine
#  10021 MAC - Thai
#  10029 MAC - Latin II
#  10081 MAC - Turkish
#  10082 MAC - Croatia
#  12000 Unicode UCS-4 Little-Endian
#  12001 Unicode UCS-4 Big-Endian
#  20000 CNS - Taiwan
#  20001 TCA - Taiwan
#  20002 Eten - Taiwan
#  20003 IBM5550 - Taiwan
#  20004 TeleText - Taiwan
#  20005 Wang - Taiwan
#  20105 IA5 IRV International Alphabet No. 5 (7-bit)
#  20106 IA5 German (7-bit)
#  20107 IA5 Swedish (7-bit)
#  20108 IA5 Norwegian (7-bit)
#  20261 T.61
#  20269 ISO 6937 Non-Spacing Accent
#  20423 IBM EBCDIC - Greek
#  20833 IBM EBCDIC - Korean Extended
#  20838 IBM EBCDIC - Thai
#  20880 IBM EBCDIC - Cyrillic (Russian)
#  20905 IBM EBCDIC - Turkish
#  20924 IBM EBCDIC - Latin-1/Open System (1047 + Euro symbol)
#  20932 JIX X 0208-1990 & 0212-1990
#  20936 Simplified Chinese (GB2312)
#  21027 Extended Alpha Lowercase
#  21866 Ukrainian (KOI8-U)
#  29001 Europa 3
#  50220 Japanese (JIS)
#  50221 Japanese (JIS-Allow 1 byte Kana)
#  50222 Japanese (JIS-Allow 1 byte Kana - SO/SI)
#  50225 Korean (ISO)
#  50227 ISO-2022 Simplified Chinese
#  50229 ISO-2022 Traditional Chinese
#  50930 Japanese (Katakana) Extended
#  50931 US/Canada and Japanese
#  50933 Korean Extended and Korean
#  50935 Simplified Chinese Extended and Simplified Chinese
#  50936 Simplified Chinese
#  50937 US/Canada and Traditional Chinese
#  50939 Japanese (Latin) Extended and Japanese
#  51932 Japanese (EUC)
#  51936 Chinese Simplified (EUC)
#  51949 Korean (EUC)
#  51950 Traditional Chinese (EUC)
#  52936 Chinese Simplified (HZ)
#  54936 GB 18030-2000 Simplified Chinese
