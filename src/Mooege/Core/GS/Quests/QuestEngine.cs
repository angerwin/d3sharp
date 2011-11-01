using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mooege.Core.GS.Actors;
using Mooege.Net.GS.Message.Fields;
using Mooege.Core.GS.Map;
using Mooege.Net.GS;
using Mooege.Common.MPQ;
using Mooege.Common.MPQ.FileFormats;
using System.Diagnostics;
using Mooege.Common;
using Mooege.Net.GS.Message.Definitions.Quest;
using Mooege.Net.GS.Message;
using Mooege.Net.GS.Message.Definitions.Conversation;
using Mooege.Common.Helpers;
using Mooege.Core.GS.Common.Types.Math;
using Mooege.Core.GS.Common.Types.SNO;
using Mooege.Core.GS.Players;
using Mooege.Core.GS.Games;

namespace Mooege.Core.GS.Quests
{

    public interface QuestNotifiable
    {
        void OnDeath(Mooege.Core.GS.Actors.Actor actor);        

        void OnEnterWorld(Mooege.Core.GS.Map.World world);

        void OnInteraction(Player player, Mooege.Core.GS.Actors.Actor actor);

        void OnEvent(String eventName);

        void OnQuestCompleted(int questSNOId);

        void OnEnterScene(Map.Scene scene);

        void OnGroupDeath(string mobGroupName);
    }

    public interface QuestEngine : QuestNotifiable
    {
        void UpdateQuestStatus(IQuest quest);        
        
        void AddPlayer(Player joinedPlayer);

        void AddQuest(IQuest quest);

        void Register(IQuestObjective objective);

        void Unregister(IQuestObjective objective);

        void TriggerConversation(Player player, Conversation conversation, Mooege.Core.GS.Actors.Actor actor);

        void TriggerQuestEvent(String eventName);

        void TriggerMobSpawn(int SNOId, int amount);

        void TriggerConversationSymbol(int p);

        void UpdateQuestObjective(QuestObjectivImpl questObjectivImpl);
    }

    public interface IQuest
    {
        Boolean IsActive();

        Boolean IsFailed();

        Boolean IsCompleted();
        
        void Start(QuestEngine engine);
        
        void Cancel();

        GameMessage CreateQuestUpdateMessage();

        int SNOId();

        List<QuestCompletionStep> GetCompletionSteps();

        void ObjectiveComplete(IQuestObjective questObjectivImpl);
    }

    public interface IQuestObjective : QuestNotifiable
    {      
        Boolean IsCompleted();

        GameMessage CreateUpdateMessage();

        QuestStepObjectiveType GetQuestObjectiveType();

        void Cancel();
    }

    public class MainQuestManager
    {
        private QuestEngine _engine;
        private MPQQuest activeMainQuest;

        private List<int> _mainQuestList;
        private IEnumerator<int> _questListEnumerator;
        public MainQuestManager(QuestEngine engine)
        {
            _engine = engine;

            _mainQuestList = new List<int>();
            _mainQuestList.Add(87700); // ProtectorOfTristram.qst
            _mainQuestList.Add(72095); // RescueCain.qst
            _mainQuestList.Add(72221); // Blacksmith.qst
            _mainQuestList.Add(72738); // Nephalem_Power.qst
            _mainQuestList.Add(72061); // King Leoric

            _questListEnumerator = _mainQuestList.GetEnumerator();

        }

        public void LoadNextMainQuest()
        {
            _questListEnumerator.MoveNext();
            Quest questData = (Quest)(MPQStorage.Data.Assets[SNOGroup.Quest][_questListEnumerator.Current].Data);
            activeMainQuest = new MPQQuest(questData);
            _engine.AddQuest(activeMainQuest);            
        }



        internal void OnQuestCompleted(int questSNOId)
        {
            if (questSNOId == activeMainQuest.SNOId())
            {
                LoadNextMainQuest();
            }
        }
    }

    public class PlayerQuestEngine : QuestEngine
    {

        private static readonly Logger Logger = LogManager.CreateLogger();
        
        private List<IQuest> _questList;
        private Player _player;
        private Game _game;
        private MainQuestManager _mainQuestManager;

        private Dictionary<QuestStepObjectiveType, List<IQuestObjective>> _activeObjectives;

        public PlayerQuestEngine(Game game)
        {
            _questList = new List<IQuest>();
            _activeObjectives = new Dictionary<QuestStepObjectiveType, List<IQuestObjective>>();            
            _game = game;
            _mainQuestManager = new MainQuestManager(this);
            LoadQuests();     
        }

        /// <summary>
        /// Register QuestObjective. QuestObjective will be notified of Questrelevant Events
        /// </summary>
        public void Register(IQuestObjective objective)
        {
            QuestStepObjectiveType type = objective.GetQuestObjectiveType();

            List<IQuestObjective> objectiveList;
            if (_activeObjectives.ContainsKey(type))
            {
                objectiveList = _activeObjectives[type];
            }
            else
            {
                objectiveList = new List<IQuestObjective>();
                _activeObjectives.Add(type, objectiveList);
            }

            objectiveList.Add(objective);
        }

        /// <summary>
        /// Unregister QuestObjective. QuestObjective will no longer be notified of Questrelevant Events
        /// </summary>
        /// <param name="objective"></param>
        public void Unregister(IQuestObjective objective)
        {
            QuestStepObjectiveType type = objective.GetQuestObjectiveType();
            List<IQuestObjective> objectiveList = null;
            if (_activeObjectives.ContainsKey(type))
            {
                objectiveList = _activeObjectives[type];
            }            
            
            if (objectiveList != null && objectiveList.Contains(objective))
            {
                objectiveList.Remove(objective);
            }
        }

