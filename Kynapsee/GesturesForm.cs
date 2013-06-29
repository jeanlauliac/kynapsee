using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Research.Kinect.Nui;
using Microsoft.Xna.Framework;

namespace Kynapsee
{
    public partial class GesturesForm : Form
    {
        private ThisAddIn addId_;
        private Kinect kinect_;

        private SkeletonFrame skeletonFrame_;

        private Dictionary<JointID, List<Vector4>> tracks_ = null;

        /// <summary>
        /// The minumum number of frames in the _video buffer before we attempt to start matching gestures
        /// </summary>
        private const int CaptureCountdownSeconds = 3;

        /// <summary>
        /// ArrayList of coordinates which are recorded in sequence to define one gesture
        /// </summary>
        private DateTime _captureCountdown = DateTime.Now;

        /// <summary>
        /// ArrayList of coordinates which are recorded in sequence to define one gesture
        /// </summary>
        private Timer _captureCountdownTimer;

        /// <summary>
        /// The red index
        /// </summary>
        private const int RedIdx = 2;

        /// <summary>
        /// The green index
        /// </summary>
        private const int GreenIdx = 1;

        /// <summary>
        /// The blue index
        /// </summary>
        private const int BlueIdx = 0;

        /// <summary>
        /// The depth frame byte array. Only supports 320 * 240 at this time
        /// </summary>
        private readonly byte[] _depthFrame32 = new byte[320 * 240 * 4];


        private readonly Dictionary<JointID, Brush> _jointColors = new Dictionary<JointID, Brush>
        { 
            {JointID.HipCenter, new SolidBrush(Color.FromArgb(169, 176, 155))},
            {JointID.Spine, new SolidBrush(Color.FromArgb(169, 176, 155))},
            {JointID.ShoulderCenter, new SolidBrush(Color.FromArgb(168, 230, 29))},
            {JointID.Head, new SolidBrush(Color.FromArgb(200, 0, 0))},
            {JointID.ShoulderLeft, new SolidBrush(Color.FromArgb(79, 84, 33))},
            {JointID.ElbowLeft, new SolidBrush(Color.FromArgb(84, 33, 42))},
            {JointID.WristLeft, new SolidBrush(Color.FromArgb(255, 126, 0))},
            {JointID.HandLeft, new SolidBrush(Color.FromArgb(215, 86, 0))},
            {JointID.ShoulderRight, new SolidBrush(Color.FromArgb(33, 79,  84))},
            {JointID.ElbowRight, new SolidBrush(Color.FromArgb(33, 33, 84))},
            {JointID.WristRight, new SolidBrush(Color.FromArgb(77, 109, 243))},
            {JointID.HandRight, new SolidBrush(Color.FromArgb(37,  69, 243))},
            {JointID.HipLeft, new SolidBrush(Color.FromArgb(77, 109, 243))},
            {JointID.KneeLeft, new SolidBrush(Color.FromArgb(69, 33, 84))},
            {JointID.AnkleLeft, new SolidBrush(Color.FromArgb(229, 170, 122))},
            {JointID.FootLeft, new SolidBrush(Color.FromArgb(255, 126, 0))},
            {JointID.HipRight, new SolidBrush(Color.FromArgb(181, 165, 213))},
            {JointID.KneeRight, new SolidBrush(Color.FromArgb(71, 222, 76))},
            {JointID.AnkleRight, new SolidBrush(Color.FromArgb(245, 228, 156))},
            {JointID.FootRight, new SolidBrush(Color.FromArgb(77, 109, 243))}
        };

        public GesturesForm()
        {
            addId_ = Globals.ThisAddIn;
            InitializeComponent();
            /*
            System.Reflection.PropertyInfo aProp =
                typeof (System.Windows.Forms.Control).GetProperty(
                    "DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

            aProp.SetValue(pictureSkeleton, true, null);
             * */
        }

