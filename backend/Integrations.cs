using System.Security.Cryptography;
using System.Text;
using System.Net.Http.Json;
using LeadPilot;
public sealed class Integrations(IConfiguration config,IHttpClientFactory clients)
{
 byte[] Key=>SHA256.HashData(Encoding.UTF8.GetBytes(config["ENCRYPTION_KEY"]??throw new InvalidOperationException("ENCRYPTION_KEY required")));
 public string Encrypt(string text){var nonce=RandomNumberGenerator.GetBytes(12);var tag=new byte[16];var data=Encoding.UTF8.GetBytes(text);var cipher=new byte[data.Length];using var aes=new AesGcm(Key,16);aes.Encrypt(nonce,data,cipher,tag);return Convert.ToBase64String([..nonce,..tag,..cipher]);}
 public string Decrypt(string text){var bytes=Convert.FromBase64String(text);using var aes=new AesGcm(Key,16);var clear=new byte[bytes.Length-28];aes.Decrypt(bytes[..12],bytes[28..],bytes[12..28],clear);return Encoding.UTF8.GetString(clear);}
 public async Task Send(Channel channel,string externalId,string body)
 {
  if(config.GetValue<bool>("DEMO_MODE"))return;
  var token=Decrypt(channel.SecretEncrypted);var client=clients.CreateClient();HttpResponseMessage response;
  if(channel.Kind=="telegram")response=await client.PostAsJsonAsync($"https://api.telegram.org/bot{token}/sendMessage",new{chat_id=externalId,text=body});
  else if(channel.Kind=="vk")response=await client.PostAsync("https://api.vk.com/method/messages.send",new FormUrlEncodedContent(new Dictionary<string,string>{{"access_token",token},{"v","5.199"},{"peer_id",externalId},{"random_id",RandomNumberGenerator.GetInt32(1,int.MaxValue).ToString()},{"message",body}}));
  else if(channel.Kind=="max"){client.DefaultRequestHeaders.Authorization=new("Bearer",token);response=await client.PostAsJsonAsync($"https://platform-api.max.ru/messages?chat_id={Uri.EscapeDataString(externalId)}",new{text=body});}
  else throw new InvalidOperationException("Unknown channel");
  response.EnsureSuccessStatusCode();
 }
 public async Task SyncLead(Lead lead,Contact contact,string system)
 {
  var client=clients.CreateClient();HttpResponseMessage response;
  if(system=="amocrm"){
   var domain=config["AMO_DOMAIN"]??throw new InvalidOperationException("AMO_DOMAIN required");if(!domain.EndsWith(".amocrm.ru")||domain.Contains('/'))throw new InvalidOperationException("Invalid amoCRM domain");client.DefaultRequestHeaders.Authorization=new("Bearer",config["AMO_ACCESS_TOKEN"]??throw new InvalidOperationException("AMO_ACCESS_TOKEN required"));response=await client.PostAsJsonAsync($"https://{domain}/api/v4/leads",new[]{new{name=lead.Name,price=lead.Value}});
  }else if(system=="bitrix"){
   var url=config["BITRIX_WEBHOOK_URL"]??throw new InvalidOperationException("BITRIX_WEBHOOK_URL required");if(!url.StartsWith("https://")||!url.Contains(".bitrix24."))throw new InvalidOperationException("Invalid Bitrix webhook");response=await client.PostAsJsonAsync(url.TrimEnd('/')+"/crm.lead.add.json",new{fields=new{TITLE=lead.Name,OPPORTUNITY=lead.Value,NAME=contact.Name,PHONE=new[]{new{VALUE=contact.Phone,VALUE_TYPE="WORK"}}}});
  }else throw new InvalidOperationException("Unknown CRM");response.EnsureSuccessStatusCode();
 }
}
