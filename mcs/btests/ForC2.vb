REM LineNo: 21
REM ExpectedError: BC30039
REM ErrorMessage: Loop control variable cannot be a property or a late-bound indexed array.

Imports System

Module ForC2

    Private index As Integer = 0
    Public Property myindex() As Integer
        Get
            Return index
        End Get
        Set(ByVal Value As Integer)
            index = Value
        End Set
    End Property

    Sub main()

        For myindex = 0 To 10
            Console.WriteLine("Hello World")
        Next

    End Sub

End Module