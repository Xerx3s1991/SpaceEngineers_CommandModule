﻿#region prescript

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using VRageMath;
using VRage.Game;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Ingame;
using Sandbox.Game.EntityComponents;
using VRage.Game.Components;
using VRage.Collections;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;

#endregion prescript
namespace SpaceEngineers
{
    public sealed class ScriptName : MyGridProgram
    {
        //=======================================================================
        //////////////////////////BEGIN//////////////////////////////////////////
        //=======================================================================

        public ScriptName()
        {
            // The constructor, called only once every session and
            // always before any other method is called. Use it to
            // initialize your script.
        }

        public void Main(string args)
        {
            // The main entry point of the script, invoked every time
            // one of the programmable block's Run actions are invoked.

            // The method itself is required, but the argument above
            // can be removed if not needed.
        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means.

            // This method is optional and can be removed if not
            // needed.
        }

        //=======================================================================
        //////////////////////////END////////////////////////////////////////////
        //=======================================================================
        #region postscript
    }
}
#endregion postscript