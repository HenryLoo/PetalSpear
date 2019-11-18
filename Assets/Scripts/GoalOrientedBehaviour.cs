using System.Collections.Generic;
using UnityEngine;

namespace GoalOrientedBehaviour
{
    struct Goal
    {
        public string Name;
        public int Value;

        static public int GetDiscontentment( int newValue )
        {
            return newValue * newValue;
        }
    }

    struct Action
    {
        public Dictionary<string, int> Insistences;

        // Return the change in insistence that carrying this action would 
        // give.
        public int GetGoalChange( Goal goal )
        {
            Insistences.TryGetValue( goal.Name, out int value );
            return value;
        }
    }

    class OverallUtility
    {
        public List<Action> Actions { get; set; }
        public List<Goal> Goals { get; set; }

        public Action ChooseAction()
        {
            // Calculate discontentment of each action.
            Action bestAction = Actions[ 0 ];
            int bestValue = calculateDiscontentment( Actions[ 0 ] );

            foreach( Action action in Actions )
            {
                int thisValue = calculateDiscontentment( action );
                if( thisValue < bestValue ||
                    ( thisValue == bestValue && Random.Range( 0, 1 ) < 0.5 ) )
                {
                    bestValue = thisValue;
                    bestAction = action;
                }
            }

            // Return the best action.
            return bestAction;
        }

        private int calculateDiscontentment( Action action )
        {
            // Keep track of total discontentment.
            int discontentment = 0;

            // Check each goal.
            foreach( Goal goal in Goals )
            {
                // Calculate the new value after the action.
                int newValue = goal.Value + action.GetGoalChange( goal );

                // Get the discontentment of this value.
                discontentment += Goal.GetDiscontentment( newValue );
            }

            // Total discontentment cannot go below 0.
            return Mathf.Max( 0, discontentment );
        }
    }
}