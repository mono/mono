s/prop\([0-9]\+\)/processorinfo\1/g
s/nspc\([0-9]\+\)/namespace\1/g
s/bool\([0-9]\+\)/boolean\1/g
s/outp\([0-9]\+\)/output\1/g
s/str\([0-9]\+\)/string\1/g
s/expr\([0-9]\+\)/expression\1/g
s/<file-path>Value-of<\/file-path>/<file-path>Valueof<\/file-path>/
s/\r//g
