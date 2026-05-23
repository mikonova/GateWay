using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
        //[Fact]


       }
}
