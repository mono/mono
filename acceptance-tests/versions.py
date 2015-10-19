#!/usr/bin/env python

import sys
import json

def find_module(submodules, name):
    for item in submodules:
        if item["name"] == name:
            return item

    print "Not found"
    sys.exit(1)


if len(sys.argv) < 2:
    print "Usage: versions.py <command>"
    sys.exit(1)

CONFIG_FILE = "SUBMODULES.json"
command = sys.argv[1]

submodules = json.load(open(CONFIG_FILE))

if command == "get-rev":
    mod = find_module(submodules, sys.argv[2])
    print mod["rev"]
elif command == "get-url":
    mod = find_module(submodules, sys.argv[2])
    print mod["url"]
elif command == "get-dir":
    mod = find_module(submodules, sys.argv[2])
    print mod["directory"]
elif command == "get-remote-branch":
    mod = find_module(submodules, sys.argv[2])
    print mod["remote-branch"]
elif command == "set-rev":
    mod = find_module(submodules, sys.argv[2])
    mod["rev"] = sys.argv[3]
    json.dump(submodules, open(CONFIG_FILE, "w"), indent = 2)
elif command == "set-branch":
    mod = find_module(submodules, sys.argv[2])
    mod["branch"] = sys.argv[3]
    json.dump(submodules, open(CONFIG_FILE, "w"), indent = 2)
elif command == "set-remote-branch":
    mod = find_module(submodules, sys.argv[2])
    mod["remote-branch"] = sys.argv[3]
    json.dump(submodules, open(CONFIG_FILE, "w"), indent = 2)
elif command == "cat":
    print json.dumps(submodules, indent = 2)
else:
    print "Unknown command "" + command + ""."
    sys.exit(1)
