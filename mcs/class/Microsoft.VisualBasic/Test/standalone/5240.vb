Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Dim a As New Sample ()
			Try
				FileOpen (1, "5240.txt", OpenMode.Output)
				PrintLine (1,a)
			Catch e As Exception
				Return e.GetType ().ToString ()
			End Try
			Throw New Exception ("Expected Argument Exception but got no exception!")
		'End Code
	End Function
End Class
Public Class Sample
End Class
