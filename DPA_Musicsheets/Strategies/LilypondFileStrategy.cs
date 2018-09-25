using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Common.Definitions;
using Common.Interfaces;
using Common.Models;
using DPA_Musicsheets.Managers;
using DPA_Musicsheets.ViewModels;

namespace DPA_Musicsheets.Strategies
{
    class LilypondFileStrategy : IFileStrategy
    {
        private readonly IViewManagerPool _pool;

        public string LilypondText { get; set; }
        public LilypondViewModel LilypondViewModel { get; set; }

        public LilypondFileStrategy(IViewManagerPool pool)
        {
            _pool = pool;
        }

        public void Handle(string filename)
        {
            // Even aanmaken voor test, zodat ik load kan aanroepen
            var clef = Clefs.Alto;

            var note = new Note(Names.C, Octaves.Four, Durations.Quarter);
            note.Modifier = Modifiers.Sharp;

            var noteList = new List<Symbol>();
            noteList.Add(note);

            var symbolGroup = new SymbolGroup
            {
                Meter = new TimeSignature { Beat = Durations.Quarter, Ticks = 4 },
                Tempo = 120,
                Symbols = noteList,
            };

            var symbolGroups = new List<SymbolGroup>();
            symbolGroups.Add(symbolGroup);

            var score = new Score { Clef = clef, SymbolGroups = symbolGroups };
            foreach (var viewManager in _pool)
            {
                viewManager.Load(score); // This will set this.LilypondViewModel not empty;
            }

            /*
                Verplaatst naar LilypondViewManager
                StringBuilder sb = new StringBuilder();
                foreach (var line in File.ReadAllLines(filename))
                {
                    sb.AppendLine(line);
                }

                this.LilypondText = sb.ToString();
                this.LilypondViewModel.LilypondTextLoaded(this.LilypondText);
            */
        }
    }
}
