REM LineNo: 38
REM ExpectedError: BC30580
REM ErrorMessage: '#ExternalSource' directives cannot be nested.

REM LineNo: 18
REM ExpectedError: BC30578
REM ErrorMessage: '#End ExternalSource' must be preceded by a matching '#ExternalSource'.

'Line 30, BC30580: ExternalSource Directives may not be nested

Imports System
Module ExternalDirectives
	Sub Main()
		#ExternalSource("/home/test.aspx",30)
            	#ExternalSource("/home/test.aspx",30)
		Console.WriteLine("In test.aspx")
		#End ExternalSource
		#End ExternalSource
	End Sub
End Module
