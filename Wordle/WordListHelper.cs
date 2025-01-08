namespace Wordle
{
    public static class WordListHelper
    {
        private static readonly string WordListFileName = "wordlist.txt";
        private static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, WordListFileName);

        public static async Task EnsureWordListAsync(string fileUrl)
        {
            // Download the file from the internet if it doesn't exist
            if (!File.Exists(FilePath))
            {
                await DownloadAndSaveFileAsync(fileUrl);
            }
        }

        private static async Task DownloadAndSaveFileAsync(string fileUrl)
        {
            using HttpClient httpClient = new();
            string wordListContent = await httpClient.GetStringAsync(fileUrl);      
            await File.WriteAllTextAsync(FilePath, wordListContent); // Save the content to a file
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
