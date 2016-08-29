using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

namespace web_log_dot_net
{
    public class WebLog
    {

        public enum LogLevel
        {
            Info,
            Warning,
            Error,
            Critical
        }


        private WebServer ws;
        //paths
        private static string htmlTempFolder = Environment.CurrentDirectory + "\\tmp";
        private static string htmlFilePath = htmlTempFolder + "\\index.html";
        private static string cssFilePath = htmlTempFolder + "\\main.css";

        //paths


        private int webLogPort;
        private string htmlRefreshRate;
        private string htmlTitle;
        private string htmlHead = @"<html>
                                    <head>
                                        <meta http-equiv='refresh' content='%refresh%'/>
                                        <meta charset='UTF-8'/>
                                        <link rel='stylesheet' href='main.css'>
                                        <link rel='stylesheet' href='https://maxcdn.bootstrapcdn.com/font-awesome/4.6.3/css/font-awesome.min.css'>
                                        <link rel='stylesheet' href='https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css'>
                                        <script src='https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js'></script>
                                    </head>";
        private string htmlBody = @"<body>
                                        <div class='container-fluid'>
                                            <div class='row title'>
                                                <div class='col-md-12 col-sm-12 col-xs-12'><h1>%title%</h1></div>
                                            </div>
                                        <!-- weblogstart -->
                                        
                                        <!-- weblogend -->
                                        </div>
                                    </body></html>";
        private string htmlLogLineTemplate = "<div class='row %errorLevel%'><i class='%icon%'></i><div class='errorName'>%errorLevel%:</div> " + DateTime.Now.ToString() + " - %logMessage%</div>";


        private string basicCSS = @"
                                    * {
                                        padding: 0;
                                        margin: 0;
                                    }
                                    h1 {
                                        background-color: #ccc;
                                        border: 1px solid #999;
                                        text-align: center;
                                        text-transform: uppercase;
                                        text-decoration: underline;
                                    }
                                    .row {
                                        padding: 5px 10px;
                                        font-size: 14px;
                                        border: 1px solid #999;
                                        border-top: none;
                                        font-weight: bold;
                                    }
                                    .info {
                                        background-color: #dff0d8;
                                    }
                                    .warning {
                                        background-color: #fcf8e3;
                                    }
                                    .error {
                                        background-color: #f2dede;
                                    }
                                    .critical {
                                        background-color: #a73232;
                                    }
                                    .errorName {
                                        min-width: 90px;
                                        max-width: 90px;
                                        width: 90px;
                                        display: inline-flex;
                                        padding-left: 5px;
                                    }
                                    i {
                                        padding: 0 5px;
                                    }
                                    ";



        public WebLog(string webLogTitle = "WebLog", int refreshRate = 3, int port = 8844)
        {
            webLogPort = port;
            htmlTitle = webLogTitle;
            htmlRefreshRate = refreshRate.ToString();
            
            init();
            ws = new WebServer(htmlTempFolder, webLogPort);

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(cleanUp);
        }

        

        /// <summary>
        /// Create required folder, html and basic CSS file file.
        /// </summary>
        private void init()
        {
            Directory.CreateDirectory(htmlTempFolder);
            string finalHTML = htmlHead.Replace("%refresh%", htmlRefreshRate) + htmlBody.Replace("%title%", htmlTitle);
            using(StreamWriter writer = new StreamWriter(htmlFilePath))
            {
                writer.Write(finalHTML);
            }
            using (StreamWriter writer = new StreamWriter(cssFilePath))
            {
                writer.Write(basicCSS);
            }
        }


        /// <summary>
        /// Delete all created files while working before application exit
        /// </summary>
        private void cleanUp(object sender, EventArgs e)
        {
            ws.Stop();
            Directory.Delete(htmlTempFolder, true);
        }

        public void write(string message, LogLevel level = LogLevel.Info, bool stopProcess = false)
        {
            string icon;

            switch (level)
            {
                case LogLevel.Info:
                    icon = "fa fa-info-circle";
                    break;
                case LogLevel.Warning:
                    icon = "fa fa-exclamation-triangle";
                    break;
                case LogLevel.Error:
                    icon = "fa fa-exclamation-circle";
                    break;
                case LogLevel.Critical:
                    icon = "fa fa-times-circle";
                    break;
                default:
                    icon = "fa fa-info-circle";
                    break;
            }

            string finalLine = htmlLogLineTemplate.Replace("%errorLevel%", level.ToString()).Replace("%logMessage%", message).Replace("%icon%", icon);
            string currentFileContent, bodyFirstPart, bodyLastPart;

            using(StreamReader reader = new StreamReader(htmlFilePath))
            {
                currentFileContent = reader.ReadToEnd();
            }

            bodyFirstPart = currentFileContent.Substring(0, currentFileContent.IndexOf("<!-- weblogend -->"));
            bodyLastPart = currentFileContent.Substring(currentFileContent.IndexOf("<!-- weblogend -->"), currentFileContent.Length - bodyFirstPart.Length);

            using(StreamWriter writer = new StreamWriter(htmlFilePath))
            {
                writer.Write(bodyFirstPart + finalLine + bodyLastPart);
            }
            if (stopProcess)
            {
                Environment.Exit(0);
            }

        }


       

    }









