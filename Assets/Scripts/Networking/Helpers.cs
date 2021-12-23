using System.IO;
using System.Net;
using System;

namespace Networking {
    public static class Helpers {
        public static string Get (string uri) {
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create (uri);
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse ())
                using (Stream stream = response.GetResponseStream ())
                using (StreamReader reader = new StreamReader (stream)) {
                    return reader.ReadToEnd ();
                }
            }
            catch (Exception) {
                return "";
            }
        }
    }
}