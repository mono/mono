Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Try
				SetAttr ("sample.bat.txt", 10)	
			Catch e As Exception
				Return e.GetType ().ToString ()
			End Try
			Throw New Exception ("Expected ArgumentException but got no exception")
		'End Code
	End Function
End Class
