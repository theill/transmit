using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.IO;

namespace JumploaderWrapper
{
    public class FileSystemFileSaver: IFileSaver
    {
        private const string DEFAULT_EXTENSION = "({0})";

        public string BaseDirectory;
        public bool appendFileName;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <param name="directory">Directory to save file ex. FileSave\\</param>
        public FileSystemFileSaver(HttpContext context, string directory)
        {
            init(context, directory, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <param name="directory">Directory to save file ex. FileSave\\</param>
        /// <param name="AppendFileName">Append the original file name (Useful when using scaledInstanceNames and no zipping)</param>
        public FileSystemFileSaver(HttpContext context, string directory, bool AppendFileName)
        {
            init(context, directory, AppendFileName);
        }

        private void init(HttpContext context, string directory, bool AppendFileName)
        {
			BaseDirectory = Path.Combine(directory, context.Request["token"]);
			Directory.CreateDirectory(BaseDirectory);

            appendFileName = AppendFileName;
        }

        #region IFileSaver Members

        public bool SaveFile(string getFileName, byte[] data)
        {
            string fileName = getFileName;
			if (appendFileName && (HttpContext.Current.Request.Form["fileName"] + "" != getFileName)) {
				fileName = getFileName + HttpContext.Current.Request.Form["fileName"];
			}

            fileName = GetUniqueName(fileName);
            string origFileName = fileName;

            int partitions = 1;
            Int32.TryParse(HttpContext.Current.Request.Form["partitionCount"] +"", out partitions);
            int partitionIndex = -1;
            Int32.TryParse(HttpContext.Current.Request.Form["partitionIndex"] + "", out partitionIndex);

            if (partitions > 1)
            {
                fileName = getParitionedName(partitionIndex,fileName);
            }

            FileStream f = new FileStream(Path.Combine(BaseDirectory, fileName), FileMode.CreateNew);
            f.Write(data, 0, data.Length);
            f.Close();

            if ((partitions-1) == partitionIndex) //last one done, combine
            {
                System.Threading.Thread.Sleep(500); //just in case
                FileStream combined = File.Open(Path.Combine(BaseDirectory, origFileName), FileMode.Append);

                for (int part = 0; part < partitions; part++)
                {
					string filePath = Path.Combine(BaseDirectory, getParitionedName(part, origFileName));
                    if (File.Exists(filePath))
                    {
                        f = File.Open(filePath, FileMode.Open);
                        byte[] content = new byte[f.Length];
                        f.Read(content, 0, (int)f.Length);
                        combined.Write(content, 0, (int)f.Length);
                        f.Close();
                        File.Delete(filePath);
                    }
                }
                combined.Close();
            }

            return true;
        }

        private string getParitionedName(int getIndex, string getName)
        {
            return "P" + HttpContext.Current.Request.Form["fileId"] + "_" + getIndex + "_" + getName;
        }

        public string GetUniqueName(string fileName)
        {
            string originalName = fileName.Substring(0, fileName.LastIndexOf("."));
            string originalExt = fileName.Substring(fileName.LastIndexOf(".") + 1);

            string returnName = fileName;
            int increment = 1;
            while (File.Exists(Path.Combine(BaseDirectory, returnName)))
            {
                returnName = originalName + string.Format(DEFAULT_EXTENSION, increment++) + "." + originalExt;
            }

            return returnName;
        }

        #endregion
    }
}
