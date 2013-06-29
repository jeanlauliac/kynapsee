using System;
using System.Collections.Generic;

namespace Kynapsee.Nui.Model
{
    /// <summary>
    /// Describes a NUI gesture.
    /// </summary>
    public class Gesture
    {
        public Gesture()
        {
            Guid = Guid.NewGuid();
        }

        /// <summary>
        /// Gets the gesture name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the gesture unique identifier.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Get a list of records for each joint of the gesture.
        /// A gesture does not need all the different kinds of joints.
        /// </summary>
        public List<JointRecord> JointRecords { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
