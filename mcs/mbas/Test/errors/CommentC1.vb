REM LineNo: 12
REM ExpectedError: BC30451
REM ErrorMessage: Name 'comment' is not declared.

Imports System

Module M
	Sub Main()
		Dim b As IntegerREM : Dim c As Integer = 10
		Dim a As Integer = 10
		Console.WriteLine(a) 'Line Continuation within _
						 comment
	End Sub
End Module

Class B
End Class

Class IntegerREM : Inherits B
End Class
