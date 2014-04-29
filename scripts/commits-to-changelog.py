#!/usr/bin/python

from __future__ import print_function
from optparse import OptionParser
import subprocess
import re
import os.path
import fnmatch
import os
import sys

# subtract 8 for the leading tabstop
fill_column = 74 - 8

path_to_root = None

all_changelogs = {}

def git (command, *args):
    popen = subprocess.Popen (["git", command] + list (args), stdout = subprocess.PIPE)
    output = popen.communicate () [0]
    if popen.returncode != 0:
        print ("Error: git failed", file=sys.stderr)
        exit (1)
    return output

def changelog_path (changelog):
    global path_to_root
    if not path_to_root:
        path_to_root = git ("rev-parse", "--show-cdup").strip ()
    (pathname, filename) = changelog
    return path_to_root + "./" + pathname + "/" + filename

def changelog_for_file (filename):
    while filename != "":
        dirname = os.path.dirname (filename)
        if dirname in all_changelogs:
            return (dirname, all_changelogs [dirname])
        filename = dirname
    assert False

def changelogs_for_file_pattern (pattern, changed_files):
    changelogs = set ()
    for filename in changed_files:
        suffix = filename
        while suffix != "":
            # FIXME: fnmatch doesn't support the {x,y} pattern
            if fnmatch.fnmatch (suffix, pattern):
                changelogs.add (changelog_for_file (filename))
            (_, _, suffix) = suffix.partition ("/")
    return changelogs

def format_paragraph (paragraph):
    lines = []
    words = paragraph.split ()
    if len (words) == 0:
        return lines
    current = words [0]
    for word in words [1:]:
        if len (current) + 1 + len (word) <= fill_column:
            current += " " + word
        else:
            lines.append ("\t" + current)
            current = word
    lines.append ("\t" + current)
    return lines

def format_changelog_paragraph (files, paragraph):
    files_string = ""
    for (filename, entity) in files:
        if len (files_string) > 0:
            files_string += ", "
        files_string += filename
        if entity:
            files_string += " (" + entity + ")"
    return format_paragraph ("* " + files_string + ": " + paragraph)

def append_paragraph (lines, paragraph):
    if len (lines):
        lines.append ("")
    lines += paragraph

def format_changelog_entries (commit, changed_files, prefix, file_entries, all_paragraphs):
    changelogs = set ()
    for f in changed_files:
        changelogs.add (changelog_for_file (f))
    marked_changelogs = set ()

    author_line = git ("log", "-n1", "--date=short", "--format=%ad  %an  <%ae>", commit).strip ()

    paragraphs = {}
    for changelog in changelogs:
        paragraphs [changelog] = [author_line]

    for (files, comments) in file_entries:
        changelog_entries = {}
        for (filename, entity) in files:
            entry_changelogs = changelogs_for_file_pattern (filename, changed_files)
            if len (entry_changelogs) == 0:
                print ("Warning: could not match file {0} in commit {1}".format (filename, commit))
            for changelog in entry_changelogs:
                if changelog not in changelog_entries:
                    changelog_entries [changelog] = []
                changelog_entries [changelog].append ((filename, entity))
                marked_changelogs.add (changelog)

        for (changelog, files) in changelog_entries.items ():
            append_paragraph (paragraphs [changelog], format_changelog_paragraph (files, comments [0]))
            for paragraph in comments [1:]:
                append_paragraph (paragraphs [changelog], format_paragraph (paragraph))

    unmarked_changelogs = changelogs - marked_changelogs
    for changelog in unmarked_changelogs:
        if len (prefix) == 0:
            print ("Warning: empty entry in {0} for commit {1}".format (changelog_path (changelog), commit))
            insert_paragraphs = all_paragraphs
        else:
            insert_paragraphs = prefix
        for paragraph in insert_paragraphs:
            append_paragraph (paragraphs [changelog], format_paragraph (paragraph))

    return paragraphs

def debug_print_commit (commit, raw_message, prefix, file_entries, changed_files, changelog_entries):
    print ("===================== Commit")
    print (commit)
    print ("--------------------- RAW")
    print (raw_message)
    print ("--------------------- Prefix")
    for line in prefix:
        print (line)
    print ("--------------------- File entries")
    for (files, comments) in file_entries:
        files_str = ""
        for (filename, entity) in files:
            if len (files_str):
                files_str = files_str + ", "
            files_str = files_str + filename
            if entity:
                files_str = files_str + " (" + entity + ")"
        print files_str
        for line in comments:
            print ("  " + line)
    print ("--------------------- Files touched")
    for f in changed_files:
        print (f)
    print ("--------------------- ChangeLog entries")
    for ((dirname, filename), lines) in changelog_entries.items ():
        print ("{0}/{1}:".format (dirname, filename))
        for line in lines:
            print (line)

