using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class GOSingleton<T> : MonoBehaviour where T : Component
{
    //싱글톤으로 유일한 게임 오브젝트 + 컴포넌트 생성
    //Singleton<T>를 상속받은 각각의 T 컴포넌트들은 자신을 가리키는 유일한 Instance를 가짐
    
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