    /// <summary>
    /// Mini Webserver Class to serve the webLog
    /// ********* SPECIAL THANKS TO <<<< aksakalli >>>> https://gist.github.com/aksakalli/9191056 for his simple webserver code
    /// </summary>
    class WebServer
    {
        private readonly string[] _indexFiles = {
        "index.html",
        "index.htm",
        "default.html",
        "default.htm"
    };

        private static IDictionary<string, string> _mimeTypeMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
        #region extension to MIME type list
        {".asf", "video/x-ms-asf"},
        {".asx", "video/x-ms-asf"},
        {".avi", "video/x-msvideo"},
        {".bin", "application/octet-stream"},
        {".cco", "application/x-cocoa"},
        {".crt", "application/x-x509-ca-cert"},
        {".css", "text/css"},
        {".deb", "application/octet-stream"},
        {".der", "application/x-x509-ca-cert"},
        {".dll", "application/octet-stream"},
        {".dmg", "application/octet-stream"},
        {".ear", "application/java-archive"},
        {".eot", "application/octet-stream"},
        {".exe", "application/octet-stream"},
        {".flv", "video/x-flv"},
        {".gif", "image/gif"},
        {".hqx", "application/mac-binhex40"},
        {".htc", "text/x-component"},
        {".htm", "text/html"},
        {".html", "text/html"},
        {".ico", "image/x-icon"},
        {".img", "application/octet-stream"},
        {".iso", "application/octet-stream"},
        {".jar", "application/java-archive"},
        {".jardiff", "application/x-java-archive-diff"},
        {".jng", "image/x-jng"},
        {".jnlp", "application/x-java-jnlp-file"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".mml", "text/mathml"},
        {".mng", "video/x-mng"},
        {".mov", "video/quicktime"},
        {".mp3", "audio/mpeg"},
        {".mpeg", "video/mpeg"},
        {".mpg", "video/mpeg"},
        {".msi", "application/octet-stream"},
        {".msm", "application/octet-stream"},
        {".msp", "application/octet-stream"},
        {".pdb", "application/x-pilot"},
        {".pdf", "application/pdf"},
        {".pem", "application/x-x509-ca-cert"},
        {".pl", "application/x-perl"},
        {".pm", "application/x-perl"},
        {".png", "image/png"},
        {".prc", "application/x-pilot"},
        {".ra", "audio/x-realaudio"},
        {".rar", "application/x-rar-compressed"},
        {".rpm", "application/x-redhat-package-manager"},
        {".rss", "text/xml"},
        {".run", "application/x-makeself"},
        {".sea", "application/x-sea"},
        {".shtml", "text/html"},
        {".sit", "application/x-stuffit"},
        {".swf", "application/x-shockwave-flash"},
        {".tcl", "application/x-tcl"},
        {".tk", "application/x-tcl"},
        {".txt", "text/plain"},
        {".war", "application/java-archive"},
        {".wbmp", "image/vnd.wap.wbmp"},
        {".wmv", "video/x-ms-wmv"},
        {".xml", "text/xml"},
        {".xpi", "application/x-xpinstall"},
        {".zip", "application/zip"},
        #endregion
    };
        private Thread _serverThread;
        private string _rootDirectory;
        private HttpListener _listener;
        private int _port;

        public int Port
        {
            get { return _port; }
            private set { }
        }

        /// <summary>
        /// Construct server with given port.
        /// </summary>
        /// <param name="path">Directory path to serve.</param>
        /// <param name="port">Port of the server.</param>
        public WebServer(string path, int port)
        {
            this.Initialize(path, port);
        }

    
        /// <summary>
        /// Stop server and dispose all functions.
        /// </summary>
        public void Stop()
        {
            _serverThread.Abort();
            _listener.Stop();
        }

        private void Listen()
        {
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add("http://*:" + _port.ToString() + "/");
                _listener.Start();
            }catch(Exception e)
            {
                MessageBox.Show("The selected port is currently in use, please select another.", "Port in use", MessageBoxButtons.OK);
                Environment.Exit(0);
            }
            while (true)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();
                    Process(context);
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void Process(HttpListenerContext context)
        {
            string filename = context.Request.Url.AbsolutePath;
            //Console.WriteLine(filename);
            filename = filename.Substring(1);

            if (string.IsNullOrEmpty(filename))
            {
                foreach (string indexFile in _indexFiles)
                {
                    if (File.Exists(Path.Combine(_rootDirectory, indexFile)))
                    {
                        filename = indexFile;
                        break;
                    }
                }
            }

            filename = Path.Combine(_rootDirectory, filename);

            if (File.Exists(filename))
            {
                try
                {
                    Stream input = new FileStream(filename, FileMode.Open);

                    //Adding permanent http response headers
                    string mime;
                    context.Response.ContentType = _mimeTypeMappings.TryGetValue(Path.GetExtension(filename), out mime) ? mime : "application/octet-stream";
                    context.Response.ContentLength64 = input.Length;
                    context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                    context.Response.AddHeader("Last-Modified", System.IO.File.GetLastWriteTime(filename).ToString("r"));

                    byte[] buffer = new byte[1024 * 16];
                    int nbytes;
                    while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                        context.Response.OutputStream.Write(buffer, 0, nbytes);
                    input.Close();

                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.OutputStream.Flush();
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }

            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }

            context.Response.OutputStream.Close();
        }

        private void Initialize(string path, int port)
        {
            this._rootDirectory = path;
            this._port = port;
            _serverThread = new Thread(this.Listen);
            _serverThread.Start();
        }
    }
}
