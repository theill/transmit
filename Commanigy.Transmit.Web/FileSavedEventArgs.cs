using System;
using System.Collections.Generic;
using System.Text;

namespace JumploaderWrapper
{
  public class UploadEventArgs : EventArgs
  {
    private int _filesUploaded;
    private int _successful;
    private int _failed;
    private string _userIPAddress;

    public UploadEventArgs(string ipAddress, int uploaded, int successful, int failed)
    {
      _filesUploaded = uploaded;
      _successful = successful;
      _failed = failed;
      _userIPAddress = ipAddress;
    }

    public string IPAddress
    {
      get { return _userIPAddress; }
    }

    public int TotalInUpload
    {
      get { return _filesUploaded; }
    }

    public int TotalSuccessful
    {
      get { return _successful; }
    }

    public int TotalFailed
    {
      get { return _failed; }
    }
  }

  public class FileSavedEventArgs : EventArgs 
  {
    private string _fileName;
    private long _length;
    private byte[] _data;

    public FileSavedEventArgs(string fileName, long length, byte[] data)
    {
      _fileName = fileName;
      _length = length;
      _data = data;
    }

    public string FileName
    {
      get { return _fileName; }
    }

    public long Length
    {
      get { return _length; }
    }

    public byte[] Data
    {
      get { return _data; }
    }
  }
}
