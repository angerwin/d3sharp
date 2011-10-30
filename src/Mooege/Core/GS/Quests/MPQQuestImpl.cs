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
using Mooege.Common;

namespace Mooege.Core.GS.Quests
{
    public class MPQQuest : IQuest
    {

        private static readonly Logger Logger = LogManager.CreateLogger();        

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
                foreach (QuestStepObjective objectiv in objectivSet.StepObjectives)
                {

                    IQuestObjective questObjective = new QuestObjectivImpl(_engine, objectiv, this);
                    _objectiveList.Add(questObjective);
                    _engine.Register(questObjective);

                    if (objectiv.ObjectiveType == QuestStepObjectiveType.EventReceived
                        || objectiv.ObjectiveType == QuestStepObjectiveType.EnterLevelArea                      
                        || objectiv.ObjectiveType == QuestStepObjectiveType.EnterTrigger                        
                        || objectiv.ObjectiveType == QuestStepObjectiveType.GameFlagSet
                        || objectiv.ObjectiveType == QuestStepObjectiveType.PlayerFlagSet
                        || objectiv.ObjectiveType == QuestStepObjectiveType.TimedEventExpired)
                    {
                        // ObjectiveType cannot handled at the moment - just ignore this objective
                        Logger.Warn("Quest Objective is ignored!!! Type: {0} id: {1}", objectiv.ObjectiveType, objectiv.I0);
                        Logger.Warn(objectiv.ToString());
                        questObjective.Cancel();                        
                    }
                  
                }
            }
        }

        private List<IQuestObjective> ActiveObjectives
        {
            get { return (List<IQuestObjective>)_objectiveList.Where(objectiv => !objectiv.IsCompleted()).ToList(); }
        }
      
        public void Cancel()
        {
            throw new NotImplementedException();
        }

        public void ObjectiveComplete(IQuestObjective objective)
        {
            _engine.Unregister(objective);
            if (ActiveObjectives.Count == 0)
            {
                NextQuestStep();
            }
        }
    }
}