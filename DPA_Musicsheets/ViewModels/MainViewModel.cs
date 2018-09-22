using DPA_Musicsheets.Managers;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using PSAMWPFControlLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DPA_Musicsheets.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private string _fileName;
        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                _fileName = value;
                RaisePropertyChanged(() => FileName);
            }
        }

        /// <summary>
        /// The current state can be used to display some text.
        /// "Rendering..." is a text that will be displayed for example.
        /// </summary>
        private string _currentState;
        public string CurrentState
        {
            get { return _currentState; }
            set { _currentState = value; RaisePropertyChanged(() => CurrentState); }
        }

        private readonly MusicLoader _musicLoader;

        public MainViewModel(MusicLoader musicLoader)
        {
            _musicLoader = musicLoader;
            FileName = @"Files/Alle-eendjes-zwemmen-in-het-water.mid";
        }

        public ICommand OpenFileCommand => new RelayCommand(() =>
        {
            OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Midi or LilyPond files (*.mid *.ly)|*.mid;*.ly" };
            if (openFileDialog.ShowDialog() == true)
            {
                FileName = openFileDialog.FileName;
            }
        });

        public ICommand LoadCommand => new RelayCommand(() =>
        {
            _musicLoader.OpenFile(FileName);
        });

        #region Focus and key commands, these can be used for implementing hotkeys
        public ICommand OnLostFocusCommand => new RelayCommand(() =>
        {
            Console.WriteLine("Maingrid Lost focus");
        });

        public ICommand OnKeyDownCommand => new RelayCommand<KeyEventArgs>((e) =>
        {
            Console.WriteLine($"Key down: {e.Key}");
        });

        public ICommand OnKeyUpCommand => new RelayCommand(() =>
        {
            Console.WriteLine("Key Up");
        });

        public ICommand OnWindowClosingCommand => new RelayCommand(() =>
        {
            ViewModelLocator.Cleanup();
        });
        #endregion Focus and key commands, these can be used for implementing hotkeys
    }
}
