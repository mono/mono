#!/bin/sh

# Remove comments from .sources files since this usage of IFS is unsuable inside make
#  (trouble with newlines)

source_files="$@"

OIFS=$IFS

for f in $source_files ; do
	IFS='
'
	for f in `cat $f` ; do
		case $f in
			\#*) ;;
			*) echo $f ;;
		esac
	done
	OIFS=$IFS
done

IFS=$OIFS

