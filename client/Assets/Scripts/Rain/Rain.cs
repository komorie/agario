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
        // 일정 높이 아래로 내려가면 파괴
        if (transform.position.y < -100f)
        {
            /*            Destroy(gameObject);*/
            pool.DisableObject(gameObject);
        }
        else
        {
            // 아래로 이동
            transform.Translate(Vector3.down * speed * Time.deltaTime);
        }
    }
}
