Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			FileOpen (1, "5201.txt", OpenMode.Input)
			Try
				WriteLine (1, "Just a test")
			Catch e As Exception
				If (e.GetType ().ToString ()) <> "System.IO.IOException" Then
					Throw New Exception ("Expected IOException but got " + e.GetType ().ToString ())
					FileClose (1)
				End If 
				Return "Success"
			End Try
			Throw New Exception ("Expected IOException but got no exception")
		'End Code
	End Function
End Class
