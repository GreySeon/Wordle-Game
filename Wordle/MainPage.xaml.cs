using Plugin.Maui.Audio;

namespace Wordle
{
    public partial class MainPage : ContentPage
    {
        // Declaring global variables
        private const int MaxAttempts = 6;
        private string WordToGuess;
        private int CurrentAttempt = 0;
        private const string WordListUrl = "https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/words.txt";
        private IDispatcherTimer GameTimer;
        private TimeSpan ElapsedTime;
        private IAudioPlayer AudioPlayer;

        public MainPage()
        {
            InitializeComponent();
            InitializeGame();
            StartNewGame();
        }

        private async Task InitializeGame()
        {
            try
            {
                await WordListHelper.EnsureWordListAsync(WordListUrl); // Ensure the word list file exists

                // Load the word list and pick a random word
                string[] words = await WordListHelper.GetWordListAsync();
                WordToGuess = SelectRandomWord(words);
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
            // Check if the word list exists
            if (words == null || words.Length == 0)
                throw new InvalidOperationException("The word list is empty or null.");

            // Choose the word using built-in randomiser
            Random random = new();
            int randomIndex = random.Next(words.Length);
            return words[randomIndex].ToUpper(); // Uppercase for consistency
        }

        private async void StartNewGame()
        {
            CurrentAttempt = 0;
            GuessContainer.Children.Clear();
            FeedbackLabel.IsVisible = false;
            await InitializeGame();
            GenerateNewRow();
            StartTimer();
        }

        private async void GenerateNewRow()
        {
            // End the game (over) and don't allow for the next row to be created
            if (CurrentAttempt >= MaxAttempts)
            {
                FeedbackLabel.Text = $"Game Over! The correct word was {WordToGuess}.";
                FeedbackLabel.TextColor = Colors.Red;
                FeedbackLabel.IsVisible = true;
                SubmitGuessButton.IsEnabled = false;
                StopTimer();
                AudioPlayer = AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("loss.mp3"));
                AudioPlayer.Play();
                return;
            }

            // Creating the row for entries
            var row = new HorizontalStackLayout
            {
                Spacing = 5,
                HorizontalOptions = LayoutOptions.Center,
                Opacity = 0 // For animation to work properly
            };

            // Adding 5 similar entries for each letter
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
            await row.FadeTo(1, 1000);
        }

        private async void SubmitGuessButton_Clicked(object sender, EventArgs e)
        {
            FeedbackLabel.IsVisible = false;

            // Cancel the task if all guesses were made
            if (CurrentAttempt >= MaxAttempts)
                return;

            // Get the current row
            var row = GuessContainer.Children[CurrentAttempt] as HorizontalStackLayout;
            var entries = row?.Children.OfType<Entry>().ToArray();

            // Check for all entries to have a letter
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

            // Update colours for each letter
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

            // In case of win
            if (guess == WordToGuess)
            {
                FeedbackLabel.Text = "Congratulations! You guessed the word!";
                FeedbackLabel.TextColor = Colors.Green;
                FeedbackLabel.IsVisible = true;
                SubmitGuessButton.IsEnabled = false;
                StopTimer();
                AudioPlayer = AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("win.mp3"));
                AudioPlayer.Play();
                return;
            }

            CurrentAttempt++;
            GenerateNewRow();
        }

        private void NewGameButton_Clicked(object sender, EventArgs e)
        {
            StartNewGame();
            SubmitGuessButton.IsEnabled = true;
            StopTimer(); // Stop the previous timer
            StartTimer(); // Start a new one
        }

        private void StartTimer()
        {
            StopTimer(); // Stop the current timer if it is running

            if (Dispatcher == null)
                return;

            ElapsedTime = TimeSpan.Zero; // Reset elapsed time to zero
            GameTimer = Dispatcher.CreateTimer(); // Create the timer instance
            GameTimer.Interval = TimeSpan.FromSeconds(1); // with interval being 1 second
            GameTimer.Tick += UpdateTimer;
            GameTimer.Start();
        }

        private void UpdateTimer(object sender, EventArgs e)
        {
            ElapsedTime = ElapsedTime.Add(TimeSpan.FromSeconds(1)); // Increment elapsed time for 1
            TimerLabel.Text = $"Time: {ElapsedTime:mm\\:ss}";
        }

        private void StopTimer()
        {
            if (GameTimer != null)
            {
                GameTimer.Stop();
                GameTimer.Tick -= UpdateTimer;
                GameTimer = null;
            }
        }
    }

}
