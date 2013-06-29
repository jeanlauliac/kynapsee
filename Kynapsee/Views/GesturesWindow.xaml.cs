using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Kynapsee.Nui.Model;
using Kynapsee.ViewModels;
using Microsoft.Research.Kinect.Nui;

namespace Kynapsee.Views
{
    /// <summary>
    /// Interaction logic for GesturesWindow.xaml
    /// </summary>
    public partial class GesturesWindow
    {
        private GesturesViewModel model_;
        
        /// <summary>
        /// Dictionary of all the joints Kinect SDK is capable of tracking. You might not want always to use them all but they are included here for thouroughness.
        /// </summary>
        private readonly Dictionary<JointID, Brush> jointColors_ = new Dictionary<JointID, Brush>
        { 
            {JointID.HipCenter, new SolidColorBrush(System.Windows.Media.Color.FromRgb(169, 176, 155))},
            {JointID.Spine, new SolidColorBrush(System.Windows.Media.Color.FromRgb(169, 176, 155))},
            {JointID.ShoulderCenter, new SolidColorBrush(System.Windows.Media.Color.FromRgb(168, 230, 29))},
            {JointID.Head, new SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 0, 0))},
            {JointID.ShoulderLeft, new SolidColorBrush(System.Windows.Media.Color.FromRgb(79, 84, 33))},
            {JointID.ElbowLeft, new SolidColorBrush(System.Windows.Media.Color.FromRgb(84, 33, 42))},
            {JointID.WristLeft, new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 126, 0))},
            {JointID.HandLeft, new SolidColorBrush(System.Windows.Media.Color.FromRgb(215, 86, 0))},
            {JointID.ShoulderRight, new SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 79,  84))},
            {JointID.ElbowRight, new SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 33, 84))},
            {JointID.WristRight, new SolidColorBrush(System.Windows.Media.Color.FromRgb(77, 109, 243))},
            {JointID.HandRight, new SolidColorBrush(System.Windows.Media.Color.FromRgb(37,  69, 243))},
            {JointID.HipLeft, new SolidColorBrush(System.Windows.Media.Color.FromRgb(77, 109, 243))},
            {JointID.KneeLeft, new SolidColorBrush(System.Windows.Media.Color.FromRgb(69, 33, 84))},
            {JointID.AnkleLeft, new SolidColorBrush(System.Windows.Media.Color.FromRgb(229, 170, 122))},
            {JointID.FootLeft, new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 126, 0))},
            {JointID.HipRight, new SolidColorBrush(System.Windows.Media.Color.FromRgb(181, 165, 213))},
            {JointID.KneeRight, new SolidColorBrush(System.Windows.Media.Color.FromRgb(71, 222, 76))},
            {JointID.AnkleRight, new SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 228, 156))},
            {JointID.FootRight, new SolidColorBrush(System.Windows.Media.Color.FromRgb(77, 109, 243))}
        };

        public GesturesWindow(GesturesViewModel model)
        {

            InitializeComponent();
            model_ = model;
            DataContext = model_;

            // Start skeleton drawing if available.
            if (model_.Provider != null)
            {
                
                model_.Provider.Runtime.SkeletonFrameReady += NuiSkeletonFrameReadyOnThread;
                
                
            }

        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            model_.Provider.Runtime.SkeletonFrameReady -= NuiSkeletonFrameReadyOnThread;
        }


        private void NuiSkeletonFrameReadyOnThread(object sender, SkeletonFrameReadyEventArgs e)
        {
            Dispatcher.Invoke(new EventHandler<SkeletonFrameReadyEventArgs>(NuiSkeletonFrameReady), sender, e);

        }

        private void listBox1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            listBox1.SelectedItem = null;
        }


        /// <summary>
        /// Gets the display position (i.e. where in the display image) of a Joint
        /// </summary>
        /// <param name="joint">Kinect NUI Joint</param>
        /// <returns>Point mapped location of sent joint</returns>
        private System.Windows.Point GetDisplayPosition(Joint joint)
        {
            var nui = model_.Provider.Runtime;

            //return new System.Windows.Point((int)(skeletonCanvas.ActualWidth / 2 + joint.Position.X * skeletonCanvas.ActualWidth / 2),
            //    (int)(skeletonCanvas.ActualHeight / 2 + joint.Position.Y * skeletonCanvas.ActualHeight / 2));

            float depthX, depthY;
            nui.SkeletonEngine.SkeletonToDepthImage(joint.Position, out depthX, out depthY);
            depthX = Math.Max(0, Math.Min(depthX * 320, 320)); // convert to 320, 240 space
            depthY = Math.Max(0, Math.Min(depthY * 240, 240)); // convert to 320, 240 space
            int colorX, colorY;
            var iv = new ImageViewArea();

            // Only ImageResolution.Resolution640x480 is supported at this point
            nui.NuiCamera.GetColorPixelCoordinatesFromDepthPixel(ImageResolution.Resolution640x480, iv, (int)depthX, (int)depthY, 0, out colorX, out colorY);

            // Map back to skeleton.Width & skeleton.Height
            return new System.Windows.Point((int)(skeletonCanvas.ActualWidth * colorX / 640.0), (int)(skeletonCanvas.ActualHeight * colorY / 480));
        }

        /// <summary>
        /// Works out how to draw a line ('bone') for sent Joints
        /// </summary>
        /// <param name="joints">Kinect NUI Joints</param>`
        /// <param name="brush">The brush we'll use to colour the joints</param>
        /// <param name="ids">The JointsIDs we're interested in</param>
        /// <returns>A line or lines</returns>
        private Polyline GetBodySegment(JointsCollection joints, Brush brush, params JointID[] ids)
        {
            var points = new PointCollection(ids.Length);
            foreach (JointID t in ids)
            {
                points.Add(GetDisplayPosition(joints[t]));
            }

            var polyline = new Polyline();
            polyline.Points = points;
            polyline.Stroke = brush;
            polyline.StrokeThickness = 5;
            return polyline;
        }

        /// <summary>
        /// Runds every time a skeleton frame is ready. Updates the skeleton canvas with new joint and polyline locations.
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Skeleton Frame Event Args</param>
        private void NuiSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            if (model_.Provider.Runtime == null)
                return;

            SkeletonFrame skeletonFrame = e.SkeletonFrame;
            int iSkeleton = 0;
            var brushes = new Brush[6];
            brushes[0] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0));
            brushes[1] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 0));
            brushes[2] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(64, 255, 255));
            brushes[3] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 64));
            brushes[4] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 64, 255));
            brushes[5] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(128, 128, 255));

            skeletonCanvas.Children.Clear();
            skeletonCanvas.Children.Add(statusText);

            foreach (SkeletonData data in skeletonFrame.Skeletons)
            {
                if (SkeletonTrackingState.Tracked == data.TrackingState)
                {
                    //Set up targetting reticules
                    Ellipse leftReticle = new Ellipse();
                    leftReticle.Stroke = System.Windows.Media.Brushes.Aquamarine;
                    leftReticle.Width = 25;
                    leftReticle.Height = 25;

                    Ellipse rightReticle = new Ellipse();
                    rightReticle.Stroke = System.Windows.Media.Brushes.Aquamarine;
                    rightReticle.Width = 25;
                    rightReticle.Height = 25;

                    skeletonCanvas.Children.Add(rightReticle);
                    skeletonCanvas.Children.Add(leftReticle);


                    // Draw bones
                    Brush brush = brushes[iSkeleton % brushes.Length];
                    skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointID.HipCenter, JointID.Spine, JointID.ShoulderCenter, JointID.Head));
                    skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointID.ShoulderCenter, JointID.ShoulderLeft, JointID.ElbowLeft, JointID.WristLeft, JointID.HandLeft));
                    skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointID.ShoulderCenter, JointID.ShoulderRight, JointID.ElbowRight, JointID.WristRight, JointID.HandRight));
                    skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointID.HipCenter, JointID.HipLeft, JointID.KneeLeft, JointID.AnkleLeft, JointID.FootLeft));
                    skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointID.HipCenter, JointID.HipRight, JointID.KneeRight, JointID.AnkleRight, JointID.FootRight));

                    // Draw joints
                    foreach (Joint joint in data.Joints)
                    {
                        System.Windows.Point jointPos = GetDisplayPosition(joint);
                        var jointLine = new Line();
                        jointLine.X1 = jointPos.X - 3;
                        jointLine.X2 = jointLine.X1 + 6;
                        jointLine.Y1 = jointLine.Y2 = jointPos.Y;
                        jointLine.Stroke = jointColors_[joint.ID];
                        jointLine.StrokeThickness = 6;
                        skeletonCanvas.Children.Add(jointLine);

                        //Place targetting reticules over left and right hand
                        if (joint.ID == JointID.HandLeft)
                        {
                            System.Windows.Controls.Canvas.SetLeft(leftReticle, jointPos.X - 12.5);
                            System.Windows.Controls.Canvas.SetTop(leftReticle, jointPos.Y - 12.5);
                        }

                        if (joint.ID == JointID.HandRight)
                        {
                            System.Windows.Controls.Canvas.SetLeft(rightReticle, jointPos.X - 12.5);
                            System.Windows.Controls.Canvas.SetTop(rightReticle, jointPos.Y - 12.5);
                        }
                    }
                }

                iSkeleton++;
            } // for each skeleton
        }



    }
}
