Imports System

Module LikeOperator
    Sub main()

        Dim a As Boolean

        a = "HELLO" Like "HELLO"
        If a <> True Then
            Console.WriteLine("#A1-LikeOperator:Failed")
        End If

        a = "HELLO" Like "HEllO"
        If a <> False Then
            Console.WriteLine("#A2-LikeOperator:Failed")
        End If

        a = "HELLO" Like "H*O"
        If a <> True Then
            Console.WriteLine("#A3-LikeOperator:Failed")
        End If

        a = "HELLO" Like "H[A-Z][!M-P][!A-K]O"
        If a <> True Then
            Console.WriteLine("#A4-LikeOperator:Failed")
        End If

        a = "HE12O" Like "H?##[L-P]"
        If a <> True Then
            Console.WriteLine("#A5-LikeOperator:Failed")
        End If

        a = "HELLO123WORLD" Like "H?*#*"
        If a <> True Then
            Console.WriteLine("#A6-LikeOperator:Failed")
        End If

        a = "HELLOworld" Like "B*O*d"
        If a <> False Then
            Console.WriteLine("#A7-LikeOperator:Failed")
        End If

        a = "" Like ""
        If a <> True Then
            Console.WriteLine("#A8-LikeOperator:Failed")
        End If

        a = "A" Like ""
        If a <> False Then
            Console.WriteLine("#A9-LikeOperator:Failed")
        End If

        a = "" Like "A"
        If a <> False Then
            Console.WriteLine("#A10-LikeOperator:Failed")
        End If

        a = "HELLO" Like "HELLO" & Nothing
        If a <> True Then
            Console.WriteLine("#A11-LikeOperator:Failed")
        End If


    End Sub

End Module
