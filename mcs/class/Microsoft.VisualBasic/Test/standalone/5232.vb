Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			'new_dir containing files
			Try
				Rmdir ("new_dir1")	
			Catch e As Exception
				Return e.GetType ().ToString ()
			End Try
			Throw New Exception ("Expected IOException but got no exception")
		'End Code
	End Function
End Class
