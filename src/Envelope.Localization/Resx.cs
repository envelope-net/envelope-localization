namespace Envelope.Localization;

public class ResxData
{
	internal ResxBuilder.rootData _data;
	public string Value { get => _data.value; set => _data.value = value; }
	public string Comment { get => _data.comment; set => _data.comment = value; }
	public string Name { get => _data.name; set => _data.name = string.IsNullOrWhiteSpace(value) ? throw new ArgumentNullException(nameof(value)) : value; }
	public string Type { get => _data.type; set => _data.type = value; }
	public string Mimetype { get => _data.mimetype; set => _data.mimetype = value; }
	public string Space { get => _data.space; set => _data.space = value; }

	public ResxData()
	{
		_data = new ResxBuilder.rootData();
		Space = "preserve";
	}

	public ResxData(string name, string value)
	{
		_data = new ResxBuilder.rootData();
		Space = "preserve";
		Name = name;
		Value = value;
	}

	public ResxData(ResxBuilder.rootData data)
	{
		_data = data ?? throw new ArgumentNullException(nameof(data));
	}
}
