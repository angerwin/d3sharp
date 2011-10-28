using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mooege.Common.MPQ.FileFormats;
using Mooege.Net.GS.Message.Fields;
using Mooege.Common.MPQ;

namespace Mooege.Core.GS.Quests
{

    public class QuestObjectivImpl : QuestNotifiable
    {

        private QuestStepObjective _objectivData;
        private Boolean _completed;
        private QuestEngine _engine;

        public QuestObjectivImpl(QuestEngine engine, QuestStepObjective objectivData)
        {
            this._objectivData = objectivData;
            this._completed = false;
            this._engine = engine;
        }

        public Boolean isCompleted()
        {
            return _completed;
        }

        public void OnDeath(Actors.Actor actor)
        {
            if (_objectivData.objectiveType == QuestStepObjectiveType.KillGroup)
            {
                _completed = true;
                return;
            }

            if (_objectivData.objectiveType == QuestStepObjectiveType.KillMonster)
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

        public void OnInteraction(Actors.Actor actor)
        {
            if (_objectivData.objectiveType == QuestStepObjectiveType.HadConversation)
            {
                Conversation conversation = (Conversation)MPQStorage.Data.Assets[SNOGroup.Conversation][_objectivData.SNOName1.SNOId].Data;
                if (conversation.SNOPrimaryNpc == actor.ActorSNO)
                {
                    _engine.InitiateConversation(conversation, actor);
                    _completed = true;
                    return;
                }
            }

            if (_objectivData.objectiveType == QuestStepObjectiveType.InteractWithActor)
            {
                _completed = true;
                return;
            }

        }
    }
   


}
