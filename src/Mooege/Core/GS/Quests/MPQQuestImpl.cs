using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mooege.Common.MPQ.FileFormats;
using Mooege.Net.GS.Message;
using Mooege.Net.GS.Message.Definitions.Quest;
using Mooege.Net.GS;
using Mooege.Net.GS.Message.Fields;

namespace Mooege.Core.GS.Quests
{
    public class MPQQuest : IQuest
    {

        private Quest _questData;
        private List<QuestStep>.Enumerator _stepEnumerator;
        private List<QuestObjectivImpl> _objectiveList;

        private Boolean _isFailed = false;
        private Boolean _isCompleted = false;
        private Boolean _isActive = false;

        private QuestEngine _engine;

        public MPQQuest(Quest quest)
        {
            this._questData = quest;
        }

        public QuestStep GetQuestStep()
        {
            return _stepEnumerator.Current;
        }

        public List<QuestStepObjectiveSet> GetQuestStepGoals()
        {
            return GetQuestStep().StepObjectiveSets;
        }

        public GameMessage CreateQuestUpdateMessage()
        {
            
            QuestUpdateMessage message = new QuestUpdateMessage
            {
                Failed = _isFailed,
                Field3 = _isActive,
                snoQuest = _questData.Header.SNOId,
                StepID = (GetQuestStep() != null) ? GetQuestStep().I0 : -1,                    
            };
            return message;
            
        }

        public bool IsActive()
        {
            return _isActive;
        }

        public bool IsFailed()
        {
            return _isFailed;
        }

        public bool IsCompleted()
        {
            return _isCompleted;
        }

        public void SendQuestInformation(GameClient client)
        {
            throw new NotImplementedException();
        }

        public void Start(QuestEngine engine)
        {
            this._engine = engine;
            _stepEnumerator = this._questData.QuestSteps.GetEnumerator();
            NextQuestStep();
        }


        private void NextQuestStep()
        {
            _stepEnumerator.MoveNext();
            if (_stepEnumerator.Current == null)
            {
                _isCompleted = true;
                _isActive = false;
                _engine.OnQuestCompleted(_questData.Header.SNOId);
            }
            else
            {
                _isActive = true;
                _objectiveList = new List<QuestObjectivImpl>();
                foreach (QuestStepObjectiveSet objectivSet in GetQuestStepGoals())
                {
                    foreach (QuestStepObjective objectiv in objectivSet.StepObjectives)
                    {

                        if (objectiv.objectiveType == QuestStepObjectiveType.EventReceived
                            || objectiv.objectiveType == QuestStepObjectiveType.EnterLevelArea
                            || objectiv.objectiveType == QuestStepObjectiveType.EnterWorld
                            || objectiv.objectiveType == QuestStepObjectiveType.EnterTrigger
                            || objectiv.objectiveType == QuestStepObjectiveType.EnterScene
                            || objectiv.objectiveType == QuestStepObjectiveType.GameFlagSet
                            || objectiv.objectiveType == QuestStepObjectiveType.PlayerFlagSet
                            || objectiv.objectiveType == QuestStepObjectiveType.TimedEventExpired
                            || objectiv.objectiveType == QuestStepObjectiveType.CompleteQuest)
                        {
                            // ObjectiveType cannot handled at the moment - just ignore this objective
                        }
                        else
                        {
                            _objectiveList.Add(new QuestObjectivImpl(_engine, objectiv));
                        }
                    }
                }
            }

            this._engine.UpdateQuestStatus(this);

           
        }


        private List<QuestObjectivImpl> ActiveObjectives
        {
            get { return _objectiveList.Where(objectiv => !objectiv.isCompleted()).ToList(); }
        }

        public void OnDeath(Actors.Actor actor)
        {
            foreach (QuestObjectivImpl objectiv in ActiveObjectives)
            {
                objectiv.OnDeath(actor);
            }

            if (ActiveObjectives.Count == 0)
            {
                NextQuestStep();
            }
        }

        public void OnPositionUpdate(Vector3D position)
        {
            foreach (QuestObjectivImpl objectiv in ActiveObjectives)
            {
                objectiv.OnPositionUpdate(position);
            }

            if (ActiveObjectives.Count == 0)
            {
                NextQuestStep();
            }
        }

        public void OnEnterWorld(Map.World world)
        {
            foreach (QuestObjectivImpl objectiv in ActiveObjectives)
            {
                objectiv.OnEnterWorld(world);
            }

            if (ActiveObjectives.Count == 0)
            {
                NextQuestStep();
            }
        }

        public void OnInteraction(Player.Player player, Actors.Actor actor)
        {
            foreach (QuestObjectivImpl objectiv in ActiveObjectives)
            {
                objectiv.OnInteraction(player, actor);
            }

            if (ActiveObjectives.Count == 0)
            {
                NextQuestStep();
            }
        }

        public void Cancel()
        {
            throw new NotImplementedException();
        }


        public void OnEvent(int eventSNOId)
        {
            foreach (QuestObjectivImpl objectiv in ActiveObjectives)
            {
                objectiv.OnEvent(eventSNOId);
            }

            if (ActiveObjectives.Count == 0)
            {
                NextQuestStep();
            }
        }


        public void OnQuestCompleted(int questSNOId)
        {
            foreach (QuestObjectivImpl objectiv in ActiveObjectives)
            {
                objectiv.OnQuestCompleted(questSNOId);
            }

            if (ActiveObjectives.Count == 0)
            {
                NextQuestStep();
            }
        }
    }

}