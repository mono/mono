CSC=mcs

all: HtmlWriter.dll

HtmlWriter.dll: HtmlWriter.cs
	$(CSC) -t:library -r:System.Web.dll $<

clean:
	rm -f HtmlWriter.dll HtmlWriter.*db

