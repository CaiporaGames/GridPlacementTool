using System.Collections.Generic;
using UnityEngine;

public class MonoPool<T> : MonoBehaviour, IObjectPool<T> where T : UnityEngine.Object
{
    [SerializeField] private T _prefab;
    [SerializeField] private int _prewarm = 10;
    private readonly Stack<T> _pool = new();
    private Transform _parent;

    private void Awake()
    {
        _parent = new GameObject(typeof(T).Name + "Pool").transform;
        _parent.SetParent(transform);
        for (int i = 0; i < _prewarm; i++)
            Release(CreateInstance());
    }

    private T CreateInstance()
    {
        var inst = Instantiate(_prefab, _parent);
        if (inst is GameObject go)
            go.SetActive(false);
        else if (inst is Component comp)
            comp.gameObject.SetActive(false);
        return inst;
    }

    public T Get()
    {
        var item = _pool.Count > 0 ? _pool.Pop() : CreateInstance();
        switch (item)
        {
            case GameObject go: go.SetActive(true); break;
            case Component comp: comp.gameObject.SetActive(true); break;
        }
        return item;
    }

    public void Release(T item)
    {
        switch (item)
        {
            case GameObject go:
                go.SetActive(false);
                go.transform.SetParent(_parent, true);
                break;
            case Component comp:
                comp.gameObject.SetActive(false);
                comp.transform.SetParent(_parent, true);
                break;
        }
        _pool.Push(item);
    }
}