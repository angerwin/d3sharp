using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mooege.Net.GS.Message.Definitions.World;
using Mooege.Core.GS.Map;
using Mooege.Net.GS.Message.Fields;
using Mooege.Common.MPQ.FileFormats;
using Mooege.Common.MPQ;
using Mooege.Common.Helpers;
using Mooege.Net.GS.Message;
using Mooege.Net.GS.Message.Definitions.Conversation;
using Mooege.Core.GS.Common.Types.Math;

namespace Mooege.Core.GS.Actors
{
    public class NPC : Actor
    {
        public override ActorType ActorType { get { return ActorType.NPC; } }
        private Dictionary<int,Conversation> _conversations;
        private ConversationList _conversationList;

        public NPC(Mooege.Core.GS.Map.World world, int actorSNO, Vector3D position)
            : base(world, world.NewActorID)
        {           

            this.ActorSNO = actorSNO;
            // FIXME: This is hardcoded crap
            this.Field2 = 0x8;
            this.Field3 = 0x0;
            this.Scale = 1.35f;
            this.Position.Set(position);
            this.RotationAmount = (float)(RandomHelper.NextDouble() * 2.0f * Math.PI);
            this.RotationAxis.X = 0f; this.RotationAxis.Y = 0f; this.RotationAxis.Z = 1f;
            this.GBHandle.Type = (int)GBHandleType.Monster; this.GBHandle.GBID = 1;
            this.Field7 = 0x00000001;
            this.Field8 = this.ActorSNO;
            this.Field10 = 0x0;
            this.Field11 = 0x0;
            this.Field12 = 0x0;
            this.Field13 = 0x0;
            
            this.Attributes[GameAttribute.Untargetable] = false;
            this.Attributes[GameAttribute.Uninterruptible] = true;
            this.Attributes[GameAttribute.Buff_Visual_Effect, 1048575] = true;
            this.Attributes[GameAttribute.Buff_Icon_Count0, 30582] = 1;
            this.Attributes[GameAttribute.Buff_Icon_Count0, 30286] = 1;
            this.Attributes[GameAttribute.Buff_Icon_Count0, 30285] = 1;
            this.Attributes[GameAttribute.Buff_Icon_Count0, 30284] = 1;
            this.Attributes[GameAttribute.Buff_Icon_Count0, 30283] = 1;
            this.Attributes[GameAttribute.Buff_Icon_Count0, 30290] = 1;
            this.Attributes[GameAttribute.Buff_Icon_Count0, 79486] = 1;
            this.Attributes[GameAttribute.Buff_Active, 30286] = true;
            this.Attributes[GameAttribute.Buff_Active, 30285] = true;
            this.Attributes[GameAttribute.Buff_Active, 30284] = true;
            this.Attributes[GameAttribute.Buff_Active, 30283] = true;
            this.Attributes[GameAttribute.Buff_Active, 30290] = true;

            this.Attributes[GameAttribute.Hitpoints_Max_Total] = 4.546875f;
            this.Attributes[GameAttribute.Buff_Active, 79486] = true;
            this.Attributes[GameAttribute.Hitpoints_Max] = 4.546875f;
            this.Attributes[GameAttribute.Hitpoints_Total_From_Level] = 0f;
            this.Attributes[GameAttribute.Hitpoints_Cur] = 4.546875f;
            this.Attributes[GameAttribute.Invulnerable] = true;
            this.Attributes[GameAttribute.Buff_Active, 30582] = true;
            this.Attributes[GameAttribute.TeamID] = 10;
            this.Attributes[GameAttribute.Level] = 1;
            this.Attributes[GameAttribute.Experience_Granted] = 125;
           
            world.Enter(this);
        }

        public override void OnTargeted(Mooege.Core.GS.Player.Player player, TargetMessage message)
        {
            StartConversation(player);
        }
       
    }
}
