using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using libSSM;

namespace libSSMTests
{
    [TestClass]
    public class SSMSimple
    {
        enum Events
        {
            Event1,
            Event2,
            Event3
        }

        static Events lastSeenTransition = Events.Event3;
        static Events lastExitedState = Events.Event3;

        [TestInitialize]
        public void Setup()
        {
            lastSeenTransition = Events.Event3;
            lastExitedState = Events.Event3;
        }          

        [TestMethod]
        public void SimpleStateTransition()
        {
            var S1 = new BasicState<Events>("S1");
            var S2 = new BasicState<Events>("S2");

            var transitions = new[]
            {
                new StateTransition<Events>{from = S1, to = S2, evtFunc = x => x == Events.Event1, guard = null },
                new StateTransition<Events>{from = S2, to = S1, evtFunc = x => x == Events.Event2, guard = null },
            };

            StateMachine<Events> sm = new StateMachine<Events>(transitions, S1);

            sm.Event(Events.Event1);

            Assert.AreEqual(sm.ActiveState, S2);
        }



        class PrintingState : IState<Events>
        {
            public void OnEnter(Events obj)
            {
                lastSeenTransition = obj;
            }

            public void OnLeave(Events obj)
            {
                lastExitedState = obj;
            }
        }

        [TestMethod]
        public void StateTransition_CallsOnEnter()
        {
            var S1 = new PrintingState();
            var S2 = new PrintingState();

            var transitions = new[]
            {
                new StateTransition<Events>{from = S1, to = S2, evtFunc = x => x == Events.Event1, guard = null },
                new StateTransition<Events>{from = S2, to = S1, evtFunc = x => x == Events.Event2, guard = null },
            };

            StateMachine<Events> sm = new StateMachine<Events>(transitions, S1);

            Assert.AreEqual(lastSeenTransition, Events.Event3);
            sm.Event(Events.Event1);
            Assert.AreEqual(lastSeenTransition, Events.Event1);            
        }

        [TestMethod]
        public void StateTransition_CallsOnLeave()
        {
            var S1 = new PrintingState();
            var S2 = new PrintingState();

            var transitions = new[]
            {
                new StateTransition<Events>{from = S1, to = S2, evtFunc = x => x == Events.Event1, guard = null },
                new StateTransition<Events>{from = S2, to = S1, evtFunc = x => x == Events.Event2, guard = null },
            };

            StateMachine<Events> sm = new StateMachine<Events>(transitions, S1);

            Assert.AreEqual(lastExitedState, Events.Event3);
            sm.Event(Events.Event1);
            Assert.AreEqual(lastExitedState, Events.Event1);
        }

        [TestMethod]
        public void PatternMatcherAndStates()
        {
            var S1 = new BasicState<char>("S1");
            var S2 = new BasicState<char>("S2");

            var transitions = new[]
            {
                new StateTransition<char>{from = S1, to = S2, evtFunc = x => "cd".Contains(x.ToString()), guard = null },
                new StateTransition<char>{from = S2, to = S1, evtFunc =  x => "ef".Contains(x.ToString()), guard = null },

            };

            StateMachine<char> sm = new StateMachine<char>(transitions, S1);

            Assert.AreEqual(sm.ActiveState, S1);
            sm.Event('d');
            Assert.AreEqual(sm.ActiveState, S2);
            sm.Event('e');
            Assert.AreEqual(sm.ActiveState, S1);
        }
    }
}