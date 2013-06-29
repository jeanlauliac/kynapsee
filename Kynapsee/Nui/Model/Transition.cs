using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Office.Interop.PowerPoint;

namespace Kynapsee.Nui.Model
{
    public enum TransitionMethod
    {
        NextSlide,
        PreviousSlide,
        TargetSlide
    }

    /// <summary>
    /// Represents a transition to another slide.
    /// </summary>
    public class Transition
    {
        private Guid guid_;
        private int slideId_;

        public void Bind(GestureSet set, Presentation pres)
        {
            Gesture = set.Gestures.Where((x) => x.Guid == guid_).FirstOrDefault();
            try
            {
                TargetSlide = pres.Slides.FindBySlideID(slideId_);
            }
            catch (COMException)
            {
                TargetSlide = null;
            }
        }

        [XmlIgnore]
        public Gesture Gesture { get; set; }

        /// <summary>
        /// Represents the source gesture
        /// </summary>
        public Guid GestureGuid
        {
            get { return Gesture != null ? Gesture.Guid : new Guid(); }
            set { guid_ = value;  }
        }

        [XmlIgnore]
        public Slide TargetSlide { get; set; }

        /// <summary>
        /// Represents the target slide id.
        /// </summary>
        public int TargetSlideId
        {
            get { return TargetSlide != null ? TargetSlide.SlideID : 0; }
            set { slideId_ = value; }
        }

        public TransitionMethod Method { get; set; }
    }
}
