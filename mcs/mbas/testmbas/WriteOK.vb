Option Explicit
Option Strict Off
Option Compare Text

Imports System, IO = System.Console

Module WriteOK

    Sub Main()
		Dim nodim as integer ' comment out to test explicit
		
        REM Testing old-fashioned comments
        Console.WriteLine("OK!") ' Simple comments
		WriteOK2.[Sub]()
		IO.WriteLine("OK! via aliased name") ' from alias
		nodim = 1 ' test for explicit
        Console.WriteLine(nodim)
		WriteOK5.ModuleSub() ' 122
		ModuleSub() ' 103
    End Sub

End Module

Public Class WriteOK2

    Friend Shared Sub [Sub]() ' Escaped identifier
		Dim Text as string ' here 'Text' isn't a keyword
		Text = "This is a test!"
        Console.WriteLine("Sub:OK! - """ & Text & """")
    End Sub

End Class

Public Module WriteOK5
    Public Sub ModuleSub()
        Console.WriteLine("ModuleSub:OK!")
    End Sub
End Module
