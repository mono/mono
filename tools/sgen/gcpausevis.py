#!/usr/bin/env python3
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
parser.add_option ('--scatter', action = 'store_true', dest = 'scatter', help = "pause time scatterplot")
parser.add_option ('--minor', action = 'store_true', dest = 'minor', help = "only show minor collections in histogram")
parser.add_option ('--major', action = 'store_true', dest = 'major', help = "only show major collections in histogram")
(options, files) = parser.parse_args ()

show_histogram = False
show_scatter = False
show_minor = True
show_major = True
if options.minor:
    show_major = False
if options.major:
    show_minor = False
if options.histogram:
    show_histogram = True
if options.scatter:
    show_scatter = True
if (options.minor or options.major) and not options.scatter:
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

class Event:
    def __init__(self, **kwargs):
        self.minor_work = kwargs['minor_work']
        self.major_work = kwargs['major_work']
        self.start = kwargs['start']
        self.stop = kwargs['stop']
        self.gc_type = kwargs['gc_type']
    def __repr__(self):
        return 'Event(minor_work={}, major_work={}, start={}, stop={}, gc_type={})'.format(
            self.minor_work,
            self.major_work,
            self.start,
            self.stop,
            self.gc_type,
        )

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
                gc_type = "nursery+update"
            else:
                gc_type = "nursery"
        else:
            major_work = True
            if concurrent:
                if finish:
                    minor_work = True
                    gc_type = "nursery+finish"
                else:
                    gc_type = "start"
            else:
                gc_type = "full"

        rec = Event(
            minor_work=minor_work,
            major_work=major_work,
            start=start,
            stop=start + msecs,
            kind=kind,
            gc_type=gc_type,
        )
        print rec
        data.append (rec)

class MajorGCEventGroup:
    pass

class FullMajorGCEventGroup(MajorGCEventGroup):
    def __init__(self, event):
        self.event = event
    def __repr__(self):
        return 'FullMajorGCEventGroup({})'.format(
            self.event,
        )

class ConcurrentMajorGCEventGroup(MajorGCEventGroup):
    def __init__(self, start, updates, finish):
        self.start = start
        self.updates = updates
        self.finish = finish
    def __repr__(self):
        return 'ConcurrentMajorEventGroup({}, {}, {})'.format(
            self.start,
            self.updates,
            self.finish,
        )

# ([Event], int) -> (MajorGCEventGroup, int) | None
def parse_next_major_gc(data, i):
    assert i >= 0
    # Find start or full event.
    while i < len(data) and data[i].gc_type not in ['start', 'full', 'nursery+update']:
        i += 1
    if i == len(data):
        return None
    # If full event, done.
    if data[i].gc_type == 'full':
        return (FullMajorGCEventGroup(data[i]), i + 1)
    start_event = data[i]
    update_events = []
    # Filter update events and find finish event.
    while i < len(data) and data[i].gc_type != 'nursery+finish':
        if data[i].gc_type == 'nursery+update':
            update_events.append(data[i])
        i += 1
    if i == len(data):
        return None
    finish_event = data[i]
    i += 1
    return (ConcurrentMajorGCEventGroup(start_event, update_events, finish_event), i)

# [Event] -> [MajorGCEventGroup]
def parse_major_gcs(data):
    major_gc_events = []
    i = 0
    while True:
        maybe_event_group = parse_next_major_gc(data, i)
        if maybe_event_group is None:
            return major_gc_events
        event_group, i = maybe_event_group
        major_gc_events.append(event_group)

