using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class KillCounter : MonoBehaviour
{
    public static KillCounter Instance { get; private set; }
    public int Kills { get; private set; }

    public event Action<int> OnChanged;

    private readonly HashSet<int> _counted = new HashSet<int>();

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        ResetCounter();
        SceneManager.sceneLoaded += (_, __) => ResetCounter();
    }

    public void ResetCounter()
    {
        Kills = 0;
        _counted.Clear();
        OnChanged?.Invoke(Kills);
    }

    public void RegisterKill(Component enemy)
    {
        if (!enemy) return;
        int id = enemy.GetInstanceID();
        if (_counted.Contains(id)) return;

        _counted.Add(id);
        Kills++;
        OnChanged?.Invoke(Kills);
    }
}