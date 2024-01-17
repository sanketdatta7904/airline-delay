using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindClosest : MonoBehaviour
{
    public GameObject WhitePrefab;
    public GameObject BlackPrefab;
    public int CountWhite;
    public int CountBlack;
    //protected List<Controller> WhiteBalls = new List<Controller>();
    //protected List<Controller> BlackBalls = new List<Controller>();
    protected KdTree<Controller> WhiteBalls = new KdTree<Controller>();
    protected KdTree<Controller> BlackBalls = new KdTree<Controller>();
    void Start()
    {
        for (int i = 0; i < CountWhite; i++)
        {
            //WhiteBalls.Add(Instantiate(WhitePrefab).GetComponent<Controller>());
            WhiteBalls.Add(Instantiate(WhitePrefab, new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10)), Quaternion.identity).GetComponent<Controller>());

        }

        for (int i = 0; i < CountBlack; i++)
        {
            //BlackBalls.Add(Instantiate(BlackPrefab).GetComponent<Controller>());
            BlackBalls.Add(Instantiate(BlackPrefab, new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10)), Quaternion.identity).GetComponent<Controller>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        // update the list if the objects in the list have moved
        BlackBalls.UpdatePositions();

        foreach (var whiteBall in WhiteBalls)
        {
            // find the closest black ball
            Controller nearestObj = BlackBalls.FindClosest(whiteBall.transform.position);

            // do something with the closest black ball
            Debug.DrawLine(whiteBall.transform.position, nearestObj.transform.position, Color.red);
        }
    }
}