        /// <summary>
        /// Register Player to QuestEngine.
        /// </summary>
        /// <param name="player"></param>
        public void AddPlayer(Player player)
        {
            if (_player != null)
                throw new Exception("Just one player allowed in this QuestEngine");
            
            _player = player;
            UpdateAllQuests(player);
            
        }

        public void RemovePlayer(Player player)
        {
            player = null;
        }

        public void UpdateAllQuests(Player player)
        {
            foreach (IQuest quest in ActiveQuests)
            {
                GameMessage message = quest.CreateQuestUpdateMessage();
                player.InGameClient.SendMessage(message, true);
            }
        }

        public void UpdateQuestStatus(IQuest quest)
        {            
            GameMessage message = quest.CreateQuestUpdateMessage();
            UpdatePlayers(message);
        }

        private void UpdatePlayers(GameMessage message)
        {
            if (message != null)
            {                
                _player.InGameClient.SendMessage(message, true);
            }
        }


        public void LoadQuests()
        {
            _mainQuestManager.LoadNextMainQuest();
        }

        public void AddQuest(IQuest quest)
        {
            _questList.Add(quest);
            quest.Start(this);
        }

        private List<IQuest> ActiveQuests
        {
            get { return _questList.Where(quest => quest.IsCompleted() == false && quest.IsFailed() == false).ToList(); }
        }

        public void TriggerConversation(Player player, Conversation conversation, Mooege.Core.GS.Actors.Actor actor)
        {
            // TODO: Trigger Converstation in an correct way
            player.PlayHeroConversation(conversation.Header.SNOId, 0);
            //TriggerConversationSymbol(conversation.Header.SNOId);
        }

        private List<IQuestObjective> GetObjectiveList(QuestStepObjectiveType type)
        {
            List<IQuestObjective> objectivesList = new List<IQuestObjective>();

            if (_activeObjectives.ContainsKey(type) &&
                _activeObjectives[type].Count > 0)
            {
                objectivesList.AddRange(_activeObjectives[type]);                
            }

            return objectivesList;
        }

        public void OnDeath(Actors.Actor actor)
        {
            foreach (IQuestObjective objective in GetObjectiveList(QuestStepObjectiveType.KillMonster))
            {
                objective.OnDeath(actor);
            }                             
        }      

        public void OnEnterWorld(Map.World world)
        {
            foreach (IQuestObjective objective in GetObjectiveList(QuestStepObjectiveType.EnterWorld))
            {
                objective.OnEnterWorld(world);
            }            
        }

        public void OnInteraction(Player player, Actors.Actor actor)
        {
            foreach (IQuestObjective objective in GetObjectiveList(QuestStepObjectiveType.HadConversation))
            {
                objective.OnInteraction(player, actor);
            }

            foreach (IQuestObjective objective in GetObjectiveList(QuestStepObjectiveType.InteractWithActor))
            {
                objective.OnInteraction(player, actor);
            }           
        }

        public void OnEvent(String eventName)
        {           
            foreach (IQuestObjective objective in GetObjectiveList(QuestStepObjectiveType.EventReceived))
            {
                objective.OnEvent(eventName);
            } 
        }

        public void OnQuestCompleted(int questSNOId)
        {
            foreach (IQuestObjective objective in GetObjectiveList(QuestStepObjectiveType.CompleteQuest))
            {
                objective.OnQuestCompleted(questSNOId);
            }
            _mainQuestManager.OnQuestCompleted(questSNOId);
        }


        public void OnEnterScene(Map.Scene scene)
        {
            foreach (IQuestObjective objective in GetObjectiveList(QuestStepObjectiveType.EnterScene))
            {
                objective.OnEnterScene(scene);
            }
        }

        public void TriggerQuestEvent(String eventName)
        {
            _game.EventManager.StartEvent(eventName);
        }


        public void OnGroupDeath(string mobGroupName)
        {
            foreach (IQuestObjective objective in GetObjectiveList(QuestStepObjectiveType.KillGroup))
            {
                objective.OnGroupDeath(mobGroupName);
            }  
        }

        // This Method is just for testiong purpose
        public void TriggerMobSpawn(int SNOId, int amount)
        {            
            _game.EventManager.SpawnMob(_game.Players.First().Value, SNOId, amount);
        }


        public void TriggerConversationSymbol(int snoId)
        {
            if (_game.Players.Count > 0)
            {
                GameClient gc = _game.Players.First().Value.InGameClient;
                if (MPQStorage.Data.Assets[SNOGroup.Conversation].ContainsKey(snoId))
                {
                    Conversation conversation = (Conversation)MPQStorage.Data.Assets[SNOGroup.Conversation][snoId].Data;

                    GameMessage msg = new QuestMeterMessage
                    {
                        snoQuest = conversation.SNOQuest,
                        Field1 = (int)_game.Players.First().Value.World.Actors.Values.Where(a => a.SNOId == 4580).ToList().First().DynamicID,
                        Field2 = 1.35f,
                    };

                    gc.SendMessage(msg, true);                    
                }
            }
        }

        public void UpdateQuestObjective(QuestObjectivImpl questObjectivImpl)
        {
            GameMessage msg = questObjectivImpl.CreateUpdateMessage();
            if (msg != null)
            {
                foreach (var player in _game.Players.Values)
                    player.InGameClient.SendMessage(msg, true);
            }
        }
    }
}