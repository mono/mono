'BC32025: '#Region' and '#End Region' directives cannot appear within method bodies
'BC30680: '#End Region' directive must be preceded by a matching '#Region'


Imports System
Module RegionDirectives
	#Region "Main"
	Sub Main()


	End Sub
	
	#Region "S"
	Sub S()



	End Sub
	#End Region

	#End Region
End Module

