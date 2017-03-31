using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libSSM
{    
    public delegate bool TransitionGuard();
   
    public interface IState<T> where T : IComparable
    {
        void OnEnter(T obj);

        void OnLeave(T obj);
    }

    public class BasicState<T> : IState<T> where T : IComparable
    {
        string stateName;

        public BasicState(string n)
        {
            stateName = n;
        }

        public override string ToString()
        {
            return stateName;
        }

        public void OnEnter(T obj)
        {
        }

        public void OnLeave(T obj)
        {
        }
    }

    public class StateTransition<EventTy> where EventTy : IComparable
    {
        public IState<EventTy> from;
        public IState<EventTy> to;
        public Func<EventTy, bool> evtFunc;
        public TransitionGuard guard = () => true;
    }

    public class StateMachine<EventTy> where EventTy : IComparable
    {

        public IState<EventTy> ActiveState { get; private set; }        
        StateTransition<EventTy>[] transitionTable;

        public StateMachine(StateTransition<EventTy>[] transitionTable, IState<EventTy> startState)
        {
            this.transitionTable = transitionTable;
            ActiveState = startState;
        }

        public void Event(EventTy evt)
        {
            var transition = transitionTable.FirstOrDefault(x => x.from == ActiveState && 
                                                                 x.evtFunc(evt) &&
                                                                 (x.guard == null ? true : x.guard()));

            if (transition == null)
                throw new Exception("Invalid input for state " + ActiveState.ToString() + ": " + evt.ToString());

            ActiveState.OnLeave(evt);
            ActiveState = transition.to;
            ActiveState.OnEnter(evt);
        }
        
    }
}
