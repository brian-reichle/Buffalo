// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using EnvDTE;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using IServiceProvider = System.IServiceProvider;

namespace Buffalo.Vsix
{
	abstract class BaseCodeGeneratorWithSite : IVsSingleFileGenerator, IObjectWithSite
	{
		[DebuggerStepThrough]
		protected BaseCodeGeneratorWithSite()
		{
		}

		protected string SourceFileName { get; private set; }
		protected string DefaultNamespace { get; private set; }

		protected void GeneratorError(bool isWarning, string text, int line, int column)
		{
			if (_progress == null) throw new InvalidOperationException("This method is only valid while generating.");
			_progress.GeneratorError(isWarning ? 1 : 0, 0, text, (uint)line, (uint)column);
		}

		protected void WriteAdditionalChild(string suffix, byte[] fileContent)
		{
			WriteAdditionalChild(suffix, fileContent, BuildAction.Auto);
		}

		protected void WriteAdditionalChild(string suffix, byte[] fileContent, BuildAction buildAction)
		{
			if (suffix == null) throw new ArgumentNullException(nameof(suffix));
			if (fileContent == null) throw new ArgumentNullException(nameof(fileContent));

			var directory = Path.GetDirectoryName(SourceFileName);
			var baseFileName = Path.GetFileNameWithoutExtension(SourceFileName);
			var filename = baseFileName + suffix;
			var targetFullName = Path.Combine(directory, filename);

			var sourceItem = (ProjectItem)_serviceProvider.GetService(typeof(ProjectItem));
			if (sourceItem == null) throw new InvalidOperationException("unable to get a hold of the source item.");

			var targetItem = GetChild(sourceItem, filename);

			try
			{
				using (var stream = File.Create(targetFullName))
				{
					stream.Write(fileContent, 0, fileContent.Length);
					stream.Flush();
					stream.Close();
				}

				if (_additionalChildren == null)
				{
					_additionalChildren = new List<string>();
				}

				_additionalChildren.Add(filename);
			}
			catch
			{
				try
				{
					if (File.Exists(targetFullName))
					{
						File.Delete(targetFullName);
					}
				}
				catch (FileNotFoundException)
				{
				}

				throw;
			}

			if (targetItem == null)
			{
				targetItem = sourceItem.ProjectItems.AddFromFile(targetFullName);
			}

			if (buildAction != BuildAction.Auto)
			{
				targetItem.Properties.Item("BuildAction").Value = (int)buildAction;
			}
		}

		protected string CalculateBaseResourceName()
		{
			var sourceItem = (ProjectItem)_serviceProvider.GetService(typeof(ProjectItem));
			if (sourceItem == null) throw new InvalidOperationException("unable to get a hold of the source item.");

			var builder = new StringBuilder();
			builder.Append(GetDefaultNamespace(sourceItem));
			builder.Append(".");
			builder.Append(Path.GetFileNameWithoutExtension(sourceItem.Name));
			return builder.ToString();
		}

		protected abstract string DefaultExtension { get; }

		protected abstract byte[] GenerateBytes(string sourceFileContent);

		#region IVsSingleFileGenerator

		int IVsSingleFileGenerator.DefaultExtension(out string pbstrDefaultExtension)
		{
			pbstrDefaultExtension = DefaultExtension;
			return 0;
		}

		int IVsSingleFileGenerator.Generate(
			string wszInputFilePath,
			string bstrInputFileContents,
			string wszDefaultNamespace,
			IntPtr[] rgbOutputFileContents,
			out uint pcbOutput,
			IVsGeneratorProgress pGenerateProgress)
		{
			byte[] result;

			try
			{
				SourceFileName = wszInputFilePath;
				DefaultNamespace = wszDefaultNamespace;
				_progress = pGenerateProgress;

				result = GenerateBytes(bstrInputFileContents ?? string.Empty);

				TrimExcessChildren();
			}
			finally
			{
				_progress = null;
				SourceFileName = null;
				DefaultNamespace = null;
				_additionalChildren = null;
			}

			if (result != null && result.Length != 0)
			{
				var pbstrOutputFileContents = Marshal.AllocCoTaskMem(result.Length);
				Marshal.Copy(result, 0, pbstrOutputFileContents, result.Length);
				rgbOutputFileContents[0] = pbstrOutputFileContents;
				pcbOutput = (uint)result.Length;
			}
			else
			{
				pcbOutput = 0;
				rgbOutputFileContents[0] = IntPtr.Zero;
			}

			return 0;
		}

		#endregion

		#region IObjectWithSite

		void IObjectWithSite.SetSite(object pUnkSite)
		{
			_site = pUnkSite;
			_serviceProvider = new ServiceProvider(_site as IOleServiceProvider);
		}

		void IObjectWithSite.GetSite(ref Guid riid, out IntPtr ppvSite)
		{
			var pUnk = IntPtr.Zero;

			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				pUnk = Marshal.GetIUnknownForObject(_site);
				var hr = Marshal.QueryInterface(pUnk, ref riid, out ppvSite);

				if (hr < 0)
				{
					Marshal.ThrowExceptionForHR(hr);
				}
			}
			finally
			{
				if (pUnk != IntPtr.Zero)
				{
					Marshal.Release(pUnk);
				}
			}
		}

		#endregion

		void TrimExcessChildren()
		{
			var sourceItem = (ProjectItem)_serviceProvider.GetService(typeof(ProjectItem));

			string[] childFileNames;

			if (_additionalChildren == null)
			{
				childFileNames = new string[] { GetTargetFileName() };
			}
			else
			{
				childFileNames = new string[_additionalChildren.Count + 1];
				childFileNames[0] = GetTargetFileName();
				_additionalChildren.CopyTo(childFileNames, 1);

				Array.Sort(childFileNames);
			}

			foreach (ProjectItem item in sourceItem.ProjectItems)
			{
				if (Array.BinarySearch(childFileNames, item.Name, StringComparer.OrdinalIgnoreCase) < 0)
				{
					item.Delete();
				}
			}
		}

		string GetTargetFileName() => Path.GetFileNameWithoutExtension(SourceFileName) + DefaultExtension;

		static ProjectItem GetChild(ProjectItem parentItem, string filename)
		{
			foreach (ProjectItem item in parentItem.ProjectItems)
			{
				if (StringComparer.OrdinalIgnoreCase.Compare(item.Name, filename) == 0)
				{
					return item;
				}
			}

			return null;
		}

		static ProjectItem GetFolder(ProjectItem item)
		{
			while (item != null)
			{
				if (StringComparer.OrdinalIgnoreCase.Equals(item.Kind, VSKinds.PhysicalFolder))
				{
					return item;
				}

				item = item.Collection.Parent as ProjectItem;
			}

			return null;
		}

		static string GetDefaultNamespace(ProjectItem projectItem)
		{
			const string DefaultNamespace = "DefaultNamespace";

			var folder = GetFolder(projectItem);
			var properties = folder == null ? projectItem.ContainingProject.Properties : folder.Properties;
			var property = properties.Item(DefaultNamespace);
			return property.Value.ToString();
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		object _site;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		IServiceProvider _serviceProvider;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		IVsGeneratorProgress _progress;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		List<string> _additionalChildren;
	}
}
