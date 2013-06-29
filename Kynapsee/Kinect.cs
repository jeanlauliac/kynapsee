using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using Kynapsee.Nui.Tools;
using Microsoft.Research.Kinect.Nui;
using Microsoft.Xna.Framework;

namespace Kynapsee
{
    public class GestureEventArgs : EventArgs
    {
        public GestureEventArgs(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }

    public class Kinect
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
        /*
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
        */
        /// <summary>
        /// Enum Comparer for speeding up use of enums as keys in dictionary.
        /// </summary>
        public static EnumComparer<JointID> comparer = EnumComparer<JointID>.Instance;

        public Dictionary<JointID, List<Vector4>> _skeletonTimeSequence = new Dictionary<JointID, List<Vector4>>(comparer)
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
        public HashSet<JointID> _jointsToCapture = new HashSet<JointID>() 
        {
                //JointID.HandLeft,
                JointID.WristLeft,
               // JointID.ElbowLeft,
                //JointID.HandRight,
                JointID.WristRight,
                //JointID.ElbowRight
        };
        
        /// <summary>
        /// The depth frame byte array. Only supports 320 * 240 at this time
        /// </summary>
        private readonly byte[] _depthFrame32 = new byte[320 * 240 * 4];

        /// <summary>
        /// Flag to show whether or not the gesture recogniser is capturing a new pose
        /// </summary>
        public bool _capturing;

        /// <summary>
        /// Dynamic Time Warping object
        /// </summary>
        public DtwGestureRecognizer _dtw;

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
        public Kinect()
        {
            
        }

        public void LoadGesturesFromFile(string path)
        {
            var _assembly = Assembly.GetExecutingAssembly();
            using (var str = new System.IO.StreamReader(path)) //_assembly.GetManifestResourceStream(
                LoadGesturesFromFile(str);
        }

        /// <summary>
        /// Load the gesture data from a file specified. 
        /// </summary>
        /// <param name="fileLocation">Where the file is</param>
        public void LoadGesturesFromFile(TextReader file) 
        {
#if false
            int itemCount = 0;
            string line;
            string gestureName = String.Empty;
            string jointName = String.Empty;
            float[] items = new float[_vectorDimension];
            Dictionary<JointID, List<Vector4>> gesture = new Dictionary<JointID, List<Vector4>>();
            JointID currentJoint = JointID.HipCenter; //Need to assign this a default value. For now I never use HipCenter, so this can be used as a sentinel value I guess.
            
            // Read the file and display it line by line.
            // FIXED: load resource file instead
            //System.IO.StreamReader file = new System.IO.StreamReader(fileLocation);
            //System.IO.StreamReader file = new System.IO.StreamReader(_assembly.GetManifestResourceStream(fileLocation));

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
#endif
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
                //Skeleton3DDataExtract.ProcessData(data);
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


        public Runtime Nui { get { return _nui; } }

        /// <summary>
        /// Runs after the window is loaded
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Routed Event Args</param>
        public void Init()
        {
            _nui = new Runtime();

            try
            {
                _nui.Initialize(RuntimeOptions.UseSkeletalTracking | RuntimeOptions.UseColor | RuntimeOptions.UseDepthAndPlayerIndex);
            }
            catch (InvalidOperationException)
            {
                //System.Windows.MessageBox.Show("Runtime initialization failed. Please make sure Kinect device is plugged in.");

                throw;
            }

            //try
            //{
            //    _nui.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
            //    _nui.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240, ImageType.DepthAndPlayerIndex);
            //}
            //catch (InvalidOperationException)
            //{
            //    //System.Windows.MessageBox.Show(
            //    //    "Failed to open stream. Please make sure to specify a supported image type and resolution.");
            //    throw;
            //}

            _lastTime = DateTime.Now;

            _dtw = new DtwGestureRecognizer(_vectorDimension, _jointsTracked, _sequenceSimilarityThreshold, _finalPositionThreshold, _maxSlope, _minGestureLength);
            RemoveAllFramesSkeletonTimeSequence();
            
            // If you want to see the depth image and frames per second then include this
            // I'mma turn this off 'cos my 'puter is proper slow
            //_nui.DepthFrameReady += NuiDepthFrameReady;

            //_nui.SkeletonFrameReady += NuiSkeletonFrameReady;
            _nui.SkeletonFrameReady += SkeletonExtractSkeletonFrameReady;

            // If you want to see the RGB stream then include this
            //_nui.VideoFrameReady += NuiColorFrameReady;

            //Skeleton3DDataExtract.Skeleton3DDataCoordReady += NuiSkeleton3DDataCoordReady;

            // Update the debug window with Sequences information
            //dtwTextOutput.Text = _dtw.RetrieveText();

            Debug.WriteLine("Finished Window Loading");
        }

        /// <summary>
        /// Runs some tidy-up code when the window is closed. This is especially important for our NUI instance because the Kinect SDK is very picky about this having been disposed of nicely.
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Event Args</param>
        public void Uninit()
        {
            Debug.WriteLine("Stopping NUI");
            _nui.Uninitialize();
            Debug.WriteLine("NUI stopped");
            
        }

        public event EventHandler<GestureEventArgs> GestureDone;
        public event EventHandler NoGestureDone;

        public int nof_;

        private bool nogesture_;

        /// <summary>
        /// Runs every time our 3D coordinates are ready.
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="a">Skeleton 3Ddata Coord Event Args</param>
        private void NuiSkeleton3DDataCoordReady(object sender, Skeleton3DDataCoordEventArgs a)
        {
#if false
            //Assume that capture equal number of frames for all joints! 
            var iterator = _skeletonTimeSequence.GetEnumerator();
            //Must MoveNext to get first element.
            iterator.MoveNext();
            int numberOfFramesCaptured = iterator.Current.Value.Count;
            //currentBufferFrame.Text = numberOfFramesCaptured.ToString();
            bool test = false;

            // We need a sensible number of frames before we start attempting to match gestures against remembered sequences
            if (numberOfFramesCaptured > MinimumFrames && _capturing == false)
            {
                string s = _dtw.Recognize(_skeletonTimeSequence);
                //results.Text = s;
                if (!s.Contains("__UNKNOWN"))
                {
                    // There was a match so reset the buffer
                    //_video = new ArrayList();
                    RemoveAllFramesSkeletonTimeSequence();
                    if (GestureDone != null)
                        GestureDone(this, new GestureEventArgs(s));
                    test = true;
                    nogesture_ = false;
                }
                else
                {
                    
                    if (!nogesture_ && NoGestureDone != null)
                        NoGestureDone(this, new EventArgs());
                    nogesture_ = true;
                }
            }
            //Get current number of frames again. Hacky. 
            iterator = _skeletonTimeSequence.GetEnumerator();
            iterator.MoveNext();
            numberOfFramesCaptured = iterator.Current.Value.Count;
            nof_ = numberOfFramesCaptured;

            // Ensures that we remember only the last x frames
            if (numberOfFramesCaptured > BufferSize)
            {
                // If we are currently capturing and we reach the maximum buffer size then automatically store
                if (_capturing)
                {
                    // FIXED: no capturing mode for Kynapsee
                    //DtwStoreClick(null, null);
                    if (CaptureEnd != null)
                        CaptureEnd(this, new EventArgs());
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
#endif
        }

        public event EventHandler CaptureEnd;



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
        public void RemoveAllFramesSkeletonTimeSequence() 
        {
            foreach (JointID joint in _skeletonTimeSequence.Keys)
            {
                _skeletonTimeSequence[joint].Clear();
            }

        }
    }
}
