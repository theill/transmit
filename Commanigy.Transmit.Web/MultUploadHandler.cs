using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace JumploaderWrapper
{
    public delegate void MFileEventHandler(object sender, FileSavedEventArgs args);
    public delegate void MUploadEventHandler(object sender, UploadEventArgs args);

    /// <summary>
    /// Base class for File or Sql Based uploads
    /// </summary>
    public class MultUploadHandler : IHttpHandler
    {
        private static List<string> MASTERDISABLED = new List<string> {};

		// use filter below if you do not want to allow certain type of files
		//private static List<string> MASTERDISABLED = new List<string> { ".aspx", ".asp", ".asax", ".ascx", ".htm", ".html", ".config", ".css", ".cs", ".vb", ".pl", ".js", ".php", ".dll" };

        public static List<string> disabledFileTypes = MASTERDISABLED;

        public MultUploadHandler(IFileSaver getfilesaver)
        {
            FileSaver = getfilesaver;
        }

        private IFileSaver _fileSaver = null;
        private event MFileEventHandler _fileSavedEvent;
        private event MUploadEventHandler _uploadedEvent;

        public event MUploadEventHandler UploadComplete
        {
            add { _uploadedEvent += value; }
            remove { _uploadedEvent -= value; }
        }

        public event MFileEventHandler FileSaved
        {
            add { _fileSavedEvent += value; }
            remove { _fileSavedEvent -= value; }
        }

        public IFileSaver FileSaver
        {
            get { return _fileSaver; }
            set { _fileSaver = value; }
        }

        private HttpRequest _request;
        private HttpResponse _response;

        public HttpRequest Request
        {
            get { return _request; }
        }

        public HttpResponse Response
        {
            get { return _response; }
        }

        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            _request = context.Request;
            _response = context.Response;

            if (context.Request.Files.Count <= 0)
            {
                _response.ContentType = "text/plain";
                _response.Write("NO FILE!");

                return;
            }

            SaveFiles();

            _response.ContentType = "text/plain";
            _response.Write("Upload OK");
        }

        protected void SaveFiles()
        {
            int failed = 0;
            int successful = 0;
            int total = _request.Files.Count;
            string ipAddress = _request.UserHostAddress;

            for (int i = 0; i < _request.Files.Count; ++i)
            {
                HttpPostedFile file = _request.Files[i];

                string extension = file.FileName.Substring(file.FileName.LastIndexOf("."));
                if (disabledFileTypes.Contains(extension))
                {
                    failed += 1;
                    continue;
                }

                try
                {
                    byte[] fileTemp = new byte[file.InputStream.Length];
                    file.InputStream.Read(fileTemp, 0, (int)file.InputStream.Length);

					if (FileSaver != null) {
						FileSaver.SaveFile(file.FileName, fileTemp);
					}

					if (_fileSavedEvent != null) {
						_fileSavedEvent(this, new FileSavedEventArgs(file.FileName, file.ContentLength, fileTemp));
					}

                    successful += 1;
                }
                catch
                {
                    failed += 1;
                }
            }

			if (_uploadedEvent != null) {
				_uploadedEvent(this, new UploadEventArgs(ipAddress, total, successful, failed));
			}
        }

        #endregion
    }
}
