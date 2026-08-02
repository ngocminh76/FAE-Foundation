using System.Collections.Generic;
using System.Windows.Input;
using FAE.Foundation.App.Core;
using FAE.Foundation.App.Services.Localization;
using FAE.Foundation.App.Features.RibbedRaft;

namespace FAE.Foundation.App.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public LocalizationService Localization => LocalizationService.Instance;

        // Current Foundation View Model
        private ObservableObject _currentFoundationViewModel;
        public ObservableObject CurrentFoundationViewModel
        {
            get => _currentFoundationViewModel;
            set => SetProperty(ref _currentFoundationViewModel, value);
        }

        // Language Commands
        public ICommand SetLanguageViCommand { get; }
        public ICommand SetLanguageEnCommand { get; }
        
        // Navigation Commands
        public ICommand OpenTheoryCommand { get; }

        public MainViewModel()
        {
            // Initialize with Ribbed Raft for now
            CurrentFoundationViewModel = new RibbedRaftViewModel();

            SetLanguageViCommand = new RelayCommand(_ => Localization.SetLanguage("vi-VN"));
            SetLanguageEnCommand = new RelayCommand(_ => Localization.SetLanguage("en-US"));
            
            OpenTheoryCommand = new RelayCommand(_ => 
            {
                var theoryWindow = new Views.TheoryWindow();
                theoryWindow.Show();
            });
        }
    }
}
