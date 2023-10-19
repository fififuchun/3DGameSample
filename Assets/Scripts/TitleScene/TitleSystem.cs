using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleSystem : MonoBehaviour
{
    void Start()
    {
        if (Time.time < 1.0f)
        {
            int i = Random.Range(35, 45);
            int j = Random.Range(3, 6);
            int k = Random.Range(-45, -35);

            if (this.gameObject.tag == "Blaze")
            {
                GameObject DuplicatePrefabs = (GameObject)Resources.Load("Blaze");
                GameObject instance = (GameObject)Instantiate(DuplicatePrefabs, new Vector3(i, j, k), Quaternion.identity);
            }
            if (this.gameObject.tag == "Aqua")
            {
                GameObject DuplicatePrefabs = (GameObject)Resources.Load("Aqua");
                GameObject instance = (GameObject)Instantiate(DuplicatePrefabs, new Vector3(i, j, k), Quaternion.identity);
            }
            if (this.gameObject.tag == "Wood")
            {
                GameObject DuplicatePrefabs = (GameObject)Resources.Load("Wood");
                GameObject instance = (GameObject)Instantiate(DuplicatePrefabs, new Vector3(i, j, k), Quaternion.identity);
            }
            if (this.gameObject.tag == "Thunder")
            {
                GameObject DuplicatePrefabs = (GameObject)Resources.Load("Thunder");
                GameObject instance = (GameObject)Instantiate(DuplicatePrefabs, new Vector3(i, j, k), Quaternion.identity);
            }
        }
    }

    void Update()
    {
        
    }

    void OnBecameInvisible()
    {
        GameObject.Destroy(this.gameObject);
    }
}
