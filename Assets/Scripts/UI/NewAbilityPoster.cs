using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewAbilityPoster : MonoBehaviour
{
    public GameObject TextPrefab;

    public float ShiftUpAmount = 20;

    List<GameObject> AddedText = new List<GameObject>();

    public void PostText(string text)
    {
        for (int i = AddedText.Count - 1; i >= 0; i--)
        {
            if (AddedText[i] == null)
            {
                AddedText.RemoveAt(i);
            }
        }

        var go = (GameObject) Instantiate(TextPrefab);
        go.transform.SetParent(this.gameObject.transform);
        go.transform.position = Vector3.zero;
        go.GetComponent<Text>().text = text;


        AddedText.Add(go);

        for (int i = 0; i < AddedText.Count - 1; i++)
        {
            AddedText[i].transform.position += new Vector3(0,ShiftUpAmount,0);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Service.AbilityPost = this;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = AddedText.Count - 1; i >= 0; i--)
        {
            if (AddedText[i] == null)
            {
                AddedText.RemoveAt(i);
            }
        }

        for (int i = AddedText.Count-1; i >= 0; i--)
        {
            if (AddedText[i])
            {
                AddedText[i].transform.position =
                    TextPrefab.transform.position + new Vector3(253 *2, (124 * 2) + ShiftUpAmount * i, 0);

            }
        }
    }
}
