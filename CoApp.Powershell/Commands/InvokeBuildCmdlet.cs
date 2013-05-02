﻿//-----------------------------------------------------------------------
// <copyright company="CoApp Project">
//     Copyright (c) 2010-2012 Garrett Serack and CoApp Contributors. 
//     Contributors can be discovered using the 'git log' command.
//     All rights reserved.
// </copyright>
// <license>
//     The software is licensed under the Apache 2.0 License (the "License")
//     You may not use the software except in compliance with the License. 
// </license>
//-----------------------------------------------------------------------

namespace CoApp.Powershell.Commands {
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using ClrPlus.Core.Exceptions;
    using ClrPlus.Core.Extensions;
    using ClrPlus.Platform;
    using ClrPlus.Powershell.Core;
    using ClrPlus.Powershell.Rest.Commands;
    using ClrPlus.Scripting.Languages.PropertySheet;
    using ClrPlus.Scripting.Languages.PropertySheetV3;
    using ClrPlus.Scripting.MsBuild.Building;

    [Cmdlet(AllVerbs.Invoke, "Build")]
    public class InvokeBuildCmdlet : RestableCmdlet<InvokeBuildCmdlet> {
        [Parameter]
        public string ScriptFile {get; set;}

        [Parameter]
        public SwitchParameter RescanTools {get; set;}

        // [Parameter(Position = 0)]
        // public string Command {get; set;}

        [Parameter(ValueFromRemainingArguments = true)]
        public string[] Targets {get; set;}

        [Parameter]
        public string[] Define {get;set;}

        [Parameter]
        public string[] Defines {get;set;}

        protected override void BeginProcessing() {
            if (Remote) {
                ProcessRecordViaRest();
                return;
            }

            using (new PushDirectory(Path.Combine(SessionState.Drive.Current.Root, SessionState.Drive.Current.CurrentLocation) )) {

                // Invoking a ptk script.
                if (string.IsNullOrWhiteSpace(ScriptFile)) {
                    // search for it.
                    ScriptFile = new[] {
                        @"copkg\.buildinfo", @"contrib\.buildinfo", @"contrib\coapp\.buildinfo", @".buildinfo"
                    }.WalkUpPaths();

                    if (string.IsNullOrEmpty(ScriptFile)) {
                        throw new ClrPlusException(@"Unable to find .buildinfo file anywhere in the current directory structure.");
                    }
                }

                using (var local = LocalEventSource) {
                    local.Events += new SourceError((code, location, message, objects) => {
                        location = location ?? SourceLocation.Unknowns;
                        Host.UI.WriteErrorLine("{0}:Error {1}:{2}".format(location.FirstOrDefault(), code, message.format(objects)));
                        return true;
                    });

                    if (!NoWarnings) {
                        local.Events += new SourceWarning((code, location, message, objects) => {
                            WriteWarning("{0}:Warning {1}:{2}".format((location ?? SourceLocation.Unknowns).FirstOrDefault(), message.format(objects)));
                            return false;
                        });
                    }

                    local.Events += new SourceDebug((code, location, message, objects) => {
                        WriteVerbose("{0}:DebugMessage {1}:{2}".format((location ?? SourceLocation.Unknowns).FirstOrDefault(), code, message.format(objects)));
                        return false;
                    });
                    using (var buildScript = new BuildScript(ScriptFile)) {
                        if (Defines != null) {
                            foreach (var i in Defines) {
                                var p = i.IndexOf("=");
                                var k = p > -1 ? i.Substring(0, p) : i;
                                var v = p > -1 ? i.Substring(p + 1) : "";
                                buildScript.AddMacro(k, v);
                            }
                        }
                        if (Define != null) {
                            foreach (var i in Define) {
                                var p = i.IndexOf("=");
                                var k = p > -1 ? i.Substring(0, p) : i;
                                var v = p > -1 ? i.Substring(p + 1) : "";
                                buildScript.AddMacro(k, v);
                            }
                        }
                        buildScript.Execute(Targets);
                    }
                }
            }
        }
    }
}