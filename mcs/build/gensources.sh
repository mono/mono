#! /bin/sh

outfile=$1
incfile=$2
excfile=$3
extfile=$4

process_includes_1() {
    sed -e '/^[ \t]*$/d' -e '/^[ \t]*#/d' $1 > $2
    if cmp -s $1 $2; then
	false
    else
	sed -n 's,^[ \t]*#include ,,p' $1 |
	while read inc; do
	    cat $inc >> $2
	    echo $outfile: $inc >> $outfile.makefrag
	    echo $inc: >> $outfile.makefrag
	done
    fi
}

process_includes() {
    i=$1; o=$2; t=${2}.tmp
    while process_includes_1 $i $o; do
	mv $o $t
	i=$t
    done
    rm -f $t
}

rm -f $outfile.makefrag

process_includes $incfile $outfile.inc

if test x$extfile != x -a -f "$extfile"; then
	cat $extfile >> $outfile.inc
fi

sort -u $outfile.inc > $outfile.inc_s
rm -f $outfile.inc

if test -z "$excfile"; then
    mv $outfile.inc_s $outfile
else
    process_includes $excfile $outfile.exc

    sort -u $outfile.exc > $outfile.exc_s
    rm -f $outfile.exc

    sort -m $outfile.inc_s $outfile.exc_s | uniq -u > $outfile
    rm -f $outfile.inc_s $outfile.exc_s
fi


