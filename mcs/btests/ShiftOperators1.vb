'Unhandled Exception: System.InvalidCastException: Cast from string 
' to type 'Long' is not valid. ---> System.FormatException: Input string was not in a
' correct format.
' Strict On disallows implicit conversions from double to long

'Option Strict On
Imports System

Module ShiftOperators1

    Sub Main()

        Dim b1 As String = "xyz"
        b1 = b1 << 109
        Console.WriteLine(b1)

    End Sub

End Module