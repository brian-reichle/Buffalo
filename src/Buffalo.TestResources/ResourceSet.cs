// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System.Collections.ObjectModel;

namespace Buffalo.TestResources
{
	public sealed class ResourceSet
	{
		public ResourceSet(Resource config, Resource code, params TypedResource[] additionalFiles)
		{
			Config = config;
			Code = code;
			AdditionalFiles = new ReadOnlyCollection<TypedResource>(additionalFiles);
		}

		public Resource Config { get; }
		public Resource Code { get; }
		public ReadOnlyCollection<TypedResource> AdditionalFiles { get; }
	}
}
