CSC=mcs

all: http-get-gzip.exe

http-get-gzip.exe: http-get-gzip.cs
	$(CSC) -out:$@ -r:Mono.Http.dll $<

clean:
	rm -f *.exe *~ *.pdb

