#!/bin/sh
#
# A little script to ease the pain of adding new resources to the test suite
#
genres ()  
{
    local line

    read line
    while test -n "$line"; do
	echo "\t$line,MonoTests.`echo $line | tr / . | cut -d '.' -f 2-`\t\\"
	read line
    done
}

find Test/WebPages -name "*" -type f | sort | genres
