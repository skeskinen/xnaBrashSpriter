/*==========================================================================
 * Project: BrashMonkeySpriter
 * File: Timeline.cs
 *
 *==========================================================================
 * Author:
 * Geoff "NowSayPillow" Lodder
 *==========================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

namespace BrashMonkeySpriter.Spriter {
    public class TimelineList : List<Timeline> {
        public Timeline this[string name] {
            get { return this.FirstOrDefault(x => x.Name == name); }
        }
    }

    public class Timeline {
        public String Name;
        public List<TimelineKey> Keys;
        public int KeyAtOrBefore(int p_elapsedTime)
        {
            // Binary search correct key
            int lo = 0, hi = Keys.Count - 1;
            while ( hi - lo > 1)
            {
                int m = (hi + lo) / 2;
                if (Keys[m].Time > p_elapsedTime) hi = m - 1;
                else lo = m;
            }
            if (Keys[hi].Time < p_elapsedTime)
                return hi;
            
            return lo;
            
        }
    }

    public struct TimelineKey {
        public TimelineType Type;
        public SpinDirection Spin;
        public Int32 Time;

        public Int32 Folder;
        public Int32 File;
        public Vector2 Location;
        public Vector2 Pivot;
        public float Rotation;
        public Vector2 Scale;
        public float Alpha;
    }
}
