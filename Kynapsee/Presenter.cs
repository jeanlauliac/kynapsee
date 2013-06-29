using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Kynapsee.Nui.Model;
using Microsoft.Office.Interop.PowerPoint;
using Application = Microsoft.Office.Interop.PowerPoint.Application;

namespace Kynapsee
{
    public class Presenter : IDisposable
    {
        private Application app_;
        private SlideShowWindow win_;
        private GestureSet gestures_;
        private TransitionSet transitions_;

        private Nui.Provider provider_;

        public Presenter(Application app, SlideShowWindow win)
        {
            app_ = app;
            win_ = win;

            try
            {
                provider_ = new Nui.Provider();
            }
            catch (InvalidOperationException)
            {
                provider_ = null;

                if (MessageBox.Show(
                    "Kinect is not detected. Check the connection and device.\nWould you like to disable Kinect for the presentation?",
                    "Kynapsee", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                {
                    win_.Presentation.SetKinectEnable(false);
                    return;
                }
                win_.View.Exit();
                return;
            }

            gestures_ = win.Presentation.GetNuiModel();

            provider_.GestureDone += GestureDone;
            app.SlideShowNextSlide += SlideShowNextSlide;

            LoadSlideGestures();
        }

        void GestureDone(object sender, Nui.NuiEventArgs e)
        {
            var tr = transitions_.Transitions.Where((x) => x.Gesture == e.Gesture).First();
            switch (tr.Method)
            {
                case TransitionMethod.NextSlide:
                    win_.View.Next();
                    break;
                case TransitionMethod.PreviousSlide:
                    win_.View.Previous();
                    break;
                case TransitionMethod.TargetSlide:
                    win_.View.GotoSlide(tr.TargetSlide.SlideIndex);
                    break;
            }
        }

        void SlideShowNextSlide(SlideShowWindow win)
        {
            LoadSlideGestures();
        }

        void LoadSlideGestures()
        {
            provider_.ClearGestures();

            transitions_ = win_.View.Slide.GetTransitions(win_.Presentation, gestures_);
            foreach (var tr in transitions_.Transitions)
            {
                provider_.AddOrUpdateGesture(tr.Gesture);
            }

        }

        public void Dispose()
        {
            if (provider_ != null)
            {
                app_.SlideShowNextSlide -= SlideShowNextSlide;
                provider_.GestureDone -= GestureDone;
                provider_.Dispose();
            }
        }
    }
}
