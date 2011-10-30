using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mooege.Common.MPQ;

using Mooege.Core.GS.Common.Types.Math;
using Mooege.Common.Helpers;
using Mooege.Core.GS.Actors;
using Mooege.Core.GS.Map;

namespace Mooege.Core.GS.Events
{

    public interface EventManager
    {
        void StartEvent(String eventName);
    }


    /// <summary>
    /// This Implementation exists just for testing purpose of the QuestEngine.
    /// TODO: Replace this with an proper implementation.
    /// </summary>
    public class DummyEventManager:EventManager
    {

        private Game.Game _game;

        public DummyEventManager(Game.Game game)
        {
            _game = game;
        }

        public void StartEvent(String eventName)
        {
            if (eventName.Equals("GizmoGroup2"))
            {
                Player.Player player = _game.Players.Values.First();                
                SpawnMob(player, 6652 /*Walking Corpse*/, 3);               
            }
            else if (eventName.Equals("InnZombies"))
            {
                Player.Player player = _game.Players.Values.First();
                SpawnMob(player, 6647 /*Voracious Zombie*/, 3);
                
            }
            else if (eventName.Equals("FemaleZombieKilled"))
            {
                Player.Player player = _game.Players.Values.First();
                SpawnQuestMob(eventName, player, 108444 /*Wretched Mother*/);               
            }
            else if (eventName.Equals("WretchedQueenIsDead"))
            {
                Player.Player player = _game.Players.Values.First();
                SpawnQuestMob(eventName, player, 176889 /*ZombieFemale_Unique_WretchedQueen*/);
                return;
            }
            else
            {
                // Unkown event.. 
                _game.QuestEngine.OnEvent(eventName);
            }
        }

        private class QuestMob : Monster
        {
            private String _deathEventName;
            private Game.Game _game;

            public QuestMob(String deathEventName, World world, int actorSNO, Vector3D position)
                : base(world, actorSNO, position)
            {
                this._deathEventName = deathEventName;
                this._game = world.Game;
            }

            public override void Die(Mooege.Core.GS.Player.Player player) 
            {
                base.Die(player);
                _game.QuestEngine.OnEvent(_deathEventName);
            }

        }


        private void SpawnMob(Player.Player player, int mobSNO, int amount) 
        {
            for (int i = 0; i < amount; i++)
            {
                var monster = new Monster(player.World, mobSNO, GetRandomPosition(player));
                player.World.Enter(monster);
            }
        }
        private void SpawnQuestMob(String eventName, Player.Player player, int mobSNO) 
        {                                       
            var monster = new QuestMob(eventName, player.World, mobSNO, GetRandomPosition(player));
            player.World.Enter(monster);                        
        }

        private Vector3D GetRandomPosition(Player.Player player)
        {
            return new Vector3D(player.Position.X + (float)RandomHelper.NextDouble() * 20f,
                                            player.Position.Y + (float)RandomHelper.NextDouble() * 20f,
                                            player.Position.Z);
        }
        
    }
}
