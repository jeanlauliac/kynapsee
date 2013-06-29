//-----------------------------------------------------------------------
// <copyright file="Skeleton3DDataExtract.cs" company="Chin Xiang Chong">
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
    using System.Windows;
    using System.Collections.Generic;
    using Microsoft.Research.Kinect.Nui;
    using Microsoft.Xna.Framework;


    /// <summary>
    /// This class is used to transform the data of the skeleton
    /// </summary>
    public class Skeleton3DDataExtract
    {
        /// <summary>
        /// Skeleton3DDataCoordEventHandler delegate
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="a">Skeleton 3Ddata Coord Event Args</param>
        public delegate void Skeleton3DDataCoordEventHandler(object sender, Skeleton3DDataCoordEventArgs a);

        /// <summary>
        /// The Skeleton 3Ddata Coord Ready event
        /// </summary>
        public static event Skeleton3DDataCoordEventHandler Skeleton3DDataCoordReady;

        /// <summary>
        /// Crunches Kinect SDK's Skeleton Data and spits out a format more useful for DTW
        /// </summary>
        /// <param name="data">Kinect SDK's Skeleton Data</param>
        public static void ProcessData(SkeletonData data, int jointsTracked)
        {
            // Extract the coordinates of the points.
            var p = new Vector4[jointsTracked];
            Vector4 shoulderRight = new Vector4(), shoulderLeft = new Vector4();
            foreach (Joint j in data.Joints)
            {
                switch (j.ID)
                {
                    case JointID.HandLeft:
                        p[0] = new Vector4(j.Position.X, j.Position.Y, j.Position.Z, j.Position.W);
                        break;
                    case JointID.WristLeft:
                        p[1] = new Vector4(j.Position.X, j.Position.Y, j.Position.Z, j.Position.W);
                        break;
                    case JointID.ElbowLeft:
                        p[2] = new Vector4(j.Position.X, j.Position.Y, j.Position.Z, j.Position.W);
                        break;
                    case JointID.HandRight:
                        p[3] = new Vector4(j.Position.X, j.Position.Y, j.Position.Z, j.Position.W);
                        break;
                    case JointID.WristRight:
                        p[4] = new Vector4(j.Position.X, j.Position.Y, j.Position.Z, j.Position.W);
                        break;
                    case JointID.ElbowRight:
                        p[5] = new Vector4(j.Position.X, j.Position.Y, j.Position.Z, j.Position.W);
                        break;
                    case JointID.ShoulderLeft:
                        shoulderLeft = new Vector4(j.Position.X, j.Position.Y, j.Position.Z, j.Position.W);
                        p[6] = shoulderLeft;
                        break;
                    case JointID.ShoulderRight:
                        shoulderRight = new Vector4(j.Position.X, j.Position.Y, j.Position.Z, j.Position.W);
                        p[7] = shoulderRight;
                        break;
                }
            }

            // Centre the data
            var center = new Vector4( (shoulderLeft.X + shoulderRight.X) / 2, (shoulderLeft.Y + shoulderRight.Y) / 2, 
                                      (shoulderLeft.Z + shoulderRight.Z) / 2, (shoulderLeft.W + shoulderRight.W) / 2);
            
            for (int i = 0; i < jointsTracked -2; i++)
            {
                p[i].X -= center.X;
                p[i].Y -= center.Y;
                p[i].Z -= center.Z;
                p[i].W -= center.W;

            }

            // Normalization of the coordinates
            double shoulderDist =
                Math.Sqrt(Math.Pow((shoulderLeft.X - shoulderRight.X), 2) +
                          Math.Pow((shoulderLeft.Y - shoulderRight.Y), 2) +
                          Math.Pow((shoulderLeft.Z - shoulderRight.Z), 2) +
                          Math.Pow((shoulderLeft.W - shoulderRight.W), 2) );
            //for (int i = 0; i < jointsTracked; i++)
            for (int i = 0; i < jointsTracked - 2; i++)
            {
                p[i] = Vector4.Divide(p[i], (float)shoulderDist);                
            }

            // Now put everything into the dictionary, and send it to the event. 
            Dictionary<JointID, Vector4> _skeletonSnapshot = new Dictionary<JointID, Vector4> 
            {
                {JointID.HandLeft, p[0]},
                {JointID.WristLeft, p[1]},
                {JointID.ElbowLeft, p[2]},
                {JointID.HandRight, p[3]},
                {JointID.WristRight, p[4]},
                {JointID.ElbowRight, p[5]},
                {JointID.ShoulderLeft, p[6]},
                {JointID.ShoulderRight, p[7]},
            };

            // Launch the event!
            Skeleton3DDataCoordReady(null, new Skeleton3DDataCoordEventArgs(_skeletonSnapshot));
        }
    }
}
