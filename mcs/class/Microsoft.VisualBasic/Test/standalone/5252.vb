Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code	
			Try
				Dim a As Long =  LOF (-1)
			Catch e As Exception
				Return e.GetType ().ToString ()
			End Try
			Throw New Exception ("Expected IOException but got no exception")
		'End Code
	End Function
End Class
Public Class Sample
End Class
