using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffect : MonoBehaviour
{
    private ParticleSystem particles;
    private AudioSource audio;

    void Awake()
    {
        particles = GetComponent<ParticleSystem>();
        audio = GetComponent<AudioSource>();
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if( particles.isStopped && (!audio || !audio.isPlaying) )
            Destroy( gameObject );
    }
}
