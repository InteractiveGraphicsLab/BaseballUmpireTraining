using System;
using UnityEngine;

[Serializable]
public class Namespace {
    [SerializeField] public int id;
    [SerializeField] public string text;
}

[Serializable]
public class Titles {
    [SerializeField] public string canonical;
    [SerializeField] public string normalized;
    [SerializeField] public string display;
}

[Serializable]
public class Thumbnail {
    [SerializeField] public string source;
    [SerializeField] public int width;
    [SerializeField] public int height;
}

[Serializable]
public class Originalimage {
    [SerializeField] public string source;
    [SerializeField] public int width;
    [SerializeField] public int height;
}

[Serializable]
public class Desktop {
    [SerializeField] public string page;
    [SerializeField] public string revisions;
    [SerializeField] public string edit;
    [SerializeField] public string talk;
}

[Serializable]
public class Mobile {
    [SerializeField] public string page;
    [SerializeField] public string revisions;
    [SerializeField] public string edit;
    [SerializeField] public string talk;
}

[Serializable]
public class ContentUrls {
    [SerializeField] public Desktop desktop;
    [SerializeField] public Mobile mobile;
}

[Serializable]
public class WikipediaJsonResponse {
    [SerializeField] public string type;
    [SerializeField] public string title;
    [SerializeField] public string displaytitle;
    [SerializeField] public Namespace @namespace;
    [SerializeField] public string wikibase_item;
    [SerializeField] public Titles titles;
    [SerializeField] public int pageid;
    [SerializeField] public Thumbnail thumbnail;
    [SerializeField] public Originalimage originalimage;
    [SerializeField] public string lang;
    [SerializeField] public string dir;
    [SerializeField] public string revision;
    [SerializeField] public string tid;
    [SerializeField] public DateTime timestamp;
    [SerializeField] public string description;
    [SerializeField] public string description_source;
    [SerializeField] public ContentUrls content_urls;
    [SerializeField] public string extract;
    [SerializeField] public string extract_html;
}
