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


namespace Mooege.Core.GS.Quests
{

    public interface QuestNotifiable
    {
        void OnDeath(Mooege.Core.GS.Actors.Actor actor);

        void OnPositionUpdate(Vector3D position);

        void OnEnterWorld(Mooege.Core.GS.Map.World world);

        void OnInteraction(Mooege.Core.GS.Actors.Actor actor);
    }

    public interface QuestEngine : QuestNotifiable
    {
        void UpdateQuestStatus(IQuest quest);

        void LoadQuests();

        void InitiateConversation(Conversation conversation, Mooege.Core.GS.Actors.Actor actor);
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
        
    }

    public class PlayerQuestEngine : QuestEngine
    {

        private static readonly Logger Logger = LogManager.CreateLogger();
        
        private List<IQuest> _questList;
        private Player.Player _player;

        public PlayerQuestEngine(Player.Player player)
        {
            this._player = player;
        }                

        public void UpdateQuestStatus(IQuest quest)
        {
            Debug.Assert(quest != null);
            _player.InGameClient.SendMessage(quest.CreateQuestUpdateMessage(), true);
        }

        public void LoadQuests()
        {
            int key = MPQStorage.Data.Assets[SNOGroup.Quest].Keys.ElementAt(0);
            Quest questData = (Quest)(MPQStorage.Data.Assets[SNOGroup.Quest][key].Data);
            IQuest quest = new MPQQuest(questData);
            _questList = new List<IQuest>();
            _questList.Add(quest);

            quest.Start(this);
        }

        private List<IQuest> ActiveQuests
        {
            get { return _questList.Where(quest => quest.IsActive()).ToList(); }
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

        public void OnInteraction(Actors.Actor actor)
        {
            foreach (IQuest quest in ActiveQuests)
            {
                quest.OnInteraction(actor);
            }
        }

        public void InitiateConversation(Conversation conversation, Mooege.Core.GS.Actors.Actor actor)
        {
            // TODO: Dummy implementation
            _player.PlayHeroConversation(conversation.Header.SNOId, 0);
        }
    }
}