//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Runtime;

// When a resource from SR.resx is called from internal accessible code FxCop can't tell that it ever gets called
[module: SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Scope = "member", Target = "System.ServiceModel.Routing.SR.get_RoutingTableNotConfigured():System.String")]
[module: SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Scope = "member", Target = "System.ServiceModel.Routing.SR.set_Culture(System.Globalization.CultureInfo):System.Void")]
