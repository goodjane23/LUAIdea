using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace LUAIdea.Services
{
    class ApiCommandServices
    {
        //ObservableCollection<> list;
        internal static async Task<string> NetLoadHttp(string s)
        {
            var rez = string.Empty;

            try
            {
                using var client = new HttpClient();
                rez = await client.GetStringAsync(s);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }

            return rez;
        }

        internal static async Task GetAlldescriptionFromHttp()
        {
            var base_http = @"http://doc.pumotix.ru/pages/viewpage.action?pageId=";

            var mas_base_http = new[]
            {
                base_http + "5180768", //in_out
                base_http + "5180766", //axis
                base_http + "5180780", //homing
                base_http + "5180770", //m6
                base_http + "5180778", //spindle
                base_http + "5180775", //plasma
                base_http + "5180773", //oxy
                base_http + "5182663", //modbus
                base_http + "5180782", //other
                base_http + "42959110", //in_out_fo
                base_http + "43843590", //axis_fo
                base_http + "43843598", //spindle_fo
                base_http + "43843603", //plasma_fo
                base_http + "43843606", //oxy_fo
                base_http + "43843608", //modbus_fo
                base_http + "43843610" //other_fo           
            };

            await using var f_new_base = new StreamWriter("f_new_base.txt");

            for (var i = 0; i < mas_base_http.Length; i++)
            {
                var http_page = await NetLoadHttp(mas_base_http[i]);
                var http_page_strings = http_page.Replace("<", "@").Split('@');

                for (var j = 0; j <= http_page_strings.Length - 1; j++)
                {
                    var s = http_page_strings[j];

                    if ((s.Contains(">bool") ||
                         s.Contains(">void") ||
                         s.Contains(">number") ||
                         s.Contains(">string") ||
                         s.Contains(">int")) &&
                         s.Contains("(") &&
                         s.Contains(")"))
                    {
                        var func = s.Substring(s.IndexOf(">")) + "\r";

                        var func_name = func.Replace(">bool", "")
                            .Replace(">void", "")
                            .Replace(">number", "")
                            .Replace(">string", "")
                            .Replace(">int", "");

                        func_name = func_name.Substring(1);

                        if (http_page_strings[j + 2].Contains("p>"))
                        {
                            var func_note = http_page_strings[j + 2];
                            func_note = func_note.Substring(func_note.IndexOf(">") + 1);

                            var func_out = $"{func_name.Replace("\r", "")}@{func_note}";
                            var name_type = mas_base_http[i].Substring(mas_base_http[i].LastIndexOf("=") + 1);

                            func_out = $"{name_type}@{func_out}";
                            await f_new_base.WriteLineAsync(func_out);
                        }
                    }
                }
            }

            f_new_base.Close();
            //await LoadFromFile();
        }
    }
}
