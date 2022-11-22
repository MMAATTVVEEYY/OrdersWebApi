
using OrdersClient;
using System.Net.Http.Headers;
using System.Net.Http.Json;

HttpClient client = new HttpClient();
client.BaseAddress = new Uri("http://localhost:12763");
client.DefaultRequestHeaders.Accept.Clear();
client.DefaultRequestHeaders.Add("Accept", "application/json");





while (true){
    //вот тут надо ждать запроса от апишки ордера
    HttpResponseMessage responce = await client.GetAsync("api/Item");
    responce.EnsureSuccessStatusCode();
    if (responce.IsSuccessStatusCode)
    {
        var items = await responce.Content.ReadFromJsonAsync<IEnumerable<ItemDTO>>();
        foreach (var item in items) {
            Console.WriteLine(item.Name);
            Console.WriteLine(item.Price);
            Console.WriteLine();
        }
    }
    else
    {
        Console.WriteLine("No Content");
    }
}



