using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class GOSingleton<T> : MonoBehaviour where T : Component
{
    //�̱������� ������ ���� ������Ʈ + ������Ʈ ����
    //Singleton<T>�� ��ӹ��� ������ T ������Ʈ���� �ڽ��� ����Ű�� ������ Instance�� ����
    
    private static T instance;
    public static T Instance { get { Init(); return instance; } private set { instance = value; } }

    private static bool isDestroyed = false;


    protected virtual void Awake()
    {
        Init();
    }

    private void OnDestroy()
    {
        isDestroyed = true;
    }

    protected static void Init()
    {
        if(isDestroyed == true) return;

        if (instance == null)
        {
            GameObject go = GameObject.Find(typeof(T).Name);

            if(go == null)
            { 
                go = new GameObject(typeof(T).Name);
            }

            if(go.TryGetComponent(out instance) == false)
            {
                Instance = go.AddComponent<T>();
            }
        }
    }
}