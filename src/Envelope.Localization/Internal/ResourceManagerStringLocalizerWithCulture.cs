using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Reflection;

namespace Envelope.Localization.Internal;

internal class ResourceManagerStringLocalizerWithCulture : IStringLocalizer
{
	private static readonly Reflection.Internal.MethodCall<ResourceManagerStringLocalizer?, object?> _getStringSafelyMethodCall;
	private static readonly Func<ResourceManagerStringLocalizer?, object?> _resourceBaseNameFiledGetter;

	static ResourceManagerStringLocalizerWithCulture()
	{
		var type = typeof(ResourceManagerStringLocalizer);
		var method = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).First(x => x.Name == "GetStringSafely");
		_getStringSafelyMethodCall = Reflection.Internal.DelegateFactory.Instance.CreateMethodCall<ResourceManagerStringLocalizer>(method);

		var field = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic).First(x => x.Name == "_resourceBaseName");
		_resourceBaseNameFiledGetter = Reflection.Internal.DelegateFactory.Instance.CreateGet<ResourceManagerStringLocalizer>(field);
	}

	private readonly string _resourceBaseName;
	private readonly ResourceManagerStringLocalizer _resourceManagerStringLocalizer;
	private readonly CultureInfo _cultureInfo;

	/// <inheritdoc />
	public LocalizedString this[string name]
	{
		get
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			var value = GetStringSafely(name);

			return new LocalizedString(name, value ?? name, resourceNotFound: value == null, searchedLocation: _resourceBaseName);
		}
	}

	/// <inheritdoc />
	public LocalizedString this[string name, params object[] arguments]
	{
		get
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			var format = GetStringSafely(name);
			var value = string.Format(_cultureInfo, format ?? name, arguments);

			return new LocalizedString(name, value, resourceNotFound: format == null, searchedLocation: _resourceBaseName);
		}
	}

	public ResourceManagerStringLocalizerWithCulture(ResourceManagerStringLocalizer resourceManagerStringLocalizer, CultureInfo cultureInfo)
	{
		_resourceManagerStringLocalizer = resourceManagerStringLocalizer ?? throw new ArgumentNullException(nameof(resourceManagerStringLocalizer));
		_cultureInfo = cultureInfo ?? throw new ArgumentNullException(nameof(cultureInfo));
		_resourceBaseName = (string)_resourceBaseNameFiledGetter(resourceManagerStringLocalizer)!;
	}

	/// <inheritdoc />
	public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
		_resourceManagerStringLocalizer.GetAllStrings(includeParentCultures);

	private string? GetStringSafely(string name)
		=> _getStringSafelyMethodCall(_resourceManagerStringLocalizer, name, _cultureInfo) as string;
}
