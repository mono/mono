REM LineNo: 21
REM ExpectedError: BC32025
REM ErrorMessage: '#Region' and '#End Region' statements are not valid within method bodies.

REM LineNo: 25
REM ExpectedError: BC30680
REM ErrorMessage: '#End Region' must be preceded by a matching '#Region'.

'Line 13, BC32025: '#Region' and '#End Region' directives cannot appear within method bodies
'Line 17, BC30680: '#End Region' directive must be preceded by a matching '#Region'


Imports System
Module RegionDirectives
	Sub Main()


	End Sub
	
	Sub S()
	#Region "S"


	End Sub
	#End Region
End Module

