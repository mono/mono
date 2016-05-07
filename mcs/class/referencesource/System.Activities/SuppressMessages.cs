//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Runtime;

// When a resource from SR.resx is only used in an expression, FxCop may issue an AvoidUncalledPrivateCode error
[module: SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Scope = "member", Target = "System.Activities.SR.SwitchCaseTypeMismatch(System.Object,System.Object):System.String")]
[module: SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Scope = "member", Target = "System.Activities.EtwTrackingParticipantTrackRecords.get_ResourceManager():System.Resources.ResourceManager")]
