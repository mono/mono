Imports System
Module LongLiteral
	Sub Main()
		Try
			Dim a As Long
			a="Hello"
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
	End Sub
End Module
