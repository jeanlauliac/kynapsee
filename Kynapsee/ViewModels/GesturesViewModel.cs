using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Timers;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Kynapsee.Nui.Model;
using Microsoft.Research.Kinect.Nui;
using Microsoft.Xna.Framework;
using Timer = System.Timers.Timer;

namespace Kynapsee.ViewModels
{
    public class GesturesViewModel : ViewModelBase, IDisposable
    {
        public ObservableCollection<GestureItem> Gestures { get; set; }
        public RelayCommand NewGestureCommand { get; set; }
        public RelayCommand SaveAndCloseCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }

        private Nui.Provider nuiProvider_;
        public Nui.Provider Provider { get { return nuiProvider_; } }

        private int recordDelay_ = 0;
        private GestureItem recordGesture_;

        private bool captureMode_;

        private GestureSet set_;
        public GestureSet GestureSet { get { return set_; } }

        private Gesture lastGesture_;
        private int lastGestureCount_;

        private Timer timer_;

        public GesturesViewModel(GestureSet set)
        {
            set_ = set;

            Gestures = new ObservableCollection<GestureItem>(set.Gestures.Select((x) => new GestureItem(this, x)));

            NewGestureCommand = new RelayCommand(NewGesture);
            SaveAndCloseCommand = new RelayCommand(() =>
                                               {
                                                   Save();
                                                   DialogResult = true;
                                               });
            SaveCommand = new RelayCommand(Save);

            try
            {
                StatusText = "Reading gestures...";
                nuiProvider_ = new Nui.Provider(set);

                nuiProvider_.CaptureEnd += NuiProviderCaptureEnd;
                nuiProvider_.CaptureProgress += NuiProviderCaptureProgress;
                nuiProvider_.GestureDone += NuiProviderGestureDone;
            }
            catch (InvalidOperationException)
            {
                StatusText = "No Kinect device connected.";
                nuiProvider_ = null;
            }
        }

        public void NewGesture()
        {
            var newGest = new GestureItem(this, new Gesture());
            Gestures.Add(newGest);
            SelectedGesture = newGest;
            newGest.IsEditing = true;
            
        }

        public void Save()
        {
            set_.Gestures.Clear();
            set_.Gestures.AddRange(Gestures.Select((x) => x.Gesture));
            
        }

        void NuiProviderCaptureEnd(object sender, Nui.NuiEventArgs e)
        {
            recordGesture_.Gesture.JointRecords = nuiProvider_.GetCapture();
            nuiProvider_.AddOrUpdateGesture(recordGesture_.Gesture);
            recordGesture_.Recorded = true;
            StatusText = "Gesture recorded: " + recordGesture_.Gesture.Name;
            captureMode_ = false;
            
        }

        void NuiProviderCaptureProgress(object sender, Nui.NuiEventArgs e)
        {
            StatusText = "Recording... " + (int)(100 * (float)e.RecordedFrameCount / Nui.Provider.BufferSize) + "%";
        }

        void NuiProviderGestureDone(object sender, Nui.NuiEventArgs e)
        {
            if (!captureMode_)
            {
                var st = "Gesture recognized: " + e.Gesture.Name;
                if (e.Gesture == lastGesture_)
                {
                    lastGestureCount_++;
                    st += " (" + lastGestureCount_ + ")";
                }
                else
                    lastGestureCount_ = 1;
                lastGesture_ = e.Gesture;
                StatusText = st;
            }
        }

        private GestureItem selectedGesture_;
        public GestureItem SelectedGesture
        {
            get { return selectedGesture_; }
            set
            {
                if (selectedGesture_ == value) return;
                if (selectedGesture_ != null)
                    selectedGesture_.IsEditing = false;
                selectedGesture_ = value;
                RaisePropertyChanged("SelectedGesture");
            }
        }

        private string statusText_;
        public string StatusText
        {
            get { return statusText_; }
            set
            {
                if (statusText_ == value) return;
                statusText_ = value;
                RaisePropertyChanged("StatusText");
            }
        }
        
        private bool? dialogResult_;
        public bool? DialogResult
        {
            get { return dialogResult_; }
            set
            {
                if (dialogResult_ == value) return;
                dialogResult_ = value;
                RaisePropertyChanged("DialogResult");
            }
        }

        public void Remove(GestureItem item)
        {
            Gestures.Remove(item);
            Provider.RemoveGesture(item.Gesture);
            
        }

        public void Record(GestureItem item)
        {
            timer_ = new Timer(1000);
            timer_.Elapsed += UpdateRecordTimer;
            recordDelay_ = 3;
            recordGesture_ = item;
            captureMode_ = true;
            UpdateRecordTimer(this, null);
            timer_.Start();
        }
        
        private void UpdateRecordTimer(object sender, ElapsedEventArgs e)
        {
            if (recordDelay_ <= 0)
            {
                StatusText = "Recording...";
                timer_.Dispose();
                nuiProvider_.StartCapture();
            }
            else
            {
                if (nuiProvider_.HasSkeleton)
                {
                    recordDelay_--;
                    StatusText = "Recording in " + recordDelay_ + " seconds...";
                }
                else
                {
                    recordDelay_ = 3;
                    StatusText = "Waiting for a skeleton...";
                }
            }
        }

        public new void Dispose()
        {
            
            if (nuiProvider_ != null)
                nuiProvider_.Dispose();

        }

    }
}
