Imports System
Module DoubleLiteral
	Sub Main()
		Try
			Dim a As Double=1.23
			Dim b As Double=1.23E+10
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
	End Sub
End Module