        private void listGestures_SelectedIndexChanged(object sender, EventArgs e)
        {
            textGesture.Text = listGestures.SelectedItem as string;
            //tracks_ = kinect_._dtw._recordedGestures[textGesture.Text];
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            
            this.Close();
        }

        private void GesturesForm_Shown(object sender, EventArgs e)
        {
            /*
            kinect_ = addId_.Kinect;
            if (kinect_ != null)
            {
                kinect_.Nui.SkeletonFrameReady += new EventHandler<Microsoft.Research.Kinect.Nui.SkeletonFrameReadyEventArgs>(Nui_SkeletonFrameReady);
                //kinect_.Nui.DepthFrameReady += new EventHandler<Microsoft.Research.Kinect.Nui.ImageFrameReadyEventArgs>(Nui_DepthFrameReady);
                //kinect_.Nui.VideoFrameReady += new EventHandler<Microsoft.Research.Kinect.Nui.ImageFrameReadyEventArgs>(Nui_VideoFrameReady);
                kinect_.GestureDone += new EventHandler<GestureEventArgs>(kinect__GestureDone);
                kinect_.CaptureEnd += new EventHandler(kinect__CaptureEnd);
                kinect_.NoGestureDone += new EventHandler(kinect__NoGestureDone);
            }
            else
            {
                buttonCapture.Enabled = false;
            }

            string[] gnames = addId_.pres_.Tags["KY_GestureNames"].Split(';');
            foreach (var gesture in gnames)
                listGestures.Items.Add(gesture);
 
            */
        }

        void kinect__NoGestureDone(object sender, EventArgs e)
        {
            labelLastGesture.Text = "Reading gestures";
        }

        void kinect__GestureDone(object sender, GestureEventArgs e)
        {
            labelLastGesture.Text = e.Name;
        }

        void Nui_VideoFrameReady(object sender, Microsoft.Research.Kinect.Nui.ImageFrameReadyEventArgs e)
        {
            
        }


        /// <summary>
        /// Converts a 16-bit grayscale depth frame which includes player indexes into a 32-bit frame that displays different players in different colors
        /// </summary>
        /// <param name="depthFrame16">The depth frame byte array</param>
        /// <returns>A depth frame byte array containing a player image</returns>
        private byte[] ConvertDepthFrame(byte[] depthFrame16)
        {
            for (int i16 = 0, i32 = 0; i16 < depthFrame16.Length && i32 < _depthFrame32.Length; i16 += 2, i32 += 4)
            {
                int player = depthFrame16[i16] & 0x07;
                int realDepth = (depthFrame16[i16 + 1] << 5) | (depthFrame16[i16] >> 3);

                // transform 13-bit depth information into an 8-bit intensity appropriate
                // for display (we disregard information in most significant bit)
                var intensity = (byte)(255 - (255 * realDepth / 0x0fff));

                _depthFrame32[i32 + RedIdx] = 0;
                _depthFrame32[i32 + GreenIdx] = 0;
                _depthFrame32[i32 + BlueIdx] = 0;

                // choose different display colors based on player
                switch (player)
                {
                    case 0:
                        _depthFrame32[i32 + RedIdx] = (byte)(intensity / 2);
                        _depthFrame32[i32 + GreenIdx] = (byte)(intensity / 2);
                        _depthFrame32[i32 + BlueIdx] = (byte)(intensity / 2);
                        break;
                    case 1:
                        _depthFrame32[i32 + RedIdx] = intensity;
                        break;
                    case 2:
                        _depthFrame32[i32 + GreenIdx] = intensity;
                        break;
                    case 3:
                        _depthFrame32[i32 + RedIdx] = (byte)(intensity / 4);
                        _depthFrame32[i32 + GreenIdx] = intensity;
                        _depthFrame32[i32 + BlueIdx] = intensity;
                        break;
                    case 4:
                        _depthFrame32[i32 + RedIdx] = intensity;
                        _depthFrame32[i32 + GreenIdx] = intensity;
                        _depthFrame32[i32 + BlueIdx] = (byte)(intensity / 4);
                        break;
                    case 5:
                        _depthFrame32[i32 + RedIdx] = intensity;
                        _depthFrame32[i32 + GreenIdx] = (byte)(intensity / 4);
                        _depthFrame32[i32 + BlueIdx] = intensity;
                        break;
                    case 6:
                        _depthFrame32[i32 + RedIdx] = (byte)(intensity / 2);
                        _depthFrame32[i32 + GreenIdx] = (byte)(intensity / 2);
                        _depthFrame32[i32 + BlueIdx] = intensity;
                        break;
                    case 7:
                        _depthFrame32[i32 + RedIdx] = (byte)(255 - intensity);
                        _depthFrame32[i32 + GreenIdx] = (byte)(255 - intensity);
                        _depthFrame32[i32 + BlueIdx] = (byte)(255 - intensity);
                        break;
                }
            }

            return _depthFrame32;
        }


