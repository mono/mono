#!/bin/sh
charset=$1
echo Extracting $charset
mono table_from.exe $charset > $charset.TXT
mono table_to.exe $charset | sort > $charset.INVERSE.TXT
