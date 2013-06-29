using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Kynapsee.Nui.Model;

namespace Kynapsee.ViewModels
{
    public class TransitionItem : ViewModelBase
    {
        private TransitionsViewModel vm_;

        public TransitionItem(TransitionsViewModel vm, Transition tr)
        {
            vm_ = vm;
            Transition = tr;
            EditCommand = new RelayCommand(() => { IsEditing = true; vm_.SelectedTransition = this; });
            FinishEditingCommand = new RelayCommand(() => IsEditing = false);
            RemoveCommand = new RelayCommand(Remove);
        }
        
        public Transition Transition { get; set; }

        private bool isEditing_;
        public bool IsEditing
        {
            get { return isEditing_; }
            set
            {
                if (isEditing_ == value) return;
                isEditing_ = value;
                RaisePropertyChanged("IsEditing");
            }
        }

        public RelayCommand EditCommand { get; set; }
        public RelayCommand RemoveCommand { get; set; }
        public RelayCommand FinishEditingCommand { get; set; }

        public void Remove()
        {
            vm_.Remove(this);
        }

    }
}
