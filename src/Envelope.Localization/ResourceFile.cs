using Envelope.Extensions;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Envelope.Localization;

public class ResourceFile : IEnumerable<Resource>
{
	public Assembly? ResourceAssembly { get; set; }
	public string? FullPath { get; set; }
	public string? RelativePath { get; set; }
	public string? Name { get; set; }
	public CultureInfo? CultureInfo { get; set; }
	public ResourceManager? ResourceManager { get; private set; }
	public string? ResourceManagerBaseName => ResourceManager?.BaseName;
	public ResourceSet? ResourceSet { get; set; }
	public List<Resource>? Resources { get; private set; }
	public List<string> Errors { get; }

	public ResourceFile()
	{
		Errors = new List<string>();
	}

	public ResourceFile(string relativePath, List<Resource>? resources)
		: this()
	{
		RelativePath = relativePath;
		Resources = resources?.OrderBy(r => r.Name).ToList();
	}

	public ResourceFile LoadResourceSet(bool parseResources)
	{
		if (ResourceAssembly == null)
			throw new InvalidOperationException($"{nameof(ResourceAssembly)} == null");

		ResourceManager = ResourceLoader.CreateResourceManager(ResourceAssembly, RelativePath);
		ResourceSet = ResourceManager.GetResourceSet(CultureInfo ?? CultureInfo.InvariantCulture, true, true);
		if (parseResources) ParseResources();
		return this;
	}

	public ResourceFile ParseResources()
	{
		if (ResourceSet != null)
		{
			Resources = new List<Resource>();
			foreach (var directoryEntry in ResourceSet)
			{
				var resource = new Resource((DictionaryEntry)directoryEntry);
				AddResource(resource);
			}
			Resources = Resources.OrderBy(r => r.Name).ToList();
		}
		return this;
	}

	public ResourceFile LoadResourcesFromResxFile()
	{
		Resources = new List<Resource>();

		if (string.IsNullOrWhiteSpace(FullPath))
			throw new InvalidOperationException($"{nameof(FullPath)} == null");

		var resxBuilder = new ResxBuilder(FullPath!);
		foreach (var data in resxBuilder.Data)
			Resources.Add(new Resource(data));
		return this;
	}

	public string GetResourceRelativeName()
	{
		return RelativePath?
			.Replace(Path.DirectorySeparatorChar, '.')
			.Replace(Path.AltDirectorySeparatorChar, '.')
			?? "";
	}

	public string GetResourceRelativeName(string resourcesPath)
	{
		var result = GetResourceRelativeName();

		if (!string.IsNullOrWhiteSpace(resourcesPath))
		{
			if (!resourcesPath.EndsWith("."))
				resourcesPath += ".";
			result = result.TrimPrefix(resourcesPath);
		}

		return result.TrimPrefix("Resources.");
	}

	private void AddResource(Resource resource)
	{
		Resources?.Add(resource);
		if (resource.Errors != null && 0 < resource.Errors.Count)
		{
			Errors.AddRange(resource.Errors);
		}
	}

	public List<string> GetConfigurationFolderStructure(string targetWebProjectFolderPath)
	{
		var filePath = Path.GetDirectoryName(FullPath);
		var relativeFilePath = filePath!.TrimPrefix(targetWebProjectFolderPath, true);
		
		if (relativeFilePath.StartsWith("\\"))
#if NETSTANDARD2_0 || NETSTANDARD2_1
			relativeFilePath = relativeFilePath.Substring(1);
#elif NET6_0_OR_GREATER
			relativeFilePath = relativeFilePath[1..];
#endif

		var tmp = relativeFilePath.Split('\\');

		var foldersStructure = tmp.ToList();

		return foldersStructure;
	}

	public string GetConfigurationFolderPath(string root, string targetWebProjectFolderPath)
	{
		var foldersStructure = GetConfigurationFolderStructure(targetWebProjectFolderPath);
		if (!string.IsNullOrWhiteSpace(root))
			foldersStructure.Insert(0, root);

		var currentFolder = targetWebProjectFolderPath;
		foreach (var folder in foldersStructure)
			currentFolder = Path.Combine(currentFolder, folder);

		return currentFolder;

		//var currentFolder = Path.Combine(targetWebProjectFolderPath, "Configuration");
		//foreach (var item in foldersStructure)
		//{
		//	currentFolder = Path.Combine(currentFolder, item);
		//	if (!Directory.Exists(currentFolder))
		//		Directory.CreateDirectory(currentFolder);
		//}

		//return currentFolder;
	}

	public IEnumerator<Resource> GetEnumerator()
	{
		if (Resources != null)
		{
			foreach (var resource in Resources)
				yield return resource;
		}
		else
		{
			if (ResourceAssembly != null && ResourceSet == null)
				LoadResourceSet(false);

			if (ResourceSet != null)
			{
				if (Resources != null)
				{
					foreach (var resource in Resources)
						yield return resource;
				}
				else
				{
					//Resources = new List<Resource>();
					//foreach (var directoryEntry in ResourceSet)
					//{
					//    var resource = new Resource((DictionaryEntry)directoryEntry);
					//    AddResource(resource);
					//    yield return resource;
					//}

					//kvoli tomu aby som dodrzal zotriedene poradie resources, tak najskor je potrebne spustit ParseResources()
					//t.j. nemozem ich yield-ovat po jednom priamo z ResourceSet, kedze tam este nie su zotriedene

					ParseResources();

					if (Resources != null)
						foreach (var resource in Resources)
							yield return resource;
				}
			}

			LoadResourcesFromResxFile();

			if (Resources != null)
				foreach (var resource in Resources)
					yield return resource;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}

	public override string ToString()
	{
		return $"{RelativePath}{(CultureInfo == CultureInfo.InvariantCulture ? "" : $".{CultureInfo}")}";
	}
}

