Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			Try
				SetAttr ("sample.bat.txt", vbReadOnly Or vbHidden)	
			Catch e As Exception
				Return e.GetType ().ToString ()
			End Try
			Throw New Exception ("Expected FileNotFoundException but got no exception")
		'End Code
	End Function
End Class
