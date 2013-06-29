using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Command;
using Kynapsee.Nui.Model;
using GalaSoft.MvvmLight;

namespace Kynapsee.ViewModels
{

    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visible = (bool)value;
            if ((string)parameter == "true")
                visible = !visible;
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GestureImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? new Uri(@"../Images/Gesture.png", UriKind.Relative) : new Uri(@"../Images/EmptyGesture.png", UriKind.Relative);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SetFocusTrigger : TargetedTriggerAction<Control>
    {
        protected override void Invoke(object parameter)
        {
            if (Target == null) return;
            Target.Focus();
        }
    }

    /// <summary>
    /// Handles an editable gesture.
    /// </summary>
    public class GestureItem : ViewModelBase
    {
        private GesturesViewModel model_;

        public GestureItem(GesturesViewModel model, Gesture gesture)
        {
            Gesture = gesture;
            model_ = model;
            EditCommand = new RelayCommand(() => { IsEditing = true; model_.SelectedGesture = this; });
            FinishEditingCommand = new RelayCommand(() => IsEditing = false);
            RemoveCommand = new RelayCommand(Remove);
            RecordCommand = new RelayCommand(Record, () => model.Provider != null);
            IsEditing = false;
            Recorded = gesture.JointRecords != null && gesture.JointRecords.Count > 0;
        }

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

        private bool recorded_;
        public bool Recorded
        {
            get { return recorded_; }
            set
            {
                if (recorded_ == value) return;
                recorded_ = value;
                RaisePropertyChanged("Recorded");
            }
        }

        public Gesture Gesture { get; set; }
        public RelayCommand EditCommand { get; set; }
        public RelayCommand RemoveCommand { get; set; }
        public RelayCommand FinishEditingCommand { get; set; }
        public RelayCommand RecordCommand { get; set; }
        


        public void Remove()
        {
            model_.Remove(this);
        }


        public void Record()
        {
            model_.Record(this);
        }

    }
}
