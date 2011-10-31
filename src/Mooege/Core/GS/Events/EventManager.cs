using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mooege.Common.MPQ;

using Mooege.Core.GS.Common.Types.Math;
using Mooege.Common.Helpers;
using Mooege.Core.GS.Actors;
using Mooege.Core.GS.Map;
using Mooege.Core.GS.Players;
using Mooege.Core.GS.Games;
using Mooege.Common.MPQ.FileFormats.Types;

namespace Mooege.Core.GS.Events
{

    public interface EventManager
    {
        void StartEvent(String eventName);

        void SpawnMob(Player player, int mobSNO, int amount);
    }


    /// <summary>
    /// This Implementation exists just for testing purpose of the QuestEngine.
    /// TODO: Replace this with an proper implementation.
    /// </summary>
    public class DummyEventManager:EventManager
    {

        private Game _game;        
        public DummyEventManager(Game game)
        {
            _game = game;           
        }

        public void StartEvent(String eventName)
        {
            if (eventName.Equals("GizmoGroup2"))
            {
                Player player = _game.Players.Values.First();                
                SpawnMobGroup(player, 6652 /*Walking Corpse*/, 3, eventName);               
            }
            else if (eventName.Equals("InnZombies"))
            {
                Player player = _game.Players.Values.First();
                SpawnMobGroup(player, 6647 /*Voracious Zombie*/, 3, eventName);
                
            }
            else if (eventName.Equals("FemaleZombieKilled"))
            {
                QuestMobObserver observer = new QuestMobObserver(_game, eventName);
                List<Actor> mobs = FindMobs(108444 /*Wretched Mother*/);
                foreach (Actor mob in mobs)
                {
                    if (mob is Living)
                    {
                        ((Living)mob).Subscribe(observer);
                    }
                }                
            }   
                
            else if (eventName.Equals("WretchedQueenIsDead"))
            {
                // Find Questmob actor and observe it for its death
                QuestMobObserver observer = new QuestMobObserver(_game, eventName);                
                List<Actor> mobs = FindMobs(176889);
                foreach (Actor mob in mobs)
                {
                    if (mob is Living)
                    {
                        ((Living)mob).Subscribe(observer);
                    }
                }                
            }    
            else
            {                
                // Unkown event.. 
                _game.QuestEngine.OnEvent(eventName);
            }
        }

        private List<Actor> FindMobs(int SNOId)
        {
            Player player = _game.Players.Values.First();
            return player.World.Actors.Values.Where(actor => actor.SNOId == SNOId).ToList<Actor>();            
        }

        private class QuestMob : Monster
        {
            private String _deathEventName;
            private Game _game;

            public QuestMob(String deathEventName, World world, int actorSNO, Vector3D position, Dictionary<int, TagMapEntry> tags)
                : base(world, actorSNO, position, tags)
            {
                this._deathEventName = deathEventName;
                this._game = world.Game;
            }

            public override void Die(Player player) 
            {
                base.Die(player);
                _game.QuestEngine.OnEvent(_deathEventName);
            }
        }


        public void SpawnMob(Player player, int mobSNO, int amount)
        {            
            for (int i = 0; i < amount; i++)
            {
                var monster = new Monster(player.World, mobSNO, GetRandomPosition(player), new Dictionary<int, Mooege.Common.MPQ.FileFormats.Types.TagMapEntry>()) { Scale = 1.35f }; ;                
                player.World.Enter(monster);
            }
        }

        
        private void SpawnMobGroup(Player player, int mobSNO, int amount, String groupName) 
        {
            MobGroupObserver groupObserver = new MobGroupObserver(_game, amount, groupName);
            for (int i = 0; i < amount; i++)
            {
                var monster = new Monster(player.World, mobSNO, GetRandomPosition(player), new Dictionary<int, Mooege.Common.MPQ.FileFormats.Types.TagMapEntry>()) { Scale = 1.35f }; ;
                monster.Subscribe(groupObserver);
                player.World.Enter(monster);
            }
        }

        private void SpawnQuestMob(String eventName, Player player, int mobSNO) 
        {
            var monster = new QuestMob(eventName, player.World, mobSNO, GetRandomPosition(player), new Dictionary<int, TagMapEntry>()) { Scale = 1.35f }; ;
            player.World.Enter(monster);                        
        }

        private Vector3D GetRandomPosition(Player player)
        {
            return new Vector3D(player.Position.X + (float)RandomHelper.NextDouble() * 20f,
                                            player.Position.Y + (float)RandomHelper.NextDouble() * 20f,
                                            player.Position.Z);
        }

        public class QuestMobObserver : IObserver<Actor>
        {

            private Game _game;
            private String _eventName;
            public QuestMobObserver(Game game, String eventName)
            {
                this._game = game;
                this._eventName = eventName;
               
            }

            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {                
            }

            public void OnNext(Actor actor)
            {
                _game.QuestEngine.OnEvent(_eventName);                
            }
        }


        public class MobGroupObserver : IObserver<Actor>
        {

            private Game _game;
            private int _counter;
            private String _mobGroupName;


            public MobGroupObserver(Game game, int counter, String _mobGroupName)
            {
                this._game = game;
                this._counter = counter;
                this._mobGroupName = _mobGroupName;

            }

            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
            }

            public void OnNext(Actor actor)
            {
                _counter--;
                if (_counter <= 0)
                {
                    _game.QuestEngine.OnGroupDeath(_mobGroupName);
                }
            }
        }
    }
}
