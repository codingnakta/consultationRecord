using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using StudentCounseling.Models;

namespace StudentCounseling.Services;

public class JsonDataRepository : IDataRepository
{
    private readonly string _dir;
    private readonly string _dataPath;
    private readonly string _backupPath;

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        Converters = { new JsonStringEnumConverter() },
    };

    public JsonDataRepository()
    {
        _dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StudentCounseling");
        _dataPath = Path.Combine(_dir, "data.json");
        _backupPath = Path.Combine(_dir, "data.backup.json");
    }

    public string DataPath => _dataPath;

    public DataStore Load()
    {
        if (!File.Exists(_dataPath))
            return new DataStore();

        var json = File.ReadAllText(_dataPath);
        if (string.IsNullOrWhiteSpace(json))
            return new DataStore();

        var data = JsonSerializer.Deserialize<DataStore>(json, Options);
        return data ?? new DataStore();
    }

    public void Save(DataStore data)
    {
        Directory.CreateDirectory(_dir);

        if (File.Exists(_dataPath))
        {
            File.Copy(_dataPath, _backupPath, overwrite: true);
        }

        var json = JsonSerializer.Serialize(data, Options);
        File.WriteAllText(_dataPath, json);
    }
}
