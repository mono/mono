// cs0619.cs: 'ObsoleteIface ' is obsolete: 'Do not use it'
// Line: 12

using System;

[Obsolete("Do not use it.", true)]
interface ObsoleteIface {
}


interface Ex: ObsoleteIface
{
}