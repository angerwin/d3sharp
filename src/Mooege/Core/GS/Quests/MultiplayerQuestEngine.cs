using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mooege.Core.GS.Games;
using Mooege.Core.GS.Players;

namespace Mooege.Core.GS.Quests
{
    class MultiplayerQuestEngine : QuestEngine
    {

        private Game _game;

        private Dictionary<Player, QuestEngine> playerQuestEngines;
        public MultiplayerQuestEngine(Game game)
        {
            this._game = game;
            playerQuestEngines = new Dictionary<Player, QuestEngine>();
        }

        public void UpdateQuestStatus(IQuest quest)
        {
            foreach (QuestEngine playerQuests in playerQuestEngines.Values)
                playerQuests.UpdateQuestStatus(quest);
        }

        public void AddPlayer(Players.Player joinedPlayer)
        {
            playerQuestEngines[joinedPlayer] = new PlayerQuestEngine(_game);
            playerQuestEngines[joinedPlayer].AddPlayer(joinedPlayer);
        }

        public void AddQuest(IQuest quest)
        {
            throw new NotImplementedException("quests cannt be added to MultiplayerQuestEngine.");
        }

        public void Register(IQuestObjective objective)
        {
            throw new NotImplementedException("Objectives cannt be added to MultiplayerQuestEngine.");
        }

        public void Unregister(IQuestObjective objective)
        {
            throw new NotImplementedException("Objectives cannt be removed from MultiplayerQuestEngine.");
        }

        public void TriggerConversation(Players.Player player, Mooege.Common.MPQ.FileFormats.Conversation conversation, Actors.Actor actor)
        {
            throw new NotImplementedException();
        }

        public void TriggerQuestEvent(string eventName)
        {
            throw new NotImplementedException();
        }

        public void TriggerMobSpawn(int SNOId, int amount)
        {
            throw new NotImplementedException();
        }

        public void TriggerConversationSymbol(int p)
        {
            throw new NotImplementedException();
        }

        public void UpdateQuestObjective(QuestObjectivImpl questObjectivImpl)
        {
            throw new NotImplementedException();
        }

        public void OnDeath(Actors.Actor actor)
        {
            foreach (QuestEngine playerQuests in playerQuestEngines.Values)
                playerQuests.OnDeath(actor);
        }

        public void OnEnterWorld(Map.World world)
        {            
            foreach (QuestEngine playerQuests in playerQuestEngines.Values)
                playerQuests.OnEnterWorld(world);
        }

        public void OnInteraction(Players.Player player, Actors.Actor actor)
        {
            foreach (QuestEngine playerQuests in playerQuestEngines.Values)
                playerQuests.OnInteraction(player, actor);
        }

        public void OnEvent(string eventName)
        {
            foreach (QuestEngine playerQuests in playerQuestEngines.Values)
                playerQuests.OnEvent(eventName);
        }

        public void OnQuestCompleted(int questSNOId)
        {
            foreach (QuestEngine playerQuests in playerQuestEngines.Values)
                playerQuests.OnQuestCompleted(questSNOId);
        }

        public void OnEnterScene(Map.Scene scene)
        {
            foreach (QuestEngine playerQuests in playerQuestEngines.Values)
                playerQuests.OnEnterScene(scene);
        }

        public void OnGroupDeath(string mobGroupName)
        {
            foreach (QuestEngine playerQuests in playerQuestEngines.Values)
                playerQuests.OnGroupDeath(mobGroupName);
        }
    }
}
