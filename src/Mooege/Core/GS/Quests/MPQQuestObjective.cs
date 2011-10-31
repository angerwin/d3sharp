using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mooege.Common.MPQ.FileFormats;
using Mooege.Net.GS.Message.Fields;
using Mooege.Common.MPQ;
using System.Diagnostics;
using Mooege.Common;
using Mooege.Core.GS.Common.Types.SNO;
using Mooege.Core.GS.Common.Types.Math;
using Mooege.Net.GS.Message.Definitions.Quest;
using Mooege.Net.GS.Message;
using Mooege.Core.GS.Players;

namespace Mooege.Core.GS.Quests
{

    public class QuestObjectivImpl : IQuestObjective
    {
        static readonly Logger Logger = LogManager.CreateLogger();
        private QuestStepObjective _objectivData;        
        private IQuest _quest;
        private Boolean _completed;       
        private QuestEngine _engine;
        

        public QuestObjectivImpl(QuestEngine engine, QuestStepObjective objectivData, IQuest quest)
        {
            this._objectivData = objectivData;
            this._completed = false;
            this._engine = engine;
            this._quest = quest;
        }

        public Boolean IsCompleted()
        {
            return _completed;
        }

        public QuestStepObjectiveType GetQuestObjectiveType()
        {
            return _objectivData.ObjectiveType;
        }

        public GameMessage CreateUpdateMessage()
        {
            return null;
        }

        private void Complete()
        {
            _completed = true;
            _quest.ObjectiveComplete(this);
        }

        public void OnDeath(Actors.Actor actor)
        {           
            if (_objectivData.ObjectiveType == QuestStepObjectiveType.KillMonster)
            {
                if (actor.SNOId == _objectivData.SNOName1.SNOId)
                {
                    Complete();                    
                    return;
                }
            }
        }
       
        public void OnPositionUpdate(Vector3D position)
        {

        }

        public void OnEnterWorld(Map.World world)
        {
            if (_objectivData.ObjectiveType == QuestStepObjectiveType.EnterWorld)
            {
                if (_objectivData.SNOName1.SNOId == world.SNOId)
                {
                    Complete();
                    return;
                }
            }
        }

        public void OnInteraction(Player player, Actors.Actor actor)
        {
            if (_objectivData.ObjectiveType == QuestStepObjectiveType.HadConversation)
            {                
                if(MPQStorage.Data.Assets[SNOGroup.Conversation].ContainsKey(_objectivData.SNOName1.SNOId))
                {
                    Conversation conversation = (Conversation)MPQStorage.Data.Assets[SNOGroup.Conversation][_objectivData.SNOName1.SNOId].Data;
                    if (conversation.SNOPrimaryNpc == actor.SNOId)
                    {
                        _engine.TriggerConversation(player, conversation, actor);
                        Complete();
                        return;
                    }
                }
                else
                {
                    Logger.Error("unknown conversation id " + _objectivData.SNOName1.SNOId);
                }
            }

            if (_objectivData.ObjectiveType == QuestStepObjectiveType.InteractWithActor)
            {
                if (_objectivData.SNOName1.SNOId == actor.SNOId)
                {
                   Complete();
                    return;
                }
            }

        }


        public void OnEvent(String eventName)
        {
            if (_objectivData.ObjectiveType == QuestStepObjectiveType.EventReceived)
            {
                if (_objectivData.Unknown1 == eventName)
                {
                    Complete();
                    return;
                }
            }
        }


        public void OnQuestCompleted(int questSNOId)
        {
            if (_objectivData.ObjectiveType == QuestStepObjectiveType.CompleteQuest)
            {
                if (_objectivData.SNOName1.SNOId == questSNOId)
                {
                   Complete();
                    return;
                }
            }
        }


        public void OnEnterScene(Map.Scene scene)
        {
            if (_objectivData.ObjectiveType == QuestStepObjectiveType.EnterScene)
            {
                if (_objectivData.SNOName1.SNOId == scene.SNOId)
                {
                    Complete();
                    return;
                }
            }
        }


        public void Cancel()
        {
            Complete();
        }


        public void OnGroupDeath(string _mobGroupName)
        {
            if (_objectivData.ObjectiveType == QuestStepObjectiveType.KillGroup)
            {
                if (_objectivData.Unknown1.Equals(_mobGroupName))
                {
                    Complete();
                    return;
                }
            }
        }
    }
   


}
