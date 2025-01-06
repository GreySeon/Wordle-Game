namespace Wordle
{
    public partial class MainPage : ContentPage
    {
        private const int MaxAttempts = 6;
        private string WordToGuess;
        private int CurrentAttempt = 0;
        private int CurrentRow = 0;
        private const string WordListUrl = "https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/words.txt";

        public MainPage()
        {
            InitializeComponent();
            InitializeGame();
            StartNewGame();
        }

        private async void InitializeGame()
        {
            try
            {
                // Ensure the word list file exists
                await WordListHelper.EnsureWordListAsync(WordListUrl);

                // Load the word list and pick a random word
                string[] words = await WordListHelper.GetWordListAsync();
                WordToGuess = SelectRandomWord(words);

                Console.WriteLine($"Word to guess (for debugging): {WordToGuess}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing game: {ex.Message}");
                FeedbackLabel.Text = "Error initializing game. Please restart the app.";
                FeedbackLabel.IsVisible = true;
            }
        }

        private string SelectRandomWord(string[] words)
        {
            if (words == null || words.Length == 0)
                throw new InvalidOperationException("The word list is empty or null.");

            Random random = new();
            int randomIndex = random.Next(words.Length);
            return words[randomIndex].ToUpper(); // Uppercase for consistency
        }

        private void StartNewGame()
        {
            CurrentAttempt = 0;
            GuessContainer.Children.Clear();
            FeedbackLabel.IsVisible = false;

            GenerateNewRow();
        }

        private void GenerateNewRow()
        {
            if (CurrentAttempt >= MaxAttempts)
            {
                FeedbackLabel.Text = $"Game Over! The correct word was {WordToGuess}.";
                FeedbackLabel.TextColor = Colors.Red;
                FeedbackLabel.IsVisible = true;
                SubmitGuessButton.IsEnabled = false;
                return;
            }

            var row = new HorizontalStackLayout
            {
                Spacing = 5,
                HorizontalOptions = LayoutOptions.Center
            };

            for (int i = 0; i < 5; i++)
            {
                row.Children.Add(new Entry
                {
                    WidthRequest = 40,
                    MaxLength = 1,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    FontSize = 18,
                    TextColor = Colors.Black,
                    BackgroundColor = Colors.White
                });
            }

            GuessContainer.Children.Add(row);
        }

        private void SubmitGuessButton_Clicked(object sender, EventArgs e)
        {
            FeedbackLabel.IsVisible = false;
            if (CurrentAttempt >= MaxAttempts)
                return;

            // Get the current row
            var row = GuessContainer.Children[CurrentAttempt] as HorizontalStackLayout;
            var entries = row?.Children.OfType<Entry>().ToArray();

            if (entries == null || entries.Length != 5)
                return;

            // Validate input
            string guess = string.Concat(entries.Select(entry => entry.Text?.ToUpper() ?? ""));
            if (guess.Length != 5 || !guess.All(char.IsLetter))
            {
                FeedbackLabel.Text = "Please enter a valid 5-letter word.";
                FeedbackLabel.IsVisible = true;
                return;
            }

            // Update feedback for each letter
            for (int i = 0; i < 5; i++)
            {
                var entry = entries[i];
                char letter = guess[i];
                if (WordToGuess[i] == letter)
                {
                    entry.BackgroundColor = Colors.Green;
                }
                else if (WordToGuess.Contains(letter))
                {
                    entry.BackgroundColor = Colors.Yellow;
                }
                else
                {
                    entry.BackgroundColor = Colors.Gray;
                }
                entry.IsEnabled = false;
            }

            if (guess == WordToGuess)
            {
                FeedbackLabel.Text = "Congratulations! You guessed the word!";
                FeedbackLabel.TextColor = Colors.Green;
                FeedbackLabel.IsVisible = true;
                SubmitGuessButton.IsEnabled = false;
                return;
            }

            CurrentAttempt++;
            GenerateNewRow();
        }

        private void NewGameButton_Clicked(object sender, EventArgs e)
        {
            StartNewGame();
            SubmitGuessButton.IsEnabled = true;
        }
    }

}
