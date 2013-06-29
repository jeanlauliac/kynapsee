using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using Kynapsee.ViewModels;
using Kynapsee.Views;
using Microsoft.Office.Interop.PowerPoint;
using Microsoft.Office.Tools;
using Microsoft.Office.Tools.Ribbon;
using Microsoft.Research.Kinect.Nui;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Office = Microsoft.Office.Core;
using System.Drawing;

namespace Kynapsee
{
    public partial class ThisAddIn
    {
        private Kinect kinect_ = null;
        public SlideShowWindow ssw_;
        public KynapseePane pane_;
        public RibbonFactory factory_;
        public Presentation pres_;
        public Selection sel_;

        private Presenter presenter_;

        private KynapseePaneModel paneModel_;

        private Dictionary<Presentation, CustomTaskPane> transitionTaskPanes_ = new Dictionary<Presentation,CustomTaskPane>();

        public Dictionary<JointID, Shape> trackedShapes_ = new Dictionary<JointID,Shape>();
        /*
        public Kinect Kinect
        {
            get
            {
                if (kinect_ == null)
                    LoadKinect();
                return kinect_;
            }
        }
        
        public void LoadKinect()
        {
            if (kinect_ != null)
                return;
            try
            {
                kinect_ = new Kinect();
                kinect_.GestureDone += kinect__GestureDone;
                kinect_.Init();
            }
            catch (Exception e)
            {
                kinect_ = null;
            }
            //kinect_.LoadGesturesFromFile("Kynapsee.gestures.txt");
            using (var str = new StringReader(pres_.Tags["KY_Gestures"]))
                kinect_.LoadGesturesFromFile(str);
        }

        public void UnloadKinect()
        {
            if (kinect_ == null)
                return;
            kinect_.Uninit();
            kinect_ = null;
        }
        */
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            Application.SlideShowBegin += Application_SlideShowBegin;
            Application.SlideShowEnd += Application_SlideShowEnd;
            Application.AfterPresentationOpen += Application_AfterPresentationOpen;
            Application.WindowActivate += Application_WindowActivate;
            Application.SlideSelectionChanged += Application_SlideSelectionChanged;
           
            Application.PresentationClose += Application_PresentationClose;
            Application.WindowSelectionChange += Application_WindowSelectionChange;
            Application.SlideShowNextSlide += Application_SlideShowNextSlide;
            
            paneModel_ = new KynapseePaneModel();
            paneModel_.PropertyChanged += paneModel__PropertyChanged;

            pane_ = Globals.Ribbons.KynapseePane;
            pane_.Bind(paneModel_);

            factory_ = Globals.Factory.GetRibbonFactory();


        }

        void transitionTaskPane__VisibleChanged(object sender, EventArgs e)
        {
            try
            {
                paneModel_.TransitionPaneVisible = ((CustomTaskPane)sender).Visible;
            }
            catch (COMException)
            {

            }
        }

        void paneModel__PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "TransitionPaneVisible":
                    if (paneModel_.Presentation == null)
                        break;
                    if (!transitionTaskPanes_.ContainsKey(paneModel_.Presentation))
                    {
                        var transitionPane = new TransitionPane(new TransitionsViewModel(paneModel_));
                        transitionTaskPanes_[paneModel_.Presentation] = CustomTaskPanes.Add(transitionPane, "Kinect Transitions");
                        transitionTaskPanes_[paneModel_.Presentation].VisibleChanged += transitionTaskPane__VisibleChanged;
                    }
                    if (transitionTaskPanes_[paneModel_.Presentation].Visible != paneModel_.TransitionPaneVisible)
                        transitionTaskPanes_[paneModel_.Presentation].Visible = paneModel_.TransitionPaneVisible;
                    break;
            }
        }

        void Application_SlideShowNextSlide(SlideShowWindow Wn)
        {
            /*
            if (kinect_ == null)
                return;

            if (trackedShapes_.Count > 0)
                kinect_.Nui.SkeletonFrameReady -= Nui_SkeletonFrameReady;
            bool hasTrackedShape = false;
            trackedShapes_.Clear();
            foreach (Shape shape in Wn.View.Slide.Shapes)
            {
                if (shape.Tags["KY_Joint"] != "")
                {
                    trackedShapes_[shape.Tags["KY_Joint"] == "LeftHand" ? JointID.HandLeft : JointID.HandRight] = shape;
                    hasTrackedShape = true;
                }
            }
            if (hasTrackedShape)
                kinect_.Nui.SkeletonFrameReady += Nui_SkeletonFrameReady;
            */
        }

        void Nui_SkeletonFrameReady(object sender, Microsoft.Research.Kinect.Nui.SkeletonFrameReadyEventArgs e)
        {
            /*
            if (ssw_ == null)
                return;

            SkeletonFrame skeletonFrame = e.SkeletonFrame;
            foreach (SkeletonData data in skeletonFrame.Skeletons)
            {
                if (data.TrackingState == SkeletonTrackingState.Tracked)
                {
                    foreach (var tr in trackedShapes_)
                    {
                        tr.Value.Left = data.Joints[tr.Key].Position.X/2f * ssw_.Width;
                        tr.Value.Top = data.Joints[tr.Key].Position.Y * ssw_.Height;
                    }
                    break;
                }
            }
             * */
        }

        void Application_WindowSelectionChange(Selection Sel)
        {
            //sel_ = Sel;
            //pane_.UpdateSelection();
            try
            {
                paneModel_.ShapeRange = Sel.ShapeRange;
            }
            catch (COMException)
            {
                paneModel_.ShapeRange = null;
            }
        }


        void Application_PresentationClose(Presentation Pres)
        {
            paneModel_.Presentation = null;
        }

        void Application_SlideSelectionChanged(SlideRange SldRange)
        {
            paneModel_.SlideRange = SldRange;
        }

        void Application_WindowActivate(Presentation Pres, DocumentWindow Wn)
        {
            paneModel_.Presentation = Pres;
        }

        void Application_AfterPresentationOpen(Presentation Pres)
        {
            paneModel_.Presentation = Pres;
        }

        void Application_SlideShowBegin(SlideShowWindow Wn)
        {
            if (!paneModel_.Presentation.GetKinectEnable())
                return;

            presenter_ = new Presenter(Application, Wn);


            /*
            if (!pres_.Tags.Get("KY_KinectEnabled", false))
                return;
            ssw_ = Wn;

            LoadKinect();

            if (kinect_ == null)
            {
                if (MessageBox.Show("Kinect is not detected. Check the connection and device.\nWould you like to disable Kinect for the presentation?",
                    "Kynapsee", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                {
                    pres_.Tags.Add("KY_KinectEnabled", false);
                }
                else
                {
                    Wn.View.Exit();
                }
            }
            else
            {
                
            }
            */
        }

        void Application_SlideShowEnd(Presentation Pres)
        {
            if (presenter_ != null)
                presenter_.Dispose();
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
