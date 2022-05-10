using System.Collections;
using System.Collections.Generic;
using Audio;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioClipsPlayer _flyUp;
    [SerializeField] private AudioClipsPlayer _tractorBeam;
    [SerializeField] private AudioClipsPlayer _flyDown;
    [SerializeField] private AudioClipsPlayer _laugh;
    [SerializeField] private AudioClipsPlayer _scream;

    public void FlyUp() => _flyUp.Play();
    public void TractorBeam() => _tractorBeam.Play();
    public void FlyDown() => _flyDown.Play();
    public void Laugh() => _laugh.Play();
    public void Scream() => _scream.Play();
}