def process_commit (commit):
    changed_files = map (lambda l: l.split () [2], git ("diff-tree", "--numstat", commit).splitlines () [1:])
    if len (filter (lambda f: re.search ("(^|/)Change[Ll]og$", f), changed_files)):
        return None
    raw_message = git ("log", "-n1", "--format=%B", commit)
    # filter SVN migration message
    message = re.sub ("(^|\n)svn path=[^\n]+revision=\d+(?=$|\n)", "", raw_message)
    # filter ChangeLog headers
    message = re.sub ("(^|\n)\d+-\d+-\d+[ \t]+((\w|[.-])+[ \t]+)+<[^\n>]+>(?=$|\n)", "", message)
    # filter leading whitespace
    message = re.sub ("^\s+", "", message)
    # filter trailing whitespace
    message = re.sub ("\s+$", "", message)
    # paragraphize - first remove whitespace at beginnings and ends of lines
    message = re.sub ("[ \t]*\n[ \t]*", "\n", message)
    # paragraphize - now replace three or more consecutive newlines with two
    message = re.sub ("\n\n\n+", "\n\n", message)
    # paragraphize - replace single newlines with a space
    message = re.sub ("(?<!\n)\n(?!\n)", " ", message)
    # paragraphize - finally, replace double newlines with single ones
    message = re.sub ("\n\n", "\n", message)

    # A list of paragraphs (strings without newlines) that occur
    # before the first file comments
    prefix = []

    # A list of tuples of the form ([(filename, entity), ...], [paragraph, ...]).
    #
    # Each describes a file comment, containing multiple paragraphs.
    # Those paragraphs belong to a list of files, each with an
    # optional entity (usually a function name).
    file_entries = []

    current_files = None
    current_files_comments = None

    message_lines = message.splitlines ()
    for line in message_lines:
        if re.match ("\*\s[^:]+:", line):
            if current_files:
                file_entries.append ((current_files, current_files_comments))

            (files, _, comments) = line.partition (":")

            current_files_comments = [comments.strip ()]

            current_files = []
            for f in re.split ("\s*,\s*", files [1:].strip ()):
                m = re.search ("\(([^()]+)\)$", f)
                if m:
                    filename = f [:m.start (0)].strip ()
                    entity = m.group (1).strip ()
                else:
                    filename = f
                    entity = None
                current_files.append ((filename, entity))
        else:
            if current_files:
                current_files_comments.append (line)
            else:
                prefix.append (line)
    if current_files:
        file_entries.append ((current_files, current_files_comments))

    changelog_entries = format_changelog_entries (commit, changed_files, prefix, file_entries, message_lines)

    #debug_print_commit (commit, raw_message, prefix, file_entries, changed_files, changelog_entries)

    return changelog_entries

def start_changelog (changelog):
    full_path = changelog_path (changelog)
    old_name = full_path + ".old"
    os.rename (full_path, old_name)
    return open (full_path, "w")

def finish_changelog (changelog, file):
    old_file = open (changelog_path (changelog) + ".old")
    file.write (old_file.read ())
    old_file.close ()
    file.close ()

def append_lines (file, lines):
    for line in lines:
        file.write (line + "\n")
    file.write ("\n")

def main ():
    usage = "usage: %prog [options] <start-commit>"
    parser = OptionParser (usage)
    parser.add_option ("-r", "--root", dest = "root", help = "Root directory of the working tree to be changed")
    (options, args) = parser.parse_args ()
    if len (args) != 1:
        parser.error ("incorrect number of arguments")
    start_commit = args [0]

    if options.root:
        global path_to_root
        path_to_root = options.root + "/"

    # MonkeyWrench uses a shared git repo but sets BUILD_REVISION,
    # if present we use it instead of HEAD
    HEAD = "HEAD"
    if 'BUILD_REVISION' in os.environ:
        HEAD = os.environ['BUILD_REVISION']

    #see if git supports %B in --format
    output = git ("log", "-n1", "--format=%B", HEAD)
    if output.startswith ("%B"):
        print ("Error: git doesn't support %B in --format - install version 1.7.2 or newer", file=sys.stderr)
        exit (1)

    for filename in git ("ls-tree", "-r", "--name-only", HEAD).splitlines ():
        if re.search ("(^|/)Change[Ll]og$", filename):
            (path, name) = os.path.split (filename)
            all_changelogs [path] = name

    commits = git ("rev-list", "--no-merges", HEAD, "^{0}".format (start_commit)).splitlines ()

    touched_changelogs = {}
    for commit in commits:
        entries = process_commit (commit)
        if entries == None:
            continue
        for (changelog, lines) in entries.items ():
            if not os.path.exists (changelog_path (changelog)):
                continue
            if changelog not in touched_changelogs:
                touched_changelogs [changelog] = start_changelog (changelog)
            append_lines (touched_changelogs [changelog], lines)
    for (changelog, file) in touched_changelogs.items ():
        finish_changelog (changelog, file)

if __name__ == "__main__":
    main ()
