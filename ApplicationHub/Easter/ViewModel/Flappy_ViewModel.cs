using ApplicationHub.Core;
using static ApplicationHub.Easter.Flappy;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System;
using ApplicationHub.Properties;

namespace ApplicationHub.Easter.ViewModel
{
    public class Flappy_ViewModel : ObservableObject
    {
        public Flappy Flappy { get; set; }

        private int _score; public int Score
        {
            get { return _score; }
            set { _score = value; OnPropertyChanged(); }
        }
        private string _pseudo; public string Pseudo
        {
            get { return _pseudo; }
            set { _pseudo = value; OnPropertyChanged(); }
        }


        private bool _isGameRunning; public bool IsGameRunning
        {
            get { return _isGameRunning; }
            set { _isGameRunning = value; OnPropertyChanged(); }
        }
        private bool _isNameInput; public bool IsNameInput
        {
            get { return _isNameInput; }
            set { _isNameInput = value; OnPropertyChanged(); }
        }



        public RelayCommand StartGameCommand { get; set; }
        public RelayCommand ValidateCommand { get; set; }


        private ObservableCollection<Player> PlayerDataList;
        public ICollectionView TopPlayerData { get; set; }
        public ICollectionView PlayerList { get; set; }

        private Dictionary<string, double> leaderBoardData;

        private string scoreFilepath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "metadata", @"data.txt");


        public Flappy_ViewModel(Flappy flappy)
        {
            this.Pseudo = Settings.Default.Pseudo;

            this.Flappy = flappy;

            this.PlayerDataList = new ObservableCollection<Player>();

            this.TopPlayerData = new CollectionViewSource { Source = this.PlayerDataList }.View;
            this.TopPlayerData.Filter = o => this.PlayerDataList.OrderByDescending(p => p.Score).ToList().IndexOf((Player)o) < 3;
            this.TopPlayerData.SortDescriptions.Add(new SortDescription("Score", ListSortDirection.Descending));

            this.PlayerList = new CollectionViewSource { Source = this.PlayerDataList }.View;
            this.PlayerList.Filter = o => !this.TopPlayerData.Contains(o);
            this.PlayerList.SortDescriptions.Add(new SortDescription("Score", ListSortDirection.Descending));

            this.RefreshLeaderboard();

            this.Flappy.OnGameStarted += Flappy_OnGameStarted;
            this.Flappy.OnGameEnded += Flappy_OnGameEnded;
            this.Flappy.OnGameScored += () => this.Score++;

            this.StartGameCommand = new RelayCommand(
                o => this.Flappy.LauchGame(),
                c => this.IsGameRunning == false);

            this.ValidateCommand = new RelayCommand(
                o => ComputeScore(),
                c => this.IsNameInput == true);
        }

        private void Flappy_OnGameStarted()
        {
            this.IsGameRunning = true;
        }

        private void Flappy_OnGameEnded()
        {
            this.IsGameRunning = false;
            this.IsNameInput = true;
            this.RefreshLeaderboard();
        }


        private void RefreshLeaderboard()
        {
            this.leaderBoardData = LeaderboardManager.GetLeaderboard(scoreFilepath);

            App.Current.Dispatcher.Invoke(() =>
            {
                this.PlayerDataList.Clear();
                foreach (var item in this.leaderBoardData)
                {
                    this.PlayerDataList.Add(new Player(item.Key, (int)item.Value));
                }
            });

            this.TopPlayerData.Refresh();
            this.PlayerList.Refresh();
        }

        private void ComputeScore()
        {
            //Update Score
            this.RefreshLeaderboard();

            Player[] topPlayers = PlayerDataList.OrderByDescending(o => o.Score).Take(3).ToArray();

            if (!string.IsNullOrEmpty(this.Pseudo))
            { 
                string pseudo = this.Pseudo;

                // Check if the pseudo already exists in the leaderboard
                if (this.leaderBoardData.ContainsKey(pseudo))
                {
                    if (this.Score > leaderBoardData[pseudo])
                        this.leaderBoardData[pseudo] = Score;
                }
                else
                {
                    this.leaderBoardData.Add(pseudo, Score);
                }

                LeaderboardManager.WriteLeaderboard(leaderBoardData, scoreFilepath);

                Settings.Default.Pseudo = pseudo;
                Settings.Default.Save();
            }

            if (this.Flappy.CloseGameAction != null)
                this.Flappy.CloseGameAction();

            this.IsNameInput = false;
        }
    }
}
