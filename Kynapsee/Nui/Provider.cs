using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kynapsee.Nui.Model;
using Kynapsee.Nui.Tools;
using Microsoft.Research.Kinect.Nui;
using Microsoft.Xna.Framework;

namespace Kynapsee.Nui
{
    /// <summary>
    /// Provide access to a NUI device.
    /// </summary>
    public class Provider : IDisposable
    {
        #region Gesture Recognition Configuration

        /// <summary>
        /// Dimension of position vector of joints you care about. Can range from 2 to 4.
        /// Set to 2 to only track X and Y coordinates, set to 3 to track X, Y and Z, and set to 4 to track X, Y, Z and W.
        /// </summary>
        public const int VectorDimension = 3;

        /// <summary>
        /// How many joints you want to track. Right now we only care about left hand,
        /// wrist and elbow, right equivalents. Left shoulder and right shoulder are used to
        /// normalize, but are not stored. 
        /// </summary>
        public const int JointsTracked = 2;

        /// <summary>
        /// Min length of gesture. When this is set to 10, for example, it checks to see if
        /// frames 0-10 are a good match for the sample gesture, or if frames 0-11 are, or if 0-12 are, etc till 0-32. 
        /// </summary>
        public const int MinGestureLength = 10;

        /// <summary>
        /// How many horizontal steps you can take before you have to take a vertical step, or vice versa. 
        /// </summary>
        public const int MaxSlope = 2;

        /// <summary>
        /// Measure of how far apart the final position of the input gesture has to be from the final
        /// position of the sample gesture. Original framework value was 2.0
        /// </summary>
        public const double FinalPositionThreshold = 1.2;

        /// <summary>
        /// Measure of how far apart the input sequence has to be from the sample sequence. 
        /// Original framework value was 0.6
        /// </summary>
        public const double SequenceSimilarityThreshold = 1.0;

        /// <summary>
        /// How many skeleton frames to ignore (_flipFlop)
        /// 1 = capture every frame, 2 = capture every second frame etc.
        /// </summary>
        public const int Ignore = 2;

        /// <summary>
        /// How many skeleton frames to store in the _video buffer
        /// </summary>
        public const int BufferSize = 32;

        /// <summary>
        /// The minumum number of frames in the _video buffer before we attempt to start matching gestures
        /// </summary>
        public const int MinimumFrames = 10;

        #endregion

        #region Internal Variables

        private Runtime nui_;
        private readonly DtwGestureRecognizer dtw_;
        private Skeleton3DDataExtract sde_ = new Skeleton3DDataExtract();

        private readonly Dictionary<JointID, List<Vector4>> skeletonTimeSequence_ = new Dictionary<JointID, List<Vector4>>(Comparer)
        {
                {JointID.WristLeft, new List<Vector4>()},
                {JointID.WristRight, new List<Vector4>()}
        };

        private static readonly EnumComparer<JointID> Comparer = EnumComparer<JointID>.Instance;

        /// <summary>
        /// Flag to show whether or not the gesture recogniser is capturing a new pose
        /// </summary>
        private bool capturing_ = false;

        /// <summary>
        /// Switch used to ignore certain skeleton frames
        /// </summary>
        private int flipFlop_;


        #endregion

        #region Events

        public event EventHandler<NuiEventArgs> FrameStart;
        public event EventHandler<NuiEventArgs> GestureDone;
        public event EventHandler<NuiEventArgs> CaptureProgress;
        public event EventHandler<NuiEventArgs> CaptureEnd;


        #endregion

        #region Properties

        /// <summary>
        /// Gets the raw NUI runtime.
        /// </summary>
        public Runtime Runtime { get { return nui_; } }

        /// <summary>
        /// Gets the raw gesture recognizer algorithm object.
        /// </summary>
        public DtwGestureRecognizer Recognizer { get { return dtw_; } }

        /// <summary>
        /// Gets the full gesture set stored in the recognizer.
        /// </summary>
        public GestureSet GestureSet
        {
            get
            {
                var gestureSet = new GestureSet();
                gestureSet.Gestures = dtw_._recordedGestures.Select((gs)
                    => gs.Key).ToList();

                return gestureSet;
            }
        }

