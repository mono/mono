#!/usr/bin/env python3

import sys
import json

def find_module(submodules, name):
    for item in submodules:
        if item["name"] == name:
            return item

    print("Not found")
    sys.exit(1)


if len(sys.argv) < 3:
    print("Usage: versions.py <path to SUBMODULES.json> <command>")
    sys.exit(1)

CONFIG_FILE = sys.argv[1]
command = sys.argv[2]

submodules = json.load(open(CONFIG_FILE))

if command == "get-rev":
    mod = find_module(submodules, sys.argv[3])
    print(mod["rev"])
elif command == "get-url":
    mod = find_module(submodules, sys.argv[3])
    print(mod["url"])
elif command == "get-dir":
    mod = find_module(submodules, sys.argv[3])
    print(mod["directory"])
elif command == "get-remote-branch":
    mod = find_module(submodules, sys.argv[3])
    print(mod["remote-branch"])
elif command == "set-rev":
    mod = find_module(submodules, sys.argv[3])
    mod["rev"] = sys.argv[4]
    json.dump(submodules, open(CONFIG_FILE, "w"), indent = 2)
elif command == "set-branch":
    mod = find_module(submodules, sys.argv[3])
    mod["branch"] = sys.argv[4]
    json.dump(submodules, open(CONFIG_FILE, "w"), indent = 2)
elif command == "set-remote-branch":
    mod = find_module(submodules, sys.argv[3])
    mod["remote-branch"] = sys.argv[4]
    json.dump(submodules, open(CONFIG_FILE, "w"), indent = 2)
elif command == "cat":
    print(json.dumps(submodules, indent = 2))
else:
    print("Unknown command "" + command + "".")
    sys.exit(1)
