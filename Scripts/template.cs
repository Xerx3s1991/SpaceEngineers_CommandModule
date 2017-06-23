#region pre_script
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game;
namespace Scripts
{
    class ExampleScipt
    {
        IMyGridTerminalSystem GridTerminalSystem;
#endregion pre_script
/* http://steamcommunity.com/sharedfiles/filedetails/?id=360966557 */
void Main()
{
    List<IMyTerminalBlock> blocks  = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyRadioAntenna>(blocks);
    if (blocks.Count == 0) return;
    IMyRadioAntenna antenna = blocks[0] as IMyRadioAntenna;
    antenna.SetCustomName("Hello Galaxy!");
}
#region post_script
    }
}
#endregion post_script
