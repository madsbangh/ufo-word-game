using System.Collections;
using System.Collections.Generic;
using Audio;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioClipsPlayer _flyUp;
    [SerializeField] private AudioClipsPlayer _tractorBeam;
    [SerializeField] private AudioClipsPlayer _flyDown;
    
    [Space] [SerializeField] private AudioClipsPlayer _laugh;
    [SerializeField] private AudioClipsPlayer _chainHoist;
    [SerializeField] private AudioClipsPlayer _drill;
    [SerializeField] private AudioClipsPlayer _burp;

    [Space] [SerializeField] private AudioClipsPlayer _spellUp;
    [SerializeField] private AudioClipsPlayer _spellDown;

    [Space] [SerializeField] private float _tractorBeamChainSoundProbability;
    [SerializeField] private float _burpProbability;
    [SerializeField] private float _drillProbability;
    [SerializeField] private float _laughProbability;
    
    [Space] [SerializeField] private float _spellPitchScale;
    [SerializeField] private float _spellPitchOffset;

    public void FlyUp() => _flyUp.Play();

    public void TractorBeam()
    {
        _tractorBeam.Play();

        if (Random.value < _tractorBeamChainSoundProbability)
        {
            _chainHoist.Play();
        }
    }

    public void FlyDown() => _flyDown.Play();

    public void RandomPostSuctionSound()
    {
        var randomValue = Random.value;
        if (randomValue < _burpProbability)
        {
            _burp.Play();
        }
        else if (randomValue < _burpProbability + _drillProbability)
        {
            _drill.Play();
        }
        else if (randomValue < _burpProbability + _drillProbability + _laughProbability)
        {
            _laugh.Play();
        }
    }

    public void AddLetter(int newLetterCount)
    {
        _spellUp.Play(pitchMultiplier: _spellPitchOffset + _spellPitchScale * newLetterCount);
    }
    
    public void RemoveLetter(int newLetterCount)
    {
        _spellDown.Play(pitchMultiplier: _spellPitchOffset + _spellPitchScale * newLetterCount);
    }
}