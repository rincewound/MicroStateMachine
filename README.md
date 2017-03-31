# MicroStateMachine
A tiny implementation of a generic statemachine.

MSM is the result of a small code dojo, that turned out something rather powerful,
given the few lines of code actually written for it. MSM allows the modelling
and use of generic state machines (hence the name!)

Usage:

##Example

```C#

void main()
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
```

The class BasicState allows you to use a generic state thingy, that does not do anything special.
If you need to actually do something within the states, you should implement IState as so:
```C#

class PrintingState: IState<char>
{
  public void OnEnter(char c)
  {
    // Called whenever the state is entered, with the value of the
    // event that caused the statetransition
  }

  public void OnLeave(char c)
  {
    // Called whenever the state is left, with the value of the
    // event that caused the statetransition
  }
}
```

For a more elaborate example, of what can be done with little code check SSMParseProtocol.cs