        //void Nui_DepthFrameReady(object sender, Microsoft.Research.Kinect.Nui.ImageFrameReadyEventArgs e)
        //{
        //    PlanarImage image = e.ImageFrame.Image;
        //    byte[] convertedDepthFrame = ConvertDepthFrame(image.Bits);

        //    /*depthImage.Source = BitmapSource.Create(
        //        image.Width, image.Height, 96, 96, PixelFormats.Bgr32, null, convertedDepthFrame, image.Width * 4);*/
            
        //    if (pictureDepth.Image != null)
        //        pictureDepth.Image.Dispose();

        //    GCHandle handle =  GCHandle.Alloc(convertedDepthFrame);
            
        //    pictureDepth.Image = new Bitmap(image.Width, image.Height, image.Width * 4, System.Drawing.Imaging.PixelFormat.Format32bppRgb, GCHandle.ToIntPtr(handle));
        //    handle.Free();

        //    /*
        //    ++_totalFrames;

        //    DateTime cur = DateTime.Now;
        //    if (cur.Subtract(_lastTime) > TimeSpan.FromSeconds(1))
        //    {
        //        int frameDiff = _totalFrames - _lastFrames;
        //        _lastFrames = _totalFrames;
        //        _lastTime = cur;
        //        frameRate.Text = frameDiff + " fps";
        //    }
        //     * */
        //}

        private Point GetDisplayPosition(Joint joint)
        {
            return GetDisplayPosition(joint.Position);
        }

        private Point GetDisplayPosition(Vector4 pos)
        {
            return GetDisplayPosition(new Vector() {X = pos.X, Y = pos.Y, Z = pos.Z, W = pos.Z});
        }

        private Point GetDisplayPosition(Vector pos)
        {
            float depthX, depthY;
            kinect_.Nui.SkeletonEngine.SkeletonToDepthImage(pos, out depthX, out depthY);
            depthX = Math.Max(0, Math.Min(depthX * 320, 320)); // convert to 320, 240 space
            depthY = Math.Max(0, Math.Min(depthY * 240, 240)); // convert to 320, 240 space
            int colorX, colorY;
            var iv = new ImageViewArea();

            // Only ImageResolution.Resolution640x480 is supported at this point
            kinect_.Nui.NuiCamera.GetColorPixelCoordinatesFromDepthPixel(ImageResolution.Resolution640x480, iv, (int)depthX, (int)depthY, 0, out colorX, out colorY);

            // Map back to skeleton.Width & skeleton.Height
            return new Point((int)(pictureSkeleton.Width * colorX / 640.0), (int)(pictureSkeleton.Height * colorY / 480));
        }

        private Point[] GetBodySegment(JointsCollection joints, params JointID[] ids)
        {
            var points = new Point[ids.Length];
            var ix = 0;
            foreach (JointID t in ids)
            {
                points[ix] = GetDisplayPosition(joints[t]);
                ix++;
            }

            return points;
        }

