using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Bushtail_Sports.Utils;

namespace Bushtail_Sports.Viewmodel
{
    public class VM_BotSetup : ViewModelBase
    {
        #region Commands
        public ICommand ICRefreshClientList { get; set; }
        private void RefreshClientList(object obj)
        { Model.Backend.RefreshHandle(); }

        public ICommand ICAddBot { get; set; }
        private void AddBot(object obj)
        { Model.Backend.AddBot(SelGameType, TargetLevel, TargetRewards, SelClient); }

        public ICommand ICStopBots { get; set; }
        private void StopBots(object obj)
        { Model.Backend.StopBots(); }
        #endregion

        #region Binded Properties

        public List<string> GameTypes
        {
            get => Enum.GetNames(typeof(Model.MinigameType)).ToList();
        }

        public int SelGameType
        {
            get => _SelGameType;
            set { SetProperty(ref _SelGameType, value); }
        }
        private int _SelGameType;

        public int TargetRewards
        {
            get => _TargetRewards;
            set { SetProperty(ref _TargetRewards, value); }
        }
        private int _TargetRewards;

        public List<string> TargetLevels
        {
            get => Enum.GetNames(typeof(Model.RewardLevel)).ToList();
        }
        public int TargetLevel
        {
            get => _TargetLevel;
            set { SetProperty(ref _TargetLevel, value); }
        }
        private int _TargetLevel;

        public List<int> ClientList
        {
            get => _ClientList;
            private set { SetProperty(ref _ClientList, value); }
        }
        private List<int> _ClientList = new List<int>();

        public int SelClient
        {
            get => _SelClient;
            set { SetProperty(ref _SelClient, value); }
        }
        private int _SelClient;
        #endregion


        public VM_BotSetup() : this(0, 0, 20) { }
        public VM_BotSetup(int _SelGameTypeInit, int _TargetLevelInit, int _TargetRewardsInit)
        {
            ICRefreshClientList = new RelayCommand(RefreshClientList);
            ICAddBot = new RelayCommand(AddBot);
            ICStopBots = new RelayCommand(StopBots);

            SelGameType = _SelGameTypeInit;
            TargetLevel = _TargetLevelInit;
            TargetRewards = _TargetRewardsInit;

            Model.Backend.ClientListChanged += UpdateClientList;
        }

        private void UpdateClientList()
        {
            ClientList = Model.Backend.HWndList;
        }
    }
}
