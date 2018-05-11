SOURCES = \
	nuget-hash-extractor.cs

nuget-hash-extractor.exe: $(SOURCES)
	 mcs /r:System.Xml.Linq  /r:System.IO.Compression -o:nuget-hash-extractor.exe $(SOURCES)

.download_stamp_file: Makefile download.sh
	echo "Downloading all the nugets";	\
	./download.sh

download: .download_stamp_file

run: download nuget-hash-extractor.exe exec-ver exec-asm exec-msbuild

run-asm: download nuget-hash-extractor.exe exec-asm

exec-all:
	mono nuget-hash-extractor.exe nugets all

exec-asm:
	mono nuget-hash-extractor.exe nugets asm

exec-msbuild:
	mono nuget-hash-extractor.exe nugets guids_for_msbuild

run-ver: download nuget-hash-extractor.exe exec-ver

exec-ver:
	mono nuget-hash-extractor.exe nugets ver

.PHONY: download run
