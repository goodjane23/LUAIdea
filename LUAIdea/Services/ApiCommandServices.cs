using LUAIdea.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace LUAIdea.Services
{
    class ApiCommandServices
    {
        private Dictionary<string, FunctionNodeModel> idFuncNodePairs;

        public ApiCommandServices()
        {
            idFuncNodePairs = new Dictionary<string, FunctionNodeModel>();
        }
        internal static async Task<string> NetLoadHttp(string s)
        {
            var rez = string.Empty;

            try
            {
                using var client = new HttpClient();
                rez = await client.GetStringAsync(s);
            }            
            catch { }

            return rez;
        }

        private static async Task GetAllDescriptionFromHttp()
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

            try
            {
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
            }
            catch (Exception ex)
            {
                var ttemp = ex.Data;
                throw;
            }

            f_new_base.Close();
        }

        private async Task FillFunctionNode()
        {

            using var reader = new StreamReader("f_new_base.txt");            
            
            while (!reader.EndOfStream)
            {               
                var s = await reader.ReadLineAsync();

                if (s == null) continue;
                var dataArray = s.Split('@');

                var funcModel = CreateFunction(dataArray);
               
                idFuncNodePairs.TryGetValue(dataArray[0], out var functionNode);

                if (functionNode is null)
                {
                    functionNode = new FunctionNodeModel();
                    idFuncNodePairs.Add(dataArray[0], functionNode);                   
                }

                if (dataArray[0][0] == '5') 
                {
                    switch (dataArray[0])
                    {
                        case "5180768":
                            functionNode.Header = "Входы и выходы";
                            break;
                        case "5180766":
                            functionNode.Header = "Оси";
                            break;
                        case "5180780":
                            functionNode.Header = "Базирование";
                            break;
                        case "5180770":
                            functionNode.Header = "Смена инструмента";
                            break;
                        case "5180778":
                            functionNode.Header = "Шпиндель";
                            break;
                        case "5180775":
                            functionNode.Header = "Плазма";
                            break;
                        case "5180773":
                            functionNode.Header = "Газокислород";
                            break;
                        case "5182663":
                            functionNode.Header = "ModBus";
                            break;
                        case "5180782":
                            functionNode.Header = "Другое";
                            break;

                        default:
                            break;
                    }
                    functionNode.Functions.Add(funcModel);
                }
                
                if (dataArray[0][0] == '4')
                {
                    switch (dataArray[0])
                    {
                        case "42959110":
                            functionNode.Header = "Входы и выходы";
                            break;
                        case "43843590":
                            functionNode.Header = "Оси";
                            break;
                        case "43843598":
                            functionNode.Header = "Шпиндель";
                            break;
                        case "43843603":
                            functionNode.Header = "Плазма";
                            break;
                        case "43843606":
                            functionNode.Header = "Газокислород";
                            break;
                        case "43843608":
                            functionNode.Header = "ModBus";
                            break;
                        case "43843610":
                            functionNode.Header = "Другие";
                            break;
                    }
                    functionNode.Functions.Add(funcModel);
                }
            }
        }

        public async Task<IEnumerable<FunctionNodeModel>> GetFunctionNode(int num)
        {
            if (num !=5 && num !=4) return null;

            var resultNode = new ObservableCollection<FunctionNodeModel>();
            await GetAllDescriptionFromHttp();
            await FillFunctionNode();            

            foreach (var item in idFuncNodePairs)
            {
                if (item.Key.StartsWith($"{num}"))
                    resultNode.Add(item.Value);
            }

            return resultNode;
        }

        private static MacroFunctionModel CreateFunction(string[] temp)
        {
            MacroFunctionModel fm = new MacroFunctionModel();
            fm.Desription = temp[2];
            fm.Name = temp[1];
            fm.Function = temp[1].Substring(0, temp[1].IndexOf('('));
            return fm;
        }
        
    }
}
