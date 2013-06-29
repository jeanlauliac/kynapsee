using System.Windows.Forms;
using Kynapsee.Nui.Model;
using Kynapsee.ViewModels;
using Kynapsee.Views;
using Microsoft.Office.Interop.PowerPoint;
using Microsoft.Office.Tools.Ribbon;
using System.Linq;

namespace Kynapsee
{
    public partial class KynapseePane
    {
        private KynapseePaneModel model_;
        private GestureSet gestures_;

        public void Bind(KynapseePaneModel model)
        {
            model_ = model;
            model_.PropertyChanged += PropertyChangedInModel;
        }

        void PropertyChangedInModel(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Presentation":
                    UpdatePresentation();
                    break;
                case "SlideRange":
                    UpdateSlideRange();
                    break;
                case "ShapeRange":
                    UpdateShapeRange();
                    break;
                case "TransitionPaneVisible":
                    toggleSlideTransitions.Checked = model_.TransitionPaneVisible;
                    break;
            }
        }

        void UpdatePresentation()
        {
            // Enable states.
            var enabled = model_.Presentation != null;
            if (toggleKinect.Enabled != enabled)
            {
                toggleKinect.Enabled = enabled;
                buttonRun.Enabled = enabled;
                buttonGestures.Enabled = enabled;
                buttonGesturesImport.Enabled = enabled;
                buttonGesturesExport.Enabled = enabled;
            }
            // Update values.
            if (enabled)
            {
                toggleKinect.Checked = model_.Presentation.GetKinectEnable();

                dropSlideTransition.Items.Clear();
                AddDropDownItem(dropSlideTransition, "[none]");
                gestures_ = model_.Presentation.GetNuiModel();
                foreach (var gesture in gestures_.Gestures)
                    AddDropDownItem(dropSlideTransition, gesture.Name);
            }
        }

        private void toggleKinect_Click(object sender, RibbonControlEventArgs e)
        {
            model_.Presentation.SetKinectEnable(toggleKinect.Checked);
        }

        
        void UpdateSlideRange()
        {
            var enabled = model_.SlideRange != null;
            if (dropSlideTransition.Enabled != enabled)
            {
                dropSlideTransition.Enabled = enabled;
                buttonSlideApply.Enabled = enabled;
                toggleSlideTransitions.Enabled = enabled;
            }
            if (enabled)
            {
                var transitions = model_.SlideRange.GetTransitions(model_.Presentation, gestures_);
                var tr = transitions.Transitions.Where((x) => x.Method == Nui.Model.TransitionMethod.NextSlide).FirstOrDefault();
                dropSlideTransition.SelectedItemIndex = 0;
                if (tr != null)
                {
                    dropSlideTransition.SelectedItemIndex = gestures_.Gestures.IndexOf(tr.Gesture) + 1;
                }
            }
        }

        void UpdateShapeRange()
        {
            var enabled = model_.ShapeRange != null;

        }

        private void KynapseePane_Load(object sender, RibbonUIEventArgs e)
        {
            AddDropDownItem(dropShapeJoint, "[none]");
            AddDropDownItem(dropShapeJoint, "LeftHand");
            AddDropDownItem(dropShapeJoint, "RightHand");
        }

        void AddDropDownItem(RibbonDropDown dd, string label)
        {
            var item = Factory.CreateRibbonDropDownItem();
            item.Label = label;
            dd.Items.Add(item);
        }

        /*
        public void UpdateAll(bool enabled = true)
        {
            if (toggleKinect.Enabled != enabled)
            {
                toggleKinect.Enabled = enabled;
                if (!enabled)
                    pres_ = null;
            }
            if (enabled)
            {

                pres_ = addId_.Application.ActivePresentation;
                toggleKinect.Checked = pres_.Tags.Get("KY_KinectEnabled", false);

                dropSlideTransition.Items.Clear();

                var item = Factory.CreateRibbonDropDownItem();
                item.Label = "[none]";
                dropSlideTransition.Items.Add(item);
                
                if (!string.IsNullOrEmpty(pres_.Tags["KY_GestureNames"]))
                {
                    string[] gnames = pres_.Tags["KY_GestureNames"].Split(';');


                    foreach (var gesture in gnames)
                    {
                        item = Factory.CreateRibbonDropDownItem();
                        item.Label = gesture;
                        dropSlideTransition.Items.Add(item);
                    }
                }
            }

            UpdateSlide(enabled);
        }
        */

