using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FakeDevice
{
    public class Update
    {
        public Update()
        {
            Version = "1.0";
        }

        public string Script { get; set; }
        public string Version { get; set; }


        public class GlobalsAlarmAsync
        {
            public int value { get; set; }
            public int threshold { get; set; }
        }

        public async Task<string> AlarmAsync(int value, int threshold)
        {
            if (string.IsNullOrWhiteSpace(Script))
            {
                return "normal";
            }

            return await CSharpScript.EvaluateAsync<string>(Script, globals: new GlobalsAlarmAsync
            {
                threshold = threshold,
                value = value
            });
        }
    }
}
