using System.Collections;
using System.Text.RegularExpressions;

namespace Envelope.Localization;

public class Resource
{
	public string Name { get; }
	public string? Value { get; }
	public List<string> NumericParameters { get; }
	public List<string> StringParameters { get; }
	public List<string> Errors { get; }

	public Resource(ResxData data)
		: this(data?.Name ?? throw new ArgumentNullException(nameof(data)), data.Value)
	{
	}

	public Resource(string name, string value)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentNullException(nameof(name));

		Name = name;
		Value = value;
		Value = Value?.Replace("\\", "\\\\");
		Value = Value?.Replace("\"", "\\\"");

		NumericParameters = new List<string>();
		Errors = new List<string> { $"{nameof(Name)} is null" };

		if (!string.IsNullOrWhiteSpace(Value))
		{
			Errors = new List<string>();
			NumericParameters = new List<string>();

			List<string> numParameters = Regex.Matches(Value, @"\{(\w+)\}")
				.Cast<Match>()
				.Select(m => m.Groups[1].Value)
				.Distinct()
				.OrderBy(m => m)
				.ToList();

			for (int i = 0; i < numParameters.Count; i++)
			{
				bool hasError = false;
				if (!int.TryParse(numParameters[i], out int intValue))
				{
					Errors.Add($"{Name} has invalid formatting parameter {numParameters[i]}. Can not cast to int.");
					hasError = true;
				}

				if (numParameters.All(p => p != i.ToString()))
				{
					Errors.Add($"IndexOutOfRangeException: {Name} has invalid formatting parameters. Index out of range.");
					hasError = true;
				}

				if (!hasError)
				{
					NumericParameters.Add(numParameters[i]);
				}
			}

			var strParameters = Regex.Matches(Value, "{([^{}:]+)(?::([^{}]+))?}")
				.Cast<Match>()
				.Select(m => m.Groups[1].Value)
				.Distinct()
				.OrderBy(m => m)
				.ToList();

			StringParameters = strParameters.Where(x => !NumericParameters.Contains(x)).ToList();
		}
		else
		{
			StringParameters = new List<string>();
		}
	}

	public Resource(DictionaryEntry? de)
		: this(de?.Key?.ToString()!, de?.Value?.ToString()!)
	{
	}

	public override string ToString()
	{
		return $"{Name} = {Value}";
	}
}

