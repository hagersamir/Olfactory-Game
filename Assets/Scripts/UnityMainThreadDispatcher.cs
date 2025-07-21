using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher instance;
    private static readonly Queue<Action> executionQueue = new Queue<Action>();

    public static UnityMainThreadDispatcher Instance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<UnityMainThreadDispatcher>();
            if (instance == null)
            {
                GameObject dispatcher = new GameObject("UnityMainThreadDispatcher");
                instance = dispatcher.AddComponent<UnityMainThreadDispatcher>();
                DontDestroyOnLoad(dispatcher);
            }
        }
        return instance;
    }

    public void Enqueue(IEnumerator action)
    {
        lock (executionQueue)
        {
            executionQueue.Enqueue(() => {
                StartCoroutine(action);
            });
        }
    }

    public void Enqueue(Action action)
    {
        Enqueue(ActionWrapper(action));
    }

    IEnumerator ActionWrapper(Action action)
    {
        action();
        yield return null;
    }

    void Update()
    {
        lock (executionQueue)
        {
            while (executionQueue.Count > 0)
            {
                executionQueue.Dequeue().Invoke();
            }
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}