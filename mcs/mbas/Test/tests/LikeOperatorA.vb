Option Compare Text

Imports System

Module LikeOperatorA
    Sub main()

        Dim a As Boolean

        a = "HEllo" Like "H[A-Z][!M-P][!A-K]O"
        If a <> True Then
            Console.WriteLine("#A1-LikeOperator:Failed")
        End If

        a = "he12O" Like "H?##[L-P]"
        If a <> True Then
            Console.WriteLine("#A2-LikeOperator:Failed")
        End If

        a = "He[ll?o*" Like "He[[]*[?]o[*]"
        If a <> True Then
            Console.WriteLine("#A3-LikeOperator:Failed")
        End If

        a = "Hell[]o*" Like "Hell[[][]]o[*]"
        If a <> True Then
            Console.WriteLine("#A4-LikeOperator:Failed")
        End If

        a = "Hell[]o*" Like "Hell[[]][[]]o[*]"
        If a <> False Then
            Console.WriteLine("#A5-LikeOperator:Failed")
        End If

        a = "Hello*" Like "Hell[]o[*]"
        If a <> True Then
            Console.WriteLine("#A6-LikeOperator:Failed")
        End If

    End Sub

End Module
