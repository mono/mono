Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As Integer
		'Begin Code
			Try
				Dim a As Integer =  InStrRev ("World `Hello World", "Hello", 0)
				Throw New Exception ("#InStrRev01")
			Catch e As Exception
				If  (e.GetType ().ToString ()) <> "System.ArgumentException" Then
					Throw New Exception ("Expected  System.ArgumentException but got " + e.GetType ().ToString ())
				End If
			End Try
			
		'End Code
	End Function
End Class