if show_histogram or show_scatter:
    bin_data_minor = []
    bin_data_both = []
    bin_data_major = []
    bin_names = []

    timeline_x = []
    timeline_y = []
    timeline_c = []

    for rec in data:
        pause = rec.stop - rec.start

        color = None
        if rec.major_work:
            if rec.minor_work:
                color = 'purple'
            else:
                color = 'red' if show_major else None
        else:
            color = 'blue' if show_minor else None

        if color:
            timeline_x.append(rec.start)
            timeline_y.append(pause)
            timeline_c.append(color)

        for i in range(100):
            time = (1.3)**(i+6)
            prev_time = 0 if i==0 else (1.3)**(i+5)
            if len(bin_names) <= i:
                bin_data_minor.append(0)
                bin_data_both.append(0)
                bin_data_major.append(0)
                bin_names.append('%d-%dms' % (int(prev_time), int(time)))
            if pause <= time:
                if rec.major_work:
                    if rec.minor_work:
                        bin_data_both[i] += pause
                    else:
                        bin_data_major[i] += pause
                else:
                    bin_data_minor[i] += pause
                break

    bin_data_minor=np.array(bin_data_minor)
    bin_data_both=np.array(bin_data_both)
    bin_data_major=np.array(bin_data_major)

    if show_scatter:
        plt.scatter(timeline_x, timeline_y, c=timeline_c)
    else:
        if show_minor:
            plt.bar(range(len(bin_data_minor)), bin_data_minor, color='blue', label="minor")  #, align='center')
            plt.bar(range(len(bin_data_both)), bin_data_both, bottom=bin_data_minor, color='purple', label="minor & major")
            if show_major:
                plt.bar(range(len(bin_data_major)), bin_data_major, bottom=(bin_data_minor+bin_data_both), color='red', label="only major")
        else:
            plt.bar(range(len(bin_data_both)), bin_data_both, color='purple', label="minor & major")
            plt.bar(range(len(bin_data_major)), bin_data_major, bottom=bin_data_both, color='red')
        plt.xticks(range(len(bin_names)), bin_names)
        plt.ylabel('Cumulative time spent in GC pauses (ms)')
        plt.xlabel('GC pause length')
        plt.xticks(rotation=60)
        plt.legend(loc='upper left')
else:
    major_gc_event_groups = parse_major_gcs(data)

    def bar(**kwargs):
        indices = kwargs['indices']
        pauses = kwargs['pauses']
        color = kwargs['color']
        if 'bottom' in kwargs:
            bottom = kwargs['bottom']
        else:
            bottom = 0
        plt.bar(
            [index for index in indices if pauses[index] is not None],
            np.array([pause for pause in pauses if pause is not None]),
            color=color,
            bottom=bottom,
        )

    indices = np.arange(len(major_gc_event_groups))
    start_pauses = [
        event_group.start.stop - event_group.start.start
        if isinstance(event_group, ConcurrentMajorGCEventGroup) else None
        for event_group in major_gc_event_groups
    ]
    bar(
        indices=indices,
        pauses=start_pauses,
        color='red',
    )
    update_pauses = [
        sum([
            update_event.stop - update_event.start
            for update_event in event_group.updates
        ]) if isinstance(event_group, ConcurrentMajorGCEventGroup) else None
        for event_group in major_gc_event_groups
    ]
    bar(
        indices=indices,
        pauses=update_pauses,
        color='green',
        bottom=[pause for pause in start_pauses if pause is not None],
    )
    finish_pauses = [
        event_group.finish.stop - event_group.finish.start
        if isinstance(event_group, ConcurrentMajorGCEventGroup) else None
        for event_group in major_gc_event_groups
    ]
    start_update_pauses = [
        a + b
        for a, b in zip(start_pauses, update_pauses)
        if a is not None and b is not None
    ]
    bar(
        indices=indices,
        pauses=finish_pauses,
        color='blue',
        bottom=start_update_pauses,
    )
    full_pauses = [
        event_group.event.stop - event_group.event.start
        if isinstance(event_group, FullMajorGCEventGroup) else None
        for event_group in major_gc_event_groups
    ]
    bar(
        indices=indices,
        pauses=full_pauses,
        color='black',
    )

    plt.ylabel("Pause Time (ms)")
    plt.xlabel("Collection")

plt.show()
