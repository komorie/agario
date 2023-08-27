using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public abstract class GOSingleton<T> : MonoBehaviour where T : Component
{
    //�̱������� ������ ���� ������Ʈ + ������Ʈ ����
    //Instance�� Singleton<T>�� ��ӹ��� ������ T ������Ʈ���� ������ ������Ʈ�� ����
    //�ϴ� ���� ����ñ��� ���� �ı����� ���� ��ü���� ����

    private static T instance;
    
    //�ν��Ͻ��� ���� �� null�̸� ���� ����. �̹� �����Ǿ������� �׳� ��ȯ 
    public static T Instance {
        get
        {
            if (instance == null)
            {
                // ���� ���� �̱����� �ֳ� ã�ƺ���. 
                instance = FindObjectOfType<T>();

                // �� üũ (�̷��� �� �ϴϱ� ������ �������µ� ������ �𸣰���)
                if (instance == null)
                {
                    string componentName = typeof(T).ToString();
                    GameObject findObject = GameObject.Find(typeof(T).ToString());

                    // ������
                    if (findObject == null)
                    {
                        findObject = new GameObject(componentName);
                    }

                    instance = findObject.AddComponent<T>();

                    // ���� ����Ǿ ��ü�� �����ǵ��� ����
                    DontDestroyOnLoad(instance);
                }
            }
            return instance;
        }
    }

/*    public static bool isDestroyed = false; //��ü �ı� �̺�Ʈ�� �̱��濡 �����ϴ� ��찡 ���� -> �̷��� ����Ƽ �����Ϳ��� ���� �� �̱��� ��ü �ı� �� �ٽ� �����ϴ� ���� �߻���
    public virtual void OnDestroy()
    {
        isDestroyed = true;
    }*/
}
