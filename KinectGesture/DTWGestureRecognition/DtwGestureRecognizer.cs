//-----------------------------------------------------------------------
// <copyright file="DtwGestureRecognizer.cs" company="Chin Xiang Chong">
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

using System.Diagnostics;

namespace DTWGestureRecognition
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Research.Kinect.Nui;
    using Microsoft.Xna.Framework;

    /// <summary>
    /// Dynamic Time Warping nearest neighbour sequence comparison class.
    /// Called 'Gesture Recognizer' but really it can work with any vectors
    /// </summary>
    public class DtwGestureRecognizer
    {

        /// <summary>
        /// Number of dimensions we care about for skeletal data. e.g. if 2, we only care about X and Y coordinates, etc. 
        /// /// </summary>
        private readonly int _dimension;

        /// <summary>
        /// Number of joints we care about. By default set to 6: Left Hand, Wrist, Elbow, and same for right. Left and Right shoulder used to centre the data points. 
        /// /// </summary>
        private readonly int _noOfJoints;

        /// <summary>
        /// Number of data points we care about. Is equal to _dimension * _noOfJoints. 
        /// /// </summary>
        private readonly int _dataSetSize;

        /// <summary>
        /// Maximum distance between the last observations of each sequence.
        /// </summary>
        private readonly double _positionThreshold;

        /// <summary>
        /// Minimum length of a gesture before it can be recognised
        /// </summary>
        private readonly int _minimumLength;

        /// <summary>
        /// Maximum DTW distance between an example and a sequence being classified.
        /// </summary>
        private readonly double _recognitionThreshold;

        /// <summary>
        /// Maximum vertical or horizontal steps in a row.
        /// </summary>
        private readonly int _maxSlope;

        /// <summary>
        /// 2 level Map between name of gesture, and a map of the joints invovled in that gesture, and the position of those joints at different times.
        /// </summary>
        private Dictionary<String, Dictionary<JointID, List<Vector4>>> _recordedGestures;

        /// <summary>
        /// Initializes a new instance of the DtwGestureRecognizer class
        /// First DTW constructor
        /// </summary>
        /// <param name="dim">Dimension</param>
        /// <param name="noJoints">number of joints the recognizer must keep track of</param>
        /// /// <param name="recogThreshold">Threshold over which current gestured being captured by the Kinect can be recognised as a recorded gesture</param>
        /// <param name="posThreshold">Maximum distance between the last frame currently captured by the Kinect and the last frame of all gestures recorded</param>
        /// <param name="minLen">Minimum length a gesture needs to be before being recognised</param>
        public DtwGestureRecognizer(int dim, int noJoints, double recogThreshold, double posThreshold, int minLen)
        {
            _dimension = dim;
            _noOfJoints = noJoints;
            _dataSetSize = _dimension * _noOfJoints;
            _recordedGestures = new Dictionary<string, Dictionary<JointID, List<Vector4>>>();
            _recognitionThreshold = recogThreshold;
            _positionThreshold = posThreshold;
            _maxSlope = int.MaxValue;
            _minimumLength = minLen;
        }

        /// <summary>
        /// Initializes a new instance of the DtwGestureRecognizer class
        /// Second DTW constructor
        /// </summary>
        /// <param name="dim">Dimension</param>
        /// <param name="noJoints">number of joints the recognizer must keep track of</param>
        /// /// <param name="recogThreshold">Threshold over which current gestured being captured by the Kinect can be recognised as a recorded gesture</param>
        /// <param name="posThreshold">Maximum distance between the last frame currently captured by the Kinect and the last frame of all gestures recorded</param>
        /// <param name="ms">Maximum vertical or horizontal steps in a row</param>
        /// <param name="minLen">Minimum length a gesture needs to be before being recognised</param>
        public DtwGestureRecognizer(int dim, int noJoints, double recogThreshold, double posThreshold, int ms, int minLen)
        {
            _dimension = dim;
            _noOfJoints = noJoints;
            _dataSetSize = _dimension * _noOfJoints;
            _recordedGestures = new Dictionary<string, Dictionary<JointID, List<Vector4>>>();
            _recognitionThreshold = recogThreshold;
            _positionThreshold = posThreshold;
            _maxSlope = ms;
            _minimumLength = minLen;
        }

        /// <summary>
        /// Add a seqence with a label to the known sequences library.
        /// The gesture MUST start on the first observation of the sequence and end on the last one.
        /// Sequences may have different lengths.
        /// </summary>
        /// <param name="skeletnTimeSequence">The sequence</param>
        /// <param name="lab">Sequence name</param>
        public void AddOrUpdate(Dictionary<JointID, List<Vector4>> skeletonTimeSequence, string lab)
        {
            _recordedGestures.Remove(lab);
            var comparer = EnumComparer<JointID>.Instance;
            //Deep copy the time Sequence
            Dictionary<JointID, List<Vector4>> seqCopy = new Dictionary<JointID, List<Vector4>>(comparer);
            foreach (JointID joint in skeletonTimeSequence.Keys)
            {
                List<Vector4> newList = new List<Vector4>();
                foreach (Vector4 v in skeletonTimeSequence[joint])
                {
                    Vector4 vClone = new Vector4(v.X, v.Y, v.Z, v.W);
                    newList.Add(vClone);
                }
                seqCopy.Add(joint, newList);
            }
            _recordedGestures.Add(lab, seqCopy);
        }

        /// <summary>
        /// Recognize gesture in the given sequence.
        /// It will always assume that the gesture ends on the last observation of that sequence.
        /// If the distance between the last observations of each sequence is too great, or if the overall DTW distance between the two sequence is too great, no gesture will be recognized.
        /// </summary>
        /// <param name="inputSequence">The sequence to recognise</param>
        /// <returns>The recognised gesture name</returns>
        public string Recognize(Dictionary<JointID, List<Vector4>> inputSequence)
        {

            double minDist = double.PositiveInfinity;
            string classification = "__UNKNOWN";

            foreach (string gestureLabel in _recordedGestures.Keys)
            {
                foreach (JointID joint in _recordedGestures[gestureLabel].Keys)
                {

                    int lastInputPosition = inputSequence[joint].Count - 1;
                    int lastRecordedPosition = _recordedGestures[gestureLabel][joint].Count - 1;


                    if (CalculateSnapshotPositionDistance(inputSequence, lastInputPosition, _recordedGestures[gestureLabel], lastRecordedPosition) < _positionThreshold)
                    {
                        //We've met the positionThreshold requirement, now perform DTW recognition
                        double d = Dtw(inputSequence, _recordedGestures[gestureLabel]) / _recordedGestures[gestureLabel][joint].Count;
                        if (d < minDist)
                        {
                            //Mark the gesture this is most simiilar to. 
                            minDist = d;
                            classification = gestureLabel;
                        }

                    }
                }

            }
            //Recognize it as a gesture as long as it hits the threshold. Right now this means all gestures that hit the threshold
            //are recognised: we probably want to eventually return only the gesture that is most similar. 
            return (minDist < _recognitionThreshold ? classification : "__UNKNOWN") + " " /*+minDist.ToString()*/;
        }

        /// <summary>
        /// Retrieves a text represeantation of the _label and its associated _sequence
        /// For use in dispaying debug information and for saving to file
        /// </summary>
        /// <returns>A string containing all recorded gestures and their names</returns>
        public string RetrieveText()
        {

            StringBuilder retStrBuilder = new StringBuilder();
            string newLine = "\r\n";
            if (_recordedGestures != null)
            {
                foreach (string gestureName in _recordedGestures.Keys)
                {
                    retStrBuilder.Append("@");
                    retStrBuilder.Append(newLine);
                    retStrBuilder.Append(gestureName);
                    retStrBuilder.Append(newLine);
                    foreach (JointID jointName in _recordedGestures[gestureName].Keys)
                    {
                        //count skeletonData
                        if (_recordedGestures[gestureName][jointName].Count < 30)
                        {
                            throw new Exception();
                        }
                        retStrBuilder.Append("+");
                        retStrBuilder.Append(newLine);
                        retStrBuilder.Append(jointName.ToString());
                        retStrBuilder.Append(newLine);
                        foreach (Vector4 pos in _recordedGestures[gestureName][jointName])
                        {
                            retStrBuilder.Append(pos.X.ToString());
                            retStrBuilder.Append(newLine);
                            retStrBuilder.Append(pos.Y.ToString());
                            retStrBuilder.Append(newLine);
                            if (_dimension >= 3)
                            {
                                retStrBuilder.Append(pos.Z.ToString());
                                retStrBuilder.Append(newLine);
                            }

                            if (_dimension >= 4)
                            {
                                retStrBuilder.Append(pos.W.ToString());
                                retStrBuilder.Append(newLine);
                            }
                            retStrBuilder.Append("~");
                            retStrBuilder.Append(newLine);
                        }
                    }
                    retStrBuilder.Append("---");
                    retStrBuilder.Append(newLine);
                }
            }
            else
            {
                throw new Exception();
            }
            return retStrBuilder.ToString();
        }

        /// <summary>
        /// Compute the min DTW distance between the inputSequence and all possible endings of recorded gestures.
        /// </summary>
        /// <param name="inputSequence">The input gesture</param>
        /// <param name="recordedGesture">Gestures we want to recognize against</param>
        /// <returns>a double indicating level of similarity with closest recorded gesture</returns>        
        public double Dtw(Dictionary<JointID, List<Vector4>> inputSequence, Dictionary<JointID, List<Vector4>> recordedGesture)
        {
            //Make assumption that all lists are same length! 
            var inputSeqIterator = inputSequence.GetEnumerator();
            inputSeqIterator.MoveNext();
            int inputLength = inputSeqIterator.Current.Value.Count;

            //Make assumption that all lists are same length! 
            var recordedGestureSeqIterator = recordedGesture.GetEnumerator();
            recordedGestureSeqIterator.MoveNext();
            int recordLength = recordedGestureSeqIterator.Current.Value.Count;

            //Book keeping, setting up and initialization.
            var tab = new double[inputLength + 1, recordLength + 1];
            var horizStepsMoved = new int[inputLength + 1, recordLength + 1];
            var vertStepsMoved = new int[inputLength + 1, recordLength + 1];

            for (int i = 0; i < inputLength + 1; ++i)
            {
                for (int j = 0; j < recordLength + 1; ++j)
                {
                    tab[i, j] = double.PositiveInfinity;
                    horizStepsMoved[i, j] = 0;
                    vertStepsMoved[i, j] = 0;
                }
            }

            tab[inputLength, recordLength] = 0;

            //Actually do the DTW algo. Read
            //http://web.science.mq.edu.au/~cassidy/comp449/html/ch11s02.html
            //For a great summary as to what it does. 
            for (int i = inputLength - 1; i > -1; --i)
            {
                for (int j = recordLength - 1; j > -1; --j)
                {
                    if (tab[i, j + 1] < tab[i + 1, j + 1] && tab[i, j + 1] < tab[i + 1, j] &&
                        horizStepsMoved[i, j + 1] < _maxSlope)
                    {
                        //Move right, move left on reverse
                        tab[i, j] = CalculateSnapshotPositionDistance(inputSequence, i, recordedGesture, j) + tab[i, j + 1];
                        horizStepsMoved[i, j] = horizStepsMoved[i, j + 1] + 1;
                        vertStepsMoved[i, j] = vertStepsMoved[i, j + 1];

                    }

                    else if (tab[i + 1, j] < tab[i + 1, j + 1] && tab[i + 1, j] < tab[i, j + 1] &&
                             vertStepsMoved[i + 1, j] < _maxSlope)
                    {
                        //Move down, move up on reverse
                        tab[i, j] = CalculateSnapshotPositionDistance(inputSequence, i, recordedGesture, j) + tab[i + 1, j];
                        horizStepsMoved[i, j] = horizStepsMoved[i + 1, j];
                        vertStepsMoved[i, j] = vertStepsMoved[i + 1, j] + 1;
                    }

                    else
                    {
                        //Move diagonally down-right
                        if (tab[i + 1, j + 1] == double.PositiveInfinity)
                        {
                            tab[i, j] = double.PositiveInfinity;
                        }
                        else
                        {
                            tab[i, j] = CalculateSnapshotPositionDistance(inputSequence, i, recordedGesture, j) + tab[i + 1, j + 1];
                        }

                        horizStepsMoved[i, j] = 0;
                        vertStepsMoved[i, j] = 0;

                    }
                }
            }

            double bestMatch = double.PositiveInfinity;

            for (int i = 0; i < inputLength; ++i)
            {
                if (tab[i, 0] < bestMatch)
                {
                    bestMatch = tab[i, 0];
                }
            }
            return bestMatch;


        }

        /// <summary>
        /// Compute the length between a frame of the inputSequence and a frame of a recorded gesture.
        /// </summary>
        /// <param name="inputSequence">The input gesture</param>
        /// <param name="inputPosition">Which frame we want from the input gesture to compare against the recorded gesture's frame</param> 
        /// <param name="recordedGesture">Gestures we want to recognize against</param>
        /// <param name="recordedPosition">Which frame we want from the recorded gesture to compare against the input gesture's frame</param>
        /// <returns>a double that is the length between the specified frame of the input gesture versus the specified frame of the recorded gesture</returns>        
        private double CalculateSnapshotPositionDistance(Dictionary<JointID, List<Vector4>> inputSequence, int inputPosition, Dictionary<JointID, List<Vector4>> recordedGesture, int recordedPosition)
        {
            double d = 0;
            foreach (JointID joint in recordedGesture.Keys)
            {
                d += (Vector4.DistanceSquared(inputSequence[joint][inputPosition], recordedGesture[joint][recordedPosition]));
            }

            return Math.Sqrt(d);
        }

    }
}