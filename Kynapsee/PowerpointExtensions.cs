using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kynapsee.Nui;
using Kynapsee.Nui.Model;
using Microsoft.Office.Interop.PowerPoint;

namespace Kynapsee
{
    /// <summary>
    /// Adds getters and setters for Kynapsee data on powerpoint COM objects.
    /// </summary>
    static class PowerpointExtensions
    {
        private const string TagKinectEnabled = "KY_KinectEnabled";
        private const string TagNuiModel = "KY_NuiModel";
        private const string TagTransitionPaneVisible = "KY_TransitionPaneVisible";

        private const string TagTransitions = "KY_Transitions";

        public static bool GetKinectEnable(this Presentation pres)
        {
            return pres.Tags.Get(TagKinectEnabled, false);
        }

        public static void SetKinectEnable(this Presentation pres, bool value)
        {
            pres.Tags.Add(TagKinectEnabled, value);
        }

        /*
        public static bool GetTransitionPaneVisible(this Presentation pres)
        {
            return pres.Tags.Get(TagTransitionPaneVisible, false);
        }

        public static void SetTransitionPaneVisible(this Presentation pres, bool value)
        {
            pres.Tags.Add(TagTransitionPaneVisible, value);
        }
        */
        public static GestureSet GetNuiModel(this Presentation pres)
        {
            if (pres.Tags[TagNuiModel] == "")
                return new GestureSet() {Gestures = new List<Gesture>()};
            return GestureSet.FromString(pres.Tags[TagNuiModel]);
        }

        public static void SetNuiModel(this Presentation pres, GestureSet model)
        {
            pres.Tags.Add(TagNuiModel, model.ToString());
        }

        public static TransitionSet GetTransitions(this SlideRange sr, Presentation pr, GestureSet set)
        {
            if (sr == null || sr.Count == 0 || sr[1].Tags[TagTransitions] == "")
                return new TransitionSet() { Transitions = new List<Transition>() };
            return TransitionSet.FromString(sr[1].Tags[TagTransitions], set, pr);
        }

        public static TransitionSet GetTransitions(this Slide sr, Presentation pr, GestureSet set)
        {
            if (sr.Tags[TagTransitions] == "")
                return new TransitionSet() { Transitions = new List<Transition>() };
            return TransitionSet.FromString(sr.Tags[TagTransitions], set, pr);
        }

        public static void SetTransitions(this SlideRange sr, TransitionSet model)
        {
            for (int i = 1; i <= sr.Count; i++)
                sr[i].Tags.Add(TagTransitions, model.ToString());
        }

        public static void SetTransitions(this Slide sr, TransitionSet model)
        {
            sr.Tags.Add(TagTransitions, model.ToString());
        }

    }
}
