Imports System
Interface I
    Sub F()
End Interface

Class C
    Implements I

    Public Sub F() Implements I.F
    End Sub
End Class

Module InterfaceTest
    Sub Main()
	Try

	        Dim x As C = New C()
        	x.F()
	Catch e As Exception
		Console.WriteLine("#A1:Interface:Failed-error creating instance of class implementing interface"+e.Message)
	End Try

	Try
	        Dim y As I = New C()
        	y.F()
	Catch e As Exception
		Console.WriteLine("#A2:Interface:Failed - error declaring varaibles of the interface")
		Console.WriteLine(e.Message)
	End Try
    End Sub
End Module
