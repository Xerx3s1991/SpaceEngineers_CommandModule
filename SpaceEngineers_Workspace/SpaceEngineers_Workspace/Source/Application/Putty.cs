#region prescript

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
    public sealed class PuttyScript : MyGridProgram
    {
        //=======================================================================
        //////////////////////////BEGIN//////////////////////////////////////////
        //=======================================================================

        // ----------------------------------------------------------------------------------------------- Constants

        public const String nameOfMasterPuttyBlock = "Insert Master Name Here";

        // ----------------------------------------------------------------------------------------------- Class Defintions

        // @brief:
        // @field:
        // @memeber:
        public abstract class Putty : MyGridProgram
        {
            IMyTextPanel myPutty;

            public Putty(String args)
            {
                // The constructor, called only once every session and
                // always before any other method is called. Use it to
                // initialize your script.
                myPutty = (IMyTextPanel)GridTerminalSystem.GetBlockWithName(args);
                // Check if wanted Blocks are in Grid
                if (null == myPutty)
                {
                    throw new ArgumentException("You must supply Putty Block");
                }
            }
        }

        // @brief:
        // @field:
        // @memeber:
        public class Master : Putty {

            public Master(String args) : base(args) {

            }
            /*SystemCmd[] syscmds = {
            new SystemCmd(constructor goes here);
            } */



            public void Main(string[] args) {
                // The main entry point of the script, invoked every time
                // one of the programmable block's Run actions are invoked.

                // The method itself is required, but the argument above
                // can be removed if not needed.
            }

            public void Save() {
                // Called when the program needs to save its state. Use
                // this method to save your state to the Storage field
                // or some other means.

                // This method is optional and can be removed if not
                // needed.
            }
        }

        //=======================================================================
        //////////////////////////END////////////////////////////////////////////
        //=======================================================================
        #region postscript
    }   // Script Class
}   // Namespace Space Engineers
#endregion postscript