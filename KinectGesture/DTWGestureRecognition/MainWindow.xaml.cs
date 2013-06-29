//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Chin Xiang Chong">
//     Open Source. Do with this as you will. Include this statement or 
//     don't - whatever you like.
//
//     This is modified from code originally from 
//     http://kinectdtw.codeplex.com/ 
//     Specifically, the changes are to allow greater flexibility
//     by allowing the user to specify what joints to use
//     when recording gestures. The original implementation records 
//     data from Left and Right Hand, Wrist, Elbow and Shoulder for 
//     all gestures. (The Shoulder data is only used to "normalize"
//     the data to account for people of different sizes).
//     This has problems like the position of your right hand affecting
//     if a gesture that only has your left hand moving being recognised or not.
//     This implementation allows you to specify what body parts count for which
//     gesture. The original implementation also only enabled 2D gestures. 
//     This allows for full 3D gestures, e.g. gestures in which you push your hand
//     forward.

//     In case you are wondering specifically what functions I changed:
//     I've pretty much touched every function that was in the original code,
//     to an extent that I'm comfortable calling it my code as opposed to
//     code I tweaked slightly. 
//     
//     Chin's website: www.waxinlyrical.com/codesamples/KinectGestures
// </copyright>
//-----------------------------------------------------------------------

