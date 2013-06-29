using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using Kynapsee.Nui.Model;
using Microsoft.Office.Interop.PowerPoint;

namespace Kynapsee.ViewModels
{

    public class TransitionsViewModel : ViewModelBase
    {
        private KynapseePaneModel submodel_;
        private TransitionSet set_;
        private SlideRange sr_;

        public List<TransitionMethod> Methods { get; set; }

        public TransitionsViewModel(KynapseePaneModel submodel)
        {
            submodel_ = submodel;

            Gestures = submodel_.Presentation.GetNuiModel();
            Methods = new List<TransitionMethod>()
                          {
                              TransitionMethod.NextSlide,
                              TransitionMethod.PreviousSlide,
                              TransitionMethod.TargetSlide
                          };

            sr_ = submodel_.SlideRange;
            set_ = sr_.GetTransitions(submodel_.Presentation, Gestures);
            Transitions = new ObservableCollection<TransitionItem>(set_.Transitions.Select((x)
                => new TransitionItem(this, x)));
            
            NewTransitionCommand = new RelayCommand(NewTransition);

            submodel_.PropertyChanged += KynapseePropertyChanged;

        }

        void KynapseePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Presentation":
                    if (submodel_.Presentation != null)
                        Gestures = submodel_.Presentation.GetNuiModel();
                    break;
                case "SlideRange":
                    sr_ = submodel_.SlideRange;
                    set_ = sr_.GetTransitions(submodel_.Presentation, Gestures);
                    Transitions = new ObservableCollection<TransitionItem>(set_.Transitions.Select((x)
                        => new TransitionItem(this, x)));
                    break;
            }
        }

        public RelayCommand NewTransitionCommand { get; set; }

        public void NewTransition()
        {
            var item = new TransitionItem(this, new Transition());
            set_.Transitions.Add(item.Transition);
            sr_.SetTransitions(set_);

            Transitions.Add(item);
            SelectedTransition = item;
            item.IsEditing = true;
            return;
        }

        private ObservableCollection<TransitionItem> transitions_;
        public ObservableCollection<TransitionItem> Transitions
        {
            get { return transitions_; }
            set
            {
                if (transitions_ == value) return;
                transitions_ = value;
                RaisePropertyChanged("Transitions");
            }
        }

        private GestureSet gestures_;
        public GestureSet Gestures
        {
            get { return gestures_; }
            set
            {
                if (gestures_ == value) return;
                gestures_ = value;
                RaisePropertyChanged("Gestures");
            }
        }

        private TransitionItem selectedTransition_;
        public TransitionItem SelectedTransition
        {
            get { return selectedTransition_; }
            set
            {
                if (selectedTransition_ == value) return;
                if (selectedTransition_ != null)
                {
                    if (sr_ != null)
                        sr_.SetTransitions(set_);
                    selectedTransition_.IsEditing = false;
                }
                selectedTransition_ = value;
                RaisePropertyChanged("SelectedTransition");
            }
        }

        public void Remove(TransitionItem item)
        {
            set_.Transitions.Remove(item.Transition);
            sr_.SetTransitions(set_);
            Transitions.Remove(item);
            
        }

    }
}
