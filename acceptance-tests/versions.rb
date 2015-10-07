#!/usr/bin/env ruby

require 'optparse'
require 'json'

def find_module(submodules, name)
  mod = submodules.find { |m| m['name'] == name }
  if mod == nil
    # FIXME:
    puts "Not found"
    exit 1
  end
  return mod
end
  
if ARGV.length < 1 then
  puts "Usage: versions.rb <command>"
  exit(1)
end

command = ARGV[0]

submodules = JSON.parse(File.read("SUBMODULES.json"))

case command
when "get-rev"
  mod = find_module(submodules, ARGV[1])
  puts mod['rev']
when "get-url"
  mod = find_module(submodules, ARGV[1])
  puts mod['url']
when "get-dir"
  mod = find_module(submodules, ARGV[1])
  puts mod['directory']
when "get-remote-branch"
  mod = find_module(submodules, ARGV[1])
  puts mod['remote-branch']
when "set-rev"
  mod = find_module(submodules, ARGV[1])
  mod['rev'] = ARGV[2]
  f = File.new("SUBMODULES.json", "w")
  f.write(JSON.pretty_generate(submodules))
  f.close()
when "set-branch"
  mod = find_module(submodules, ARGV[1])
  mod['branch'] = ARGV[2]
  f = File.new("SUBMODULES.json", "w")
  f.write(JSON.pretty_generate(submodules))
  f.close()
when "set-remote-branch"
  mod = find_module(submodules, ARGV[1])
  mod['remote-branch'] = ARGV[2]
  f = File.new("SUBMODULES.json", "w")
  f.write(JSON.pretty_generate(submodules))
  f.close()
when "cat"
  puts JSON.pretty_generate(submodules)
else
  puts "Unknown command '#{command}'."
  exit 1
end
