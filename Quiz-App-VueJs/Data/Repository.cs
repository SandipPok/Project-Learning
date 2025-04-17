

using Quiz_App_VueJs.Models;
using System.Collections.ObjectModel;
using System.Text;
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
    }
}