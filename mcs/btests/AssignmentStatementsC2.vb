REM LineNo: 13
REM ExpectedError: BC32007
REM ErrorMessage: 'Integer' values cannot be converted to 'Char'. Use 'Microsoft.VisualBasic.ChrW' 
REM               to interpret a numeric value as a Unicode character or first convert it to 'String' to produce a digit.

Imports System

Module AssignmentStatementsC2

    Sub main()

        Dim ch As Char
        ch = 1
        Console.WriteLine(ch)

    End Sub

End Module
