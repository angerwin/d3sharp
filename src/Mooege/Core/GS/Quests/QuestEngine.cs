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

    public interface IQuestEngine : QuestNotifiable
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

        void Start(IQuestEngine engine);
        
        void Cancel();

        GameMessage CreateQuestUpdateMessage();
        
    }

    public interface QuestChain
    {
        IQuest GetNext();
    }

    public class PlayerQuestEngine : IQuestEngine
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
            _player.PlayHeroConversation(0x0002A73F, RandomHelper.Next(0, 8));               
        }
    }


    public class MPQQuest : IQuest
    {

        private Quest _questData;
        private List<QuestStep>.Enumerator _stepEnumerator;
        private List<QuestObjectiv> _objectiveList;
              
        private Boolean _isFailed = false;
        private Boolean _isCompleted = false;
        private Boolean _isActive = false;

        private IQuestEngine _engine;

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
                Failed = false,
                Field3 = true,
                snoQuest = _questData.Header.SNOId,
                StepID = GetQuestStep().I0,
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

        public void Start(IQuestEngine engine)
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
            }
            else
            {
                _isActive = true;
                _objectiveList = new List<QuestObjectiv>();
                foreach (QuestStepObjectiveSet objectivSet in GetQuestStepGoals())
                {
                    foreach (QuestStepObjective objectiv in objectivSet.StepObjectives)
                    {
                        _objectiveList.Add(new QuestObjectiv(_engine, objectiv));
                    }
                }               
            }

            this._engine.UpdateQuestStatus(this);
        }


        private List<QuestObjectiv> ActiveObjectives
        {
            get { return _objectiveList.Where(objectiv => !objectiv.isCompleted()).ToList(); }
        }

        public void OnDeath(Actors.Actor actor)
        {

            OnInteraction(actor); // TODO: at the moment Interaction is not possible. So use kill as interaction
            foreach (QuestObjectiv objectiv in ActiveObjectives)
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
            foreach (QuestObjectiv objectiv in ActiveObjectives)
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
            foreach (QuestObjectiv objectiv in ActiveObjectives)
            {
                objectiv.OnEnterWorld(world);
            }

            if (ActiveObjectives.Count == 0)
            {
                NextQuestStep();
            }
        }

        public void OnInteraction(Actors.Actor actor)
        {
            foreach (QuestObjectiv objectiv in ActiveObjectives)
            {
                objectiv.OnInteraction(actor);
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
    }


    public class QuestObjectiv : QuestNotifiable
    {

        private QuestStepObjective _objectiv;
        private Boolean _completed;
        private IQuestEngine _engine;
        
        public QuestObjectiv(IQuestEngine engine, QuestStepObjective objectiv)
        {        
            this._objectiv = objectiv;
            this._completed = false;
            this._engine = engine;
        }

        public Boolean isCompleted()
        {
            return _completed;
        }

        public void OnDeath(Actors.Actor actor)
        {
            if (_objectiv.objectiveType == QuestStepObjectiveType.KillGroup)
            {
                _completed = true;
                return;
            }

            if (_objectiv.objectiveType == QuestStepObjectiveType.KillMonster)
            {
                if (actor.ActorSNO == _objectiv.SNOName1.SNOId)
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
            if (_objectiv.objectiveType == QuestStepObjectiveType.HadConversation)
            {
                Conversation conversation = (Conversation)MPQStorage.Data.Assets[SNOGroup.Conversation][_objectiv.SNOName1.SNOId].Data;
                if (conversation.SNOPrimaryNpc == actor.ActorSNO)
                {
                    _engine.InitiateConversation(conversation, actor);
                    _completed = true;
                    return;
                }
            }

            if (_objectiv.objectiveType == QuestStepObjectiveType.InteractWithActor)
            {
                _completed = true;
                return;
            }
            
        }
    }
}
