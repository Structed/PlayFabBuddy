﻿using Microsoft.Extensions.Configuration;
using PlayFabBuddy.Lib.Admin;
using PlayFabBuddy.Lib.Commands.Player;
using PlayFabBuddy.Lib.Entities.Accounts;
using PlayFabBuddy.Lib.Util.Config;
using PlayFabBuddy.Lib.Util.IoC;
using PlayFabBuddy.Lib.Util.Repository;

namespace PlayFabBuddy.Cli;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var switchMappings = new Dictionary<string, string>
        {
            {"-c", "concurrent"},
            {"-i", "input"},
            {"-o", "output"}
        };

        var builder = new ConfigurationBuilder();
        builder.AddJsonFile("settings.json");
        builder.AddJsonFile("local.settings.json", true);
        builder.AddCommandLine(args, switchMappings);

        var config = builder.Build();

        if (config["devSecret"] == null || config["titleId"] == null)
        {
            Console.WriteLine("Could not load PlayFab TitleId and Developer Secret from local.appsettings.json");

            return 1;
        }

        var pfConfig = new PlayFabConfig(config["titleId"], config["devSecret"]);

        pfConfig.InitAsync();

        DependencyInjection.Instance.Register<IConfig>(() => pfConfig, RegistrationType.Singleton);

        var defaultAccountOutputPath = "MasterAccountOutput.json";

        if (config["output"] != null)
        {
            defaultAccountOutputPath = config["output"];
        }

        DependencyInjection.Instance.Register<IRepository<MasterPlayerAccountEntity>>(
            () => new LocalMasterPlayerAccountRepository(defaultAccountOutputPath), RegistrationType.New);


        int concurrentUsers;

        if (config["concurrent"] == null || !int.TryParse(config["concurrent"], out concurrentUsers))
        {
            Console.WriteLine("You have to define the Number of concurrent users!");
            Console.WriteLine("CreateOrLoginUsers -c <int>");
            Console.WriteLine("CreateOrLoginUsers --concurrent <int>");

            return 1;
        }

        Console.WriteLine("Starting " + concurrentUsers + " Tasks to run concurrent....\n");

        var commands = new List<Task<MasterPlayerAccountEntity>>();

        for (var i = 0; i < concurrentUsers; i++)
        {
            Console.Write(".");

            commands.Add(new RegisterNewPlayerCommand().ExecuteAsync());
        }

        var results = await Task.WhenAll(commands);

        var repo = DependencyInjection.Instance.Resolve<IRepository<MasterPlayerAccountEntity>>();
        await repo.Save(results.ToList());

        Console.ReadKey();

        var delete = new DeletePlayersCommand().ExecuteAsync();

        //If there is no predifined User List to use, create random users!
        if (config["input"] == null)
        {
            var createUsers = new RegisterNewPlayerCommand();
        }

        return 0;
    }
}