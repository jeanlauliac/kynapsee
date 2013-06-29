namespace Kynapsee
{
    partial class KynapseePane : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public KynapseePane()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.kynapseeTab = this.Factory.CreateRibbonTab();
            this.presentationGroup = this.Factory.CreateRibbonGroup();
            this.slideGroup = this.Factory.CreateRibbonGroup();
            this.dropSlideTransition = this.Factory.CreateRibbonDropDown();
            this.objectShape = this.Factory.CreateRibbonGroup();
            this.dropShapeJoint = this.Factory.CreateRibbonDropDown();
            this.separator1 = this.Factory.CreateRibbonSeparator();
            this.checkJointX = this.Factory.CreateRibbonCheckBox();
            this.checkJointY = this.Factory.CreateRibbonCheckBox();
            this.separator2 = this.Factory.CreateRibbonSeparator();
            this.editBox1 = this.Factory.CreateRibbonEditBox();
            this.editBox2 = this.Factory.CreateRibbonEditBox();
            this.gesturesGroup = this.Factory.CreateRibbonGroup();
            this.toggleKinect = this.Factory.CreateRibbonToggleButton();
            this.buttonRun = this.Factory.CreateRibbonButton();
            this.buttonSlideApply = this.Factory.CreateRibbonButton();
            this.toggleSlideTransitions = this.Factory.CreateRibbonToggleButton();
            this.buttonGestures = this.Factory.CreateRibbonButton();
            this.buttonGesturesImport = this.Factory.CreateRibbonButton();
            this.buttonGesturesExport = this.Factory.CreateRibbonButton();
            this.kynapseeTab.SuspendLayout();
            this.presentationGroup.SuspendLayout();
            this.slideGroup.SuspendLayout();
            this.objectShape.SuspendLayout();
            this.gesturesGroup.SuspendLayout();
            // 
            // kynapseeTab
            // 
            this.kynapseeTab.Groups.Add(this.presentationGroup);
            this.kynapseeTab.Groups.Add(this.slideGroup);
            this.kynapseeTab.Groups.Add(this.objectShape);
            this.kynapseeTab.Groups.Add(this.gesturesGroup);
            this.kynapseeTab.Label = "Kinect";
            this.kynapseeTab.Name = "kynapseeTab";
            // 
            // presentationGroup
            // 
            this.presentationGroup.Items.Add(this.toggleKinect);
            this.presentationGroup.Items.Add(this.buttonRun);
            this.presentationGroup.Label = "Presentation";
            this.presentationGroup.Name = "presentationGroup";
            // 
            // slideGroup
            // 
            this.slideGroup.Items.Add(this.dropSlideTransition);
            this.slideGroup.Items.Add(this.buttonSlideApply);
            this.slideGroup.Items.Add(this.toggleSlideTransitions);
            this.slideGroup.Label = "Slide";
            this.slideGroup.Name = "slideGroup";
            // 
            // dropSlideTransition
            // 
            this.dropSlideTransition.Enabled = false;
            this.dropSlideTransition.Label = "Transition:";
            this.dropSlideTransition.Name = "dropSlideTransition";
            this.dropSlideTransition.OfficeImageId = "XmlTransformation";
            this.dropSlideTransition.ShowImage = true;
            this.dropSlideTransition.SelectionChanged += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.dropSlideTransition_SelectionChanged);
            // 
            // objectShape
            // 
            this.objectShape.Items.Add(this.dropShapeJoint);
            this.objectShape.Items.Add(this.separator1);
            this.objectShape.Items.Add(this.checkJointX);
            this.objectShape.Items.Add(this.checkJointY);
            this.objectShape.Items.Add(this.separator2);
            this.objectShape.Items.Add(this.editBox1);
            this.objectShape.Items.Add(this.editBox2);
            this.objectShape.Label = "Shape";
            this.objectShape.Name = "objectShape";
            // 
            // dropShapeJoint
            // 
            this.dropShapeJoint.Enabled = false;
            this.dropShapeJoint.Label = "Joint:";
            this.dropShapeJoint.Name = "dropShapeJoint";
            this.dropShapeJoint.OfficeImageId = "DiagramChangeToRadialClassic";
            this.dropShapeJoint.ShowImage = true;
            this.dropShapeJoint.SelectionChanged += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.dropShapeJoint_SelectionChanged);
            // 
            // separator1
            // 
            this.separator1.Name = "separator1";
            // 
            // checkJointX
            // 
            this.checkJointX.Checked = true;
            this.checkJointX.Label = "Bind abscisse";
            this.checkJointX.Name = "checkJointX";
            // 
            // checkJointY
            // 
            this.checkJointY.Checked = true;
            this.checkJointY.Label = "Bind ordinate";
            this.checkJointY.Name = "checkJointY";
            // 
            // separator2
            // 
            this.separator2.Name = "separator2";
            // 
            // editBox1
            // 
            this.editBox1.Label = "Horizontal diff";
            this.editBox1.Name = "editBox1";
            this.editBox1.Text = null;
            // 
            // editBox2
            // 
            this.editBox2.Label = "Vertical diff";
            this.editBox2.Name = "editBox2";
            this.editBox2.Text = null;
            // 
            // gesturesGroup
            // 
            this.gesturesGroup.Items.Add(this.buttonGestures);
            this.gesturesGroup.Items.Add(this.buttonGesturesImport);
            this.gesturesGroup.Items.Add(this.buttonGesturesExport);
            this.gesturesGroup.Label = "Gestures";
            this.gesturesGroup.Name = "gesturesGroup";
            // 
            // toggleKinect
            // 
            this.toggleKinect.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.toggleKinect.Enabled = false;
            this.toggleKinect.Image = global::Kynapsee.Properties.Resources.Kinect;
            this.toggleKinect.Label = "Activate Kinect";
            this.toggleKinect.Name = "toggleKinect";
            this.toggleKinect.ShowImage = true;
            this.toggleKinect.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.toggleKinect_Click);
            // 
            // buttonRun
            // 
            this.buttonRun.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.buttonRun.Label = "Slideshow with Kinect";
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.OfficeImageId = "SlideShowFromBeginning";
            this.buttonRun.ShowImage = true;
            this.buttonRun.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonRun_Click);
            // 
            // buttonSlideApply
            // 
            this.buttonSlideApply.Enabled = false;
            this.buttonSlideApply.Label = "Apply to all slides";
            this.buttonSlideApply.Name = "buttonSlideApply";
            this.buttonSlideApply.OfficeImageId = "SlideTransitionApplyToAll";
            this.buttonSlideApply.ShowImage = true;
            this.buttonSlideApply.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonSlideApply_Click);
            // 
            // toggleSlideTransitions
            // 
            this.toggleSlideTransitions.Label = "Complex transitions";
            this.toggleSlideTransitions.Name = "toggleSlideTransitions";
            this.toggleSlideTransitions.OfficeImageId = "ConnectShapes";
            this.toggleSlideTransitions.ShowImage = true;
            this.toggleSlideTransitions.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.toggleSlideTransitions_Click);
            // 
            // buttonGestures
            // 
            this.buttonGestures.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.buttonGestures.Image = global::Kynapsee.Properties.Resources.Gesture;
            this.buttonGestures.Label = "Manage gestures";
            this.buttonGestures.Name = "buttonGestures";
            this.buttonGestures.ShowImage = true;
            this.buttonGestures.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonGestures_Click);
            // 
            // buttonGesturesImport
            // 
            this.buttonGesturesImport.Label = "Import gestures";
            this.buttonGesturesImport.Name = "buttonGesturesImport";
            this.buttonGesturesImport.OfficeImageId = "ImportTemplate";
            this.buttonGesturesImport.ShowImage = true;
            // 
            // buttonGesturesExport
            // 
            this.buttonGesturesExport.Label = "Export gestures";
            this.buttonGesturesExport.Name = "buttonGesturesExport";
            this.buttonGesturesExport.OfficeImageId = "ExportTextFile";
            this.buttonGesturesExport.ShowImage = true;
            // 
            // KynapseePane
            // 
            this.Name = "KynapseePane";
            this.RibbonType = "Microsoft.PowerPoint.Presentation";
            this.Tabs.Add(this.kynapseeTab);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.KynapseePane_Load);
            this.kynapseeTab.ResumeLayout(false);
            this.kynapseeTab.PerformLayout();
            this.presentationGroup.ResumeLayout(false);
            this.presentationGroup.PerformLayout();
            this.slideGroup.ResumeLayout(false);
            this.slideGroup.PerformLayout();
            this.objectShape.ResumeLayout(false);
            this.objectShape.PerformLayout();
            this.gesturesGroup.ResumeLayout(false);
            this.gesturesGroup.PerformLayout();

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab kynapseeTab;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup presentationGroup;
        internal Microsoft.Office.Tools.Ribbon.RibbonToggleButton toggleKinect;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup slideGroup;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup objectShape;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup gesturesGroup;
        internal Microsoft.Office.Tools.Ribbon.RibbonDropDown dropSlideTransition;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonSlideApply;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonRun;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonGesturesImport;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonGesturesExport;
        internal Microsoft.Office.Tools.Ribbon.RibbonDropDown dropShapeJoint;
        internal Microsoft.Office.Tools.Ribbon.RibbonToggleButton toggleSlideTransitions;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonGestures;
        internal Microsoft.Office.Tools.Ribbon.RibbonCheckBox checkJointX;
        internal Microsoft.Office.Tools.Ribbon.RibbonCheckBox checkJointY;
        internal Microsoft.Office.Tools.Ribbon.RibbonSeparator separator1;
        internal Microsoft.Office.Tools.Ribbon.RibbonSeparator separator2;
        internal Microsoft.Office.Tools.Ribbon.RibbonEditBox editBox1;
        internal Microsoft.Office.Tools.Ribbon.RibbonEditBox editBox2;
    }

    partial class ThisRibbonCollection
    {
        internal KynapseePane KynapseePane
        {
            get { return this.GetRibbon<KynapseePane>(); }
        }
    }
}
