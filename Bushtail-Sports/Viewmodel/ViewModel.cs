using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bushtail_Sports.Utils;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;

namespace Bushtail_Sports.Viewmodel
{
    public class ViewModel : ViewModelBase
    {
        private HamburgerMenuItemCollection _menuItems;
        private HamburgerMenuItemCollection _menuOptionItems;

        public ViewModel()
        {
            LoadModels();
            CreateMenuItems();
        }

        ~ViewModel()
        {
            Model.Backend.ResetWindowNames();
        }

        public VM_BotSetup VM_BotSetup { get; private set; }
        public VM_BotControl VM_BotControl { get; private set; }
        public VM_Settings VM_Settings { get; private set; }

        private void LoadModels()
        {
            VM_BotSetup = new VM_BotSetup();
            VM_BotControl = new VM_BotControl();
            VM_Settings = new VM_Settings();
        }

        public void CreateMenuItems()
        {
            MenuItems = new HamburgerMenuItemCollection
            {
                new HamburgerMenuIconItem()
                {
                    Icon = new PackIconMaterial() {Kind = PackIconMaterialKind.Wrench},
                    Label = "BotSetup",
                    ToolTip = "Create Bot Instances",
                    Tag = VM_BotSetup
                },
                new HamburgerMenuIconItem()
                {
                    Icon = new PackIconMaterial() {Kind = PackIconMaterialKind.PlayBoxMultiple},
                    Label = "BotControl",
                    ToolTip = "Start and Stop the Bots",
                    Tag = VM_BotControl
                },
                new HamburgerMenuIconItem()
                {
                    Icon = new PackIconMaterial() {Kind = PackIconMaterialKind.SettingsHelper},
                    Label = "Settings",
                    ToolTip = "Remanent Settings",
                    Tag = VM_Settings
                },
            };

        }

        public HamburgerMenuItemCollection MenuItems
        {
            get => _menuItems;
            set { SetProperty(ref _menuItems, value); }
        }

        public HamburgerMenuItemCollection MenuOptionItems
        {
            get => _menuOptionItems;
            set { SetProperty(ref _menuOptionItems, value); }
        }


    }
}
