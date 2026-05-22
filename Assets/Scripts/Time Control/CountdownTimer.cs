using System;
using UnityEngine;

public class CountdownTimer
{
    public enum TimerState { Stopped, Running, Paused }

    private float _initialTime;
    private float _currentTime;

    public float CurrentTime => _currentTime;
    public float Progress => _initialTime > 0f ? _currentTime / _initialTime : 0f;
    public TimerState State { get; private set; } = TimerState.Stopped;

    public event Action OnStarted;
    public event Action<float> OnTicked;
    public event Action OnPaused;
    public event Action OnCompleted;

    public CountdownTimer(float initialTime)
    {
        _initialTime = Mathf.Max(0f, initialTime);
    }

    public void Tick(float deltaTime)
    {
        if (State != TimerState.Running) return;

        _currentTime -= deltaTime;
        OnTicked?.Invoke(_currentTime);

        if (_currentTime <= 0f)
        {
            _currentTime = 0f;
            State = TimerState.Stopped;
            OnCompleted?.Invoke();
        }
    }

    public void Play()
    {
        _currentTime = _initialTime;
        State = TimerState.Running;
        OnStarted?.Invoke();
    }
    public void Play(float customTime)
    {
        _initialTime = Mathf.Max(0f, customTime);

        Play();
    }

    public void Pause()
    {
        if (State != TimerState.Running) return;

        State = TimerState.Paused;
        OnPaused?.Invoke();
    }

    public void Resume()
    {
        if (State != TimerState.Paused) return;

        State = TimerState.Running;
    }

    public void Stop()
    {
        State = TimerState.Stopped;
        _currentTime = 0f;
    }

    public void Reset()
    {
        _currentTime = _initialTime;
        State = TimerState.Stopped;
    }
    
    public void AddTime(float amount)
    {
        _currentTime = Mathf.Max(0f, _currentTime + amount);
    }
}