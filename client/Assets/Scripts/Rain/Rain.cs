using UnityEngine;

public class Rain : MonoBehaviour
{
    private float speed = 100f; 
    private Pool pool;

    private void Awake()
    {
        pool = Pool.Instance;
    }

    void Update()
    {
        // ���� ���� �Ʒ��� �������� �ı�
        if (transform.position.y < -100f)
        {
            /*            Destroy(gameObject);*/
            pool.DisableObject(gameObject);
        }
        else
        {
            // �Ʒ��� �̵�
            transform.Translate(Vector3.down * speed * Time.deltaTime);
        }
    }
}
