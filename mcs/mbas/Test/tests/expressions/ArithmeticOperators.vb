
Imports System

Module ArithmeticOperators

    Sub main()
        Dim a1, a3 As Integer
        Dim a2 As String

        a1 = 2 + 3
        If a1 <> 5 Then
            throw new System.Exception("#A1-AdditionOperator:Failed")
        End If

        a1 = 1204.08 + 3433
        If a1 <> 4637 Then
            throw new System.Exception("#A2-AdditionOperator:Failed")
        End If

        a3 = 2
        a2 = "235"
        a1 = a2 + a3
        If a1 <> 237 Then
            throw new System.Exception("#A3-AdditionOperator:Failed")
        End If

        a1 = a3 + Nothing
        If a1 <> 2 Then
            throw new System.Exception("#A4-AdditionOperator:Failed")
        End If

        Dim b1, b2, b3 As Char
        b1 = "a"
        b2 = "c"
        b3 = b1 + b2
        If b3 <> "a" Then
            throw new System.Exception("#A5-AdditionOperator:Failed")
        End If

        Dim c1 As Double
        c1 = 463.338 - 338.333
        If c1 <> 125.005 Then
            throw new System.Exception("#A6-SubtractionOperator:Failed")
        End If

        c1 = 463.338 * 338.3
        If c1 <> 156747.2454 Then
            throw new System.Exception("#A7-MultiplicationOperator:Failed")
        End If

    End Sub

End Module