using Microsoft.Extensions.Localization;
using Envelope.Text;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;

namespace Envelope.Localization;

public static class ResourceLoader
{
	/// <summary>
	/// Create <see cref="ResourceManager"/> based on type and project relative path to *.resx file
	/// </summary>
	/// <param name="resourceSource">Type to resolve the assembly with embeded resource</param>
	/// <param name="resourcesRelativePath">Relative path to resx file in csproj without resx extension. Example: for "Resources\\Labels.resx" relative path is "Resources\\Labels"</param>
	/// <returns></returns>
	public static ResourceManager CreateResourceManager(Type resourceSource, string resourcesRelativePath)
	{
		if (resourceSource == null)
			throw new ArgumentNullException(nameof(resourceSource));

		var typeInfo = resourceSource.GetTypeInfo();
		var resourceAssembly = typeInfo.Assembly;

		return CreateResourceManager(resourceAssembly, resourcesRelativePath);
	}

	public static ResourceManager CreateResourceManager(Assembly resourceAssembly, string? resourcesRelativePath)
	{
		if (resourceAssembly == null)
			throw new ArgumentNullException(nameof(resourceAssembly));

		if (string.IsNullOrWhiteSpace(resourceAssembly.FullName))
			throw new ArgumentException("resourceAssembly.FullName == null", nameof(resourceAssembly));

		var assemblyName = new AssemblyName(resourceAssembly.FullName);
		var resourcePath = GetResourcePath(resourceAssembly, resourcesRelativePath);

		if (string.IsNullOrWhiteSpace(assemblyName.Name))
			throw new ArgumentException("assemblyName.Name == null", nameof(resourceAssembly));

		var baseName = GetResourcePrefix(assemblyName.Name, resourcePath);
		return new ResourceManager(baseName, resourceAssembly);
	}

	public static ResourceSet? GetResourceSet(Type resourceSource, string resourcesRelativePath, CultureInfo culture)
	{
		ResourceManager resMgr = CreateResourceManager(resourceSource, resourcesRelativePath);
		return resMgr?.GetResourceSet(culture ?? CultureInfo.InvariantCulture, true, true);
	}

	public static ResourceSet? GetResourceSet(Assembly resourceAssembly, string resourcesRelativePath, CultureInfo culture)
	{
		ResourceManager resMgr = CreateResourceManager(resourceAssembly, resourcesRelativePath);
		return resMgr?.GetResourceSet(culture ?? CultureInfo.InvariantCulture, true, true);
	}

	private static string GetResourcePath(Assembly assembly, string? resourcesRelativePath)
	{
		var resourceLocationAttribute = GetResourceLocationAttribute(assembly);

		// If we don't have an attribute assume all assemblies use the same resource location.
		var resourceLocation = resourceLocationAttribute == null
			? (resourcesRelativePath ?? "")
			: resourceLocationAttribute.ResourceLocation + ".";
		resourceLocation = resourceLocation
			.Replace(Path.DirectorySeparatorChar, '.')
			.Replace(Path.AltDirectorySeparatorChar, '.');

		return resourceLocation;
	}

	private static string GetResourcePrefix(string baseNamespace, string resourcesRelativePath)
	{
		if (string.IsNullOrEmpty(baseNamespace))
			throw new ArgumentNullException(nameof(baseNamespace));

		return string.IsNullOrEmpty(resourcesRelativePath)
			? baseNamespace
			: baseNamespace + "." + resourcesRelativePath /*+ StringHelper.TrimPrefix(typeInfo.FullName, baseNamespace + ".")*/;
	}

	private static ResourceLocationAttribute? GetResourceLocationAttribute(Assembly assembly)
		=> assembly.GetCustomAttribute<ResourceLocationAttribute>();

