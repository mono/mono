Imports System
Module SingleLiteral
	Sub Main()
		Try
			Dim a As Single=1.23
			Dim b As Single=1.23E+10
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
	End Sub
End Module
