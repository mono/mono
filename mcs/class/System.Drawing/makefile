all: windows

windows:
	$(CSC) /target:library /out:System.Drawing.dll /nowarn:1595 /recurse:*.cs

clean:
	if test -e System.dll; then \
		rm System.Drawing.dll;  \
	fi; \