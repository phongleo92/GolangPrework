using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    #region DEF
    static string URL = "https://www.thesaigontimes.vn";

    [Serializable]
    public class Poster
    {
        public string Url, Author, DatePublish;
    }
    #endregion

    #region REF
    [SerializeField] Text TextURL;
    [SerializeField] Transform UIContent;
    [SerializeField] Transform UIElement;
    #endregion

    #region CACHE
    public List<Poster> CachePoster = new List<Poster>();
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        UIElement.gameObject.SetActive(false);
        TextURL.text = string.Format("URL: {0}", URL);

        StartCoroutine(ParseData(URL));
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    IEnumerator ParseData(string originUrl)
    {
        Queue<string> links = new Queue<string>();
        links.Enqueue(originUrl);

        while (links.Count > 0)
        {
            // Get a link
            string url = links.Dequeue();

            // Get content from link
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                string content = www.downloadHandler.text;
                Debug.LogWarning("Data: " + content);

                // Split node
                if (!string.IsNullOrEmpty(content))
                {
                    var posters = SplitNotes(content);

                    if(posters != null && posters.Count > 0)
                    {
                        for (int i = 0; i < posters.Count; i++)
                        {
                            CachePoster.Add(posters[i]);
                        }

                    }
                }



            }


        }


        Debug.Log("Parse Data Done !!!");
    }

    List<Poster> SplitNotes(string data)
    {
        List<Poster> result = new List<Poster>();

        //int start = data.IndexOf("<body>");
        //int end = data.IndexOf("<\body>");
        //data = data.Substring(start, (end - start));

        var pattern = "(<body|</body>)";
        var rawSplit = Regex.Split(data, pattern);
        string rawbody = rawSplit.GetValue(rawSplit.Length / 2).ToString();
        string body = rawbody.Substring(rawbody.IndexOf('>') + 1);
        body = body.Replace("\n", "").Replace("\n ", "").Replace("\r", "").Replace("\r ", "");

        Debug.Log("Body: " + body);

        if (result.Count > 0)
            return result;
        return null;
    }

    void RefeshUI()
    {
        
    }

}
