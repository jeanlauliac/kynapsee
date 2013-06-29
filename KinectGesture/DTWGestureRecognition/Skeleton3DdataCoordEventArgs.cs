//-----------------------------------------------------------------------
// <copyright file="Skeleton3DDataCoordEventArgs.cs" company="Chin Xiang Chong">
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
    using System.Windows;
    using Microsoft.Xna.Framework;
    using System.Collections.Generic;
    using Microsoft.Research.Kinect.Nui;

    /// <summary>
    /// Takes Kinect SDK Skeletal Frame coordinates and converts them into a format useful to th DTW
    /// </summary>
    public class Skeleton3DDataCoordEventArgs
    {
        
        /// <summary>
        /// Map of joints and their positions
        /// </summary>
        private readonly Dictionary<JointID, Vector4> _skeletonSnapshot;


        /// <summary>
        /// Construct the event
        /// </summary>
        /// <returns>A map of joints versus their positions at a particular frame</returns>
        public Skeleton3DDataCoordEventArgs(Dictionary<JointID, Vector4> skeletonSnapshot) 
        {
            _skeletonSnapshot = skeletonSnapshot;
        }
          

        /// <summary>
        /// Gets the snapshot of the skeleton
        /// </summary>
        /// <returns>A map of joints versus their positions at a particular frame</returns>
        public Dictionary<JointID, Vector4> GetSkeletonSnapshot() 
        {
            return _skeletonSnapshot;
        }
    }
}