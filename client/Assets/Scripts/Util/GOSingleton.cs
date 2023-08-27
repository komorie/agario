using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public abstract class GOSingleton<T> : MonoBehaviour where T : Component
{
    //싱글톤으로 유일한 게임 오브젝트 + 컴포넌트 생성
    //Instance로 Singleton<T>를 상속받은 각각의 T 컴포넌트들의 유일한 컴포넌트에 접근
    //일단 게임 종료시까지 절대 파괴되지 않을 객체에만 쓴다

    private static T instance;
    
    //인스턴스로 접근 시 null이면 새로 생성. 이미 생성되어있으면 그냥 반환 
    public static T Instance {
        get
        {
            if (instance == null)
            {
                // 현재 씬에 싱글톤이 있나 찾아본다. 
                instance = FindObjectOfType<T>();

                // 또 체크 (이렇게 안 하니까 가끔씩 오류나는데 이유를 모르겠음)
                if (instance == null)
                {
                    string componentName = typeof(T).ToString();
                    GameObject findObject = GameObject.Find(typeof(T).ToString());

                    // 없으면
                    if (findObject == null)
                    {
                        findObject = new GameObject(componentName);
                    }

                    instance = findObject.AddComponent<T>();

                    // 씬이 변경되어도 객체가 유지되도록 설정
                    DontDestroyOnLoad(instance);
                }
            }
            return instance;
        }
    }

/*    public static bool isDestroyed = false; //객체 파괴 이벤트로 싱글톤에 접근하는 경우가 있음 -> 이러면 유니티 에디터에서 종료 시 싱글톤 객체 파괴 후 다시 생성하는 오류 발생함
    public virtual void OnDestroy()
    {
        isDestroyed = true;
    }*/
}
