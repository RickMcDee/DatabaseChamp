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

        public void Add<T>(object objectToAdd)
        {
            var tableName = FindTableForDatatype<T>();

            if (objectToAdd.GetType() == typeof(T))
            {
                var path = Path.Combine(_databaseFolder, $"{tableName}.json");
                var existingFileStringContent = File.ReadAllText(path);
                var existingFileContent = JsonSerializer.Deserialize<List<T>>(existingFileStringContent, _jsonSerializerOptions)!;
                existingFileContent.Add((T)objectToAdd);
                File.WriteAllText(path, JsonSerializer.Serialize(existingFileContent, _jsonSerializerOptions));
            }
            else
            {
                throw new WrongDatatypeException(typeof(T).ToString(), objectToAdd.GetType().ToString());
            }
        }

        public IEnumerable<T> GetAll<T>()
        {
            var tableName = FindTableForDatatype<T>();

            var path = Path.Combine(_databaseFolder, $"{tableName}.json");
            var existingFileStringContent = File.ReadAllText(path);
            return JsonSerializer.Deserialize<IEnumerable<T>>(existingFileStringContent, _jsonSerializerOptions)!;
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
}