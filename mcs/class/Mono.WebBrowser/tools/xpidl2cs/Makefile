all: clean make-interfaces

clean: 
	rm -f *.cs

make-interfaces:
	./xpidl2cs.pl nsIDOMHTMLDocument.idl idl/
	./xpidl2cs.pl nsIWebNavigation.idl idl/
	./xpidl2cs.pl nsIDOMHTMLDocument.idl idl/
	./xpidl2cs.pl nsIDocCharset.idl idl/
	./xpidl2cs.pl nsIInterfaceRequestor.idl idl/
	./xpidl2cs.pl nsIDOMDocumentView.idl idl/
	./xpidl2cs.pl nsIDOMNSHTMLElement.idl idl/
	./xpidl2cs.pl nsIDOMHTMLBodyElement.idl idl/
	./xpidl2cs.pl nsIDOMDocumentRange.idl idl/
	./xpidl2cs.pl nsIWebBrowserFocus.idl idl/
	./xpidl2cs.pl nsIWebBrowserChromeFocus.idl idl/