	private static List<ResourceFile> LoadResources(string rootFolder, bool readResourcesFromResx = true, CultureInfo? searchForCultureIfExists = null, SearchOption searchOption = SearchOption.AllDirectories, List<CultureInfo>? checkForCultures = null, bool compareResourceKyes = false)
	{
		if (string.IsNullOrWhiteSpace(rootFolder))
			throw new ArgumentNullException(nameof(rootFolder));

		if (!rootFolder.EndsWith("\\"))
			rootFolder += "\\";

		var result = new List<ResourceFile>();
		foreach (var resourcePath in Directory.EnumerateFiles(rootFolder, "*.resx", searchOption))
		{
			string resourceFullName = Path.GetFileNameWithoutExtension(resourcePath);
			var relativePath = StringHelper.TrimPrefix(resourcePath, rootFolder);
			CultureInfo cultureInfo;

			string resourceName = "";
			int lastDotIndex = resourceFullName.LastIndexOf(".");
			if (lastDotIndex <= 0 || lastDotIndex == resourceFullName.Length - 1)
			{
				resourceName = resourceFullName;
				cultureInfo = CultureInfo.InvariantCulture;
				relativePath = StringHelper.TrimPostfix(relativePath, ".resx");
			}
			else
			{
#if NETSTANDARD2_0 || NETSTANDARD2_1
				resourceName = resourceFullName.Substring(0, lastDotIndex);
				var resourceCulture = resourceFullName.Substring(lastDotIndex + 1);
#elif NET6_0_OR_GREATER
				resourceName = resourceFullName[..lastDotIndex];
				var resourceCulture = resourceFullName[(lastDotIndex + 1)..];
#endif
				cultureInfo = CultureInfo.GetCultureInfo(resourceCulture);
				relativePath = StringHelper.TrimPostfix(relativePath, $".{resourceCulture}.resx");
			}

			List<Resource>? resources = null;
			if (readResourcesFromResx)
			{
				resources = new List<Resource>();
				//XDocument xdoc = XDocument.Load(resourcePath);
				//foreach (var data in xdoc.Descendants().Where(d => d.Name == "data" && d.Parent == xdoc.Root))
				//{
				//	string name = data.Attribute("name").Value;
				//	string value = data.Descendants("value").FirstOrDefault().Value;
				//	resources.Add(new Resource(name, value));
				//}
				var resxBuilder = new ResxBuilder(resourcePath);
				foreach (var data in resxBuilder.Data)
					resources.Add(new Resource(data));
			}

			var resourceFile = new ResourceFile(relativePath, resources)
			{
				FullPath = resourcePath,
				Name = resourceName,
				CultureInfo = cultureInfo
			};
			result.Add(resourceFile);
		}

		if (0 < checkForCultures?.Count || compareResourceKyes)
		{
			var sb = new StringBuilder();
			foreach (var relativePathGroup in result.GroupBy(rf => rf.RelativePath))
			{
				if (0 < checkForCultures?.Count)
				{
					var currentCultures = relativePathGroup.Select(x => x.CultureInfo);
					var missingCultures = checkForCultures.Where(x => !currentCultures.Contains(x)).ToList();
					if (0 < missingCultures.Count)
						sb.AppendLine($"\t{relativePathGroup.Key}: Missing cultures: {string.Join(", ", missingCultures.Select(x => x.TwoLetterISOLanguageName))}");
				}

				if (compareResourceKyes && 1 < relativePathGroup.Count())
				{
					var allKeys = relativePathGroup.Where(x => x.Resources != null).SelectMany(x => x.Resources!).Select(x => x.Name).Distinct().ToList() ?? new List<string>();
					foreach (var resourceFile in relativePathGroup)
					{
						if (0 < resourceFile.Resources?.Count)
						{
							var currentKeys = resourceFile.Resources.Select(x => x.Name).ToList();
							var missingInCurrent = allKeys.Where(x => !currentKeys.Contains(x)).ToList();
							if (0 < missingInCurrent.Count)
								sb.AppendLine($"\t{resourceFile.RelativePath}.{resourceFile.CultureInfo!.TwoLetterISOLanguageName}: Missing keys: {string.Join(", ", missingInCurrent)}");
						}
						else
						{
							foreach (var key in allKeys)
							{
								sb.AppendLine($"\t{resourceFile.RelativePath}.{resourceFile.CultureInfo!.TwoLetterISOLanguageName}: Missing keys: {string.Join(", ", allKeys)}");
							}
						}
					}
				}
			}
			var errors = sb.ToString();
			if (!string.IsNullOrWhiteSpace(errors))
				throw new IndexOutOfRangeException($"\n\n\nRESOURCES ERROR:\n{errors}");
			else
			{
				if (0 < checkForCultures?.Count)
				{
					System.Diagnostics.Debug.WriteLine($"ALL CUTURES {string.Join(", ", checkForCultures)} - OK");
					Console.WriteLine($"ALL CUTURES {string.Join(", ", checkForCultures)} - OK");
				}

				if (compareResourceKyes)
				{
					System.Diagnostics.Debug.WriteLine("ALL KEYS - OK");
					Console.WriteLine("ALL KEYS - OK");
				}
			}
		}

		if (searchForCultureIfExists != null)
		{
			result = result
				.GroupBy(rf => rf.RelativePath)
				.Select(group =>
				{
					var rf = group.FirstOrDefault(r => r.CultureInfo == searchForCultureIfExists);

					if (rf == null)
						rf = group.OrderBy(x => x.CultureInfo).FirstOrDefault();

					return rf;
				})
				.Where(rf => rf != null)
				.ToList()!;
		}

		return result.OrderBy(rf => rf.RelativePath).ThenBy(rf => rf.CultureInfo?.Name ?? "").ToList();
	}

	public static List<ResourceFile> LoadResources(
		string path,
		Assembly resourceAssembly,
		CultureInfo? searchForCultureIfExists = null,
		ResourceLoadOptions resourceLoadOptions = ResourceLoadOptions.LoadResxAllResources,
		SearchOption searchOption = SearchOption.AllDirectories,
		List<CultureInfo>? checkForCultures = null,
		bool compareResourceKyes = false)
	{
		List<ResourceFile> result = LoadResources(
			path,
			resourceLoadOptions == ResourceLoadOptions.LoadResxAllResources,
			searchForCultureIfExists,
			searchOption,
			checkForCultures,
			compareResourceKyes);

		foreach (var resourceFile in result)
		{
			resourceFile.ResourceAssembly = resourceAssembly;

			if (resourceLoadOptions == ResourceLoadOptions.LoadAssemblyResourceSet
				|| resourceLoadOptions == ResourceLoadOptions.LoadAssemblyResourceSetWithAllResources)
				resourceFile.LoadResourceSet(resourceLoadOptions == ResourceLoadOptions.LoadAssemblyResourceSetWithAllResources);
		}

		return result;
	}

	public static List<ResourceFile> LoadResources(
		string path,
		Type resourceSource,
		CultureInfo? searchForCultureIfExists = null,
		ResourceLoadOptions resourceLoadOptions = ResourceLoadOptions.None,
		SearchOption searchOption = SearchOption.AllDirectories,
		List<CultureInfo>? checkForCultures = null,
		bool compareResourceKyes = false)
	{
		if (resourceSource == null)
			throw new ArgumentNullException(nameof(resourceSource));

		var typeInfo = resourceSource.GetTypeInfo();
		var resourceAssembly = typeInfo.Assembly;

		return LoadResources(path, resourceAssembly, searchForCultureIfExists, resourceLoadOptions, searchOption, checkForCultures, compareResourceKyes);
	}
}
