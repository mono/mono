#!/usr/bin/sh

outfile=$1
infile=$2
namespace=$3

echo "namespace $3 {" > $1 || exit
echo "\tstatic partial class Res {" >> $1 || exit
sed -e 's/^;.*//' -e 's/doesn..t/doesn''t/' -e 's/`echo "\0222"`/''/' -e 's/\"/\"\"/g' -e 's/^\([_0-9a-ZA-Z]*\)=\(.*\)/public const string \1 = @"\2";/' $2 >> $1 || exit
echo "}" >> $1 || exit
echo "}" >> $1 || exit
