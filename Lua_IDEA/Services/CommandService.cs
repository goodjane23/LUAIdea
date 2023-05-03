using Lua_IDEA.Data;
using Lua_IDEA.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Lua_IDEA.Services;

public class CommandService
{
    private const string BaseUrl = "http://doc.pumotix.ru/pages/viewpage.action?pageId=";

    private readonly Dictionary<string, string> commandCategories;

    private readonly NetworkChecker networkChecker;
    private readonly IDbContextFactory<AppDbContext> contextFactory;

    public CommandService(
        NetworkChecker networkChecker,
        IDbContextFactory<AppDbContext> contextFactory)
    {
        this.networkChecker = networkChecker;
        this.contextFactory = contextFactory;

        commandCategories = new Dictionary<string, string>()
        {
            { "5180768", "Входы и выходы" },
            { "5180766", "Оси" },
            { "5180780", "Базирование" },
            { "5180770", "Смена инструмента" },
            { "5180778", "Шпиндель" },
            { "5180775", "Плазма" },
            { "5180773", "Газокислород" },
            { "5182663", "ModBus" },
            { "5180782", "Другое" },
            { "42959110", "Входы и выходы" },
            { "43843590", "Оси" },
            { "43843598", "Шпиндель" },
            { "43843603", "Плазма" },
            { "43843606", "Газокислород" },
            { "43843608", "ModBus" },
            { "43843610", "Другие" },
        };
    }

    public async Task<IEnumerable<CommandCategory>> LoadCommands()
    {
        using var httpClient = new HttpClient();

        var isInternetAvailable = await networkChecker.IsInternetAvailable(httpClient, "http://doc.pumotix.ru/");

        if (isInternetAvailable)
            return await LoadCommandsFromWeb(httpClient);
        else
            return await LoadCommandsFromDb();
    }

    private async Task<IEnumerable<CommandCategory>> LoadCommandsFromWeb(HttpClient httpClient)
    {
        await using var appContext = await contextFactory.CreateDbContextAsync();

        var result = new List<CommandCategory>();

        foreach (var address in commandCategories.Keys)
        {
            var content = await httpClient.GetStringAsync($"{BaseUrl}{address}");
            var pageStrings = content.Replace("<", "@").Split('@');

            for (var j = 0; j <= pageStrings.Length - 1; j++)
            {
                var s = pageStrings[j];

                if ((s.Contains(">bool") ||
                     s.Contains(">void") ||
                     s.Contains(">number") ||
                     s.Contains(">string") ||
                     s.Contains(">int")) &&
                    s.Contains('(') &&
                    s.Contains(')'))
                {
                    var func = s.Substring(s.IndexOf(">")) + "\r";

                    var commandName = func.Replace(">bool", "")
                        .Replace(">void", "")
                        .Replace(">number", "")
                        .Replace(">string", "")
                        .Replace(">int", "")
                        .Replace("\r", "");

                    commandName = commandName.Substring(1);

                    if (pageStrings[j + 2].Contains("p>"))
                    {
                        var commandDescription = pageStrings[j + 2];
                        commandDescription = commandDescription.Substring(commandDescription.IndexOf(">") + 1);

                        var commandId = address.Substring(address.LastIndexOf("=") + 1);

                        var header = commandCategories[commandId];
                        var resultCommand = result.FirstOrDefault(x => x.Name == header && x.IsMacro == commandId.StartsWith('5'));

                        if (resultCommand is not null)
                        {
                            if (resultCommand.Commands?.Count > 0)
                            {
                                resultCommand.Commands.Add(new Command()
                                {
                                    Name = commandName,
                                    Description = commandDescription,
                                });

                                continue;
                            }

                            resultCommand.Commands = new List<Command>()
                            {
                                new Command()
                                {
                                    Name = commandName,
                                    Description = commandDescription,
                                }
                            };

                            continue;
                        }

                        result.Add(new CommandCategory()
                        {
                            Name = header,
                            IsMacro = commandId.StartsWith('5'),
                            Commands = new List<Command>()
                            {
                                new Command()
                                {
                                    Name = commandName,
                                    Description = commandDescription
                                }
                            }

                        });
                    }
                }
            }
        }

        // TODO: Удалять записи перед сохранением новых

        await appContext.CommandCategory.AddRangeAsync(result);
        await appContext.SaveChangesAsync();

        return result;
    }

    private async Task<IEnumerable<CommandCategory>> LoadCommandsFromDb()
    {
        await using var appDbContext = await contextFactory.CreateDbContextAsync();

        var result = await appDbContext.CommandCategory
            .Include(x => x.Commands)
            .ToListAsync();

        return result;
    }


}
