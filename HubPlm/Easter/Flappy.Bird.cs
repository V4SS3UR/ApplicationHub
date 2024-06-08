using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ApplicationHub.Easter
{
    public partial class Flappy
    {
        public class Bird
        {
            public event Action OnBirdDeath;

            public int Score;
            public bool IsDead;

            private Flappy canvas;
            private double birdVelocity;

            public Rectangle Rectangle { get; set; }

            public Bird(Flappy canvas)
            {
                this.canvas = canvas;

                CreateBird();
            }

            private void CreateBird()
            {
                ImageBrush imageBrush = new ImageBrush();
                imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Easter/Images/logoBackground.png"));
                imageBrush.Stretch = Stretch.UniformToFill;

                Rectangle = new Rectangle
                {
                    Width = 50,
                    Height = 50,
                    Fill = new SolidColorBrush(Colors.Red),
                    RenderTransformOrigin = new Point(0.5, 0.5)
                };

                Canvas.SetTop(Rectangle, canvas.ActualHeight / 2 - Rectangle.ActualHeight / 2);
                Canvas.SetLeft(Rectangle, canvas.ActualWidth / 4 - Rectangle.ActualWidth / 2);
                canvas.Children.Add(Rectangle);
            }

            public async Task Move()
            {
                // Update bird position
                if (birdVelocity < 4)
                    birdVelocity += 0.2;

                Canvas.SetTop(Rectangle, Canvas.GetTop(Rectangle) + birdVelocity);

                // Rotate bird based on its velocity
                double rotationAngle = Math.Atan2(birdVelocity, 20) * 180 / Math.PI;
                Rectangle.RenderTransform = new RotateTransform(rotationAngle, 0.5, 0.5);
            }

            public void Jump()
            {
                birdVelocity = -4;
            }

            public async Task<bool> CheckCollisions(Obstacle currentObstacle)
            {
                //Check if bird is colliding with the current obstacle
                if (currentObstacle != null)
                {
                    Rect birdBounds = new Rect(Canvas.GetLeft(Rectangle), Canvas.GetTop(Rectangle), Rectangle.ActualWidth, Rectangle.ActualHeight);

                    var topTube = currentObstacle.TopTube;
                    Rect TopObstacleBounds = new Rect(Canvas.GetLeft(topTube), Canvas.GetTop(topTube), topTube.ActualWidth, topTube.ActualHeight);
                    if (birdBounds.IntersectsWith(TopObstacleBounds))
                    {
                        return true;
                    }

                    var bottomTube = currentObstacle.BottomTube;
                    Rect BottomObstacleBounds = new Rect(Canvas.GetLeft(bottomTube), Canvas.GetTop(bottomTube), bottomTube.ActualWidth, bottomTube.ActualHeight);
                    if (birdBounds.IntersectsWith(BottomObstacleBounds))
                    {
                        return true;
                    }
                }
                else
                {
                    throw new Exception("No next obstacle found");
                }

                return false;
            }
            public async Task<bool> CheckIfBirdPassed(Obstacle currentObstacle)
            {
                if (currentObstacle.HasBeenPassed)
                {
                    return false;
                }

                if (Canvas.GetLeft(currentObstacle.TopTube) + currentObstacle.TopTube.ActualWidth < Canvas.GetLeft(Rectangle))
                {
                    currentObstacle.HasBeenPassed = true;
                    return true;
                }

                return false;
            }
            public async Task<bool> CheckInScreen()
            {
                // Check for bird going off screen
                if (Canvas.GetTop(Rectangle) > canvas.ActualHeight || Canvas.GetTop(Rectangle) < 0)
                {
                    return false;
                }

                return true;
            }

            public Obstacle GetCurrentObstacle(IEnumerable<Obstacle> obstacles)
            {
                //Get the current collision base on current bird and obstacles position
                var obstaclesList = obstacles.OrderBy(o => Canvas.GetLeft(o.TopTube)).ToArray();

                Obstacle currentObstacle = null;
                foreach (var obstacle in obstaclesList)
                {
                    if (Canvas.GetLeft(obstacle.TopTube) + obstacle.TopTube.Width < Canvas.GetLeft(Rectangle))
                    {
                        continue;
                    }
                    if (Canvas.GetLeft(obstacle.TopTube) + obstacle.TopTube.Width > Canvas.GetLeft(Rectangle))
                    {
                        currentObstacle = obstacle;
                        break;
                    }
                }

                return currentObstacle;
            }

            public void KillBird()
            {
                canvas.Children.Remove(Rectangle);
                IsDead = true;
                OnBirdDeath?.Invoke();
            }
        }
    }
}