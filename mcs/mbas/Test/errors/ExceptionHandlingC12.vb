REM LineNo: 10
REM ExpectedError: BC30132
REM ErrorMessage: Label '11' is not defined

Imports System

Module ExceptionHandlingC12

    Sub Main()
        Resume 11   ' Resume [Next | line ] 
        Console.WriteLine("Hello World")
    End Sub

End Module

