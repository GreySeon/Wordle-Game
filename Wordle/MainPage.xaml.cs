namespace Wordle
{
    public partial class MainPage : ContentPage
    {
        private const int MaxAttempts = 6;
        private string WordToGuess = "APPLE"; // Hardcoded for now. Replace with dynamic logic.
        private int CurrentRow = 0;
        private const string WordListUrl = "https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/words.txt";

        public MainPage()
        {
            InitializeComponent();
            InitializeGrid();
            InitializeGame();
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
            return words[randomIndex].ToUpper(); // Ensure uppercase for consistency
        }

        private void InitializeGrid()
        {
            for (int row = 0; row < MaxAttempts; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    var frame = new Frame
                    {
                        BackgroundColor = Colors.White,
                        BorderColor = Colors.Black, // Adds a border
                        CornerRadius = 0, // Keeps it rectangular
                        WidthRequest = 50,
                        HeightRequest = 50,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        Content = new Label
                        {
                            Text = string.Empty,
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center,
                            FontSize = 18,
                            TextColor = Colors.Black
                        }
                    };

                    // Add the frame to the grid
                    GuessGrid.Children.Add(frame);
                    Grid.SetRow(frame, row); // Set the row
                    Grid.SetColumn(frame, col); // Set the column
                }
            }
        }

        private void SubmitButton_Clicked(object sender, EventArgs e)
        {
            string guess = GuessEntry.Text?.ToUpper();

            if (string.IsNullOrWhiteSpace(guess) || guess.Length != 5)
            {
                FeedbackLabel.Text = "Please enter a valid 5-letter word.";
                FeedbackLabel.IsVisible = true;
                return;
            }

            FeedbackLabel.IsVisible = false;
            ProcessGuess(guess);
        }

        private void ProcessGuess(string guess)
        {
            if (CurrentRow >= MaxAttempts)
            {
                FeedbackLabel.Text = "Game Over! You've used all attempts.";
                FeedbackLabel.TextColor = Colors.Red;
                FeedbackLabel.IsVisible = true;
                return;
            }

            for (int col = 0; col < 5; col++)
            {
                var frame = (Frame)GuessGrid.Children[CurrentRow * 5 + col];
                frame.BackgroundColor = GetLetterColor(guess[col], col);

                if (frame.Content is Label label)
                {
                    label.Text = guess[col].ToString();
                }
            }

            if (guess == WordToGuess)
            {
                FeedbackLabel.Text = "Congratulations! You guessed the word!";
                FeedbackLabel.TextColor = Colors.Green;
                FeedbackLabel.IsVisible = true;
                return;
            }

            CurrentRow++;
            GuessEntry.Text = string.Empty;

            if (CurrentRow == MaxAttempts)
            {
                FeedbackLabel.Text = $"Game Over! The word was {WordToGuess}.";
                FeedbackLabel.TextColor = Colors.Red;
                FeedbackLabel.IsVisible = true;
            }
        }

        private Color GetLetterColor(char letter, int position)
        {
            if (WordToGuess[position] == letter)
                return Colors.Green; // Correct position
            if (WordToGuess.Contains(letter))
                return Colors.Yellow; // Wrong position
            return Colors.Gray; // Not in the word
        }
    }

}
