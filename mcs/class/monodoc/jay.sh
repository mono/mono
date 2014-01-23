#!/bin/sh

TOPDIR=$1
INPUT=$2
OUTPUT=$3
FLAGS=$4

TEMPFILE=jay-tmp-$RANDOM.out

$TOPDIR/jay/jay $FLAGS < $TOPDIR/jay/skeleton.cs $INPUT > $TEMPFILE && mv $TEMPFILE $OUTPUT
