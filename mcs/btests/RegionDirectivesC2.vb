REM LineNo: 14
REM ExpectedError: BC30681
REM ErrorMessage: '#Region' statement must end with a matching '#End Region'.



Imports System
Module RegionDirectives
	Sub Main()


	End Sub

	#Region "S"
	Sub S()

	#End Region
	End Sub
End Module