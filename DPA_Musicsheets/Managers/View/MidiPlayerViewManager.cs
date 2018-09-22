using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Interfaces;
using Common.Models;
using DPA_Musicsheets.ViewModels;

namespace DPA_Musicsheets.Managers.View
{
    class MidiPlayerViewManager : IViewManager
    {
        private MidiPlayerViewModel _viewModel;

        public MidiPlayerViewManager()
        {
        }

        public void RegisterViewModel(MidiPlayerViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public void Load(Score score)
        {
            // throw new NotImplementedException();

            // TODO: convert score to midi sequence, set _viewModel.MidiSequence
        }
    }
}
