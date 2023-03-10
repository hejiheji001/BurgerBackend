namespace Image.API;

public class ImageSettings
{
    
    public string EventBusConnection { get; set; }
    public string ConnectionString { get; set; }
    public string OSSEndpoint { get; set; }
    public bool OSSSecure;
    public string OSSAccKey;
    public string OSSSecKey;
    public long fileSizeLimit { get; set; } = 1048576;
}