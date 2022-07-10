using Newtonsoft.Json;

namespace Spotiloader.Config;

public class SpotifyApplication
{
    [JsonProperty(PropertyName = "ClientId", Required = Required.Always)]
    public string ClientId { get; set; } = "";
    
    [JsonProperty(PropertyName = "ClientSecret", Required = Required.Always)]
    public string ClientSecret { get; set; } = "";
}