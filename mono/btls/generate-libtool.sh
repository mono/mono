#!/bin/sh

template=$1
lafile=$2
name=$3
dlname=$4
libdir=$5
library_names=$6
old_library=$7
target_current=$8
target_age=$9
target_revision=${10}
target_installed=${11}

sed -e "s,@lafile@,$lafile,g" -e "s,@name@,$name,g" -e "s,@dlname@,$dlname,g" -e "s,@libdir@,$libdir,g" \
	-e "s,@library_names@,$library_names,g" -e "s,@old_library@,$old_library,g" \
	-e "s,@target_current@,$target_current,g" -e "s,@target_age@,$target_age,g" -e "s,@target_revision@,$target_revision,g" \
	-e "s,@target_installed@,$target_installed,g" \
		 < $template > $lafile

