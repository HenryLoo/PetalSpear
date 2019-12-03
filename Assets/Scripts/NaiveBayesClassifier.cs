using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Simple Naive Bayes implementation, with only binary discrete attributes.
public class NaiveBayesClassifier
{
    private int numPositiveExamples;
    private int numNegativeExamples;

    // Number of times each attribute was true.
    private List<int> numPositiveAttributes;
    private List<int> numNegativeAttributes;
    private int numAttributes;

    public NaiveBayesClassifier( int numAttributes )
    {
        numPositiveAttributes = new List<int>();
        numNegativeAttributes = new List<int>();

        this.numAttributes = numAttributes;
    }

    // Assume attributes has the same number of elements as 
    // numPositiveAttributes and numNegativeAttributes.
    public void Update( List<bool> attributes, bool isPositiveExample )
    {
        for( int i = 0; i < attributes.Count; ++i )
        {
            // Check if this is a positive or negative example.
            // Update all counts.
            int count = attributes[ i ] ? 1 : 0;
            if( isPositiveExample )
            {
                numPositiveAttributes[ i ] += count;
                ++numPositiveExamples;
            }
            else
            {
                numNegativeAttributes[ i ] += count;
                ++numNegativeExamples;
            }
        }
    }

    public bool Predict( List<bool> attributes )
    {
        float x = 0;
        if( numNegativeExamples > 0 )
        {
            x = naiveProbabilities( attributes, numPositiveAttributes,
                ( float ) numPositiveExamples, ( float ) numNegativeExamples );
        }

        float y = 0;
        if( numPositiveExamples > 0 )
        {
            y = naiveProbabilities( attributes, numNegativeAttributes,
                ( float ) numNegativeExamples, ( float ) numPositiveExamples );
        }

        // No examples.
        if( x == 0 && y == 0 )
            return false;

        //Debug.Log( "NBayes predict: Positive: " + x + ", Negative: " + y );
        return ( x >= y );
    }

    private float naiveProbabilities( List<bool> attributes, List<int> counts, float m, float n )
    {
        // Compute the prior.
        float prior = m / ( m + n );

        // Naive assumption of conditional independence.
        float p = 1.0f;
        for( int i = 0; i < attributes.Count; ++i )
        {
            p /= m;
            if( attributes[ i ] )
                p *= counts[ i ];
            else
                p *= m - counts[ i ];
        }

        return prior * p;
    }

    public void Reset()
    {
        numPositiveExamples = 0;
        numNegativeExamples = 0;

        numPositiveAttributes.Clear();
        numNegativeAttributes.Clear();

        for( int i = 0; i < numAttributes; ++i )
            numPositiveAttributes.Add( 0 );
        for( int i = 0; i < numAttributes; ++i )
            numNegativeAttributes.Add( 0 );
    }
}
