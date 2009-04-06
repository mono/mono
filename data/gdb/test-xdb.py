# Tester script for the xdb functionality
# Run using gdb -P test-xdb.py <NORMAL GDB COMMAND LINE>

import sys

gdb.execute ("file %s" % sys.argv [0])
gdb.execute ("r --break *:* %s" % " ".join (sys.argv[1:len(sys.argv)]))

while True:
	try:
		if gdb.threads () == None:
			break
		gdb.execute("xdb")
		gdb.execute("bt")
		gdb.execute("info args")
		gdb.execute("info locals")
		gdb.execute("c")
	except:
		gdb.execute ("quit")
