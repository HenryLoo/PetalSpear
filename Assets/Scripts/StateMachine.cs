using System;
using System.Collections.Generic;

public class StateMachine
{
    public class Transition
    {
        public Action Action = () => { };
        public string TargetState;
        public Func<bool> IsTriggered;
    }

    public class State
    {
        public Action Action = () => { };
        public Action EntryAction = () => { };
        public Action ExitAction = () => { };
        public List<Transition> Transitions { get; set; }

        public State()
        {
            Transitions = new List<Transition>();
        }
    };

    private Dictionary<String, State> states;
    private State currentState;

    public StateMachine()
    {
        states = new Dictionary<string, State>();
    }

    public void AddState( string label, State state )
    {
        // If there are no existing states, set this one to
        // the current state.
        if( states.Count == 0 )
        {
            currentState = state;
            currentState.EntryAction();
        }

        states.Add( label, state );
    }

    // Call this once per frame.
    // Check all transitions to see if the state is transitioning.
    // Return a list of all actions to perform for this frame.
    public List<Action> Update()
    {
        Transition triggeredTransition = null;

        // Check each transition and get the first one that triggered.
        foreach( Transition transition in currentState.Transitions )
        {
            if( transition.IsTriggered() )
            {
                triggeredTransition = transition;
                break;
            }
        }

        // If a transition was triggered, set to its target state.
        List<Action> actions = new List<Action>();
        if( triggeredTransition != null )
        {
            string targetLabel = triggeredTransition.TargetState;
            State targetState;
            bool isFound = states.TryGetValue( targetLabel, out targetState );
            if( isFound )
            {
                // Compile a list of actions to perform for transitioning.
                actions.Add( currentState.ExitAction );
                actions.Add( triggeredTransition.Action );
                actions.Add( targetState.EntryAction );

                // Finish the transition to the target state and then return the
                // list of actions.
                currentState = targetState;
                return actions;
            }
        }

        // Otherwise, just return the current state's action.
        actions.Add( currentState.Action );
        return actions;
    }

}