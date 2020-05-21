using Bushtail_Sports.Model;
using Bushtail_Sports.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Bushtail_Sports.Viewmodel
{
    public class VM_BotControl : Utils.ViewModelBase
    {
        public ICommand ICStartAllBots { get; set; }
        private void StartAllBots(object obj)
        { Model.Backend.StartBots(); }

        public ICommand ICStartSelBot { get; set; }
        private void StartSelBot(object obj)
        { }

        public ICommand ICStopAllBots { get; set; }
        private void StopAllBots(object obj)
        { Model.Backend.StopBots(); }

        public ICommand ICStopSelBot { get; set; }
        private void StopSelBot(object obj)
        { }

        public ICommand ICDelAllBots { get; set; }
        private void DelAllBots(object obj)
        { Model.Backend.DelBots(); }

        public ICommand ICDelSelBot { get; set; }
        private void DelSelBot(object obj)
        { Model.Backend.DelSelBot(SelBot); }

        public List<Model.BotEntry> BotList
        {
            get => _BotList;
            set { SetProperty(ref _BotList, value); }
        }
        private List<Model.BotEntry> _BotList;

        public BotEntry SelBot
        {
            get => _SelBotID;
            set { SetProperty(ref _SelBotID, value); }
        }
        private BotEntry _SelBotID;

        public VM_BotControl()
        {
            ICStartAllBots = new RelayCommand(StartAllBots);
            ICStartSelBot = new RelayCommand(StartSelBot);
            ICStopAllBots = new RelayCommand(StopAllBots);
            ICStopSelBot = new RelayCommand(StopSelBot);
            ICDelAllBots = new RelayCommand(DelAllBots);
            ICDelSelBot = new RelayCommand(DelSelBot);

            Model.Backend.BotListChanged += UpdateBotList;
        }

        private void UpdateBotList()
        {
            BotList = null; //dirty hack to force refreshing
            BotList = Model.Backend.BotList;
        }
    }
}
