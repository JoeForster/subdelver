using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSectionID : MonoBehaviour
{
    public int IdentificationNumber; //What ID is this section
    public List<int> IDsICanWorkWith; //Which IDs can I connect with
    public Transform EndPoint; // snap point

    public void Start()
    {
        if(EndPoint == null)
        EndPoint = transform.Find("End_Snap_Point").transform;
        
    }

}
