Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass
	Public Function Test() As String
		'Begin Code
			Try
				Dim d As double = NPV (0.5,nothing)	
				Throw New Exception ("#NPV1")
			Catch ex As Exception	
				If (ex.GetType ().ToString () <> "System.ArgumentException")
					Throw New Exception ("#NPV2")
				End If
			End Try
			
		'End Code
	End Function
End Class