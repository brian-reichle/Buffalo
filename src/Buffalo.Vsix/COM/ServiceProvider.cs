// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Runtime.InteropServices;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Buffalo.Vsix
{
	sealed class ServiceProvider : IServiceProvider
	{
		public ServiceProvider(IOleServiceProvider oleServiceProvider)
		{
			if (oleServiceProvider == null) throw new ArgumentNullException(nameof(oleServiceProvider));
			_oleServiceProvider = oleServiceProvider;
		}

		object GetServiceCore(Guid guid)
		{
			if (guid == Guid.Empty)
			{
				return null;
			}
			else
			{
				_oleServiceProvider.QueryService(guid, new Guid(NativeMethods.IID_IUnknown), out var result);
				return Marshal.GetObjectForIUnknown(result);
			}
		}

		public object GetService(Type serviceType)
		{
			if (serviceType == null)
			{
				return null;
			}
			else if (_oleServiceProvider == null)
			{
				return null;
			}
			else if (serviceType == typeof(IOleServiceProvider))
			{
				return _oleServiceProvider;
			}
			else
			{
				return GetServiceCore(serviceType.GUID);
			}
		}

		readonly IOleServiceProvider _oleServiceProvider;

		static class NativeMethods
		{
			public const string IID_IUnknown = "{00000000-0000-0000-C000-000000000046}";
		}
	}
}
