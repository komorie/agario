using UnityEngine;

public class RainSpawner : MonoBehaviour
{
    public GameObject rainPrefab;
    private string objName = "Rain";
    private Pool pool;
    private float spawnRate = 0.01f;  
    private float nextSpawnTime;

    private void Awake()
    {
        rainPrefab = Resources.Load<GameObject>("Prefabs/Rain");
        pool = Pool.Instance;
    }

    void Update()
    {
        if (Time.time > nextSpawnTime)
        {
            SpawnRain();
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    void SpawnRain()
    {
        // 카메라 위쪽의 랜덤 위치에서 Rain 프리팹을 생성
        for (int i = 0; i < 10; i++)
        {
            float randomX = Random.Range(-100f, 100f);
            Vector3 spawnPosition = new Vector3(randomX, 100f, -5f);
/*            Instantiate(rainPrefab, spawnPosition, Quaternion.identity);*/
            GameObject obj = pool.GetObject(objName);
            obj.transform.position = spawnPosition;
        }
    }
}