        void Nui_SkeletonFrameReady(object sender, Microsoft.Research.Kinect.Nui.SkeletonFrameReadyEventArgs e)
        {
            skeletonFrame_ = e.SkeletonFrame;
            pictureSkeleton.Refresh();

            if (kinect_._capturing)
            {
                labelLastGesture.Text = "Recording gesture" + textGesture.Text + ": " + (int)((kinect_.nof_ / 32f)*100f) + " %";
            }

        }

        private void GesturesForm_Load(object sender, EventArgs e)
        {

        }

        private void pictureSkeleton_Paint(object sender, PaintEventArgs e)
        {
            if (skeletonFrame_ == null)
                return;

            int iSkeleton = 0;
            var brushes = new Brush[6];
            brushes[0] = new SolidBrush(Color.FromArgb(255, 0, 0));
            brushes[1] = new SolidBrush(Color.FromArgb(0, 255, 0));
            brushes[2] = new SolidBrush(Color.FromArgb(64, 255, 255));
            brushes[3] = new SolidBrush(Color.FromArgb(255, 255, 64));
            brushes[4] = new SolidBrush(Color.FromArgb(255, 64, 255));
            brushes[5] = new SolidBrush(Color.FromArgb(128, 128, 255));

            Graphics g = e.Graphics;
            g.Clear(Color.Black);
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            /*
            if (tracks_ != null)
            {
                Pen trackPen = new Pen(Color.DarkGray, 3);
                foreach (var track in tracks_.Values)
                {
                    Point[] trackLine =
                        track.Select(
                            (x) => new Point((int) (x.X*pictureSkeleton.Width), (int) (x.Y*pictureSkeleton.Height))).
                            ToArray();

                    g.DrawLines(trackPen, trackLine);

                }
            }*/

            foreach (SkeletonData data in skeletonFrame_.Skeletons)
            {
                if (SkeletonTrackingState.Tracked == data.TrackingState)
                {
                    /*
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
                    */

                    // Draw bones
                    Brush brush = brushes[iSkeleton % brushes.Length];
                    Pen pen = new Pen(brush, 3);

                    g.DrawLines(pen,
                                GetBodySegment(data.Joints, JointID.HipCenter, JointID.Spine, JointID.ShoulderCenter,
                                               JointID.Head));
                    g.DrawLines(pen,
                                GetBodySegment(data.Joints, JointID.ShoulderCenter, JointID.ShoulderLeft,
                                               JointID.ElbowLeft, JointID.WristLeft, JointID.HandLeft));
                    g.DrawLines(pen,
                                GetBodySegment(data.Joints, JointID.ShoulderCenter, JointID.ShoulderRight,
                                               JointID.ElbowRight, JointID.WristRight, JointID.HandRight));

                    g.DrawLines(pen,
                                GetBodySegment(data.Joints, JointID.HipCenter, JointID.HipLeft, JointID.KneeLeft,
                                               JointID.AnkleLeft, JointID.FootLeft));
                    g.DrawLines(pen,
                                GetBodySegment(data.Joints, JointID.HipCenter, JointID.HipRight, JointID.KneeRight,
                                               JointID.AnkleRight, JointID.FootRight));


                    // Draw joints
                    foreach (Joint joint in data.Joints)
                    {
                        Point jointPos = GetDisplayPosition(joint);

                        g.FillEllipse(_jointColors[joint.ID], jointPos.X - 5, jointPos.Y - 5, 10, 10);

                        /*
                        var jointLine = new Line();
                        jointLine.X1 = jointPos.X - 3;
                        jointLine.X2 = jointLine.X1 + 6;
                        jointLine.Y1 = jointLine.Y2 = jointPos.Y;
                        jointLine.Stroke = _jointColors[joint.ID];
                        jointLine.StrokeThickness = 6;
                        skeletonCanvas.Children.Add(jointLine);
                         */

                        /*
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
                        */
                    }
                }

                iSkeleton++;
            } // for each skeleton

        }

