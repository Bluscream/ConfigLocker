using IniParser.Parser;
using IniParser.Model;
using System.IO;
using IniParser;

public class Ini {
    private readonly IniData _data;

    public void IniConfig(string filePath) {
        _data = ReadIniFile(filePath);
    }

    private IniData ReadIniFile(string filePath) {
        var parser = new IniParser();
        return parser.ReadFile(filePath);
    }

    public string GetValue(string section, string key) {
        return _data[section][key];
    }

    public void SetValue(string section, string key, string value) {
        _data[section][key] = value;
        WriteIniFile(section, key, value);
    }

    private void WriteIniFile(string section, string key, string value) {
        var parser = new IniParser();
        parser.WriteFile(_data, section, key, value);
    }
}
