Imports System
Module LongLiteral
	Sub Main()
		Try
			Dim a As Long
			a=True
			If a<>-1 Then
				Console.WriteLine("#A1:LongLiteralA:Failed")
			End If
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
		 Try
                	Dim a As Long
	                a=1.23
        	        If a<>1 Then
                	        Console.WriteLine("#A2:LongLiteralA:Failed")
	                End If
        	Catch e As Exception
                	Console.WriteLine(e.Message)
	        End Try
	End Sub
End Module
