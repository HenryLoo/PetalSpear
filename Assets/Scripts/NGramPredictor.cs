using System.Collections.Generic;
using UnityEngine;

public class NGramPredictor
{
    public struct KeyDataRecord
    {
        // Hold the counts for each successor action.
        public Dictionary<string, int> Counts;

        // Hold the total number of times the window has been seen.
        public int Total;
    }

    // Hold frequency data.
    private Dictionary<string, KeyDataRecord> data;

    // Hold the size of the window + 1.
    private int nValue;

    public NGramPredictor( int size )
    {
        nValue = size + 1;
        data = new Dictionary<string, KeyDataRecord>();
    }

    private static string GetKeyFromList( List<string> actions )
    {
        return string.Join( "", actions.ToArray() );
    }

    // Register a set of actions with a predictor.
    // Actions must have nValue elements in it.
    public void RegisterSequence( List<string> actions )
    {
        // Split the sequence into a key and a value.
        string key = GetKeyFromList( actions.GetRange( 0, nValue - 1 ) );
        string value = actions[ nValue - 1 ];
        //Debug.Log( "NGrams key: " + key + ", value: " + value );

        // Try to get the data structure.
        KeyDataRecord keyData;
        if( !data.TryGetValue( key, out keyData ) )
        {
            keyData = new KeyDataRecord();
            data.Add( key, keyData );
        }

        // Initialize record for the value if it doesn't exist.
        if( keyData.Counts == null )
        {
            keyData.Counts = new Dictionary<string, int>();
        }
        if( !keyData.Counts.ContainsKey( value ) )
        {
            keyData.Counts.Add( value, 0 );
        }

        // Add to the total and increment the count for the value.
        ++keyData.Counts[ value ];
        ++keyData.Total;
        data[ key ] = keyData;
    }

    // Predict the most likely next action from the given window of actions.
    // Actions has nValue - 1 elements in it (the size of the window).
    public string PredictNextAction( List<string> actions )
    {
        // Get the key data.
        KeyDataRecord keyData;
        string key = GetKeyFromList( actions );
        if( !data.TryGetValue( key, out keyData ) )
        {
            keyData = new KeyDataRecord();
            data.Add( key, keyData );
        }

        // Find the highest probability.
        int highestValue = 0;
        string bestAction = string.Empty;

        // Get the list of actions from the store.
        if( keyData.Counts == null )
        {
            keyData.Counts = new Dictionary<string, int>();
        }
        List<string> storedActions = new List<string>( keyData.Counts.Keys );

        foreach( string thisAction in storedActions )
        {
            // Check the highest value.
            int thisValue = keyData.Counts[ thisAction ];
            if( keyData.Counts[ thisAction ] > highestValue )
            {
                // Store the action.
                highestValue = thisValue;
                bestAction = thisAction;
            }
        }

        //Debug.Log( "NGrams Predict: " + bestAction );
        return bestAction;
    }

    public void Reset()
    {
        data.Clear();
    }
}
