using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffect : MonoBehaviour
{
    private ParticleSystem particles;
    private AudioSource particleAudio;

    void Awake()
    {
        particles = GetComponent<ParticleSystem>();
        particleAudio = GetComponent<AudioSource>();
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if( particles.isStopped && (!particleAudio || !particleAudio.isPlaying) )
            Destroy( gameObject );
    }
}
