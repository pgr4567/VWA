using System.IO;
using System.Net;

public static class Helpers {
    public static string Get (string uri) {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create (uri);

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse ())
        using (Stream stream = response.GetResponseStream ())
        using (StreamReader reader = new StreamReader (stream)) {
            return reader.ReadToEnd ();
        }
    }
}