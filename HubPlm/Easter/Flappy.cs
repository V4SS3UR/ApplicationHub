using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ApplicationHub.Easter
{
    public partial class Flappy : Canvas, INotifyPropertyChanged
    {
        public delegate void GameStateEvent();
        public event GameStateEvent OnGameStarted;
        public event GameStateEvent OnGameEnded;
        public event GameStateEvent OnGameScored;

        private int score;
        private bool isGameRunning;
        private const int MAX_TUBE_NUMBER = 10;
        private Rectangle bird;
        private List<Rectangle> backgrounds;
        private List<Obstacle> obstacles;
        private double birdVelocity;
        private double tubesVelocity;
        private DispatcherTimer gameTimer;
        private Random random;

        public Action CloseGameAction;

        public Flappy()
        {
            this.Loaded += Flappy_Loaded;
            this.random = new Random();
            this.obstacles = new List<Obstacle>();
            this.backgrounds = new List<Rectangle>();
            this.ClipToBounds = true;
            this.Background = Brushes.LightBlue;
        }

        private void Flappy_Loaded(object sender, RoutedEventArgs e)
        {
            this.InitializeGame();
        }

        private void InitializeGame()
        {
            score = 0;
            birdVelocity = 0;
            tubesVelocity = 3;

            CreateBackground();
            CreateObstacle(MAX_TUBE_NUMBER);

            gameTimer = new DispatcherTimer(DispatcherPriority.Background, Application.Current.Dispatcher);
            gameTimer.Interval = TimeSpan.FromMilliseconds(10);

            gameTimer.Tick += LobyGameLoop;
            gameTimer.Start();
        }

        private void CreateObstacle(int number)
        {
            for (int i = 0; i < number; i++)
            {
                Obstacle Obstacle = new Obstacle(this, random, i * 200);
                obstacles.Add(Obstacle);
            }
        }
        private void ResetObstacles()
        {
            for (int i = 0; i < obstacles.Count; i++)
            {
                obstacles[i].DeleteTubes();
            }
            obstacles.Clear();
        }

        private void CreateBird()
        {
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Easter/Images/logoBackground.png"));
            imageBrush.Stretch = Stretch.UniformToFill;
            this.bird = new Rectangle
            {
                Width = 50,
                Height = 50,
                Fill = imageBrush,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };
            Canvas.SetTop(bird, this.ActualHeight / 2 - bird.ActualHeight / 2);
            Canvas.SetLeft(bird, this.ActualWidth / 4 - bird.ActualWidth / 2);
            this.Children.Add(bird);
        }
        private void CreateBackground()
        {
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Easter/Images/background.png"));

            int numberOfTile = (int)Math.Ceiling(this.ActualWidth * 2 / imageBrush.ImageSource.Width);
            double canvasRatio = this.ActualHeight / this.ActualWidth;
            for (int i = 0; i < numberOfTile; i++)
            {
                Rectangle background = new Rectangle
                {
                    Width = imageBrush.ImageSource.Width * canvasRatio,
                    Height = this.ActualHeight,
                    Fill = imageBrush,
                    Effect = new BlurEffect { Radius = 10 },
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    Opacity = 0.5,
                };
                Canvas.SetTop(background, 0);
                Canvas.SetLeft(background, i * background.Width);
                this.Children.Add(background);
                backgrounds.Add(background);
            }
        }

        public void LauchGame()
        {
            gameTimer.Stop();
            gameTimer.Tick -= LobyGameLoop;

            this.ResetObstacles();
            this.CreateObstacle(MAX_TUBE_NUMBER);

            this.CreateBird();
            isGameRunning = true;

            this.MouseDown += OnMouseDown;

            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            OnGameStarted?.Invoke();
        }

        private async void LobyGameLoop(object sender, EventArgs e)
        {
            await MoveObstaclesLobby();
            await MoveBackground();
        }
        private async void GameLoop(object sender, EventArgs e)
        {
            if (!isGameRunning)
                return;

            await MoveBird();
            await MoveObstacles();
            await MoveBackground();
            await CheckCollisions();
            await CheckBirdInScreen();
        }

        private async Task MoveBird()
        {
            // Update bird position
            if (birdVelocity < 4)
                birdVelocity += 0.2;

            Canvas.SetTop(bird, Canvas.GetTop(bird) + birdVelocity);

            // Rotate bird based on its velocity
            double rotationAngle = Math.Atan2(birdVelocity, 20) * 180 / Math.PI;
            bird.RenderTransform = new RotateTransform(rotationAngle, 0.5, 0.5);
        }
        private async Task MoveObstaclesLobby()
        {
            foreach (Obstacle obstacle in obstacles)
            {
                obstacle.Move(-tubesVelocity);
                obstacle.CheckRespawn();
            }
        }
        private async Task MoveObstacles()
        {
            foreach (Obstacle obstacle in obstacles)
            {
                obstacle.Move(-tubesVelocity);

                if (Canvas.GetLeft(obstacle.TopTube) <= (Canvas.GetLeft(bird) + bird.Width))
                {
                    // Check if bird passed the obstacle
                    if (obstacle.CheckIfBirdPassed(bird))
                    {
                        score++;
                        OnGameScored?.Invoke();

                        if (score % 10 == 0)
                            tubesVelocity += 1;
                    }
                }

                obstacle.CheckRespawn();
            }
        }
        private async Task MoveBackground()
        {
            foreach (Rectangle background in backgrounds)
            {
                Canvas.SetLeft(background, Canvas.GetLeft(background) - tubesVelocity * 0.7);

                if (Canvas.GetLeft(background) < -background.Width)
                {
                    Canvas.SetLeft(background, background.Width * (backgrounds.Count - 1));
                }
            }
        }
        private async Task CheckCollisions()
        {
            Rect birdBounds = new Rect(Canvas.GetLeft(bird), Canvas.GetTop(bird), bird.ActualWidth, bird.ActualHeight);

            foreach (Obstacle obstacle in obstacles)
            {
                if (obstacle.CheckCollisions(birdBounds))
                {
                    EndGame();
                    return;
                }
            }
        }
        private async Task CheckBirdInScreen()
        {
            // Check for bird going off screen
            if (Canvas.GetTop(bird) > this.ActualHeight || Canvas.GetTop(bird) < 0)
            {
                EndGame();
                return;
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            birdVelocity = -4;
        }

        private double GetNextObstacleOffset(Obstacle tube)
        {
            int tubeIndex = obstacles.IndexOf(tube);
            double nextOffset;

            switch (tubeIndex)
            {
                case 0:
                    nextOffset = Canvas.GetLeft(obstacles.Last().TopTube);
                    break;

                default:
                    nextOffset = Canvas.GetLeft(obstacles[tubeIndex - 1].TopTube);
                    break;
            }

            return nextOffset;
        }

        private void EndGame()
        {
            gameTimer.Stop();

            this.MouseDown -= OnMouseDown;

            OnGameEnded?.Invoke();
        }




        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}