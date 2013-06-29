using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kynapsee.Nui.Model;

namespace Kynapsee.Nui
{
    public class NuiEventArgs : EventArgs
    {
        public NuiEventArgs(Gesture gesture, int recordedFrameCount)
        {
            Gesture = gesture;
            RecordedFrameCount = recordedFrameCount;
        }

        public Gesture Gesture { get; private set; }
        public int RecordedFrameCount { get; private set; }
    }
}
