Imports System
Module LongLiteral
	Sub Main()
			Dim a As Long
			a=True
			If a<>-1 Then
				Throw new System.Exception("#A1:LongLiteralA:Failed")
			End If
                  a=1.23
                  If a<>1 Then
                	      Throw new System.Exception("#A2:LongLiteralA:Failed")
	            End If
	End Sub
End Module