namespace DTWGestureRecognition
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;
    using Microsoft.Research.Kinect.Nui;
    using Microsoft.Xna.Framework;
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        // We want to control how depth data gets converted into false-color data
        // for more intuitive visualization, so we keep 32-bit color frame buffer versions of
        // these, to be updated whenever we receive and process a 16-bit frame.

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
        /// How many skeleton frames to ignore (_flipFlop)
        /// 1 = capture every frame, 2 = capture every second frame etc.
        /// </summary>
        private const int Ignore = 2;

        /// <summary>
        /// How many skeleton frames to store in the _video buffer
        /// </summary>
        private const int BufferSize = 32;

        /// <summary>
        /// The minumum number of frames in the _video buffer before we attempt to start matching gestures
        /// </summary>
        private const int MinimumFrames = 10;

        /// <summary>
        /// The minumum number of frames in the _video buffer before we attempt to start matching gestures
        /// </summary>
        private const int CaptureCountdownSeconds = 3;

        /// <summary>
        /// Where we will save our gestures to. The app will append a data/time and .txt to this string
        /// </summary>
        private string GestureSaveFileLocation = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        


        /// <summary>
        /// Where we will save our gestures to. The app will append a data/time and .txt to this string
        /// </summary>
        //private const string GestureSaveFileNamePrefix = @"\RecordedGestures\";
        private const string GestureSaveFileNamePrefix = @"\KinectGesture";

        /// <summary>
        /// Dictionary of all the joints Kinect SDK is capable of tracking. You might not want always to use them all but they are included here for thouroughness.
        /// </summary>
        private readonly Dictionary<JointID, Brush> _jointColors = new Dictionary<JointID, Brush>
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

        /// <summary>
        /// Enum Comparer for speeding up use of enums as keys in dictionary.
        /// </summary>
        private static EnumComparer<JointID> comparer = EnumComparer<JointID>.Instance;
        
        private Dictionary<JointID, List<Vector4>> _skeletonTimeSequence = new Dictionary<JointID, List<Vector4>>(comparer)
        {
                {JointID.HandLeft, new List<Vector4>()},
                {JointID.WristLeft, new List<Vector4>()},
                {JointID.ElbowLeft, new List<Vector4>()},
                {JointID.HandRight, new List<Vector4>()},
                {JointID.WristRight, new List<Vector4>()},
                {JointID.ElbowRight, new List<Vector4>()},
                {JointID.ShoulderLeft, new List<Vector4>()},
                {JointID.ShoulderRight, new List<Vector4>()}
              
        };

        /// <summary>
        /// Set of all joints we care about
        /// </summary>
        private HashSet<JointID> _jointsToCapture = new HashSet<JointID>() 
        {
                JointID.HandLeft,
                JointID.WristLeft,
                JointID.ElbowLeft,
                JointID.HandRight,
                JointID.WristRight,
                JointID.ElbowRight
        };
        
             
        

        /// <summary>
        /// The depth frame byte array. Only supports 320 * 240 at this time
        /// </summary>
        private readonly byte[] _depthFrame32 = new byte[320 * 240 * 4];

        /// <summary>
        /// Flag to show whether or not the gesture recogniser is capturing a new pose
        /// </summary>
        private bool _capturing;

        /// <summary>
        /// Dynamic Time Warping object
        /// </summary>
        private DtwGestureRecognizer _dtw;

        //Dimension of position vector of joints you care about. Can range from 2 to 4. Set to 2 to only track X and Y coordinates, set to 3 to track X, Y and Z, and set to 4 to track X, Y, Z and W.
        static readonly int _vectorDimension = 3;

        //How many joints you want to track. Right now we only care about left hand, wrist and elbow, right equivalents. Left shoulder and right shoulder are used to
        //normalize, but are not stored. 
        static readonly int _jointsTracked = 8;

        //Min length of gesture. When this is set to 10, for example, it checks to see if frames 0-10 are a good match for the sample gesture, or if frames 0-11 are, or if 0-12 are, etc till 0-32. 
        static readonly int _minGestureLength = 10;

        //How many horizontal steps you can take before you have to take a vertical step, or vice versa. 
        static readonly int _maxSlope = 2;

        //Measure of how far apart the final position of the input gesture has to be from the final position of the sample gesture. 
        //Original framework value was 2.0
        static readonly double _finalPositionThreshold= 1.2;

        //Measure of how far apart the input sequence has to be from the sample sequence. 
        //original framework value was 0.6
        //static readonly double _sequenceSimilarityThreshold = 1.0;
        static readonly double _sequenceSimilarityThreshold = 1.0;

        /// <summary>
        /// How many frames occurred 'last time'. Used for calculating frames per second
        /// </summary>
        private int _lastFrames;

        /// <summary>
        /// The 'last time' DateTime. Used for calculating frames per second
        /// </summary>
        private DateTime _lastTime = DateTime.MaxValue;

        /// <summary>
        /// The Natural User Interface runtime
        /// </summary>
        private Runtime _nui;

        /// <summary>
        /// Total number of framed that have occurred. Used for calculating frames per second
        /// </summary>
        private int _totalFrames;

        /// <summary>
        /// Switch used to ignore certain skeleton frames
        /// </summary>
        private int _flipFlop;

        /// <summary>
        /// ArrayList of coordinates which are recorded in sequence to define one gesture
        /// </summary>
        private DateTime _captureCountdown = DateTime.Now;

        /// <summary>
        /// ArrayList of coordinates which are recorded in sequence to define one gesture
        /// </summary>
        private Timer _captureCountdownTimer;

        /// <summary>
        /// Initializes a new instance of the MainWindow class
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Load the gesture data from a file specified. 
        /// </summary>
        /// <param name="fileLocation">Where the file is</param>
        public void LoadGesturesFromFile(string fileLocation) 
        {
            int itemCount = 0;
            string line;
            string gestureName = String.Empty;
            string jointName = String.Empty;
            float[] items = new float[_vectorDimension];
            Dictionary<JointID, List<Vector4>> gesture = new Dictionary<JointID, List<Vector4>>();
            JointID currentJoint = JointID.HipCenter; //Need to assign this a default value. For now I never use HipCenter, so this can be used as a sentinel value I guess.

            // Read the file and display it line by line.
            System.IO.StreamReader file = new System.IO.StreamReader(fileLocation);
            while ((line = file.ReadLine()) != null)
            {
                if (line.StartsWith("@")) 
                {
                    //What follows is the name of the joint;
                    if ((line = file.ReadLine()) != null)
                    {
                        gestureName = line;
                        gesture = new Dictionary<JointID, List<Vector4>>();
                        continue;
                    }
                    else 
                    {
                        throw new Exception("Error parsing file: End of file found when expecting gesture name!");
                    }
                    
                }
                if (line.StartsWith("+"))
                {
                    //What follows is the name of the joint;
                    if ((line = file.ReadLine()) != null)
                    {
                        if (Enum.TryParse(line, out currentJoint))
                        {
                            gesture.Add(currentJoint, new List<Vector4>());
                            continue;
                        }
                        else 
                        {
                            //String does not correspond to a valid JointID!
                            throw new Exception("Invalid joint ID : " + line + "found!");
                        }
                    }
                    else 
                    {
                        //No name of joint detected?
                        throw new Exception("Error parsing file: End of file found when expecting valid joint ID!");
                    }
                    
                }

                if (line.StartsWith("~"))
                {
                    //I'm sure there's a better way to do this, but this is fine for now
                    switch (_vectorDimension) 
                    {
                        case (2): 
                        {
                            gesture[currentJoint].Add(new Vector4(items[0], items[1], 0, 0));
                            break;
                        }

                        case (3):
                        {
                            gesture[currentJoint].Add(new Vector4(items[0], items[1], items[2], 0));
                            break;
                        }

                        case (4):
                        {
                            gesture[currentJoint].Add(new Vector4(items[0], items[1], items[2], items[3]));
                            break;
                        }

                        //default to dimension 4
                        default:                          
                        {
                            gesture[currentJoint].Add(new Vector4(items[0], items[1], items[2], items[3]));
                            break;
                        }


                    }
                    itemCount = 0;
                    continue;
                }

                if (!line.StartsWith("---"))
                {
                    items[itemCount] = Single.Parse(line);
                    ++itemCount;
                    continue;
                }
                else 
                {
                    _dtw.AddOrUpdate(gesture, gestureName);
                    itemCount = 0;
                    gestureName = String.Empty;
                    continue; 
                }
            }
            file.Close();
        }

        /// <summary>
        /// Called each time a skeleton frame is ready. Passes skeletal data to the DTW processor
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Skeleton Frame Ready Event Args</param>
        private static void SkeletonExtractSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame skeletonFrame = e.SkeletonFrame;
            foreach (SkeletonData data in skeletonFrame.Skeletons)
            {
                Skeleton3DDataExtract.ProcessData(data, _jointsTracked);
            }
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

        /// <summary>
        /// Called when each depth frame is ready
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Image Frame Ready Event Args</param>
        private void NuiDepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            PlanarImage image = e.ImageFrame.Image;
            byte[] convertedDepthFrame = ConvertDepthFrame(image.Bits);

            depthImage.Source = BitmapSource.Create(
                image.Width, image.Height, 96, 96, PixelFormats.Bgr32, null, convertedDepthFrame, image.Width * 4);

            ++_totalFrames;

            DateTime cur = DateTime.Now;
            if (cur.Subtract(_lastTime) > TimeSpan.FromSeconds(1))
            {
                int frameDiff = _totalFrames - _lastFrames;
                _lastFrames = _totalFrames;
                _lastTime = cur;
                frameRate.Text = frameDiff + " fps";
            }
        }

        /// <summary>
        /// Gets the display position (i.e. where in the display image) of a Joint
        /// </summary>
        /// <param name="joint">Kinect NUI Joint</param>
        /// <returns>Point mapped location of sent joint</returns>
        private System.Windows.Point GetDisplayPosition(Joint joint)
        {
            float depthX, depthY;
            _nui.SkeletonEngine.SkeletonToDepthImage(joint.Position, out depthX, out depthY);
            depthX = Math.Max(0, Math.Min(depthX * 320, 320)); // convert to 320, 240 space
            depthY = Math.Max(0, Math.Min(depthY * 240, 240)); // convert to 320, 240 space
            int colorX, colorY;
            var iv = new ImageViewArea();

            // Only ImageResolution.Resolution640x480 is supported at this point
            _nui.NuiCamera.GetColorPixelCoordinatesFromDepthPixel(ImageResolution.Resolution640x480, iv, (int)depthX, (int)depthY, 0, out colorX, out colorY);

            // Map back to skeleton.Width & skeleton.Height
            return new System.Windows.Point((int)(skeletonCanvas.Width * colorX / 640.0), (int)(skeletonCanvas.Height * colorY / 480));
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
                        jointLine.Stroke = _jointColors[joint.ID];
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

        /// <summary>
        /// Called every time a video (RGB) frame is ready
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Image Frame Ready Event Args</param>
        private void NuiColorFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            // 32-bit per pixel, RGBA image
            PlanarImage image = e.ImageFrame.Image;
            videoImage.Source = BitmapSource.Create(
                image.Width, image.Height, 96, 96, PixelFormats.Bgr32, null, image.Bits, image.Width * image.BytesPerPixel);
        }

        /// <summary>
        /// Runs after the window is loaded
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Routed Event Args</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            _nui = new Runtime();

            try
            {
                _nui.Initialize(RuntimeOptions.UseDepthAndPlayerIndex | RuntimeOptions.UseSkeletalTracking |
                               RuntimeOptions.UseColor);
            }
            catch (InvalidOperationException)
            {
                System.Windows.MessageBox.Show("Runtime initialization failed. Please make sure Kinect device is plugged in.");
                return;
            }

            try
            {
                _nui.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
                _nui.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240, ImageType.DepthAndPlayerIndex);
            }
            catch (InvalidOperationException)
            {
                System.Windows.MessageBox.Show(
                    "Failed to open stream. Please make sure to specify a supported image type and resolution.");
                return;
            }

            _lastTime = DateTime.Now;

            _dtw = new DtwGestureRecognizer(_vectorDimension, _jointsTracked, _sequenceSimilarityThreshold, _finalPositionThreshold, _maxSlope, _minGestureLength);
            RemoveAllFramesSkeletonTimeSequence();
            
            // If you want to see the depth image and frames per second then include this
            // I'mma turn this off 'cos my 'puter is proper slow
            _nui.DepthFrameReady += NuiDepthFrameReady;

            _nui.SkeletonFrameReady += NuiSkeletonFrameReady;
            _nui.SkeletonFrameReady += SkeletonExtractSkeletonFrameReady;

            // If you want to see the RGB stream then include this
            _nui.VideoFrameReady += NuiColorFrameReady;

            Skeleton3DDataExtract.Skeleton3DDataCoordReady += NuiSkeleton3DDataCoordReady;

            // Update the debug window with Sequences information
            dtwTextOutput.Text = _dtw.RetrieveText();

            Debug.WriteLine("Finished Window Loading");
        }

        /// <summary>
        /// Runs some tidy-up code when the window is closed. This is especially important for our NUI instance because the Kinect SDK is very picky about this having been disposed of nicely.
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Event Args</param>
        private void WindowClosed(object sender, EventArgs e)
        {
            Debug.WriteLine("Stopping NUI");
            _nui.Uninitialize();
            Debug.WriteLine("NUI stopped");
            Environment.Exit(0);
        }

        /// <summary>
        /// Runs every time our 3D coordinates are ready.
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="a">Skeleton 3Ddata Coord Event Args</param>
        private void NuiSkeleton3DDataCoordReady(object sender, Skeleton3DDataCoordEventArgs a)
        {
            
            //Assume that capture equal number of frames for all joints! 
            var iterator = _skeletonTimeSequence.GetEnumerator();
            //Must MoveNext to get first element.
            iterator.MoveNext();
            int numberOfFramesCaptured = iterator.Current.Value.Count;
            currentBufferFrame.Text = numberOfFramesCaptured.ToString();
            bool test = false;

            // We need a sensible number of frames before we start attempting to match gestures against remembered sequences
            if (numberOfFramesCaptured > MinimumFrames && _capturing == false)
            {
                string s = _dtw.Recognize(_skeletonTimeSequence);
                results.Text = s;
                if (!s.Contains("__UNKNOWN"))
                {
                    // There was a match so reset the buffer
                    //_video = new ArrayList();
                    RemoveAllFramesSkeletonTimeSequence();
                    test = true;
                }
            }
            //Get current number of frames again. Hacky. 
            iterator = _skeletonTimeSequence.GetEnumerator();
            iterator.MoveNext();
            numberOfFramesCaptured = iterator.Current.Value.Count;

            // Ensures that we remember only the last x frames
            if (numberOfFramesCaptured > BufferSize)
            {
                // If we are currently capturing and we reach the maximum buffer size then automatically store
                if (_capturing)
                {
                    DtwStoreClick(null, null);
                }
                else
                {
                    // Remove the first frame in the buffer
                    //_video.RemoveAt(0);
                    if (test) 
                    {
                        throw new Exception();
                    }
                    RemoveFirstFrameSkeletonTimeSequence();
                }
            }

            // Decide which skeleton frames to capture. Only do so if the frames actually returned a number. 
            // For some reason my Kinect/PC setup didn't always return a double in range (i.e. infinity) even when standing completely within the frame.
            // TODO Weird. Need to investigate this
            //if (!double.IsNaN(a.GetPoint(0).X))
            var snapShot = a.GetSkeletonSnapshot();
            var snapShotIterator = snapShot.GetEnumerator();
            snapShotIterator.MoveNext();
            if (!double.IsNaN(snapShotIterator.Current.Value.X))
            {
                // Optionally register only 1 frame out of every n
                _flipFlop = (_flipFlop + 1) % Ignore;
                if (_flipFlop == 0)
                {
                    //_video.Add(a.GetCoords());
                    AppendToSkeletonTimeSequence(snapShot);
                }
            }

            // Update the debug window with Sequences information
            //dtwTextOutput.Text = _dtw.RetrieveText();
        }

        /// <summary>
        /// Read mode. Sets our control variables and button enabled states
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Routed Event Args</param>
        private void DtwReadClick(object sender, RoutedEventArgs e)
        {
            // Set the buttons enabled state
            dtwRead.IsEnabled = false;
            dtwCapture.IsEnabled = true;
            dtwStore.IsEnabled = false;

            // Set the capturing? flag
            _capturing = false;

            // Update the status display
            status.Text = "Reading";
        }

        /// <summary>
        /// Starts a countdown timer to enable the player to get in position to record gestures
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Routed Event Args</param>
        private void DtwCaptureClick(object sender, RoutedEventArgs e)
        {
            _captureCountdown = DateTime.Now.AddSeconds(CaptureCountdownSeconds);

            _captureCountdownTimer = new Timer();
            _captureCountdownTimer.Interval = 50;
            _captureCountdownTimer.Start();
            _captureCountdownTimer.Tick += CaptureCountdown;
        }

        /// <summary>
        /// The method fired by the countdown timer. Either updates the countdown or fires the StartCapture method if the timer expires
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Event Args</param>
        private void CaptureCountdown(object sender, EventArgs e)
        {
            if (sender == _captureCountdownTimer)
            {
                if (DateTime.Now < _captureCountdown)
                {
                    status.Text = "Wait " + ((_captureCountdown - DateTime.Now).Seconds + 1) + " seconds";
                }
                else
                {
                    _captureCountdownTimer.Stop();
                    status.Text = "Recording gesture";
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
            dtwRead.IsEnabled = false;
            dtwCapture.IsEnabled = false;
            dtwStore.IsEnabled = true;

            // Set the capturing? flag
            _capturing = true;

            ////_captureCountdownTimer.Dispose();

            status.Text = "Recording gesture" + gestureList.Text;

            // Clear the _video buffer and start from the beginning
            RemoveAllFramesSkeletonTimeSequence();
        }

        /// <summary>
        /// Stores our gesture to the DTW sequences list
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Routed Event Args</param>
        private void DtwStoreClick(object sender, RoutedEventArgs e)
        {
            // Set the buttons enabled state
            dtwRead.IsEnabled = false;
            dtwCapture.IsEnabled = true;
            dtwStore.IsEnabled = false;

            // Set the capturing? flag
            _capturing = false;

            status.Text = "Remembering " + gestureList.Text;

            Dictionary<JointID, List<Vector4>> filteredSkeleteonTimeSequence = new Dictionary<JointID, List<Vector4>>();

            foreach (JointID joint in _jointsToCapture) 
            {
                filteredSkeleteonTimeSequence.Add(joint, _skeletonTimeSequence[joint]);
            }

            // Add the current video buffer to the dtw sequences list
            _dtw.AddOrUpdate(filteredSkeleteonTimeSequence, gestureList.Text);
            results.Text = "Gesture " + gestureList.Text + "added";

            // Scratch the _video buffer
            RemoveAllFramesSkeletonTimeSequence();

            // Switch back to Read mode
            DtwReadClick(null, null);
        }

        /// <summary>
        /// Stores our gesture to the DTW sequences list
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Routed Event Args</param>
        private void DtwSaveToFile(object sender, RoutedEventArgs e)
        {
            string fileName = GestureSaveFileNamePrefix + DateTime.Now.ToString("MM-dd_HH-mm") + ".txt";
            System.IO.File.WriteAllText(GestureSaveFileLocation + fileName, _dtw.RetrieveText());
            status.Text = "Saved to " + fileName;
        }

        /// <summary>
        /// Loads the user's selected gesture file
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Routed Event Args</param>
        private void DtwLoadFile(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text documents (.txt)|*.txt";

            dlg.InitialDirectory = GestureSaveFileLocation;
            

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                LoadGesturesFromFile(dlg.FileName);
                GestureSaveFileLocation = System.IO.Path.GetDirectoryName(dlg.FileName);
                dtwTextOutput.Text = _dtw.RetrieveText();
                status.Text = "Gestures loaded!";
            } 
        }

        /// <summary>
        /// Stores our gesture to the DTW sequences list
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Routed Event Args</param>
        private void DtwShowGestureText(object sender, RoutedEventArgs e)
        {
            dtwTextOutput.Text = _dtw.RetrieveText();
        }

        /// <summary>
        /// Logic to handle checking of boxes
        /// </summary>
        private void HandleChecked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox cb = sender as System.Windows.Controls.CheckBox;
            switch (cb.Name) 
            {
                case ("lHand"):
                    _jointsToCapture.Add(JointID.HandLeft);
                    break;
                case ("lWrist"):
                    _jointsToCapture.Add(JointID.WristLeft);
                    break;
                case("lElbow"):
                    _jointsToCapture.Add(JointID.ElbowLeft);
                    break;
                case ("rHand"):
                    _jointsToCapture.Add(JointID.HandRight);
                    break;
                case("rWrist"):
                    _jointsToCapture.Add(JointID.WristRight);
                    break;
                case("rElbow"):
                    _jointsToCapture.Add(JointID.ElbowRight);
                    break;
                default:
                    Debug.WriteLine("Check box malfunction?");
                    break;
            }
            
        }

        /// <summary>
        /// Logic to handle unchecking of boxes
        /// </summary>
        private void HandleUnchecked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox cb = sender as System.Windows.Controls.CheckBox;
            switch (cb.Name)
            {
                case ("lHand"):
                    _jointsToCapture.Remove(JointID.HandLeft);
                    break;
                case ("lWrist"):
                    _jointsToCapture.Remove(JointID.WristLeft);
                    break;
                case ("lElbow"):
                    _jointsToCapture.Remove(JointID.ElbowLeft);
                    break;
                case ("rHand"):
                    _jointsToCapture.Remove(JointID.HandRight);
                    break;
                case ("rWrist"):
                    _jointsToCapture.Remove(JointID.WristRight);
                    break;
                case ("rElbow"):
                    _jointsToCapture.Remove(JointID.ElbowRight);
                    break;
                default:
                    Debug.WriteLine("Check box malfunction?");
                    break;
            }
        }

        /// <summary>
        /// Append a single skeleton snapshot to our ongoing skeleton time sequence
        /// </summary>
        /// <param name="skeletonSnapshot">The snap shot of the skeleton we get</param>
        private void AppendToSkeletonTimeSequence(Dictionary<JointID, Vector4> skeletonSnapshot) 
        {
            foreach (JointID joint in skeletonSnapshot.Keys)
            {
                _skeletonTimeSequence[joint].Add(skeletonSnapshot[joint]);

            }
        }

        /// <summary>
        /// Remove the first frame of the skeleton time sequence
        /// </summary>
        private void RemoveFirstFrameSkeletonTimeSequence() 
        {
            foreach (JointID joint in _skeletonTimeSequence.Keys) 
            {
                _skeletonTimeSequence[joint].RemoveAt(0);
            }
        }

        /// <summary>
        /// Remove all frames of the skeleton time sequence
        /// </summary>
        private void RemoveAllFramesSkeletonTimeSequence() 
        {
            foreach (JointID joint in _skeletonTimeSequence.Keys)
            {
                _skeletonTimeSequence[joint].Clear();
            }

        }

    }
}