'Author:
'   K. Satya Sudha (ksathyasudha@novell.com)
'
' (C) 2005 Novell, Inc.
                                                                                                               
Imports System
Module Test
        Sub Main()
                Dim a As Boolean = "c" Like "[abc6!0-58-9]"
                if a <> true then
                        throw new Exception ("#A1 - Like operator failed")
                end if

		a = "5" Like "[abc0-4def8-9!]"
                if a <> true then
                        throw new Exception ("#A2 - Like operator failed")
                end if

		a = "5" Like "[!abc0-4def8-9]"
                if a <> true then
                        throw new Exception ("#A3 - Like operator failed")
                end if

		a = "e" Like "[a-f!567o-z]"
                if a <> true then
                        throw new Exception ("#A4 - Like operator failed")
                end if

		a = "p" Like "[a-f!567o-z]"
                if a <> false then
                        throw new Exception ("#A5 - Like operator failed")
                end if

		a = "d" Like "[a-c-e-g]"
                if a <> false then
                        throw new Exception ("#A6 - Like operator failed")
                end if

		a = "b" Like "[a-c-e-g]"
                if a <> true then
                        throw new Exception ("#A7 - Like operator failed")
                end if

        End Sub
End Module

