Imports System

'Testing a mustoverride method with a method body

MustInherit Class C3
    Private MustOverride Function F2() As Integer
        Console.WriteLine("If you see this then there is something wrong!!!")
    End Function
End Class

Module MustInheritTest
	Sub Main()
	End Sub
End Module

