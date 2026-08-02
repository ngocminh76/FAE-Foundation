using System.Collections.Generic;
using System.Windows.Input;
using FAE.Foundation.App.Core;
using FAE.Foundation.App.Services.Localization;
using FAE.Foundation.App.Features.RibbedRaft;
using FAE.Foundation.App.Features.Home;

namespace FAE.Foundation.App.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public LocalizationService Localization => LocalizationService.Instance;

        // Current Foundation View Model
        private ObservableObject _currentView;
        public ObservableObject CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        // Language Commands
        public ICommand SetLanguageViCommand { get; }
        public ICommand SetLanguageEnCommand { get; }
        
        // Navigation Commands
        public ICommand NavigateHomeCommand { get; }
        public ICommand NavigateRibbedRaftCommand { get; }
        public ICommand OpenTheoryCommand { get; }

        public MainViewModel()
        {
            // Start at the Home View
            CurrentView = new HomeViewModel();

            SetLanguageViCommand = new RelayCommand(_ => Localization.SetLanguage("vi-VN"));
            SetLanguageEnCommand = new RelayCommand(_ => Localization.SetLanguage("en-US"));
            
            NavigateHomeCommand = new RelayCommand(_ => CurrentView = new HomeViewModel());
            NavigateRibbedRaftCommand = new RelayCommand(_ => CurrentView = new RibbedRaftViewModel());
            
            OpenTheoryCommand = new RelayCommand(_ => 
            {
                var theoryWindow = new Views.TheoryWindow();
                theoryWindow.Show();
            });
        }
    }
}
