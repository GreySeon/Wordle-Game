using System.Net.Http;
using System.IO;
using System.Threading.Tasks;

namespace Wordle
{
    public static class WordListHelper
    {
        private static readonly string WordListFileName = "wordlist.txt";
        private static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, WordListFileName);

        public static async Task EnsureWordListAsync(string fileUrl)
        {
            if (!File.Exists(FilePath))
            {
                await DownloadAndSaveFileAsync(fileUrl);
            }
        }

        private static async Task DownloadAndSaveFileAsync(string fileUrl)
        {
            using HttpClient httpClient = new();
            string wordListContent = await httpClient.GetStringAsync(fileUrl);

            // Save the content to a file
            await File.WriteAllTextAsync(FilePath, wordListContent);
        }

        public static async Task<string[]> GetWordListAsync()
        {
            if (!File.Exists(FilePath))
            {
                throw new FileNotFoundException("Word list file not found.");
            }

            // Read all lines from the file
            string[] words = await File.ReadAllLinesAsync(FilePath);
            return words;
        }
    }
}
