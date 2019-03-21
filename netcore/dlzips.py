#!/usr/bin/env python

import sys
import json
import subprocess

if len(sys.argv) < 3:
    print("Usage: dlzips.py testlist outdir")
    sys.exit(1)

infilename = sys.argv [1]
outdir = sys.argv [2]
testlist = json.load (open (infilename))
for item in testlist:
    print (item ['PayloadUri'])
    res = subprocess.call (["wget", "-P", outdir, str (item ['PayloadUri'])])
    if res != 0:
        print("Download failed.")
        sys.exit (1)




