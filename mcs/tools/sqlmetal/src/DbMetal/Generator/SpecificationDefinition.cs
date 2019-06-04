#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;

namespace DbMetal.Generator
{
    [Flags]
    public enum SpecificationDefinition
    {
        ProtectionClass     = 0x00000F,
        Public              = 0x000000,
        Protected           = 0x000001,
        Private             = 0x000002,
        Internal            = 0x000004,

        InheritanceClass    = 0x00FF00,
        Abstract            = 0x000100,
        Virtual             = 0x000200,
        Override            = 0x000400,
        Static              = 0x000800,
        Sealed              = 0x001000,
        New                 = 0x002000,

        DomainClass         = 0x0F0000,
        Partial             = 0x010000,

        DirectionClass      = 0x0000F0,
        In                  = 0x000010,
        Out                 = 0x000020,
        Ref                 = 0x000030,

        Event               = 0x100000,
    }
}
