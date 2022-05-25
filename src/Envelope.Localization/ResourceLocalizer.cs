using Microsoft.Extensions.Localization;

namespace Envelope.Localization;

public interface IResourceLocalizer
{
	IStringLocalizer Localizer { get; }
}

public interface IResourceLocalizer<TKeys> : IResourceLocalizer
{
}

public abstract class ResourceLocalizer<TKeys> : IResourceLocalizer<TKeys>
{
	public IStringLocalizer Localizer { get; }

	public ResourceLocalizer(IServiceProvider serviceProvider, string baseName, string assemblyName)
	{
		Localizer = ResourceLocalizerFactory.Create(serviceProvider, baseName, assemblyName);
	}

	public ResourceLocalizer(IStringLocalizer localizer)
	{
		Localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
	}
}
