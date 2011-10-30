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

namespace Mooege.Core.GS.Quests
{

    public interface QuestNotifiable
    {
        void OnDeath(Mooege.Core.GS.Actors.Actor actor);

        void OnPositionUpdate(Vector3D position);

        void OnEnterWorld(Mooege.Core.GS.Map.World world);

        void OnInteraction(Player.Player player, Mooege.Core.GS.Actors.Actor actor);

        void OnEvent(int eventSNOId);

        void OnQuestCompleted(int questSNOId);

        void OnEnterScene(Map.Scene scene);
    }

    public interface QuestEngine : QuestNotifiable
    {
        void UpdateQuestStatus(IQuest quest);        

        void InitiateConversation(Player.Player player, Conversation conversation, Mooege.Core.GS.Actors.Actor actor);

        void AddPlayer(Player.Player joinedPlayer);

        void AddQuest(IQuest quest);

        void UpdateQuestStepObjective(IQuestObjective questObjectivImpl);
    }

    public interface IQuest : QuestNotifiable
    {
        Boolean IsActive();

        Boolean IsFailed();

        Boolean IsCompleted();

        void SendQuestInformation(GameClient client);

        void Start(QuestEngine engine);
        
        void Cancel();

        GameMessage CreateQuestUpdateMessage();

        int SNOId();

        List<QuestCompletionStep> GetCompletionSteps();
    }

    public interface IQuestObjective : QuestNotifiable
    {      
        Boolean IsCompleted();

        GameMessage CreateUpdateMessage();
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
        private List<Player.Player> _players;
        private Game.Game _game;
        private MainQuestManager _mainQuestManager;

        public PlayerQuestEngine(Game.Game game)
        {
            this._players = new List<Player.Player>();
            _questList = new List<IQuest>();
            _game = game;
            _mainQuestManager = new MainQuestManager(this);
            LoadQuests();     
        }

        public void AddPlayer(Player.Player player)
        {

            if (!_players.Contains(player))
            {
                _players.Add(player);
                UpdateAllQuests(player);
            }
        }

        public void RemovePlayer(Player.Player player)
        {
            if(_players.Contains(player)){
                _players.Remove(player);
            }
        }

        public void UpdateAllQuests(Player.Player player)
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
                foreach (Player.Player player in _players)
                {
                    player.InGameClient.SendMessage(message, true);
                }
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

        public void OnDeath(Actors.Actor actor)
        {
            foreach (IQuest quest in ActiveQuests)
            {
                quest.OnDeath(actor);
            }
        }

        public void OnPositionUpdate(Vector3D position)
        {
            foreach (IQuest quest in ActiveQuests)
            {
                quest.OnPositionUpdate(position);
            }
        }

        public void OnEnterWorld(Map.World world)
        {
            foreach (IQuest quest in ActiveQuests)
            {
                quest.OnEnterWorld(world);
            }
        }

        public void OnInteraction(Player.Player player, Actors.Actor actor)
        {
            foreach (IQuest quest in ActiveQuests)
            {
                quest.OnInteraction(player, actor);
            }
        }

        public void InitiateConversation(Player.Player player, Conversation conversation, Mooege.Core.GS.Actors.Actor actor)
        {
            // TODO: Dummy implementation
            player.PlayHeroConversation(conversation.Header.SNOId, 0);
        }


        public void OnEvent(int eventSNOId)
        {
            foreach (IQuest quest in ActiveQuests)
            {
                quest.OnEvent(eventSNOId);
            }
        }

        public void OnQuestCompleted(int questSNOId)
        {
            foreach (IQuest quest in ActiveQuests)
            {
                quest.OnQuestCompleted(questSNOId);
            }
            _mainQuestManager.OnQuestCompleted(questSNOId);
        }


        public void OnEnterScene(Map.Scene scene)
        {
            foreach (IQuest quest in ActiveQuests)
            {
                quest.OnEnterScene(scene);
            }
        }


        public void UpdateQuestStepObjective(IQuestObjective questObjectiv)
        {            
            UpdatePlayers(questObjectiv.CreateUpdateMessage());
        }
    }
}