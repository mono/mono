REM LineNo: 15
REM ExpectedError: BC32025
REM ErrorMessage: '#Region' and '#End Region' statements are not valid within method bodies.

REM LineNo: 17
REM ExpectedError: BC32025
REM ErrorMessage: '#Region' and '#End Region' statements are not valid within method bodies.

'Line 7, BC32025: '#Region' or '#End Region' directives cannot appear within a method body
'Line 9, BC32025: '#Region' or '#End Region' directives cannot appear within a method body

Imports System
Module RegionDirectives
	Sub Main()
		#Region

		#End Region	
	End Sub
End Module
