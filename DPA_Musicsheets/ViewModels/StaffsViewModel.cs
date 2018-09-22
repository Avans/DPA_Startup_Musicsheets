using DPA_Musicsheets.Managers;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using PSAMControlLibrary;
using PSAMWPFControlLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Common.Interfaces;
using DPA_Musicsheets.Builders;
using DPA_Musicsheets.Managers.View;

namespace DPA_Musicsheets.ViewModels
{
    public class StaffsViewModel : ViewModelBase
    {
        // These staffs will be bound to.
        public ObservableCollection<MusicalSymbol> Staffs { get; }

        private readonly PsamViewManager _viewManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="musicLoader">We need the musicloader so it can set our staffs.</param>
        public StaffsViewModel(IViewManagerPool pool)
        {
            _viewManager = pool.GetInstance<PsamViewManager>();

//            _viewManager = (PsamViewManager) viewManagers.First(viewManager => viewManager is PsamViewManager);
            _viewManager.RegisterViewModel(this);

            Staffs = new ObservableCollection<MusicalSymbol>();
        }

        /// <summary>
        /// SetStaffs fills the observablecollection with new symbols. 
        /// We don't want to reset the collection because we don't want other classes to create an observable collection.
        /// </summary>
        /// <param name="symbols">The new symbols to show.</param>
        public void SetStaffs(IList<MusicalSymbol> symbols)
        {
            Staffs.Clear();
            foreach (var symbol in symbols)
            {
                Staffs.Add(symbol);
            }
        }
    }
}
