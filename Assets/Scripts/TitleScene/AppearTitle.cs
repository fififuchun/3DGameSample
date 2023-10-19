using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppearTitle : MonoBehaviour
{
    [SerializeField] private GameObject mergePinsUI;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time> 1.0f){
            mergePinsUI.SetActive(true);
        }
    }
}
