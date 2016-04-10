#! /bin/sh

outfile=$1
incfile=$2
excfile=$3
extfile=$4
extexcfile=$5

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
	process_includes $extfile $outfile.ext.inc
	cat $outfile.ext.inc >> $outfile.inc
	rm -f $outfile.ext.inc
fi

sort -u $outfile.inc > $outfile.inc_s
rm -f $outfile.inc


if test -n "$excfile" -a -f "$excfile"; then
    process_includes $excfile $outfile.exc
fi

if test -n "$extexcfile"; then
    process_includes $extexcfile $outfile.ext_exc
	cat $outfile.ext_exc >> $outfile.exc
	rm -f $outfile.ext_exc
fi

if test -f $outfile.exc; then
	# So what we're doing below with uniq -u is that we take 
	# lines that have not been duplicated. This computes the 
	# symmetric difference between the files. This is not
	# what we want. If a file is in the excludes but not in
	# the sources, we want that file not to show up. By duplicating the
	# excludes, we ensure that we won't end up in this failure state.
	sort -u $outfile.exc > $outfile.exc_s

	# Duplicate excludes
	cat $outfile.exc_s >> $outfile.exc_s_dup
	cat $outfile.exc_s >> $outfile.exc_s_dup

	rm -f $outfile.exc $outfile.exc_s

	cat $outfile.inc_s $outfile.exc_s_dup | sort | uniq -u > $outfile
	rm -f $outfile.inc_s $outfile.exc_s_dup
else
	mv $outfile.inc_s $outfile
fi


