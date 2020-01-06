#!/usr/bin/python

#
# print-table.py: Print the function table for a webassembly .wast file
#

import sys

prefix=" (elem (i32.const 1) "

if len(sys.argv) < 2:
    print "Usage: python print-table.py <path to mono.wast>"
    sys.exit (1)

f = open (sys.argv [1])
table_line = None
for line in f:
     if prefix in line:
         table_line = line[len(prefix):]
         break
     
for (index, v) in enumerate (table_line.split (" ")):
    print "" + str(index) + ": " + v
    index += 1