        /*
        public void UpdateSlide(bool enabled = true)
        {
            try
            {
                slide_ = addId_.Application.ActiveWindow.View.Slide as Slide;
                var transName = slide_.Tags.Get("KY_Transition", "NextSlide");
                dropSlideTransition.SelectedItemIndex = 0;
                foreach (var item in dropSlideTransition.Items)
                {
                    if (item.Label == transName)
                    {
                        dropSlideTransition.SelectedItem = item;
                        break;
                    }
                }
            }
            catch (COMException)
            {
                enabled = false;
            }
            if (dropSlideTransition.Enabled != enabled)
            {
                dropSlideTransition.Enabled = enabled;
                buttonSlideApply.Enabled = enabled;
                if (!enabled)
                    slide_ = null;
            }
            UpdateSelection();
        }
        */

        /*
        public void UpdateSelection()
        {
            bool enabled = addId_.sel_ != null;
            if (enabled)
            {
                try
                {
                    var shape = addId_.sel_.ShapeRange;

                    var jointName = shape.Tags.Get("KY_Joint", "");
                    dropShapeJoint.SelectedItemIndex = 0;
                    foreach (var item in dropShapeJoint.Items)
                    {
                        if (item.Label == jointName)
                        {
                            dropShapeJoint.SelectedItem = item;
                            break;
                        }
                    }

                }
                catch (COMException)
                {
                    enabled = false;
                }
            }
            if (enabled != dropShapeJoint.Enabled)
            {
                dropShapeJoint.Enabled = enabled;
            }
        }
        */

        private void dropSlideTransition_SelectionChanged(object sender, RibbonControlEventArgs e)
        {
            var transitions = model_.SlideRange.GetTransitions(model_.Presentation, gestures_);
            var tr = transitions.Transitions.Where((x) => x.Method == TransitionMethod.NextSlide).FirstOrDefault();
            if (dropSlideTransition.SelectedItemIndex == 0)
            {
                if (tr != null)
                    transitions.Transitions.Remove(tr);
            }
            else
            {
                var gesture = gestures_.Gestures[dropSlideTransition.SelectedItemIndex - 1];
                if (tr == null)
                {
                    tr = new Transition() {Gesture = gesture, Method = TransitionMethod.NextSlide};
                }
                else
                    tr.Gesture = gesture;
                
            }
            model_.SlideRange.SetTransitions(transitions);
            //var slide = addId_.Application.ActiveWindow.View.Slide;
            //slide.Tags.Add("KY_Transition", dropSlideTransition.SelectedItem.Label);

        }

        private void buttonSlideApply_Click(object sender, RibbonControlEventArgs e)
        {
            var tr = model_.SlideRange.GetTransitions(model_.Presentation, gestures_);
            foreach (Slide slide in model_.Presentation.Slides)
            {
                slide.SetTransitions(tr);
            }
        }

        private void buttonRun_Click(object sender, RibbonControlEventArgs e)
        {
            //toggleKinect.Checked = true;
            //toggleKinect_Click(this, null);
            //addId_.Application.ActivePresentation.SlideShowSettings.Run();
        }

        private void buttonGestures_Click(object sender, RibbonControlEventArgs e)
        {
            //var form = new GesturesForm();
            //form.ShowDialog();
            Cursor.Current = Cursors.WaitCursor;
            using (var viewModel = new GesturesViewModel(model_.Presentation.GetNuiModel()))
            {
                var gesturesWindow = new GesturesWindow(viewModel);
                gesturesWindow.ShowDialog();
                
                model_.Presentation.SetNuiModel(viewModel.GestureSet);
                
            }
            Cursor.Current = Cursors.Default;
            model_.UpdatePresentation();

        }

        private void dropShapeJoint_SelectionChanged(object sender, RibbonControlEventArgs e)
        {
            //addId_.sel_.ShapeRange.Tags.Add("KY_Joint", dropShapeJoint.SelectedItem.Label);
        }

        private void toggleSlideTransitions_Click(object sender, RibbonControlEventArgs e)
        {
            model_.TransitionPaneVisible = toggleSlideTransitions.Checked;
        }
        

    }
}
