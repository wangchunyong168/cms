﻿using System;
using System.Threading.Tasks;
using Mono.Options;
using SSCMS.Cli.Abstractions;
using SSCMS.Cli.Core;
using SSCMS.Plugins;

namespace SSCMS.Cli.Jobs
{
    public class PluginUnPublishJob : IJobService
    {
        public string CommandName => "plugin unpublish";

        private bool _isHelp;

        private readonly ICliApiService _cliApiService;
        private readonly OptionSet _options;

        public PluginUnPublishJob(ICliApiService cliApiService)
        {
            _cliApiService = cliApiService;
            _options = new OptionSet
            {
                {
                    "h|help", "Display help",
                    v => _isHelp = v != null
                }
            };
        }

        public void PrintUsage()
        {
            Console.WriteLine($"Usage: sscms {CommandName} <pluginId>");
            Console.WriteLine("Summary: unpublishes a plugin. Example plugin id: sscms.hits");
            Console.WriteLine("Options:");
            _options.WriteOptionDescriptions(Console.Out);
            Console.WriteLine();
        }

        public async Task ExecuteAsync(IPluginJobContext context)
        {
            if (!CliUtils.ParseArgs(_options, context.Args)) return;

            if (_isHelp)
            {
                PrintUsage();
                return;
            }

            if (context.Extras == null || context.Extras.Length == 0)
            {
                await WriteUtils.PrintErrorAsync("missing required pluginId");
                return;
            }

            var (status, failureMessage) = await _cliApiService.GetStatusAsync();
            if (status == null)
            {
                await WriteUtils.PrintErrorAsync(failureMessage);
                return;
            }

            bool success;
            (success, failureMessage) = await _cliApiService.PluginUnPublishAsync(context.Extras[0]);
            if (success)
            {
                await WriteUtils.PrintSuccessAsync($"Plugin {context.Extras[0]} unpublished.");
            }
            else
            {
                await WriteUtils.PrintErrorAsync(failureMessage);
            }
        }
    }
}
