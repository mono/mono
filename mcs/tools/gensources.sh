#!/usr/bin/env bash

includefile=$1
excludefile=$2

## input variables:
## 	$filelist:
##		A colon (':') separated list of files already read.
##		Must be initialized to ":".
##	$excludelist:
##		A newline separated of element (support Shell Patterns) to exclude.
##	$separator:
##		The separator used in list for the output
##	
## output variables:
## 	$list:
##		A list of elements separated by the separator given in $separator.
##		The new elements will be appended to the list.
readlist () {
	local onelist
	local onelistcontent
	onelist=$1

	if [ ":$onelist:" = "::" ] ; then return ; fi
	if [ ! -f $onelist ] ; then return ; fi
	if [ ":${filelist##*:$onelist:*}:" = "::" ]  ; then return ; fi
	filelist=":$onelist$filelist"

	onelistcontent=`cat $onelist | sed "s=[ \t]*$==g" | while read line ; do echo -n $line ; echo -n ":" ; done`

	OFS="$IFS"
	IFS=":"
	for line in $onelistcontent ; do
		line2=${line##\#}
		if [ ":$line:" = ":$line2:" ] ; then
			for linex in $excludelist ; do
				if [ ":${line##$linex}:" = "::" ] ; then line="" ; fi
			done
			if [ ":$line:" != "::" ] ; then
				if [ ":$list:" = "::" ] ; then
					list="$line"
				else
					list="$list$separator$line"
				fi
			fi
		else
			line3=${line2##include }
			if [ ":$line3:" != ":$line2:" -a ":$line3:" != "::" ] ; then
				readlist "$line3"
			fi
		fi
	done
	IFS="$OFS"
}

list=""
filelist=":"
excludelist=""
separator=":"
readlist "$excludefile"

excludelist="$list"
list=""
filelist=":"
separator="
"
readlist "$includefile"
echo "$list" | sort | uniq
