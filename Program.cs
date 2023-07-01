using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;

using Npgsql;

const string cfg = "app.cfg";

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

if (!File.Exists(cfg)){
	Console.WriteLine("Для работы необходим файл " + cfg);
}else{
	var config = new string[]{};
	try{
		config = (await File.ReadAllLinesAsync(cfg))//раз сказали await везде...
            .Where(line => line.FirstOrDefault() != '#' && !string.IsNullOrEmpty(line.Trim())).ToArray();
	}catch(Exception ex){
		Console.WriteLine("Ошибка чтения файла " + cfg + " текст ошибки:" + ex.ToString());
	}
	if (config.Count() < 4){
		Console.WriteLine(cfg + " должен содержать url нашего сервиса, url API и описание одного или более поля результата");	
	}else{
		var dbConfig = config.First();
		var ourUrl = config.Skip(1).First();
		var url = config.Skip(2).First();
		var mappings = new Dictionary<string, Mapping>();
		try{	
			mappings = config.Skip(3).Select( line => new Mapping(line) ).ToDictionary( item => item.JsonField);
		}catch(Exception ex){
			Console.WriteLine("Ошибка чтения конфигурации полей: " + ex.ToString());	
		}

		app.MapGet("/", async (HttpContext context) => {
            context.Response.StatusCode = 405;
            await context.Response.WriteAsync("Use POST, not GET");
        });
		app.MapPost("/", 
			async (HttpContext context) => {
				var client = new HttpClient();
				JsonDocument doc = null;
				int itemCount = 0;
				try{
					var data = await client.GetStreamAsync(url);
					doc = await JsonDocument.ParseAsync(data);
				}catch(Exception ex){
					context.Response.StatusCode = 500;
					await context.Response.WriteAsync("Ошибка вызова API, проверьте страницу " + url + " в браузере. Текст ошибки: " + ex.ToString());
				}
				try
				{
					itemCount = Query.Insert(mappings, doc, dbConfig);
				}
				catch (Exception ex)
				{
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Ошибка обращения к SQL серверу, проверьте доступность сервера, правильность конфигурации и отображения полей " + dbConfig + ". Текст ошибки: " + ex.ToString());
                }
                await context.Response.WriteAsync("В базу добавлено " + itemCount.ToString());
			}
		);

		if (mappings.Keys.Count > 0){
			app.Run(ourUrl);
		}
		else
		{
            Console.WriteLine("Сконфигурировано ноль полей");
        }
    }
}
