using System.Collections.Generic;
using Microsoft.Research.Kinect.Nui;
using Microsoft.Xna.Framework;

namespace Kynapsee.Nui.Model
{
    /// <summary>
    /// Describe the record of a specified joint.
    /// </summary>
    public class JointRecord
    {
        /// <summary>
        /// Joint ID.
        /// </summary>
        public JointID Id { get; set; }

        /// <summary>
        /// Positions successively recorded for the joint.
        /// </summary>
        public List<Vector4> Positions { get; set; }
    }
}
