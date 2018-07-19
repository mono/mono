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
			*)
			# some lines in .sources may contain quick syntax to exclude files i.e.:
			# ../dir/*.cs:File1.cs,File2.cs (include everything except File1.cs and File2.cs)
			# let's drop that ":files" suffix
			for line in `echo $f | cut -d \: -f 1` ; do
				echo $line
			done
		esac
	done
	OIFS=$IFS
done

IFS=$OIFS

