Imports System
Module ExternalSourceDirective
	Sub Main()
		#ExternalSource("/home/main.aspx",30)
		Dim I As Integer
		'Compiler should report error in "/home/main.aspx" at line 32 	
		Dim B As Int	
		#End ExternalSource
	End Sub


	Sub A()
		#ExternalSource("/home/a.aspx",23)
		Dim I As Integer
		'Compiler should report error in "/home/a.aspx" at line 25
		Dim B As Int	
		#End ExternalSource		
	End Sub

	Sub C()
		Dim I As Integer
		'Compiler should report error in "ExternalSourceDirectivesC4.vb" at line 23
		Dim B As Int
	End Sub
End Module
