#!/usr/bin/env ruby

CURRENT = "2.8"

Dir["*.tar.gz"].each { |file|
	system("scp #{file} mono-web@go-mono.com:go-mono/masterinfos/#{CURRENT}")
}
