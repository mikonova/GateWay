using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text;

namespace CoreClasses.Tests
{
    public class TemplateTestes
    {
        private readonly Templates tmpl;
        public TemplateTestes()
        {
            tmpl = new Templates("C:\\Users\\Family\\Desktop\\GateWayTests");
        }

        [Fact]
        public void TryToLoad()
        {
            try
            {
                tmpl.LoadAllChats();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Упало с ошибкой: {ex.Message}");
            }


        }
        [Fact]
        public async Task Register_ShouldReturnSuccess()
        {
            var http = new HttpClient();
            var body = JsonSerializer.Serialize(new { nickname = "testuser", password = "testpass123" });

            var response = await http.PostAsync("http://192.168.43.151:8000/register",
                new StringContent(body, Encoding.UTF8, "application/json"));

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Статус: {response.StatusCode}");
            Console.WriteLine($"Ответ: {content}");

            Assert.True(response.IsSuccessStatusCode);
        }

    }
}