        public bool HasSkeleton { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new provider on the first found NUI device.
        /// </summary>
        /// <param name="gestures">Gesture model.</param>
        /// <param name="enableColor">Enable color capture.</param>
        /// <param name="enableDepth">Enable depth capture.</param>
        public Provider(GestureSet gestures = null, bool enableColor = false, bool enableDepth = false)
        {
            try
            {

                // limititation: we take the first found Kinect.
                // todo: let the user choose?
                nui_ = Runtime.Kinects[0];

                nui_.Initialize(RuntimeOptions.UseSkeletalTracking
                                | (enableColor ? RuntimeOptions.UseColor : 0)
                                | (enableDepth ? RuntimeOptions.UseDepthAndPlayerIndex : 0));

            }
            catch (Exception)
            {
                throw new InvalidOperationException("There is no connected NUI device.");
            }

            dtw_ = new DtwGestureRecognizer(VectorDimension, JointsTracked, SequenceSimilarityThreshold, FinalPositionThreshold, MaxSlope, MinGestureLength);
            RemoveAllFramesSkeletonTimeSequence();

            if (gestures != null)
            {
                foreach (var gesture in gestures.Gestures)
                {
                    dtw_.AddOrUpdate(gesture);
                }
            }

            nui_.SkeletonFrameReady += SkeletonExtractSkeletonFrameReady;
            sde_.Skeleton3DDataCoordReady += NuiSkeleton3DDataCoordReady;
            
        }

        public void StartCapture()
        {
            capturing_ = true;
            RemoveAllFramesSkeletonTimeSequence();
        }

        public List<JointRecord> GetCapture()
        {
            List<JointRecord> recordList = skeletonTimeSequence_.Select((x) => new JointRecord() { Id = x.Key, Positions = x.Value.ToList() }).ToList();
            return recordList;
        }

        public void AddOrUpdateGesture(Gesture gesture)
        {
            dtw_.AddOrUpdate(gesture);
        }

        public void RemoveGesture(Gesture gesture)
        {
            dtw_.Delete(gesture);
        }

        public void ClearGestures()
        {
            dtw_.Clear();
        }

        private void SkeletonExtractSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame skeletonFrame = e.SkeletonFrame;
            HasSkeleton = false;
            foreach (SkeletonData data in skeletonFrame.Skeletons)
            {
                if (data.TrackingState != SkeletonTrackingState.Tracked)
                    continue;
                sde_.ProcessData(data);
                HasSkeleton = true;
            }
        }

        private void NuiSkeleton3DDataCoordReady(object sender, Skeleton3DDataCoordEventArgs a)
        {
            
            //Assume that capture equal number of frames for all joints! 
            var iterator = skeletonTimeSequence_.GetEnumerator();
            //Must MoveNext to get first element.
            iterator.MoveNext();
            int numberOfFramesCaptured = iterator.Current.Value.Count;
            //currentBufferFrame.Text = numberOfFramesCaptured.ToString();
            
            if (FrameStart != null)
                FrameStart(this, new NuiEventArgs(null, numberOfFramesCaptured));

            // We need a sensible number of frames before we start attempting to match gestures against remembered sequences
            if (numberOfFramesCaptured > MinimumFrames && !capturing_)
            {
                Gesture s = dtw_.Recognize(skeletonTimeSequence_);
                
                if (s != null)
                {
                    if (GestureDone != null)
                        GestureDone(this, new NuiEventArgs(s, numberOfFramesCaptured));
                    RemoveAllFramesSkeletonTimeSequence();
                    numberOfFramesCaptured = 0;
                }
                else
                {
                    /*
                    if (!nogesture_ && NoGestureDone != null)
                        NoGestureDone(this, new EventArgs());
                    nogesture_ = true;*
                     */
                }
            }
            //Get current number of frames again. Hacky. 
            //iterator = _skeletonTimeSequence.GetEnumerator();
            //iterator.MoveNext();
            //numberOfFramesCaptured = iterator.Current.Value.Count;
            //nof_ = numberOfFramesCaptured;

            if (capturing_ && CaptureProgress != null)
            {
                CaptureProgress(this, new NuiEventArgs(null, numberOfFramesCaptured));
            }

            // Ensures that we remember only the last x frames
            if (numberOfFramesCaptured >= BufferSize)
            {
                // If we are currently capturing and we reach the maximum buffer size then automatically store
                if (capturing_)
                {
                    // FIXED: no capturing mode for Kynapsee
                    //DtwStoreClick(null, null);
                    if (CaptureEnd != null)
                        CaptureEnd(this, new NuiEventArgs(null, numberOfFramesCaptured));
                    capturing_ = false;
                    RemoveAllFramesSkeletonTimeSequence();
                }
                else
                {
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
                flipFlop_ = (flipFlop_ + 1) % Ignore;
                if (flipFlop_ == 0)
                {
                    AppendToSkeletonTimeSequence(snapShot);
                }
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
                skeletonTimeSequence_[joint].Add(skeletonSnapshot[joint]);

            }
        }

        /// <summary>
        /// Remove the first frame of the skeleton time sequence
        /// </summary>
        private void RemoveFirstFrameSkeletonTimeSequence()
        {
            foreach (JointID joint in skeletonTimeSequence_.Keys)
            {
                skeletonTimeSequence_[joint].RemoveAt(0);
            }
        }

        /// <summary>
        /// Remove all frames of the skeleton time sequence
        /// </summary>
        private void RemoveAllFramesSkeletonTimeSequence()
        {
            foreach (JointID joint in skeletonTimeSequence_.Keys)
            {
                skeletonTimeSequence_[joint].Clear();
            }

        }

        /// <summary>
        /// Releases resources and NUI device.
        /// </summary>
        public void Dispose()
        {
            // Needed because the nui device object is never recreated
            nui_.SkeletonFrameReady -= SkeletonExtractSkeletonFrameReady;
            nui_.Uninitialize();
            nui_ = null;
        }

        #endregion

        
    }
}
