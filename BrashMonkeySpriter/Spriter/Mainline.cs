/*==========================================================================
 * Project: BrashMonkeySpriter
 * File: Mainline.cs
 *
 *==========================================================================
 * Author:
 * Geoff "NowSayPillow" Lodder
 *==========================================================================*/

using System;
using System.Collections.Generic;

namespace BrashMonkeySpriter.Spriter {
    public struct MainlineKey {
        public Int64 Time;

        public List<Reference> Body;
        public List<Reference> Bones;
    }
}
