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