        private void buttonCapture_Click(object sender, EventArgs e)
        {
            _captureCountdown = DateTime.Now.AddSeconds(CaptureCountdownSeconds);

            _captureCountdownTimer = new Timer();
            _captureCountdownTimer.Interval = 50;
            _captureCountdownTimer.Start();
            _captureCountdownTimer.Tick += new EventHandler(_captureCountdownTimer_Tick);
        }

        void _captureCountdownTimer_Tick(object sender, EventArgs e)
        {
            if (sender == _captureCountdownTimer)
            {
                if (DateTime.Now < _captureCountdown)
                {
                    labelLastGesture.Text = "Wait " + ((_captureCountdown - DateTime.Now).Seconds + 1) + " seconds";
                }
                else
                {
                    _captureCountdownTimer.Stop();
                    //labelLastGesture.Text = "Recording gesture";
                    StartCapture();
                }
            }
        }

        /// <summary>
        /// Capture mode. Sets our control variables and button enabled states
        /// </summary>
        private void StartCapture()
        {
            // Set the buttons enabled state
            buttonCapture.Enabled = false;
            buttonDelete.Enabled = false;
            textGesture.Enabled = false;

            // Set the capturing? flag
            kinect_._capturing = true;

            ////_captureCountdownTimer.Dispose();

            labelLastGesture.Text = "Recording gesture" + textGesture.Text;

            // Clear the _video buffer and start from the beginning
            kinect_.RemoveAllFramesSkeletonTimeSequence();
        }

        void kinect__CaptureEnd(object sender, EventArgs e)
        {
            // Set the capturing? flag
            kinect_._capturing = false;

            //status.Text = "Remembering " + gestureList.Text;

            Dictionary<JointID, List<Vector4>> filteredSkeleteonTimeSequence = new Dictionary<JointID, List<Vector4>>();

            foreach (JointID joint in kinect_._jointsToCapture)
            {
                filteredSkeleteonTimeSequence.Add(joint, kinect_._skeletonTimeSequence[joint]);
            }

            // Add the current video buffer to the dtw sequences list
            //kinect_._dtw.AddOrUpdate(filteredSkeleteonTimeSequence, textGesture.Text);
            
            
            //results.Text = "Gesture " + gestureList.Text + "added";

            if (!listGestures.Items.Contains(textGesture.Text))
            {
                listGestures.Items.Add(textGesture.Text);
            }

            // Scratch the _video buffer
            kinect_.RemoveAllFramesSkeletonTimeSequence();

            // Switch back to Read mode
            //DtwReadClick(null, null);
            // Set the buttons enabled state
            buttonCapture.Enabled = true;
            buttonDelete.Enabled = true;
            textGesture.Enabled = true;

            // Update the status display
            labelLastGesture.Text = "Reading gestures...";

        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (listGestures.SelectedIndex < 0)
                return;
            //kinect_._dtw.Delete(listGestures.SelectedItem as string);
            listGestures.Items.RemoveAt(listGestures.SelectedIndex);
        }

        private void GesturesForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            addId_.pres_.Tags.Add("KY_Gestures", kinect_._dtw.RetrieveText());
            addId_.pres_.Tags.Add("KY_GestureNames", string.Join(";", kinect_._dtw._recordedGestures.Keys));
            
            //addId_.UnloadKinect();

            //addId_.pane_.UpdateAll();
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Gesture file|*.gst";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                kinect_.LoadGesturesFromFile(dialog.FileName);
                listGestures.Items.Clear();
                foreach (var ges in kinect_._dtw._recordedGestures.Keys)
                {
                    listGestures.Items.Add(ges);
                }
            }
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "Gesture file|*.gst";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (var str = new StreamWriter(dialog.FileName))
                    str.Write(kinect_._dtw.RetrieveText());
            }
        }

    }
}
