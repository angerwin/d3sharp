using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mooege.Common.MPQ.FileFormats;
using Mooege.Net.GS.Message;
using Mooege.Net.GS.Message.Definitions.Quest;
using Mooege.Net.GS;
using Mooege.Net.GS.Message.Fields;
using Mooege.Core.GS.Common.Types.Math;

namespace Mooege.Core.GS.Quests
{
    public class MPQQuest : IQuest
    {

        private Quest _questData;
        private List<QuestStep>.Enumerator _stepEnumerator;
        private List<IQuestObjective> _objectiveList;

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

        public List<QuestCompletionStep> GetCompletionSteps()
        {
            return _questData.QuestCompletionSteps;
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

        public int SNOId()
        {
            return _questData.Header.SNOId;
        }

        public void SendQuestInformation(GameClient client)
        {
            throw new NotImplementedException();
        }

        public void Start(QuestEngine engine)
        {
            this._engine = engine;
            _stepEnumerator = this._questData.QuestSteps.GetEnumerator();            
            AddQuestObjectives(this._questData.QuestUnassignedStep.StepObjectiveSets);
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
                AddQuestObjectives(GetQuestStepGoals());
            }

            this._engine.UpdateQuestStatus(this);

           
        }


        private void AddQuestObjectives(List<QuestStepObjectiveSet> objectiveSets)
        {
            _objectiveList = new List<IQuestObjective>();
            foreach (QuestStepObjectiveSet objectivSet in objectiveSets)
            {

                int taksIndex = 0;
                foreach (QuestStepObjective objectiv in objectivSet.StepObjectives)
                {

                    if (objectiv.ObjectiveType == QuestStepObjectiveType.EventReceived
                        || objectiv.ObjectiveType == QuestStepObjectiveType.EnterLevelArea                      
                        || objectiv.ObjectiveType == QuestStepObjectiveType.EnterTrigger                        
                        || objectiv.ObjectiveType == QuestStepObjectiveType.GameFlagSet
                        || objectiv.ObjectiveType == QuestStepObjectiveType.PlayerFlagSet
                        || objectiv.ObjectiveType == QuestStepObjectiveType.TimedEventExpired)
                    {
                        // ObjectiveType cannot handled at the moment - just ignore this objective
                    }
                    else
                    {
                        _objectiveList.Add(new QuestObjectivImpl(taksIndex , _engine, objectiv, GetQuestStep(), _questData));
                    }
                    taksIndex++;
                }
            }
            
        }

        private List<IQuestObjective> ActiveObjectives
        {
            get { return (List<IQuestObjective>)_objectiveList.Where(objectiv => !objectiv.IsCompleted()).ToList(); }
        }

        public void OnDeath(Actors.Actor actor)
        {
            foreach (IQuestObjective objectiv in ActiveObjectives)
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
            foreach (IQuestObjective objectiv in ActiveObjectives)
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
            foreach (IQuestObjective objectiv in ActiveObjectives)
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
            foreach (IQuestObjective objectiv in ActiveObjectives)
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
            foreach (IQuestObjective objectiv in ActiveObjectives)
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
            foreach (IQuestObjective objectiv in ActiveObjectives)
            {
                objectiv.OnQuestCompleted(questSNOId);
            }

            if (ActiveObjectives.Count == 0)
            {
                NextQuestStep();
            }
        }


        public void OnEnterScene(Map.Scene scene)
        {
            foreach (IQuestObjective objectiv in ActiveObjectives)
            {
                objectiv.OnEnterScene(scene);
            }

            if (ActiveObjectives.Count == 0)
            {
                NextQuestStep();
            }
        }
    }

}