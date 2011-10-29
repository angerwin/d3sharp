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

namespace Mooege.Core.GS.Quests
{

    public class QuestObjectivImpl : IQuestObjective
    {

        static readonly Logger Logger = LogManager.CreateLogger();


        private QuestStepObjective _objectivData;
        private Boolean _completed;
        private QuestEngine _engine;

        public QuestObjectivImpl(QuestEngine engine, QuestStepObjective objectivData)
        {
            this._objectivData = objectivData;
            this._completed = false;
            this._engine = engine;
        }

        public Boolean IsCompleted()
        {
            return _completed;
        }

        public void OnDeath(Actors.Actor actor)
        {
            if (_objectivData.ObjectiveType == QuestStepObjectiveType.KillGroup)
            {
                _completed = true;
                return;
            }

            if (_objectivData.ObjectiveType == QuestStepObjectiveType.KillMonster)
            {
                if (actor.ActorSNO == _objectivData.SNOName1.SNOId)
                {
                    _completed = true;
                    return;
                }
            }
        }

        public void OnPositionUpdate(Vector3D position)
        {

        }

        public void OnEnterWorld(Map.World world)
        {

        }

        public void OnInteraction(Player.Player player, Actors.Actor actor)
        {
            if (_objectivData.ObjectiveType == QuestStepObjectiveType.HadConversation)
            {                
                if(MPQStorage.Data.Assets[SNOGroup.Conversation].ContainsKey(_objectivData.SNOName1.SNOId))
                {
                    Conversation conversation = (Conversation)MPQStorage.Data.Assets[SNOGroup.Conversation][_objectivData.SNOName1.SNOId].Data;
                    if (conversation.SNOPrimaryNpc == actor.ActorSNO)
                    {
                        _engine.InitiateConversation(player, conversation, actor);
                        _completed = true;
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
                if (_objectivData.SNOName1.SNOId == actor.ActorSNO)
                {
                    _completed = true;
                    return;
                }
            }

        }


        public void OnEvent(int eventSNOId)
        {
            if (_objectivData.ObjectiveType == QuestStepObjectiveType.EventReceived)
            {
                if (_objectivData.SNOName1.SNOId == eventSNOId)
                {                    
                    _completed = true;
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
                    _completed = true;
                    return;
                }
            }
        }


        public void OnEnterScene(Map.Scene scene)
        {
            if (_objectivData.ObjectiveType == QuestStepObjectiveType.EnterScene)
            {
                if (_objectivData.SNOName1.SNOId == scene.SceneSNO)
                {
                    _completed = true;
                    return;
                }
            }
        }
    }
   


}
