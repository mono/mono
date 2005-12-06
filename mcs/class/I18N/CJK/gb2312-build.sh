#!/bin/sh

#
# NOTE: Mono doesn't use it anymore: GB2312.TXT vanished from unicode.org.
#

# Usage: gb2312-build.sh GB2312.TXT > gb2312.table
#
# Get the input file from ftp://ftp.unicode.org/Public/MAPPINGS/OBSOLETE/EASTASIA/GB/GB2312.TXT
#
# This algorithm was taken from the glibc iconv documentation in
# iconvdata/gb2312.c


# GB2312 to Unicode

egrep '^0x' $1 | perl -e \
'
@vals;
while (<>) {
    local($gb, $uni, %rest) = split;
    local($u)=hex($uni);
    local($g)=hex($gb);
    #printf("Setting vals[%d] to 0x%04x\n", int(($g - 0x2121) / 256) * 94 + (($g - 0x2121) & 0xff), $u);
    @vals[int(($g - 0x2121) / 256) * 94 + (($g - 0x2121) & 0xff)]=$u;
}
$size=($#vals+1)*2;
printf("\001\000\000\000%c%c%c%c", $size & 0xFF, ($size >> 8) & 0xFF, ($size >> 16) & 0xFF, ($size >> 24) & 0xFF);
for ($i=0; $i < $#vals+1; $i++) {
    printf("%c%c", $vals[$i] & 0xFF, ($vals[$i] >> 8) & 0xFF);
}
'

# Gb2312 from Unicode, table 1

perl -e \
'
@vals;
@vals[0x00]=0x21 + (0x68 << 8);
@vals[0x03]=0x21 + (0x6c << 8);
@vals[0x04]=0x21 + (0x27 << 8);
@vals[0x0c]=0x21 + (0x63 << 8);
@vals[0x0d]=0x21 + (0x40 << 8);
@vals[0x33]=0x21 + (0x41 << 8);
@vals[0x3c]=0x28 + (0x24 << 8);
@vals[0x3d]=0x28 + (0x22 << 8);
@vals[0x44]=0x28 + (0x28 << 8);
@vals[0x45]=0x28 + (0x26 << 8);
@vals[0x46]=0x28 + (0x3a << 8);
@vals[0x48]=0x28 + (0x2c << 8);
@vals[0x49]=0x28 + (0x2a << 8);
@vals[0x4e]=0x28 + (0x30 << 8);
@vals[0x4f]=0x28 + (0x2e << 8);
@vals[0x53]=0x21 + (0x42 << 8);
@vals[0x55]=0x28 + (0x34 << 8);
@vals[0x56]=0x28 + (0x32 << 8);
@vals[0x58]=0x28 + (0x39 << 8);
@vals[0x5d]=0x28 + (0x21 << 8);

$size=($#vals+1)*2;
printf("\002\000\000\000%c%c%c%c", $size & 0xFF, ($size >> 8) & 0xFF, ($size >> 16) & 0xFF, ($size >> 24) & 0xFF);
for ($i=0; $i < $#vals+1; $i++) {
    printf("%c%c", $vals[$i] & 0xFF, ($vals[$i] >> 8) & 0xFF);
}
'

# Gb2312 from Unicode, table 2

egrep '^0x' $1 | awk '{ print $2, $1 }' | sort | egrep '^0x03' | perl -e \
'
@vals;
while(<>) {
    local($uni, $gb, %rest) = split;
    local($u)=hex($uni);
    local($g)=hex($gb);
    #printf STDERR ("Setting vals[0x%04x] to 0x%04x\n", $u - 0x391, ($g < 0x100 ? $g : int($g/256)) + (($g < 0x100 ? 0 : $g&255) << 8));
    @vals[$u - 0x391]=($g < 0x100 ? $g : int($g/256)) + (($g < 0x100 ? 0 : $g&255) << 8);
}
$size=($#vals+1)*2;
printf("\003\000\000\000%c%c%c%c", $size & 0xFF, ($size >> 8) & 0xFF, ($size >> 16) & 0xFF, ($size >> 24) & 0xFF);
for ($i=0; $i < $#vals+1; $i++) {
    printf("%c%c", $vals[$i] & 0xFF, ($vals[$i] >> 8) & 0xFF);
}
'

# Gb2312 from Unicode, table 3

egrep '^0x' $1 | awk '{ print $2, $1 }' | sort | egrep '^0x04' | perl -e \
'
@vals;
while(<>) {
    local($uni, $gb, %rest) = split;
    local($u)=hex($uni);
    local($g)=hex($gb);
    #printf STDERR ("Setting vals[0x%04x] to 0x%04x\n", $u - 0x401, ($g < 0x100 ? $g : int($g/256)) + (($g < 0x100 ? 0 : $g&255) << 8));
    @vals[$u - 0x401]=($g < 0x100 ? $g : int($g/256)) + (($g < 0x100 ? 0 : $g&255) << 8);
}
$size=($#vals+1)*2;
printf("\004\000\000\000%c%c%c%c", $size & 0xFF, ($size >> 8) & 0xFF, ($size >> 16) & 0xFF, ($size >> 24) & 0xFF);
for ($i=0; $i < $#vals+1; $i++) {
    printf("%c%c", $vals[$i] & 0xFF, ($vals[$i] >> 8) & 0xFF);
}
'

# Gb2312 from Unicode, table 4

egrep '^0x' $1 | awk '{ print $2, $1 }' | sort | egrep '^0x20' | perl -e \
'
@vals;
while(<>) {
    local($uni, $gb, %rest) = split;
    local($u)=hex($uni);
    local($g)=hex($gb);
    #printf STDERR ("Setting vals[0x%04x] to 0x%04x\n", $u - 0x2015, ($g < 0x100 ? $g : int($g/256)) + (($g < 0x100 ? 0 : $g&255) << 8));
    @vals[$u - 0x2015]=($g < 0x100 ? $g : int($g/256)) + (($g < 0x100 ? 0 : $g&255) << 8);
}
$size=($#vals+1)*2;
printf("\005\000\000\000%c%c%c%c", $size & 0xFF, ($size >> 8) & 0xFF, ($size >> 16) & 0xFF, ($size >> 24) & 0xFF);
for ($i=0; $i < $#vals+1; $i++) {
    printf("%c%c", $vals[$i] & 0xFF, ($vals[$i] >> 8) & 0xFF);
}
'

# Gb2312 from Unicode, table 5

egrep '^0x' $1 | awk '{ print $2, $1 }' | sort | egrep '^0x2[12]' | perl -e \
'
@vals;
while(<>) {
    local($uni, $gb, %rest) = split;
    local($u)=hex($uni);
    local($g)=hex($gb);
    #printf STDERR ("Setting vals[0x%04x] to 0x%04x\n", $u - 0x2103, ($g < 0x100 ? $g : int($g/256)) + (($g < 0x100 ? 0 : $g&255) << 8));
    @vals[$u - 0x2103]=($g < 0x100 ? $g : int($g/256)) + (($g < 0x100 ? 0 : $g&255) << 8);
}
$size=($#vals+1)*2;
printf("\006\000\000\000%c%c%c%c", $size & 0xFF, ($size >> 8) & 0xFF, ($size >> 16) & 0xFF, ($size >> 24) & 0xFF);
for ($i=0; $i < $#vals+1; $i++) {
    printf("%c%c", $vals[$i] & 0xFF, ($vals[$i] >> 8) & 0xFF);
}
'


# Gb2312 from Unicode, table 6

egrep '^0x' $1 | awk '{ print $2, $1 }' | sort | egrep '^0x24' | perl -e \
'
@vals;
while(<>) {
    local($uni, $gb, %rest) = split;
    local($u)=hex($uni);
    local($g)=hex($gb);
    #printf STDERR ("Setting vals[0x%04x] to 0x%04x\n", $u - 0x2460, ($g < 0x100 ? $g : int($g/256)) + (($g < 0x100 ? 0 : $g&255) << 8));
    @vals[$u - 0x2460]=($g < 0x100 ? $g : int($g/256)) + (($g < 0x100 ? 0 : $g&255) << 8);
}
$size=($#vals+1)*2;
printf("\007\000\000\000%c%c%c%c", $size & 0xFF, ($size >> 8) & 0xFF, ($size >> 16) & 0xFF, ($size >> 24) & 0xFF);
for ($i=0; $i < $#vals+1; $i++) {
    printf("%c%c", $vals[$i] & 0xFF, ($vals[$i] >> 8) & 0xFF);
}
'


# Gb2312 from Unicode, table 7

egrep '^0x' $1 | awk '{ print $2, $1 }' | sort | egrep '^0x3[01]' | perl -e \
'
@vals;
while(<>) {
    local($uni, $gb, %rest) = split;
    local($u)=hex($uni);
    local($g)=hex($gb);
    #printf STDERR ("Setting vals[0x%04x] to 0x%04x\n", $u - 0x3000, ($g < 0x100 ? $g : int($g/256)) + (($g < 0x100 ? 0 : $g&255) << 8));
    @vals[$u - 0x3000]=($g < 0x100 ? $g : int($g/256)) + (($g < 0x100 ? 0 : $g&255) << 8);
}
$size=($#vals+1)*2;
printf("\010\000\000\000%c%c%c%c", $size & 0xFF, ($size >> 8) & 0xFF, ($size >> 16) & 0xFF, ($size >> 24) & 0xFF);
for ($i=0; $i < $#vals+1; $i++) {
    printf("%c%c", $vals[$i] & 0xFF, ($vals[$i] >> 8) & 0xFF);
}
'

# Gb2312 from Unicode, table 8

egrep '^0x' $1 | awk '{ print $2, $1 }' | sort | egrep '^0x[4-9]' | perl -e \
'
@vals;
while(<>) {
    local($uni, $gb, %rest) = split;
    local($u)=hex($uni);
    local($g)=hex($gb);
    printf STDERR ("Setting vals[0x%04x] to 0x%04x\n", $u - 0x4e00, ($g < 0x100 ? $g : int($g/256)) + (($g < 0x100 ? 0 : $g&255) << 8));
    @vals[$u - 0x4e00]=($g < 0x100 ? $g : int($g/256)) + (($g < 0x100 ? 0 : $g&255) << 8);
}
$size=($#vals+1)*2;
printf("\011\000\000\000%c%c%c%c", $size & 0xFF, ($size >> 8) & 0xFF, ($size >> 16) & 0xFF, ($size >> 24) & 0xFF);
for ($i=0; $i < $#vals+1; $i++) {
    printf("%c%c", $vals[$i] & 0xFF, ($vals[$i] >> 8) & 0xFF);
}
'

# Gb2312 from Unicode, table 9

egrep '^0x' $1 | awk '{ print $2, $1 }' | sort | egrep '^0xFF[0-5]' | perl -e \
'
@vals;
while(<>) {
    local($uni, $gb, %rest) = split;
    local($u)=hex($uni);
    local($g)=hex($gb);
    #printf STDERR ("Setting vals[0x%04x] to 0x%04x\n", $u - 0xff01, ($g < 0x100 ? $g : int($g/256)) + (($g < 0x100 ? 0 : $g&255) << 8));
    @vals[$u - 0xff01]=($g < 0x100 ? $g : int($g/256)) + (($g < 0x100 ? 0 : $g&255) << 8);
}
$size=($#vals+1)*2;
printf("\012\000\000\000%c%c%c%c", $size & 0xFF, ($size >> 8) & 0xFF, ($size >> 16) & 0xFF, ($size >> 24) & 0xFF);
for ($i=0; $i < $#vals+1; $i++) {
    printf("%c%c", $vals[$i] & 0xFF, ($vals[$i] >> 8) & 0xFF);
}
'


