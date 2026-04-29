using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
    [SerializeField] private SpriteAnimationClip[] clips;

    public event Action OnComplete;

    private SpriteRenderer _renderer;
    private Dictionary<string, SpriteAnimationClip> _clipMap;
    private SpriteAnimationClip _current;
    private float _timer;
    private int _frame;
    private bool _loop = true;
    private bool _playing;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _clipMap = new Dictionary<string, SpriteAnimationClip>();
        foreach (var c in clips)
            _clipMap[c.name] = c;
    }

    public void Play(string clipName, bool loop = true)
    {
        if (!_clipMap.TryGetValue(clipName, out var clip)) return;
        if (_current == clip && _playing) return; // already playing

        _current = clip;
        _loop = loop;
        _frame = 0;
        _timer = 0f;
        _playing = true;

        _renderer.SetFrame(_current, _frame);
    }

    private void Update()
    {
        if (!_playing || _current == null) return;

        _timer += Time.deltaTime;

        if (_timer >= _current.secondsPerFrame)
        {
            _timer -= _current.secondsPerFrame;
            _frame++;

            if (_frame >= _current.frameCount)
            {
                if (_loop)
                {
                    _frame = 0;
                }
                else
                {
                    _frame = _current.frameCount - 1;
                    _playing = false;
                    OnComplete?.Invoke();
                    return;
                }
            }

            _renderer.SetFrame(_current, _frame);
        }
    }
}