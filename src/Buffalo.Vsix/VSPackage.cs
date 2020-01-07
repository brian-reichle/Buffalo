// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace Buffalo.Vsix
{
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[InstalledProductRegistration("Buffalo", "", "1.0")]
	[Guid("2e0d9bdd-3cb1-48ac-aef8-dd1648dcd827")]
	public sealed class VSPackage : AsyncPackage
	{
	}
}
