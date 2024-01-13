using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateLevel : MonoBehaviour
{
    public List<GameObject> pipeSections;
    public List<GameObject> LevelOneList;

    public Transform levelStartPoint;
    public Transform LastPipeSpawnEndPoint;

    public int numberOfPipesToCreate;

    public bool levelStart = false;
    public bool CreateAuthoredLevel = false;


    /*
     * We need the ability for a designer to add prefers selections to this script that holds a list of each addition
     * ideally we should start with a straight down pipe section for each level 
     * it should automatically add a connecting pipe section and snap to it
     * */


    // Start is called before the first frame update
    void Start()
    {
        if(CreateAuthoredLevel == true)
        {
            Debug.Log("Generated authored level due to designer selection");

            GameObject newObject = Instantiate(pipeSections[0].gameObject, levelStartPoint.transform);
            LastPipeSpawnEndPoint = newObject.GetComponent<LevelSectionID>().EndPoint;
            levelStart = true;

            
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            GenerateNewLevel();
        }
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            GeneratePredefinedLevel(LevelOneList);
        }
    }

    public void GenerateNewLevel()
    {
        //generate a pipe section to fall down into 
        //Connect to that section a new pipe prefer that connects to the end point of the straight section
        //grab the rule IDs on that next section to determine what can be spawned onto it
        //

        //Rule IDs - how do I handle these? each pipe is assigned it's own ID??
        //each pipe holds a set of values that relate 
        if(levelStart == false)
        {
            GameObject newObject =  Instantiate(pipeSections[0].gameObject, levelStartPoint.transform);
            LastPipeSpawnEndPoint = newObject.GetComponent<LevelSectionID>().EndPoint;
            levelStart = true;
        }else
        {
            ChooseNextPipeSection();
        }
    }

    public void ChooseNextPipeSection()
    {
        //pass in the current pipe connection
        //find out what pipes it could connect with 
        //spawn this in the level
        //get current pipe section info
        List<int> IDs = pipeSections[0].GetComponent<LevelSectionID>().IDsICanWorkWith;
        int ChooseRandomIDToSelect = Random.Range(1, IDs.Count);
        Debug.Log("random selection = " + ChooseRandomIDToSelect);

        //if I know what the pipe section ID number is and I know what the random section I have choosen is
        //then find the prefab that matches the ID based on the list provided in pipesections list
        //search pipesections list and check if it's ID number equals the number that is generated 

        int _index;
        for (_index = 0; _index < pipeSections.Count; _index++)
        {
            if (ChooseRandomIDToSelect == pipeSections[_index].GetComponent<LevelSectionID>().IdentificationNumber)
            {
               GameObject newObject = Instantiate(pipeSections[_index].gameObject, new Vector3 (LastPipeSpawnEndPoint.position.x, LastPipeSpawnEndPoint.position.y,
                    LastPipeSpawnEndPoint.position.z), LastPipeSpawnEndPoint.rotation, this.transform);
                LastPipeSpawnEndPoint = newObject.GetComponent<LevelSectionID>().EndPoint;
            }
        }
        _index = 0;
    }
    /*
     * TODO need to update this func to use the list of data to inform which pipe sections to generate.
     */
    public void GeneratePredefinedLevel(List<GameObject> PipesToCreate)
    {
        Debug.Log(PipesToCreate.Count);
        int _index;
        for (_index = 0; _index < PipesToCreate.Count; _index++)
        {
            Debug.Log(_index);
            GameObject newObject = Instantiate(PipesToCreate[_index].gameObject, new Vector3(LastPipeSpawnEndPoint.position.x, LastPipeSpawnEndPoint.position.y,
                    LastPipeSpawnEndPoint.position.z), LastPipeSpawnEndPoint.rotation, this.transform);
            Debug.Log(newObject.name + "name of object created");
            LastPipeSpawnEndPoint = newObject.GetComponent<LevelSectionID>().EndPoint;
        }
    }
}
