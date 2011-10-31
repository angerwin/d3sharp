using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mooege.Core.GS.Map;
using Mooege.Core.GS.Common.Types.Math;
using Mooege.Common.MPQ.FileFormats.Types;
using Mooege.Net.GS.Message;
using bnet.protocol.game_master;
using Mooege.Net.GS.Message.Definitions.World;

namespace Mooege.Core.GS.Actors.Implementations
{
    [HandledSNO(4580)]
    public class Leah : InteractiveNPC
    {
        public Leah(World world, int actorSNO, Vector3D position, Dictionary<int, TagMapEntry> tags)
            : base(world, actorSNO, position, tags)
        {
            this.Attributes[GameAttribute.MinimapActive] = true;
        }        
    }
}
