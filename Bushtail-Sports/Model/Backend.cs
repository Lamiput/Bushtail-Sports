using Bushtail_Sports.Utils;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Bushtail_Sports.Model
{
    public static class Backend
    {
        public static List<int> HWndList { get; private set; }
        public static List<BotEntry> BotList { get; private set; }
        private delegate bool CallbackDef(int hWnd, int lParam);


        [DllImport("user32.dll")]
        static extern bool SetWindowText(IntPtr hWnd, string text);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern int EnumWindows(CallbackDef callback, int lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(int hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetClientRect(IntPtr hwnd, out SearchImage.RECT lpRect);



        public static void RefreshHandle()
        {
            HWndList = new List<int>();
            CallbackDef callback = new CallbackDef(ShowWindowHandler);
            EnumWindows(callback, 0);
            ClientListChanged();
        }

        private static bool ShowWindowHandler(int hWnd, int lParam)
        {
            StringBuilder stringBuilder = new StringBuilder(255);
            GetWindowText(hWnd, stringBuilder, 255);
            string text = stringBuilder.ToString();
            if (text.Contains("NosTale"))
            {
                HWndList.Insert(0, hWnd);
                SetWindowText((IntPtr)hWnd, "NosTale - (" + hWnd.ToString() + ")");
            }
            return true;
        }

        public static bool ResetWindowNames()
        {
            if (HWndList == null)
            { return true; }
            if (HWndList.Count == 0)
            { return false; }
            foreach (int hWnd in HWndList)
            { SetWindowText((IntPtr)hWnd, "NosTale"); }
            return true;
        }

        public static bool AddBot(int _SelType, int _SelLevel, int _SelRewards, int _TargethWnd)
        {
            if (_TargethWnd == 0)
            { return false; }

            //check if target is already in use
            if (BotList.Any(c => c.Target == _TargethWnd))
            {
                MessageBox.Show("There is already a bot defined for this Instance",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            //check if bot is implemented for targets resolution
            SearchImage.RECT rect;
            GetClientRect((IntPtr)_TargethWnd, out rect);
            ResolutionType _Resolution;
            if (!Enum.TryParse("Res_" + rect.Right.ToString() + "x" + rect.Bottom.ToString(), out _Resolution))
            {
                MessageBox.Show("Unknown Resolution or not yet implemented\nConsider using 1440x900",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
#if DEBUG
                Console.WriteLine("Detected Resolution: Res_{1}x{2}", rect.Right.ToString(), rect.Bottom.ToString());
#endif
                return false;
            }

            //check if bot is implemented for type of game
            if (!Enum.IsDefined(typeof(MinigameType), _SelType))
            {
                MessageBox.Show("Unknown Type of Minigame or not yet implemented",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            //check if reward level is defined
            if (!Enum.IsDefined(typeof(RewardLevel), _SelLevel))
            {
                MessageBox.Show("Unknown reward Level",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            //check if number of desired number of rewards is positive
            if (_SelRewards <= 0)
            {
                MessageBox.Show("Number of desired rewards must be greater than 0",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }



            //show debug to console
#if DEBUG
            Console.WriteLine("{0} - desired {1} times {2} - detected Resolution: {3}, handle: {4}",
                (MinigameType)_SelType, _SelRewards, (RewardLevel)_SelLevel, _Resolution, _TargethWnd);
#endif

            BotList.Add(new BotEntry((MinigameType)_SelType, _TargethWnd, (RewardLevel)_SelLevel, _SelRewards, _Resolution));
            BotListChanged();
            return true;
        }

        public static bool StartBots()
        {
            return true;
        }

        public static bool StopBots()
        {
            return true;
        }

        public static bool StartSelBot(BotEntry _Bot)
        {
            return true;
        }

        public static bool StopSelBot(BotEntry _Bot)
        {
            return true;
        }

        public static bool DelBots()
        {
            BotList.Clear();
            BotListChanged();
            return true;
        }

        public static bool DelSelBot(BotEntry _Bot)
        {
            BotList.Remove(_Bot);
            BotListChanged();
            return true;
        }

        #region Events
        public static event ClientListChangedHandler ClientListChanged;
        public delegate void ClientListChangedHandler();

        public static event BotListChangedHandler BotListChanged;
        public delegate void BotListChangedHandler();
        #endregion

        static Backend()
        {
            HWndList = new List<int>();
            BotList = new List<BotEntry>();
        }
    }

    public enum MinigameType
    {
        //FishPond,
        //ShootingField,
        SawMill//,
        //Quarry
    }

    public enum RewardLevel
    {
        //Level_1,
        //Level_2,
        //Level_3,
        //Level_4,
        Level_5
    }

    public enum ResolutionType
    {
        //Res_1024x700 = 0,
        //Res_1024x768 = 1,
        //Res_1280x800 = 2,
        //Res_1280x1024 = 3,
        Res_1440x900 = 4//,
        //Res_1680x1050 = 5
    }


    public class BotEntry
    {
        public MinigameType Type { get; set; }
        public int Target { get; set; }
        public bool Running { get; set; }
        public RewardLevel DesiredLevel { get; set; }
        public int DesiredReward { get; set; }
        public int Progress { get; set; }
        public ResolutionType Resolution { get; set; }

        public BotEntry(MinigameType _Type, int _Target, RewardLevel _DesiredLevel,
            int _DesiredReward, ResolutionType _Resolution)
        {
            Type = _Type;
            Target = _Target;
            Running = false;
            DesiredLevel = _DesiredLevel;
            DesiredReward = _DesiredReward;
            Progress = 0;
            Resolution = _Resolution;
        }
    }
}
