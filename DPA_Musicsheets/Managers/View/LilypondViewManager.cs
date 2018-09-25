using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Exceptions;
using Common.Interfaces;
using Common.Models;
using DPA_Musicsheets.ViewModels;

namespace DPA_Musicsheets.Managers.View
{
    class LilypondViewManager : IViewManager
    {
        private LilypondViewModel ViewModel { get; set; }

        public void Load(Score score)
        {
            if (ViewModel == null) throw new ViewModelNotFoundException();

            /*
                Todo: Zet score om naar een string (of iets anders)

                // Oude werking:
                StringBuilder sb = new StringBuilder();
                foreach (var line in File.ReadAllLines(filename)) 
                {
                    sb.AppendLine(line);
                }
                this.LilypondText = sb.ToString();
                this.LilypondViewModel.LilypondTextLoaded(this.LilypondText);
            */

            // Todo: Zorgt nu voor een foutmelding, omdat MainViewModel n MusicLoader gekoppeld is aan de LilypondViewModel (events).
            ViewModel.LilypondText = "To be continued";
            ViewModel.LilypondTextLoaded(ViewModel.LilypondText);
        }

        public void RegisterViewModel(LilypondViewModel viewModel)
        {
            ViewModel = viewModel;
        }
    }
}
