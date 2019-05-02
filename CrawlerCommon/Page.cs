using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Win32;
using System.Net;
using CrawlerCommon;
using CrawlerCommon.TagDef.StrictXHTML;

public class Page
{
    public bool DebugMode { get; set;}

    string _id = Guid.NewGuid().ToString();
    public string ReferenceID 
    {
        get
        {
            return _id;
        }
    }

    public bool TrimWhitespace { get; set; }

    List<string> _response = null;
    public void Load(string url)
    {
        this.Load(url, null);
    }
    public void Load(string url, string filename)
    {
        HtmlTokenizer tokenizer = new HtmlTokenizer() { TrimWhitespace = this.TrimWhitespace, DebugMode = false, ShowWorkMode = false };
        this.Headers = tokenizer.Load(url, filename);
        this._response = tokenizer.Tokens;
    }
    public void Load(FileInfo file)
    {
        HtmlTokenizer tokenizer = new HtmlTokenizer() { TrimWhitespace = this.TrimWhitespace, DebugMode = false, ShowWorkMode = false };
        tokenizer.Load(file);
        this._response = tokenizer.Tokens;
    }

    public string[] Tokens
    {
        get
        {
            return _response.ToArray();
        }
    }

    public Document GetTokenTree()
    {
        DefaultBuilder builder = new DefaultBuilder();
        return builder.ParsedTree;
    }

    public void Post(string url)
    {
        throw new NotImplementedException();

        // Create a request using a URL that can receive a post. 
        WebRequest request = WebRequest.Create(url);
        // Set the Method property of the request to POST.
        request.Method = "POST";
        // Create POST data and convert it to a byte array.
        string postData = "ctl00%24MainContent%24ddlReports=422&ctl00%24MainContent%24txtStartDateFilter=5%2F5%2F2012&ctl00%24MainContent%24txtEndDateFilter=11%2F5%2F2012";
        byte[] byteArray = Encoding.UTF8.GetBytes(postData);
        // Set the ContentType property of the WebRequest.
        request.ContentType = "application/x-www-form-urlencoded";
        request.Headers.Add("x-microsoftajax", "Delta=true");
        request.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; MS-RTC LM 8; InfoPath.1; .NET4.0C; .NET4.0E)");

        // Set the ContentLength property of the WebRequest.
        request.ContentLength = byteArray.Length;
        // Get the request stream.
        Stream dataStream = request.GetRequestStream();
        // Write the data to the request stream.
        dataStream.Write(byteArray, 0, byteArray.Length);
        // Close the Stream object.
        dataStream.Close();
        // Get the response.
        WebResponse response = request.GetResponse();
        // Display the status.
        Console.WriteLine(((HttpWebResponse)response).StatusDescription);
        // Get the stream containing content returned by the server.
        dataStream = response.GetResponseStream();
        // Open the stream using a StreamReader for easy access.
        StreamReader reader = new StreamReader(dataStream);
        // Read the content.
        string responseFromServer = reader.ReadToEnd();
        // Display the content.
        Console.WriteLine(responseFromServer);
        // Clean up the streams.
        reader.Close();
        dataStream.Close();
        response.Close();

        /*
        ctl00$MainContent$txtStartDateFilter
        ctl00$MainContent$txtEndDateFilter
        <option value="422" Class="ANDIDropdownItem">GSSi 4.9 Commit</option>
        ctl00$MainContent$ddlReports
        */


        //return string.Join("", _response.ToArray()); ;
    }

    public Dictionary<string, string> Headers { get; private set; }

    public string GetResponse(bool trimwhitespace)
    {
        string[] tokens = _response.ToArray();

        if (trimwhitespace)
        {
            for (int i = 0; i < tokens.Length; i++)
                tokens[i] = tokens[i].Replace("\n", " ").Replace("\r", " ").Trim();
            tokens = tokens.Where<string>(c => c.Length > 0).ToArray<string>();
        }
        
        return string.Join(" | ", tokens);
    }

    public readonly TokenCollection ChildElements = new TokenCollection();
    

    public string GetForms()
    {
        return null;
    }
}




//strict XHTML










