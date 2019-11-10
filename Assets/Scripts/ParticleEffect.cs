using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffect : MonoBehaviour
{
    private ParticleSystem particles;
    public AudioSource Audio;

    // Use this for initialization
    void Start()
    {
        particles = GetComponent<ParticleSystem>();
        Audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if( particles.isStopped && (!Audio || !Audio.isPlaying) )
            Destroy( gameObject );
    }
}
