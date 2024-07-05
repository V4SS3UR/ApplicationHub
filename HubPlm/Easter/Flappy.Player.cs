using ApplicationHub.Core;

namespace ApplicationHub.Easter
{
    public partial class Flappy
    {
        public class Player : ObservableObject
        {
            private string _name; public string Name
            {
                get { return _name; }
                set { _name = value; OnPropertyChanged(); }
            }

            private int _score; public int Score
            {
                get { return _score; }
                set { _score = value; OnPropertyChanged(); }
            }

            public Player(string name, int score)
            {
                this.Name = name;
                this.Score = score;
            }
        }
    }
}
