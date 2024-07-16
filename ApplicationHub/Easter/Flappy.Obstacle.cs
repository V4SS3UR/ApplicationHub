using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace ApplicationHub.Easter
{
    public partial class Flappy
    {
        public class Obstacle
        {
            private Flappy canvas;
            public Rectangle TopTube { get; private set; }
            public Rectangle BottomTube { get; private set; }

            public Rectangle TopCap { get; private set; }
            public Rectangle BottomCap { get; private set; }


            public bool HasBeenPassed { get; set; }

            private Random random;
            private double width;
            private double height;
            private double gapHeight;

            public Obstacle(Flappy canvas, Random random, double offset)
            {
                this.canvas = canvas;
                this.random = random;
                this.width = 50;

                CreateTubes(offset);
                canvas.Children.Add(TopTube);
                canvas.Children.Add(BottomTube);

                CreateCaps(offset);
                canvas.Children.Add(TopCap);
                canvas.Children.Add(BottomCap);
            }

            public void CreateTubes(double offset)
            {
                this.height = random.Next(30, 150);
                this.gapHeight = random.Next(100, 200);

                // Create an image brush from an image file
                ImageBrush tubeImage = new ImageBrush();
                tubeImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Easter/Images/tube.png"));
                tubeImage.Stretch = Stretch.Fill;

                TopTube = new Rectangle
                {
                    Width = width,
                    Height = height,
                    Fill = tubeImage
                };

                BottomTube = new Rectangle
                {
                    Width = width,
                    Height = canvas.ActualHeight - height - gapHeight,
                    Fill = tubeImage
                };

                Canvas.SetTop(TopTube, 0);
                Canvas.SetTop(BottomTube, height + gapHeight);
                Canvas.SetLeft(TopTube, canvas.ActualWidth + offset);
                Canvas.SetLeft(BottomTube, canvas.ActualWidth + offset);
            }
            public void DeleteTubes()
            {
                canvas.Children.Remove(TopTube);
                canvas.Children.Remove(BottomTube);
                canvas.Children.Remove(TopCap);
                canvas.Children.Remove(BottomCap);
            }

            public void CreateCaps(double offset)
            {
                double capHeight = (width * 1.2) / 2.5;
                double capWidth = width * 1.2;

                ImageBrush capImage = new ImageBrush();
                capImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Easter/Images/cap.png"));
                capImage.Stretch = Stretch.UniformToFill;

                TopCap = new Rectangle
                {
                    Width = capWidth,
                    Height = capHeight,
                    Fill = capImage,
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    RenderTransform = new RotateTransform(0, 0.5, 0.5)
                };

                BottomCap = new Rectangle
                {
                    Width = capWidth,
                    Height = capHeight,
                    Fill = capImage,
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    RenderTransform = new RotateTransform(180, 0.5, 0.5)
                };

                Canvas.SetTop(TopCap, height - capHeight + 3);
                Canvas.SetTop(BottomCap, height + gapHeight - 3);
                Canvas.SetLeft(TopCap, canvas.ActualWidth - (capWidth - width) / 2 + offset);
                Canvas.SetLeft(BottomCap, canvas.ActualWidth - (capWidth - width) / 2 + offset);
            }

            public void ReDraw(double offset)
            {
                double lastTubeOffset = canvas.GetNextObstacleOffset(this);
                offset += lastTubeOffset;

                this.height = random.Next(30, 150);
                this.gapHeight = random.Next(100, 200);

                TopTube.Height = height;
                BottomTube.Height = canvas.ActualHeight - height - gapHeight;

                Canvas.SetTop(TopTube, 0);
                Canvas.SetTop(BottomTube, height + gapHeight);
                Canvas.SetLeft(TopTube, offset);
                Canvas.SetLeft(BottomTube, offset);

                double capHeight = (width * 1.2) / 2.5;
                double capWidth = width * 1.2;

                Canvas.SetTop(TopCap, height - capHeight + 3);
                Canvas.SetTop(BottomCap, height + gapHeight - 3);
                Canvas.SetLeft(TopCap, Canvas.GetLeft(TopTube) - (capWidth - width) / 2);
                Canvas.SetLeft(BottomCap, Canvas.GetLeft(TopTube) - (capWidth - width) / 2);
            }

            public void Move(double y)
            {
                Canvas.SetLeft(TopTube, Canvas.GetLeft(TopTube) + y);
                Canvas.SetLeft(BottomTube, Canvas.GetLeft(BottomTube) + y);
                Canvas.SetLeft(TopCap, Canvas.GetLeft(TopCap) + y);
                Canvas.SetLeft(BottomCap, Canvas.GetLeft(BottomCap) + y);
            }


            public bool CheckIfBirdPassed(Rectangle bird)
            {
                if (HasBeenPassed) return false;

                if (Canvas.GetLeft(TopTube) + TopTube.ActualWidth < Canvas.GetLeft(bird))
                {
                    HasBeenPassed = true;
                    return true;
                }

                return false;
            }

            public void CheckRespawn()
            {
                // Respawn obstacle if it goes off screen
                if (Canvas.GetLeft(TopTube) < -TopTube.ActualWidth)
                {
                    //Respawn it with random offset
                    double rndDouble = new Random().Next(150, 250);
                    ReDraw(rndDouble);

                    HasBeenPassed = false;
                }
            }

            public bool CheckCollisions(Rect birdBounds)
            {
                Rect TopObstacleBounds = new Rect(Canvas.GetLeft(TopTube), Canvas.GetTop(TopTube), TopTube.ActualWidth, TopTube.ActualHeight);
                if (birdBounds.IntersectsWith(TopObstacleBounds))
                {
                    return true;
                }

                Rect BottomObstacleBounds = new Rect(Canvas.GetLeft(BottomTube), Canvas.GetTop(BottomTube), BottomTube.ActualWidth, BottomTube.ActualHeight);
                if (birdBounds.IntersectsWith(BottomObstacleBounds))
                {
                    return true;
                }

                return false;
            }
        }
    }
}
