using System.IO;
using System.Text.Json;

namespace KindredSchematics.Services;
internal class ConfigSettingsService
{
	private static readonly string CONFIG_PATH = Path.Combine(BepInEx.Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME);
	private static readonly string SETTINGS_PATH = Path.Combine(CONFIG_PATH, "settings.json");

	public bool FreeBuildDisabled
    {
		get {
			return config.FreeBuildDisabled;

		}
		set { 
			config.FreeBuildDisabled = value; 
			SaveConfig();
		}
	}

	struct Config
	{
		public bool FreeBuildDisabled { get; set; }
	}

	Config config;

	public ConfigSettingsService()
	{
		LoadConfig();
	}

	void LoadConfig()
	{
		if (!File.Exists(SETTINGS_PATH))
		{
			config = new Config();
			SaveConfig();
			return;
		}

		var json = File.ReadAllText(SETTINGS_PATH);
		config = JsonSerializer.Deserialize<Config>(json);
	}

	void SaveConfig()
	{
		if(!Directory.Exists(CONFIG_PATH))
			Directory.CreateDirectory(CONFIG_PATH);
		var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true });
		File.WriteAllText(SETTINGS_PATH, json);
	}
}
