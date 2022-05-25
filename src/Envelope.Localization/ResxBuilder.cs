using Envelope.Serializer;

namespace Envelope.Localization;

public class ResxBuilder
{
	private readonly string _resxFilePath;
	private root _root;
	private readonly List<ResxData> _dataList;

	public IReadOnlyList<ResxData> Data => _dataList;

	public ResxBuilder(string resxFilePath, bool createIfDoesNotExists = true)
	{
		if (string.IsNullOrWhiteSpace(resxFilePath))
			throw new ArgumentNullException(nameof(resxFilePath));

		if (!File.Exists(resxFilePath) && createIfDoesNotExists)
		{
			_resxFilePath = resxFilePath;
			CreateNew();
		}
		else
		{
			_resxFilePath = resxFilePath;
			_root = XmlSerializerHelper.ReadFromXml<root>(_resxFilePath) ?? throw new ArgumentException("_root == null", nameof(resxFilePath));
		}

		_dataList = new List<ResxData>();
		foreach (var item in _root!.Items)
			if (item is rootData data)
				_dataList.Add(new ResxData(data));
	}

	private void CreateNew()
	{
		_root = new root
		{
			Items = new List<object>
			{
				new rootResheader{ name = "resmimetype", value = "text/microsoft-resx" },
				new rootResheader{ name = "version", value = "2.0" },
				new rootResheader{ name = "reader", value = "System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" },
				new rootResheader{ name = "writer", value = "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" }
			}
		};
	}

	public bool Add(string name, string value)
	{
		return Add(new ResxData(new rootData { name = name, value = value ?? "", space = "preserve" }));
	}

	public bool Add(ResxData data)
	{
		var dataExists = _dataList.Any(d => d.Name == data.Name);
		if (dataExists)
			return false;

		_root.Items.Add(data._data);
		_dataList.Add(data);
		return true;
	}

	public ResxBuilder Clear()
	{
		_root.Items = new List<object>(_root.Items.Where(item => item is not rootData));
		_dataList.Clear();
		return this;
	}

	public bool Remove(string name)
	{
		var data = _dataList.FirstOrDefault(d => d.Name == name);
		if (data == null)
			throw new ArgumentException("data == null", nameof(name));

		return Remove(data);
	}

	public bool Remove(string name, string value)
	{
		var data = _dataList.FirstOrDefault(d => d.Name == name && d.Value == value);
		if (data == null)
			throw new ArgumentException("data == null", nameof(name));

		return Remove(data);
	}

	public bool Remove(ResxData data)
	{
		return _root.Items.Remove(data._data) && _dataList.Remove(data);
	}

	public void Serialize(string? resxFilePath = null)
	{
		if (string.IsNullOrWhiteSpace(resxFilePath))
			resxFilePath = _resxFilePath;

		if (string.IsNullOrWhiteSpace(resxFilePath))
			throw new ArgumentNullException(nameof(resxFilePath));

		XmlSerializerHelper.WriteToXml(resxFilePath!, _root);
	}

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	/// <remarks/>
	[System.Serializable()]
	[System.Diagnostics.DebuggerStepThrough()]
	[System.ComponentModel.DesignerCategory("code")]
	[System.Xml.Serialization.XmlType(AnonymousType = true)]
	[System.Xml.Serialization.XmlRoot(Namespace = "", IsNullable = false)]
	public partial class root
	{

		private List<object> itemsField;

		/// <remarks/>
		[System.Xml.Serialization.XmlElement("assembly", typeof(rootAssembly), Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		[System.Xml.Serialization.XmlElement("data", typeof(rootData), Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		[System.Xml.Serialization.XmlElement("metadata", typeof(rootMetadata), Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		[System.Xml.Serialization.XmlElement("resheader", typeof(rootResheader), Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public List<object> Items
		{
			get
			{
				return this.itemsField;
			}
			set
			{
				this.itemsField = value;
			}
		}
	}

	/// <remarks/>
	[System.Serializable()]
	[System.Diagnostics.DebuggerStepThrough()]
	[System.ComponentModel.DesignerCategory("code")]
	[System.Xml.Serialization.XmlType(AnonymousType = true)]
	public partial class rootAssembly
	{

		private string aliasField;

		private string nameField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttribute()]
		public string alias
		{
			get
			{
				return this.aliasField;
			}
			set
			{
				this.aliasField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttribute()]
		public string name
		{
			get
			{
				return this.nameField;
			}
			set
			{
				this.nameField = value;
			}
		}
	}

	/// <remarks/>
	[System.Serializable()]
	[System.Diagnostics.DebuggerStepThrough()]
	[System.ComponentModel.DesignerCategory("code")]
	[System.Xml.Serialization.XmlType(AnonymousType = true)]
	public partial class rootData
	{

		private string valueField;

		private string commentField;

		private string nameField;

		private string typeField;

		private string mimetypeField;

		private string spaceField;

		/// <remarks/>
		[System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public string value
		{
			get
			{
				return this.valueField;
			}
			set
			{
				this.valueField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public string comment
		{
			get
			{
				return this.commentField;
			}
			set
			{
				this.commentField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttribute()]
		public string name
		{
			get
			{
				return this.nameField;
			}
			set
			{
				this.nameField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttribute()]
		public string type
		{
			get
			{
				return this.typeField;
			}
			set
			{
				this.typeField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttribute()]
		public string mimetype
		{
			get
			{
				return this.mimetypeField;
			}
			set
			{
				this.mimetypeField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
		public string space
		{
			get
			{
				return this.spaceField;
			}
			set
			{
				this.spaceField = value;
			}
		}
	}

	/// <remarks/>
	[System.Serializable()]
	[System.Diagnostics.DebuggerStepThrough()]
	[System.ComponentModel.DesignerCategory("code")]
	[System.Xml.Serialization.XmlType(AnonymousType = true)]
	public partial class rootMetadata
	{

		private string valueField;

		private string nameField;

		private string typeField;

		private string mimetypeField;

		private string spaceField;

		/// <remarks/>
		[System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public string value
		{
			get
			{
				return this.valueField;
			}
			set
			{
				this.valueField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttribute()]
		public string name
		{
			get
			{
				return this.nameField;
			}
			set
			{
				this.nameField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttribute()]
		public string type
		{
			get
			{
				return this.typeField;
			}
			set
			{
				this.typeField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttribute()]
		public string mimetype
		{
			get
			{
				return this.mimetypeField;
			}
			set
			{
				this.mimetypeField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
		public string space
		{
			get
			{
				return this.spaceField;
			}
			set
			{
				this.spaceField = value;
			}
		}
	}

	/// <remarks/>
	[System.Serializable()]
	[System.Diagnostics.DebuggerStepThrough()]
	[System.ComponentModel.DesignerCategory("code")]
	[System.Xml.Serialization.XmlType(AnonymousType = true)]
	public partial class rootResheader
	{

		private string valueField;

		private string nameField;

		/// <remarks/>
		[System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public string value
		{
			get
			{
				return this.valueField;
			}
			set
			{
				this.valueField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttribute()]
		public string name
		{
			get
			{
				return this.nameField;
			}
			set
			{
				this.nameField = value;
			}
		}
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	}
}
