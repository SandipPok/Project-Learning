

using Quiz_App_VueJs.Models;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Quiz_App_VueJs.Data
{
    public class Repository : IRepository
    {
        public async Task<ICollection<T>> GetCollectionAsync<T>(string filePath, Func<T, bool>? filter = null)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            await using FileStream fs = File.OpenRead(filePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            var results = new List<T>();

            await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<T>(fs, options))
            {
                if (item == null)
                {
                    continue;
                }

                if (filter == null || filter(item))
                    results.Add(item);
            }

            return results;
        }

        public async Task<ICollection<T>> UpdateCollectionAsync<T>(
            string filePath,
            Func<T, bool>? filter = null,
            Action<T>? updateAction = null)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"File not found: {filePath}");

                var json = await File.ReadAllTextAsync(filePath);
                var collection = JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();

                // If update action is provided
                if (updateAction != null)
                {
                    // If no filter is provided, update all items
                    // Otherwise, update only the filtered items
                    var itemsToUpdate = filter == null ? collection : collection.Where(filter);

                    foreach (var item in itemsToUpdate)
                    {
                        updateAction(item);
                    }
                }

                var updatedJson = JsonSerializer.Serialize(collection, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                await File.WriteAllTextAsync(filePath, updatedJson);

                return collection;
            }
            catch
            {
                throw;
            }
        }
    }
}