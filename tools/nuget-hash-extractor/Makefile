SOURCES = \
	nuget-hash-extractor.cs

nuget-hash-extractor.exe: $(SOURCES)
	 mcs /r:System.Xml.Linq  /r:System.IO.Compression -o:nuget-hash-extractor.exe $(SOURCES)

.download_stamp_file: Makefile download.sh
	echo "Downloading all the nugets";	\
	./download.sh

download: .download_stamp_file

run: nuget-hash-extractor.exe .download_stamp_file
	mono nuget-hash-extractor.exe nugets

run-asm: nuget-hash-extractor.exe .download_stamp_file
	mono nuget-hash-extractor.exe nugets asm

run-ver: nuget-hash-extractor.exe .download_stamp_file
	mono nuget-hash-extractor.exe nugets ver

.PHONY: download run
