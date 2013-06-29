using GalaSoft.MvvmLight;
using Microsoft.Office.Interop.PowerPoint;
using System.Collections.Generic;

namespace Kynapsee.ViewModels
{
    /// <summary>
    /// Contains view model of the Kynapsee pane.
    /// </summary>
    public class KynapseePaneModel : ViewModelBase
    {
        private Dictionary<Presentation, bool> showPanels_ = new Dictionary<Presentation,bool>();

        public void UpdatePresentation()
        {
            RaisePropertyChanged("Presentation");
            UpdateSlideRange();
        }

        public void UpdateSlideRange()
        {
            RaisePropertyChanged("SlideRange");
        }

        private Presentation presentation_;

        /// <summary>
        /// Gets the presentation currently edited by the user.
        /// </summary>
        public Presentation Presentation
        {
            get { return presentation_; }
            set
            {
                if (presentation_ == value) return;
                if (value == null)
                    TransitionPaneVisible = false;
                SlideRange = null;
                presentation_ = value;
                RaisePropertyChanged("Presentation");
                if (presentation_ != null)
                    TransitionPaneVisible = showPanels_.ContainsKey(presentation_) ? showPanels_[presentation_] : false;
            }
        }

        private SlideRange slideRange_;

        /// <summary>
        /// Gets the slide range currently edited and/or selected by the user.
        /// </summary>
        public SlideRange SlideRange
        {
            get { return slideRange_; }
            set
            {
                if (slideRange_ == value) return;
                slideRange_ = value;
                RaisePropertyChanged("SlideRange");
            }
        }

        private ShapeRange shapeRange_;

        /// <summary>
        /// Gets the shape range currently selected 
        /// </summary>
        public ShapeRange ShapeRange
        {
            get { return shapeRange_; }
            set
            {
                if (shapeRange_ == value) return;
                shapeRange_ = value;
                RaisePropertyChanged("ShapeRange");
            }
        }

        private bool transitionPaneVisible_;

        /// <summary>
        /// Gets visibility of the transition pane.
        /// </summary>
        public bool TransitionPaneVisible
        {
            get { return transitionPaneVisible_; }
            set
            {
                if (transitionPaneVisible_ == value) return;
                transitionPaneVisible_ = value;
                RaisePropertyChanged("TransitionPaneVisible");
                if (presentation_ != null)
                    showPanels_[presentation_] = transitionPaneVisible_;
            }
        }

        
    }

}
