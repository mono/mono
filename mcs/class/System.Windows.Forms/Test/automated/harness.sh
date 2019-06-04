#!/bin/bash

mode=$1
subdir=$2
XNEST_DISPLAY=:5

if [ x$mode != "xbaseline" -a x$mode != "xtest" ] ; then
    echo unknown mode: $mode
    exit 1
fi


cd $subdir

Xnest -once -geometry 1000x700 -ac $XNEST_DISPLAY >/dev/null 2>&1 &

echo Running tests in $subdir

# start up the test and give it some time to appear
export DISPLAY=$XNEST_DISPLAY
mono test.exe 2> /dev/null &
sleep 3

steps=`ls *.xnr 2> /dev/null`

for i in 0 $steps; do
    step=`basename $i .xnr`

    resultprefix=result-$step
    resultfile=$resultprefix.xwd
    resultpng=$resultprefix.png
    baseprefix=$step
    baseline=$baseprefix.xwd
    basepng=$baseprefix.png
    differencespng=differences-$step.png

    if test -f $i; then
	# replay the recorded data
	#/opt/xnee/bin/cnee -display $XNEST_DISPLAY --replay -f $i -rwp > /dev/null 2>&1
	/opt/xnee/bin/cnee -display $XNEST_DISPLAY --replay -f $i > /dev/null 2>&1
	#sleep 1
    fi

    if test x$mode == "xbaseline"; then
	echo -n "    generating baseline for step $step..."
	# take a screendump and store out the new baseline
	xwd -silent -display $XNEST_DISPLAY -root -out $baseline > /dev/null
	xwdtopnm $baseline 2> /dev/null | pnmtopng -compression 9 2> /dev/null > $basepng
	rm -f $baseline
	echo done.
    elif test x$mode == "xtest"; then
	echo -n "    step $step..."

	rm -f $resultpng $differencespng

	# take a screendump of the end result
	xwd -silent -display $XNEST_DISPLAY -root -out $resultfile 2> /dev/null
	xwdtopnm $resultfile 2> /dev/null | pnmtopng -compression 9 2> /dev/null > $resultpng
	rm -f $resultfile

	# and compare to our baseline
	if diff $resultpng $basepng; then
	    echo PASSED.
	    rm -f $resultpng
	else
	    echo FAILED.
	    convert $resultpng $basepng -compose difference -composite $differencespng
	fi
    fi
done

# kill Xnest
kill %1
