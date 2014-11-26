#!/usr/bin/env python
import matplotlib.pyplot as plt
from matplotlib.dates import DateFormatter, MinuteLocator, SecondLocator
import numpy as np
from StringIO import StringIO
import os
import re
import sys
from optparse import OptionParser
import subprocess

parser = OptionParser (usage = "Usage: %prog [options] BINARY-PROTOCOL")
parser.add_option ('--histogram', action = 'store_true', dest = 'histogram', help = "pause time histogram")
parser.add_option ('--minor', action = 'store_true', dest = 'minor', help = "only show minor collections in histogram")
parser.add_option ('--major', action = 'store_true', dest = 'major', help = "only show major collections in histogram")
(options, files) = parser.parse_args ()

show_histogram = False
show_minor = True
show_major = True
if options.minor:
    show_histogram = True
    show_major = False
if options.major:
    show_histogram = True
    show_minor = False
if options.histogram:
    show_histogram = True

script_path = os.path.realpath (__file__)
sgen_grep_path = os.path.join (os.path.dirname (script_path), 'sgen-grep-binprot')

if not os.path.isfile (sgen_grep_path):
    sys.stderr.write ('Error: `%s` does not exist.\n' % sgen_grep_path)
    sys.exit (1)

if len (files) != 1:
    parser.print_help ()
    sys.exit (1)

data = []

grep_input = open (files [0])
proc = subprocess.Popen ([sgen_grep_path, '--pause-times'], stdin = grep_input, stdout = subprocess.PIPE)
for line in iter (proc.stdout.readline, ''):
    m = re.match ('^pause-time (\d+) (\d+) (\d+) (\d+) (\d+)', line)
    if m:
        minor_work = major_work = False
        generation = int (m.group (1))
        concurrent = int (m.group (2)) != 0
        finish = int (m.group (3)) != 0
        msecs = int (m.group (4)) / 10.0 / 1000.0
        start = int (m.group (5)) / 10.0 / 1000.0

        if concurrent:
            kind = "CONC"
        else:
            kind = "SYNC"

        if generation == 0:
            minor_work = True
            if concurrent:
                major_work = True
                gctype = "nursery+update"
            else:
                gctype = "nursery"
        else:
            major_work = True
            if concurrent:
                if finish:
                    minor_work = True
                    gctype = "nursery+finish"
                else:
                    gctype = "start"
            else:
                gctype = "full"

        rec = (minor_work, major_work, start, start + msecs, kind, gctype)
        print rec
        data.append (rec)

if show_histogram:
    minor_pausetimes = []
    major_pausetimes = []

    for rec in data:
        (minor_work, major_work, start_ts, finish_ts, kind, gctype) = rec
        pause = finish_ts - start_ts
        if minor_work and major_work and show_minor and show_major:
            major_pausetimes.append (pause)
        else:
            if minor_work:
                minor_pausetimes.append (pause)
            if major_work:
                major_pausetimes.append (pause)

    pausetimes = []
    if show_minor:
        pausetimes += minor_pausetimes
    if show_major:
        pausetimes += major_pausetimes
    plt.hist (pausetimes, 100)
    plt.xlabel ('Pause time in msec')
else:
    data = np.array (data, dtype = [('caption', '|S20'), ('start', int), ('stop', int), ('kind', '|S20')])
    cap, start, stop=data['caption'], data['start'], data['stop']

    #Check the status, because we paint all lines with the same color
    #together
    is_sync= (data['kind']=='SYNC')
    not_sync=np.logical_not(is_sync)

    #Get unique captions and there indices and the inverse mapping
    captions, unique_idx, caption_inv=np.unique(cap, 1,1)
    print captions

    #Build y values from the number of unique captions.
    y=(caption_inv+1)/float(len(captions)+1)

    #Plot function
    def timelines(y, xstart, xstop,color='b'):
        """Plot timelines at y from xstart to xstop with given color."""
        plt.hlines(y,xstart,xstop,color,lw=4)
        plt.vlines(xstart, y+0.03,y-0.03,color,lw=2)
        plt.vlines(xstop, y+0.03,y-0.03,color,lw=2)

    #Plot ok tl black
    timelines(y[is_sync],start[is_sync],stop[is_sync],'r')
    #Plot fail tl red
    timelines(y[not_sync],start[not_sync],stop[not_sync],'k')

    #Setup the plot
    ax=plt.gca()
    #ax.xaxis_date()
    #myFmt = DateFormatter('%H:%M:%S')
    #ax.xaxis.set_major_formatter(myFmt)
    #ax.xaxis.set_major_locator(SecondLocator(0,interval=20))

    #To adjust the xlimits a timedelta is needed.
    delta=(stop.max()-start.min())/10

    plt.yticks(y[unique_idx],captions)
    plt.ylim(0,1)
    plt.xlim(start.min()-delta, stop.max()+delta)
    plt.xlabel('Time')

plt.show()
