using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    #region DEF
    static string URL = "https://www.thesaigontimes.vn";
    static string FILE_NAME = "GolangPreworkData.csv";
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
    [Serializable]
    public enum EventButton
    {
        Search,
        DoneSearch
    }
    #endregion

    #region REF
    [SerializeField] Text TextTitle, TextLog, TextButton;
    [SerializeField] Transform UIContent;
    [SerializeField] Button ButtonAction;
    #endregion

    #region CACHE
    public List<Poster> CachePoster = new List<Poster>();
    Queue<string> CacheLinks = new Queue<string>();
    List<string> CacheRequestedLinks = new List<string>();
    bool m_IsLoadedOriginPage;
    bool m_BreakQueue;
    EventButton m_EventButton;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        TextTitle.text = string.Format("GOLANG PREWORK - {0}", URL);
        m_EventButton = EventButton.Search;
    }

    void DefaultValue()
    {
        CachePoster.Clear();
        CacheLinks.Clear();
        CacheRequestedLinks.Clear();
        m_IsLoadedOriginPage = false;
        m_BreakQueue = false;
        TextLog.text = string.Format("Seaching...");
        CacheLinks.Enqueue(URL);
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
        while (CacheLinks.Count > 0 && !m_BreakQueue)
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

                TextLog.text = string.Format("Found: {0} item", CachePoster.Count);

            }

        }

        string pathData = ExportCSV();

        TextLog.text = string.Format("Process complete !\n File Path: {0}", pathData);
        Debug.Log(TextLog.text);

        ButtonAction.enabled = true;
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
        else
        {
            ButtonAction.enabled = true;
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
            date = date.Replace(",", " -");
            date = date.Replace("  ", " ");
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

    string ExportCSV()
    {
        string path = string.Format("{0}/{1}", Application.persistentDataPath, FILE_NAME);
        string exportData = "Golang Prework Data";
        foreach (var poster in CachePoster)
        {
            exportData += string.Format("\n{0}, {1}, {2}, {3}", poster.Url, poster.Title, poster.Author, poster.DatePublish);
        }
        StreamWriter writer = new StreamWriter(path, false);
        writer.Write(exportData);
        writer.Close();
        return path;
    }

    #region Event for buttons
    public void ButtonEvent()
    {
        switch (m_EventButton)
        {
            case EventButton.Search:
                {
                    DefaultValue();
                    StartCoroutine(SearchPosterFromURL());
                    m_EventButton = EventButton.DoneSearch;
                    TextButton.text = "Break Queue";
                }
                break;
            case EventButton.DoneSearch:
                {
                    m_BreakQueue = true;
                    m_EventButton = EventButton.Search;
                    TextButton.text = "Start Search";
                }
                break;
        }
        ButtonAction.enabled = false;
    }
    #endregion

}
