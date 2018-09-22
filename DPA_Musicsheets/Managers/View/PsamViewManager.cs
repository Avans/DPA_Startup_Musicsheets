using DPA_Musicsheets.Builders;
using DPA_Musicsheets.ViewModels;
using Common.Interfaces;
using Common.Models;
using Note = Common.Models.Note;
using Rest = Common.Models.Rest;

namespace DPA_Musicsheets.Managers.View
{
    public class PsamViewManager : IViewManager
    {
        private readonly PsamViewBuilder _builder;
        private StaffsViewModel _viewModel;

        public PsamViewManager()
        {
            _builder = new PsamViewBuilder();
        }

        public void RegisterViewModel(StaffsViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public void Load(Score score)
        {
            _builder.Reset(); // reset builder so symbols don't stack

            _builder.AddClef(score.Clef);
            _builder.AddTimeSignature(score.SymbolGroups[0].Meter);

            foreach (var symbolGroup in score.SymbolGroups)
            {
                _builder.AddTimeSignature(symbolGroup.Meter);

                foreach (var symbol in symbolGroup.Symbols)
                {
                    if (symbol is Note note)
                    {
                        _builder.AddNote(note);
                    }

                    if (symbol is Rest rest)
                    {
                        _builder.AddRest(rest);
                    }
                }
            }

            _viewModel?.SetStaffs(_builder.Build());
        }
    }
}
