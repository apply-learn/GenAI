using System.IO;
using System.Threading.Tasks;

namespace MafRagPoc.Services
{
    public static class DocumentLoader
    {
        public static async Task<string> LoadAsync(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Document not found: {path}");
            }

            return await File.ReadAllTextAsync(path);
        }
    }
}
