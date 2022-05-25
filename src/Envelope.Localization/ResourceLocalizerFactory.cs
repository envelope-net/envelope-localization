using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Envelope.Localization;

public static class ResourceLocalizerFactory
{
	public static IStringLocalizer Create(IServiceProvider serviceProvider, string baseName, string assemblyName)
	{
		if (serviceProvider == null)
			throw new ArgumentNullException(nameof(serviceProvider));

		if (string.IsNullOrWhiteSpace(baseName))
			throw new ArgumentNullException(nameof(baseName));

		if (string.IsNullOrWhiteSpace(assemblyName))
			throw new ArgumentNullException(nameof(assemblyName));

		var stringLocalizerFactory = serviceProvider.GetRequiredService<IStringLocalizerFactory>();
		return stringLocalizerFactory.Create(baseName, assemblyName);
	}
}
