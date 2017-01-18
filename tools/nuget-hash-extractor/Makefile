SOURCES = \
	nuget-hash-extractor.cs

nuget-hash-extractor.exe: $(SOURCES)
	 mcs /r:System.Xml.Linq  /r:System.IO.Compression -o:nuget-hash-extractor.exe $(SOURCES)

download:
	echo "Downloading all the nugets";	\
	./download.sh

run: nuget-hash-extractor.exe
	mono nuget-hash-extractor.exe nugets
.PHONY: download run
