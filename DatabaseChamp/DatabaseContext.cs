using DatabaseChamp.Exceptions;
using System.Text.Json;

namespace DatabaseChamp
{
    public class DatabaseContext
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never,
            PropertyNameCaseInsensitive = true,
        };

        private readonly string _databaseFolder = Path.Combine(Environment.CurrentDirectory, "databaseFiles");
        private readonly Dictionary<Type, string> _databaseTables = new();

        public DatabaseContext(string databaseFolder = "")
        {
            if (!string.IsNullOrEmpty(databaseFolder))
            {
                _databaseFolder = databaseFolder;
            }

            Directory.CreateDirectory(_databaseFolder);

            // TODO: Scan for existing tables
        }

        public void CreateTable<T>(string tableName, bool overwriteExisting = false)
        {
            var path = Path.Combine(_databaseFolder, $"{tableName}.json");
            if (File.Exists(path) && !overwriteExisting)
            {
                return;
            }

            _databaseTables.Add(typeof(T), tableName);
            try
            {
                using var fs = File.Create(path);
                fs.Close();
                fs.Dispose();

                File.WriteAllText(path, "[]");
            }
            catch (Exception)
            {
                _databaseTables.Remove(typeof(T));
                throw;
            }

            // TODO: Persists table information
        }

        public void Add<T>(T objectToAdd)
        {
            ArgumentNullException.ThrowIfNull(objectToAdd, nameof(objectToAdd));

            var tableName = FindTableForDatatype<T>();

            var path = Path.Combine(_databaseFolder, $"{tableName}.json");
            var existingFileStringContent = File.ReadAllText(path);
            var existingFileContent = JsonSerializer.Deserialize<List<T>>(existingFileStringContent, _jsonSerializerOptions)!;
            if (existingFileContent.Any(i => IsEqual(i, objectToAdd)))
            {
                throw new DublicateException();
            }

            existingFileContent.Add(objectToAdd);
            File.WriteAllText(path, JsonSerializer.Serialize(existingFileContent, _jsonSerializerOptions));
        }

        public void Remove<T>(T objectToRemove)
        {
            ArgumentNullException.ThrowIfNull(objectToRemove, nameof(objectToRemove));

            var tableName = FindTableForDatatype<T>();

            var path = Path.Combine(_databaseFolder, $"{tableName}.json");
            var existingFileStringContent = File.ReadAllText(path);
            var existingFileContent = JsonSerializer.Deserialize<List<T>>(existingFileStringContent, _jsonSerializerOptions)!;
            var item = existingFileContent.SingleOrDefault(i => IsEqual(i, objectToRemove));
            if (item is null)
            {
                throw new ItemNotFoundException(tableName);
            }

            existingFileContent.Remove(item);
            File.WriteAllText(path, JsonSerializer.Serialize(existingFileContent, _jsonSerializerOptions));
        }

        public IEnumerable<T> GetAll<T>()
        {
            var tableName = FindTableForDatatype<T>();

            var path = Path.Combine(_databaseFolder, $"{tableName}.json");
            var existingFileStringContent = File.ReadAllText(path);
            return JsonSerializer.Deserialize<IEnumerable<T>>(existingFileStringContent, _jsonSerializerOptions)!;
        }

        // TODO: I think this will not work with nested objects
        private static bool IsEqual<T>(T objectOne, T objectTwo)
        {
            if (objectOne == null || objectTwo == null)
            {
                return false;
            }

            var type = typeof(T);
            foreach (System.Reflection.PropertyInfo property in type.GetProperties())
            {
                if (property.Name != "ExtensionData")
                {
                    var objectOneValue = string.Empty;
                    var objectTwoValue = string.Empty;
                    if (type.GetProperty(property.Name)?.GetValue(objectOne, null) != null)
                    {
                        objectOneValue = type.GetProperty(property.Name)?.GetValue(objectOne, null)?.ToString() ?? string.Empty;
                    }

                    if (type.GetProperty(property.Name)?.GetValue(objectTwo, null) != null)
                    {
                        objectTwoValue = type.GetProperty(property.Name)?.GetValue(objectTwo, null)?.ToString() ?? string.Empty;
                    }

                    if (objectOneValue.Trim() != objectTwoValue.Trim())
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private string FindTableForDatatype<T>()
        {
            if (!_databaseTables.TryGetValue(typeof(T), out var tableName))
            {
                throw new TableNotFoundException(typeof(T).ToString());
            }

            return tableName;
        }
    }

    // TODO: Implement Remove-Method
    // TODO: Implement Update-Method
    // TODO: Implement AddRange
    // TODO: Implement RemoveRange
}