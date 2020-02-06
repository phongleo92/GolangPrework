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
    static string TAG_TITLE = "<title>|</title>";
    static string TAG_DATE = "\"Date\"";
    static string TAG_AUTHOR = "\"ReferenceSourceTG\"";

    [Serializable]
    public class Poster
    {
        public string Url, Title, Author, DatePublish;

        public Poster(string Url, string Title, string Author, string DatePublish)
        {
            this.Url = Url;
            this.Title = Title;
            this.Author = Author;
            this.DatePublish = DatePublish;
        }
    }
    #endregion

    #region REF
    [SerializeField] Text TextURL;
    [SerializeField] Transform UIContent;
    [SerializeField] Transform UIElement;
    #endregion

    #region CACHE
    public List<Poster> CachePoster = new List<Poster>();
    Queue<string> CacheLinks = new Queue<string>();
    List<string> CacheRequestedLinks = new List<string>();
    bool m_IsLoadedOriginPage;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        m_IsLoadedOriginPage = false;
        UIElement.gameObject.SetActive(false);
        TextURL.text = string.Format("URL: {0}", URL);

        CacheLinks.Enqueue(URL);
        StartCoroutine(SearchPosterFromURL());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    IEnumerator SearchPosterFromURL()
    {
        while (CacheLinks.Count > 0)
        {
            // Get a link
            string url = CacheLinks.Dequeue();

            // Get content from link
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();
            Debug.LogWarning("Request url count >>>");
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                string content = www.downloadHandler.text;
                //Debug.LogWarning("Data: " + content);

                // Parse content
                if (!string.IsNullOrEmpty(content))
                    ParseContent(url, content);

            }


        }


        Debug.Log("Parse Data Done !!!");
    }

    void ParseContent(string url, string data)
    {
        var rawSplit = Regex.Split(data, "href=\"");
        List<string> hrefSplit = new List<string>();

        if (m_IsLoadedOriginPage)
        {
            // Get detail page
            Poster poster = GetDetailPage(url, data);

            if(poster != null)
                CachePoster.Add(poster);
        }

        foreach (string item in rawSplit)
        {
            string alink = item.Substring(0, item.IndexOf("\""));
            if (alink.IndexOf(".html") >= 0 && alink.IndexOf("https") < 0)
            {
                // Compare url requested
                if (!CacheRequestedLinks.Contains(alink))
                {
                    // Save the path to make sure not to call only 2 times for 1 link
                    CacheRequestedLinks.Add(alink);

                    // Add to the request queues
                    CacheLinks.Enqueue(URL + alink);
                }

            }
        }
        rawSplit = null;

        m_IsLoadedOriginPage = true;


    }

    Poster GetDetailPage(string url, string data)
    {
        var rawDetail = Regex.Split(data, TAG_TITLE);
        string title = rawDetail.Length > 1 ? rawDetail.GetValue(1).ToString().Replace("/r", "").Replace("/n", "").Trim() : "";

        string date = "";
        rawDetail = Regex.Split(data, TAG_DATE);
        if (rawDetail.Length > 1)
        {
            date = rawDetail.GetValue(1).ToString();
            date = date.Substring(1, date.IndexOf("<") - 1);
            date = date.Replace("&nbsp;", " ");
        }

        string author = "";
        rawDetail = Regex.Split(data, TAG_AUTHOR);
        if (rawDetail.Length > 1)
        {
            author = rawDetail.GetValue(1).ToString();
            author = author.Substring(1, author.IndexOf("<") - 1);
        }

        if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(date) && !string.IsNullOrEmpty(author))
            return new Poster(url, title, author, date);
        return null;
    }

    void RefeshUI()
    {

    }

}
