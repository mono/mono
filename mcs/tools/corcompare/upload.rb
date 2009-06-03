#!/usr/bin/env ruby

CURRENT = "2.6"

Dir["*.tar.gz"].each { |file|
	system("scp #{file} mono-web@mono.ximian.com:go-mono/masterinfos/#{CURRENT}")
}
