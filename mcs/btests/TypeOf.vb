Imports System

Module M
	Class Test1
	End Class

        Class Test2
	End Class

	Sub Main
        	Dim o As Object
		o = new Test1()
		If Not TypeOf o Is Test1 Then
			Throw New Exception("#A1: TypeOf failed")
		End If
		If TypeOf o Is Test2 Then
			Throw New Exception("#A2: TypeOf failed")
		End If
	End Sub
End Module
		
